# [BIM_IFC_EGRESS]

The Bim-internal IFC re-author: `SemanticProjector.Emit` lowers a seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` back into IFC bytes at the `IfcWireForm` row a caller names — STEP, zipped STEP, ifcXML, ifcJSON — and `Sniff` reads the schema off foreign bytes BEFORE the database is constructed. `Emit` is a Bim-INTERNAL method on the `Projection/semantic#SEMANTIC_PROJECTOR` projector (a `partial class SemanticProjector` continuation), NOT an `IElementProjection` member — IFC egress is one runtime's wire concern and the seam owns only ingress projection — so it is the exact inverse of the `Project` ingress: where `Project` lowers GeometryGym into seam `Node`s and neutral `Relationship` edges, `Emit` re-authors the seam graph into the IFC entity graph, reading the seam graph ONLY (the `Material` node + the `Associate` edge `MaterialUsage`, the `PropertySet`/`QuantitySet` bag nodes, the `Object` node `Classification`), never a retired `Rasm.Materials` wire carrier. GeometryGym stays captured internally and the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg: `ReleaseRaise` and `Sniff` both resolve through the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap` correspondence, faulting `Model/faults#FAULT_BAND` `BimFault.CodecReject` BARE on an unmapped member — the `IFC4X3_ADD2` silent default is DELETED on both legs — and neither enum ever reaches the seam `Header`.

The egress is the round-trip authority for the `Model/elements#IFC_CLASS` egress gate, the `GlobalId` projection, the diff-derived `OwnerHistory`, schema sniffing, the full typed `PropertyValue` family, and ordered nesting. Numeric, binary, temporal, measured, logical, aggregate, and bounded values re-author through their typed GeometryGym entities rather than `Render` text, and `IfcRelNests.RelatedObjects` order restores from the integer ordinal attribute. The relationship re-author reverses the `Projection/relations#RELATION_ALGEBRA` roster through `IfcRelKind.ForNeutral`/`Author`, and the material/property/classification subgraphs re-author through their dedicated folds.

## [01]-[INDEX]

- [02]-[IFC_EGRESS]: `SemanticProjector.Emit` the Bim-internal `ElementGraph` → IFC bytes re-author — the `IfcWireForm` serialization-container-and-rank row set whose seal delegate owns the write, the railed `ReleaseRaise` schema target, the `EmitContext` carrier (diff-prior, the `Closure`-hulled partial-export scope, the declared unit regime whose inverse `UnitScale.Declare` folds every raised magnitude, the composition hooks the `rasm.bim.projection.emit` veto rides), the `IfcClass` egress gate, the content-derived `GlobalId` and diff-derived `OwnerHistory` stamps, `ReauthorMaterials`, the complete typed `PropertyValue`/`MeasureValue` inverse with the group-nested quantity rebuild, `ReauthorClassifications`, and the row-driven `ReauthorRelationships` inverse including ordered nesting, realizing sets, eccentric constraints, and connection interfaces — plus the railed georeference inverse returning its `GeoAuthored` level, the railed `Sniff` schema admission, and the egress half of the `FidelityReceipt` drop ledger.

## [02]-[IFC_EGRESS]

- Owner: `SemanticProjector.Emit` the Bim-internal `ElementGraph`→IFC bytes re-author — NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern; `IfcWireForm` the owned wire-form vocabulary carrying the GeometryGym `FormatIfcSerialization` and the interop `FidelityRank` as columns beside the entry extension and the row's own SEAL and ADMIT delegates, so serialization, container, rank, and both byte directions are ONE value (the zipped STEP row the enum cannot express has a seat, and a zip flag beside the serialization is the deleted parallel knob); it resolves the emit schema through the railed `ReleaseRaise` member (defined HERE on the egress fence, reading the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap.Raise` inverse the ingress `ReleaseLower` mirrors over `ReleaseMap.Lower` — `BimFault.CodecReject` on a seam schema GG cannot serialize, `Ifc5` today), constructs the `DatabaseIfc` at that release with the seam `Header.Tolerance` restored, runs the `Model/elements#IFC_CLASS` egress gate per `"ifc"`-classified `Object` node — `Instantiable: false` faults, the per-token `AdmitPredefined` admits, the admitted token stamps the entity — publishing a `NodeId`→`IfcObjectDefinition` map covering occurrences AND types AND the non-product object families (the `IfcProject` context root wired through its db-binding ctor, groups/systems/zones, processes, controls, actors, resources — an `IfcWallType` or an `IfcSystem` is NOT an `IfcProduct`, so an `IfcProduct`-typed map is the deleted crash-cast form), and re-authors the entity graph: `ReauthorMaterials` (the `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` per seam `Material` node + `Associate` usage edge [OCCURRENCE_USAGE_RULING]), `ReauthorProperties` (the `IfcPropertySet`/`IfcElementQuantity` rebuilt from the bag nodes + the `IfcRelDefinesByProperties` onto the elements, every typed `PropertyValue` case re-authoring its exact `IfcProperty` counterpart), `ReauthorClassifications` (`Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` per `Object` node, every authored dictionary URI deriving from the ctor-held `BsddPins`), and `ReauthorRelationships` (the neutral-edge → `IfcRel*` row-driven re-author, ordinal-bearing `Generic` nests grouped per parent so `IfcRelNests.RelatedObjects` order round-trips); `Sniff` the ingress counterpart reading `FILE_SCHEMA` (STEP) or `schemaIdentifier` (JSON) off the bytes onto a `ReleaseMap`-admitted GeometryGym release BEFORE the database is constructed, `BimFault.CodecReject` on an absent or unmapped header so the schema is never guessed [H8].
- Law: the emit is GATED then TOTAL — the `rasm.bim.projection.emit` veto point brackets the whole write through the capsule's GUARDED fire, so a deliverable policy refuses on the wire form, the raised schema, and the in-scope node count BEFORE a `DatabaseIfc` exists and a refusal is the emit's typed verdict rather than a written artifact nobody wanted; a bare fire followed by an unconditional write is the deleted form.
- Law: every raise states its failure — `RaiseValue` rails `Fin<IfcValue>` because a composite or reference cell has no IFC value spelling, and `StampPredefined` rails a token its own class enum cannot parse; a thrown escape off this pipeline and a silently unstamped `NOTDEFINED` entity are both deleted, the second because it read identically to a class carrying no predefined slot at all.
- Law: every drop this leg incurs is RETURNED on the `Noted` writer rail and lands ONCE through `Land` at the seal, so a refused write charges the ledger for nothing and a rerun re-derives the same receipt.
- Exemption: `Register` is the ONE statement seam over GeometryGym's construct-registers idiom — an `IfcRel*` constructor binds its entity onto the database as a ctor side effect, so construction IS the authoring act and no expression form can both perform it and name it; a scattered `ignore(new IfcRel…)` whose discarded value reads as a dropped result is the deleted form.
- Entry: `SemanticProjector.Emit(ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default)` re-authors the graph into IFC BYTES — the one currency, so no caller re-encodes a returned string and a zipped container is expressible — the `EmitContext` record collapses the four orthogonal emit axes onto one optional carrier (the composition `Hooks` the emit veto fires through, the diff-`Prior` snapshot, the partial-export `Scope` whose `Closure` fold is the one owned law of what a coherent partial model drags along — the spatial ancestor chain to the root plus each member's bound type — those roots sliced through the seam `Rasm.Element/Graph/element#FEDERATION` `graph.Extract(roots, key)` into a Members-closed model nothing dangles out of, with bags and materials following structurally through the authored-subject gates — and the declared `Units` regime defaulting to `Header.Units` so a mm-source import re-emits mm and an explicitly-passed foot regime lands the contractual imperial deliverable), never a parallel `Option` tail (the `IIfcProfileStore` capability rides the projector's primary constructor — a second `profiles` parameter re-passing the instance dependency is the deleted knob) — for each `"ifc"`-classified `Object` node it resolves the `IfcClass` row from the generic `Classification` code, rejects a schema-abstract row (`Instantiable: false` → `BimFault.UnmappedClass` `abstract-class-at-egress:` — the row is legal CLASSIFICATION vocabulary, illegal as an authored entity class), runs `IfcClass.AdmitPredefined(token, objectType, schema, key)` validating the predefined token against the row's per-token `PredefinedRow` spans AND the class schema span → `BimFault.UnmappedClass` [PREDEFINED_TOKEN_RULING][H8], constructs the entity at the resolved schema, STAMPS the admitted token onto the entity's own `Ifc*TypeEnum` property (`IfcObject.ObjectType`/`IfcElementType.ElementType` authored from the node's own `ObjectType` column on `USERDEFINED`), assigns the `ExternalId` `GlobalId` (or derives one for a from-scratch node by feeding its id-inclusive `ContentAddress` through `ParserIfc.EncodeGuid`, so the mint is reproducible) [H6], and re-stamps the `OwnerHistory` with a `ChangeAction` diff-derived against the `prior` snapshot, matching the rooted node on its stable `ExternalId` GlobalId across re-ingest [H9] — publishing the `NodeId`→`IfcObjectDefinition` map the re-author folds bind against; `ReauthorMaterials` authors each seam `Material` node's type-level definition + `MaterialPropertySet` Psets ONCE and the per-occurrence `MaterialUsage` [OCCURRENCE_USAGE_RULING] over each `Associate` edge, the ctor-held `profiles` store reconstituting a `ProfileSet`'s `IfcProfileDef` from the preserved profile store while a store-missed Rasm-authored `ProfileSet` authors the profile entity the carried `DetailSchema.Realization` `ProfileSubtype` row names off the baked section dims (`ProfileSubtypeOf` resolves the Materials-seeded occupancy token off the carried graph row, never a Materials call); `Fin<T>` aborts on the release miss, the gate, or an unraisable quantity dimension, each typed case lifting BARE; `SemanticProjector.Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format, Op key)` returns `Fin<GGRelease>` — the release the import-rail seeds the database with, faulted typed on a missing or unmapped header.
- Auto: `Emit` resolves the `"ifc"`-classified `Object` roster ONCE — a foreign-system node, a Rhino-native capture classified outside `"ifc"`, is NOT IFC-representable and is skipped by classification, never a fault that aborts a federated emit — publishes its magnitude on the `rasm.bim.projection.emit` admission fact, and folds it through one `TraverseM` inside the guarded fire; the egress gate resolves the `IfcClass` row, rejects the abstract supertype, validates the predefined token per-token against `Header.Schema` (an IFC4.3 `WAVEWALL` token targeting an IFC2x3 emit faults rather than writing a token the schema forbids), and constructs + stamps the entity — the `IfcProject` row through its db-binding `new IfcProject(DatabaseIfc, name)` ctor so the emitted file carries its mandatory root context, every other row through `Factory.Construct`; the `GlobalId` round-trips from `ExternalId` so a re-imported model re-emits its original GUIDs (1:1) and a from-scratch node DERIVES its compressed identifier from its own id-inclusive `ContentAddress` through `ParserIfc.EncodeGuid`, so re-emitting an unchanged node re-mints byte-identical bytes and a re-export of an unedited graph diffs empty [H6]; `ReauthorRelationships` first groups the ordinal-bearing `Generic` `IfcRelNests` edges per relating parent, orders each group by the `NestOrdinal` attribute, and authors ONE `IfcRelNests` whose `RelatedObjects` fill in that order (the `[AMENDMENTS]` ordered-nest round-trip — the typed `Compose(nest)` case stays the order-free per-pair author), then re-authors the neutral `Compose`/`Connect`/`Void` edges and the `Assign.TypeDefinition`/`Group` edges by reverse-indexing the `IfcRelKind` row and the `Generic` long-tail by its wire-name through `IfcRelKind.Author`, the directionality reconstructing from the row's relating/related names — so the long-tail families re-emit exactly as they were read [NEUTRAL_EDGE_RULING], the structural member/activity idealization and the space boundaries INCLUDED (they are ingest-landed IFC round-trip state whose loss broke every re-exported analysis/energy model — the space boundary re-authoring its exact 1st/2nd-level subtype from the ingress `BoundaryLevel` attr through the level-refined construct and its interface surface from the `InterfaceKey` attr, the element connect re-attaching its interface surface from the typed `Connect.Interface` slot, the eccentric member binding its exact subtype from the `Model/structural#STRUCTURAL_PROJECTION`-owned `Eccentricity` row with the constraint restored from the store, the restraint/load node payload through `ReauthorStructural` composing the `Model/structural` `StructuralProjection.Author` inverse on its own rail and folding the returned unconsumed-row residue into the drop ledger, and the realizing `Connect` fan re-grouped by endpoint pair so every realizer re-emits on ONE `IfcRelConnectsWithRealizingElements`), while the `Rasm.Compute`-authored `Assign.Assessment` receipts alone are skipped: an analysis receipt has no IFC entity and the seam mints no phantom `IfcPerformanceHistory`/`IfcControl` for a Rasm-native result — an imported IFC `IfcRelAssignsToControl` assessment-family relation rode the rostered `Generic` wire-name path at ingest and re-emits from it, so no IFC assessment relation is dropped (the `Assign.PropertyDefinition` edge re-authors through `ReauthorProperties`, not here); the seam `Material` subgraph re-authors through `ReauthorMaterials` and the property/quantity bags through `ReauthorProperties` — EMIT-SCOPED (a bag bound only to foreign-system subjects never authors, an unbound source Pset round-trips, the projector-minted `TypeSignatureSet` bookkeeping bag never exports) and the FULL `Projection/semantic#SEMANTIC_PROJECTOR` inverse: `Boolean`/`Text`/`Integer`/`Number`/`Binary`/`Temporal` their typed scalar `IfcPropertySingleValue` values, `Measure` ITS OWN typed `IfcValue` through the `RaiseMeasure` mint DERIVED from the ingress `MeasureDimensions` table (an `IfcThermalTransmittanceMeasure` re-emits as itself, never a flattened `IfcReal`), the three-valued `Logical` a typed `IfcLogical`, `Enumerated` the `IfcPropertyEnumeratedValue` with its `IfcPropertyEnumeration` allowed-set reference when the seam carries one, `Reference` the `IfcPropertyReferenceValue` carrying its `UsageName` AND its re-attached `PropertyReference` when the seam target resolves to an authored select member (the non-rooted resource identity staying the ingress-named bounded drop), `Bounded` the `IfcPropertyBoundedValue` typed lower/upper/setpoint, `List` the `IfcPropertyListValue`, `Table` the `IfcPropertyTableValue` with its curve rule, `Complex` the `IfcComplexProperty` RECURSING the same raise over its sub-bag — so an import→export cycle degrades NO typed case to a string or bare real, the quantity resolving its `IfcQuantity*` through the frozen `QuantityRaisers` QTO-type table with the base-dimension `CanonicalQuantities` fallback and re-nesting under one `IfcPhysicalComplexQuantity` per bag `Groups` prefix with its `Discrimination`/`Quality`/`Usage` restored, and the emitted database declaring the RESOLVED unit regime — `DeclareUnits` authors the `IfcUnitAssignment` the regime's length family names (the `LengthRegimes` frozen row table over the seam `UnitScheme` tokens; an empty or unmapped scheme keeps the GG SI defaults) and every raised magnitude folds SI→declared through the ONE inverse `UnitScale.Declare` over the regime derived off the constructed database — the model tolerance included, so no call site divides by a bare axis factor — the property and quantity values, the material layer thicknesses and usage offsets `ReauthorMaterials` re-declares, and the tolerance, so a mm-source import re-emits verbatim-mm and an SI graph under a foot regime lands the survey-foot deliverable (the map-conversion offsets and the structural payload magnitudes stay the geo and structural owners' SI author — the two named residual rows of the regime); the `OwnerHistory` re-emit derives the `ChangeAction` from the generated `Node.Object.EqualityComparer.Default.Inequalities` structured diff (the rooted node matched on its stable 1:1 GlobalId `ExternalId` since the neutral `NodeId` is freshly minted each ingest, the seam owner's `[IgnoreEquality]` `Id` override keeping that fresh mint out of the verdict by construction — `ADDED`/`MODIFIED`/`NOCHANGE`) so the IFC owner history reflects the real edit [H9]; the `StepHeader` re-authors `FILE_DESCRIPTION`/`FILE_NAME` from `Header.Step`; the georeference inverse is the last rail rung before the seal — `Semantics/georeference#GEO_PROJECTION` `GeoReferenceProjector.Author` re-authors `Header.Reference` at its own LoGeoRef level under the emit regime and RETURNS the `GeoAuthored` level, a projectless database or an absent anchor entity aborting the emit typed and a `Conversion` level over an anisotropy-carrying frame noting the counted collapse [M1].
- Auto: `Sniff` reads the schema off the bytes before `new DatabaseIfc` — the STEP `FILE_SCHEMA(('IFC4X3_ADD2'))` quoted token, the ifcXML header xmlns schema URI (`.../ifcXML/<release>`), or the IFC-JSON `schemaIdentifier` member — parsing the token onto the GeometryGym `ReleaseVersion` and gating membership in the frozen `ReleaseMap.Lower` key set (the `IFC4X4_DRAFT` member is excluded by law), so an absent header, an unparseable token, or an unadmitted release faults `BimFault.CodecReject` BARE and the import never guesses 4x3 over a 2x3 file [H8].
- Receipt: every named bounded drop the egress incurs RETURNS on its leg's `Noted` rail — the measure-flatten tail of `RaiseMeasure`, the store-missed eccentricity degrade, the deliberate assessment skip, the linear-placement re-anchor, each unconsumed `StructuralProjection.Author` residue row, and the `GeoAuthored.Conversion` level an anisotropic frame collapses onto below `IFC4X3_ADD2` — the legs' logs join monoidally and enter the projector's ledger ONCE through `Land` at the seal, so `SemanticProjector.Fidelity` reads as one per-exchange drop ledger across both halves and a federation manager audits the emit instead of trusting prose.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Generator.Equals
- Growth: a new wire form is one `IfcWireForm` row carrying its `FormatIfcSerialization` column, its entry extension, its `FidelityRank`, and the seal/admit delegate pair one of the existing writers and readers already answers — a new CONTAINER over an existing serialization is a row whose extension GeometryGym's own write dispatch already reads, whose read unwraps into the shared text parse, and whose rank ties the serialization it repeats, never an emit-side or import-side branch; a new GG release is one `ReleaseMap` row BOTH lowerings read (`Lower` at ingress, `Raise` at egress) with zero new arm; a new predefined token or schema span is one `PredefinedRow` on the generated `IfcClass` roster the same per-token gate reads; a new property-value case is one generated-`Switch` arm `RaiseProperty` breaks on at compile time; a new physical-quantity entity is one `QuantityRaisers` row keyed on the QTO type its ingress `PropertyLowering.QuantityTypes` peer stamps; a new measure type re-raises from its ingress `MeasureDimensions` row with ZERO egress edits (the `RaiseMeasure` mint derives); a new carried profile-subtype token is one seed-computed realization-bag row the same `ProfileSubtypeOf` read resolves with zero egress edits; a new subtype refinement is one `Refined` arm over its discriminating attr; a new order-bearing relationship family is one wire-name beside `IfcRelKind.Nests.Key` at the ordered-author gate and its `RelKindOf` exclusion arm — never a per-class egress branch.
- Boundary: `Emit` is Bim-INTERNAL and absent from the `IElementProjection` contract — exposing it on the seam is the named violation; the wire form is the `IfcWireForm` ROW: the write container is GeometryGym's own entry-extension dispatch inside `WriteStream` and the read container is the row's own `Admit` body, so a `bool zipped` beside the serialization, a caller-side `ZipArchive` over emitted text, a re-parse-and-rezip of a written STEP string, and an import `_` tail handing ZIP bytes to the text parser are the deleted forms, and a `false` writer return rails `BimFault.CodecReject` because an empty buffer read as a written model is the forged artifact; `Emit` returns BYTES and a `string` return re-encoded at every call site is the deleted intermediate; the predefined validity is an EGRESS gate (validated per-token when the IFC entity is authored, against the `PredefinedRow` spans and the class schema span) [PREDEFINED_TOKEN_RULING] and the admitted token is STAMPED onto the entity — a gate that validates then discards the token, a per-call regex, or silent acceptance of an out-of-schema predefined is the deleted form; an `Instantiable: false` class at egress faults `BimFault.UnmappedClass` and authoring a schema-abstract supertype entity is the deleted form — the occurrence/refinement owns concretization; the `GlobalId` is the node `ExternalId` round-tripped 1:1 [H6] and a from-scratch node's identifier DERIVES from its own id-inclusive `ContentAddress` — a `Guid.NewGuid()` mint anywhere on this leg is the deleted form, because a random identifier re-keys the same node on every emit, turns every re-export into a whole-file diff, and breaks every external reference into the model; the reproducibility is the contract, not an optimization; the schema is sniffed AND release-mapped [H8] — a hardcoded `IFC4X3_ADD2` over a foreign-schema file, an `Enum.TryParse` silent default, or a second GG↔seam release table beside the frozen `ReleaseMap` is the named defect; the `ChangeAction` is diff-derived through the BARE generated `Node.Object.EqualityComparer` structured diff [H9] — the seam owner's `[IgnoreEquality]` `Id` override excludes the fresh mint, so an egress-side member filter, a blanket `ADDED` stamp, and a `with { Id = … }` clone-then-`Equals` re-spelling the comparer are all the deleted form; a quantity bag re-authoring its `Groups` rows as dotted flat names is the deleted lossy form — the prefix carried the nesting and nothing carried the grouping identity, so a classified takeoff hierarchy re-emitted as one flat set; a `Bounded`/`List`/`Complex` property degrading to its `Render` string is the deleted lossy form — `Text` alone is the string arm, and the raise switch is the generated TOTAL dispatch so every new seam case breaks this page at compile time; a `Measure` (scalar, cell, or bound) re-authoring as a bare `IfcReal` when its `QuantityType` names a GG `IfcValue` type or its dimension a canonical base measure is the deleted flattening — the `RaiseMeasure` mint DERIVES from the ingress `MeasureDimensions` keys, so the two directions cannot drift and a second hand-rostered raise table is the named defect; the bag egress is EMIT-SCOPED — authoring a bag bound only to foreign-system subjects (an orphan `IfcPropertySet` in a federated emit) or exporting the projector-minted `TypeSignatureSet` bookkeeping bag is the deleted form, while an unbound source Pset round-trips; the `USERDEFINED` label re-stamps VERBATIM from the seam `Node.Object` `ObjectType` column onto `IfcObject.ObjectType` or `IfcElementType.ElementType` — the `Name` substitution is DELETED (it collapsed two same-named entities carrying distinct labels), a `None` label on a `USERDEFINED` node faults `predefined-objecttype-miss` at the `AdmitPredefined` gate, and a bag row, an attachment edge, or an egress label index re-carrying the label beside the column is the deleted form; a `Rasm.Compute`-authored `Assign.Assessment` edge is NON-IFC-NATIVE and INTENTIONALLY not re-authored — the analysis receipt is Rasm-native enrichment re-derivable from the content-keyed inputs, so forcing it into a phantom `IfcRelAssignsToControl`/`IfcPerformanceHistory` is the deleted form, while an IMPORTED assessment-family relation round-trips by `Generic` wire-name; an ordinal-bearing `Generic` nest authors ONCE per parent in ordinal order and a per-pair re-author dropping `IfcRelNests.RelatedObjects` order is the deleted form; the emit rail re-authors SEMANTICS alone — no body representation authors here (geometry egress rides the glTF/3dm deliverables), so the `Semantics/appearance#APPEARANCE_PROJECTION` `Author`/`Bind` egress pair stays armed on a body-representation author joining this rail and no styled item is minted without an item to style; the material/property/classification egress reads the seam graph ONLY — a `Rasm.Materials` wire carrier crossing into `Emit` is the deleted form, the type-level material definition authored ONCE per `Material` node and the per-occurrence usage wrapping it [OCCURRENCE_USAGE_RULING]; the georeference inverse and the structural re-author are RAIL RUNGS whose returns are CONSUMED — a discarded `GeoReferenceProjector.Author` call (reporting a written frame it never wrote) and an `ignore`d `StructuralProjection.Author` residue (the drop promise no receipt can observe) are the deleted forms; the bSDD hosted-version pins are the ctor-held composition value and a `BsddPins.Default` spelled at the egress author is the deleted form that freezes a registry-published version into the emit leg; a connection interface whose content key the profiles store cannot answer faults `BimFault.DanglingReference` — the geometry is OPTIONAL on `IfcRelConnectsElements`/`IfcRelSpaceBoundary`, so an ABSENT key is plain topology while a present unanswerable one names a surface the ingest located and the emit cannot restore, and the eccentricity degrade is no precedent here because its refinement is legally droppable and an interface is not; a SCOPED deliverable is a seam `Graph/element#FEDERATION` `Extract` SLICE over the `Closure` roots and a hull-predicate filter over the whole graph is the deleted form — the slice is Members-closed, so no edge straddles the boundary and the emit legs meet no half-resolved joint; the relationship rail and EVERY drop note it carries run only where BOTH endpoints resolve the authored map, the same emit-scoping the bag and material folds hold, and that compensation now answers the FEDERATED and foreign-system-bounded emit alone — the case no slice closes, where an unguarded rail aborts on a joint it never writes and charges its receipt for relationships nothing authored, an over-counted ledger being as unusable as a silent one; a `ProfileSet` with no preserved STEP fragment resolves its profile subtype from the carried `DetailSchema.Realization` `ProfileSubtype` row — the rectangle token authors whole off the baked `SectionProperties` dims, while a token whose mandatory interior geometry only a preserved fragment carries (`IfcArbitraryProfileDefWithVoids` inner curves — inline curve geometry never rides the seam [M2]) keeps the typed `DanglingReference` fault, never a bare subtype with unassigned mandatory curves and never a Materials call; the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg through `ReleaseRaise`/`Sniff` and a leak into the seam `Header` is the named defect; the authored map is `NodeId`→`IfcObjectDefinition` — an `IfcProduct`-typed map is the deleted crash-cast form (a type node authors an `IfcTypeObject` subtype and the context root authors `IfcProject`, neither an `IfcProduct`); `Header.Tolerance` restores onto the constructed database, and the seam `Header.View` round-trips as the VERBATIM `FILE_DESCRIPTION` `ViewDefinition` line `ReauthorHeader` restores — a `ViewRaise` assigning `DatabaseIfc.ModelView` stands as a second release authority beside the railed `ReleaseRaise` and is the rejected form; a quantity carrying neither a rostered QTO type nor a base-dimension signature faults `BimFault.CodecReject` — the `QuantityRaisers` rows key on the seam `QuantityType` the ingress stamped because `Count`, `Number`, `Ratio`, and `Angle` all sign `Dimensionless`, so the retired `Dimension` key re-authored every IFC4.3 `IfcQuantityNumber` as the integral `IfcQuantityCount`, and `Dimensionless` is absent from the signature fallback because answering an untyped scalar with a `Count` is the same masked-error form the silent `IfcQuantityLength` coercion was.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// Emit/Sniff continue the Projection/semantic#SEMANTIC_PROJECTOR partial class — the egress half of the
// one projector. The prelude mirrors the ingress page so the partial compiles standalone; the GeometryGym
// ReleaseVersion enum stays codec-leg-local through the GGRelease alias (both lowerings ride the frozen
// Model/elements#TAXONOMY_EMITTER ReleaseMap) and never reaches the seam Header (the seam currency is the
// Rasm.Element alias).
using System.Buffers.Binary;                          // the UInt128 content address -> Guid layout the deterministic GlobalId mint writes
using System.Collections.Frozen;
using System.Globalization;                            // the X32 hex canon every UInt128 content key crosses an attr as
using System.IO;
using System.IO.Compression;                          // ZipArchive — the read-side container the StepZip row opens; the write side is GG's own
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using GeometryGym.Ifc;
using GeometryGym.STEP;
using LanguageExt;
using NodaTime;
using Rasm.Bim;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Rasm.Element.Classification;
using Rasm.Element.Geospatial;                        // the seam GeoReference frame the georeference egress inverse reads
using Rasm.Element.Graph;
using Rasm.Element.Projection;                        // the seam ContentAddress the deterministic GlobalId derives from
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;   // the seam schema currency the Header carries — disambiguated from
using GGRelease = GeometryGym.Ifc.ReleaseVersion;     // the GeometryGym IFC-text enum ReleaseRaise/Sniff resolve

namespace Rasm.Bim.Projection;

// IfcWireForm pairs the serialization GeometryGym writes with the container it writes it into, one owned row set
// because both are ONE caller decision — a zip flag beside the serialization re-describes what the row already
// carries. GeometryGym's FormatIfcSerialization enum stays a COLUMN rather than the emit currency, so a codec-leg
// enum never becomes the vocabulary and zipped STEP gets a seat that enum cannot express.
// Container selection on the WRITE side stays GeometryGym's OWN: WriteStream reads the entry extension, opens a
// ZipArchive over the stream for `.ifczip` and writes STEP text as one `<stem>.ifc` entry, writes the ifcXML
// document for `.xml`, and writes STEP text otherwise — so no local container ladder exists there. Only the ifcJSON
// row falls to ToString, because that writer carries no JSON arm.
// The READ side is the row's own: GG opens no archive and its TextReader door elects XML or STEP by peek alone, so
// each row carries an Admit delegate SYMMETRIC to its Seal — the two text serializations share one body, the zipped
// container unzips into that same body, and ifcJSON takes its own public reader. Binding the read to the row is what
// keeps a `_` tail from handing ZIP bytes to a text parser, which reads as a malformed model rather than as a
// container the reader never opened.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IfcWireForm {
    public static readonly IfcWireForm Step    = new("step",     FormatIfcSerialization.STEP, ".ifc",     0, Streamed, Parsed);
    public static readonly IfcWireForm StepZip = new("step-zip", FormatIfcSerialization.STEP, ".ifczip",  0, Streamed, Unzipped);
    public static readonly IfcWireForm Xml     = new("xml",      FormatIfcSerialization.XML,  ".ifcxml",  1, Streamed, Parsed);
    public static readonly IfcWireForm Json    = new("json",     FormatIfcSerialization.JSON, ".ifcjson", 2, Texted,   Jsoned);

    public FormatIfcSerialization Serialization { get; }

    // Container dispatch reads this entry extension AND the zip entry inherits it.
    public string Extension { get; }

    // The interop-fidelity rank, ascending — the ROW owns it because the row owns the serialization it ranks. A
    // CONTAINER ranks with the serialization it repeats (zipped STEP ties plain STEP), so a negotiation fold reading
    // this column orders forms without a second switch over FormatIfcSerialization whose `_` tail would seat every
    // future serialization kind beside ifcJSON.
    public int FidelityRank { get; }

    // Rows own their byte seal. Option, not Fin: the row owns the write while its caller owns the fault vocabulary,
    // so a refused write lifts under the emitting Op rather than minting a second fault family here. False from the
    // writer signals REFUSAL, never an empty artifact — an empty buffer read as a written model is the
    // forged-artifact form.
    [UseDelegateFromConstructor]
    public partial Option<ReadOnlyMemory<byte>> Seal(DatabaseIfc target, string entry);

    static Option<ReadOnlyMemory<byte>> Streamed(DatabaseIfc target, string entry) {
        using MemoryStream sink = new();
        return target.WriteStream(sink, entry) ? Some<ReadOnlyMemory<byte>>(sink.ToArray()) : None;
    }

    // Only the ifcJSON row binds this delegate, so the serialization it names is its own column, not a ladder value.
    static Option<ReadOnlyMemory<byte>> Texted(DatabaseIfc target, string entry) =>
        Optional(target.ToString(FormatIfcSerialization.JSON))
            .Map(static text => (ReadOnlyMemory<byte>)Encoding.UTF8.GetBytes(text));

    // Rows own their byte ADMISSION, the exact counterpart of Seal and Option for the same reason: the row owns the
    // read while its caller — the import rail, holding the Sniff verdict — owns the fault vocabulary. `release` is
    // the ADMITTED schema Sniff already gated against the frozen ReleaseMap, stamped after the parse so the database
    // carries the admitted release rather than GG's own header guess, which is the whole point of sniffing first.
    [UseDelegateFromConstructor]
    public partial Option<DatabaseIfc> Admit(ReadOnlyMemory<byte> bytes, GGRelease release);

    // The two TEXT serializations share ONE body: DatabaseIfc(TextReader) peeks the first character and reads an XML
    // document on '<' or a STEP stream otherwise, so the Step and Xml rows need no arm of their own and a local
    // format switch here would restate a dispatch GG already owns.
    static Option<DatabaseIfc> Parsed(ReadOnlyMemory<byte> bytes, GGRelease release) =>
        Admitted(Encoding.UTF8.GetString(bytes.Span), release);

    static Option<DatabaseIfc> Admitted(string text, GGRelease release) {
        if (text.Length == 0) { return None; }
        using StringReader source = new(text);
        return Some(new DatabaseIfc(source) { Release = release });
    }

    // The zipped container unzips into that SAME body. GG opens no archive on the read side — its container dispatch
    // lives in WriteStream alone — so an unzipped read has no other door, and a `_` tail handing ZIP bytes to the
    // text parser would surface as a malformed model instead of as a container nothing opened. The archive's first
    // entry IS the model (WriteStream lays down exactly one); an empty archive REFUSES rather than admitting an
    // empty database, the same forged-artifact law the seal side holds.
    static Option<DatabaseIfc> Unzipped(ReadOnlyMemory<byte> bytes, GGRelease release) {
        using MemoryStream source = new(bytes.ToArray(), writable: false);
        using ZipArchive archive = new(source, ZipArchiveMode.Read);
        return archive.Entries is { Count: > 0 } entries ? Entry(entries[0], release) : None;
    }

    static Option<DatabaseIfc> Entry(ZipArchiveEntry entry, GGRelease release) {
        using StreamReader reader = new(entry.Open(), Encoding.UTF8);
        return Admitted(reader.ReadToEnd(), release);
    }

    // The ifcJSON reader is its OWN public door — the TextReader peek elects XML or STEP and never JSON — so the row
    // constructs the database at the admitted release and reads the payload into it: the read IS the mutation, the
    // same construct-is-the-authoring-act seam Register names on the relationship side.
    static Option<DatabaseIfc> Jsoned(ReadOnlyMemory<byte> bytes, GGRelease release) {
        if (bytes.IsEmpty) { return None; }
        using StringReader source = new(Encoding.UTF8.GetString(bytes.Span));
        DatabaseIfc target = new(release);
        target.ReadJSONFile(source);
        return Some(target);
    }
}

// The one emit-context record: the four orthogonal emit axes — the diff-prior snapshot, the partial-export scope,
// the declared unit regime, and the composition's hook registry — collapsed onto one optional carrier so the
// entrypoint never grows a parallel Option tail; every absent axis derives its default from the graph (no prior ->
// ADDED, no scope -> the whole graph, no units -> the Header.Units declared scheme, itself the empty-SI default, no
// hooks -> the unbracketed rail). Hooks ride the CARRIER rather than the observability page's optional entry-slot
// idiom because this entrypoint already owns one context argument, and a second Option tail beside it is the parallel
// knob this record exists to delete.
public sealed record EmitContext(
    Option<ElementGraph> Prior = default,
    Option<ElementSet> Scope = default,
    Option<UnitScheme> Units = default,
    Option<BimHooks> Hooks = default) {
    public static readonly EmitContext Whole = new();
}

public sealed partial class SemanticProjector {
    // The Bim-internal IFC egress: ElementGraph -> DatabaseIfc -> bytes, the ONE currency, so no caller re-encodes a
    // returned string and the zipped-STEP container is expressible. The emit schema resolves FIRST through the
    // railed ReleaseRaise below (the ReleaseMap.Raise inverse of the ingress ReleaseLower — CodecReject on a seam
    // schema GG cannot serialize, the IFC4X3_ADD2 silent default DELETED) [H8]; Header.Tolerance restores onto the
    // database (Header.View round-trips VERBATIM through the ReauthorHeader FILE_DESCRIPTION restore — never a
    // ModelView assignment standing a second release authority); the Instantiable + per-token AdmitPredefined gate
    // runs per "ifc"-classified Object node and the admitted token stamps the entity [PREDEFINED_TOKEN_RULING]; the GlobalId round-trips
    // 1:1 [H6]; the OwnerHistory ChangeAction is diff-derived against the prior snapshot [H9]. The authoring publishes
    // a NodeId->IfcObjectDefinition map (occurrences, types, the IfcProject root, groups/processes/actors/resources);
    // the ctor-held profiles resolver reconstitutes a ProfileSet's IfcProfileDef from the content-addressed STEP store
    // the ProfileRef.ContentKey keys. Never a seam member.
    public Fin<ReadOnlyMemory<byte>> Emit(ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default) =>
        ReleaseRaise(graph.Header.Schema, key).Bind(release => {
            EmitContext ctx = context.IfNone(EmitContext.Whole);
            // The partial-export scope [X1]: a caller-selected ElementSet closes over what a coherent partial model
            // drags along, and Scoped SLICES that closure into its own graph, so "everything on storey 3 in the
            // plumbing domain" emits as a conforming standalone IFC in one expression. No scope = the whole graph.
            // Only "ifc"-classified nodes are IFC-representable: a foreign-system node (a sibling projector's native
            // capture) is out of scope by classification rather than by fault. The roster resolves ONCE off the sliced
            // model so the admission fact's node count and the authoring fold read one set.
            return Scoped(graph, ctx, key).Bind(model => {
                Seq<Node.Object> targets = model.Nodes.Values
                    .Choose(static node => node is Node.Object { Classification.System: "ifc" } obj ? Some(obj) : None)
                    .ToSeq();
                // The rasm.bim.projection.emit VETO point brackets the WHOLE authoring: an app deliverable policy
                // refuses on the elected wire form, the raised target schema, and the in-scope node magnitude BEFORE a
                // DatabaseIfc exists, so a refusal is the emit's typed verdict and no entity, no unit declaration, and
                // no byte is ever authored. The GUARDED fire is the one shape — a bare Fire followed by an
                // unconditional write would run exactly the work the veto exists to stop — and a hook-less composition
                // takes the identical rail with the body applied directly, paying one IsNone test.
                BimFact.Egress admission = new(key, form.Key, model.Header.Schema.Key, targets.Count);
                return ctx.Hooks.Match(
                    Some: hooks => hooks.Egress.Fire(admission, _ => Write(model, targets, form, release, ctx, key)),
                    None: () => Write(model, targets, form, release, ctx, key));
            });
        });

    // A SCOPED deliverable is a real SLICE, never a filter over the whole graph. The Bim Closure elects WHICH nodes an
    // IFC deliverable needs (the spatial ancestor chain to the root plus each member's bound type), then the seam
    // Graph/element#FEDERATION Extract slices that root set into its own graph under the SOURCE Header — an edge joins
    // only with its WHOLE Members set inside, so nothing dangles and the re-author legs below never meet a joint one
    // of whose ends the slice dropped. The per-relationship endpoint compensation those legs still carry answers the
    // FEDERATED emit alone — a foreign-system endpoint this projector never authors, which no slice can close.
    static Fin<ElementGraph> Scoped(ElementGraph graph, EmitContext ctx, Op key) =>
        ctx.Scope.Match(
            Some: selection => graph.Extract(Closure(graph, selection.Ids).ToSeq(), key),
            None: () => Fin.Succ(graph));

    // The write the emit veto gates: construct the database at the raised release, author every in-scope node, then
    // run the re-author legs and seal. Split from Emit so the veto brackets an expression rather than a lambda body,
    // and so the admission fact and the authoring fold read the ONE resolved target roster.
    Fin<ReadOnlyMemory<byte>> Write(ElementGraph graph, Seq<Node.Object> targets, IfcWireForm form, GGRelease release, EmitContext ctx, Op key) {
        Option<ElementGraph> prior = ctx.Prior;
        // The release-only ctor is the whole construction: it seeds NO default project, owner history, or unit
        // assignment, which is exactly what an emit authoring its own IfcProject and its own IfcUnitAssignment
        // needs. The `(bool generate, ReleaseVersion)` pair that spelled the same intent chains this ctor and then
        // conditionally seeds — it is retired upstream and its `false` arm is a no-op over this one.
        var target = new DatabaseIfc(release) { Tolerance = graph.Header.Tolerance };
        // Re-ingest correlation [H6]: the neutral rooted NodeId is freshly minted each Project, so a re-imported
        // graph shares NO NodeId with the prior snapshot — the diff-derived ChangeAction matches a rooted node on
        // the stable 1:1 GlobalId (Node.Object.ExternalId), indexed once here, the NodeId fallback covering a
        // from-scratch node.
        var priorByExternal = prior.Map(static p => p.Nodes.Values
                .Choose(static n => n is Node.Object o ? o.ExternalId.Map(ext => (Ext: ext, Node: o)) : None)
                .Fold(Map<string, Node.Object>(), static (m, e) => m.AddOrUpdate(e.Ext, e.Node)))
            .IfNone(Map<string, Node.Object>());
        var histories = new Dictionary<IfcChangeActionEnum, IfcOwnerHistory>();
        return targets
            .TraverseM(obj => Author(target, obj, graph.Header.Schema, graph.Header.Tolerance, key, prior, priorByExternal, histories).Map(entity => (Id: obj.Id, Entity: entity)))
            .As()
            .Map(static entities => entities.Fold(Map<NodeId, IfcObjectDefinition>(), static (m, e) => m.AddOrUpdate(e.Id, e.Entity)))
            .Bind(authored => {
                // The declared-unit-regime raise [P1]: the caller-chosen regime (default: the model's own
                // Header.Units declared scheme — a mm-source import re-emits mm; empty = SI verbatim) authors the
                // matching IfcUnitAssignment on the authored context, and the inverse per-axis UnitScale derived
                // OFF the constructed database folds every raised magnitude — properties, quantities, the
                // tolerance — through the one dimensional factor; the map-conversion offset leg stays the geo
                // owner's SI author, the one residual named row of the regime.
                UnitScheme regime = ctx.Units.IfNone(graph.Header.Units);
                DeclareUnits(target, regime);
                UnitScale emitScale = UnitScale.Of(target);
                // The tolerance crosses through the ONE Declare transform like every other magnitude — a bare
                // division by the length axis is the call-site factor multiply the coercion pair exists to delete.
                target.Tolerance = emitScale.Declare(graph.Header.Tolerance, MeasureRow.Length, null);
                return ReauthorMaterials(target, graph, authored, emitScale, key)
                    .Bind(materials => ReauthorProperties(target, graph, authored, emitScale, key)
                        .Map(properties => materials.Log + properties.Log))
                    .Bind(log => { ReauthorClassifications(target, graph, authored); return ReauthorRelationships(target, graph, authored, key).Map(rel => log + rel.Log); })
                    .Bind(log => {
                        ReauthorProject(graph, authored);
                        ReauthorHeader(target, graph.Header.Step);
                        return ReauthorStructural(target, graph, authored, key).Map(structural => log + structural.Log);
                    })
                    // The georeference round-trip inverse [M1]: Header.Reference re-authors IfcProjectedCRS/
                    // IfcMapConversion (or the IfcSite geographic position) through the CRS mechanics owner — a
                    // LoGeoRef-50 model exporting geo-stripped was the named drop this compose closes. It is a
                    // RAIL RUNG: a projectless database and an absent anchor entity are typed faults the emit
                    // aborts on, and the returned GeoAuthored level is the evidence the receipt folds — a
                    // discarded call reported a written frame it never wrote. `emitScale` is the MODEL regime the
                    // site elevation alone rides; the authored IfcProjectedCRS declares no MapUnit, so the map
                    // ordinates land metre-verbatim.
                    // GeoAuthored.Conversion answers BOTH a genuinely isotropic frame AND an anisotropic one whose
                    // pre-IFC4X3_ADD2 target carries no IfcMapConversionScaled, so this COUNTED collapse separates
                    // them on the seam frame's OWN derived Scale — None exactly when the three axes disagree. That
                    // read is the seam's declared isotropy, never a re-derived level election, so the election's
                    // tolerance stays the geo owner's alone.
                    .Bind(log => GeoReferenceProjector.Author(target, graph.Header.Reference, emitScale, key)
                        .Map(level => level == GeoAuthored.Conversion && graph.Header.Reference.Scale.IsNone
                            ? log.Note(FidelityDrop.GeoLevelLowered, level.Key)
                            : log))
                    .Bind(log => {
                        // The writer's ENTRY name: the seam header's own FILE_NAME stem under the row's
                        // extension, so GeometryGym's container dispatch reads the row and a zipped emit names
                        // its inner entry after the model. A nameless header falls to the release token rather
                        // than an empty stem, because the writer derives the zip entry from this string.
                        string entry = $"{(Path.GetFileNameWithoutExtension(graph.Header.Step.Name) is { Length: > 0 } stem ? stem : release.ToString())}{form.Extension}";
                        // The ONE run-edge Land for the egress half: every leg RETURNED its facts on the Noted
                        // rail and the joined log enters the projector's ledger exactly once, so a refused write
                        // charges the receipt for nothing and a rerun re-derives the same ledger.
                        return form.Seal(target, entry)
                            .ToFin(new BimFault.CodecReject(key, $"ifc-write-refused:{form.Key}"))
                            .Map(bytes => Land(log, bytes));
                    });
            });
    }

    // The scoped-emit closure — the ONE owned law of what a coherent partial model drags along: every selected node,
    // its transitive spatial ancestor chain (Contain first, Aggregate second — the same up-chain the query Ancestry
    // reach walks) to the IfcProject root, and each member's bound type object. Bags, materials, and classifications
    // need no closure rows: the re-author folds gate on authored subjects, so a bag bound only to out-of-scope
    // subjects never authors. Relationship coherence is the SLICE's, not this fold's — Scoped hands these roots to
    // the seam Extract, whose Members-closed walk pulls every reached edge's full member set in.
    static LanguageExt.HashSet<NodeId> Closure(ElementGraph graph, LanguageExt.HashSet<NodeId> selected) {
        Seq<NodeId> ancestors = selected.ToSeq().Bind(id => AncestorChain(graph, id, Seq<NodeId>()));
        Seq<NodeId> types = selected.ToSeq().Bind(id => graph.EdgesAt(id)
            .Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.TypeDefinition && a.Subject == id
                ? Some(a.Definition) : None).ToSeq());
        return selected.TryAddRange(ancestors).TryAddRange(types);
    }

    static Seq<NodeId> AncestorChain(ElementGraph graph, NodeId node, Seq<NodeId> seen) =>
        graph.EdgesAt(node)
            .Choose(e => e is Relationship.Compose c && c.Part == node
                && (c.SubKind == ComposeKind.Contain || c.SubKind == ComposeKind.Aggregate) ? Some(c.Whole) : None)
            .ToSeq().Head
            .Filter(parent => !seen.Contains(parent))
            .Map(parent => parent.Cons(AncestorChain(graph, parent, seen.Add(parent))))
            .IfNone(Seq<NodeId>());

    // The declared-regime unit author: the scheme's Length row resolves its GG length family through the frozen
    // UnitsNet-name -> GG-enum table and lands the IfcUnitAssignment on the authored context (GG fills the derived
    // area/volume conversions and the SI residue); an empty or unmapped scheme keeps the GG SI defaults — the
    // value-equivalent SI-declared emit, total either way. The non-length declared axes ride the SI residue, the
    // named bounded residual of the regime. Metre carries its OWN row rather than falling through the unmapped tail:
    // an explicitly metre-declared model must AUTHOR its metre declaration, and a factor-of-one fallthrough left it
    // indistinguishable from an undeclared model — the exact confusion the UnitAxis token exists to end, so the
    // regime is read as a TOKEN here and never as a float compare.
    static readonly FrozenDictionary<string, IfcUnitAssignment.Length> LengthRegimes = new Dictionary<string, IfcUnitAssignment.Length>(StringComparer.Ordinal) {
        ["Meter"] = IfcUnitAssignment.Length.Metre,
        ["Centimeter"] = IfcUnitAssignment.Length.Centimetre,
        ["Millimeter"] = IfcUnitAssignment.Length.Millimetre,
        ["Foot"] = IfcUnitAssignment.Length.Foot,
        ["Inch"] = IfcUnitAssignment.Length.Inch,
        ["UsSurveyFoot"] = IfcUnitAssignment.Length.USSurveyFoot,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static void DeclareUnits(DatabaseIfc target, UnitScheme regime) =>
        regime.UnitFor(QuantityType.Length)
            .Bind(token => LengthRegimes.TryGetValue(token, out IfcUnitAssignment.Length family) ? Some(family) : None)
            .IfSome(family => { target.Project.UnitsInContext = new IfcUnitAssignment(target, family); });

    // The seam->GG raise this page owns beside Sniff: the frozen ReleaseMap.Raise identity-name inverse of the ingress
    // ReleaseLower — a seam schema with no GG writer (Ifc5) faults CodecReject BARE; the IFC4X3_ADD2 silent default is
    // the deleted form.
    internal static Fin<GGRelease> ReleaseRaise(ReleaseVersion schema, Op key) =>
        ReleaseMap.Raise.TryGetValue(schema, out GGRelease raised)
            ? Fin.Succ(raised)
            : Fin.Fail<GGRelease>(new BimFault.CodecReject(key, $"release-unraisable:{schema.Key}"));

    // The egress gate: resolve the IfcClass row from the generic Classification code, reject the schema-abstract
    // supertype (classification vocabulary, never an authored entity class), admit the predefined token against the
    // per-token PredefinedRow spans AND the class schema span [PREDEFINED_TOKEN_RULING][H8], then construct the entity, STAMP the admitted
    // token, and round-trip the GlobalId [H6] + diff-derived OwnerHistory [H9]. The map is IfcObjectDefinition-wide:
    // a type node authors its IfcTypeObject subtype and the context root authors IfcProject through the db-binding
    // ctor that wires DatabaseIfc.Context — neither is an IfcProduct, so a product-typed mint is the deleted form.
    static Fin<IfcObjectDefinition> Author(DatabaseIfc target, Node.Object obj, ReleaseVersion schema, double tolerance, Op key, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal, Dictionary<IfcChangeActionEnum, IfcOwnerHistory> histories) =>
        IfcClass.Resolve(obj.Classification.Code, key)
            .Bind(cls => !cls.Instantiable
                ? Fin.Fail<IfcObjectDefinition>(new BimFault.UnmappedClass(key, $"abstract-class-at-egress:{cls.Key}"))
                : cls.AdmitPredefined(obj.PredefinedType.Token, obj.ObjectType.IfNone(""), schema, key)
                    .Bind(token => {
                        var entity = (IfcObjectDefinition)(cls == IfcClass.Project
                            ? new IfcProject(target, obj.Name)
                            : target.Factory.Construct(cls.Key));
                        entity.GlobalId = obj.ExternalId.IfNone(() => ParserIfc.EncodeGuid(ContentGuid(obj, tolerance)));
                        entity.Name = obj.Name;
                        return StampPredefined(entity, token, obj.ObjectType, key).Map(_ => {
                            obj.History.IfSome(_ => entity.OwnerHistory = OwnerHistoryOf(target, histories, ChangeOf(obj, prior, priorByExternal)));
                            return entity;
                        });
                    }));

    // The from-scratch GlobalId is DERIVED, never random [H6]. A node the ingest rooted re-emits its ExternalId 1:1;
    // a node no IFC file ever carried mints its identifier from its OWN content — the id-INCLUSIVE ContentAddress (the
    // kernel seed-zero XxHash128 over the node id plus its canonical bytes) is exactly 128 bits, exactly a Guid, and
    // ParserIfc.EncodeGuid compresses it to the 22-character IFC identifier. REPRODUCIBILITY IS THE LAW: re-emitting
    // an unchanged node re-mints a BYTE-IDENTICAL GlobalId, so a re-export of an unedited graph diffs empty and a
    // downstream reference keyed on that GlobalId survives the round trip; the id-inclusive address keeps two
    // occurrences of identical content distinct, which a content-EXCLUSIVE address could not. Guid.NewGuid() re-keyed
    // every from-scratch entity on every emit — the deleted form that turned each re-export into a whole-file diff and
    // silently broke every external reference into the model.
    static Guid ContentGuid(Node.Object obj, double tolerance) {
        Span<byte> address = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(address, ContentAddress.Of(obj, tolerance).Value);
        return new Guid(address, bigEndian: true);
    }

    // The admitted token stamps the entity's OWN Ifc*TypeEnum property — the per-class enum type is the entity's, so
    // the slot resolves reflectively like the relations Author fills its Relating/Related slots — and USERDEFINED
    // additionally authors the user-defined label on its owner (IfcObject.ObjectType for an occurrence,
    // IfcElementType.ElementType for a type); GeometryGym's own validPredefinedType setter guard sits beneath the
    // AdmitPredefined gate as defense-in-depth. A class with no PredefinedType property skips silently. The label is
    // the seam Object node's own ObjectType column VERBATIM — the Projection/semantic UserLabel read reversed onto the
    // same two entity slots — so no bag row, no attachment edge, and no egress index stand between the ends of the
    // round-trip. A USERDEFINED node carrying None never reaches here: AdmitPredefined already faulted it
    // predefined-objecttype-miss, and the Name substitution that masked that malformed pair is DELETED — it collapsed
    // two same-named entities holding distinct labels onto one. The parse RAILS: a class that publishes a
    // PredefinedType slot and a token its own enum cannot parse ships an entity whose type reads NOTDEFINED where the
    // source declared a value, and the silent skip made that indistinguishable from a class carrying no slot at all —
    // so the two cases are now two verdicts, the slotless class succeeding vacuously and the unparseable token
    // faulting under the same UnmappedClass band the AdmitPredefined gate above uses.
    static Fin<Unit> StampPredefined(IfcObjectDefinition entity, string token, Option<string> objectType, Op key) {
        if (token == "USERDEFINED") {
            objectType.IfSome(label => {
                switch (entity) {
                    case IfcObject occurrence: occurrence.ObjectType = label; break;
                    case IfcElementType type: type.ElementType = label; break;
                }
            });
        }
        if (entity.GetType().GetProperty(nameof(PredefinedType)) is not { CanWrite: true, PropertyType.IsEnum: true } slot) {
            return Fin.Succ(unit);
        }
        if (!Enum.TryParse(slot.PropertyType, token, ignoreCase: true, out object? member)) {
            return Fin.Fail<Unit>(new BimFault.UnmappedClass(key, $"predefined-token-unstampable:{entity.GetType().Name}:{token}"));
        }
        slot.SetValue(entity, member);
        return Fin.Succ(unit);
    }

    // The ChangeAction is the diff verdict against the prior snapshot, never a blanket stamp [H9]: a rooted node
    // matches the prior on the stable 1:1 GlobalId (ExternalId) ACROSS re-ingest since the NodeId is freshly minted
    // each ingest, falling back to the NodeId for a from-scratch node — absent prior -> ADDED; present prior -> the
    // generated Node.Object.EqualityComparer structured diff decides NOCHANGE/MODIFIED, the lazy Inequalities
    // enumeration short-circuiting on the first member difference — the freshly-minted Id is outside the verdict BY
    // CONSTRUCTION, the seam Node owner declaring [IgnoreEquality] on the case's Id override, so an egress-side path
    // filter and the id-normalizing with{} clone-then-Equals are both the deleted form.
    static IfcChangeActionEnum ChangeOf(Node.Object obj, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal) {
        Option<Node.Object> before = obj.ExternalId.Bind(ext => priorByExternal.Find(ext))
            | prior.Bind(graph => graph.Find(obj.Id)).Bind(static n => n is Node.Object o ? Some(o) : Option<Node.Object>.None);
        return before.Match(
            None: static () => IfcChangeActionEnum.ADDED,
            Some: previous => Node.Object.EqualityComparer.Default.Inequalities(previous, obj).Any()
                ? IfcChangeActionEnum.MODIFIED
                : IfcChangeActionEnum.NOCHANGE);
    }

    // One IfcOwnerHistory entity per DISTINCT ChangeAction, memoized for the emit: ADDED is the canonical factory
    // stamp verbatim; MODIFIED/NOCHANGE mint ONCE through Factory.Construct, the canonical stamp donating the owning
    // user/application. Mutating the factory's single OwnerHistoryAdded per node is the deleted aliasing form — every
    // earlier assignment references that one record, so a later action retro-flipped the whole emit.
    static IfcOwnerHistory OwnerHistoryOf(DatabaseIfc target, Dictionary<IfcChangeActionEnum, IfcOwnerHistory> histories, IfcChangeActionEnum change) {
        if (change == IfcChangeActionEnum.ADDED) { return target.Factory.OwnerHistoryAdded; }
        if (!histories.TryGetValue(change, out IfcOwnerHistory? history)) {
            IfcOwnerHistory canonical = target.Factory.OwnerHistoryAdded;
            history = (IfcOwnerHistory)target.Factory.Construct(nameof(IfcOwnerHistory));
            history.OwningUser = canonical.OwningUser;
            history.OwningApplication = canonical.OwningApplication;
            history.ChangeAction = change;
            histories[change] = history;
        }
        return history;
    }

    // The schema sniff [H8]: read the quoted FILE_SCHEMA token / the ifcJSON schemaIdentifier member / the ifcXML
    // xmlns schema URI off the bytes before constructing the database, parse it onto the GG ReleaseVersion, and gate
    // membership in the frozen ReleaseMap.Lower key set (IFC4X4_DRAFT excluded by law) — an absent OR unreadable
    // header (a malformed JSON payload funnels through Try.lift, never a thrown escape off the Fin rail), an
    // unparseable token, or an unadmitted release faults CodecReject BARE; the silent IFC4X3_ADD2 default is DELETED.
    public static Fin<GGRelease> Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format, Op key) {
        var token = format == InterchangeFormat.IfcJson
            ? Try.lift(() => Optional((JsonNode.Parse(bytes.Span) as JsonObject)?["schemaIdentifier"]?.ToString())).Run().ToOption().Flatten()
            : format == InterchangeFormat.IfcXml
                ? XmlSchemaToken(bytes.Span)
                : StepSchemaToken(bytes.Span);
        return token.Match(
            None: () => Fin.Fail<GGRelease>(new BimFault.CodecReject(key, "schema-header-missing")),
            Some: raw => Enum.TryParse(raw, ignoreCase: true, out GGRelease sniffed) && ReleaseMap.Lower.ContainsKey(sniffed)
                ? Fin.Succ(sniffed)
                : Fin.Fail<GGRelease>(new BimFault.CodecReject(key, $"schema-header-unmapped:{raw}")));
    }

    // The header window both probes decode: a schema declaration that is not in the first few KiB of an IFC payload is
    // not a header, so the read is bounded rather than materializing an arbitrary file as a string.
    const int HeaderProbeBytes = 4096;

    static readonly char[] XmlTokenEnd = ['/', '"', '\''];
    static readonly char[] StepTokenEnd = ['\''];

    // The ifcXML header xmlns schema URI — ".../ifcXML/<release>[/AddN]" (IFC2x3 FINAL, IFC4 Add2, IFC4X3): the first
    // path segment after "ifcXML/" is the release token Enum.TryParse admits, so an ifcXML payload sniffs its true
    // schema instead of falling through the STEP probe to schema-header-missing. The segment starts AT the marker, so
    // the reader passes no opening delimiter.
    static Option<string> XmlSchemaToken(ReadOnlySpan<byte> bytes) =>
        Delimited(Encoding.UTF8.GetString(bytes[..Math.Min(bytes.Length, HeaderProbeBytes)]),
            "ifcXML/", StringComparison.OrdinalIgnoreCase, opening: '\0', XmlTokenEnd);

    // The FILE_SCHEMA(('IFC4X3_ADD2')) token between the first quote pair after the keyword — the quote IS the opening
    // delimiter, which is the whole difference between the two probes.
    static Option<string> StepSchemaToken(ReadOnlySpan<byte> bytes) =>
        Delimited(Encoding.ASCII.GetString(bytes[..Math.Min(bytes.Length, HeaderProbeBytes)]),
            "FILE_SCHEMA", StringComparison.Ordinal, opening: '\'', StepTokenEnd);

    // The ONE marker-then-delimited-slice read both header probes take, expression-shaped and TOTAL: a missing marker,
    // a missing opening delimiter, and an unterminated token are three Nones, never an index that walks off the
    // window. '\0' as the opening delimiter means the token starts immediately after the marker.
    static Option<string> Delimited(string header, string marker, StringComparison how, char opening, char[] closing) =>
        header.IndexOf(marker, how) switch {
            < 0 => None,
            var at => (opening == '\0' ? at + marker.Length : header.IndexOf(opening, at) + 1) switch {
                <= 0 => None,
                var start => header.IndexOfAny(closing, start) switch {
                    var end when end > start => Some(header[start..end]),
                    _ => None,
                },
            },
        };

    // The seam Material subgraph -> IFC: each Material node authors its type-level definition + Psets ONCE through
    // MaterialProjection.AuthorComposition, then each incident Associate edge authors the per-occurrence MaterialUsage
    // [OCCURRENCE_USAGE_RULING] wrapping the shared definition (AuthorUsage) and the IfcRelAssociatesMaterial onto the bound element — so a
    // wall and its mirror share one IfcMaterialLayerSet with two IfcMaterialLayerSetUsage instances. REPLACES the
    // retired Materials wire carriers; the material reads off the projected graph, never a wire. Instance member: the
    // profiles resolver is the ingress-part capture-promoted field, never a re-passed parameter. A ProfileSet with no
    // preserved STEP fragment resolves its profile-def subtype from the carried DetailSchema.Realization ProfileSubtype
    // row (ProfileSubtypeOf below) — the Materials-seeded occupancy token — so the profile lane reads the carried row.
    // `scale` is the inverse UnitScale derived off the constructed database, so the layer thickness and the
    // layer/profile usage offsets re-declare in the emitted file's own length unit rather than landing SI metres
    // inside a millimetre deliverable.
    // EMIT-SCOPED like the bag egress: the usage fold binds only AUTHORED subjects, so a scoped export authors a
    // material once for its surviving usages and a material whose every subject is out-of-scope or foreign-system
    // never authors — the per-subject DanglingReference fault fired on legitimate federated/partial emits and is
    // the deleted form; a truly-corrupt graph still faults at the seam Link law before any emit sees it.
    Fin<Noted<Unit>> ReauthorMaterials(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, UnitScale scale, Op key) =>
        graph.Nodes.Values.Choose(static n => n is Node.Material m ? Some(m) : None)
            .Map(material => (Material: material, Usages: graph.EdgesAt(material.Id)
                .Choose(e => e is Relationship.Associate a && a.Resource == material.Id && authored.ContainsKey(a.Subject) ? Some(a) : None)
                .ToSeq()))
            .Filter(static row => !row.Usages.IsEmpty)
            .TraverseM(row => MaterialProjection.AuthorComposition(target, row.Material, profiles, ProfileSubtypeOf(graph, row.Material.Id), scale).Bind(definition =>
                row.Usages
                    .TraverseM(edge => MaterialProjection.AuthorUsage(definition, edge.Usage, scale)
                        .Bind(select => Register(new IfcRelAssociatesMaterial(select, Seq((IfcDefinitionSelect)authored[edge.Subject])), key)))
                    .As().Map(static _ => unit)))
            .As().Map(static _ => Noted.Clean(unit));

    // The ONE named registration seam for GeometryGym's construct-registers idiom: an IfcRel* constructor BINDS the
    // entity onto its database as a ctor side effect, so construction IS the authoring act and the reference is spent
    // at the call. Register names that act, so a re-author leg reads as a step instead of scattering `ignore(new
    // IfcRel…)` expressions whose discarded value reads as a dropped result. The database binding is the ctor's own
    // contract, so an entity that comes back unbound is a GG contract break the emit refuses rather than a silent
    // orphan the writer never serializes. STATEMENT EXEMPTION: the construction is the effect, and no expression form
    // can both perform it and name it.
    static Fin<Unit> Register(IfcRelationship registered, Op key) =>
        Optional(registered.Database)
            .ToFin(new BimFault.CodecReject(key, $"relationship-unregistered:{registered.GetType().Name}"))
            .Map(static _ => unit);

    // The carried profile-subtype read: the material's Associate subject binds its DetailSchema.Realization bag
    // through Assign.PropertyDefinition, and the ProfileSubtype row is the Materials-seeded occupancy-derived
    // profile-def token (the cmu IfcArbitraryProfileDefWithVoids/IfcRectangleProfileDef decision) — the subtype
    // resolves off the carried graph row, so the profile lane never calls an AEC peer.
    static Option<string> ProfileSubtypeOf(ElementGraph graph, NodeId materialId) =>
        graph.EdgesAt(materialId)
            .Choose(e => e is Relationship.Associate a && a.Resource == materialId ? Some(a.Subject) : None)
            .Bind(subject => graph.EdgesAt(subject)
                .Choose(e => e is Relationship.Assign g && g.SubKind == AssignKind.PropertyDefinition && g.Subject == subject ? Some(g.Definition) : None))
            .Choose(definition => graph.Nodes.Find(definition).Case is Node.PropertySet { Bag: var bag } && bag.SetName == DetailSchema.Realization.SetName
                ? bag.Find(DetailSchema.ProfileSubtype).Bind(static v => v is PropertyValue.Text t ? Some(t.Value) : Option<string>.None)
                : Option<string>.None)
            .ToSeq()
            .Head;

    // The property/quantity bags -> IFC, RAILED and EMIT-SCOPED: the Assign.PropertyDefinition index decides which
    // bag nodes are THIS wire's data — a bag binding at least one authored subject authors ONCE, a bag with NO
    // attachment edge authors too (an unbound source Pset round-trips), and a bag bound ONLY to foreign-system
    // subjects is a sibling projector's capture and never authors (GG writes every constructed entity, so a
    // federated emit would otherwise strand orphan IfcPropertySets); the projector-minted TypeSignatureSet /
    // PortAttributeSet / StructuralDefinitionSet / PositioningAttributeSet / ProjectAttributeSet bags are
    // reconciliation and entity-attribute bookkeeping and never author — the port flow, structural definition, and
    // project Phase/LongName attributes re-author on the ENTITY at Emit, the station rows stay ingest-landed evidence
    // whose IfcLinearPlacement re-author is the named bounded drop the fidelity receipt counts, so exporting a
    // synthesized bag would mint a phantom Pset the source never carried. An unraisable quantity dimension aborts typed, never coerces;
    // each authored-subject edge then authors the IfcRelDefinesByProperties onto its element — the round-trip the
    // retired stringly BimElement.PropertyBinding never had.
    Fin<Noted<Unit>> ReauthorProperties(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, UnitScale scale, Op key) {
        Map<NodeId, Seq<Relationship.Assign>> attachments = graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Assign a && a.SubKind == AssignKind.PropertyDefinition ? Some(a) : None)
            .Fold(Map<NodeId, Seq<Relationship.Assign>>(), static (m, a) => m.AddOrUpdate(a.Definition, s => s.Add(a), () => Seq(a)));
        // Each ingest-landed positioning bag whose subject THIS emit authors is a COUNTED linear-placement drop: the
        // station rows are evidence, the IfcLinearPlacement entity re-anchors from content-keyed geometry rather than
        // re-authoring from scalars. The authored gate is the same one the bag egress applies below and it is
        // load-bearing on the ledger: a scoped or federated emit that counted every positioning bag in the graph
        // would report drops against elements it never wrote, and an over-counted receipt is as unusable as a silent
        // one. The scan FOLDS its facts into a log rather than swapping a cell, so the count is a value this leg
        // returns.
        FidelityLog placements = graph.Nodes.Values.AsIterable()
            .Choose(static n => n is Node.PropertySet { Bag.SetName: var set } p && set == PositioningAttributeSet ? Some(p) : None)
            .Filter(p => attachments.Find(p.Id).Exists(edges => edges.Exists(a => authored.ContainsKey(a.Subject))))
            .Fold(FidelityLog.Empty, static (log, p) => log.Note(FidelityDrop.LinearPlacement, p.Id.Value));
        return graph.Nodes.Values
            .Filter(node => (node is not Node.PropertySet ps
                    || (ps.Bag.SetName != TypeSignatureSet && ps.Bag.SetName != PortAttributeSet
                        && ps.Bag.SetName != StructuralDefinitionSet && ps.Bag.SetName != PositioningAttributeSet
                        && ps.Bag.SetName != ProjectAttributeSet))
                && attachments.Find(node.Id).Match(Some: edges => edges.Exists(a => authored.ContainsKey(a.Subject)), None: () => true))
            .Choose(node => AuthorBag(target, node, authored, scale, key).Map(fin => fin.Map(noted => noted.Map(set => (Id: node.Id, Set: set)))))
            .TraverseM(identity).As()
            .Bind(rows => {
                Noted<Seq<(NodeId Id, IfcPropertySetDefinition Set)>> authoredBags = Noted.Join(rows.ToSeq());
                Map<NodeId, IfcPropertySetDefinition> bags = authoredBags.Value.Fold(Map<NodeId, IfcPropertySetDefinition>(), static (m, e) => m.AddOrUpdate(e.Id, e.Set));
                // Each authored-subject attachment edge registers its IfcRelDefinesByProperties through the ONE named
                // registration seam, so the round-trip the retired stringly BimElement.PropertyBinding never had is a
                // railed step rather than a discarded construction.
                return toSeq(attachments.Values).Flatten()
                    .Choose(a => bags.Find(a.Definition).Bind(set => authored.Find(a.Subject).Map(subject => (Subject: subject, Set: set))))
                    .TraverseM(pair => Register(new IfcRelDefinesByProperties(pair.Subject, pair.Set), key))
                    .As().Map(_ => new Noted<Unit>(placements + authoredBags.Log, unit));
            });
    }

    // The empty-bag guard is load-bearing: the IfcPropertySet(name, IEnumerable)/IfcElementQuantity(name, IEnumerable)
    // ctors derive their database from the FIRST member (`members.First().mDatabase`), so an empty bag would throw at
    // the boundary — an empty Pset/Qto carries no IFC data, so it is skipped (its DefinesByProperties edge then
    // resolves no bag and re-authors nothing), lossless and exception-free, never a crashing `.First()`.
    static Option<Fin<Noted<IfcPropertySetDefinition>>> AuthorBag(DatabaseIfc target, Node node, Map<NodeId, IfcObjectDefinition> authored, UnitScale scale, Op key) => node switch {
        Node.PropertySet ps when !ps.Bag.Values.IsEmpty => Some(ps.Bag.Values.AsIterable().ToSeq()
            .TraverseM(kv => RaiseProperty(target, authored, kv.Key, kv.Value, scale, key)).As()
            .Map(raised => Noted.Join(raised).Map(properties => (IfcPropertySetDefinition)new IfcPropertySet(ps.Bag.SetName, properties)))),
        Node.QuantitySet qs when !qs.Bag.Values.IsEmpty => Some(
            RaiseQuantities(target, qs.Bag, scale, key)
                .Map(quantities => Noted.Clean((IfcPropertySetDefinition)new IfcElementQuantity(qs.Bag.SetName, quantities)))),
        _ => Option<Fin<Noted<IfcPropertySetDefinition>>>.None,
    };

    // The NESTED quantity rebuild, the exact inverse of the ingress FlattenQuantities: every bag Groups row names a
    // dot-path prefix whose GroupIdentity re-authors ONE IfcPhysicalComplexQuantity carrying the restored
    // Discrimination/Quality/Usage, its member values re-authored beneath it under their leaf names, and ungrouped
    // values stay flat on the IfcElementQuantity. Prefix-only reconstruction — flat rows whose dotted key spelling was
    // the whole grouping evidence — is the DELETED lossy form: it re-emitted a group's identity as nothing and its
    // nesting as a name, so a consuming takeoff read one flat set where the source carried a classified hierarchy.
    static Fin<Seq<IfcPhysicalQuantity>> RaiseQuantities(DatabaseIfc target, QuantityBag bag, UnitScale scale, Op key) =>
        bag.Values.AsIterable().ToSeq().TraverseM(kv => RaiseMember(target, bag.Groups, kv.Key, kv.Value, scale, key)).As()
            .Map(raised => Nest(bag.Groups, raised, ""));

    // A value re-authors under its OWNING group with its LEAF name (the ingress `{prefix}{Name}` join reversed), and
    // ungrouped under its WHOLE key — so a name that merely contains a dot is never split by a group it never joined.
    static Fin<(string Owner, IfcPhysicalQuantity Quantity)> RaiseMember(
        DatabaseIfc target, Map<string, GroupIdentity> groups, PropertyName name, MeasureValue measure, UnitScale scale, Op key) =>
        from owner in Fin.Succ(OwnerOf(groups, name))
        from quantity in RaiseQuantity(target, owner.Length == 0 ? name : PropertyName.Create(Leaf(name.Value)), measure, scale, key)
        select (Owner: owner, Quantity: quantity);

    // The owning group of a value key: the LONGEST Groups prefix the dotted key sits under, "" for an ungrouped row —
    // so a value under "A.B" binds to "A.B" and never to its ancestor "A", which is what makes the rebuild's nesting
    // depth equal the source's rather than collapsing every descendant onto the outermost group.
    static string OwnerOf(Map<string, GroupIdentity> groups, PropertyName name) =>
        toSeq(toSeq(groups.Keys)
            .Filter(prefix => name.Value.StartsWith($"{prefix}.", StringComparison.Ordinal))
            .OrderByDescending(static prefix => prefix.Length))
            .Head.IfNone("");

    // One level of the rebuild: the values owned by `parent` beside one complex quantity per CHILD group of `parent`,
    // each recursing its own level, so a nest the ingest walked N deep re-authors N deep. The complex ctor derives its
    // database from its FIRST member, so a group whose whole subtree is empty authors nothing rather than throwing.
    static Seq<IfcPhysicalQuantity> Nest(Map<string, GroupIdentity> groups, Seq<(string Owner, IfcPhysicalQuantity Quantity)> raised, string parent) =>
        raised.Filter(row => row.Owner == parent).Map(static row => row.Quantity)
        + toSeq(groups).Filter(entry => ParentOf(entry.Key) == parent).Choose(entry =>
            Nest(groups, raised, entry.Key) is { IsEmpty: false } children
                ? Some((IfcPhysicalQuantity)new IfcPhysicalComplexQuantity(Leaf(entry.Key), children, entry.Value.Discrimination.IfNone("")) {
                    Quality = entry.Value.Quality.IfNone(""),
                    Usage = entry.Value.Usage.IfNone(""),
                })
                : Option<IfcPhysicalQuantity>.None);

    // The dot-path split both ends share — the parent prefix ("" for a root group) and the trailing segment the IFC
    // Name carries. Discrimination re-authors through the ctor because GG writes it unconditionally (schema-mandatory);
    // Quality/Usage assign after, an absent Option restoring the empty spelling GG writes back as the IFC `$`.
    static string ParentOf(string path) => path.LastIndexOf('.') is var cut && cut > 0 ? path[..cut] : "";

    static string Leaf(string path) => path.LastIndexOf('.') is var cut && cut >= 0 ? path[(cut + 1)..] : path;

    // The seam PropertyValue -> the IFC property re-author, the exact VALUE_NARROWING inverse over the generated TOTAL
    // Switch (a new seam case breaks HERE at compile time, never a silent string arm): every typed case rebuilds
    // its IFC counterpart — Text its scalar IfcPropertySingleValue, Boolean its typed bool, Measure ITS OWN typed
    // IfcValue through the derived RaiseMeasure mint (never a flattened IfcReal), the three-valued Logical a
    // typed IfcLogical (UNKNOWN survives), Enumerated its SELECTED list plus the IfcPropertyEnumeration allowed set
    // when the seam carries one, Reference the UsageName carrier PLUS its re-attached PropertyReference when the
    // seam target resolves to an authored select member, Bounded the lower/upper/setpoint entity, List the
    // typed value list, Table the CurveInterpolation carrier, Complex the IfcComplexProperty RECURSING this raise —
    // so no import->export cycle degrades a typed case to its Render string or a measure to a bare real.
    static Fin<Noted<IfcProperty>> RaiseProperty(DatabaseIfc target, Map<NodeId, IfcObjectDefinition> authored, PropertyName name, PropertyValue value, UnitScale scale, Op key) =>
        value.Switch<(DatabaseIfc Db, Map<NodeId, IfcObjectDefinition> Authored, PropertyName Name, UnitScale Scale, Op Key), Fin<Noted<IfcProperty>>>(
            state: (Db: target, Authored: authored, Name: name, Scale: scale, Key: key),
            text:       static (s, t) => Fin.Succ(Noted.Clean<IfcProperty>(new IfcPropertySingleValue(s.Db, s.Name.Value, t.Value))),
            measure:    static (s, m) => Fin.Succ(RaiseMeasure(m.Value, s.Scale).Map(IfcProperty (raised) => new IfcPropertySingleValue(s.Db, s.Name.Value, raised))),
            boolean:    static (s, b) => Fin.Succ(Noted.Clean<IfcProperty>(new IfcPropertySingleValue(s.Db, s.Name.Value, b.Value))),
            logical:    static (s, l) => Fin.Succ(Noted.Clean<IfcProperty>(new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcLogical(RaiseLogical(l.Value))))),
            integer:    static (s, i) => Fin.Succ(Noted.Clean<IfcProperty>(new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcInteger(checked((long)i.Value))))),
            number:     static (s, n) => Fin.Succ(Noted.Clean<IfcProperty>(new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcReal(n.Value)))),
            binary:     static (s, b) => Fin.Succ(Noted.Clean<IfcProperty>(new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcBinary(b.Value.ToArray())))),
            enumerated: static (s, e) =>
                from selected in e.Selected.TraverseM(v => RaiseValue(v, s.Scale, s.Key)).As()
                from allowed in e.Allowed.TraverseM(v => RaiseValue(v, s.Scale, s.Key)).As()
                select Noted.Join(selected).Bind(picked => Noted.Join(allowed).Map(IfcProperty (rows) => e.Allowed.IsEmpty
                    ? new IfcPropertyEnumeratedValue(s.Db, s.Name.Value, picked)
                    : new IfcPropertyEnumeratedValue(s.Name.Value, picked, new IfcPropertyEnumeration(s.Db, s.Name.Value, rows)))),
            reference:  static (s, r) => Fin.Succ(Noted.Clean<IfcProperty>(RaiseReference(s.Db, s.Authored, s.Name, r))),
            bounded:    static (s, b) => Fin.Succ(RaiseBounded(s.Db, s.Name, b, s.Scale).Map(IfcProperty (raised) => raised)),
            list:       static (s, l) => l.Values.TraverseM(v => RaiseValue(v, s.Scale, s.Key)).As()
                .Map(rows => Noted.Join(rows).Map(IfcProperty (values) => new IfcPropertyListValue(s.Db, s.Name.Value, values))),
            table:      static (s, t) => RaiseTable(s.Db, s.Name, t, s.Scale, s.Key).Map(static raised => raised.Map(IfcProperty (value) => value)),
            complex:    static (s, c) => c.Properties.AsIterable().ToSeq()
                .TraverseM(kv => RaiseProperty(s.Db, s.Authored, kv.Key, kv.Value, s.Scale, s.Key)).As()
                .Map(rows => Noted.Join(rows).Map(IfcProperty (members) => new IfcComplexProperty(s.Db, s.Name.Value, c.UsageName, members))),
            temporal:   static (s, t) => Fin.Succ(Noted.Clean<IfcProperty>(new IfcPropertySingleValue(s.Db, s.Name.Value, RaiseTemporal(t.Value)))));

    // The Reference inverse restores BOTH halves the ingress arm distinguishes: a target resolving through the
    // authored map to an IfcObjectReferenceSelect member re-attaches as the outbound PropertyReference; the
    // non-rooted resource identity the ingress content-keys (its entity deliberately not round-tripped — the
    // ingress-named bounded drop) resolves no authored node and stays the UsageName-only carrier, honestly distinct.
    static IfcPropertyReferenceValue RaiseReference(DatabaseIfc db, Map<NodeId, IfcObjectDefinition> authored, PropertyName name, PropertyValue.Reference reference) {
        IfcPropertyReferenceValue raised = new(db, name.Value) { UsageName = reference.UsageName.IfNone("") };
        authored.Find(reference.Target).Iter(entity => { if (entity is IfcObjectReferenceSelect select) { raised.PropertyReference = select; } });
        return raised;
    }

    // The seam Logical's Option<bool> -> the IFC three-valued IfcLogicalEnum (None is UNKNOWN); the inverse of LogicalOpt.
    static IfcLogicalEnum RaiseLogical(Option<bool> logical) =>
        logical.Match(Some: static b => b ? IfcLogicalEnum.TRUE : IfcLogicalEnum.FALSE, None: static () => IfcLogicalEnum.UNKNOWN);

    // The seam Interpolation -> the IFC IfcCurveInterpolationEnum through the generated total Switch; the inverse of InterpolationOf.
    static IfcCurveInterpolationEnum RaiseInterp(Interpolation interp) => interp.Switch(
        notDefined: static () => IfcCurveInterpolationEnum.NOTDEFINED,
        linear:     static () => IfcCurveInterpolationEnum.LINEAR,
        logLinear:  static () => IfcCurveInterpolationEnum.LOG_LINEAR,
        logLog:     static () => IfcCurveInterpolationEnum.LOG_LOG);

    // A seam list/table/enumeration cell -> an IFC value. Every scalar arm retains its IFC value-domain discriminant;
    // the composite/reference cases have no IFC value spelling at all, so the tail RAILS CodecReject instead of
    // throwing — a thrown escape off a Fin pipeline is the one failure mode this whole egress rail exists to delete,
    // and it aborted an emit with no typed cause a caller could read.
    static Fin<Noted<IfcValue>> RaiseValue(PropertyValue value, UnitScale scale, Op key) => value switch {
        PropertyValue.Text t     => Fin.Succ(Noted.Clean<IfcValue>(new IfcLabel(t.Value))),
        PropertyValue.Measure m  => Fin.Succ(RaiseMeasure(m.Value, scale)),
        PropertyValue.Boolean b  => Fin.Succ(Noted.Clean<IfcValue>(new IfcBoolean(b.Value))),
        PropertyValue.Logical l  => Fin.Succ(Noted.Clean<IfcValue>(new IfcLogical(RaiseLogical(l.Value)))),
        PropertyValue.Integer i  => Fin.Succ(Noted.Clean<IfcValue>(new IfcInteger(checked((long)i.Value)))),
        PropertyValue.Number n   => Fin.Succ(Noted.Clean<IfcValue>(new IfcReal(n.Value))),
        PropertyValue.Binary b   => Fin.Succ(Noted.Clean<IfcValue>(new IfcBinary(b.Value.ToArray()))),
        PropertyValue.Temporal t => Fin.Succ(Noted.Clean<IfcValue>(RaiseTemporal(t.Value))),
        _ => Fin.Fail<Noted<IfcValue>>(new BimFault.CodecReject(key, $"value-cell-unraisable:{value.GetType().Name}")),
    };

    static IfcValue RaiseTemporal(TemporalValue temporal) => temporal.Switch<IfcValue>(
        date: static value => new IfcDate(value.Value.AtMidnight().ToDateTimeUnspecified()),
        moment: static value => new IfcDateTime(value.Value.ToDateTimeUnspecified()),
        time: static value => new IfcTime { Value = value.Value.On(new LocalDate(1970, 1, 1)).ToDateTimeUnspecified() },
        span: static value => new IfcDuration {
            Years = value.Value.Years,
            Months = value.Value.Months,
            Days = value.Value.Days,
            Hours = value.Value.Hours,
            Minutes = value.Value.Minutes,
            Seconds = value.Value.Seconds + (double)value.Value.Nanoseconds / NodaConstants.NanosecondsPerSecond,
        },
        stamp: static value => new IfcTimeStamp(checked((int)value.Value.ToUnixTimeSeconds())));

    // The seam Bounded -> the IFC IfcPropertyBoundedValue: each present Option slot assigns its TYPED bound through
    // the derived mint, an absent slot stays the IFC optional — the lower/upper/setpoint semantics AND the bound
    // measure types survive the round-trip.
    static Noted<IfcPropertyBoundedValue> RaiseBounded(DatabaseIfc target, PropertyName name, PropertyValue.Bounded bounded, UnitScale scale) {
        IfcPropertyBoundedValue raised = new(target, name.Value);
        FidelityLog log = FidelityLog.Empty;
        bounded.Lower.IfSome(m => { Noted<IfcValue> bound = RaiseMeasure(m, scale); log += bound.Log; raised.LowerBoundValue = bound.Value; });
        bounded.Upper.IfSome(m => { Noted<IfcValue> bound = RaiseMeasure(m, scale); log += bound.Log; raised.UpperBoundValue = bound.Value; });
        bounded.SetPoint.IfSome(m => { Noted<IfcValue> bound = RaiseMeasure(m, scale); log += bound.Log; raised.SetPointValue = bound.Value; });
        return new Noted<IfcPropertyBoundedValue>(log, raised);
    }

    // The seam Table -> the IFC IfcPropertyTableValue: the defining/defined cells fill the value lists and the seam
    // Interpolation re-authors the CurveInterpolation curve rule, so the lookup-table semantics survive the round-trip.
    static Fin<Noted<IfcPropertyTableValue>> RaiseTable(DatabaseIfc target, PropertyName name, PropertyValue.Table table, UnitScale scale, Op key) =>
        from defining in table.Rows.TraverseM(r => RaiseValue(r.Defining, scale, key)).As()
        from defined in table.Rows.TraverseM(r => RaiseValue(r.Defined, scale, key)).As()
        select Noted.Join(defining).Bind(columns => Noted.Join(defined).Map(values => {
            IfcPropertyTableValue raised = new(target, name.Value) { CurveInterpolation = RaiseInterp(table.Interp) };
            raised.DefiningValues.AddRange(columns);
            raised.DefinedValues.AddRange(values);
            return raised;
        }));

    // The frozen QTO-type->IfcQuantity* raiser table, the exact inverse of the ingress `PropertyLowering.QuantityTypes`
    // row set over the SEVEN concrete GG physical quantities. The key is the seam `QuantityType` the ingress stamped,
    // never the `Dimension`: Count, Number, Ratio, and Angle all sign `Dimensionless`, so a dimension key cannot
    // separate an integral tally from a real one and silently re-authored every IFC4.3 `IfcQuantityNumber` as the
    // integral `IfcQuantityCount`.
    static readonly FrozenDictionary<QuantityType, Func<DatabaseIfc, string, double, IfcPhysicalQuantity>> QuantityRaisers =
        new Dictionary<QuantityType, Func<DatabaseIfc, string, double, IfcPhysicalQuantity>> {
            [QuantityType.Length]     = static (db, name, si) => new IfcQuantityLength(db, name, si),
            [QuantityType.Area]       = static (db, name, si) => new IfcQuantityArea(db, name, si),
            [QuantityType.Volume]     = static (db, name, si) => new IfcQuantityVolume(db, name, si),
            [QuantityType.Mass]       = static (db, name, si) => new IfcQuantityWeight(db, name, si),
            [QuantityType.Duration]   = static (db, name, si) => new IfcQuantityTime(db, name, si),
            [QuantityType.Count]      = static (db, name, si) => new IfcQuantityCount(db, name, si),
            [PropertyLowering.Number] = static (db, name, si) => new IfcQuantityNumber(db, name, si),
        }.ToFrozenDictionary();

    // The signature fallback for a Rasm-authored measure whose type is dimension-anonymous (an `OfSi(Dimension, _)`
    // detail-bag mint, a `Multiply`/`Divide` product no consumer re-stamped): the base-dimension identities alone,
    // because the derived preimage is not injective. `Dimensionless` is DELIBERATELY absent — an untyped
    // dimensionless scalar names no IFC physical quantity, and answering it with a `Count` is the masked-error form
    // the silent `IfcQuantityLength` coercion already taught.
    static readonly FrozenDictionary<Dimension, QuantityType> CanonicalQuantities = new Dictionary<Dimension, QuantityType> {
        [Dimension.LengthDim] = QuantityType.Length, [Dimension.AreaDim] = QuantityType.Area,
        [Dimension.VolumeDim] = QuantityType.Volume, [Dimension.MassDim] = QuantityType.Mass,
        [Dimension.DurationDim] = QuantityType.Duration,
    }.ToFrozenDictionary();

    // The typed-measure raise table DERIVED from the ONE ingress PropertyLowering.MeasureDimensions table (the
    // ReleaseMap two-direction law — one table, both legs, zero drift): each key resolves its GG IfcValue type and
    // its (double) ctor ONCE at static init, so an ingested IfcThermalTransmittanceMeasure re-emits ITS OWN typed
    // value — the bare-IfcReal flattening that stripped every measure's IfcValue type (and broke property-template
    // conformance for a consuming tool) is the deleted lossy form.
    static readonly FrozenDictionary<string, Func<double, IfcValue>> MeasureMints =
        PropertyLowering.MeasureDimensions.Keys.AsIterable()
            .Choose(static name => Optional(typeof(IfcValue).Assembly.GetType($"{typeof(IfcValue).Namespace}.{name}"))
                .Filter(static shape => typeof(IfcValue).IsAssignableFrom(shape))
                .Bind(static shape => Optional(shape.GetConstructor([typeof(double)])))
                .Map(ctor => (Name: name, Mint: (Func<double, IfcValue>)(si => (IfcValue)ctor.Invoke([si])))))
            .ToFrozenDictionary(static row => row.Name, static row => row.Mint, StringComparer.Ordinal);

    // The dimension->canonical-measure fallback for a Rasm-AUTHORED base measure (QuantityType "Length"/"Area"/…,
    // or a dimension-only detail-bag mint) whose type name is no GG IfcValue — the base-dimension identities only,
    // because the derived-dimension preimage is not injective (PressureDim answers four measure types).
    static readonly FrozenDictionary<Dimension, string> CanonicalMeasures = new Dictionary<Dimension, string> {
        [Dimension.LengthDim] = "IfcLengthMeasure", [Dimension.AreaDim] = "IfcAreaMeasure", [Dimension.VolumeDim] = "IfcVolumeMeasure",
        [Dimension.MassDim] = "IfcMassMeasure", [Dimension.DurationDim] = "IfcTimeMeasure",
    }.ToFrozenDictionary();

    // The seam MeasureValue -> its typed IfcValue: name-first (the ingested measure-type identity), then the
    // dimension-canonical row, then IfcReal — typed-first, lossy-last, the lossy tail a COUNTED measure-flatten fact;
    // the magnitude folds SI -> declared through the ONE inverse dimensional factor (the SI regime's factor is 1).
    // The declared-regime row a seam measure re-declares on: its own ingress row when the type name is a
    // MeasureDimensions key (so an angle re-declares on the declared angle factor, never on an exponent fold over a
    // vector it signs as dimensionless), else the dimensional fold over its own signature.
    static MeasureRow RowOf(MeasureValue measure) =>
        PropertyLowering.MeasureDimensions.TryGetValue(measure.Type.Value, out MeasureRow row)
            ? row
            : MeasureRow.Of(measure.Dimension);

    static Noted<IfcValue> RaiseMeasure(MeasureValue measure, UnitScale scale) =>
        scale.Declare(measure.Si, RowOf(measure), null) is var declared
        && MeasureMints.TryGetValue(measure.Type.Value, out var typed) ? Noted.Clean<IfcValue>(typed(declared))
        : CanonicalMeasures.TryGetValue(measure.Dimension, out var canonical) && MeasureMints.TryGetValue(canonical, out var mint) ? Noted.Clean<IfcValue>(mint(declared))
        : Noted.Drop<IfcValue>(FidelityDrop.MeasureFlattened, measure.Type.Value, new IfcReal(declared));

    // The seam MeasureValue -> the IFC physical quantity, typed-first then signature-canonical — the SAME two-rung
    // ladder RaiseMeasure takes, so the two egress legs elect their IFC spelling by one rule. A measure carrying
    // neither a rostered QTO type nor a base-dimension signature has NO IFC physical-quantity spelling and faults
    // CodecReject; the prior silent IfcQuantityLength coercion claimed a wrong quantity type, the same masked-error
    // family the deleted release fallbacks were.
    static Fin<IfcPhysicalQuantity> RaiseQuantity(DatabaseIfc target, PropertyName name, MeasureValue measure, UnitScale scale, Op key) =>
        (QuantityRaisers.TryGetValue(measure.Type, out var raiser)
            ? Some(raiser)
            : CanonicalQuantities.TryGetValue(measure.Dimension, out var canonical) && QuantityRaisers.TryGetValue(canonical, out var byDimension)
                ? Some(byDimension)
                : Option<Func<DatabaseIfc, string, double, IfcPhysicalQuantity>>.None)
        .Match(
            Some: elected => Fin.Succ(elected(target, name.Value, scale.Declare(measure.Si, RowOf(measure), null))),
            None: () => Fin.Fail<IfcPhysicalQuantity>(new BimFault.CodecReject(key, $"quantity-type-unmapped:{name.Value}:{measure.Type.Value}")));

    // The element classification set -> IFC: each Object node authors its primary Classification AND every standard-
    // system reference in its Classifications set through ClassificationSystem.Author (which returns None for the "ifc"
    // entity-type code the Author above already resolved as the IfcClass) — a Uniclass + OmniClass co-applied object
    // re-emits BOTH IfcRelAssociatesClassification references it was imported with. Instance member like
    // ReauthorMaterials: the BsddPins hosted-version policy every authored dictionary URI derives from is the
    // ctor-held composition value, so a registry that re-publishes a version is one composition argument and never an
    // Emit parameter, a per-call-site literal, or a durable roster edit.
    void ReauthorClassifications(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored) =>
        graph.Nodes.Values.Choose(static n => n is Node.Object o ? Some(o) : None)
            .Iter(obj => authored.Find(obj.Id).IfSome(entity =>
                obj.Classifications.Add(obj.Classification).Iter(classification =>
                    ignore(ClassificationSystem.Author(target, (IfcDefinitionSelect)entity, classification, pins)))));

    // The ordered-nest ordinal attribute the ingress stamps onto a Relationship.Generic IfcRelNests edge (a
    // PropertyValue.Integer carrying the per-parent-continuous child index — an ordinal is a count, never a
    // physical Measure) — the [AMENDMENTS] carrier that makes ComposeKind.Nest's ordered-children promise
    // representable without touching the frozen 5-kind edge algebra.
    internal static readonly PropertyName NestOrdinal = PropertyName.Create("ordinal");

    // The space-boundary level attr the ingress stamps ("" base-undeclared / "1st" / "2nd") — the egress
    // refined-construct discriminant and the Rasm.Compute filter key, declared ONCE here like NestOrdinal so the two
    // projector halves never drift.
    internal static readonly PropertyName BoundaryLevel = PropertyName.Create("BoundaryLevel");

    // The connection-interface attr a space-boundary Generic edge carries: the UInt128 content key of its
    // IfcConnectionGeometry STEP fragment in the profiles store. An element connect carries the same key on the TYPED
    // seam Connect.Interface slot instead — the boundary keeps an attr because the seam ConnectKind medium vocabulary
    // is closed at element/path/port — so both ends of the interface round-trip read one store through two carriers.
    internal static readonly PropertyName InterfaceKey = PropertyName.Create("InterfaceKey");

    // The attr-refined subtype constructs: the three-valued BoundaryLevel attr names the exact IfcRelSpaceBoundary
    // subtype and the Model/structural-owned StructuralProjection.Eccentricity row the eccentric structural-member
    // subtype, so both riders survive the full cycle instead of degrading to their base class; every other row
    // constructs its own Key.
    static Option<string> Refined(IfcRelKind kind, Relationship edge) => (kind, edge) switch {
        var (k, e) when k == IfcRelKind.SpaceBoundary && e is Relationship.Generic g =>
            g.Attributes.Find(BoundaryLevel).Bind(static v => v switch {
                PropertyValue.Text { Value: "2nd" } => Some("IfcRelSpaceBoundary2ndLevel"),
                PropertyValue.Text { Value: "1st" } => Some("IfcRelSpaceBoundary1stLevel"),
                _                                   => Option<string>.None,
            }),
        var (k, e) when k == IfcRelKind.ConnectsStructMember && e is Relationship.Generic g && g.Attributes.ContainsKey(StructuralProjection.Eccentricity) =>
            Some("IfcRelConnectsWithEccentricity"),
        _ => Option<string>.None,
    };

    static BigInteger OrdinalOf(Relationship.Generic edge) =>
        edge.Attributes.Find(NestOrdinal)
            .Bind(static value => value is PropertyValue.Integer integer ? Some(integer.Value) : Option<BigInteger>.None)
            .IfNone(BigInteger.Pow(2, 256));

    // The neutral edge algebra -> IfcRel*: the ordinal-bearing Generic nests author FIRST — grouped per relating
    // parent, ordered by the NestOrdinal attribute, ONE IfcRelNests per parent whose RelatedObjects fill in that order
    // (the row Author's reflected Add preserves insertion order), so IfcRelNests.RelatedObjects order round-trips —
    // then the realizing Connect FAN re-groups by (From, To) into ONE IfcRelConnectsWithRealizingElements whose
    // RealizingElements.Add takes EVERY realizer (the ingress fans one edge per member, so a multi-realizer joint
    // re-emits whole; the one-member restoration was the closed cardinality loss) — then each typed Compose/Connect/
    // Void edge and the Assign.TypeDefinition/Group edge re-author their IFC relationship by the reverse-indexed
    // IfcRelKind row, the Generic long-tail — the structural idealization and the space boundaries INCLUDED, because
    // those edges are ingest-landed IFC data whose loss broke every re-exported analysis/energy model — by its
    // wire-name [NEUTRAL_EDGE_RULING], the space boundary through the level-refined subtype construct and the eccentric member binding
    // through the Eccentricity-refined construct whose mandatory ConnectionConstraint reconstitutes from the
    // ctor-held profiles store's STEP-fragment lane; the material/property/classification edges resolve to None
    // (authored by their dedicated re-author). Each per-pair edge re-authors per (relating, related) pair against the
    // authored entity map — a one-to-many family thus re-emits one IfcRel* per part (denormalized but lossless).
    // The fold is Fin-RAILED because the OPTIONAL connection interface has no legal degrade: every connect and space
    // boundary reconstitutes its ConnectionGeometry from the same store, and an unanswerable key aborts the emit.
    Fin<Noted<Unit>> ReauthorRelationships(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, Op key) {
        // The ordered nests author FIRST and on the RAIL: an unauthorable IfcRelNests is the same typed failure every
        // other relationship reports, never a discarded Option. A group whose children this emit never authored has no
        // relation to write — the emit-scoping law — so it succeeds without calling Author at all rather than
        // reporting the empty related set as a fault.
        return graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Generic g && g.WireName == IfcRelKind.Nests.Key && g.Attributes.ContainsKey(NestOrdinal) ? Some(g) : None)
            .GroupBy(static g => g.Relating)
            .AsIterable().ToSeq()
            .TraverseM(group => authored.Find(group.Key)
                .Map(relating => toSeq(group.OrderBy(OrdinalOf)).Choose(g => authored.Find(g.Related)) is { IsEmpty: false } children
                    ? IfcRelKind.Nests.Author(target, relating, children, key).Map(static _ => unit)
                    : Fin.Succ(unit))
                .IfNone(Fin.Succ(unit)))
            .As()
            .Bind(_ => Realizing(target, graph, authored, key))
            .Bind(_ => Rostered(target, graph, authored, key));
    }

    // The realizing-Connect fan re-grouped by endpoint pair into ONE IfcRelConnectsWithRealizingElements carrying every
    // realizer — split from the main rostered fold because the grouping IS the arm's law, not a case of it. The arm
    // incurs no bounded drop, so it stays off the Noted rail rather than returning a log that is always empty.
    Fin<Unit> Realizing(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, Op key) {
        return toSeq(graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Connect { Realizing.IsSome: true } c && c.SubKind == ConnectKind.Element ? Some(c) : None)
            .GroupBy(static c => (From: c.From, To: c.To))
            .AsIterable())
            // Both endpoints gate BEFORE the rail: a scoped export and a federated emit both leave joints whose ends
            // this emit never authored, and resolving their interface key on the rail would abort the whole emit over
            // a surface no artifact was going to carry.
            .Filter(group => authored.ContainsKey(group.Key.From) && authored.ContainsKey(group.Key.To))
            // Every edge of a realizing fan came from ONE relation, so the group's interface key is one value the head
            // carries; it resolves BEFORE the author so a store miss faults instead of writing an interface-less joint.
            .TraverseM(group => InterfaceOf(toSeq(group).Head.Bind(static c => c.Interface), key)
                .Bind(surface => IfcRelKind.ConnectsRealizing.Author(target, authored[group.Key.From], Seq(authored[group.Key.To]), key).Map(rel => {
                    if (rel is IfcRelConnectsWithRealizingElements realized) {
                        surface.IfSome(geometry => realized.ConnectionGeometry = geometry);
                        group.AsIterable().Iter(c => c.Realizing.Bind(authored.Find).IfSome(re => {
                            if (re is IfcElement element) { realized.RealizingElements.Add(element); }
                        }));
                    }
                    return unit;
                }))).As()
            .Map(static _ => unit);
    }

    // The rostered per-edge fold: every typed Compose/Connect/Void edge and the Assign.TypeDefinition/Group edge
    // re-author by the reverse-indexed IfcRelKind row, the Generic long-tail by its wire-name.
    Fin<Noted<Unit>> Rostered(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, Op key) {
        return graph.Edges.AsIterable().ToSeq().TraverseM(edge => {
            // The Rasm-native analytical receipt is the RETURNED deliberate skip — no phantom IfcControl is minted,
            // and the receiving party reads the count instead of trusting prose. The subject gate keeps that count
            // to what this emit actually wrote, the same ledger law the positioning drop above holds.
            FidelityLog skipped = edge is Relationship.Assign { SubKind: var assigned } assessment
                && assigned == AssignKind.Assessment && authored.ContainsKey(assessment.Subject)
                    ? FidelityLog.Empty.Note(FidelityDrop.AssessmentSkipped, assessment.Definition.Value)
                    : FidelityLog.Empty;
            return RelKindOf(edge).Match(None: () => Fin.Succ(new Noted<Unit>(skipped, unit)), Some: kind => {
            // The Inverted Assign family (DefinesByType/AssignsToGroup) stored the seam Subject(occurrence)->Definition
            // (the inverse of the IFC relating(type/group)->related), so egress re-inverts to the IFC orientation the
            // row's Relating/Related names expect — the round-trip directionality matching the ingest [NEUTRAL_EDGE_RULING]; every other
            // row already reads in IFC orientation, so the endpoints pass straight through.
            var (ifcRelating, ifcRelated) = kind.Inverted ? (edge.Related, edge.Relating) : (edge.Relating, edge.Related);
            // BOTH endpoints resolve BEFORE any rail work or ledger note, and their absence is the whole verdict for
            // this edge. A scoped export, a federated emit, and a foreign-system endpoint each leave relationships
            // this emit never authors: resolving one of those on the interface rail aborts the whole emit over a
            // surface no artifact carries, and noting its eccentricity degrade charges the receipt for a relationship
            // nothing wrote. The single resolution also retires the nested Find/IfSome ladder the author took.
            if (authored.Find(ifcRelating).Case is not IfcObjectDefinition relating
                || authored.Find(ifcRelated).Case is not IfcObjectDefinition related) {
                return Fin.Succ(new Noted<Unit>(skipped, unit));
            }
            // The eccentric constraint resolves BEFORE the subtype is chosen: IfcRelConnectsWithEccentricity carries a
            // MANDATORY ConnectionConstraint, so the refinement is legal only when the Eccentricity content key
            // resolves its preserved IfcConnectionGeometry STEP fragment — a store miss drops the refinement and the
            // edge authors as its base binding, never a schema-invalid bare subtype with an unassigned constraint.
            Option<IfcConnectionGeometry> constraint = edge is Relationship.Generic gen && kind == IfcRelKind.ConnectsStructMember
                ? gen.Attributes.Find(StructuralProjection.Eccentricity).Bind(ContentKeyOf)
                    .Bind(profiles.Find<IfcConnectionGeometry>)
                : None;
            // A store-missed constraint drops the refinement to the base binding — the RETURNED eccentricity degrade.
            FidelityLog degrade = edge is Relationship.Generic degraded && kind == IfcRelKind.ConnectsStructMember
                && degraded.Attributes.ContainsKey(StructuralProjection.Eccentricity) && constraint.IsNone
                    ? skipped.Note(FidelityDrop.EccentricityDegraded, degraded.Relating.Value)
                    : skipped;
            Option<string> refined = Refined(kind, edge).Filter(_ => kind != IfcRelKind.ConnectsStructMember || constraint.IsSome);
            // The interface surface resolves on the rail BEFORE the author, so a key the store cannot answer aborts the
            // emit typed rather than writing an entity whose ConnectionGeometry the ingest recorded and the emit lost.
            return InterfaceOf(InterfaceKeyOf(edge), key)
                .Bind(surface => kind.Author(target, relating, Seq(related), key, refined).Map(rel => {
                    if (rel is IfcRelConnectsWithEccentricity eccentric) { constraint.IfSome(c => eccentric.ConnectionConstraint = c); }
                    switch (rel) {
                        case IfcRelConnectsElements connects: surface.IfSome(geometry => connects.ConnectionGeometry = geometry); break;
                        case IfcRelSpaceBoundary boundary: surface.IfSome(geometry => boundary.ConnectionGeometry = geometry); break;
                    }
                    return new Noted<Unit>(degrade, unit);
                }));
            });
        }).As()
            .Map(static rows => Noted.Join(rows.ToSeq()).Map(static _ => unit));
    }

    // The interface content key an edge carries: an element connect the TYPED seam Connect.Interface slot, a
    // space-boundary Generic edge the InterfaceKey attr (the Eccentricity row's text-encoded idiom) — two carriers because
    // the seam ConnectKind medium vocabulary is closed at element/path/port and a space↔surface boundary is none of them.
    static Option<UInt128> InterfaceKeyOf(Relationship edge) => edge switch {
        Relationship.Connect c => c.Interface,
        Relationship.Generic g => g.Attributes.Find(InterfaceKey).Bind(ContentKeyOf),
        _ => Option<UInt128>.None,
    };

    // The ONE decode both attr-borne content keys take: every UInt128 key crosses a PropertyValue.Text as the corpus
    // X32 hex canon (the same spelling NodeId.OfContent formats), so the parse states HexNumber explicitly — a default
    // decimal parse silently missed every key its writer had already formatted as hex, which read exactly like an
    // absent key and degraded the binding it was meant to restore.
    static Option<UInt128> ContentKeyOf(PropertyValue value) =>
        value is PropertyValue.Text t && UInt128.TryParse(t.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 parsed)
            ? Some(parsed)
            : Option<UInt128>.None;

    // The preserved-interface reconstitution [M2]: the key names an IfcConnectionGeometry STEP fragment in the
    // ctor-held store, so the re-authored relationship carries its exact interface surface back. ConnectionGeometry is
    // OPTIONAL on both carriers, so an ABSENT key is plain topology — but a PRESENT key the store cannot answer is the
    // typed DanglingReference (the profile-fragment law), never the eccentricity degrade, whose refinement is legally
    // droppable while a lost interface silently unlocates a joint the source located.
    Fin<Option<IfcConnectionGeometry>> InterfaceOf(Option<UInt128> content, Op key) =>
        content.Match(
            None: static () => Fin.Succ(Option<IfcConnectionGeometry>.None),
            Some: address => profiles.Find<IfcConnectionGeometry>(address)
                .ToFin(new BimFault.DanglingReference(key, $"connection-interface-miss:{address}"))
                .Map(Some));

    // The neutral seam edge -> its IfcRelKind row: the typed cases reverse-index (axis, sub-kind) through ForNeutral (the
    // inverse of IfcRelKind.Edge), a realizing Connect additionally resolving IfcRelConnectsWithRealizingElements by its
    // Realizing field, the Generic passthrough by its IFC wire-name — EVERY rostered wire-name re-emits, the structural
    // member/activity idealization and the space boundaries included: those edges are ingest-landed IFC round-trip state
    // (the endpoints and directionality restore from the row; the node-level AppliedCondition/AppliedLoad payload
    // re-stamps through ReauthorStructural — the Model/structural StructuralProjection.Author inverse), and only a Rasm-AUTHORED
    // analytical edge never existed on the wire to begin with. The Assign axis re-authors ONLY the two IFC-objectified
    // sub-kinds the ByNeutral index carries — TypeDefinition (IfcRelDefinesByType) and Group (IfcRelAssignsToGroup);
    // PropertyDefinition is re-authored by ReauthorProperties (the IfcRelDefinesByProperties round-trip), and
    // Assessment is a Rasm.Compute analytical receipt (NOT an IFC entity), so an Assign.Assessment edge is
    // INTENTIONALLY skipped — an imported IfcRelAssignsToControl assessment-family relation rides the Generic
    // wire-name path instead, so no IFC assessment relation is dropped and no phantom IfcPerformanceHistory/IfcControl
    // entity is minted. An Associate edge returns None (ReauthorMaterials owns it), as does an ordinal-bearing Generic
    // nest (the grouped ordered-nest author owns it) and an unrostered wire-name (never re-authoring an entity the
    // roster never declared).
    static Option<IfcRelKind> RelKindOf(Relationship edge) => edge switch {
        Relationship.Compose c => IfcRelKind.ForNeutral(RelationshipKind.Compose, c.SubKind.Key),
        // A realizing Connect (Realizing=Some on the element medium) is GROUP-authored by endpoint pair (the fan-in author
        // above, mirroring the ordinal-nest exclusion) so every realizer lands on ONE IfcRelConnectsWithRealizingElements;
        // a bare element Connect (Realizing=None) re-authors IfcRelConnectsElements — realization is the seam
        // Connect.Realizing FIELD, so both carry ConnectKind.Element and the field presence disambiguates (ConnectsRealizing
        // is excluded from the ByNeutral (axis,sub-kind) index for exactly this reason); Path/Port resolve through ForNeutral.
        Relationship.Connect { SubKind: var sub, Realizing.IsSome: true } when sub == ConnectKind.Element => Option<IfcRelKind>.None,
        Relationship.Connect c => IfcRelKind.ForNeutral(RelationshipKind.Connect, c.SubKind.Key),
        Relationship.Void v    => IfcRelKind.ForNeutral(RelationshipKind.Void, v.SubKind.Key),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition || a.SubKind == AssignKind.Group => IfcRelKind.ForNeutral(RelationshipKind.Assign, a.SubKind.Key),
        // PropertyDefinition -> ReauthorProperties; Assessment -> Rasm-native analytical receipt, NOT IFC-round-trip state.
        Relationship.Assign { SubKind: var sub } when sub == AssignKind.PropertyDefinition || sub == AssignKind.Assessment => Option<IfcRelKind>.None,
        Relationship.Generic g when g.WireName == IfcRelKind.Nests.Key && g.Attributes.ContainsKey(NestOrdinal) => Option<IfcRelKind>.None,
        Relationship.Generic g when IfcRelKind.TryGet(g.WireName, out IfcRelKind? row) && row is { } resolved => Some(resolved),
        _ => Option<IfcRelKind>.None,
    };

    // The Model/structural#STRUCTURAL_PROJECTION Author counterpart: each StructuralDefinition bag (the ingest
    // SourceBag synthesis ReauthorProperties deliberately skips — never a phantom Pset) re-stamps the node-level
    // AppliedCondition/AppliedLoad on ITS authored structural entity through the Assign.PropertyDefinition edge
    // that bound it at ingest — the [RELATIONSHIP_REEMIT] restraint/load drop this compose closes. Author RETURNS the
    // unconsumed-row residue on its own Fin rail — a payload with no verified re-author ctor (a line/planar/temperature
    // action, a trapezoid configuration, a displacement) — so each residual row NOTES a counted structural drop
    // anchored on its subject node and the GG ctor throw crosses as BimFault.CodecReject rather than escaping the
    // fold; discarding that residue behind a void surface made the promise unobservable and is the deleted form.
    Fin<Noted<Unit>> ReauthorStructural(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, Op key) =>
        graph.Edges.AsIterable().ToSeq()
            .Choose(edge => edge is Relationship.Assign { SubKind: var sub, Subject: var subject, Definition: var definition }
                && sub == AssignKind.PropertyDefinition
                && graph.Nodes.Find(definition).Case is Node.PropertySet { Bag: var bag }
                && bag.SetName == StructuralDefinitionSet
                    ? authored.Find(subject).Map(entity => (Subject: subject, Entity: entity, Bag: bag))
                    : None)
            .TraverseM(row => StructuralProjection.Author(target, row.Entity, row.Bag.Values, key)
                .Map(residue => new Noted<Unit>(
                    residue.Fold(FidelityLog.Empty, (log, _) => log.Note(FidelityDrop.StructuralResidue, row.Subject.Value)),
                    unit)))
            .As().Map(static rows => Noted.Join(rows.ToSeq()).Map(static _ => unit));

    // The Projection/semantic ProjectAttributeSet counterpart — the same skipped-bag-to-entity restamp lane the
    // structural bags ride: the context root's Phase lifecycle label and LongName display title re-author VERBATIM
    // onto the constructed IfcContext (the db-binding IfcProject), so the free-text label round-trips whole and
    // its ProjectStage interpretation stays the Planning/schedule#SCHEDULE StageLabels caller admission, never an
    // egress concern. Text rows alone restamp — the ingest arm mints nothing else into this bag.
    static void ReauthorProject(ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored) =>
        graph.Edges.Iter(edge => {
            if (edge is Relationship.Assign { SubKind: var sub, Subject: var subject, Definition: var definition }
                && sub == AssignKind.PropertyDefinition
                && graph.Nodes.Find(definition).Case is Node.PropertySet { Bag: var bag }
                && bag.SetName == ProjectAttributeSet
                && authored.Find(subject).Case is IfcContext context) {
                bag.Values.Find(PropertyName.Create("Phase")).IfSome(v => { if (v is PropertyValue.Text t) { context.Phase = t.Value; } });
                bag.Values.Find(PropertyName.Create("LongName")).IfSome(v => { if (v is PropertyValue.Text t) { context.LongName = t.Value; } });
            }
        });

    // The StepHeader -> the STEP physical-file header on the database: FILE_DESCRIPTION (FileDescriptions) and the
    // FILE_NAME fields restored from the seam header [H9], so an import -> export cycle preserves provenance instead of
    // stripping it. FILE_SCHEMA already rides target.Release (set at the DatabaseIfc construction from the railed
    // ReleaseRaise), so the schema is restored there.
    static void ReauthorHeader(DatabaseIfc target, StepHeader header) {
        STEPFileInformation info = target.OriginatingFileInformation;
        info.FileDescriptions = header.Descriptions.ToList();
        info.FileName = header.Name;
        info.TimeStamp = header.TimeStamp.ToDateTimeUtc();
        info.Author = header.Authors.ToList();
        info.Organization = header.Organizations.ToList();
        info.PreProcessorVersion = header.Preprocessor;
        info.OriginatingSystem = header.OriginatingSystem;
    }
}
```

## [03]-[RESEARCH]

(none)

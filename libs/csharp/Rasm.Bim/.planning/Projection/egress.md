# [BIM_IFC_EGRESS]

The Bim-internal IFC re-author: `SemanticProjector.Emit` lowers a seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` back into IFC STEP/XML/JSON bytes, and `Sniff` reads the schema off foreign bytes BEFORE the database is constructed. `Emit` is a Bim-INTERNAL method on the `Projection/semantic#SEMANTIC_PROJECTOR` projector (a `partial class SemanticProjector` continuation), NOT an `IElementProjection` member — IFC egress is one runtime's wire concern and the seam owns only ingress projection — so it is the exact inverse of the `Project` ingress: where `Project` lowers GeometryGym into seam `Node`s and neutral `Relationship` edges, `Emit` re-authors the seam graph into the IFC entity graph, reading the seam graph ONLY (the `Material` node + the `Associate` edge `MaterialUsage`, the `PropertySet`/`QuantitySet` bag nodes, the `Object` node `Classification`), never a retired `Rasm.Materials` wire carrier. GeometryGym stays captured internally and the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg: `ReleaseRaise` and `Sniff` both resolve through the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap` correspondence, faulting `Model/faults#FAULT_BAND` `BimFault.CodecReject` BARE on an unmapped member — the `IFC4X3_ADD2` silent default is DELETED on both legs — and neither enum ever reaches the seam `Header`.

The egress is the round-trip authority for six invariants the seam delegates to Bim: the `Model/elements#IFC_CLASS` egress gate — the `Instantiable: false` schema-abstract supertype faulting `BimFault.UnmappedClass` (`abstract-class-at-egress`, concretization owned by the occurrence/refinement) and the per-token `AdmitPredefined` span gate whose admitted token STAMPS the entity's own `Ifc*TypeEnum` property plus the `ObjectType` label on `USERDEFINED` [C6][H8]; the 1:1 `GlobalId` round-trip (the node `ExternalId` re-emitted, or `ParserIfc.EncodeGuid` minting one for a from-scratch node [H6]); the diff-derived `OwnerHistory` `ChangeAction` (the generated `Node.Object.EqualityComparer` structured diff over the `prior` snapshot with the freshly-minted `NodeId` member filtered out, the rooted node matched on its stable 1:1 GlobalId [H9]); the schema sniff (`FILE_SCHEMA`/`schema_identifier` read off the bytes onto a `ReleaseMap`-admitted release, never a hardcoded `IFC4X3_ADD2` [H8]); the FULL ten-case `PropertyValue` round-trip (`Bounded`/`List`/`Complex` re-author their typed `IfcPropertyBoundedValue`/`IfcPropertyListValue`/`IfcComplexProperty` entities, never a lossy `Render` string, and every `Measure` re-authors ITS OWN typed `IfcValue` through the `RaiseMeasure` mint derived from the ingress `MeasureDimensions` table — never a flattened `IfcReal`); and the ordered-nest ordinal round-trip (`IfcRelNests.RelatedObjects` order restored from the `Relationship.Generic` ordinal attribute the ingress stamps). The relationship re-author reverses the `Projection/relations#RELATION_ALGEBRA` roster through `IfcRelKind.ForNeutral`/`Author` — the structural idealization and space boundaries included, the boundary's 1st/2nd-level subtype restored from the `BoundaryLevel` attr — so the long-tail families re-emit exactly as they were read [C5]; the material/property/classification subgraphs re-author through the dedicated `ReauthorMaterials`/`ReauthorProperties`/`ReauthorClassifications` folds — the seam-graph egress that REPLACES the retired `Rasm.Materials` material/property wires, emit-scoped so a federated graph's foreign-system bags never export as orphan Psets.

## [01]-[INDEX]

- [01]-[IFC_EGRESS]: `SemanticProjector.Emit` the Bim-internal `ElementGraph` → IFC bytes re-author — the railed `ReleaseRaise` schema target through the frozen `ReleaseMap` inverse → `BimFault.CodecReject`, the `Model/elements#IFC_CLASS` `Instantiable` + per-token `AdmitPredefined` egress gate → `BimFault.UnmappedClass` [C6][H8] with the admitted token stamped onto the entity's `Ifc*TypeEnum`/`ObjectType`, the `GlobalId` round-trip [H6], the diff-derived `OwnerHistory` `ChangeAction` re-stamp through the generated `Node.Object.EqualityComparer` structured diff [H9], `ReauthorMaterials` (`Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` per `Material` node + `Associate` usage [C7]), `ReauthorProperties` (the FULL ten-case `PropertyValue`/`MeasureValue` → `IfcProperty`/`IfcPhysicalQuantity` round-trip + `IfcRelDefinesByProperties`), `ReauthorClassifications` (`Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` per `Object` node), and `ReauthorRelationships` (the `Projection/relations#RELATION_ALGEBRA` neutral edge → `IfcRel*` row-driven re-author + the ordered-nest ordinal grouping) — the seam-graph egress that REPLACES the retired `Rasm.Materials` material wires, plus the railed `FILE_SCHEMA`/`schema_identifier` `Sniff` [H8].

## [02]-[IFC_EGRESS]

- Owner: `SemanticProjector.Emit` the Bim-internal `ElementGraph`→IFC bytes re-author — NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern; it resolves the emit schema through the railed `ReleaseRaise` member (defined HERE on the egress fence, reading the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap.Raise` inverse the ingress `ReleaseLower` mirrors over `ReleaseMap.Lower` — `BimFault.CodecReject` on a seam schema GG cannot serialize, `Ifc5` today), constructs the `DatabaseIfc` at that release with the seam `Header.Tolerance` restored, runs the `Model/elements#IFC_CLASS` egress gate per `"ifc"`-classified `Object` node — `Instantiable: false` faults, the per-token `AdmitPredefined` admits, the admitted token stamps the entity — publishing a `NodeId`→`IfcObjectDefinition` map covering occurrences AND types AND the non-product object families (the `IfcProject` context root wired through its db-binding ctor, groups/systems/zones, processes, controls, actors, resources — an `IfcWallType` or an `IfcSystem` is NOT an `IfcProduct`, so an `IfcProduct`-typed map is the deleted crash-cast form), and re-authors the entity graph: `ReauthorMaterials` (the `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` per seam `Material` node + `Associate` usage edge [C7]), `ReauthorProperties` (the `IfcPropertySet`/`IfcElementQuantity` rebuilt from the bag nodes + the `IfcRelDefinesByProperties` onto the elements, every typed `PropertyValue` case re-authoring its exact `IfcProperty` counterpart), `ReauthorClassifications` (`Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` per `Object` node), and `ReauthorRelationships` (the neutral-edge → `IfcRel*` row-driven re-author, ordinal-bearing `Generic` nests grouped per parent so `IfcRelNests.RelatedObjects` order round-trips); `Sniff` the ingress counterpart reading `FILE_SCHEMA` (STEP) or `schema_identifier` (JSON) off the bytes onto a `ReleaseMap`-admitted GeometryGym release BEFORE the database is constructed, `BimFault.CodecReject` on an absent or unmapped header so the schema is never guessed [H8].
- Entry: `SemanticProjector.Emit(ElementGraph graph, FormatIfcSerialization format, Op key, Option<ElementGraph> prior)` re-authors the graph into IFC text (the `IIfcProfileStore` capability rides the projector's primary constructor — a second `profiles` parameter re-passing the instance dependency is the deleted knob) — for each `"ifc"`-classified `Object` node it resolves the `IfcClass` row from the generic `Classification` code, rejects a schema-abstract row (`Instantiable: false` → `BimFault.UnmappedClass` `abstract-class-at-egress:` — the row is legal CLASSIFICATION vocabulary, illegal as an authored entity class), runs `IfcClass.AdmitPredefined(token, objectType, schema, key)` validating the predefined token against the row's per-token `PredefinedRow` spans AND the class schema span → `BimFault.UnmappedClass` [C6][H8], constructs the entity at the resolved schema, STAMPS the admitted token onto the entity's own `Ifc*TypeEnum` property (`IfcObject.ObjectType`/`IfcElementType.ElementType` authored from the node `Name` on `USERDEFINED`), assigns the `ExternalId` `GlobalId` (or mints one through `ParserIfc.EncodeGuid` for a from-scratch node) [H6], and re-stamps the `OwnerHistory` with a `ChangeAction` diff-derived against the `prior` snapshot, matching the rooted node on its stable `ExternalId` GlobalId across re-ingest [H9] — publishing the `NodeId`→`IfcObjectDefinition` map the re-author folds bind against; `ReauthorMaterials` authors each seam `Material` node's type-level definition + `MaterialPropertySet` Psets ONCE and the per-occurrence `MaterialUsage` [C7] over each `Associate` edge, the ctor-held `profiles` store reconstituting a `ProfileSet`'s `IfcProfileDef` from the preserved profile store; `Fin<T>` aborts on the release miss, the gate, or an unraisable quantity dimension, each typed case lifting BARE; `SemanticProjector.Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format, Op key)` returns `Fin<GGRelease>` — the release the import-rail seeds the database with, faulted typed on a missing or unmapped header.
- Auto: `Emit` folds the `graph.Nodes` once through one `TraverseM` over the `"ifc"`-classified `Object` nodes (a foreign-system node — a Rhino-native capture classified outside `"ifc"` — is NOT IFC-representable and is skipped by classification, the same out-of-scope law the `IfcLegality` `Vocabulary` arm applies, never a fault that aborts a federated emit) — the egress gate resolves the `IfcClass` row, rejects the abstract supertype, validates the predefined token per-token against `Header.Schema` (an IFC4.3 `WAVEWALL` token targeting an IFC2x3 emit faults rather than writing a token the schema forbids), and constructs + stamps the entity — the `IfcProject` row through its db-binding `new IfcProject(DatabaseIfc, name)` ctor so the emitted file carries its mandatory root context, every other row through `Factory.Construct`; the `GlobalId` round-trips from `ExternalId` so a re-imported model re-emits its original GUIDs (1:1) [H6]; `ReauthorRelationships` first groups the ordinal-bearing `Generic` `IfcRelNests` edges per relating parent, orders each group by the `NestOrdinal` attribute, and authors ONE `IfcRelNests` whose `RelatedObjects` fill in that order (the `[AMENDMENTS]` ordered-nest round-trip — the typed `Compose(nest)` case stays the order-free per-pair author), then re-authors the neutral `Compose`/`Connect`/`Void` edges and the `Assign.TypeDefinition`/`Group` edges by reverse-indexing the `IfcRelKind` row and the `Generic` long-tail by its wire-name through `IfcRelKind.Author`, the directionality reconstructing from the row's relating/related names — so the long-tail families re-emit exactly as they were read [C5], the structural member/activity idealization and the space boundaries INCLUDED (they are ingest-landed IFC round-trip state whose loss broke every re-exported analysis/energy model — the space boundary re-authoring its exact 1st/2nd-level subtype from the ingress `BoundaryLevel` attr through the level-refined construct, the restraint/load node-payload inverse the NAMED pending `Model/structural` `Author` counterpart), while the `Rasm.Compute`-authored `Assign.Assessment` receipts alone are skipped: an analysis receipt has no IFC entity and the seam mints no phantom `IfcPerformanceHistory`/`IfcControl` for a Rasm-native result — an imported IFC `IfcRelAssignsToControl` assessment-family relation rode the rostered `Generic` wire-name path at ingest and re-emits from it, so no IFC assessment relation is dropped (the `Assign.PropertyDefinition` edge re-authors through `ReauthorProperties`, not here); the seam `Material` subgraph re-authors through `ReauthorMaterials` and the property/quantity bags through `ReauthorProperties` — EMIT-SCOPED (a bag bound only to foreign-system subjects never authors, an unbound source Pset round-trips, the projector-minted `TypeSignatureSet` bookkeeping bag never exports) and the FULL `Projection/semantic#VALUE_NARROWING` inverse: `Boolean`/`Text` their scalar `IfcPropertySingleValue` ctors, `Measure` ITS OWN typed `IfcValue` through the `RaiseMeasure` mint DERIVED from the ingress `MeasureDimensions` table (an `IfcThermalTransmittanceMeasure` re-emits as itself, never a flattened `IfcReal`), the three-valued `Logical` a typed `IfcLogical`, `Enumerated` the `IfcPropertyEnumeratedValue` with its `IfcPropertyEnumeration` allowed-set reference when the seam carries one, `Reference` the `UsageName`-bearing `IfcPropertyReferenceValue`, `Bounded` the `IfcPropertyBoundedValue` typed lower/upper/setpoint, `List` the `IfcPropertyListValue`, `Table` the `IfcPropertyTableValue` with its curve rule, `Complex` the `IfcComplexProperty` RECURSING the same raise over its sub-bag — so an import→export cycle degrades NO typed case to a string or bare real, the quantity dimension resolving its `IfcQuantity*` through the frozen `QuantityRaisers` row table, and the emitted database declaring the GG SI-default units so the seam's SI-canonical magnitudes (the ingress `UnitScale` coercion) write through unchanged — a mm-source import re-emits as the value-equivalent SI-declared model; the `OwnerHistory` re-emit derives the `ChangeAction` from the generated `Node.Object.EqualityComparer.Default.Inequalities` structured diff (the rooted node matched on its stable 1:1 GlobalId `ExternalId` since the neutral `NodeId` is freshly minted each ingest, the `Id` member filtered out of the verdict — `ADDED`/`MODIFIED`/`NOCHANGE`) so the IFC owner history reflects the real edit [H9]; the `StepHeader` re-authors `FILE_DESCRIPTION`/`FILE_NAME` from `Header.Step`.
- Auto: `Sniff` reads the schema off the bytes before `new DatabaseIfc` — the STEP `FILE_SCHEMA(('IFC4X3_ADD2'))` quoted token, the ifcXML header xmlns schema URI (`.../ifcXML/<release>`), or the IFC-JSON `schema_identifier` member — parsing the token onto the GeometryGym `ReleaseVersion` and gating membership in the frozen `ReleaseMap.Lower` key set (the `IFC4X4_DRAFT` member is excluded by law), so an absent header, an unparseable token, or an unadmitted release faults `BimFault.CodecReject` BARE and the import never guesses 4x3 over a 2x3 file [H8].
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Generator.Equals
- Growth: a new emit format is one `FormatIfcSerialization` the `db.ToString` switch already carries; a new GG release is one `ReleaseMap` row BOTH lowerings read (`Lower` at ingress, `Raise` at egress) with zero new arm; a new predefined token or schema span is one `PredefinedRow` on the generated `IfcClass` roster the same per-token gate reads; a new property-value case is one generated-`Switch` arm `RaiseProperty` breaks on at compile time; a new quantity dimension is one `QuantityRaisers` row; a new measure type re-raises from its ingress `MeasureDimensions` row with ZERO egress edits (the `RaiseMeasure` mint derives); a new subtype refinement is one `Refined` arm over its discriminating attr; a new order-bearing relationship family is one wire-name beside `IfcRelKind.Nests.Key` at the ordered-author gate and its `RelKindOf` exclusion arm — never a per-class egress branch.
- Boundary: `Emit` is Bim-INTERNAL and absent from the `IElementProjection` contract — exposing it on the seam is the named violation; the predefined validity is an EGRESS gate (validated per-token when the IFC entity is authored, against the `PredefinedRow` spans and the class schema span) [C6] and the admitted token is STAMPED onto the entity — a gate that validates then discards the token, a per-call regex, or silent acceptance of an out-of-schema predefined is the deleted form; an `Instantiable: false` class at egress faults `BimFault.UnmappedClass` and authoring a schema-abstract supertype entity is the deleted form — the occurrence/refinement owns concretization; the `GlobalId` is the node `ExternalId` round-tripped 1:1 [H6] and a freshly-minted GUID on a re-emitted node is the deleted form; the schema is sniffed AND release-mapped [H8] — a hardcoded `IFC4X3_ADD2` over a foreign-schema file, an `Enum.TryParse` silent default, or a second GG↔seam release table beside the frozen `ReleaseMap` is the named defect; the `ChangeAction` is diff-derived through the generated `Node.Object.EqualityComparer` structured diff with the freshly-minted `Id` member filtered [H9] — a blanket `ADDED` stamp or a `with { Id = … }` clone-then-`Equals` re-spelling the comparer is the deleted form; a `Bounded`/`List`/`Complex` property degrading to its `Render` string is the deleted lossy form — `Text` alone is the string arm, and the raise switch is the generated TOTAL dispatch so an eleventh seam case breaks this page at compile time; a `Measure` (scalar, cell, or bound) re-authoring as a bare `IfcReal` when its `QuantityType` names a GG `IfcValue` type or its dimension a canonical base measure is the deleted flattening — the `RaiseMeasure` mint DERIVES from the ingress `MeasureDimensions` keys, so the two directions cannot drift and a second hand-rostered raise table is the named defect; the bag egress is EMIT-SCOPED — authoring a bag bound only to foreign-system subjects (an orphan `IfcPropertySet` in a federated emit) or exporting the projector-minted `TypeSignatureSet` bookkeeping bag is the deleted form, while an unbound source Pset round-trips; the `USERDEFINED` label authored from the node `Name` is a NAMED bounded re-derivation — the source `ObjectType` has no seam `Node.Object` slot (the carrier is seam-owned `Rasm.Element` work; imported types preserve theirs through the signature bag); a `Rasm.Compute`-authored `Assign.Assessment` edge is NON-IFC-NATIVE and INTENTIONALLY not re-authored — the analysis receipt is Rasm-native enrichment re-derivable from the content-keyed inputs, so forcing it into a phantom `IfcRelAssignsToControl`/`IfcPerformanceHistory` is the deleted form, while an IMPORTED assessment-family relation round-trips by `Generic` wire-name; an ordinal-bearing `Generic` nest authors ONCE per parent in ordinal order and a per-pair re-author dropping `IfcRelNests.RelatedObjects` order is the deleted form; the material/property/classification egress reads the seam graph ONLY — a `Rasm.Materials` wire carrier crossing into `Emit` is the deleted form, the type-level material definition authored ONCE per `Material` node and the per-occurrence usage wrapping it [C7]; the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg through `ReleaseRaise`/`Sniff` and a leak into the seam `Header` is the named defect; the authored map is `NodeId`→`IfcObjectDefinition` — an `IfcProduct`-typed map is the deleted crash-cast form (a type node authors an `IfcTypeObject` subtype and the context root authors `IfcProject`, neither an `IfcProduct`); `Header.Tolerance` restores onto the constructed database, and the seam `Header.View` round-trips as the VERBATIM `FILE_DESCRIPTION` `ViewDefinition` line `ReauthorHeader` restores — a `ViewRaise` assigning `DatabaseIfc.ModelView` would stand a second release authority beside the railed `ReleaseRaise` and is the rejected form; an unrostered quantity dimension faults `BimFault.CodecReject` — the six `QuantityRaisers` rows ARE the complete `IfcPhysicalSimpleQuantity` vocabulary, so the prior silent `IfcQuantityLength` coercion of a non-base dimension is the deleted masked-error form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// Emit/Sniff continue the Projection/semantic#SEMANTIC_PROJECTOR partial class — the egress half of the
// one projector. The prelude mirrors the ingress page so the partial compiles standalone; the GeometryGym
// ReleaseVersion enum stays codec-leg-local through the GGRelease alias (both lowerings ride the frozen
// Model/elements#TAXONOMY_EMITTER ReleaseMap) and never reaches the seam Header (the seam currency is the
// Rasm.Element alias).
using System.Collections.Frozen;
using System.Text;
using System.Text.Json.Nodes;
using GeometryGym.Ifc;
using GeometryGym.STEP;
using LanguageExt;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries
using ReleaseVersion = Rasm.Element.ReleaseVersion;   // the seam schema currency the Header carries — disambiguated from
using GGRelease = GeometryGym.Ifc.ReleaseVersion;     // the GeometryGym IFC-text enum ReleaseRaise/Sniff resolve

namespace Rasm.Bim;

public sealed partial class SemanticProjector {
    // The Bim-internal IFC egress: ElementGraph -> DatabaseIfc -> bytes. The emit schema resolves FIRST through the
    // railed ReleaseRaise below (the ReleaseMap.Raise inverse of the ingress ReleaseLower — CodecReject on a seam
    // schema GG cannot serialize, the IFC4X3_ADD2 silent default DELETED) [H8]; Header.Tolerance restores onto the
    // database (Header.View round-trips VERBATIM through the ReauthorHeader FILE_DESCRIPTION restore — never a
    // ModelView assignment standing a second release authority); the Instantiable + per-token AdmitPredefined gate
    // runs per "ifc"-classified Object node and the admitted token stamps the entity [C6]; the GlobalId round-trips
    // 1:1 [H6]; the OwnerHistory ChangeAction is diff-derived against the prior snapshot [H9]. The authoring publishes
    // a NodeId->IfcObjectDefinition map (occurrences, types, the IfcProject root, groups/processes/actors/resources);
    // the ctor-held profiles resolver reconstitutes a ProfileSet's IfcProfileDef from the content-addressed STEP store
    // the ProfileRef.ContentKey keys. Never a seam member.
    public Fin<string> Emit(ElementGraph graph, FormatIfcSerialization format, Op key, Option<ElementGraph> prior) =>
        ReleaseRaise(graph.Header.Schema, key).Bind(release => {
            var target = new DatabaseIfc(false, release) { Tolerance = graph.Header.Tolerance };
            // Re-ingest correlation [H6]: the neutral rooted NodeId is freshly minted each Project, so a re-imported
            // graph shares NO NodeId with the prior snapshot — the diff-derived ChangeAction matches a rooted node on
            // the stable 1:1 GlobalId (Node.Object.ExternalId), indexed once here, the NodeId fallback covering a
            // from-scratch node.
            var priorByExternal = prior.Map(static p => p.Nodes.Values
                    .Choose(static n => n is Node.Object o ? o.ExternalId.Map(ext => (Ext: ext, Node: o)) : None)
                    .Fold(Map<string, Node.Object>(), static (m, e) => m.AddOrUpdate(e.Ext, e.Node)))
                .IfNone(Map<string, Node.Object>());
            var histories = new Dictionary<IfcChangeActionEnum, IfcOwnerHistory>();
            // Only "ifc"-classified nodes are IFC-representable: a foreign-system node (a sibling projector's native
            // capture) is out of scope by classification — the same law the IfcLegality Vocabulary arm applies.
            return graph.Nodes.Values.Choose(static node => node is Node.Object { Classification.System: "ifc" } obj ? Some(obj) : None)
                .TraverseM(obj => Author(target, obj, graph.Header.Schema, key, prior, priorByExternal, histories).Map(entity => (Id: obj.Id, Entity: entity)))
                .As()
                .Map(static entities => entities.Fold(Map<NodeId, IfcObjectDefinition>(), static (m, e) => m.AddOrUpdate(e.Id, e.Entity)))
                .Bind(authored => ReauthorMaterials(target, graph, authored)
                    .Bind(_ => ReauthorProperties(target, graph, authored, key))
                    .Map(_ => {
                        ReauthorClassifications(target, graph, authored);
                        ReauthorRelationships(target, graph, authored);
                        ReauthorStructural(target, graph, authored);
                        ReauthorHeader(target, graph.Header.Step);
                        // The georeference round-trip inverse [M1]: Header.Reference re-authors IfcProjectedCRS/
                        // IfcMapConversion (or the IfcSite geographic position) through the CRS mechanics owner —
                        // a LoGeoRef-50 model exporting geo-stripped was the named drop this compose closes.
                        GeoReferenceProjector.Author(target, graph.Header.Reference);
                        return target.ToString(format);
                    }));
        });

    // The seam->GG raise this page owns beside Sniff: the frozen ReleaseMap.Raise identity-name inverse of the ingress
    // ReleaseLower — a seam schema with no GG writer (Ifc5) faults CodecReject BARE; the IFC4X3_ADD2 silent default is
    // the deleted form.
    internal static Fin<GGRelease> ReleaseRaise(ReleaseVersion schema, Op key) =>
        ReleaseMap.Raise.TryGetValue(schema, out GGRelease raised)
            ? FinSucc(raised)
            : FinFail<GGRelease>(new BimFault.CodecReject(key, $"release-unraisable:{schema.Key}"));

    // The egress gate: resolve the IfcClass row from the generic Classification code, reject the schema-abstract
    // supertype (classification vocabulary, never an authored entity class), admit the predefined token against the
    // per-token PredefinedRow spans AND the class schema span [C6][H8], then construct the entity, STAMP the admitted
    // token, and round-trip the GlobalId [H6] + diff-derived OwnerHistory [H9]. The map is IfcObjectDefinition-wide:
    // a type node authors its IfcTypeObject subtype and the context root authors IfcProject through the db-binding
    // ctor that wires DatabaseIfc.Context — neither is an IfcProduct, so a product-typed mint is the deleted form.
    static Fin<IfcObjectDefinition> Author(DatabaseIfc target, Node.Object obj, ReleaseVersion schema, Op key, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal, Dictionary<IfcChangeActionEnum, IfcOwnerHistory> histories) =>
        IfcClass.Resolve(obj.Classification.Code, key)
            .Bind(cls => !cls.Instantiable
                ? FinFail<IfcObjectDefinition>(new BimFault.UnmappedClass(key, $"abstract-class-at-egress:{cls.Key}"))
                : cls.AdmitPredefined(obj.PredefinedType.Token, obj.Name, schema, key)
                    .Map(token => {
                        var entity = (IfcObjectDefinition)(cls == IfcClass.Project
                            ? new IfcProject(target, obj.Name)
                            : target.Factory.Construct(cls.Key));
                        entity.GlobalId = obj.ExternalId.IfNone(() => ParserIfc.EncodeGuid(Guid.NewGuid()));
                        entity.Name = obj.Name;
                        StampPredefined(entity, token, obj.Name);
                        obj.History.IfSome(_ => entity.OwnerHistory = OwnerHistoryOf(target, histories, ChangeOf(obj, prior, priorByExternal)));
                        return entity;
                    }));

    // The admitted token stamps the entity's OWN Ifc*TypeEnum property — the per-class enum type is the entity's, so
    // the slot resolves reflectively like the relations Author fills its Relating/Related slots — and USERDEFINED
    // additionally authors the user-defined label on its owner (IfcObject.ObjectType for an occurrence,
    // IfcElementType.ElementType for a type); GeometryGym's own validPredefinedType setter guard sits beneath the
    // AdmitPredefined gate as defense-in-depth. A class with no PredefinedType property skips silently. The
    // occurrence label re-derives from obj.Name TODAY — the seam Graph/element Growth pins the decided
    // Option<string> ObjectType column (next-campaign, NodeWire frozen); on landing this reads the slot verbatim.
    static void StampPredefined(IfcObjectDefinition entity, string token, string objectType) {
        if (token == "USERDEFINED") {
            switch (entity) {
                case IfcObject occurrence: occurrence.ObjectType = objectType; break;
                case IfcElementType type: type.ElementType = objectType; break;
            }
        }
        if (entity.GetType().GetProperty(nameof(PredefinedType)) is { CanWrite: true, PropertyType.IsEnum: true } slot
            && Enum.TryParse(slot.PropertyType, token, ignoreCase: true, out object? member)) {
            slot.SetValue(entity, member);
        }
    }

    // The ChangeAction is the diff verdict against the prior snapshot, never a blanket stamp [H9]: a rooted node
    // matches the prior on the stable 1:1 GlobalId (ExternalId) ACROSS re-ingest since the NodeId is freshly minted
    // each ingest, falling back to the NodeId for a from-scratch node — absent prior -> ADDED; present prior -> the
    // generated Node.Object.EqualityComparer structured diff decides NOCHANGE/MODIFIED with the freshly-minted Id
    // member filtered out of the verdict, the lazy Inequalities enumeration short-circuiting on the first real
    // member difference — the id-normalizing with{} clone-then-Equals is the deleted form.
    static IfcChangeActionEnum ChangeOf(Node.Object obj, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal) {
        Option<Node.Object> before = obj.ExternalId.Bind(ext => priorByExternal.Find(ext))
            | prior.Bind(graph => graph.Find(obj.Id)).Bind(static n => n is Node.Object o ? Some(o) : Option<Node.Object>.None);
        return before.Match(
            None: static () => IfcChangeActionEnum.ADDED,
            Some: previous => Node.Object.EqualityComparer.Default.Inequalities(previous, obj)
                .All(static delta => delta.Path.ToString() == nameof(Node.Object.Id))
                    ? IfcChangeActionEnum.NOCHANGE
                    : IfcChangeActionEnum.MODIFIED);
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

    // The schema sniff [H8]: read the quoted FILE_SCHEMA token / the ifcJSON schema_identifier member / the ifcXML
    // xmlns schema URI off the bytes before constructing the database, parse it onto the GG ReleaseVersion, and gate
    // membership in the frozen ReleaseMap.Lower key set (IFC4X4_DRAFT excluded by law) — an absent OR unreadable
    // header (a malformed JSON payload funnels through Try.lift, never a thrown escape off the Fin rail), an
    // unparseable token, or an unadmitted release faults CodecReject BARE; the silent IFC4X3_ADD2 default is DELETED.
    public static Fin<GGRelease> Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format, Op key) {
        var token = format == InterchangeFormat.IfcJson
            ? Try.lift(() => Optional((JsonNode.Parse(bytes.Span) as JsonObject)?["schema_identifier"]?.ToString())).Run().ToOption().Flatten()
            : format == InterchangeFormat.IfcXml
                ? XmlSchemaToken(bytes.Span)
                : StepSchemaToken(bytes.Span);
        return token.Match(
            None: () => FinFail<GGRelease>(new BimFault.CodecReject(key, "schema-header-missing")),
            Some: raw => Enum.TryParse(raw, ignoreCase: true, out GGRelease sniffed) && ReleaseMap.Lower.ContainsKey(sniffed)
                ? FinSucc(sniffed)
                : FinFail<GGRelease>(new BimFault.CodecReject(key, $"schema-header-unmapped:{raw}")));
    }

    // The ifcXML header xmlns schema URI — ".../ifcXML/<release>[/AddN]" (IFC2x3 FINAL, IFC4 Add2, IFC4X3) — probed
    // in the leading 4 KiB: the first path segment after "ifcXML/" is the release token Enum.TryParse admits, so an
    // ifcXML payload sniffs its true schema instead of falling through the STEP probe to schema-header-missing.
    static Option<string> XmlSchemaToken(ReadOnlySpan<byte> bytes) {
        var header = Encoding.UTF8.GetString(bytes[..Math.Min(bytes.Length, 4096)]);
        var at = header.IndexOf("ifcXML/", StringComparison.OrdinalIgnoreCase);
        if (at < 0) { return None; }
        var start = at + "ifcXML/".Length;
        var end = header.IndexOfAny(['/', '"', '\''], start);
        return end > start ? Some(header[start..end]) : None;
    }

    // The FILE_SCHEMA(('IFC4X3_ADD2')) token between the first quote pair after the keyword, probed in the leading 4 KiB.
    static Option<string> StepSchemaToken(ReadOnlySpan<byte> bytes) {
        var header = Encoding.ASCII.GetString(bytes[..Math.Min(bytes.Length, 4096)]);
        var at = header.IndexOf("FILE_SCHEMA", StringComparison.Ordinal);
        var open = at < 0 ? -1 : header.IndexOf('\'', at);
        var close = open < 0 ? -1 : header.IndexOf('\'', open + 1);
        return close > open + 1 ? Some(header[(open + 1)..close]) : None;
    }

    // The seam Material subgraph -> IFC: each Material node authors its type-level definition + Psets ONCE through
    // MaterialProjection.AuthorComposition, then each incident Associate edge authors the per-occurrence MaterialUsage
    // [C7] wrapping the shared definition (AuthorUsage) and the IfcRelAssociatesMaterial onto the bound element — so a
    // wall and its mirror share one IfcMaterialLayerSet with two IfcMaterialLayerSetUsage instances. REPLACES the
    // retired Materials wire carriers; the material reads off the projected graph, never a wire. Instance member: the
    // profiles resolver is the ingress-part capture-promoted field, never a re-passed parameter.
    Fin<Unit> ReauthorMaterials(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored) =>
        graph.Nodes.Values.Choose(static n => n is Node.Material m ? Some(m) : None)
            .TraverseM(material => MaterialProjection.AuthorComposition(target, material, profiles).Map(definition => {
                graph.EdgesAt(material.Id)
                    .Choose(e => e is Relationship.Associate a && a.Resource == material.Id ? Some(a) : None)
                    .Iter(edge => authored.Find(edge.Subject).IfSome(bound =>
                        ignore(new IfcRelAssociatesMaterial(MaterialProjection.AuthorUsage(definition, edge.Usage), Seq((IfcDefinitionSelect)bound)))));
                return unit;
            }))
            .As().Map(static _ => unit);

    // The property/quantity bags -> IFC, RAILED and EMIT-SCOPED: the Assign.PropertyDefinition index decides which
    // bag nodes are THIS wire's data — a bag binding at least one authored subject authors ONCE, a bag with NO
    // attachment edge authors too (an unbound source Pset round-trips), and a bag bound ONLY to foreign-system
    // subjects is a sibling projector's capture and never authors (GG writes every constructed entity, so a
    // federated emit would otherwise strand orphan IfcPropertySets); the projector-minted TypeSignatureSet /
    // PortAttributeSet / StructuralDefinitionSet bags are reconciliation and entity-attribute bookkeeping and never
    // author — the port flow and structural definition attributes re-author on the ENTITY at Emit, so exporting the
    // synthesized bag would mint a phantom Pset the source never carried. An unraisable quantity dimension aborts typed, never coerces;
    // each authored-subject edge then authors the IfcRelDefinesByProperties onto its element — the round-trip the
    // retired stringly BimElement.PropertyBinding never had.
    static Fin<Unit> ReauthorProperties(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, Op key) {
        Map<NodeId, Seq<Relationship.Assign>> attachments = graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Assign a && a.SubKind == AssignKind.PropertyDefinition ? Some(a) : None)
            .Fold(Map<NodeId, Seq<Relationship.Assign>>(), static (m, a) => m.AddOrUpdate(a.Definition, s => s.Add(a), () => Seq(a)));
        return graph.Nodes.Values
            .Filter(node => (node is not Node.PropertySet ps
                    || (ps.Bag.SetName != TypeSignatureSet && ps.Bag.SetName != PortAttributeSet && ps.Bag.SetName != StructuralDefinitionSet))
                && attachments.Find(node.Id).Match(Some: edges => edges.Exists(a => authored.ContainsKey(a.Subject)), None: () => true))
            .Choose(node => AuthorBag(target, node, key).Map(fin => fin.Map(set => (Id: node.Id, Set: set))))
            .TraverseM(identity).As()
            .Map(pairs => {
                var bags = pairs.Fold(Map<NodeId, IfcPropertySetDefinition>(), static (m, e) => m.AddOrUpdate(e.Id, e.Set));
                attachments.Iter((_, edges) => edges.Iter(a => bags.Find(a.Definition).IfSome(set =>
                    authored.Find(a.Subject).IfSome(subject => ignore(new IfcRelDefinesByProperties(subject, set))))));
                return unit;
            });
    }

    // The empty-bag guard is load-bearing: the IfcPropertySet(name, IEnumerable)/IfcElementQuantity(name, IEnumerable)
    // ctors derive their database from the FIRST member (`members.First().mDatabase`), so an empty bag would throw at
    // the boundary — an empty Pset/Qto carries no IFC data, so it is skipped (its DefinesByProperties edge then
    // resolves no bag and re-authors nothing), lossless and exception-free, never a crashing `.First()`.
    static Option<Fin<IfcPropertySetDefinition>> AuthorBag(DatabaseIfc target, Node node, Op key) => node switch {
        Node.PropertySet ps when !ps.Bag.Values.IsEmpty => Some(FinSucc((IfcPropertySetDefinition)new IfcPropertySet(ps.Bag.SetName,
            ps.Bag.Values.AsIterable().Map(kv => RaiseProperty(target, kv.Key, kv.Value))))),
        Node.QuantitySet qs when !qs.Bag.Values.IsEmpty => Some(
            qs.Bag.Values.AsIterable().ToSeq().TraverseM(kv => RaiseQuantity(target, kv.Key, kv.Value, key)).As()
                .Map(quantities => (IfcPropertySetDefinition)new IfcElementQuantity(qs.Bag.SetName, quantities))),
        _ => Option<Fin<IfcPropertySetDefinition>>.None,
    };

    // The seam PropertyValue -> the IFC property re-author, the exact VALUE_NARROWING inverse over the generated TOTAL
    // Switch (an eleventh seam case breaks HERE at compile time, never a silent string arm): every typed case rebuilds
    // its IFC counterpart — Text its scalar IfcPropertySingleValue, Boolean its typed bool, Measure ITS OWN typed
    // IfcValue through the derived RaiseMeasure mint (never a flattened IfcReal), the three-valued Logical a
    // typed IfcLogical (UNKNOWN survives), Enumerated its SELECTED list plus the IfcPropertyEnumeration allowed set
    // when the seam carries one, Reference the UsageName carrier, Bounded the lower/upper/setpoint entity, List the
    // typed value list, Table the CurveInterpolation carrier, Complex the IfcComplexProperty RECURSING this raise —
    // so no import->export cycle degrades a typed case to its Render string or a measure to a bare real.
    static IfcProperty RaiseProperty(DatabaseIfc target, PropertyName name, PropertyValue value) =>
        value.Switch<(DatabaseIfc Db, PropertyName Name), IfcProperty>(
            state: (Db: target, Name: name),
            text:       static (s, t) => new IfcPropertySingleValue(s.Db, s.Name.Value, t.Value),
            measure:    static (s, m) => new IfcPropertySingleValue(s.Db, s.Name.Value, RaiseMeasure(m.Value)),
            boolean:    static (s, b) => new IfcPropertySingleValue(s.Db, s.Name.Value, b.Value),
            logical:    static (s, l) => new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcLogical(RaiseLogical(l.Value))),
            enumerated: static (s, e) => e.Allowed.IsEmpty
                ? new IfcPropertyEnumeratedValue(s.Db, s.Name.Value, e.Selected.Map(static v => (IfcValue)new IfcLabel(v)))
                : new IfcPropertyEnumeratedValue(s.Name.Value, e.Selected.Map(static v => (IfcValue)new IfcLabel(v)),
                    new IfcPropertyEnumeration(s.Db, s.Name.Value, e.Allowed.Map(static v => (IfcValue)new IfcLabel(v)))),
            reference:  static (s, r) => new IfcPropertyReferenceValue(s.Db, s.Name.Value) { UsageName = r.UsageName.IfNone("") },
            bounded:    static (s, b) => RaiseBounded(s.Db, s.Name, b),
            list:       static (s, l) => new IfcPropertyListValue(s.Db, s.Name.Value, l.Values.Map(RaiseValue)),
            table:      static (s, t) => RaiseTable(s.Db, s.Name, t),
            complex:    static (s, c) => new IfcComplexProperty(s.Db, s.Name.Value, c.UsageName,
                c.Properties.AsIterable().Map(kv => RaiseProperty(s.Db, kv.Key, kv.Value))));

    // The seam Logical's Option<bool> -> the IFC three-valued IfcLogicalEnum (None is UNKNOWN); the inverse of LogicalOpt.
    static IfcLogicalEnum RaiseLogical(Option<bool> logical) =>
        logical.Match(Some: static b => b ? IfcLogicalEnum.TRUE : IfcLogicalEnum.FALSE, None: static () => IfcLogicalEnum.UNKNOWN);

    // The seam Interpolation -> the IFC IfcCurveInterpolationEnum through the generated total Switch; the inverse of InterpolationOf.
    static IfcCurveInterpolationEnum RaiseInterp(Interpolation interp) => interp.Switch(
        notDefined: static () => IfcCurveInterpolationEnum.NOTDEFINED,
        linear:     static () => IfcCurveInterpolationEnum.LINEAR,
        logLinear:  static () => IfcCurveInterpolationEnum.LOG_LINEAR,
        logLog:     static () => IfcCurveInterpolationEnum.LOG_LOG);

    // A seam list/table cell -> an IFC value: a Measure authors its OWN typed IfcValue through the derived
    // RaiseMeasure mint, a Boolean a typed IfcBoolean, every other case its canonical Render token — the cell
    // grammar, not the property grammar.
    static IfcValue RaiseValue(PropertyValue value) => value switch {
        PropertyValue.Measure m => RaiseMeasure(m.Value),
        PropertyValue.Boolean b => new IfcBoolean(b.Value),
        _                       => new IfcLabel(value.Render()),
    };

    // The seam Bounded -> the IFC IfcPropertyBoundedValue: each present Option slot assigns its TYPED bound through
    // the derived mint, an absent slot stays the IFC optional — the lower/upper/setpoint semantics AND the bound
    // measure types survive the round-trip.
    static IfcPropertyBoundedValue RaiseBounded(DatabaseIfc target, PropertyName name, PropertyValue.Bounded bounded) {
        IfcPropertyBoundedValue raised = new(target, name.Value);
        bounded.Lower.IfSome(m => raised.LowerBoundValue = RaiseMeasure(m));
        bounded.Upper.IfSome(m => raised.UpperBoundValue = RaiseMeasure(m));
        bounded.SetPoint.IfSome(m => raised.SetPointValue = RaiseMeasure(m));
        return raised;
    }

    // The seam Table -> the IFC IfcPropertyTableValue: the defining/defined cells fill the value lists and the seam
    // Interpolation re-authors the CurveInterpolation curve rule, so the lookup-table semantics survive the round-trip.
    static IfcPropertyTableValue RaiseTable(DatabaseIfc target, PropertyName name, PropertyValue.Table table) {
        IfcPropertyTableValue raised = new(target, name.Value) { CurveInterpolation = RaiseInterp(table.Interp) };
        raised.DefiningValues.AddRange(table.Rows.Map(static r => RaiseValue(r.Defining)));
        raised.DefinedValues.AddRange(table.Rows.Map(static r => RaiseValue(r.Defined)));
        return raised;
    }

    // The frozen dimension->IfcQuantity* row table the quantity raise reads — the six rows ARE the complete
    // IfcPhysicalSimpleQuantity vocabulary, so a new base-quantity dimension is one row, never a new ternary arm.
    static readonly FrozenDictionary<Dimension, Func<DatabaseIfc, string, double, IfcPhysicalQuantity>> QuantityRaisers =
        new Dictionary<Dimension, Func<DatabaseIfc, string, double, IfcPhysicalQuantity>> {
            [Dimension.LengthDim]     = static (db, name, si) => new IfcQuantityLength(db, name, si),
            [Dimension.AreaDim]       = static (db, name, si) => new IfcQuantityArea(db, name, si),
            [Dimension.VolumeDim]     = static (db, name, si) => new IfcQuantityVolume(db, name, si),
            [Dimension.MassDim]       = static (db, name, si) => new IfcQuantityWeight(db, name, si),
            [Dimension.DurationDim]   = static (db, name, si) => new IfcQuantityTime(db, name, si),
            [Dimension.Dimensionless] = static (db, name, si) => new IfcQuantityCount(db, name, si),
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
    // dimension-canonical row, then IfcReal — typed-first, lossy-last.
    static IfcValue RaiseMeasure(MeasureValue measure) =>
        MeasureMints.TryGetValue(measure.Type.Value, out var typed) ? typed(measure.Si)
        : CanonicalMeasures.TryGetValue(measure.Dimension, out var canonical) && MeasureMints.TryGetValue(canonical, out var mint) ? mint(measure.Si)
        : new IfcReal(measure.Si);

    // The seam MeasureValue -> the IFC physical quantity by its Dimension row. An off-table dimension has NO IFC
    // physical-quantity spelling and faults CodecReject — the prior silent IfcQuantityLength coercion claimed a wrong
    // quantity type, the same masked-error family the deleted release fallbacks were.
    static Fin<IfcPhysicalQuantity> RaiseQuantity(DatabaseIfc target, PropertyName name, MeasureValue measure, Op key) =>
        QuantityRaisers.TryGetValue(measure.Dimension, out var raiser)
            ? FinSucc(raiser(target, name.Value, measure.Si))
            : FinFail<IfcPhysicalQuantity>(new BimFault.CodecReject(key, $"quantity-dimension-unmapped:{name.Value}"));

    // The element classification set -> IFC: each Object node authors its primary Classification AND every standard-
    // system reference in its Classifications set through ClassificationSystem.Author (which returns None for the "ifc"
    // entity-type code the Author above already resolved as the IfcClass) — a Uniclass + OmniClass co-applied object
    // re-emits BOTH IfcRelAssociatesClassification references it was imported with.
    static void ReauthorClassifications(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored) =>
        graph.Nodes.Values.Choose(static n => n is Node.Object o ? Some(o) : None)
            .Iter(obj => authored.Find(obj.Id).IfSome(entity =>
                obj.Classifications.Add(obj.Classification).Iter(classification =>
                    ignore(ClassificationSystem.Author(target, (IfcDefinitionSelect)entity, classification)))));

    // The ordered-nest ordinal attribute the ingress stamps onto a Relationship.Generic IfcRelNests edge (a
    // PropertyValue.Measure over Dimension.Dimensionless carrying the child index) — the [AMENDMENTS] carrier that
    // makes ComposeKind.Nest's ordered-children promise representable without touching the frozen 5-kind edge algebra.
    internal static readonly PropertyName NestOrdinal = PropertyName.Create("ordinal");

    // The space-boundary level attr the ingress stamps ("" base-undeclared / "1st" / "2nd") — the egress
    // refined-construct discriminant and the Rasm.Compute filter key, declared ONCE here like NestOrdinal so the two
    // projector halves never drift.
    internal static readonly PropertyName BoundaryLevel = PropertyName.Create("BoundaryLevel");

    // The level-refined space-boundary construct: the three-valued BoundaryLevel attr names the exact
    // IfcRelSpaceBoundary subtype the row re-authors, so the 1st/2nd-level distinction survives the full cycle
    // instead of degrading to the base class; every other row constructs its own Key.
    static Option<string> Refined(IfcRelKind kind, Relationship edge) =>
        kind == IfcRelKind.SpaceBoundary && edge is Relationship.Generic g
            ? g.Attributes.Find(BoundaryLevel).Bind(static v => v switch {
                PropertyValue.Text { Value: "2nd" } => Some("IfcRelSpaceBoundary2ndLevel"),
                PropertyValue.Text { Value: "1st" } => Some("IfcRelSpaceBoundary1stLevel"),
                _                                   => Option<string>.None,
            })
            : Option<string>.None;

    static double OrdinalOf(Relationship.Generic edge) =>
        edge.Attributes.Find(NestOrdinal)
            .Bind(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : Option<double>.None)
            .IfNone(double.MaxValue);

    // The neutral edge algebra -> IfcRel*: the ordinal-bearing Generic nests author FIRST — grouped per relating
    // parent, ordered by the NestOrdinal attribute, ONE IfcRelNests per parent whose RelatedObjects fill in that order
    // (the row Author's reflected Add preserves insertion order), so IfcRelNests.RelatedObjects order round-trips —
    // then each typed Compose/Connect/Void edge and the Assign.TypeDefinition/Group edge re-author their IFC
    // relationship by the reverse-indexed IfcRelKind row, the Generic long-tail — the structural idealization and the
    // space boundaries INCLUDED, because those edges are ingest-landed IFC data whose loss broke every re-exported
    // analysis/energy model — by its wire-name [C5], the space boundary through the level-refined subtype construct;
    // the material/property/classification edges resolve to None (authored by their dedicated re-author). Each
    // per-pair edge re-authors per (relating, related) pair against the authored entity map — a one-to-many family
    // thus re-emits one IfcRel* per part (denormalized but lossless).
    static void ReauthorRelationships(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored) {
        graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Generic g && g.WireName == IfcRelKind.Nests.Key && g.Attributes.ContainsKey(NestOrdinal) ? Some(g) : None)
            .GroupBy(static g => g.Relating)
            .AsIterable()
            .Iter(group => authored.Find(group.Key).IfSome(relating =>
                ignore(IfcRelKind.Nests.Author(target, relating, toSeq(group.OrderBy(OrdinalOf)).Choose(g => authored.Find(g.Related))))));
        graph.Edges.AsIterable().Iter(edge => RelKindOf(edge).IfSome(kind => {
            // The Inverted Assign family (DefinesByType/AssignsToGroup) stored the seam Subject(occurrence)->Definition
            // (the inverse of the IFC relating(type/group)->related), so egress re-inverts to the IFC orientation the
            // row's Relating/Related names expect — the round-trip directionality matching the ingest [C5]; every other
            // row already reads in IFC orientation, so the endpoints pass straight through.
            var (ifcRelating, ifcRelated) = kind.Inverted ? (edge.Related, edge.Relating) : (edge.Relating, edge.Related);
            authored.Find(ifcRelating).IfSome(relating =>
                authored.Find(ifcRelated).IfSome(related =>
                    kind.Author(target, relating, Seq(related), Refined(kind, edge)).IfSome(rel => {
                        // A realizing Connect re-authors its third endpoint onto IfcRelConnectsWithRealizingElements so the
                        // realizing intermediary round-trips, not just the From/To pair the row-driven Author binds.
                        if (edge is Relationship.Connect { Realizing: var realizing } && rel is IfcRelConnectsWithRealizingElements realized) {
                            realizing.Bind(authored.Find).IfSome(re => { if (re is IfcElement element) { realized.RealizingElements.Add(element); } });
                        }
                    })));
        }));
    }

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
        Relationship.Compose c => IfcRelKind.ForNeutral(EdgeAxis.Compose, c.SubKind.Key),
        // A realizing Connect (Realizing=Some on the element medium) re-authors IfcRelConnectsWithRealizingElements, a bare
        // element Connect (Realizing=None) IfcRelConnectsElements — realization is the seam Connect.Realizing FIELD, so both
        // carry ConnectKind.Element and the field presence disambiguates the IFC class (ConnectsRealizing is excluded from the
        // ByNeutral (axis,sub-kind) index for exactly this reason); a Path/Port connect resolves by sub-kind through ForNeutral.
        Relationship.Connect { SubKind: var sub, Realizing.IsSome: true } when sub == ConnectKind.Element => Some(IfcRelKind.ConnectsRealizing),
        Relationship.Connect c => IfcRelKind.ForNeutral(EdgeAxis.Connect, c.SubKind.Key),
        Relationship.Void v    => IfcRelKind.ForNeutral(EdgeAxis.Void, v.SubKind.Key),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition || a.SubKind == AssignKind.Group => IfcRelKind.ForNeutral(EdgeAxis.Assign, a.SubKind.Key),
        // PropertyDefinition -> ReauthorProperties; Assessment -> Rasm-native analytical receipt, NOT IFC-round-trip state.
        Relationship.Assign { SubKind: var sub } when sub == AssignKind.PropertyDefinition || sub == AssignKind.Assessment => Option<IfcRelKind>.None,
        Relationship.Generic g when g.WireName == IfcRelKind.Nests.Key && g.Attributes.ContainsKey(NestOrdinal) => Option<IfcRelKind>.None,
        Relationship.Generic g when IfcRelKind.TryGet(g.WireName, out IfcRelKind? row) && row is { } resolved => Some(resolved),
        _ => Option<IfcRelKind>.None,
    };

    // The Model/structural#STRUCTURAL_PROJECTION Author counterpart: each StructuralDefinition bag (the ingest
    // SourceBag synthesis ReauthorProperties deliberately skips — never a phantom Pset) re-stamps the node-level
    // AppliedCondition/AppliedLoad on ITS authored structural entity through the Assign.PropertyDefinition edge
    // that bound it at ingest — the [RELATIONSHIP_REEMIT] restraint/load drop this compose closes.
    static void ReauthorStructural(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored) =>
        graph.Edges.Iter(edge => {
            if (edge is Relationship.Assign { SubKind: var sub, Subject: var subject, Definition: var definition }
                && sub == AssignKind.PropertyDefinition
                && graph.Nodes.Find(definition).Case is Node.PropertySet { Bag: var bag }
                && bag.SetName == StructuralDefinitionSet) {
                authored.Find(subject).IfSome(entity => StructuralProjection.Author(target, entity, bag.Values));
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

- [EGRESS_GATE]: the egress gate grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C6/H8 and the rebuilt `Model/elements#IFC_CLASS` owner — `IfcClass.Resolve(string, Op)`, the `Instantiable` roster flag (`false` on a schema-abstract supertype such as `IfcBuiltElement`, sourced from the emitter's `AbstractSupertypes` overlay because the CLR `IsAbstract` flag is schema-falsified), and the per-token `AdmitPredefined(string, string, ReleaseVersion, Op)` whose `PredefinedRow` spans rank chronologically through `IfcSchema.Rank` (matching GeometryGym's own `validPredefinedType(value, release)` setter guard); the admitted token stamps the entity through the reflected per-class `PredefinedType` enum property (`Enum.TryParse(Type, string, bool, out object?)` — every `Ifc*TypeEnum` carries `NOTDEFINED`/`USERDEFINED` members) plus the user-defined label on its owner — `IfcObject.ObjectType` for an occurrence, `IfcElementType.ElementType` for a type (both `.api/api-geometrygym-ifc` rows) — the same reflected-slot idiom the `Projection/relations#RELATION_ALGEBRA` `Author` fills endpoint slots with; `ParserIfc.EncodeGuid` mints the from-scratch `GlobalId` [H6]; the diff-derived `ChangeAction` grounds against the `libs/csharp/.api/api-generator-equals` generated surface — the seam `Node` class-root `[Union]` carries `[Equatable]`, so `Node.Object.EqualityComparer.Default.Inequalities(before, after)` yields the lazy member-path diff whose single-segment `MemberPath.ToString()` names the differing member, and filtering the freshly-minted `Id` member out of the verdict IS the id-normalization (the `with {}` clone-then-`Equals` re-derivation is retired) [H9]; `IfcOwnerHistory.CreationDate`/`LastModifiedDate` lower through `Instant.FromDateTimeUtc`, never `FromUnixTimeSeconds`.
- [RELEASE_MAP]: the railed `ReleaseRaise`/`Sniff` ground against the `ReleaseMap` law — the ONE frozen GG↔seam release correspondence `Model/elements#TAXONOMY_EMITTER` owns (`Lower : FrozenDictionary<GGRelease, ReleaseVersion>`, its raise inverse DERIVED from the identity-named rows — `Ifc2X3→IFC2x3`, `Ifc4→IFC4`, `Ifc4X1→IFC4X1`, `Ifc4X3→IFC4X3`, `Ifc4X3Add2→IFC4X3_ADD2` — so the alias preimages `IFC2X`/`IFC2x2`/`IFC4A1`/`IFC4A2`/`IFC4X2`/`RC1-4` never become raise targets and the two directions cannot drift); `Emit` composes the `ReleaseRaise(ReleaseVersion, Op)` member THIS fence defines beside `Sniff` (the ingress page defines only `ReleaseLower` — one member per direction, each on its owning leg), `IFC4X4_DRAFT` excluded by law at `Lower` and a seam member with no GG writer (`Ifc5`) missing at raise — both misses fault `BimFault.CodecReject` (band 2600, `Expected`-derived, lifting BARE), so the two `IFC4X3_ADD2` silent defaults (the egress `Enum.TryParse` fallback and the sniff `Contains` ladder) are DELETED; the STEP token reads between the first quote pair after `FILE_SCHEMA` in the leading 4 KiB (ISO 10303-21 mandates the header section lead the exchange structure) and the IFC-JSON token off the `schema_identifier` member via `JsonNode.Parse(ReadOnlySpan<byte>)`, `Sniff` gating the parsed member against the frozen `ReleaseMap.Lower` key set.
- [MATERIAL_PROPERTY_CLASSIFICATION_EGRESS]: `ReauthorMaterials` composes the `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition(DatabaseIfc, Node.Material, IIfcProfileStore)`/`AuthorUsage(IfcMaterialDefinition, MaterialUsage)` (returning `Fin<IfcMaterialDefinition>`/`IfcMaterialSelect`) per `ELEMENT-REBUILD-PLAN.md` §6/§4-RT C7; `ReauthorClassifications` composes the `Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author(DatabaseIfc, IfcDefinitionSelect, Classification)` (returning `None` for the `"ifc"` entity-type code); `ReauthorProperties` rebuilds `IfcPropertySet(name, IEnumerable<IfcProperty>)`/`IfcElementQuantity(name, IEnumerable<IfcPhysicalQuantity>)` + `IfcRelDefinesByProperties(IfcObjectDefinition, IfcPropertySetDefinition)` and the FULL ten-case raise is decompile-confirmed against GeometryGym 25.7.30: `IfcPropertySingleValue(DatabaseIfc, string, bool|int|double|string|IfcValue)` (the scalar family minting `IfcBoolean`/`IfcInteger`/`IfcReal`/`IfcLabel`), `IfcPropertyEnumeratedValue(DatabaseIfc, string, IEnumerable<IfcValue>)` AND the reference-carrying `(string, IEnumerable<IfcValue>, IfcPropertyEnumeration)` overload with `IfcPropertyEnumeration(DatabaseIfc, string, IEnumerable<IfcValue>)` re-authoring the seam `Enumerated.Allowed` set the ingress read off `EnumerationReference`, `IfcPropertyBoundedValue(DatabaseIfc, string)` with settable `LowerBoundValue`/`UpperBoundValue`/`SetPointValue` (`IfcValue`), `IfcPropertyListValue(DatabaseIfc, string, IEnumerable<IfcValue>)`, `IfcPropertyTableValue(DatabaseIfc, string)` with `DefiningValues`/`DefinedValues`/`CurveInterpolation`, and `IfcComplexProperty(DatabaseIfc, string, string, IEnumerable<IfcProperty>)` recursing the raise over the sub-bag — the exact inverses of the ingress `PropertyLowering` members (`NominalValue`, `EnumerationValues`/`EnumerationReference`, `LowerBoundValue`/`UpperBoundValue`/`SetPointValue`, `ListValues`, `DefiningValues`/`DefinedValues`, `UsageName`/`HasProperties`); the seam `Interpolation` `[SmartEnum]` raises through its generated total `Switch` (`NotDefined`/`Linear`/`LogLinear`/`LogLog` → `IfcCurveInterpolationEnum`), and the quantity dimension resolves its `IfcQuantity*` ctor through the frozen `QuantityRaisers` `Dimension`-keyed row table.
- [RELATIONSHIP_REEMIT]: the analytical families are INGEST-LANDED IFC round-trip state, never Rasm-derived enrichment — the `Projection/relations#RELATION_ALGEBRA` `Structural`/`SpatialBoundaries` folds read them OFF THE FILE, and `Rasm.Compute` only READS the landed edges (its own product is the `Assign.Assessment` receipt) — so the retired `AnalyticalWires` skip silently stripped `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity`/`IfcRelSpaceBoundary` from every re-exported analysis/energy model, the masked-loss form this page deletes: every rostered `Generic` wire-name re-emits through `IfcRelKind.Author`, the space boundary constructing its exact `IfcRelSpaceBoundary1stLevel`/`2ndLevel` subtype (both decompile-present GG concretes minted by `Factory.Construct(name)` through the `Author` refined slot) from the three-valued ingress `BoundaryLevel` attr; the node-level `AppliedCondition`/`AppliedLoad` payload re-stamps through `ReauthorStructural` — the `Model/structural` `StructuralProjection.Author` inverse composed in `Emit`, routing each `StructuralDefinition` bag to its authored entity by the `Assign.PropertyDefinition` edge (the prior named drop closed); the one residual NAMED drop is the `IfcRelConnectsWithEccentricity` subtype (its MANDATORY `ConnectionConstraint` is an `IfcConnectionGeometry` the geometry-inline prohibition keeps off the attrs [M2], so a bare subtype would be schema-invalid — it re-emits as the base member binding).
- [MEASURE_MINT]: the typed-measure raise grounds against the ingress `Projection/semantic#VALUE_NARROWING` `MeasureDimensions` table (internal, the one shared narrowing/raising authority — the `ReleaseMap` two-direction law) — `MeasureMints` resolves each key's GG `IfcValue` type and `(double)` constructor once at static init through the assembly probe (`typeof(IfcValue).Assembly.GetType`), so a key whose GG surface lacks the ctor DEGRADES to `IfcReal` instead of asserting an unverifiable member, and the ingested measure-type identity (`MeasureValue.Type.Value`, the IFC measure-type name the ingress stamps) re-authors the exact typed value the file carried; `CanonicalMeasures` covers the Rasm-authored base measures (dimension-keyed, base identities only — the derived-dimension preimage is not injective: `PressureDim` answers four measure types, so no derived fallback exists by design); the emitted database carries the GG SI-default unit assignment, so the seam's `UnitScale`-coerced SI magnitudes write through unchanged.
- [NEST_ORDINAL]: the ordered-nest round-trip grounds against the ordinal-carrier amendment law — `IfcRelNests.RelatedObjects` is schema-ORDERED (`LIST [1:?]`), so the ingress routes an ordered nest through `Relationship.Generic(IfcRelKind.Nests.Key, parent, child, attributes)` stamping the `NestOrdinal` (`"ordinal"`) attribute as a `PropertyValue.Measure` over `Dimension.Dimensionless` carrying the child index, and the egress groups those edges per relating parent, orders by `OrdinalOf`, and authors ONE `IfcRelNests` through `IfcRelKind.Nests.Author(DatabaseIfc, IfcObjectDefinition, Seq<IfcObjectDefinition>)` whose reflected `Add` fill preserves the ordered insertion — the typed `Compose(nest)` case stays byte-identical (the frozen 5-kind edge algebra untouched) and remains the order-free per-pair author for an in-process nest without ordinals; a per-pair re-author of an ordinal-bearing nest (one `IfcRelNests` per child, order lost across instances) is the deleted form.

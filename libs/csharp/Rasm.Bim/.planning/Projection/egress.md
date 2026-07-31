# [BIM_IFC_EGRESS]

The Bim-internal IFC re-author: `SemanticProjector.Emit` lowers a seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` back into IFC bytes at the `IfcWireForm` row a caller names — STEP, zipped STEP, ifcXML, ifcJSON — and `Sniff` reads the schema off foreign bytes BEFORE the database is constructed. `Emit` is a Bim-INTERNAL method on the `Projection/semantic#SEMANTIC_PROJECTOR` projector (a `partial class SemanticProjector` continuation), NOT an `IElementProjection` member — IFC egress is one runtime's wire concern and the seam owns only ingress projection — so it is the exact inverse of the `Project` ingress: where `Project` lowers GeometryGym into seam `Node`s and neutral `Relationship` edges, `Emit` re-authors the seam graph into the IFC entity graph, reading the seam graph ONLY (the `Material` node + the `Associate` edge `MaterialUsage`, the `PropertySet`/`QuantitySet` bag nodes, the `Object` node `Classification`), never a retired `Rasm.Materials` wire carrier. GeometryGym stays captured internally and the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg: `ReleaseRaise` and `Sniff` both resolve through the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap` correspondence, faulting `Model/faults#FAULT_BAND` `BimFault.CodecReject` BARE on an unmapped member — the `IFC4X3_ADD2` silent default is DELETED on both legs — and neither enum ever reaches the seam `Header`.

The egress is the round-trip authority for the `Model/elements#IFC_CLASS` egress gate, the `GlobalId` projection, the diff-derived `OwnerHistory`, schema sniffing, the full typed `PropertyValue` family, and ordered nesting. Numeric, binary, temporal, measured, logical, aggregate, and bounded values re-author through their typed GeometryGym entities rather than `Render` text, and `IfcRelNests.RelatedObjects` order restores from the integer ordinal attribute. The relationship re-author reverses the `Projection/relations#RELATION_ALGEBRA` roster through `IfcRelKind.ForNeutral`/`Author`, and the material/property/classification subgraphs re-author through their dedicated folds.

## [01]-[INDEX]

- [02]-[IFC_EGRESS]: `SemanticProjector.Emit` the Bim-internal `ElementGraph` → IFC bytes re-author — the `IfcWireForm` serialization-and-container row set whose seal delegate owns the write, the railed `ReleaseRaise` schema target, the `EmitContext` carrier (diff-prior, the `Closure`-hulled partial-export scope, the declared unit regime whose inverse `UnitScale.Factor` folds every raised magnitude), the `IfcClass` egress gate, the `GlobalId` and diff-derived `OwnerHistory` stamps, `ReauthorMaterials`, the complete typed `PropertyValue`/`MeasureValue` inverse, `ReauthorClassifications`, and the row-driven `ReauthorRelationships` inverse including ordered nesting, realizing sets, and eccentric constraints — plus the railed `Sniff` schema admission and the egress half of the `FidelityReceipt` drop ledger.

## [02]-[IFC_EGRESS]

- Owner: `SemanticProjector.Emit` the Bim-internal `ElementGraph`→IFC bytes re-author — NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern; `IfcWireForm` the owned wire-form vocabulary carrying the GeometryGym `FormatIfcSerialization` as a column beside the entry extension and the row's own seal delegate, so serialization and container are ONE value (the zipped STEP row the enum cannot express has a seat, and a zip flag beside the serialization is the deleted parallel knob); it resolves the emit schema through the railed `ReleaseRaise` member (defined HERE on the egress fence, reading the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap.Raise` inverse the ingress `ReleaseLower` mirrors over `ReleaseMap.Lower` — `BimFault.CodecReject` on a seam schema GG cannot serialize, `Ifc5` today), constructs the `DatabaseIfc` at that release with the seam `Header.Tolerance` restored, runs the `Model/elements#IFC_CLASS` egress gate per `"ifc"`-classified `Object` node — `Instantiable: false` faults, the per-token `AdmitPredefined` admits, the admitted token stamps the entity — publishing a `NodeId`→`IfcObjectDefinition` map covering occurrences AND types AND the non-product object families (the `IfcProject` context root wired through its db-binding ctor, groups/systems/zones, processes, controls, actors, resources — an `IfcWallType` or an `IfcSystem` is NOT an `IfcProduct`, so an `IfcProduct`-typed map is the deleted crash-cast form), and re-authors the entity graph: `ReauthorMaterials` (the `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` per seam `Material` node + `Associate` usage edge [OCCURRENCE_USAGE_RULING]), `ReauthorProperties` (the `IfcPropertySet`/`IfcElementQuantity` rebuilt from the bag nodes + the `IfcRelDefinesByProperties` onto the elements, every typed `PropertyValue` case re-authoring its exact `IfcProperty` counterpart), `ReauthorClassifications` (`Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` per `Object` node), and `ReauthorRelationships` (the neutral-edge → `IfcRel*` row-driven re-author, ordinal-bearing `Generic` nests grouped per parent so `IfcRelNests.RelatedObjects` order round-trips); `Sniff` the ingress counterpart reading `FILE_SCHEMA` (STEP) or `schema_identifier` (JSON) off the bytes onto a `ReleaseMap`-admitted GeometryGym release BEFORE the database is constructed, `BimFault.CodecReject` on an absent or unmapped header so the schema is never guessed [H8].
- Entry: `SemanticProjector.Emit(ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default)` re-authors the graph into IFC BYTES — the one currency, so no caller re-encodes a returned string and a zipped container is expressible — the `EmitContext` record collapses the three orthogonal emit axes onto one optional carrier (the diff-`Prior` snapshot, the partial-export `Scope` whose `Closure` fold is the one owned law of what a coherent partial model drags along — the spatial ancestor chain to the root plus each member's bound type, with bags/materials/relationships following structurally through the authored-subject gates — and the declared `Units` regime defaulting to `Header.Units` so a mm-source import re-emits mm and an explicitly-passed foot regime lands the contractual imperial deliverable), never a parallel `Option` tail (the `IIfcProfileStore` capability rides the projector's primary constructor — a second `profiles` parameter re-passing the instance dependency is the deleted knob) — for each `"ifc"`-classified `Object` node it resolves the `IfcClass` row from the generic `Classification` code, rejects a schema-abstract row (`Instantiable: false` → `BimFault.UnmappedClass` `abstract-class-at-egress:` — the row is legal CLASSIFICATION vocabulary, illegal as an authored entity class), runs `IfcClass.AdmitPredefined(token, objectType, schema, key)` validating the predefined token against the row's per-token `PredefinedRow` spans AND the class schema span → `BimFault.UnmappedClass` [PREDEFINED_TOKEN_RULING][H8], constructs the entity at the resolved schema, STAMPS the admitted token onto the entity's own `Ifc*TypeEnum` property (`IfcObject.ObjectType`/`IfcElementType.ElementType` authored from the node's own `ObjectType` column on `USERDEFINED`), assigns the `ExternalId` `GlobalId` (or mints one through `ParserIfc.EncodeGuid` for a from-scratch node) [H6], and re-stamps the `OwnerHistory` with a `ChangeAction` diff-derived against the `prior` snapshot, matching the rooted node on its stable `ExternalId` GlobalId across re-ingest [H9] — publishing the `NodeId`→`IfcObjectDefinition` map the re-author folds bind against; `ReauthorMaterials` authors each seam `Material` node's type-level definition + `MaterialPropertySet` Psets ONCE and the per-occurrence `MaterialUsage` [OCCURRENCE_USAGE_RULING] over each `Associate` edge, the ctor-held `profiles` store reconstituting a `ProfileSet`'s `IfcProfileDef` from the preserved profile store while a store-missed Rasm-authored `ProfileSet` authors the profile entity the carried `DetailSchema.Realization` `ProfileSubtype` row names off the baked section dims (`ProfileSubtypeOf` resolves the Materials-seeded occupancy token off the carried graph row, never a Materials call); `Fin<T>` aborts on the release miss, the gate, or an unraisable quantity dimension, each typed case lifting BARE; `SemanticProjector.Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format, Op key)` returns `Fin<GGRelease>` — the release the import-rail seeds the database with, faulted typed on a missing or unmapped header.
- Auto: `Emit` folds the `graph.Nodes` once through one `TraverseM` over the `"ifc"`-classified `Object` nodes (a foreign-system node — a Rhino-native capture classified outside `"ifc"` — is NOT IFC-representable and is skipped by classification, the same out-of-scope law the `IfcLegality` `Vocabulary` arm applies, never a fault that aborts a federated emit) — the egress gate resolves the `IfcClass` row, rejects the abstract supertype, validates the predefined token per-token against `Header.Schema` (an IFC4.3 `WAVEWALL` token targeting an IFC2x3 emit faults rather than writing a token the schema forbids), and constructs + stamps the entity — the `IfcProject` row through its db-binding `new IfcProject(DatabaseIfc, name)` ctor so the emitted file carries its mandatory root context, every other row through `Factory.Construct`; the `GlobalId` round-trips from `ExternalId` so a re-imported model re-emits its original GUIDs (1:1) [H6]; `ReauthorRelationships` first groups the ordinal-bearing `Generic` `IfcRelNests` edges per relating parent, orders each group by the `NestOrdinal` attribute, and authors ONE `IfcRelNests` whose `RelatedObjects` fill in that order (the `[AMENDMENTS]` ordered-nest round-trip — the typed `Compose(nest)` case stays the order-free per-pair author), then re-authors the neutral `Compose`/`Connect`/`Void` edges and the `Assign.TypeDefinition`/`Group` edges by reverse-indexing the `IfcRelKind` row and the `Generic` long-tail by its wire-name through `IfcRelKind.Author`, the directionality reconstructing from the row's relating/related names — so the long-tail families re-emit exactly as they were read [NEUTRAL_EDGE_RULING], the structural member/activity idealization and the space boundaries INCLUDED (they are ingest-landed IFC round-trip state whose loss broke every re-exported analysis/energy model — the space boundary re-authoring its exact 1st/2nd-level subtype from the ingress `BoundaryLevel` attr through the level-refined construct, the eccentric member binding its exact subtype from the `EccentricityKey` attr with the constraint restored from the store, the restraint/load node payload through `ReauthorStructural` composing the `Model/structural` `StructuralProjection.Author` inverse, and the realizing `Connect` fan re-grouped by endpoint pair so every realizer re-emits on ONE `IfcRelConnectsWithRealizingElements`), while the `Rasm.Compute`-authored `Assign.Assessment` receipts alone are skipped: an analysis receipt has no IFC entity and the seam mints no phantom `IfcPerformanceHistory`/`IfcControl` for a Rasm-native result — an imported IFC `IfcRelAssignsToControl` assessment-family relation rode the rostered `Generic` wire-name path at ingest and re-emits from it, so no IFC assessment relation is dropped (the `Assign.PropertyDefinition` edge re-authors through `ReauthorProperties`, not here); the seam `Material` subgraph re-authors through `ReauthorMaterials` and the property/quantity bags through `ReauthorProperties` — EMIT-SCOPED (a bag bound only to foreign-system subjects never authors, an unbound source Pset round-trips, the projector-minted `TypeSignatureSet` bookkeeping bag never exports) and the FULL `Projection/semantic#SEMANTIC_PROJECTOR` inverse: `Boolean`/`Text`/`Integer`/`Number`/`Binary`/`Temporal` their typed scalar `IfcPropertySingleValue` values, `Measure` ITS OWN typed `IfcValue` through the `RaiseMeasure` mint DERIVED from the ingress `MeasureDimensions` table (an `IfcThermalTransmittanceMeasure` re-emits as itself, never a flattened `IfcReal`), the three-valued `Logical` a typed `IfcLogical`, `Enumerated` the `IfcPropertyEnumeratedValue` with its `IfcPropertyEnumeration` allowed-set reference when the seam carries one, `Reference` the `IfcPropertyReferenceValue` carrying its `UsageName` AND its re-attached `PropertyReference` when the seam target resolves to an authored select member (the non-rooted resource identity staying the ingress-named bounded drop), `Bounded` the `IfcPropertyBoundedValue` typed lower/upper/setpoint, `List` the `IfcPropertyListValue`, `Table` the `IfcPropertyTableValue` with its curve rule, `Complex` the `IfcComplexProperty` RECURSING the same raise over its sub-bag — so an import→export cycle degrades NO typed case to a string or bare real, the quantity dimension resolving its `IfcQuantity*` through the frozen `QuantityRaisers` row table, and the emitted database declaring the RESOLVED unit regime — `DeclareUnits` authors the `IfcUnitAssignment` the regime's length family names (the `LengthRegimes` frozen row table over the seam `UnitScheme` tokens; an empty or unmapped scheme keeps the GG SI defaults) and every raised magnitude folds SI→declared through the ONE inverse `UnitScale.Factor` derived off the constructed database, the tolerance included, so a mm-source import re-emits verbatim-mm and an SI graph under a foot regime lands the survey-foot deliverable (the map-conversion offsets and the structural payload magnitudes stay the geo and structural owners' SI author — the two named residual rows of the regime); the `OwnerHistory` re-emit derives the `ChangeAction` from the generated `Node.Object.EqualityComparer.Default.Inequalities` structured diff (the rooted node matched on its stable 1:1 GlobalId `ExternalId` since the neutral `NodeId` is freshly minted each ingest, the `Id` member filtered out of the verdict — `ADDED`/`MODIFIED`/`NOCHANGE`) so the IFC owner history reflects the real edit [H9]; the `StepHeader` re-authors `FILE_DESCRIPTION`/`FILE_NAME` from `Header.Step`.
- Auto: `Sniff` reads the schema off the bytes before `new DatabaseIfc` — the STEP `FILE_SCHEMA(('IFC4X3_ADD2'))` quoted token, the ifcXML header xmlns schema URI (`.../ifcXML/<release>`), or the IFC-JSON `schema_identifier` member — parsing the token onto the GeometryGym `ReleaseVersion` and gating membership in the frozen `ReleaseMap.Lower` key set (the `IFC4X4_DRAFT` member is excluded by law), so an absent header, an unparseable token, or an unadmitted release faults `BimFault.CodecReject` BARE and the import never guesses 4x3 over a 2x3 file [H8].
- Receipt: the egress notes every named bounded drop it incurs onto the projector's one `FidelityLog` — the measure-flatten tail of `RaiseMeasure`, the store-missed eccentricity degrade, the deliberate assessment skip, and the linear-placement re-anchor — so `SemanticProjector.Fidelity` reads as one per-exchange drop ledger across both halves and a federation manager audits the emit instead of trusting prose.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Generator.Equals
- Growth: a new emit form is one `IfcWireForm` row carrying its `FormatIfcSerialization` column, its entry extension, and the seal delegate one of the two existing writers already answers — a new CONTAINER over an existing serialization is a row whose extension GeometryGym's own stream dispatch already reads, never an emit-side branch; a new GG release is one `ReleaseMap` row BOTH lowerings read (`Lower` at ingress, `Raise` at egress) with zero new arm; a new predefined token or schema span is one `PredefinedRow` on the generated `IfcClass` roster the same per-token gate reads; a new property-value case is one generated-`Switch` arm `RaiseProperty` breaks on at compile time; a new quantity dimension is one `QuantityRaisers` row; a new measure type re-raises from its ingress `MeasureDimensions` row with ZERO egress edits (the `RaiseMeasure` mint derives); a new carried profile-subtype token is one seed-computed realization-bag row the same `ProfileSubtypeOf` read resolves with zero egress edits; a new subtype refinement is one `Refined` arm over its discriminating attr; a new order-bearing relationship family is one wire-name beside `IfcRelKind.Nests.Key` at the ordered-author gate and its `RelKindOf` exclusion arm — never a per-class egress branch.
- Boundary: `Emit` is Bim-INTERNAL and absent from the `IElementProjection` contract — exposing it on the seam is the named violation; the wire form is the `IfcWireForm` ROW and the container is GeometryGym's own entry-extension dispatch inside `WriteStream` — a `bool zipped` beside the serialization, a caller-side `ZipArchive` over emitted text, or a re-parse-and-rezip of a written STEP string is the deleted form, and a `false` writer return rails `BimFault.CodecReject` because an empty buffer read as a written model is the forged artifact; `Emit` returns BYTES and a `string` return re-encoded at every call site is the deleted intermediate; the predefined validity is an EGRESS gate (validated per-token when the IFC entity is authored, against the `PredefinedRow` spans and the class schema span) [PREDEFINED_TOKEN_RULING] and the admitted token is STAMPED onto the entity — a gate that validates then discards the token, a per-call regex, or silent acceptance of an out-of-schema predefined is the deleted form; an `Instantiable: false` class at egress faults `BimFault.UnmappedClass` and authoring a schema-abstract supertype entity is the deleted form — the occurrence/refinement owns concretization; the `GlobalId` is the node `ExternalId` round-tripped 1:1 [H6] and a freshly-minted GUID on a re-emitted node is the deleted form; the schema is sniffed AND release-mapped [H8] — a hardcoded `IFC4X3_ADD2` over a foreign-schema file, an `Enum.TryParse` silent default, or a second GG↔seam release table beside the frozen `ReleaseMap` is the named defect; the `ChangeAction` is diff-derived through the generated `Node.Object.EqualityComparer` structured diff with the freshly-minted `Id` member filtered [H9] — a blanket `ADDED` stamp or a `with { Id = … }` clone-then-`Equals` re-spelling the comparer is the deleted form; a `Bounded`/`List`/`Complex` property degrading to its `Render` string is the deleted lossy form — `Text` alone is the string arm, and the raise switch is the generated TOTAL dispatch so every new seam case breaks this page at compile time; a `Measure` (scalar, cell, or bound) re-authoring as a bare `IfcReal` when its `QuantityType` names a GG `IfcValue` type or its dimension a canonical base measure is the deleted flattening — the `RaiseMeasure` mint DERIVES from the ingress `MeasureDimensions` keys, so the two directions cannot drift and a second hand-rostered raise table is the named defect; the bag egress is EMIT-SCOPED — authoring a bag bound only to foreign-system subjects (an orphan `IfcPropertySet` in a federated emit) or exporting the projector-minted `TypeSignatureSet` bookkeeping bag is the deleted form, while an unbound source Pset round-trips; the `USERDEFINED` label re-stamps VERBATIM from the seam `Node.Object` `ObjectType` column onto `IfcObject.ObjectType` or `IfcElementType.ElementType` — the `Name` substitution is DELETED (it collapsed two same-named entities carrying distinct labels), a `None` label on a `USERDEFINED` node faults `predefined-objecttype-miss` at the `AdmitPredefined` gate, and a bag row, an attachment edge, or an egress label index re-carrying the label beside the column is the deleted form; a `Rasm.Compute`-authored `Assign.Assessment` edge is NON-IFC-NATIVE and INTENTIONALLY not re-authored — the analysis receipt is Rasm-native enrichment re-derivable from the content-keyed inputs, so forcing it into a phantom `IfcRelAssignsToControl`/`IfcPerformanceHistory` is the deleted form, while an IMPORTED assessment-family relation round-trips by `Generic` wire-name; an ordinal-bearing `Generic` nest authors ONCE per parent in ordinal order and a per-pair re-author dropping `IfcRelNests.RelatedObjects` order is the deleted form; the emit rail re-authors SEMANTICS alone — no body representation authors here (geometry egress rides the glTF/3dm deliverables), so the `Semantics/appearance#APPEARANCE_PROJECTION` `Author`/`Bind` egress pair stays armed on a body-representation author joining this rail and no styled item is minted without an item to style; the material/property/classification egress reads the seam graph ONLY — a `Rasm.Materials` wire carrier crossing into `Emit` is the deleted form, the type-level material definition authored ONCE per `Material` node and the per-occurrence usage wrapping it [OCCURRENCE_USAGE_RULING]; a `ProfileSet` with no preserved STEP fragment resolves its profile subtype from the carried `DetailSchema.Realization` `ProfileSubtype` row — the rectangle token authors whole off the baked `SectionProperties` dims, while a token whose mandatory interior geometry only a preserved fragment carries (`IfcArbitraryProfileDefWithVoids` inner curves — inline curve geometry never rides the seam [M2]) keeps the typed `DanglingReference` fault, never a bare subtype with unassigned mandatory curves and never a Materials call; the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg through `ReleaseRaise`/`Sniff` and a leak into the seam `Header` is the named defect; the authored map is `NodeId`→`IfcObjectDefinition` — an `IfcProduct`-typed map is the deleted crash-cast form (a type node authors an `IfcTypeObject` subtype and the context root authors `IfcProject`, neither an `IfcProduct`); `Header.Tolerance` restores onto the constructed database, and the seam `Header.View` round-trips as the VERBATIM `FILE_DESCRIPTION` `ViewDefinition` line `ReauthorHeader` restores — a `ViewRaise` assigning `DatabaseIfc.ModelView` stands as a second release authority beside the railed `ReleaseRaise` and is the rejected form; an unrostered quantity dimension faults `BimFault.CodecReject` — the six `QuantityRaisers` rows ARE the complete `IfcPhysicalSimpleQuantity` vocabulary, so the prior silent `IfcQuantityLength` coercion of a non-base dimension is the deleted masked-error form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// Emit/Sniff continue the Projection/semantic#SEMANTIC_PROJECTOR partial class — the egress half of the
// one projector. The prelude mirrors the ingress page so the partial compiles standalone; the GeometryGym
// ReleaseVersion enum stays codec-leg-local through the GGRelease alias (both lowerings ride the frozen
// Model/elements#TAXONOMY_EMITTER ReleaseMap) and never reaches the seam Header (the seam currency is the
// Rasm.Element alias).
using System.Collections.Frozen;
using System.IO;
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
using Rasm.Element.Graph;
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
// Container selection stays GeometryGym's OWN: WriteStream reads the entry extension, opens a ZipArchive over the
// stream for `.ifczip` and writes STEP text as one `<stem>.ifc` entry, writes the ifcXML document for `.xml`, and
// writes STEP text otherwise — so no local container ladder exists here. Only the ifcJSON row falls to ToString,
// because that writer carries no JSON arm.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IfcWireForm {
    public static readonly IfcWireForm Step    = new("step",     FormatIfcSerialization.STEP, ".ifc",     Streamed);
    public static readonly IfcWireForm StepZip = new("step-zip", FormatIfcSerialization.STEP, ".ifczip",  Streamed);
    public static readonly IfcWireForm Xml     = new("xml",      FormatIfcSerialization.XML,  ".ifcxml",  Streamed);
    public static readonly IfcWireForm Json    = new("json",     FormatIfcSerialization.JSON, ".ifcjson", Texted);

    public FormatIfcSerialization Serialization { get; }

    // Container dispatch reads this entry extension AND the zip entry inherits it.
    public string Extension { get; }

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
}

// The one emit-context record: the three orthogonal emit axes — the diff-prior snapshot, the partial-export scope,
// and the declared unit regime — collapsed onto one optional carrier so the entrypoint never grows a parallel
// Option tail; every absent axis derives its default from the graph (no prior -> ADDED, no scope -> the whole
// graph, no units -> the Header.Units declared scheme, itself the empty-SI default).
public sealed record EmitContext(
    Option<ElementGraph> Prior = default,
    Option<ElementSet> Scope = default,
    Option<UnitScheme> Units = default) {
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
            Option<ElementGraph> prior = ctx.Prior;
            // The partial-export scope [X1]: a caller-selected ElementSet closes over what a coherent partial model
            // drags along — the spatial ancestor chain to the root and each member's bound type — and the node fold
            // authors only the closure; bags, materials, and relationships follow structurally through the
            // authored-subject gates, so "everything on storey 3 in the plumbing domain" emits as a conforming
            // standalone IFC in one expression. No scope = the whole graph, today's behavior verbatim.
            Option<LanguageExt.HashSet<NodeId>> scope = ctx.Scope.Map(selection => Closure(graph, selection.Ids));
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
            // capture) is out of scope by classification — the same law the IfcLegality Vocabulary arm applies; the
            // emit closure additionally gates a scoped export to the selection's coherent hull.
            return graph.Nodes.Values.Choose(node => node is Node.Object { Classification.System: "ifc" } obj
                    && scope.Match(Some: hull => hull.Contains(obj.Id), None: static () => true) ? Some(obj) : None)
                .TraverseM(obj => Author(target, obj, graph.Header.Schema, key, prior, priorByExternal, histories).Map(entity => (Id: obj.Id, Entity: entity)))
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
                    target.Tolerance = graph.Header.Tolerance / emitScale.L;
                    return ReauthorMaterials(target, graph, authored, key)
                        .Bind(_ => ReauthorProperties(target, graph, authored, emitScale, key))
                        .Bind(_ => {
                            ReauthorClassifications(target, graph, authored);
                            ReauthorRelationships(target, graph, authored);
                            ReauthorStructural(target, graph, authored);
                            ReauthorHeader(target, graph.Header.Step);
                            // The georeference round-trip inverse [M1]: Header.Reference re-authors IfcProjectedCRS/
                            // IfcMapConversion (or the IfcSite geographic position) through the CRS mechanics owner —
                            // a LoGeoRef-50 model exporting geo-stripped was the named drop this compose closes.
                            GeoReferenceProjector.Author(target, graph.Header.Reference);
                            // The writer's ENTRY name: the seam header's own FILE_NAME stem under the row's
                            // extension, so GeometryGym's container dispatch reads the row and a zipped emit names
                            // its inner entry after the model. A nameless header falls to the release token rather
                            // than an empty stem, because the writer derives the zip entry from this string.
                            string entry = $"{(Path.GetFileNameWithoutExtension(graph.Header.Step.Name) is { Length: > 0 } stem ? stem : release.ToString())}{form.Extension}";
                            return form.Seal(target, entry)
                                .ToFin(new BimFault.CodecReject(key, $"ifc-write-refused:{form.Key}"));
                        });
                });
        });

    // The scoped-emit closure — the ONE owned law of what a coherent partial model drags along: every selected node,
    // its transitive spatial ancestor chain (Contain first, Aggregate second — the same up-chain the query Ancestry
    // reach walks) to the IfcProject root, and each member's bound type object. Bags, materials, classifications,
    // and relationships need no closure rows: the re-author folds gate on authored subjects, so a bag bound only to
    // out-of-scope subjects never authors and a relationship re-emits only when both endpoints survive.
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
    // named bounded residual of the regime.
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
            ? FinSucc(raised)
            : FinFail<GGRelease>(new BimFault.CodecReject(key, $"release-unraisable:{schema.Key}"));

    // The egress gate: resolve the IfcClass row from the generic Classification code, reject the schema-abstract
    // supertype (classification vocabulary, never an authored entity class), admit the predefined token against the
    // per-token PredefinedRow spans AND the class schema span [PREDEFINED_TOKEN_RULING][H8], then construct the entity, STAMP the admitted
    // token, and round-trip the GlobalId [H6] + diff-derived OwnerHistory [H9]. The map is IfcObjectDefinition-wide:
    // a type node authors its IfcTypeObject subtype and the context root authors IfcProject through the db-binding
    // ctor that wires DatabaseIfc.Context — neither is an IfcProduct, so a product-typed mint is the deleted form.
    static Fin<IfcObjectDefinition> Author(DatabaseIfc target, Node.Object obj, ReleaseVersion schema, Op key, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal, Dictionary<IfcChangeActionEnum, IfcOwnerHistory> histories) =>
        IfcClass.Resolve(obj.Classification.Code, key)
            .Bind(cls => !cls.Instantiable
                ? FinFail<IfcObjectDefinition>(new BimFault.UnmappedClass(key, $"abstract-class-at-egress:{cls.Key}"))
                : cls.AdmitPredefined(obj.PredefinedType.Token, obj.ObjectType.IfNone(""), schema, key)
                    .Map(token => {
                        var entity = (IfcObjectDefinition)(cls == IfcClass.Project
                            ? new IfcProject(target, obj.Name)
                            : target.Factory.Construct(cls.Key));
                        entity.GlobalId = obj.ExternalId.IfNone(() => ParserIfc.EncodeGuid(Guid.NewGuid()));
                        entity.Name = obj.Name;
                        StampPredefined(entity, token, obj.ObjectType);
                        obj.History.IfSome(_ => entity.OwnerHistory = OwnerHistoryOf(target, histories, ChangeOf(obj, prior, priorByExternal)));
                        return entity;
                    }));

    // The admitted token stamps the entity's OWN Ifc*TypeEnum property — the per-class enum type is the entity's, so
    // the slot resolves reflectively like the relations Author fills its Relating/Related slots — and USERDEFINED
    // additionally authors the user-defined label on its owner (IfcObject.ObjectType for an occurrence,
    // IfcElementType.ElementType for a type); GeometryGym's own validPredefinedType setter guard sits beneath the
    // AdmitPredefined gate as defense-in-depth. A class with no PredefinedType property skips silently. The label is
    // the seam Object node's own ObjectType column VERBATIM — the Projection/semantic UserLabel read reversed onto the
    // same two entity slots — so no bag row, no attachment edge, and no egress index stand between the ends of the
    // round-trip. A USERDEFINED node carrying None never reaches here: AdmitPredefined already faulted it
    // predefined-objecttype-miss, and the Name substitution that masked that malformed pair is DELETED — it collapsed
    // two same-named entities holding distinct labels onto one.
    static void StampPredefined(IfcObjectDefinition entity, string token, Option<string> objectType) {
        if (token == "USERDEFINED") {
            objectType.IfSome(label => {
                switch (entity) {
                    case IfcObject occurrence: occurrence.ObjectType = label; break;
                    case IfcElementType type: type.ElementType = label; break;
                }
            });
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
    // [OCCURRENCE_USAGE_RULING] wrapping the shared definition (AuthorUsage) and the IfcRelAssociatesMaterial onto the bound element — so a
    // wall and its mirror share one IfcMaterialLayerSet with two IfcMaterialLayerSetUsage instances. REPLACES the
    // retired Materials wire carriers; the material reads off the projected graph, never a wire. Instance member: the
    // profiles resolver is the ingress-part capture-promoted field, never a re-passed parameter. A ProfileSet with no
    // preserved STEP fragment resolves its profile-def subtype from the carried DetailSchema.Realization ProfileSubtype
    // row (ProfileSubtypeOf below) — the Materials-seeded occupancy token — so the profile lane reads the carried row.
    // EMIT-SCOPED like the bag egress: the usage fold binds only AUTHORED subjects, so a scoped export authors a
    // material once for its surviving usages and a material whose every subject is out-of-scope or foreign-system
    // never authors — the per-subject DanglingReference fault fired on legitimate federated/partial emits and is
    // the deleted form; a truly-corrupt graph still faults at the seam Link law before any emit sees it.
    Fin<Unit> ReauthorMaterials(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, Op key) =>
        graph.Nodes.Values.Choose(static n => n is Node.Material m ? Some(m) : None)
            .Map(material => (Material: material, Usages: graph.EdgesAt(material.Id)
                .Choose(e => e is Relationship.Associate a && a.Resource == material.Id && authored.ContainsKey(a.Subject) ? Some(a) : None)
                .ToSeq()))
            .Filter(static row => !row.Usages.IsEmpty)
            .TraverseM(row => MaterialProjection.AuthorComposition(target, row.Material, profiles, ProfileSubtypeOf(graph, row.Material.Id)).Bind(definition =>
                row.Usages
                    .TraverseM(edge => MaterialProjection.AuthorUsage(definition, edge.Usage)
                        .Map(select => {
                            ignore(new IfcRelAssociatesMaterial(select, Seq((IfcDefinitionSelect)authored[edge.Subject])));
                            return unit;
                        }))
                    .As().Map(static _ => unit)))
            .As().Map(static _ => unit);

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
    // PortAttributeSet / StructuralDefinitionSet / PositioningAttributeSet bags are reconciliation and
    // entity-attribute bookkeeping and never author — the port flow and structural definition attributes re-author on
    // the ENTITY at Emit, the station rows stay ingest-landed evidence whose IfcLinearPlacement re-author is the named
    // bounded drop the fidelity receipt counts, so exporting a synthesized bag would mint a phantom Pset the source
    // never carried. An unraisable quantity dimension aborts typed, never coerces;
    // each authored-subject edge then authors the IfcRelDefinesByProperties onto its element — the round-trip the
    // retired stringly BimElement.PropertyBinding never had.
    Fin<Unit> ReauthorProperties(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, UnitScale scale, Op key) {
        Map<NodeId, Seq<Relationship.Assign>> attachments = graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Assign a && a.SubKind == AssignKind.PropertyDefinition ? Some(a) : None)
            .Fold(Map<NodeId, Seq<Relationship.Assign>>(), static (m, a) => m.AddOrUpdate(a.Definition, s => s.Add(a), () => Seq(a)));
        // Each ingest-landed positioning bag is a COUNTED linear-placement drop: the station rows are evidence, the
        // IfcLinearPlacement entity re-anchors from content-keyed geometry rather than re-authoring from scalars.
        graph.Nodes.Values.AsIterable()
            .Choose(static n => n is Node.PropertySet { Bag.SetName: var set } p && set == PositioningAttributeSet ? Some(p) : None)
            .Iter(p => ignore(fidelity.Noted(FidelityDrop.LinearPlacement, p.Id.Value, unit)));
        return graph.Nodes.Values
            .Filter(node => (node is not Node.PropertySet ps
                    || (ps.Bag.SetName != TypeSignatureSet && ps.Bag.SetName != PortAttributeSet
                        && ps.Bag.SetName != StructuralDefinitionSet && ps.Bag.SetName != PositioningAttributeSet))
                && attachments.Find(node.Id).Match(Some: edges => edges.Exists(a => authored.ContainsKey(a.Subject)), None: () => true))
            .Choose(node => AuthorBag(target, node, authored, scale, fidelity, key).Map(fin => fin.Map(set => (Id: node.Id, Set: set))))
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
    static Option<Fin<IfcPropertySetDefinition>> AuthorBag(DatabaseIfc target, Node node, Map<NodeId, IfcObjectDefinition> authored, UnitScale scale, FidelityLog log, Op key) => node switch {
        Node.PropertySet ps when !ps.Bag.Values.IsEmpty => Some(FinSucc((IfcPropertySetDefinition)new IfcPropertySet(ps.Bag.SetName,
            ps.Bag.Values.AsIterable().Map(kv => RaiseProperty(target, authored, kv.Key, kv.Value, scale, log))))),
        Node.QuantitySet qs when !qs.Bag.Values.IsEmpty => Some(
            qs.Bag.Values.AsIterable().ToSeq().TraverseM(kv => RaiseQuantity(target, kv.Key, kv.Value, scale, key)).As()
                .Map(quantities => (IfcPropertySetDefinition)new IfcElementQuantity(qs.Bag.SetName, quantities))),
        _ => Option<Fin<IfcPropertySetDefinition>>.None,
    };

    // The seam PropertyValue -> the IFC property re-author, the exact VALUE_NARROWING inverse over the generated TOTAL
    // Switch (a new seam case breaks HERE at compile time, never a silent string arm): every typed case rebuilds
    // its IFC counterpart — Text its scalar IfcPropertySingleValue, Boolean its typed bool, Measure ITS OWN typed
    // IfcValue through the derived RaiseMeasure mint (never a flattened IfcReal), the three-valued Logical a
    // typed IfcLogical (UNKNOWN survives), Enumerated its SELECTED list plus the IfcPropertyEnumeration allowed set
    // when the seam carries one, Reference the UsageName carrier PLUS its re-attached PropertyReference when the
    // seam target resolves to an authored select member, Bounded the lower/upper/setpoint entity, List the
    // typed value list, Table the CurveInterpolation carrier, Complex the IfcComplexProperty RECURSING this raise —
    // so no import->export cycle degrades a typed case to its Render string or a measure to a bare real.
    static IfcProperty RaiseProperty(DatabaseIfc target, Map<NodeId, IfcObjectDefinition> authored, PropertyName name, PropertyValue value, UnitScale scale, FidelityLog log) =>
        value.Switch<(DatabaseIfc Db, Map<NodeId, IfcObjectDefinition> Authored, PropertyName Name, UnitScale Scale, FidelityLog Log), IfcProperty>(
            state: (Db: target, Authored: authored, Name: name, Scale: scale, Log: log),
            text:       static (s, t) => new IfcPropertySingleValue(s.Db, s.Name.Value, t.Value),
            measure:    static (s, m) => new IfcPropertySingleValue(s.Db, s.Name.Value, RaiseMeasure(m.Value, s.Scale, s.Log)),
            boolean:    static (s, b) => new IfcPropertySingleValue(s.Db, s.Name.Value, b.Value),
            logical:    static (s, l) => new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcLogical(RaiseLogical(l.Value))),
            integer:    static (s, i) => new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcInteger(checked((long)i.Value))),
            number:     static (s, n) => new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcReal(n.Value)),
            binary:     static (s, b) => new IfcPropertySingleValue(s.Db, s.Name.Value, new IfcBinary(b.Value.ToArray())),
            enumerated: static (s, e) => e.Allowed.IsEmpty
                ? new IfcPropertyEnumeratedValue(s.Db, s.Name.Value, e.Selected.Map(v => RaiseValue(v, s.Scale, s.Log)))
                : new IfcPropertyEnumeratedValue(s.Name.Value, e.Selected.Map(v => RaiseValue(v, s.Scale, s.Log)),
                    new IfcPropertyEnumeration(s.Db, s.Name.Value, e.Allowed.Map(v => RaiseValue(v, s.Scale, s.Log)))),
            reference:  static (s, r) => RaiseReference(s.Db, s.Authored, s.Name, r),
            bounded:    static (s, b) => RaiseBounded(s.Db, s.Name, b, s.Scale, s.Log),
            list:       static (s, l) => new IfcPropertyListValue(s.Db, s.Name.Value, l.Values.Map(v => RaiseValue(v, s.Scale, s.Log))),
            table:      static (s, t) => RaiseTable(s.Db, s.Name, t, s.Scale, s.Log),
            complex:    static (s, c) => new IfcComplexProperty(s.Db, s.Name.Value, c.UsageName,
                c.Properties.AsIterable().Map(kv => RaiseProperty(s.Db, s.Authored, kv.Key, kv.Value, s.Scale, s.Log))),
            temporal:   static (s, t) => new IfcPropertySingleValue(s.Db, s.Name.Value, RaiseTemporal(t.Value)));

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
    // composite/reference arms are excluded by the seam admission for IFC-value cells.
    static IfcValue RaiseValue(PropertyValue value, UnitScale scale, FidelityLog log) => value switch {
        PropertyValue.Text t    => new IfcLabel(t.Value),
        PropertyValue.Measure m => RaiseMeasure(m.Value, scale, log),
        PropertyValue.Boolean b => new IfcBoolean(b.Value),
        PropertyValue.Logical l => new IfcLogical(RaiseLogical(l.Value)),
        PropertyValue.Integer i => new IfcInteger(checked((long)i.Value)),
        PropertyValue.Number n  => new IfcReal(n.Value),
        PropertyValue.Binary b  => new IfcBinary(b.Value.ToArray()),
        PropertyValue.Temporal t => RaiseTemporal(t.Value),
        _ => throw new InvalidOperationException($"IFC value cell cannot carry {value.GetType().Name}"),
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
    static IfcPropertyBoundedValue RaiseBounded(DatabaseIfc target, PropertyName name, PropertyValue.Bounded bounded, UnitScale scale, FidelityLog log) {
        IfcPropertyBoundedValue raised = new(target, name.Value);
        bounded.Lower.IfSome(m => raised.LowerBoundValue = RaiseMeasure(m, scale, log));
        bounded.Upper.IfSome(m => raised.UpperBoundValue = RaiseMeasure(m, scale, log));
        bounded.SetPoint.IfSome(m => raised.SetPointValue = RaiseMeasure(m, scale, log));
        return raised;
    }

    // The seam Table -> the IFC IfcPropertyTableValue: the defining/defined cells fill the value lists and the seam
    // Interpolation re-authors the CurveInterpolation curve rule, so the lookup-table semantics survive the round-trip.
    static IfcPropertyTableValue RaiseTable(DatabaseIfc target, PropertyName name, PropertyValue.Table table, UnitScale scale, FidelityLog log) {
        IfcPropertyTableValue raised = new(target, name.Value) { CurveInterpolation = RaiseInterp(table.Interp) };
        raised.DefiningValues.AddRange(table.Rows.Map(r => RaiseValue(r.Defining, scale, log)));
        raised.DefinedValues.AddRange(table.Rows.Map(r => RaiseValue(r.Defined, scale, log)));
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
    // dimension-canonical row, then IfcReal — typed-first, lossy-last, the lossy tail a COUNTED measure-flatten fact;
    // the magnitude folds SI -> declared through the ONE inverse dimensional factor (the SI regime's factor is 1).
    static IfcValue RaiseMeasure(MeasureValue measure, UnitScale scale, FidelityLog log) =>
        (measure.Si / scale.Factor(measure.Dimension, measure.Type.Value)) is var declared
        && MeasureMints.TryGetValue(measure.Type.Value, out var typed) ? typed(declared)
        : CanonicalMeasures.TryGetValue(measure.Dimension, out var canonical) && MeasureMints.TryGetValue(canonical, out var mint) ? mint(declared)
        : log.Noted(FidelityDrop.MeasureFlattened, measure.Type.Value, (IfcValue)new IfcReal(declared));

    // The seam MeasureValue -> the IFC physical quantity by its Dimension row. An off-table dimension has NO IFC
    // physical-quantity spelling and faults CodecReject — the prior silent IfcQuantityLength coercion claimed a wrong
    // quantity type, the same masked-error family the deleted release fallbacks were.
    static Fin<IfcPhysicalQuantity> RaiseQuantity(DatabaseIfc target, PropertyName name, MeasureValue measure, UnitScale scale, Op key) =>
        QuantityRaisers.TryGetValue(measure.Dimension, out var raiser)
            ? FinSucc(raiser(target, name.Value, measure.Si / scale.Factor(measure.Dimension, measure.Type.Value)))
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
    // PropertyValue.Integer carrying the per-parent-continuous child index — an ordinal is a count, never a
    // physical Measure) — the [AMENDMENTS] carrier that makes ComposeKind.Nest's ordered-children promise
    // representable without touching the frozen 5-kind edge algebra.
    internal static readonly PropertyName NestOrdinal = PropertyName.Create("ordinal");

    // The space-boundary level attr the ingress stamps ("" base-undeclared / "1st" / "2nd") — the egress
    // refined-construct discriminant and the Rasm.Compute filter key, declared ONCE here like NestOrdinal so the two
    // projector halves never drift.
    internal static readonly PropertyName BoundaryLevel = PropertyName.Create("BoundaryLevel");

    // The eccentric-connection attr the ingress Structural fold stamps: the UInt128 content key of the mandatory
    // IfcRelConnectsWithEccentricity.ConnectionConstraint STEP fragment preserved in the profiles store — the egress
    // refined-construct discriminant whose constraint reconstitutes from the store, declared ONCE here beside its peers.
    internal static readonly PropertyName EccentricityKey = PropertyName.Create("EccentricityKey");

    // The attr-refined subtype constructs: the three-valued BoundaryLevel attr names the exact IfcRelSpaceBoundary
    // subtype and the EccentricityKey attr the eccentric structural-member subtype, so both riders survive the full
    // cycle instead of degrading to their base class; every other row constructs its own Key.
    static Option<string> Refined(IfcRelKind kind, Relationship edge) => (kind, edge) switch {
        var (k, e) when k == IfcRelKind.SpaceBoundary && e is Relationship.Generic g =>
            g.Attributes.Find(BoundaryLevel).Bind(static v => v switch {
                PropertyValue.Text { Value: "2nd" } => Some("IfcRelSpaceBoundary2ndLevel"),
                PropertyValue.Text { Value: "1st" } => Some("IfcRelSpaceBoundary1stLevel"),
                _                                   => Option<string>.None,
            }),
        var (k, e) when k == IfcRelKind.ConnectsStructMember && e is Relationship.Generic g && g.Attributes.ContainsKey(EccentricityKey) =>
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
    // through the EccentricityKey-refined construct whose mandatory ConnectionConstraint reconstitutes from the
    // ctor-held profiles store's STEP-fragment lane; the material/property/classification edges resolve to None
    // (authored by their dedicated re-author). Each per-pair edge re-authors per (relating, related) pair against the
    // authored entity map — a one-to-many family thus re-emits one IfcRel* per part (denormalized but lossless).
    void ReauthorRelationships(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored) {
        graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Generic g && g.WireName == IfcRelKind.Nests.Key && g.Attributes.ContainsKey(NestOrdinal) ? Some(g) : None)
            .GroupBy(static g => g.Relating)
            .AsIterable()
            .Iter(group => authored.Find(group.Key).IfSome(relating =>
                ignore(IfcRelKind.Nests.Author(target, relating, toSeq(group.OrderBy(OrdinalOf)).Choose(g => authored.Find(g.Related))))));
        graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Connect { Realizing.IsSome: true } c && c.SubKind == ConnectKind.Element ? Some(c) : None)
            .GroupBy(static c => (From: c.From, To: c.To))
            .AsIterable()
            .Iter(group => authored.Find(group.Key.From).IfSome(relating =>
                authored.Find(group.Key.To).IfSome(related =>
                    IfcRelKind.ConnectsRealizing.Author(target, relating, Seq(related)).IfSome(rel => {
                        if (rel is IfcRelConnectsWithRealizingElements realized) {
                            group.AsIterable().Iter(c => c.Realizing.Bind(authored.Find).IfSome(re => {
                                if (re is IfcElement element) { realized.RealizingElements.Add(element); }
                            }));
                        }
                    }))));
        graph.Edges.AsIterable().Iter(edge => {
            // The Rasm-native analytical receipt is the COUNTED deliberate skip — no phantom IfcControl is minted,
            // and the receiving party reads the count instead of trusting prose.
            if (edge is Relationship.Assign { SubKind: var skipped } assessment && skipped == AssignKind.Assessment) {
                ignore(fidelity.Noted(FidelityDrop.AssessmentSkipped, assessment.Definition.Value, unit));
            }
            RelKindOf(edge).IfSome(kind => {
            // The Inverted Assign family (DefinesByType/AssignsToGroup) stored the seam Subject(occurrence)->Definition
            // (the inverse of the IFC relating(type/group)->related), so egress re-inverts to the IFC orientation the
            // row's Relating/Related names expect — the round-trip directionality matching the ingest [NEUTRAL_EDGE_RULING]; every other
            // row already reads in IFC orientation, so the endpoints pass straight through.
            var (ifcRelating, ifcRelated) = kind.Inverted ? (edge.Related, edge.Relating) : (edge.Relating, edge.Related);
            // The eccentric constraint resolves BEFORE the subtype is chosen: IfcRelConnectsWithEccentricity carries a
            // MANDATORY ConnectionConstraint, so the refinement is legal only when the EccentricityKey content key
            // resolves its preserved IfcConnectionGeometry STEP fragment — a store miss drops the refinement and the
            // edge authors as its base binding, never a schema-invalid bare subtype with an unassigned constraint.
            Option<IfcConnectionGeometry> constraint = edge is Relationship.Generic gen && kind == IfcRelKind.ConnectsStructMember
                ? gen.Attributes.Find(EccentricityKey)
                    .Bind(static v => v is PropertyValue.Text t && UInt128.TryParse(t.Value, out UInt128 parsed) ? Some(parsed) : Option<UInt128>.None)
                    .Bind(profiles.Find<IfcConnectionGeometry>)
                : None;
            // A store-missed constraint drops the refinement to the base binding — the COUNTED eccentricity degrade.
            if (edge is Relationship.Generic degraded && kind == IfcRelKind.ConnectsStructMember
                && degraded.Attributes.ContainsKey(EccentricityKey) && constraint.IsNone) {
                ignore(fidelity.Noted(FidelityDrop.EccentricityDegraded, degraded.Relating.Value, unit));
            }
            Option<string> refined = Refined(kind, edge).Filter(_ => kind != IfcRelKind.ConnectsStructMember || constraint.IsSome);
            authored.Find(ifcRelating).IfSome(relating =>
                authored.Find(ifcRelated).IfSome(related =>
                    kind.Author(target, relating, Seq(related), refined).IfSome(rel => {
                        if (rel is IfcRelConnectsWithEccentricity eccentric) { constraint.IfSome(c => eccentric.ConnectionConstraint = c); }
                    })));
            });
        });
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
        // A realizing Connect (Realizing=Some on the element medium) is GROUP-authored by endpoint pair (the fan-in author
        // above, mirroring the ordinal-nest exclusion) so every realizer lands on ONE IfcRelConnectsWithRealizingElements;
        // a bare element Connect (Realizing=None) re-authors IfcRelConnectsElements — realization is the seam
        // Connect.Realizing FIELD, so both carry ConnectKind.Element and the field presence disambiguates (ConnectsRealizing
        // is excluded from the ByNeutral (axis,sub-kind) index for exactly this reason); Path/Port resolve through ForNeutral.
        Relationship.Connect { SubKind: var sub, Realizing.IsSome: true } when sub == ConnectKind.Element => Option<IfcRelKind>.None,
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
    // that bound it at ingest — the [RELATIONSHIP_REEMIT] restraint/load drop this compose closes. Author returns
    // the unconsumed-row residue (the fidelity drop set); ignore marks the deliberate discard.
    static void ReauthorStructural(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored) =>
        graph.Edges.Iter(edge => {
            if (edge is Relationship.Assign { SubKind: var sub, Subject: var subject, Definition: var definition }
                && sub == AssignKind.PropertyDefinition
                && graph.Nodes.Find(definition).Case is Node.PropertySet { Bag: var bag }
                && bag.SetName == StructuralDefinitionSet) {
                authored.Find(subject).IfSome(entity => ignore(StructuralProjection.Author(target, entity, bag.Values)));
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

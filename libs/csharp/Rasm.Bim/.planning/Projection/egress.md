# [BIM_IFC_EGRESS]

The Bim-internal IFC re-author: `SemanticProjector.Emit` lowers a seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` back into IFC STEP/XML/JSON bytes, and `Sniff` reads the schema off foreign bytes BEFORE the database is constructed. `Emit` is a Bim-INTERNAL method on the `Projection/semantic#SEMANTIC_PROJECTOR` projector (a `partial class SemanticProjector` continuation), NOT an `IElementProjection` member — IFC egress is one runtime's wire concern and the seam owns only ingress projection — so it is the exact inverse of the `Project` ingress: where `Project` lowers GeometryGym into seam `Node`s and neutral `Relationship` edges, `Emit` re-authors the seam graph into the IFC entity graph, reading the seam graph ONLY (the `Material` node + the `Associate` edge `MaterialUsage`, the `PropertySet`/`QuantitySet` bag nodes, the `Object` node `Classification`), never a retired `Rasm.Materials` wire carrier. GeometryGym stays captured internally and the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg through `ReleaseRaise`/`Sniff`, never leaking into the seam `Header`.

The egress is the round-trip authority for four invariants the seam delegates to Bim: the `Model/elements#IFC_CLASS` `PredefinedType` egress gate (the predefined token validated against the frozen valid set AND the schema span per `Object`, faulting `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` BARE [C6][H8]), the 1:1 `GlobalId` round-trip (the node `ExternalId` re-emitted, or `ParserIfc.EncodeGuid` minting one for a from-scratch node [H6]), the diff-derived `OwnerHistory` `ChangeAction` (the seam `Node` record equality over the `prior` snapshot, the rooted node matched on its stable 1:1 GlobalId since the `NodeId` is freshly minted each ingest [H9]), and the schema sniff (`FILE_SCHEMA`/`schema_identifier` read off the bytes, never a hardcoded `IFC4X3_ADD2` [H8]). The relationship re-author reverses the `Projection/relations#RELATION_ALGEBRA` roster through `IfcRelKind.ForNeutral`/`Author`, so the long-tail families re-emit exactly as they were read [C5]; the material/property/classification subgraphs re-author through the dedicated `ReauthorMaterials`/`ReauthorProperties`/`ReauthorClassifications` folds the seam-graph egress that REPLACES the retired `Rasm.Materials` material/property wires.

## [01]-[INDEX]

- [01]-[IFC_EGRESS]: `SemanticProjector.Emit` the Bim-internal `ElementGraph` → IFC bytes re-author — the `ReleaseRaise` schema target, the `Model/elements#IFC_CLASS` `PredefinedType` egress gate over the frozen valid set and schema span → `BimFault.UnmappedClass` [C6], the `GlobalId` round-trip [H6], the diff-derived `OwnerHistory` `ChangeAction` re-stamp [H9], `ReauthorMaterials` (`Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` per node + `Associate` usage [C7]), `ReauthorProperties` (the `IfcPropertySet`/`IfcElementQuantity` + `IfcRelDefinesByProperties` round-trip), `ReauthorClassifications` (`Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` per `Object` node), and `ReauthorRelationships` (the `Projection/relations#RELATION_ALGEBRA` neutral edge → `IfcRel*` row-driven re-author) — the seam-graph egress that REPLACES the retired `Rasm.Materials` material wires, plus the `FILE_SCHEMA`/`schema_identifier` `Sniff` [H8].

## [02]-[IFC_EGRESS]

- Owner: `SemanticProjector.Emit` the Bim-internal `ElementGraph`→IFC bytes re-author — NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern; it constructs the `DatabaseIfc` at the `ReleaseRaise` schema target, runs the `Model/elements#IFC_CLASS` `PredefinedType` egress gate per object (publishing a `NodeId`→`IfcProduct` map), and re-authors the entity graph — `ReauthorMaterials` (the `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` per seam `Material` node + `Associate` usage edge [C7]), `ReauthorProperties` (the `IfcPropertySet`/`IfcElementQuantity` rebuilt from the bag nodes + the `IfcRelDefinesByProperties` onto the elements), `ReauthorClassifications` (`Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` per `Object` node), and `ReauthorRelationships` (the neutral-edge → `IfcRel*` row-driven re-author); `Sniff` the ingress counterpart reading `FILE_SCHEMA` (STEP) or `schema_identifier` (JSON) off the bytes BEFORE the database is constructed so the schema is never hardcoded [H8].
- Entry: `SemanticProjector.Emit(ElementGraph graph, FormatIfcSerialization format, Op key, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles)` re-authors the graph into IFC text — for each `Object` node it resolves the `IfcClass` row from the generic `Classification` code, runs `IfcClass.AdmitPredefined(token, objectType, schema, key)` validating the predefined token against the row's frozen valid set AND the schema span → `BimFault.UnmappedClass` [C6][H8], constructs the entity at the resolved schema, assigns the `ExternalId` `GlobalId` (or mints one through `ParserIfc.EncodeGuid` for a from-scratch node) [H6], and re-stamps the `OwnerHistory` with a `ChangeAction` diff-derived against the `prior` snapshot, matching the rooted node on its stable `ExternalId` GlobalId across re-ingest [H9] — publishing the `NodeId`→`IfcProduct` map the re-author folds bind against; `ReauthorMaterials` authors each seam `Material` node's type-level definition + `MaterialPropertySet` Psets ONCE and the per-occurrence `MaterialUsage` [C7] over each `Associate` edge, the `profiles` resolver reconstituting a `ProfileSet`'s `IfcProfileDef` from the content-addressed STEP store; `Fin<T>` aborts on the gate, lifting BARE; `SemanticProjector.Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format)` returns the GeometryGym `ReleaseVersion` the import-rail seeds the database with.
- Auto: `Emit` folds the `graph.Nodes` once — the `Object` egress gate resolves the `IfcClass` row, validates the predefined token and the `IntroducedIn`/`RemovedIn` schema span against `Header.Schema` (an IFC4.3 infrastructure class targeting an IFC2x3 emit faults `BimFault.UnmappedClass` rather than writing an entity the schema forbids), and constructs the entity; the `GlobalId` round-trips from `ExternalId` so a re-imported model re-emits its original GUIDs (1:1) [H6]; `ReauthorRelationships` re-authors the neutral `Compose`/`Connect`/`Void` edges and the `Assign.TypeDefinition`/`Group` edges by reverse-indexing the `IfcRelKind` row and the `Generic` long-tail by its wire-name through `IfcRelKind.Author`, the directionality reconstructing from the row's relating/related names — so the long-tail families re-emit exactly as they were read [C5] (the analytical structural/space-boundary `Generic` edges AND the `Rasm.Compute`-authored `Assign.Assessment` edges are intentionally skipped: they are `Rasm.Compute` analytical enrichment — a FEA utilization, a U-value, an LCA receipt — re-derivable from the content-keyed inputs, NOT IFC-round-trip state, and the seam mints no phantom `IfcPerformanceHistory`/`IfcControl` for a Rasm-native result; an imported IFC `IfcRelAssignsToControl` assessment-family relation rode the rostered `Generic` wire-name path at ingest and re-emits from it, so no IFC assessment relation is dropped; the `Assign.PropertyDefinition` edge re-authors through `ReauthorProperties`, not here); the seam `Material` subgraph re-authors through `ReauthorMaterials` and the property/quantity bags through `ReauthorProperties` (the `IfcPropertySingleValue`/`IfcQuantity*` rebuilt from each typed `PropertyValue`/`MeasureValue` + the `IfcRelDefinesByProperties` onto each bound element) — the seam-graph egress that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers; the `OwnerHistory` re-emit derives the `ChangeAction` from the structural record diff (the seam `Node` record equality over the prior snapshot, the rooted node matched on its stable 1:1 GlobalId `ExternalId` since the neutral `NodeId` is freshly minted each ingest — `ADDED`/`MODIFIED`/`NOCHANGE`) so the IFC owner history reflects the real edit [H9]; the `StepHeader` re-authors `FILE_DESCRIPTION`/`FILE_NAME` from `Header.Step`.
- Auto: `Sniff` reads the schema off the bytes before `new DatabaseIfc` — the STEP `FILE_SCHEMA(('IFC4X3_ADD2'))` header line or the IFC-JSON `schema_identifier` member — mapping the token onto the GeometryGym `ReleaseVersion` and defaulting to `IFC4X3_ADD2` only when the header is absent, so the import never hardcodes 4x3 over a 2x3 file [H8].
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new emit format is one `FormatIfcSerialization` the `db.ToString` switch already carries; a new schema is one `ReleaseVersion` the `Sniff`/`ReleaseRaise` resolves and the span validates; the predefined gate is one `AdmitPredefined` call composed once per object; a new re-authored property kind is one `RaiseProperty` arm — never a per-class egress branch.
- Boundary: `Emit` is Bim-INTERNAL and absent from the `IElementProjection` contract — exposing it on the seam is the named violation; the predefined validity is an EGRESS gate (validated when the IFC entity is authored, against the frozen valid set and the schema span) [C6], never a per-call regex and never silent acceptance of an out-of-schema predefined; the `GlobalId` is the node `ExternalId` round-tripped 1:1 [H6] and a freshly-minted GUID on a re-emitted node is the deleted form; the schema is sniffed [H8] and a hardcoded `IFC4X3_ADD2` over a foreign-schema file is the named defect; the `ChangeAction` is diff-derived [H9] and a blanket `ADDED` stamp is the deleted form; a `Rasm.Compute`-authored `Assign.Assessment` edge is NON-IFC-NATIVE and INTENTIONALLY not re-authored — the analysis receipt (a FEA utilization, a U-value, an LCA) is Rasm-native enrichment re-derivable from the content-keyed inputs, so forcing it into a phantom `IfcRelAssignsToControl`/`IfcPerformanceHistory` (which the graph never carries a node for) is the deleted form; an IMPORTED IFC assessment-family relation (`IfcRelAssignsToControl`) is a rostered `Generic` edge that round-trips by wire-name, so no IFC assessment relation is lost; the material/property/classification egress reads the seam graph ONLY — the `Material` node + the `Associate` edge `MaterialUsage`, the `PropertySet`/`QuantitySet` bag nodes, and the `Object` node `Classification` — and a `Rasm.Materials` wire carrier crossing into `Emit` is the deleted form (those Materials wires are retired), the type-level material definition authored ONCE per `Material` node and the per-occurrence usage wrapping it so a usage duplicated onto the type-level set is the deleted form [C7]; the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg through `ReleaseRaise`/`Sniff` and a leak into the seam `Header` is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// Emit/Sniff continue the Projection/semantic#SEMANTIC_PROJECTOR partial class — the egress half of the
// one projector. The prelude mirrors the ingress page so the partial compiles standalone; the GeometryGym
// ReleaseVersion enum stays codec-leg-local through the GGRelease alias (ReleaseRaise/Sniff target) and
// never reaches the seam Header (the seam currency is the Rasm.Element alias).
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
using GGRelease = GeometryGym.Ifc.ReleaseVersion;     // the GeometryGym IFC-text enum ReleaseRaise/Sniff target

namespace Rasm.Bim;

public sealed partial class SemanticProjector {
    // The Bim-internal IFC egress: ElementGraph -> DatabaseIfc -> bytes. The PredefinedType egress gate [C6] and the
    // schema-span validation [H8] run per Object; the GlobalId round-trips 1:1 [H6]; the OwnerHistory ChangeAction is
    // diff-derived against the prior snapshot [H9]. The Object authoring publishes a NodeId->IfcProduct map the re-author
    // folds bind against. The profiles resolver reconstitutes a ProfileSet's IfcProfileDef from the content-addressed STEP
    // store the ProfileRef.ContentKey keys. Never a seam member.
    public Fin<string> Emit(ElementGraph graph, FormatIfcSerialization format, Op key, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles) {
        var target = new DatabaseIfc(false, ReleaseRaise(graph.Header.Schema));
        // Re-ingest correlation [H6]: the neutral rooted NodeId is freshly minted each Project, so a re-imported graph
        // shares NO NodeId with the prior snapshot — the diff-derived ChangeAction matches a rooted node on the stable
        // 1:1 GlobalId (Node.Object.ExternalId), indexed once here, falling back to the NodeId for a from-scratch node.
        var priorByExternal = prior.Map(static p => p.Nodes.Values
                .Choose(static n => n is Node.Object o ? o.ExternalId.Map(ext => (Ext: ext, Node: o)) : None)
                .Fold(Map<string, Node.Object>(), static (m, e) => m.AddOrUpdate(e.Ext, e.Node)))
            .IfNone(Map<string, Node.Object>());
        return graph.Nodes.Values.Choose(static node => node is Node.Object obj ? Some(obj) : None)
            .Map(obj => Author(target, obj, graph.Header.Schema, key, prior, priorByExternal).Map(product => (Id: obj.Id, Product: product)))
            .Sequence()
            .Map(static authored => authored.Fold(Map<NodeId, IfcProduct>(), static (m, e) => m.AddOrUpdate(e.Id, e.Product)))
            .Bind(products => ReauthorMaterials(target, graph, products, profiles).Map(_ => {
                ReauthorProperties(target, graph, products);
                ReauthorClassifications(target, graph, products);
                ReauthorRelationships(target, graph, products);
                ReauthorHeader(target, graph.Header.Step);
                return target.ToString(format);
            }));
    }

    // The egress gate: resolve the IfcClass row from the generic Classification code, admit the predefined token against
    // the frozen valid set AND the schema span, fault UnmappedClass on a miss [C6][H8], then construct the entity and
    // round-trip the GlobalId [H6] + diff-derived OwnerHistory [H9]. The seam Object carries no IFC ObjectType, so a
    // USERDEFINED predefined falls back to the node Name as its label.
    static Fin<IfcProduct> Author(DatabaseIfc target, Node.Object obj, ReleaseVersion schema, Op key, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal) =>
        IfcClass.Resolve(obj.Classification.Code, key)
            .Bind(cls => cls.AdmitPredefined(obj.PredefinedType.Token, obj.Name, schema, key)
                .Map(_ => {
                    var entity = (IfcProduct)target.Factory.Construct(cls.Key);
                    entity.GlobalId = obj.ExternalId.IfNone(() => ParserIfc.EncodeGuid(Guid.NewGuid()));
                    entity.Name = obj.Name;
                    obj.History.IfSome(_ => entity.OwnerHistory = OwnerHistoryOf(target, ChangeOf(obj, prior, priorByExternal)));
                    return entity;
                }));

    // The ChangeAction is the diff verdict against the prior snapshot through the id-normalized seam Node record equality,
    // never a blanket stamp [H9]: a rooted node matches the prior on the stable 1:1 GlobalId (ExternalId) ACROSS re-ingest since
    // the NodeId is freshly minted each ingest, falling back to the NodeId for a from-scratch node — absent prior ->
    // ADDED, structurally equal -> NOCHANGE, else MODIFIED.
    static IfcChangeActionEnum ChangeOf(Node.Object obj, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal) {
        Option<Node> before = obj.ExternalId.Bind(ext => priorByExternal.Find(ext)).Map(static o => (Node)o)
            | prior.Bind(graph => graph.Find(obj.Id));
        return before.Match(
            None: () => IfcChangeActionEnum.ADDED,
            // The prior rooted node carries a DIFFERENT freshly-minted NodeId each ingest [H6], so the structural compare
            // normalizes the id first — the raw record Equals would otherwise see every re-ingested node as MODIFIED and
            // the NOCHANGE arm would be dead.
            Some: b => b is Node.Object before2 && (before2 with { Id = obj.Id }).Equals(obj) ? IfcChangeActionEnum.NOCHANGE : IfcChangeActionEnum.MODIFIED);
    }

    static IfcOwnerHistory OwnerHistoryOf(DatabaseIfc target, IfcChangeActionEnum change) {
        IfcOwnerHistory history = target.Factory.OwnerHistoryAdded;
        history.ChangeAction = change;
        return history;
    }

    // The schema sniff [H8]: read FILE_SCHEMA / schema_identifier off the bytes before constructing the database, mapping
    // the token onto the GeometryGym ReleaseVersion; default IFC4X3_ADD2 only when the header is absent.
    public static GGRelease Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format) =>
        format == InterchangeFormat.IfcJson
            ? SchemaToken((JsonNode.Parse(bytes.Span) as JsonObject)?["schema_identifier"]?.ToString() ?? "")
            : SchemaToken(StepSchemaLine(bytes.Span));

    static GGRelease SchemaToken(string token) =>
        token.Contains("IFC2X3", StringComparison.OrdinalIgnoreCase) ? GGRelease.IFC2x3
        : token.Contains("IFC4X3", StringComparison.OrdinalIgnoreCase) ? GGRelease.IFC4X3_ADD2
        : token.Contains("IFC4", StringComparison.OrdinalIgnoreCase) ? GGRelease.IFC4
        : GGRelease.IFC4X3_ADD2;

    static string StepSchemaLine(ReadOnlySpan<byte> bytes) {
        string header = System.Text.Encoding.ASCII.GetString(bytes[..Math.Min(bytes.Length, 4096)]);
        int start = header.IndexOf("FILE_SCHEMA", StringComparison.Ordinal);
        return start < 0 ? "" : header[start..Math.Min(header.Length, start + 64)];
    }

    // The seam Material subgraph -> IFC: each Material node authors its type-level definition + Psets ONCE through
    // MaterialProjection.AuthorComposition, then each incident Associate edge authors the per-occurrence MaterialUsage [C7]
    // wrapping the shared definition (AuthorUsage) and the IfcRelAssociatesMaterial onto the bound element — so a wall and
    // its mirror share one IfcMaterialLayerSet with two IfcMaterialLayerSetUsage instances. REPLACES the retired Materials
    // wire carriers; the material reads off the projected graph, never a wire.
    static Fin<Unit> ReauthorMaterials(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products, Func<ProfileRef, Option<IfcProfileDef>> profiles) =>
        graph.Nodes.Values.Choose(static n => n is Node.Material m ? Some(m) : None)
            .Map(material => MaterialProjection.AuthorComposition(target, material, profiles).Map(definition => {
                graph.EdgesAt(material.Id)
                    .Choose(e => e is Relationship.Associate a && a.Resource == material.Id ? Some(a) : None)
                    .Iter(edge => products.Find(edge.Subject).IfSome(product => {
                        _ = new IfcRelAssociatesMaterial(MaterialProjection.AuthorUsage(definition, edge.Usage), Seq((IfcDefinitionSelect)product));
                    }));
                return unit;
            }))
            .Sequence().Map(static _ => unit);

    // The property/quantity bags -> IFC: each PropertySet/QuantitySet node authors its IfcPropertySet/IfcElementQuantity
    // from the typed bag ONCE, then each incident Assign.PropertyDefinition edge authors the IfcRelDefinesByProperties onto
    // the bound element — the round-trip the retired stringly BimElement.PropertyBinding never had, the typed PropertyValue/
    // MeasureValue raising to the IfcPropertySingleValue/IfcQuantity* the seam value carries.
    static void ReauthorProperties(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products) {
        var bags = graph.Nodes.Values
            .Choose(n => AuthorBag(target, n).Map(set => (Id: n.Id, Set: set)))
            .Fold(Map<NodeId, IfcPropertySetDefinition>(), static (m, e) => m.AddOrUpdate(e.Id, e.Set));
        graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Assign a && a.SubKind == AssignKind.PropertyDefinition ? Some(a) : None)
            .Iter(a => bags.Find(a.Definition).IfSome(set => products.Find(a.Subject).IfSome(product => {
                _ = new IfcRelDefinesByProperties((IfcObjectDefinition)product, set);
            })));
    }

    // The empty-bag guard is load-bearing: the IfcPropertySet(name, IEnumerable)/IfcElementQuantity(name, IEnumerable)
    // ctors derive their database from the FIRST member (`members.First().mDatabase`), so an empty bag would throw at the
    // boundary — an empty Pset/Qto carries no IFC data, so it is skipped (its DefinesByProperties edge then resolves no
    // bag and re-authors nothing), lossless and exception-free, never a crashing `.First()` on the empty enumerable.
    static Option<IfcPropertySetDefinition> AuthorBag(DatabaseIfc target, Node node) => node switch {
        Node.PropertySet ps when !ps.Bag.Values.IsEmpty => Some((IfcPropertySetDefinition)new IfcPropertySet(ps.Bag.SetName,
            ps.Bag.Values.AsIterable().Map(kv => RaiseProperty(target, kv.Key, kv.Value)))),
        Node.QuantitySet qs when !qs.Bag.Values.IsEmpty => Some((IfcPropertySetDefinition)new IfcElementQuantity(qs.Bag.SetName,
            qs.Bag.Values.AsIterable().Map(kv => RaiseQuantity(target, kv.Key, kv.Value)))),
        _ => Option<IfcPropertySetDefinition>.None,
    };

    // The seam PropertyValue -> the IFC property re-author: each typed case rebuilds its IFC counterpart so the round-trip
    // preserves the data type the PropertyLowering narrowed — Boolean/Measure their scalar IfcPropertySingleValue ctor, the
    // three-valued Logical a typed IfcLogical (the UNKNOWN state survives), Enumerated the IfcPropertyEnumeratedValue SELECTED
    // list, Reference an IfcPropertyReferenceValue carrying its UsageName, Table an IfcPropertyTableValue carrying the
    // CurveInterpolation curve rule; the Bounded/List composites round-trip as their canonical Render string, never dropped.
    static IfcProperty RaiseProperty(DatabaseIfc target, PropertyName name, PropertyValue value) => value switch {
        PropertyValue.Boolean b    => new IfcPropertySingleValue(target, name.Value, b.Value),
        PropertyValue.Measure m    => new IfcPropertySingleValue(target, name.Value, m.Value.Si),
        PropertyValue.Logical l    => new IfcPropertySingleValue(target, name.Value, new IfcLogical(RaiseLogical(l.Value))),
        PropertyValue.Enumerated e => new IfcPropertyEnumeratedValue(target, name.Value, e.Selected.Map(static s => (IfcValue)new IfcLabel(s))),
        PropertyValue.Reference r  => new IfcPropertyReferenceValue(target, name.Value) { UsageName = r.UsageName.IfNone("") },
        PropertyValue.Table t      => RaiseTable(target, name, t),
        _                          => new IfcPropertySingleValue(target, name.Value, value.Render()),
    };

    // The seam Logical's Option<bool> -> the IFC three-valued IfcLogicalEnum (None is UNKNOWN); the inverse of LogicalOpt.
    static IfcLogicalEnum RaiseLogical(Option<bool> logical) =>
        logical.Match(Some: static b => b ? IfcLogicalEnum.TRUE : IfcLogicalEnum.FALSE, None: static () => IfcLogicalEnum.UNKNOWN);

    // The seam Interpolation -> the IFC IfcCurveInterpolationEnum; the inverse of InterpolationOf.
    static IfcCurveInterpolationEnum RaiseInterp(Interpolation interp) =>
        interp == Interpolation.Linear      ? IfcCurveInterpolationEnum.LINEAR
        : interp == Interpolation.LogLinear ? IfcCurveInterpolationEnum.LOG_LINEAR
        : interp == Interpolation.LogLog    ? IfcCurveInterpolationEnum.LOG_LOG
        : IfcCurveInterpolationEnum.NOTDEFINED;

    // A seam table cell -> an IFC value: a Measure authors a typed IfcReal over its SI magnitude, every other case its
    // canonical Render string, so a defining->defined row re-emits its numeric or token cell.
    static IfcValue RaiseValue(PropertyValue value) => value switch {
        PropertyValue.Measure m => new IfcReal(m.Value.Si),
        _                       => new IfcLabel(value.Render()),
    };

    // The seam Table -> the IFC IfcPropertyTableValue: the defining/defined cells fill the value lists and the seam
    // Interpolation re-authors the CurveInterpolation curve rule, so the lookup-table semantics survive the round-trip.
    static IfcPropertyTableValue RaiseTable(DatabaseIfc target, PropertyName name, PropertyValue.Table table) {
        IfcPropertyTableValue raised = new(target, name.Value) { CurveInterpolation = RaiseInterp(table.Interp) };
        raised.DefiningValues.AddRange(table.Rows.Map(static r => RaiseValue(r.Defining)));
        raised.DefinedValues.AddRange(table.Rows.Map(static r => RaiseValue(r.Defined)));
        return raised;
    }

    // The seam MeasureValue -> the IFC physical quantity by its Dimension row: the SI magnitude authors the matching
    // IfcQuantity* (Area/Volume/Weight/Time/Count, defaulting to Length), so a base-quantity takeoff re-emits its Qto.
    static IfcPhysicalQuantity RaiseQuantity(DatabaseIfc target, PropertyName name, MeasureValue measure) =>
        measure.Dimension == Dimension.AreaDim     ? new IfcQuantityArea(target, name.Value, measure.Si)
        : measure.Dimension == Dimension.VolumeDim ? new IfcQuantityVolume(target, name.Value, measure.Si)
        : measure.Dimension == Dimension.MassDim   ? new IfcQuantityWeight(target, name.Value, measure.Si)
        : measure.Dimension == Dimension.DurationDim ? new IfcQuantityTime(target, name.Value, measure.Si)
        : measure.Dimension == Dimension.Dimensionless ? new IfcQuantityCount(target, name.Value, measure.Si)
        : new IfcQuantityLength(target, name.Value, measure.Si);

    // The element classification set -> IFC: each Object node authors its primary Classification AND every standard-system
    // reference in its Classifications set through ClassificationSystem.Author (which returns None for the "ifc" entity-type
    // code the Author above already resolved as the IfcClass, so the entity type never re-authors as a classification
    // reference) — a Uniclass + OmniClass co-applied object thus re-emits BOTH IfcRelAssociatesClassification references it
    // was imported with. This is the element-classification egress subsuming the retired material-wire classification half.
    static void ReauthorClassifications(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products) =>
        graph.Nodes.Values.Choose(static n => n is Node.Object o ? Some(o) : None)
            .Iter(obj => products.Find(obj.Id).IfSome(product =>
                obj.Classifications.Add(obj.Classification).Iter(classification =>
                    ClassificationSystem.Author(target, (IfcDefinitionSelect)product, classification).IfSome(static _ => { }))));

    // The neutral edge algebra -> IfcRel*: each typed Compose/Connect/Void edge and the Assign.TypeDefinition/Group edge
    // re-author their IFC relationship by the reverse-indexed IfcRelKind row, the Generic long-tail by its wire-name [C5];
    // the material/property/classification edges and the analytical structural/space-boundary Generic edges resolve to None
    // (authored by their dedicated re-author or re-derived by Rasm.Compute), so they are skipped here. Each edge re-authors
    // per (relating, related) pair against the authored product map — a one-to-many family thus re-emits one IfcRel* per
    // part (denormalized but lossless), the row driving the rest.
    static void ReauthorRelationships(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products) =>
        graph.Edges.AsIterable().Iter(edge => RelKindOf(edge).IfSome(kind => {
            // The Inverted Assign family (DefinesByType/AssignsToGroup) stored the seam Subject(occurrence)->Definition
            // (the inverse of the IFC relating(type/group)->related), so egress re-inverts to the IFC orientation the
            // row's Relating/Related names expect — the round-trip directionality matching the ingest [C5]; every other
            // row already reads in IFC orientation, so the endpoints pass straight through.
            var (ifcRelating, ifcRelated) = kind.Inverted ? (edge.Related, edge.Relating) : (edge.Relating, edge.Related);
            products.Find(ifcRelating).IfSome(relating =>
                products.Find(ifcRelated).IfSome(related =>
                    kind.Author(target, (IfcProduct)relating, Seq((IfcProduct)related)).IfSome(rel => {
                        // A realizing Connect re-authors its third endpoint onto IfcRelConnectsWithRealizingElements so the
                        // realizing intermediary round-trips, not just the From/To pair the row-driven Author binds.
                        if (edge is Relationship.Connect { Realizing: var realizing } && rel is IfcRelConnectsWithRealizingElements realized) {
                            realizing.Bind(products.Find).IfSome(re => { if (re is IfcElement element) { realized.RealizingElements.Add(element); } });
                        }
                    })));
        }));

    // The analytical Generic wire-names (structural connectivity + space boundaries) are NOT IFC-round-trip state — they
    // are Rasm.Compute enrichment re-derivable from the content-keyed geometry, so ReauthorRelationships skips them.
    static readonly FrozenSet<string> AnalyticalWires = new[] {
        IfcRelKind.ConnectsStructMember.Key, IfcRelKind.ConnectsStructActivity.Key, IfcRelKind.SpaceBoundary.Key,
    }.ToFrozenSet(StringComparer.Ordinal);

    // The neutral seam edge -> its IfcRelKind row: the typed cases reverse-index (axis, sub-kind) through ForNeutral (the
    // exact inverse of IfcRelKind.Edge), the Generic passthrough resolves by its IFC wire-name. The Assign axis re-authors
    // ONLY the two IFC-objectified sub-kinds the ByNeutral index carries — TypeDefinition (IfcRelDefinesByType) and Group
    // (IfcRelAssignsToGroup); the other two Assign sub-kinds are authored elsewhere or are non-IFC-native: PropertyDefinition
    // is re-authored by ReauthorProperties (the IfcRelDefinesByProperties round-trip), and Assessment is a Rasm.Compute
    // analytical receipt (a FEA utilization, a U-value, an LCA — NOT an IFC entity), so an Assign.Assessment edge is
    // INTENTIONALLY skipped here (the SAME non-IFC-round-trip-state treatment the analytical structural/space-boundary
    // Generic edges get) — an imported IFC IfcRelAssignsToControl assessment-family relation rides the Generic wire-name
    // path instead (it is rostered Generic), so no IFC assessment relation is dropped and no phantom IfcPerformanceHistory/
    // IfcControl entity is minted for a Rasm-native result. An Associate edge returns None (ReauthorMaterials owns it), as
    // does an analytical Generic edge and an unrostered wire-name (never re-authoring an entity the roster never declared).
    static Option<IfcRelKind> RelKindOf(Relationship edge) => edge switch {
        Relationship.Compose c => IfcRelKind.ForNeutral(EdgeAxis.Compose, c.SubKind.Key),
        Relationship.Connect c => IfcRelKind.ForNeutral(EdgeAxis.Connect, c.SubKind.Key),
        Relationship.Void v    => IfcRelKind.ForNeutral(EdgeAxis.Void, v.SubKind.Key),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition || a.SubKind == AssignKind.Group => IfcRelKind.ForNeutral(EdgeAxis.Assign, a.SubKind.Key),
        // PropertyDefinition -> ReauthorProperties; Assessment -> Rasm-native analytical receipt, NOT IFC-round-trip state.
        Relationship.Assign { SubKind: var sub } when sub == AssignKind.PropertyDefinition || sub == AssignKind.Assessment => Option<IfcRelKind>.None,
        Relationship.Generic g when !AnalyticalWires.Contains(g.WireName) && IfcRelKind.TryGet(g.WireName, out IfcRelKind? row) && row is { } resolved => Some(resolved),
        _ => Option<IfcRelKind>.None,
    };

    // The StepHeader -> the STEP physical-file header on the database: FILE_DESCRIPTION (FileDescriptions) and the FILE_NAME
    // fields restored from the seam header [H9], so an import -> export cycle preserves provenance instead of stripping it.
    // FILE_SCHEMA already rides target.Release (set at the DatabaseIfc construction from Header.Schema), so the schema is
    // restored there.
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

- [EGRESS_GATE]: the `PredefinedType` egress gate (resolve the `IfcClass` row from `code`, run `AdmitPredefined(token, objectType, schema, key)` against the frozen valid set + the `IntroducedIn`/`RemovedIn` schema span → `BimFault.UnmappedClass`) grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C6/H8 and the `Model/elements#IFC_CLASS` `IfcClass.Resolve(string, Op)`/`AdmitPredefined(string, string, ReleaseVersion, Op)` owner (both `Op`-keyed); the schema gate ranks the SEAM `ReleaseVersion` chronologically (the `[SmartEnum]` has no ordinal), and the GeometryGym `ReleaseVersion` enum stays on the codec leg through `ReleaseRaise`/`Sniff` + `ParserIfc.EncodeGuid` for the 1:1 `GlobalId` round-trip [H6]; the diff-derived `ChangeAction` grounds against the seam `Node` record equality over the prior snapshot (a rooted node matched on the stable 1:1 GlobalId `ExternalId` since the `NodeId` is freshly minted each ingest) and `IfcOwnerHistory.CreationDate`/`LastModifiedDate` (`DateTime`, lowered through `Instant.FromDateTimeUtc`, never `FromUnixTimeSeconds`) [H9].
- [MATERIAL_PROPERTY_CLASSIFICATION_EGRESS]: the `ReauthorMaterials`/`ReauthorProperties`/`ReauthorClassifications` egress grounds against `ELEMENT-REBUILD-PLAN.md` §6 (the `Rasm.Materials` ripple — the `ComponentProjector` lowers the material subgraph, `Rasm.Bim` reads it) and §4-RT C7 (the `Associate` material edge carries the typed `MaterialUsage`; the type-level `MaterialComposition` set stays). `ReauthorMaterials` composes the `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition(DatabaseIfc, Node.Material, Func<ProfileRef, Option<IfcProfileDef>>)`/`AuthorUsage(IfcMaterialDefinition, MaterialUsage)` (returning `Fin<IfcMaterialDefinition>`/`IfcMaterialSelect`); `ReauthorProperties` rebuilds the `IfcPropertySet(name, IEnumerable<IfcProperty>)`/`IfcElementQuantity(name, IEnumerable<IfcPhysicalQuantity>)` + `IfcRelDefinesByProperties(IfcObjectDefinition, IfcPropertySetDefinition)` (ctors decompile-confirmed) — the round-trip the retired stringly `BimElement.PropertyBinding`/`QuantityBinding` never had; `ReauthorClassifications` composes the `Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author(DatabaseIfc, IfcDefinitionSelect, Classification)` (returning `None` for the `"ifc"` entity-type code), authoring `IfcRelAssociatesClassification`/`IfcClassificationReference`; the `IfcRelAssociatesMaterial(IfcMaterialSelect, IEnumerable<IfcDefinitionSelect>)` egress is decompile-confirmed, the material reading off the projected seam `Material` node, never a retired Materials wire.

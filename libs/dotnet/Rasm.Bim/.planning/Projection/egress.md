# [BIM_IFC_EGRESS]

The Bim-internal IFC re-author: `SemanticProjector.Emit` lowers a seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` back into IFC bytes at the `Projection/wireform#IFC_WIRE_FORM` `IfcWireForm` a caller names.

`Emit` is a Bim-INTERNAL member on the `Projection/semantic#SEMANTIC_PROJECTOR` projector — a `partial class SemanticProjector` continuation, NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern and the seam owns ingress projection alone. It is the exact inverse of `Project`: where `Project` lowers GeometryGym into seam `Node`s and neutral `Relationship` edges, `Emit` re-authors the seam graph into the IFC entity graph, reading the seam graph ONLY and never a retired `Rasm.Materials` wire carrier.

This leg owns the release raise, the `IfcClass` egress gate, the content-derived `GlobalId`, the diff-derived `OwnerHistory`, the declared unit regime, and the re-author leg roster. Value raising is `Projection/raise#VALUE_RAISE`, the wire form and schema sniff `Projection/wireform#IFC_WIRE_FORM`, the drop ledger `Projection/fidelity#FIDELITY_LEDGER`, and the relationship roster `Projection/relations#RELATION_ALGEBRA`.

GeometryGym's `ReleaseVersion` stays on this codec leg through `ReleaseRaise` and never reaches the seam `Header`.

## [01]-[INDEX]

- [02]-[IFC_EGRESS]: `SemanticProjector.Emit` — the gated write, the `Closure`-hulled partial-export slice, the railed `ReleaseRaise`, the accumulating per-node egress gate, the `EmitFrame` carrier and its one edge partition, the re-author leg roster, and the seal.

## [02]-[IFC_EGRESS]

- Owner: `SemanticProjector.Emit` the Bim-internal `ElementGraph`-to-IFC-bytes re-author; `EmitFrame` the one carrier every re-author leg takes (the constructed database, the source graph, the authored entity map, the emit-declared unit regime, the single edge partition, the operation key); `PriorIndex` the diff-prior snapshot beside its `ExternalId` index; `Legs` the leg roster folded on the writer rail; `ReleaseRaise` the seam-to-GeometryGym schema target; `Register` the named construct-registers seam; `StepHeaderMapper` the `[Mapper]` restoring the STEP physical-file header.
- Law: the emit is GATED then TOTAL — the `rasm.bim.projection.emit` veto brackets the WHOLE write through the capsule's GUARDED fire, so a deliverable policy refuses on the wire form, the raised schema, and the in-scope node count BEFORE a `DatabaseIfc` exists and a refusal is the emit's typed verdict rather than an artifact nobody wanted. A bare fire followed by an unconditional write is the deleted form.
- Law: a re-author leg is a VALUE of one shape returning the writer carrier, so a `void` leg — whose failure no caller can observe and whose drops no ledger can count — is unrepresentable. Roster ORDER is the dependency: units declare before any magnitude raises, materials before the usages that wrap them, and the georeference inverse is the last rung before the seal.
- Law: the per-node egress gate ACCUMULATES. One unmappable class, one schema-abstract row, or one unparseable predefined token used to abort the whole emit on a first defect, so an authoring pass saw one reject per run; `Validation<Error, T>` collects every reject across the roster and lowers once through the rail aggregate.
- Entry: `SemanticProjector.Emit(ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default)` returns `Fin<ReadOnlyMemory<byte>>` — BYTES, the one currency, so no caller re-encodes a returned string and a zipped container is expressible. The `EmitContext` carrier holds the four orthogonal emit axes; the `IIfcProfileStore` and `BsddPins` capabilities ride the projector's primary constructor, so a second parameter re-passing an instance dependency is the deleted knob.
- Auto: `Emit` raises the schema, gates `form.Published(schema, key)` so nothing is sealed that a peer decoder refuses, slices the scope, resolves the `"ifc"`-classified `Object` roster ONCE off the sliced model, publishes its magnitude on the admission fact, and folds the authoring inside the guarded fire. A foreign-system node — a sibling projector's native capture classified outside `"ifc"` — is out of scope BY CLASSIFICATION, never a fault that aborts a federated emit. The gate then resolves the `IfcClass` row, rejects the schema-abstract supertype, admits the predefined token per-token against the `PredefinedRow` spans AND the class schema span [PREDEFINED_TOKEN_RULING][H8], constructs the entity (the `IfcProject` row through its db-binding ctor so the file carries its mandatory root context, every other row through `Factory.Construct`), stamps the admitted token through the type-init slot census, round-trips the `GlobalId` from `ExternalId` 1:1 or DERIVES it from the node's own id-inclusive `ContentAddress` [H6], and re-stamps the `OwnerHistory` with a `ChangeAction` diff-derived against the prior snapshot [H9].
- Auto: the legs then run on one rail over one `EmitFrame` — units declared and the tolerance re-declared through the seam `Render` inverse; materials authored ONCE per `Material` node with the per-occurrence `MaterialUsage` wrapping them [OCCURRENCE_USAGE_RULING]; the property and quantity bags raised through `Projection/raise#VALUE_RAISE` and bound by `IfcRelDefinesByProperties`; classifications registered per `Object` node; relationships re-authored ordinal-nests-first, then the realizing fan, then the rostered roster [NEUTRAL_EDGE_RULING]; the structural node payload re-stamped through the `Model/structural#STRUCTURAL_PROJECTION` inverse with its unconsumed-row residue counted; the context root's `Phase`/`LongName` restamped; the STEP header restored; and the georeference inverse authored at its own LoGeoRef level, its `Conversion` collapse over an anisotropy-carrying frame counted [M1].
- Receipt: every bounded drop this leg incurs RETURNS on the `Fidelity` carrier and the joined ledger leaves through the single `Fidelity.Run` at the seal, so a refused write charges the receipt for nothing and a rerun re-derives the same ledger.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, Riok.Mapperly, LanguageExt.Core, Generator.Equals
- Growth: a new re-author concern is one `Legs` row; a new GG release is one `Model/elements#IFC_CLASS` `ReleaseMap` row both lowerings read; a new predefined token or schema span is one `PredefinedRow` the same per-token gate reads; a new declarable unit is one `Projection/value#UNIT_INGRESS` `DeclaredLengths` row the declaration index derives from; a new subtype refinement is one `Refined` arm over its discriminating attr; a new order-bearing relationship family is one wire-name beside `IfcRelKind.Nests.Key` at the ordered-author gate and its `RelKindOf` exclusion arm — never a per-class egress branch.
- Boundary: `Emit` is Bim-INTERNAL and absent from the `IElementProjection` contract. The predefined validity is an EGRESS gate whose admitted token is STAMPED — a gate that validates then discards the token, a per-call regex, or silent acceptance of an out-of-schema predefined is the deleted form; an `EgressEligibility.Vocabulary` class faults, because the row is legal CLASSIFICATION vocabulary and illegal as an authored entity class. The `GlobalId` is the node `ExternalId` round-tripped 1:1 and a from-scratch node's identifier DERIVES from its own content — a `Guid.NewGuid()` mint anywhere on this leg re-keys the same node on every emit, turns every re-export into a whole-file diff, and breaks every external reference into the model; reproducibility is the contract, not an optimization. The `ChangeAction` is the BARE generated `Node.Object.EqualityComparer` structured diff [H9] — the seam owner's `[IgnoreEquality]` `Id` override excludes the fresh mint by construction, so an egress-side member filter and a `with { Id = … }` clone-then-`Equals` are both deleted. One `IfcOwnerHistory` per DISTINCT `ChangeAction`, pre-minted once per emit: mutating the factory's single `OwnerHistoryAdded` per node aliased every earlier assignment, so a later action retro-flipped the whole file.
- Boundary: a SCOPED deliverable is a seam `Graph/element#FEDERATION` `Extract` SLICE over the `Closure` roots and a hull-predicate filter over the whole graph is the deleted form — the slice is Members-closed, so no edge straddles the boundary and the legs meet no half-resolved joint. The per-relationship endpoint compensation the legs still carry therefore answers the FEDERATED and foreign-system-bounded emit ALONE, the case no slice closes: an unguarded rail there aborts on a joint it never writes and charges its receipt for relationships nothing authored, an over-counted ledger being as unusable as a silent one. The bag egress is EMIT-SCOPED — a bag bound only to foreign-system subjects and the projector-minted bookkeeping bags never author, while an unbound source Pset round-trips. The material/property/classification egress reads the seam graph ONLY.
- Boundary: a `Rasm.Compute`-authored `Assign.Assessment` edge is NON-IFC-NATIVE and INTENTIONALLY not re-authored — the analysis receipt is Rasm-native enrichment re-derivable from content-keyed inputs, so a phantom `IfcRelAssignsToControl`/`IfcPerformanceHistory` is the deleted form, while an IMPORTED assessment-family relation round-trips by `Generic` wire-name. An ordinal-bearing `Generic` nest authors ONCE per parent in ordinal order, and a per-pair re-author dropping `IfcRelNests.RelatedObjects` order is the deleted form. A connection interface whose content key the profiles store cannot answer faults `BimFault.Refused` with `BimReason.DanglingReference` — the geometry is OPTIONAL on both carriers, so an ABSENT key is plain topology while a present unanswerable one names a surface the ingest located and the emit lost; the eccentricity degrade is no precedent, its refinement being legally droppable where an interface is not. The emit rail re-authors SEMANTICS alone — no body representation authors here, so the `Semantics/appearance#APPEARANCE_PROJECTION` pair stays armed on a body-representation author joining this rail. The authored map is `NodeId`-to-`IfcObjectDefinition`: a type node authors an `IfcTypeObject` subtype and the context root an `IfcProject`, neither an `IfcProduct`, so a product-typed map is the deleted crash-cast form. `Header.View` round-trips as the VERBATIM `FILE_DESCRIPTION` line the header mapper restores — a `ViewRaise` assigning `DatabaseIfc.ModelView` stands a second release authority beside the railed `ReleaseRaise` and is the rejected form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// Emit continues the Projection/semantic#SEMANTIC_PROJECTOR partial class — the egress half of the one projector,
// reading the profiles/pins fields the declaring part capture-promotes. The prelude carries what THIS fence spells
// and nothing more; the wire-form, value-raise, and ledger clusters carry their own.
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;
using GeometryGym.Ifc;
using GeometryGym.STEP;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Rasm.Element.Classification;
using Rasm.Element.Geospatial;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;
using GGRelease = GeometryGym.Ifc.ReleaseVersion;

namespace Rasm.Bim.Projection;

// --- [MODELS] -----------------------------------------------------------------------------
// The diff prior as ONE value: the snapshot and the ExternalId index DERIVED from it. Threading both as separate
// parameters let a caller pass an index built from a different snapshot, and the correlation depends on them
// agreeing — the rooted NodeId is freshly minted each Project, so the diff matches on the stable GlobalId [H6].
public sealed record PriorIndex(Option<ElementGraph> Graph, Map<string, Node.Object> ByExternal) {
    public static readonly PriorIndex Absent = new(None, Map<string, Node.Object>());

    public static PriorIndex Of(Option<ElementGraph> prior) =>
        new(prior, prior.Map(static graph => graph.Nodes.Values
                .Choose(static node => node is Node.Object obj ? obj.ExternalId.Map(ext => (Ext: ext, Node: obj)) : None)
                .Fold(Map<string, Node.Object>(), static (map, row) => map.AddOrUpdate(row.Ext, row.Node)))
            .IfNone(Map<string, Node.Object>()));
}

// The ONE walk of graph.Edges, yielding every bucket the re-author legs read. Five folds each re-walked the whole
// edge set on a whole-building graph; the predicates that discriminated them are the arms' own law and read here
// beside one another instead of being re-derived five times.
public sealed record EdgeBuckets(
    Seq<Relationship.Generic> Nests,
    Seq<Relationship.Connect> Realizing,
    Map<NodeId, Seq<Relationship.Assign>> Attachments) {
    public static readonly EdgeBuckets Empty =
        new(Seq<Relationship.Generic>(), Seq<Relationship.Connect>(), Map<NodeId, Seq<Relationship.Assign>>());
}

// The one carrier every re-author leg takes. The five-argument thread it replaces was passed seven times, and each
// leg re-derived from it whatever it needed; the partition rides here because the legs SHARE it.
public sealed record EmitFrame(
    DatabaseIfc Target,
    ElementGraph Graph,
    Map<NodeId, IfcObjectDefinition> Authored,
    UnitScheme Scale,
    EdgeBuckets Edges,
    Op Key);

// --- [BOUNDARIES] -------------------------------------------------------------------------
// The StepHeader -> the STEP physical-file header, the ONE field-wise correspondence on this leg [H9]: seven hand
// assignments over one shape are exactly the [MappingTarget] update Mapperly generates, so an import-to-export
// cycle preserves provenance instead of stripping it. Schema is the ignored source column — FILE_SCHEMA rides
// target.Release, set at construction from the railed ReleaseRaise, and a second write here would stand a rival
// release authority.
[Mapper]
[MapperIgnoreSource(nameof(StepHeader.Schema))]
internal static partial class StepHeaderMapper {
    [MapProperty([nameof(StepHeader.Descriptions)], [nameof(STEPFileInformation.FileDescriptions)])]
    [MapProperty([nameof(StepHeader.Name)], [nameof(STEPFileInformation.FileName)])]
    [MapProperty([nameof(StepHeader.Authors)], [nameof(STEPFileInformation.Author)])]
    [MapProperty([nameof(StepHeader.Organizations)], [nameof(STEPFileInformation.Organization)])]
    [MapProperty([nameof(StepHeader.Preprocessor)], [nameof(STEPFileInformation.PreProcessorVersion)])]
    [MapProperty([nameof(StepHeader.TimeStamp)], [nameof(STEPFileInformation.TimeStamp)], Use = nameof(Stamped))]
    public static partial void Restore(StepHeader header, [MappingTarget] STEPFileInformation info);

    [UserMapping] static DateTime Stamped(Instant at) => at.ToDateTimeUtc();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public sealed partial class SemanticProjector {
    // The leg roster folded on the one rail, in dependency order.
    static readonly Seq<Func<SemanticProjector, EmitFrame, WriterT<FidelityLog, Fin, Unit>>> Legs = Seq(
        static (self, frame) => self.ReauthorMaterials(frame),
        static (self, frame) => self.ReauthorProperties(frame),
        static (self, frame) => self.ReauthorClassifications(frame),
        static (self, frame) => self.ReauthorRelationships(frame),
        static (self, frame) => self.ReauthorStructural(frame),
        static (_, frame) => ReauthorProject(frame),
        static (_, frame) => ReauthorHeader(frame),
        static (_, frame) => Georeference(frame));

    // --- [ENTRY]

    // The seam-facing egress: BYTES alone, the one currency, so no caller re-encodes a returned string.
    public Fin<ReadOnlyMemory<byte>> Emit(ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default) =>
        Egress(graph, form, key, context).Map(static run => run.Bytes);

    // The Bim-internal egress, mirroring the ingress Ingest: the bytes AND the run's whole ledger as ONE returned
    // value, so a caller wanting the drop receipt takes this entry rather than reading an instance field a second
    // run half-overwrites.
    public Fin<(ReadOnlyMemory<byte> Bytes, FidelityLog Fidelity)> Egress(
        ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default) =>
        form.Published(graph.Header.Schema, key)
            .Bind(_ => ReleaseRaise(graph.Header.Schema, key))
            .Bind(release => {
                EmitContext ctx = context.IfNone(EmitContext.Whole);
                // The partial-export scope [X1]: a caller-selected ElementQuery closes over what a coherent partial
                // model drags along, and Scoped SLICES that closure into its own graph, so "everything on storey 3
                // in the plumbing domain" emits as a conforming standalone IFC in one expression. The roster
                // resolves ONCE off the sliced model, so the admission fact's count and the authoring fold read
                // one set.
                return Scoped(graph, ctx, key).Bind(model => {
                    Seq<Node.Object> targets = model.Nodes.Values
                        .Choose(static node => node is Node.Object { Classification.System: "ifc" } obj ? Some(obj) : None)
                        .ToSeq();
                    // The GUARDED fire is the one shape — a bare Fire followed by an unconditional write would run
                    // exactly the work the veto exists to stop — and a hook-less composition takes the identical
                    // rail with the body applied directly, paying one IsNone test.
                    BimFact.Egress admission = new(key, form.Key, model.Header.Schema.Key, targets.Count);
                    return ctx.Rail.Match(
                        Some: rail => rail.Fire(BimPoint.Egress, admission, key, _ => Write(model, targets, form, release, ctx, key)),
                        None: () => Write(model, targets, form, release, ctx, key));
                });
            });

    // The write the veto gates: construct at the raised release, author every in-scope node, declare the regime,
    // fold the leg roster on the writer rail, then seal — the ONE run edge, where the joined ledger leaves the
    // carrier exactly once and a refused write charges the receipt for nothing.
    Fin<(ReadOnlyMemory<byte> Bytes, FidelityLog Fidelity)> Write(
        ElementGraph graph, Seq<Node.Object> targets, IfcWireForm form, GGRelease release, EmitContext ctx, Op key) {
        // The release-only ctor is the whole construction: it seeds NO default project, owner history, or unit
        // assignment, which is exactly what an emit authoring its own IfcProject and its own IfcUnitAssignment
        // needs. The `(bool generate, ReleaseVersion)` pair that spelled the same intent chains this ctor and then
        // conditionally seeds — retired upstream, its `false` arm a no-op over this one.
        var target = new DatabaseIfc(release);
        return AuthorAll(target, targets, graph, PriorIndex.Of(ctx.Prior), key)
            .Bind(authored => Declared(target, graph, authored, ctx.Units.IfNone(graph.Header.Units), key))
            .Bind(frame => Fidelity.Run(Legs.TraverseM(leg => leg(this, frame)).As()))
            .Bind(run => form.Seal(target, Entry(graph, form, release))
                .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "ifc-write-refused", form.Key })))
                .Map(bytes => (Bytes: bytes, Fidelity: run.Log)));
    }

    // The writer's ENTRY name: the seam header's own FILE_NAME stem under the form's extension, so GeometryGym's
    // container dispatch reads the form and a zipped emit names its inner entry after the model. A nameless header
    // falls to the release token rather than an empty stem, because the writer derives the zip entry from it.
    static string Entry(ElementGraph graph, IfcWireForm form, GGRelease release) =>
        $"{(Path.GetFileNameWithoutExtension(graph.Header.Step.Name) is { Length: > 0 } stem ? stem : release.ToString())}{form.Extension}";

    // --- [SCOPE]

    // A SCOPED deliverable is a real SLICE, never a filter over the whole graph. The Bim Closure elects WHICH nodes
    // an IFC deliverable needs, then the seam Graph/element#FEDERATION Extract slices that root set into its own
    // graph under the SOURCE Header — an edge joins only with its WHOLE Members set inside, so nothing dangles.
    static Fin<ElementGraph> Scoped(ElementGraph graph, EmitContext ctx, Op key) =>
        ctx.Scope.Match(
            Some: selection => graph.Extract(Closure(graph, selection.Ids).ToSeq(), key),
            None: () => Fin.Succ(graph));

    // The ONE owned law of what a coherent partial model drags along: every selected node, its transitive spatial
    // ancestor chain (Contain first, Aggregate second — the same up-chain the query Ancestry reach walks) to the
    // IfcProject root, and each member's bound type object. Bags, materials, and classifications need no closure
    // rows: the legs gate on authored subjects, so a bag bound only to out-of-scope subjects never authors.
    static LanguageExt.HashSet<NodeId> Closure(ElementGraph graph, Seq<NodeId> selected) {
        Seq<NodeId> ancestors = selected.Bind(id => AncestorChain(graph, id, Seq<NodeId>()));
        Seq<NodeId> types = selected.Bind(id => graph.EdgesAt(id)
            .Choose(e => e is Relationship.Assign { SubKind: var kind } assign && kind == AssignKind.TypeDefinition && assign.Subject == id
                ? Some(assign.Definition) : None).ToSeq());
        return toHashSet(selected).TryAddRange(ancestors).TryAddRange(types);
    }

    static Seq<NodeId> AncestorChain(ElementGraph graph, NodeId node, Seq<NodeId> seen) =>
        graph.EdgesAt(node)
            .Choose(e => e is Relationship.Compose compose && compose.Part == node
                && (compose.SubKind == ComposeKind.Contain || compose.SubKind == ComposeKind.Aggregate) ? Some(compose.Whole) : None)
            .ToSeq().Head
            .Filter(parent => !seen.Contains(parent))
            .Map(parent => parent.Cons(AncestorChain(graph, parent, seen.Add(parent))))
            .IfNone(Seq<NodeId>());

    // --- [SCHEMA_RAISE]

    // The seam->GG raise: the frozen ReleaseMap.Raise identity-name inverse of the ingress ReleaseLower — a seam
    // schema with no GG writer (Ifc5) faults Refused/BimReason.Codec BARE; the IFC4X3_ADD2 silent default is the deleted form.
    internal static Fin<GGRelease> ReleaseRaise(ReleaseVersion schema, Op key) =>
        ReleaseMap.Raise.TryGetValue(schema, out GGRelease raised)
            ? Fin.Succ(raised)
            : Fin.Fail<GGRelease>(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "release-unraisable", schema.Key })));

    // --- [NODE_GATE]

    // The per-node gate over the whole roster, ACCUMULATING: every exact Error lands and the rail lowers the set once.
    // The three OwnerHistory entities pre-mint ONCE here — the key space is exactly the three ChangeAction members
    // and every value derives from the factory's canonical stamp, so the mutable dictionary threaded through the
    // gate as a parameter had nothing to decide.
    Fin<Map<NodeId, IfcObjectDefinition>> AuthorAll(
        DatabaseIfc target, Seq<Node.Object> targets, ElementGraph graph, PriorIndex prior, Op key) {
        Map<IfcChangeActionEnum, IfcOwnerHistory> histories = Histories(target);
        return targets
            .Traverse(obj => (Author(target, obj, graph.Header.Schema, graph.Header.Tolerance, key, prior, histories)).ToValidation()
                .Map(entity => (Id: obj.Id, Entity: entity)))
            .As()
            .Match(
                Succ: rows => Fin.Succ(rows.Fold(Map<NodeId, IfcObjectDefinition>(), static (map, row) => map.AddOrUpdate(row.Id, row.Entity))),
                Fail: errors => Fin.Fail<Map<NodeId, IfcObjectDefinition>>(errors));
    }

    // One IfcOwnerHistory per DISTINCT ChangeAction, minted once per emit: ADDED is the canonical factory stamp
    // verbatim, MODIFIED and NOCHANGE mint through Construct with the canonical stamp donating the owning
    // user/application. Mutating the factory's single OwnerHistoryAdded per node is the deleted aliasing form —
    // every earlier assignment references that one record, so a later action retro-flipped the whole emit.
    static Map<IfcChangeActionEnum, IfcOwnerHistory> Histories(DatabaseIfc target) {
        IfcOwnerHistory canonical = target.Factory.OwnerHistoryAdded;
        return Seq(IfcChangeActionEnum.MODIFIED, IfcChangeActionEnum.NOCHANGE)
            .Fold(Map((IfcChangeActionEnum.ADDED, canonical)), (histories, change) => histories.Add(change, Minted(target, canonical, change)));
    }

    static IfcOwnerHistory Minted(DatabaseIfc target, IfcOwnerHistory canonical, IfcChangeActionEnum change) {
        var history = (IfcOwnerHistory)target.Factory.Construct(nameof(IfcOwnerHistory));
        history.OwningUser = canonical.OwningUser;
        history.OwningApplication = canonical.OwningApplication;
        history.ChangeAction = change;
        return history;
    }

    // The IfcProject row constructs through the db-binding ctor that wires DatabaseIfc.Context; every other row
    // through Factory.Construct. Neither a type node's IfcTypeObject nor the context root is an IfcProduct, so the
    // map is IfcObjectDefinition-wide and a product-typed mint is the deleted crash-cast form. The class gate is
    // AdmitPredefined's own first read (Model/elements#IFC_CLASS Admits): abstract-class-at-egress and
    // class-out-of-schema reach their rows from the vocabulary's eligibility column, so no arm re-derives here.
    static Fin<IfcObjectDefinition> Author(
        DatabaseIfc target, Node.Object obj, ReleaseVersion schema, double tolerance, Op key,
        PriorIndex prior, Map<IfcChangeActionEnum, IfcOwnerHistory> histories) =>
        IfcClass.Resolve(obj.Classification.Code, key)
            .Bind(cls => cls.AdmitPredefined(obj.PredefinedType.Token, obj.ObjectType.IfNone(""), schema, key)
                .Bind(token => {
                    var entity = (IfcObjectDefinition)(cls == IfcClass.Project
                        ? new IfcProject(target, obj.Name)
                        : target.Factory.Construct(cls.Key));
                    entity.GlobalId = obj.ExternalId.IfNone(() => ParserIfc.EncodeGuid(ContentGuid(obj, tolerance)));
                    entity.Name = obj.Name;
                    return StampPredefined(entity, cls, token, obj.ObjectType, key).Map(_ => {
                        obj.History.IfSome(_ => entity.OwnerHistory = histories[ChangeOf(obj, prior)]);
                        return entity;
                    });
                }));

    // The from-scratch GlobalId is DERIVED, never random [H6]: the id-INCLUSIVE ContentAddress (the kernel
    // seed-zero XxHash128 over the node id plus its canonical bytes) is exactly 128 bits, exactly a Guid, and
    // ParserIfc.EncodeGuid compresses it to the 22-character IFC identifier. The id-inclusive address keeps two
    // occurrences of identical content distinct, which a content-EXCLUSIVE address could not.
    static Guid ContentGuid(Node.Object obj, double tolerance) {
        Span<byte> address = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(address, ContentAddress.Of(obj, tolerance).Value);
        return new Guid(address, bigEndian: true);
    }

    // The predefined-type slot per IfcClass row, resolved ONCE at type init off the GeometryGym surface — the
    // relations RelSlots census idiom. A row whose class publishes no writable enum slot carries no entry, so the
    // slotless class succeeds vacuously and a stamp costs one dictionary read, not a per-node reflection probe on
    // every authored entity. The member name is reflected because the per-class enum type is the entity's own.
    static readonly FrozenDictionary<string, PropertyInfo> PredefinedSlots =
        IfcClass.Items.AsIterable()
            .Choose(static row => Optional(typeof(IfcObjectDefinition).Assembly.GetType($"{typeof(IfcObjectDefinition).Namespace}.{row.Key}"))
                .Bind(static shape => Optional(shape.GetProperty("PredefinedType")))
                .Filter(static slot => slot is { CanWrite: true, PropertyType.IsEnum: true })
                .Map(slot => (Class: row.Key, Slot: slot)))
            .ToFrozenDictionary(static row => row.Class, static row => row.Slot, StringComparer.Ordinal);

    // The admitted token stamps the entity's own slot, and USERDEFINED additionally authors the user-defined label
    // on whichever slot the entity family owns — the SAME two-slot pair the ingress UserLabel read, so no bag row,
    // attachment edge, or egress index stands between the ends of the round-trip.
    static Fin<Unit> StampPredefined(IfcObjectDefinition entity, IfcClass cls, string token, Option<string> objectType, Op key) =>
        from labelled in Fin.Succ(Labelled(entity, token, objectType))
        from stamped in Stamp(entity, cls, token, key)
        select stamped;

    // A USERDEFINED node carrying None never reaches here — AdmitPredefined already faulted it
    // predefined-objecttype-miss — and the Name substitution that masked that malformed pair is DELETED: it
    // collapsed two same-named entities holding distinct labels onto one.
    static Unit Labelled(IfcObjectDefinition entity, string token, Option<string> objectType) =>
        token == "USERDEFINED"
            ? objectType.Match(Some: label => entity switch {
                IfcObject occurrence => ignore(occurrence.ObjectType = label),
                IfcElementType type  => ignore(type.ElementType = label),
                _                    => unit,
            }, None: static () => unit)
            : unit;

    // A class publishing no writable enum slot succeeds vacuously; one publishing a slot and a token its own enum
    // cannot parse RAILS, because an entity reading NOTDEFINED where the source declared a value was
    // indistinguishable from a class carrying no slot at all — one silence covering two different facts.
    static Fin<Unit> Stamp(IfcObjectDefinition entity, IfcClass cls, string token, Op key) =>
        !PredefinedSlots.TryGetValue(cls.Key, out PropertyInfo? slot) || slot is null
            ? Fin.Succ(unit)
            : Enum.TryParse(slot.PropertyType, token, ignoreCase: true, out object? member)
                ? Fin.Succ(Stamped(slot, entity, member))
                : Fin.Fail<Unit>(new BimFault.Refused(key, BimScope.Projection, BimReason.Unmapped, string.Join(':', new object?[] { "predefined-token-unstampable", entity.GetType().Name, token })));

    static Unit Stamped(PropertyInfo slot, IfcObjectDefinition entity, object? member) {
        slot.SetValue(entity, member);
        return unit;
    }

    // The ChangeAction is the diff verdict against the prior snapshot, never a blanket stamp [H9]: a rooted node
    // matches the prior on the stable 1:1 GlobalId ACROSS re-ingest, falling back to the NodeId for a from-scratch
    // node — absent prior -> ADDED; present prior -> the generated Node.Object.EqualityComparer structured diff
    // decides NOCHANGE/MODIFIED, the lazy Inequalities enumeration short-circuiting on the first difference.
    static IfcChangeActionEnum ChangeOf(Node.Object obj, PriorIndex prior) =>
        (obj.ExternalId.Bind(ext => prior.ByExternal.Find(ext))
            | prior.Graph.Bind(graph => graph.Find(obj.Id)).Bind(static node => node is Node.Object o ? Some(o) : Option<Node.Object>.None))
        .Match(
            None: static () => IfcChangeActionEnum.ADDED,
            Some: previous => Node.Object.EqualityComparer.Default.Inequalities(previous, obj).Any()
                ? IfcChangeActionEnum.MODIFIED
                : IfcChangeActionEnum.NOCHANGE);

    // --- [UNIT_DECLARATION]

    // The declared-regime index, DERIVED from the ONE Projection/value#UNIT_INGRESS DeclaredLengths roster: its row
    // carries both the UnitsNet member the ingress stamps as the axis token and the GeometryGym assignment family
    // the egress authors, so the two directions read one authority. The hand token-to-family table this replaces
    // was the second half of a correspondence nothing bound to its first. The direct ToFrozenDictionary IS the
    // injectivity gate — two declarations claiming one token fail at type initialization.
    static readonly FrozenDictionary<string, IfcUnitAssignment.Length> DeclaredFamilies =
        IfcUnits.DeclaredLengths.Values.AsIterable()
            .ToFrozenDictionary(static row => row.Metric.ToString(), static row => row.Declared, StringComparer.Ordinal);

    // The regime author and the frame mint in one step [P1], TOTAL rather than railed because every arm lands: the
    // caller-chosen regime (default the model's own declared scheme, so a mm-source import re-emits mm) authors the
    // matching IfcUnitAssignment where the token resolves and keeps the GG SI defaults where it does not; the inverse
    // per-axis scheme derives OFF the constructed database, and the tolerance re-declares through the seam Render
    // inverse like every other magnitude — a bare division by the length axis is the call-site factor multiply the
    // one entry exists to delete. The non-length declared axes ride that SI residue, the regime's one named bounded
    // row, as do the map-conversion offsets and the structural payload magnitudes their owners author.
    // Metre carries its OWN row rather than a factor-of-one fallthrough, which left an explicitly metre-declared
    // model indistinguishable from an undeclared one — the exact confusion the axis TOKEN exists to end.
    static Fin<EmitFrame> Declared(
        DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcObjectDefinition> authored, UnitScheme regime, Op key) {
        regime.UnitFor(QuantityType.Length)
            .Bind(token => DeclaredFamilies.TryGetValue(token, out IfcUnitAssignment.Length family) ? Some(family) : None)
            .IfSome(family => { target.Project.UnitsInContext = new IfcUnitAssignment(target, family); });
        UnitScheme scale = IfcUnits.SchemeOf(target);
        return MeasureValue.OfSi(Dimension.LengthDim, graph.Header.Tolerance, key)
            .Map(scale.Render)
            .Map(declared => {
                target.Tolerance = declared.Value;
                return new EmitFrame(target, graph, authored, scale, Partitioned(graph), key);
            });
    }

    // --- [EDGE_PARTITION]

    // The ONE edge walk: the ordinal-bearing nests, the realizing Connect fan, and the property/quantity
    // attachments keyed by definition. Everything else the rostered fold reads straight off graph.Edges, so no
    // bucket restates the roster's own reverse index.
    static EdgeBuckets Partitioned(ElementGraph graph) =>
        graph.Edges.AsIterable().Fold(EdgeBuckets.Empty, static (buckets, edge) => edge switch {
            Relationship.Generic nest when nest.WireName == IfcRelKind.Nests.Key && nest.Attributes.ContainsKey(NestOrdinal) =>
                buckets with { Nests = buckets.Nests.Add(nest) },
            Relationship.Connect { Realizing.IsSome: true } realizing when realizing.SubKind == ConnectKind.Element =>
                buckets with { Realizing = buckets.Realizing.Add(realizing) },
            Relationship.Assign attachment when attachment.SubKind == AssignKind.PropertyDefinition =>
                buckets with { Attachments = buckets.Attachments.AddOrUpdate(attachment.Definition, s => s.Add(attachment), () => Seq(attachment)) },
            _ => buckets,
        });

    // --- [REGISTRATION]

    // The ONE named registration seam for GeometryGym's construct-registers idiom: an IfcRel* constructor BINDS the
    // entity onto its database as a ctor side effect, so construction IS the authoring act and the reference is
    // spent at the call. Register names that act, so a leg reads as a step instead of scattering `ignore(new
    // IfcRel…)` expressions whose discarded value reads as a dropped result. An entity that comes back unbound is a
    // GG contract break the emit refuses rather than a silent orphan the writer never serializes. STATEMENT
    // EXEMPTION: the construction is the effect, and no expression form can both perform it and name it.
    static Fin<Unit> Register(IfcRelationship registered, Op key) =>
        Optional(registered.Database)
            .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "relationship-unregistered", registered.GetType().Name })))
            .Map(static _ => unit);

    // --- [MATERIAL_LEG]

    // Each Material node authors its type-level definition ONCE and each incident Associate edge the per-occurrence
    // usage wrapping it, so a wall and its mirror share one IfcMaterialLayerSet with two usages. The usage fold
    // binds only AUTHORED subjects: the per-subject Refused/BimReason.DanglingReference fault fired on legitimate federated emits, and
    // a truly-corrupt graph still faults at the seam Link law before any emit sees it.
    WriterT<FidelityLog, Fin, Unit> ReauthorMaterials(EmitFrame frame) =>
        Fidelity.Lift(frame.Graph.Nodes.Values.Choose(static n => n is Node.Material material ? Some(material) : None)
            .Map(material => (Material: material, Usages: frame.Graph.EdgesAt(material.Id)
                .Choose(e => e is Relationship.Associate associate && associate.Resource == material.Id
                    && frame.Authored.ContainsKey(associate.Subject) ? Some(associate) : None)
                .ToSeq()))
            .Filter(static row => !row.Usages.IsEmpty)
            .TraverseM(row => MaterialProjection.AuthorComposition(frame.Target, row.Material, profiles,
                    ProfileSubtypeOf(frame.Graph, row.Material.Id), frame.Scale)
                .Bind(definition => row.Usages
                    .TraverseM(edge => MaterialProjection.AuthorUsage(definition, edge.Usage, frame.Scale)
                        .Bind(select => Register(new IfcRelAssociatesMaterial(select, Seq((IfcDefinitionSelect)frame.Authored[edge.Subject])), frame.Key)))
                    .As().Map(static _ => unit)))
            .As().Map(static _ => unit));

    // The carried profile-subtype read: the material's Associate subject binds its DetailSchema.Realization bag
    // through Assign.PropertyDefinition, and the ProfileSubtype row is the Materials-seeded occupancy-derived
    // profile-def token — resolved off the carried graph row, never a Materials call.
    static Option<string> ProfileSubtypeOf(ElementGraph graph, NodeId materialId) =>
        graph.EdgesAt(materialId)
            .Choose(e => e is Relationship.Associate associate && associate.Resource == materialId ? Some(associate.Subject) : None)
            .Bind(subject => graph.EdgesAt(subject)
                .Choose(e => e is Relationship.Assign assign && assign.SubKind == AssignKind.PropertyDefinition && assign.Subject == subject
                    ? Some(assign.Definition) : None))
            .Choose(definition => graph.Nodes.Find(definition).Case is Node.PropertySet { Bag: var bag } && bag.SetName == DetailSchema.Realization.SetName
                ? bag.Find(DetailSchema.ProfileSubtype).Bind(static v => v is PropertyValue.Text text ? Some(text.Value) : Option<string>.None)
                : Option<string>.None)
            .ToSeq()
            .Head;

    // --- [BAG_LEG]

    // The attachment index decides which bag nodes are THIS wire's data: a bag binding at least one authored
    // subject authors, a bag with NO attachment edge authors too (an unbound source Pset round-trips), and a bag
    // bound ONLY to foreign-system subjects never authors — GG writes every constructed entity, so a federated emit
    // would otherwise strand orphan IfcPropertySets.
    WriterT<FidelityLog, Fin, Unit> ReauthorProperties(EmitFrame frame) =>
        frame.Graph.Nodes.Values
            .Filter(node => node is not Node.PropertySet ps || !Bookkeeping.Value.Contains(ps.Bag.SetName))
            .Filter(node => frame.Edges.Attachments.Find(node.Id)
                .Match(Some: edges => edges.Exists(a => frame.Authored.ContainsKey(a.Subject)), None: () => true))
            .Choose(node => ValueRaise.Bag(frame.Target, node, frame.Authored, frame.Scale, frame.Key)
                .Map(raise => raise.Map(set => (Id: node.Id, Set: set))))
            .AsIterable().ToSeq()
            .TraverseM(identity).As()
            .Bind(rows => Placements(frame).Bind(_ => Bind(frame, rows)));

    // The bookkeeping bags the ingest synthesizes and the egress never exports, ONE roster so a leg cannot test a
    // subset: reconciliation and entity-attribute evidence, each re-authored on the entity by its own leg. Lazy
    // because the set names statics the DECLARING part of this partial holds, and initializer order across the
    // parts of one class is unspecified.
    static readonly Lazy<FrozenSet<PropertyName>> Bookkeeping = new(static () =>
        new[] { TypeSignatureSet, PortAttributeSet, StructuralDefinitionSet, PositioningAttributeSet, ProjectAttributeSet }
            .ToFrozenSet());

    // Each ingest-landed positioning bag whose subject THIS emit authors is a COUNTED linear-placement drop: the
    // station rows are evidence and the IfcLinearPlacement entity re-anchors from content-keyed geometry rather
    // than re-authoring from scalars. The authored gate is load-bearing on the ledger — a scoped or federated emit
    // counting every positioning bag would report drops against elements it never wrote.
    static WriterT<FidelityLog, Fin, Unit> Placements(EmitFrame frame) =>
        frame.Graph.Nodes.Values.AsIterable()
            .Choose(static n => n is Node.PropertySet { Bag.SetName: var set } bag && set == PositioningAttributeSet ? Some(bag) : None)
            .Filter(bag => frame.Edges.Attachments.Find(bag.Id).Exists(edges => edges.Exists(a => frame.Authored.ContainsKey(a.Subject))))
            .ToSeq()
            .TraverseM(bag => Fidelity.Drop(FidelityDrop.LinearPlacement, bag.Id.Value, unit)).As()
            .Map(static _ => unit);

    static WriterT<FidelityLog, Fin, Unit> Bind(EmitFrame frame, Seq<(NodeId Id, IfcPropertySetDefinition Set)> rows) {
        Map<NodeId, IfcPropertySetDefinition> bags =
            rows.Fold(Map<NodeId, IfcPropertySetDefinition>(), static (map, row) => map.AddOrUpdate(row.Id, row.Set));
        return Fidelity.Lift(toSeq(frame.Edges.Attachments.Values).Flatten()
            .Choose(a => bags.Find(a.Definition).Bind(set => frame.Authored.Find(a.Subject).Map(subject => (Subject: subject, Set: set))))
            .TraverseM(pair => Register(new IfcRelDefinesByProperties(pair.Subject, pair.Set), frame.Key))
            .As().Map(static _ => unit));
    }

    // --- [CLASSIFICATION_LEG]

    // The element classification set -> IFC: each Object node authors its primary Classification AND every
    // standard-system reference through ClassificationSystem.Author, which returns None for the "ifc" entity-type
    // code the node gate already resolved as the IfcClass — the ONE lawful skip. A Some is a constructed relation
    // and rides the named registration seam, so the discarded author call whose result no receipt could observe is
    // gone. The BsddPins hosted-version policy every authored dictionary URI derives from is the ctor-held
    // composition value, never an Emit parameter or a per-site literal.
    WriterT<FidelityLog, Fin, Unit> ReauthorClassifications(EmitFrame frame) =>
        Fidelity.Lift(frame.Graph.Nodes.Values.Choose(static n => n is Node.Object obj ? Some(obj) : None)
            .Bind(obj => frame.Authored.Find(obj.Id)
                .Map(entity => obj.Classifications.Add(obj.Classification).ToSeq().Map(row => (Entity: entity, Row: row)))
                .IfNone(Seq<(IfcObjectDefinition Entity, Classification Row)>()))
            .AsIterable().ToSeq()
            .TraverseM(pair => ClassificationSystem.Author(frame.Target, (IfcDefinitionSelect)pair.Entity, pair.Row, pins)
                .Match(Some: rel => Register(rel, frame.Key), None: () => Fin.Succ(unit)))
            .As().Map(static _ => unit));

    // --- [RELATIONSHIP_LEG]

    // The ordered-nest ordinal attribute the ingress stamps onto a Relationship.Generic IfcRelNests edge (a
    // PropertyValue.Integer carrying the per-parent-continuous child index — an ordinal is a count, never a
    // physical Measure): the carrier that makes ComposeKind.Nest's ordered-children promise representable without
    // touching the frozen 5-kind edge algebra.
    internal static readonly PropertyName NestOrdinal = PropertyCategory.Seam.Row("ordinal");

    // The connection-interface attr a space-boundary Generic edge carries: the UInt128 content key of its
    // IfcConnectionGeometry STEP fragment in the profiles store. An element connect carries the same key on the
    // TYPED seam Connect.Interface slot instead — the boundary keeps an attr because the seam ConnectKind medium
    // vocabulary is closed at element/path/port — so both ends read one store through two carriers.
    internal static readonly PropertyName InterfaceKey = PropertyCategory.Seam.Row("InterfaceKey");

    // Order is the law: ordinal nests first (the row author's reflected Add preserves insertion order, which is
    // what carries the sort into RelatedObjects), then the realizing fan, then the rostered fold.
    WriterT<FidelityLog, Fin, Unit> ReauthorRelationships(EmitFrame frame) =>
        Nested(frame).Bind(_ => Realizing(frame)).Bind(_ => Rostered(frame));

    // A group whose children this emit never authored has no relation to write — the emit-scoping law — so it
    // succeeds without calling Author at all rather than reporting an empty related set as a fault.
    static WriterT<FidelityLog, Fin, Unit> Nested(EmitFrame frame) =>
        Fidelity.Lift(frame.Edges.Nests
            .GroupBy(static nest => nest.Relating)
            .AsIterable().ToSeq()
            .TraverseM(group => frame.Authored.Find(group.Key)
                .Map(relating => Ordered(toSeq(group)).Choose(nest => frame.Authored.Find(nest.Related)) is { IsEmpty: false } children
                    ? IfcRelKind.Nests.Author(frame.Target, relating, children, frame.Key).Map(static _ => unit)
                    : Fin.Succ(unit))
                .IfNone(Fin.Succ(unit)))
            .As().Map(static _ => unit));

    // Ordinal-STAMPED edges first in ordinal order, then the unstamped residue in graph order. The 2^256 sentinel
    // that stood for "no ordinal" was a sort key inside the ordinal's own domain, so a large real ordinal sorted
    // as absence; two buckets carry the same total order with nothing to collide with.
    static Seq<Relationship.Generic> Ordered(Seq<Relationship.Generic> nests) =>
        toSeq(nests.Choose(static nest => OrdinalOf(nest).Map(ordinal => (Edge: nest, Ordinal: ordinal)))
                   .OrderBy(static row => row.Ordinal).Map(static row => row.Edge))
        + nests.Filter(static nest => OrdinalOf(nest).IsNone);

    static Option<BigInteger> OrdinalOf(Relationship.Generic edge) =>
        edge.Attributes.Find(NestOrdinal)
            .Bind(static value => value is PropertyValue.Integer integer ? Some(integer.Value) : Option<BigInteger>.None);

    // The realizing fan re-grouped by endpoint pair — split from the rostered fold because the grouping IS the
    // arm's law, not a case of it. Both endpoints gate BEFORE the rail: a scoped export and a federated emit both
    // leave joints this emit never authored, and resolving their interface key on the rail would abort the whole
    // emit over a surface no artifact carries. Every edge of a fan came from ONE relation, so the group's interface
    // key is the head's, resolved BEFORE the author so a store miss faults instead of writing an unlocated joint.
    WriterT<FidelityLog, Fin, Unit> Realizing(EmitFrame frame) =>
        Fidelity.Lift(toSeq(frame.Edges.Realizing.GroupBy(static c => (From: c.From, To: c.To)).AsIterable())
            .Filter(group => frame.Authored.ContainsKey(group.Key.From) && frame.Authored.ContainsKey(group.Key.To))
            .TraverseM(group => InterfaceOf(toSeq(group).Head.Bind(static c => c.Interface), frame.Key)
                .Bind(surface => IfcRelKind.ConnectsRealizing
                    .Author(frame.Target, frame.Authored[group.Key.From], Seq(frame.Authored[group.Key.To]), frame.Key)
                    .Map(rel => Realized(rel, surface, toSeq(group), frame.Authored))))
            .As().Map(static _ => unit));

    static Unit Realized(IfcRelationship rel, Option<IfcConnectionGeometry> surface, Seq<Relationship.Connect> fan, Map<NodeId, IfcObjectDefinition> authored) {
        if (rel is not IfcRelConnectsWithRealizingElements realized) { return unit; }
        surface.IfSome(geometry => realized.ConnectionGeometry = geometry);
        fan.Iter(edge => edge.Realizing.Bind(authored.Find).IfSome(member => {
            if (member is IfcElement element) { realized.RealizingElements.Add(element); }
        }));
        return unit;
    }

    // The rostered per-edge fold: every typed Compose/Connect/Void edge and the Assign.TypeDefinition/Group edge
    // re-author by the reverse-indexed IfcRelKind row, the Generic long-tail by its wire-name.
    WriterT<FidelityLog, Fin, Unit> Rostered(EmitFrame frame) =>
        frame.Graph.Edges.AsIterable().ToSeq().TraverseM(edge => Edge(frame, edge)).As().Map(static _ => unit);

    WriterT<FidelityLog, Fin, Unit> Edge(EmitFrame frame, Relationship edge) =>
        Skipped(frame, edge).Bind(_ => RelKindOf(edge).Match(
            None: static () => Fidelity.Clean(unit),
            Some: kind => Endpoints(frame, edge, kind).Match(
                None: static () => Fidelity.Clean(unit),
                Some: ends => Authored(frame, edge, kind, ends))));

    // The Rasm-native analytical receipt is the RETURNED deliberate skip — no phantom IfcControl is minted and the
    // receiving party reads the count instead of trusting prose. The subject gate keeps that count to what this
    // emit actually wrote, the same ledger law the positioning drop holds.
    static WriterT<FidelityLog, Fin, Unit> Skipped(EmitFrame frame, Relationship edge) =>
        edge is Relationship.Assign { SubKind: var assigned } assessment && assigned == AssignKind.Assessment
            && frame.Authored.ContainsKey(assessment.Subject)
                ? Fidelity.Drop(FidelityDrop.AssessmentSkipped, assessment.Definition.Value, unit)
                : Fidelity.Clean(unit);

    // BOTH endpoints resolve BEFORE any rail work or ledger note, and their absence is the whole verdict for this
    // edge: a scoped export, a federated emit, and a foreign-system endpoint each leave relationships this emit
    // never authors, so resolving one of those on the interface rail would abort the emit over a surface no
    // artifact carries and noting its degrade would charge the receipt for a relationship nothing wrote. The
    // Inverted Assign family stored the seam Subject(occurrence)->Definition, so egress re-inverts to the IFC
    // orientation the row's names expect [NEUTRAL_EDGE_RULING]; every other row already reads in IFC orientation.
    static Option<(IfcObjectDefinition Relating, IfcObjectDefinition Related)> Endpoints(EmitFrame frame, Relationship edge, IfcRelKind kind) =>
        (kind.Inverted ? (Relating: edge.Related, Related: edge.Relating) : (Relating: edge.Relating, Related: edge.Related)) is var ends
        && frame.Authored.Find(ends.Relating).Case is IfcObjectDefinition relating
        && frame.Authored.Find(ends.Related).Case is IfcObjectDefinition related
            ? Some((Relating: relating, Related: related))
            : None;

    // The eccentric constraint resolves BEFORE the subtype is chosen: IfcRelConnectsWithEccentricity carries a
    // MANDATORY ConnectionConstraint, so the refinement is legal only when the Eccentricity content key resolves
    // its preserved STEP fragment — a store miss drops the refinement to the base binding and RETURNS the degrade,
    // never a schema-invalid bare subtype with an unassigned constraint. The interface surface resolves on the rail
    // BEFORE the author, so a key the store cannot answer aborts typed rather than writing an entity whose
    // ConnectionGeometry the ingest recorded and the emit lost.
    WriterT<FidelityLog, Fin, Unit> Authored(EmitFrame frame, Relationship edge, IfcRelKind kind, (IfcObjectDefinition Relating, IfcObjectDefinition Related) ends) {
        Option<UInt128> eccentric = edge is Relationship.Generic gen && kind == IfcRelKind.ConnectsStructMember
            ? gen.Attributes.Find(StructuralProjection.Eccentricity).Bind(ContentKeyOf)
            : None;
        Option<IfcConnectionGeometry> constraint = eccentric.Bind(profiles.Find<IfcConnectionGeometry>);
        WriterT<FidelityLog, Fin, Unit> degrade = eccentric.IsSome && constraint.IsNone
            ? Fidelity.Drop(FidelityDrop.EccentricityDegraded, edge.Relating.Value, unit)
            : Fidelity.Clean(unit);
        Option<string> refined = Refined(kind, edge).Filter(_ => eccentric.IsNone || constraint.IsSome);
        return degrade.Bind(_ => Fidelity.Lift(InterfaceOf(InterfaceKeyOf(edge), frame.Key)
            .Bind(surface => kind.Author(frame.Target, ends.Relating, Seq(ends.Related), frame.Key, refined)
                .Map(rel => Surfaced(rel, surface, constraint)))));
    }

    static Unit Surfaced(IfcRelationship rel, Option<IfcConnectionGeometry> surface, Option<IfcConnectionGeometry> constraint) {
        if (rel is IfcRelConnectsWithEccentricity eccentric) { constraint.IfSome(c => eccentric.ConnectionConstraint = c); }
        return rel switch {
            IfcRelConnectsElements connects => surface.Map(geometry => ignore(connects.ConnectionGeometry = geometry)).IfNone(unit),
            IfcRelSpaceBoundary boundary    => surface.Map(geometry => ignore(boundary.ConnectionGeometry = geometry)).IfNone(unit),
            _                               => unit,
        };
    }

    // The attr-refined subtype constructs: the three-valued BoundaryLevel attr names the exact IfcRelSpaceBoundary
    // subtype and the Model/structural-owned Eccentricity row the eccentric structural-member subtype, so both
    // riders survive the full cycle instead of degrading to their base class; every other row constructs its Key.
    static Option<string> Refined(IfcRelKind kind, Relationship edge) => (kind, edge) switch {
        var (k, e) when k == IfcRelKind.SpaceBoundary && e is Relationship.Generic boundary =>
            boundary.Attributes.Find(BoundaryRows.BoundaryLevel).Bind(static value => value switch {
                PropertyValue.Text { Value: "2nd" } => Some("IfcRelSpaceBoundary2ndLevel"),
                PropertyValue.Text { Value: "1st" } => Some("IfcRelSpaceBoundary1stLevel"),
                _                                   => Option<string>.None,
            }),
        var (k, e) when k == IfcRelKind.ConnectsStructMember && e is Relationship.Generic member
            && member.Attributes.ContainsKey(StructuralProjection.Eccentricity) => Some("IfcRelConnectsWithEccentricity"),
        _ => Option<string>.None,
    };

    // The interface content key an edge carries: an element connect the TYPED seam Connect.Interface slot, a
    // space-boundary Generic edge the InterfaceKey attr — two carriers because the seam ConnectKind medium
    // vocabulary is closed at element/path/port and a space-to-surface boundary is none of them.
    static Option<UInt128> InterfaceKeyOf(Relationship edge) => edge switch {
        Relationship.Connect connect => connect.Interface,
        Relationship.Generic generic => generic.Attributes.Find(InterfaceKey).Bind(ContentKeyOf),
        _                            => Option<UInt128>.None,
    };

    // The ONE decode both attr-borne content keys take: every UInt128 key crosses a PropertyValue.Text as the
    // corpus X32 hex canon, so the parse states HexNumber explicitly — a default decimal parse silently missed
    // every key its writer had already formatted as hex, reading exactly like an absent key.
    static Option<UInt128> ContentKeyOf(PropertyValue value) =>
        value is PropertyValue.Text text && UInt128.TryParse(text.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 parsed)
            ? Some(parsed)
            : Option<UInt128>.None;

    // The preserved-interface reconstitution [M2]: the key names an IfcConnectionGeometry STEP fragment in the
    // ctor-held store. ConnectionGeometry is OPTIONAL on both carriers, so an ABSENT key is plain topology — but a
    // PRESENT key the store cannot answer is the typed Refused/BimReason.DanglingReference, never the eccentricity degrade, whose
    // refinement is legally droppable while a lost interface silently unlocates a joint the source located.
    Fin<Option<IfcConnectionGeometry>> InterfaceOf(Option<UInt128> content, Op key) =>
        content.Match(
            None: static () => Fin.Succ(Option<IfcConnectionGeometry>.None),
            Some: address => profiles.Find<IfcConnectionGeometry>(address)
                .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.DanglingReference, string.Join(':', new object?[] { "connection-interface-miss", address })))
                .Map(Some));

    // A realizing Connect is GROUP-authored above, so it is excluded here: realization is the seam
    // Connect.Realizing FIELD and both arms carry ConnectKind.Element. The Assign axis re-authors ONLY the two
    // IFC-objectified sub-kinds — PropertyDefinition rides the bag leg and Assessment is a Rasm.Compute receipt,
    // not an IFC entity. An Associate edge, an ordinal-bearing nest, and an unrostered wire-name each return None.
    static Option<IfcRelKind> RelKindOf(Relationship edge) => edge switch {
        Relationship.Compose compose => IfcRelKind.ForNeutral(RelationshipKind.Compose, compose.SubKind.Key),
        Relationship.Connect { SubKind: var sub, Realizing.IsSome: true } when sub == ConnectKind.Element => Option<IfcRelKind>.None,
        Relationship.Connect connect => IfcRelKind.ForNeutral(RelationshipKind.Connect, connect.SubKind.Key),
        Relationship.Void voided     => IfcRelKind.ForNeutral(RelationshipKind.Void, voided.SubKind.Key),
        Relationship.Assign assign when assign.SubKind == AssignKind.TypeDefinition || assign.SubKind == AssignKind.Group =>
            IfcRelKind.ForNeutral(RelationshipKind.Assign, assign.SubKind.Key),
        Relationship.Generic nest when nest.WireName == IfcRelKind.Nests.Key && nest.Attributes.ContainsKey(NestOrdinal) => Option<IfcRelKind>.None,
        Relationship.Generic generic when IfcRelKind.TryGet(generic.WireName, out IfcRelKind? row) && row is { } resolved => Some(resolved),
        _ => Option<IfcRelKind>.None,
    };

    // --- [ENTITY_RESTAMP]

    // The Model/structural#STRUCTURAL_PROJECTION Author counterpart: each StructuralDefinition bag re-stamps the
    // node-level AppliedCondition/AppliedLoad on ITS authored structural entity through the attachment edge that
    // bound it at ingest. Author RETURNS the unconsumed-row residue — a payload with no verified re-author ctor —
    // so each residual row NOTES a counted drop anchored on its subject and the GG ctor throw crosses as
    // BimFault.Refused rather than escaping the fold.
    WriterT<FidelityLog, Fin, Unit> ReauthorStructural(EmitFrame frame) =>
        Restamped(frame, StructuralDefinitionSet)
            .TraverseM(row => Fidelity.Lift(StructuralProjection.Author(frame.Target, row.Entity, row.Bag.Values, frame.Key))
                .Bind(residue => residue.TraverseM(_ => Fidelity.Drop(FidelityDrop.StructuralResidue, row.Subject.Value, unit)).As()
                    .Map(static _ => unit)))
            .As().Map(static _ => unit);

    // The ProjectAttributeSet counterpart on the same restamp lane: the context root's Phase lifecycle label and
    // LongName display title re-author VERBATIM onto the constructed IfcContext, so the free-text label round-trips
    // whole and its ProjectStage interpretation stays the Planning/schedule#SCHEDULE caller admission. Text rows
    // alone restamp — the ingest arm mints nothing else into this bag.
    static WriterT<FidelityLog, Fin, Unit> ReauthorProject(EmitFrame frame) =>
        Restamped(frame, ProjectAttributeSet)
            .Choose(static row => row.Entity is IfcContext context ? Some((Context: context, Bag: row.Bag)) : None)
            .TraverseM(static row => Fidelity.Clean(Contexted(row.Context, row.Bag))).As()
            .Map(static _ => unit);

    static Unit Contexted(IfcContext context, PropertyBag bag) {
        bag.Values.Find(Phase).IfSome(value => { if (value is PropertyValue.Text text) { context.Phase = text.Value; } });
        bag.Values.Find(LongName).IfSome(value => { if (value is PropertyValue.Text text) { context.LongName = text.Value; } });
        return unit;
    }

    // The ONE restamp read both entity legs take: a named bookkeeping bag, its attachment edge, and the authored
    // entity that edge bound — resolved off the shared partition, so neither leg re-walks the edge set.
    static Seq<(NodeId Subject, IfcObjectDefinition Entity, PropertyBag Bag)> Restamped(EmitFrame frame, PropertyName set) =>
        toSeq(frame.Edges.Attachments.Values).Flatten()
            .Choose(attachment => frame.Graph.Nodes.Find(attachment.Definition).Case is Node.PropertySet { Bag: var bag } && bag.SetName == set
                ? frame.Authored.Find(attachment.Subject).Map(entity => (Subject: attachment.Subject, Entity: entity, Bag: bag))
                : None)
            .ToSeq();

    static WriterT<FidelityLog, Fin, Unit> ReauthorHeader(EmitFrame frame) =>
        Fidelity.Clean(ignore(StepHeaderMapper.Restore(frame.Graph.Header.Step, frame.Target.OriginatingFileInformation)));

    // --- [GEOREFERENCE_LEG]

    // GeoAuthored.Conversion answers BOTH a genuinely isotropic frame AND an anisotropic one whose pre-IFC4X3_ADD2
    // target carries no IfcMapConversionScaled, so this COUNTED collapse separates them on the seam frame's OWN
    // derived Scale — None exactly when the three axes disagree — never a re-derived level election. The emit scale
    // is the MODEL regime the site elevation alone rides; the authored IfcProjectedCRS declares no MapUnit, so the
    // map ordinates land metre-verbatim.
    static WriterT<FidelityLog, Fin, Unit> Georeference(EmitFrame frame) =>
        Fidelity.Lift(GeoReferenceProjector.Author(frame.Target, frame.Graph.Header.Reference, frame.Scale, frame.Key))
            .Bind(level => level == GeoAuthored.Conversion && frame.Graph.Header.Reference.Scale.IsNone
                ? Fidelity.Drop(FidelityDrop.GeoLevelLowered, level.Key, unit)
                : Fidelity.Clean(unit));
}
```

## [03]-[RESEARCH]

(none)

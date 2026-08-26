# [BIM_IFC_EGRESS]

The Bim-internal IFC re-author: `SemanticProjector.Emit` lowers a shared `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` back into IFC bytes at the `Projection/wireform#IFC_WIRE_FORM` `IfcWireForm` a caller names.

`Emit` is a Bim-INTERNAL member on the `Projection/semantic#SEMANTIC_PROJECTOR` projector — a `partial class SemanticProjector` continuation, NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern and the contract owns ingress projection alone. It is the exact inverse of `Project`: where `Project` lowers GeometryGym into shared `Node`s and neutral `Relationship` edges, `Emit` re-authors the element graph into the IFC entity graph, reading the element graph ONLY and never a retired `Rasm.Materials` wire carrier.

This leg owns the release raise, the `IfcClass` egress gate, the content-derived `GlobalId`, the diff-derived `OwnerHistory`, the declared unit regime, and the re-author leg roster. Value raising is `Projection/raise#VALUE_RAISE`, the wire form and schema sniff `Projection/wireform#IFC_WIRE_FORM`, the drop ledger `Projection/fidelity#FIDELITY_LEDGER`, and the relationship roster `Projection/relations#RELATION_ALGEBRA`.

GeometryGym's `ReleaseVersion` stays on this codec leg through `ReleaseRaise` and never reaches the shared `Header`.

## [01]-[INDEX]

- [02]-[IFC_EGRESS]: `SemanticProjector.Emit` — the gated write, the `Closure`-hulled partial-export slice, the result-returning `ReleaseRaise`, the accumulating per-node egress gate, the `EmitFrame` carrier and its one edge partition, the re-author leg roster, and the seal.

## [02]-[IFC_EGRESS]

- Owner: `SemanticProjector.Emit` the Bim-internal `ElementGraph`-to-IFC-bytes re-author; `EmitFrame` the one carrier every re-author leg takes (the constructed database, the source graph, the authored entity map, the emit-declared unit regime, the single edge partition, the operation key); `PriorIndex` the diff-prior snapshot beside its `ExternalId` index; `Legs` the leg roster folded on the writer path; `ReleaseRaise` the contract-to-GeometryGym schema target; `Register` the named construct-registers boundary; `StepHeaderMapper` the `[Mapper]` restoring the STEP physical-file header.
- Law: the emit is GATED then TOTAL — the `rasm.bim.projection.emit` veto brackets the WHOLE write through the capsule's GUARDED fire, so a deliverable policy refuses on the wire form, the raised schema, and the in-scope node count BEFORE a `DatabaseIfc` exists and a refusal is the emit's typed verdict rather than an artifact nobody wanted. A bare fire followed by an unconditional write is the deleted form.
- Law: a re-author leg is a VALUE of one shape returning the writer carrier, so a `void` leg — whose failure no caller can observe and whose drops no ledger can count — is unrepresentable. Roster ORDER is the dependency: units declare before any magnitude raises, materials before the usages that wrap them, and the georeference inverse is the last rung before the seal.
- Law: the per-node egress gate ACCUMULATES. One unmappable class, one schema-abstract row, or one unparseable predefined token used to abort the whole emit on a first defect, so an authoring pass saw one reject per run; `Validation<Error, T>` collects every reject across the roster and lowers once through the result aggregate.
- Entry: `SemanticProjector.Emit(ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default)` returns `Fin<ReadOnlyMemory<byte>>` — BYTES, the one currency, so no caller re-encodes a returned string and a zipped container is expressible. The `EmitContext` carrier holds the four orthogonal emit axes; the `IIfcProfileStore` and `BsddPins` capabilities ride the projector's primary constructor, so a second parameter re-passing an instance dependency is the deleted knob.
- Auto: `Emit` raises the schema, gates `form.Published(schema, key)` so nothing is sealed that a peer decoder refuses, slices the scope, resolves the `"ifc"`-classified `Object` roster ONCE off the sliced model, publishes its magnitude on the admission fact, and folds the authoring inside the guarded fire. A foreign-system node — a sibling projector's native capture classified outside `"ifc"` — is out of scope BY CLASSIFICATION, never a fault that aborts a federated emit. The gate then resolves the `IfcClass` row, rejects the schema-abstract supertype, admits the predefined token per-token against the `PredefinedRow` spans AND the class schema span [PREDEFINED_TOKEN_RULING][H8], constructs the entity (the `IfcProject` row through its db-binding ctor so the file carries its mandatory root context, every other row through `Factory.Construct`), stamps the admitted token through the type-init slot census, round-trips the `GlobalId` from `ExternalId` 1:1 or DERIVES it from the node's own id-inclusive `ContentAddress` [H6], and re-stamps the `OwnerHistory` with a `ChangeAction` diff-derived against the prior snapshot [H9].
- Auto: the legs then run on one path over one `EmitFrame` — units declared and the tolerance re-declared through the shared `Render` inverse; materials authored ONCE per `Material` node with the per-occurrence `MaterialUsage` wrapping them [OCCURRENCE_USAGE_RULING]; the property and quantity bags raised through `Projection/raise#VALUE_RAISE` and bound by `IfcRelDefinesByProperties`; classifications registered per `Object` node; relationships re-authored ordinal-nests-first, then the realizing fan, then the rostered roster [NEUTRAL_EDGE_RULING]; the structural node payload re-stamped through the `Model/structural#STRUCTURAL_PROJECTION` inverse with its unconsumed-row residue counted; the context root's `Phase`/`LongName` restamped; the STEP header restored; and the georeference inverse authored at its own LoGeoRef level, its `Conversion` collapse over an anisotropy-carrying frame counted [M1].
- Output: every bounded drop this leg incurs RETURNS on the `Fidelity` carrier and the joined ledger leaves through the single `Fidelity.Run` at the seal, so a refused write charges the ledger for nothing and a rerun re-derives the same ledger.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, Riok.Mapperly, LanguageExt.Core, Generator.Equals
- Growth: a new re-author concern is one `Legs` row; a new GG release is one `Model/elements#IFC_CLASS` `ReleaseMap` row both lowerings read; a new predefined token or schema span is one `PredefinedRow` the same per-token gate reads; a new declarable unit is one `Projection/value#UNIT_INGRESS` `DeclaredLengths` row the declaration index derives from; a new subtype refinement is one `Refined` arm over its discriminating attr; a new order-bearing relationship family is one wire-name beside `IfcRelKind.Nests.Key` at the ordered-author gate and its `RelKindOf` exclusion arm — never a per-class egress branch.
- Boundary: `Emit` is Bim-INTERNAL and absent from the `IElementProjection` contract. The predefined validity is an EGRESS gate whose admitted token is STAMPED — a gate that validates then discards the token, a per-call regex, or silent acceptance of an out-of-schema predefined is the deleted form; an `EgressEligibility.Vocabulary` class faults, because the row is legal CLASSIFICATION vocabulary and illegal as an authored entity class. The `GlobalId` is the node `ExternalId` round-tripped 1:1 and a from-scratch node's identifier DERIVES from its own content — a `Guid.NewGuid()` mint anywhere on this leg re-keys the same node on every emit, turns every re-export into a whole-file diff, and breaks every external reference into the model; reproducibility is the contract, not an optimization. The `ChangeAction` is the BARE generated `Node.Object.EqualityComparer` structured diff [H9] — the contract owner's `[IgnoreEquality]` `Id` override excludes the fresh mint by construction, so an egress-side member filter and a `with { Id = … }` clone-then-`Equals` are both deleted. One `IfcOwnerHistory` per DISTINCT `ChangeAction`, pre-minted once per emit: mutating the factory's single `OwnerHistoryAdded` per node aliased every earlier assignment, so a later action retro-flipped the whole file.
- Boundary: a SCOPED deliverable is a shared `Graph/element#FEDERATION` `Extract` SLICE over the `Closure` roots and a hull-predicate filter over the whole graph is the deleted form — the slice is Members-closed, so no edge straddles the boundary and the legs meet no half-resolved joint. The per-relationship endpoint compensation the legs still carry therefore answers the FEDERATED and foreign-system-bounded emit ALONE, the case no slice closes: an unguarded path there aborts on a joint it never writes and charges its ledger for relationships nothing authored, an over-counted ledger being as unusable as a silent one. The bag egress is EMIT-SCOPED — a bag bound only to foreign-system subjects and the projector-minted bookkeeping bags never author, while an unbound source Pset round-trips. The material/property/classification egress reads the element graph ONLY.
- Boundary: a `Rasm.Compute`-authored `Assign.Assessment` edge is NON-IFC-NATIVE and INTENTIONALLY not re-authored — the assessment is Rasm-native enrichment re-derivable from content-keyed inputs, so a phantom `IfcRelAssignsToControl`/`IfcPerformanceHistory` is the deleted form, while an IMPORTED assessment-family relation round-trips by `Generic` wire-name. An ordinal-bearing `Generic` nest authors ONCE per parent in ordinal order, and a per-pair re-author dropping `IfcRelNests.RelatedObjects` order is the deleted form. A connection interface whose content key the profiles store cannot answer faults `BimFault.Refused` with `BimReason.DanglingReference` — the geometry is OPTIONAL on both carriers, so an ABSENT key is plain topology while a present unanswerable one names a surface the ingest located and the emit lost; the eccentricity degrade is no precedent, its refinement being legally droppable where an interface is not. The emit path re-authors SEMANTICS alone — no body representation authors here, so the `Semantics/appearance#APPEARANCE_PROJECTION` pair stays armed on a body-representation author joining this path. The authored map is `NodeId`-to-`IfcObjectDefinition`: a type node authors an `IfcTypeObject` subtype and the context root an `IfcProject`, neither an `IfcProduct`, so a product-typed map is the deleted crash-cast form. `Header.View` round-trips as the VERBATIM `FILE_DESCRIPTION` line the header mapper restores — a `ViewRaise` assigning `DatabaseIfc.ModelView` stands a second release authority beside the result-returning `ReleaseRaise` and is the rejected form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PriorIndex(Option<ElementGraph> Graph, Map<string, Node.Object> ByExternal) {
    public static readonly PriorIndex Absent = new(None, Map<string, Node.Object>());

    public static PriorIndex Of(Option<ElementGraph> prior) =>
        new(prior, prior.Map(static graph => graph.Nodes.Values
                .Choose(static node => node is Node.Object obj ? obj.ExternalId.Map(ext => (Ext: ext, Node: obj)) : None)
                .Fold(Map<string, Node.Object>(), static (map, row) => map.AddOrUpdate(row.Ext, row.Node)))
            .IfNone(Map<string, Node.Object>()));
}

public sealed record EdgeBuckets(
    Seq<Relationship.Generic> Nests,
    Seq<Relationship.Connect> Realizing,
    Map<NodeId, Seq<Relationship.Assign>> Attachments) {
    public static readonly EdgeBuckets Empty =
        new(Seq<Relationship.Generic>(), Seq<Relationship.Connect>(), Map<NodeId, Seq<Relationship.Assign>>());
}

public sealed record EmitFrame(
    DatabaseIfc Target,
    ElementGraph Graph,
    Map<NodeId, IfcObjectDefinition> Authored,
    UnitScheme Scale,
    EdgeBuckets Edges,
    Op Key);

// --- [BOUNDARIES] ----------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed partial class SemanticProjector {
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

    public Fin<ReadOnlyMemory<byte>> Emit(ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default) =>
        Egress(graph, form, key, context).Map(static run => run.Bytes);

    public Fin<(ReadOnlyMemory<byte> Bytes, FidelityLog Fidelity)> Egress(
        ElementGraph graph, IfcWireForm form, Op key, Option<EmitContext> context = default) =>
        form.Published(graph.Header.Schema, key)
            .Bind(_ => ReleaseRaise(graph.Header.Schema, key))
            .Bind(release => {
                EmitContext ctx = context.IfNone(EmitContext.Whole);
                return Scoped(graph, ctx, key).Bind(model => {
                    Seq<Node.Object> targets = model.Nodes.Values
                        .Choose(static node => node is Node.Object { Classification.System: "ifc" } obj ? Some(obj) : None)
                        .ToSeq();
                    BimFact.Egress admission = new(key, form.Key, model.Header.Schema.Key, targets.Count);
                    return ctx.Hooks.Match(
                        Some: hooks => hooks.Fire(BimPoint.Egress, admission, key, _ => Write(model, targets, form, release, ctx, key)),
                        None: () => Write(model, targets, form, release, ctx, key));
                });
            });

    Fin<(ReadOnlyMemory<byte> Bytes, FidelityLog Fidelity)> Write(
        ElementGraph graph, Seq<Node.Object> targets, IfcWireForm form, GGRelease release, EmitContext ctx, Op key) {
        var target = new DatabaseIfc(release);
        return AuthorAll(target, targets, graph, PriorIndex.Of(ctx.Prior), key)
            .Bind(authored => Declared(target, graph, authored, ctx.Units.IfNone(graph.Header.Units), key))
            .Bind(frame => Fidelity.Run(Legs.TraverseM(leg => leg(this, frame)).As()))
            .Bind(run => form.Seal(target, Entry(graph, form, release))
                .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "ifc-write-refused", form.Key })))
                .Map(bytes => (Bytes: bytes, Fidelity: run.Log)));
    }

    static string Entry(ElementGraph graph, IfcWireForm form, GGRelease release) =>
        $"{(Path.GetFileNameWithoutExtension(graph.Header.Step.Name) is { Length: > 0 } stem ? stem : release.ToString())}{form.Extension}";

    // --- [SCOPE]

    static Fin<ElementGraph> Scoped(ElementGraph graph, EmitContext ctx, Op key) =>
        ctx.Scope.Match(
            Some: selection => graph.Extract(Closure(graph, selection.Ids).ToSeq(), key),
            None: () => Fin.Succ(graph));

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

    internal static Fin<GGRelease> ReleaseRaise(ReleaseVersion schema, Op key) =>
        ReleaseMap.Raise.TryGetValue(schema, out GGRelease raised)
            ? Fin.Succ(raised)
            : Fin.Fail<GGRelease>(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "release-unraisable", schema.Key })));

    // --- [NODE_GATE]

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

    static Guid ContentGuid(Node.Object obj, double tolerance) {
        Span<byte> address = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(address, ContentAddress.Of(obj, tolerance).Value);
        return new Guid(address, bigEndian: true);
    }

    static readonly FrozenDictionary<string, PropertyInfo> PredefinedSlots =
        IfcClass.Items.AsIterable()
            .Choose(static row => Optional(typeof(IfcObjectDefinition).Assembly.GetType($"{typeof(IfcObjectDefinition).Namespace}.{row.Key}"))
                .Bind(static shape => Optional(shape.GetProperty("PredefinedType")))
                .Filter(static slot => slot is { CanWrite: true, PropertyType.IsEnum: true })
                .Map(slot => (Class: row.Key, Slot: slot)))
            .ToFrozenDictionary(static row => row.Class, static row => row.Slot, StringComparer.Ordinal);

    static Fin<Unit> StampPredefined(IfcObjectDefinition entity, IfcClass cls, string token, Option<string> objectType, Op key) =>
        from labelled in Fin.Succ(Labelled(entity, token, objectType))
        from stamped in Stamp(entity, cls, token, key)
        select stamped;

    static Unit Labelled(IfcObjectDefinition entity, string token, Option<string> objectType) =>
        token == "USERDEFINED"
            ? objectType.Match(Some: label => entity switch {
                IfcObject occurrence => ignore(occurrence.ObjectType = label),
                IfcElementType type  => ignore(type.ElementType = label),
                _                    => unit,
            }, None: static () => unit)
            : unit;

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

    static IfcChangeActionEnum ChangeOf(Node.Object obj, PriorIndex prior) =>
        (obj.ExternalId.Bind(ext => prior.ByExternal.Find(ext))
            | prior.Graph.Bind(graph => graph.Find(obj.Id)).Bind(static node => node is Node.Object o ? Some(o) : Option<Node.Object>.None))
        .Match(
            None: static () => IfcChangeActionEnum.ADDED,
            Some: previous => Node.Object.EqualityComparer.Default.Inequalities(previous, obj).Any()
                ? IfcChangeActionEnum.MODIFIED
                : IfcChangeActionEnum.NOCHANGE);

    // --- [UNIT_DECLARATION]

    static readonly FrozenDictionary<string, IfcUnitAssignment.Length> DeclaredFamilies =
        IfcUnits.DeclaredLengths.Values.AsIterable()
            .ToFrozenDictionary(static row => row.Metric.ToString(), static row => row.Declared, StringComparer.Ordinal);

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

    static Fin<Unit> Register(IfcRelationship registered, Op key) =>
        Optional(registered.Database)
            .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "relationship-unregistered", registered.GetType().Name })))
            .Map(static _ => unit);

    // --- [MATERIAL_LEG]

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

    static readonly Lazy<FrozenSet<PropertyName>> Bookkeeping = new(static () =>
        new[] { TypeSignatureSet, PortAttributeSet, StructuralDefinitionSet, PositioningAttributeSet, ProjectAttributeSet }
            .ToFrozenSet());

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

    internal static readonly PropertyName NestOrdinal = PropertyCategory.Neutral.Row("ordinal");

    internal static readonly PropertyName InterfaceKey = PropertyCategory.Neutral.Row("InterfaceKey");

    WriterT<FidelityLog, Fin, Unit> ReauthorRelationships(EmitFrame frame) =>
        Nested(frame).Bind(_ => Realizing(frame)).Bind(_ => Rostered(frame));

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

    static Seq<Relationship.Generic> Ordered(Seq<Relationship.Generic> nests) =>
        toSeq(nests.Choose(static nest => OrdinalOf(nest).Map(ordinal => (Edge: nest, Ordinal: ordinal)))
                   .OrderBy(static row => row.Ordinal).Map(static row => row.Edge))
        + nests.Filter(static nest => OrdinalOf(nest).IsNone);

    static Option<BigInteger> OrdinalOf(Relationship.Generic edge) =>
        edge.Attributes.Find(NestOrdinal)
            .Bind(static value => value is PropertyValue.Integer integer ? Some(integer.Value) : Option<BigInteger>.None);

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

    WriterT<FidelityLog, Fin, Unit> Rostered(EmitFrame frame) =>
        frame.Graph.Edges.AsIterable().ToSeq().TraverseM(edge => Edge(frame, edge)).As().Map(static _ => unit);

    WriterT<FidelityLog, Fin, Unit> Edge(EmitFrame frame, Relationship edge) =>
        Skipped(frame, edge).Bind(_ => RelKindOf(edge).Match(
            None: static () => Fidelity.Clean(unit),
            Some: kind => Endpoints(frame, edge, kind).Match(
                None: static () => Fidelity.Clean(unit),
                Some: ends => Authored(frame, edge, kind, ends))));

    static WriterT<FidelityLog, Fin, Unit> Skipped(EmitFrame frame, Relationship edge) =>
        edge is Relationship.Assign { SubKind: var assigned } assessment && assigned == AssignKind.Assessment
            && frame.Authored.ContainsKey(assessment.Subject)
                ? Fidelity.Drop(FidelityDrop.AssessmentSkipped, assessment.Definition.Value, unit)
                : Fidelity.Clean(unit);

    static Option<(IfcObjectDefinition Relating, IfcObjectDefinition Related)> Endpoints(EmitFrame frame, Relationship edge, IfcRelKind kind) =>
        (kind.Inverted ? (Relating: edge.Related, Related: edge.Relating) : (Relating: edge.Relating, Related: edge.Related)) is var ends
        && frame.Authored.Find(ends.Relating).Case is IfcObjectDefinition relating
        && frame.Authored.Find(ends.Related).Case is IfcObjectDefinition related
            ? Some((Relating: relating, Related: related))
            : None;

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

    static Option<UInt128> InterfaceKeyOf(Relationship edge) => edge switch {
        Relationship.Connect connect => connect.Interface,
        Relationship.Generic generic => generic.Attributes.Find(InterfaceKey).Bind(ContentKeyOf),
        _                            => Option<UInt128>.None,
    };

    static Option<UInt128> ContentKeyOf(PropertyValue value) =>
        value is PropertyValue.Text text && UInt128.TryParse(text.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 parsed)
            ? Some(parsed)
            : Option<UInt128>.None;

    Fin<Option<IfcConnectionGeometry>> InterfaceOf(Option<UInt128> content, Op key) =>
        content.Match(
            None: static () => Fin.Succ(Option<IfcConnectionGeometry>.None),
            Some: address => profiles.Find<IfcConnectionGeometry>(address)
                .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.DanglingReference, string.Join(':', new object?[] { "connection-interface-miss", address })))
                .Map(Some));

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

    WriterT<FidelityLog, Fin, Unit> ReauthorStructural(EmitFrame frame) =>
        Restamped(frame, StructuralDefinitionSet)
            .TraverseM(row => Fidelity.Lift(StructuralProjection.Author(frame.Target, row.Entity, row.Bag.Values, frame.Key))
                .Bind(residue => residue.TraverseM(_ => Fidelity.Drop(FidelityDrop.StructuralResidue, row.Subject.Value, unit)).As()
                    .Map(static _ => unit)))
            .As().Map(static _ => unit);

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

    static Seq<(NodeId Subject, IfcObjectDefinition Entity, PropertyBag Bag)> Restamped(EmitFrame frame, PropertyName set) =>
        toSeq(frame.Edges.Attachments.Values).Flatten()
            .Choose(attachment => frame.Graph.Nodes.Find(attachment.Definition).Case is Node.PropertySet { Bag: var bag } && bag.SetName == set
                ? frame.Authored.Find(attachment.Subject).Map(entity => (Subject: attachment.Subject, Entity: entity, Bag: bag))
                : None)
            .ToSeq();

    static WriterT<FidelityLog, Fin, Unit> ReauthorHeader(EmitFrame frame) =>
        Fidelity.Clean(ignore(StepHeaderMapper.Restore(frame.Graph.Header.Step, frame.Target.OriginatingFileInformation)));

    // --- [GEOREFERENCE_LEG]

    static WriterT<FidelityLog, Fin, Unit> Georeference(EmitFrame frame) =>
        Fidelity.Lift(GeoReferenceProjector.Author(frame.Target, frame.Graph.Header.Reference, frame.Scale, frame.Key))
            .Bind(level => level == GeoAuthored.Conversion && frame.Graph.Header.Reference.Scale.IsNone
                ? Fidelity.Drop(FidelityDrop.GeoLevelLowered, level.Key, unit)
                : Fidelity.Clean(unit));
}
```

## [03]-[RESEARCH]

(none)

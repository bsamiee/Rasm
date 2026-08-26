# [BIM_RELATION_ALGEBRA]

The IFC relationship vocabulary `Rasm.Bim` owns as the SOLE GeometryGym/IFC owner: the `IfcRelKind` `[SmartEnum<string>]` roster over the full concrete node-to-node `IfcRelationship` subtree — each row carrying the IFC relating/related inverse-attribute names and the neutral edge constructor it lowers through — and the `EdgeProjection` fold landing every relationship family on a NEUTRAL `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship` edge.

This page is the relationship half of the `Projection/semantic#SEMANTIC_PROJECTOR` ingress, and the SAME roster reverses at the `Projection/egress#IFC_EGRESS` re-author through `ForNeutral` and `Author`.

GeometryGym leaks no relationship case below the boundary: the typed case carries only its `SubKind`, the IFC wire-name and inverse living on the ROW, and only `Generic` carries a wire-name and a per-edge attribute payload [NEUTRAL_EDGE_RULING].

The roster is the ADMISSION for that wire-name — a `WireName` mints from a row key and nowhere else — so a name no row declares cannot be constructed on the producing side.

## [01]-[INDEX]

- [02]-[RELATION_ALGEBRA]: `IfcRelKind` the full `IfcRel*` roster and its wire-name admission, `RelSlots` the reflected fill-and-read surface the census resolves once, and the `EdgeProjection` fold — the row-driven generic families, the ordinal-carrying nests, the realizing fan, the property attachment, and the structural/space-boundary/material payload arms.

## [02]-[RELATION_ALGEBRA]

- Owner: `IfcRelKind` the `[SmartEnum<string>]` roster keyed on the IFC relationship entity name, each row carrying its relating/related inverse-attribute names and its `[UseDelegateFromConstructor]` `Lower` arm; `RelSlots` the reflected relating/related/`Add` triple ONE census resolves per row, read by BOTH the ingest fold and the egress author; `EdgeProjection` the static fold lowering every family onto neutral edges.
- Cases: the typed families `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelReferencedInSpatialStructure` (`Compose`), `IfcRelConnectsElements`/`IfcRelConnectsWithRealizingElements`/`IfcRelConnectsPorts` (`Connect`), `IfcRelDefinesByType`/`IfcRelAssignsToGroup` (`Assign`), `IfcRelVoidsElement`/`IfcRelFillsElement` (`Void`), and the twenty-one families that lower to `Generic` because no neutral sub-kind fits (the port-to-element and path-element joins, the three structural bindings, the space boundary, interference, sequence, the two covering families, the four non-group assignments, declaration, building service, the additive projection, surface-feature adherence, flow control, positioning, and declaring-object typing). `IfcRelAssociatesMaterial`, `IfcRelDefinesByProperties`, the structural/space-boundary payloads, and the two realizer-carrying connects (`RealizingElements` set fan-out; the ports join's optional `RealizingElement`) ride DEDICATED folds, not the generic path.
- Law: the row's `Lower` arm is the SINGLE owner of the row's neutral identity — the lowering axis, the neutral `SubKind`, and the directionality inversion all READ OFF it through one sentinel probe, so no Bim-side axis enum stands beside the contract's own `RelationshipKind` and no hand-kept `Inverted` column can drift from the arm that built the edge.
- Law: the wire name is the ROW KEY, supplied at the ONE lowering site rather than repeated inside each passthrough arm. Twenty-one delegate literals restated column one of their own row and a census arm existed solely to prove they matched; the key supplied here makes that verdict unrepresentable rather than checked.
- Law: a census hit proves CALLABILITY, never a name — a row's related slot resolves the exact `Add(memberType)` overload the fan-out invokes when the slot is a SET and its setter when it is single-valued, and the census, the ingest READ, and the egress FILL are ONE resolution. A name-only `GetMethod("Add")` matches several arities and throws the ambiguous match before naming which one it meant.
- Law: an ABSENT endpoint attribute and an UNROOTED one are two verdicts. GeometryGym backs a missing endpoint with null; coalescing that to an empty string handed a malformed file and a projection gap to one fault whose detail was empty, and a federation manager reads no difference between a broken source and a missed projection.
- Entry: `EdgeProjection.All(project, rooted, tolerance, scale, eurocode, templates, profiles, key)` returns `WriterT<FidelityLog, Fin, Seq<Relationship>>` — the fold's own fidelity facts RETURNED beside the edges, never a ledger this page holds; `IfcRelKind.Admit(wireName, key)` re-admits a crossed wire name against the roster; `kind.Author(db, relating, related, key, refined)` is the egress fill.
- Auto: the generic families take ONE row-driven fold reading the census-resolved slots — the relating endpoint through its slot, the related endpoints through theirs (a SET slot yielding its members, a single-valued slot the one), every value admitted through the same `IfcRoot` read, and the row's own derived `Inverted` deciding the shared orientation. The arm groups are INDEPENDENT and ACCUMULATE, so one dangling endpoint no longer hides every other family's rejects. Bespoke arms survive only where a real extra law lives: the ordered nest's running ordinal, the realizing fan-out, the many-to-many property attachment, and the three payload folds.
- Auto: `IfcRelNests.RelatedObjects` is a schema `LIST`, so INGEST routes every nest through `Generic` stamping the egress-declared `SemanticProjector.NestOrdinal` attribute; ordinals are PER-PARENT CONTINUOUS across relations because `IsNestedBy` is a schema SET, so a parent with N nest relations collides per-relation zero-based indices and the egress per-parent merge interleaves nondeterministically. The typed `Compose{Nest}` case and its canonical bytes stay byte-identical for authored graphs.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new IFC relationship is one `IfcRelKind` row — its inverse-attribute names, and a `Lower` arm only where a typed shared case fits — with one dedicated `EdgeProjection` arm where it carries a payload. A relationship to a NON-node IFC resource is a SEPARATE Growth axis: a new shared `Node` case plus its row, never silently in scope of the covered set. A rider subtype's extra payload is either SOLVED or a NAMED bounded drop: the eccentricity, the connection interface, and the space-boundary level are solved, while `IfcRelAssignsToGroupByFactor.Factor` and the `IfcRelOverridesProperties` override semantics stay counted drops on the `NestOrdinal` precedent.
- Boundary: the shared `Relationship` is the NEUTRAL edge algebra plus `Generic` — re-introducing typed `IfcRel*` cases is the deleted form [NEUTRAL_EDGE_RULING]; the IFC names, directionality, and inverse live HERE, reconstructing at egress through the reverse index. The material occurrence-usage rides the `Associate` edge's typed `MaterialUsage` payload [OCCURRENCE_USAGE_RULING] and a parallel usage node is deleted; the structural and space-boundary connectivity ride the NEUTRAL `Generic` wire-name payload, so a space boundary's interface surface rides the `InterfaceKey` ATTR while an element connect's rides the typed `Connect.Interface` SLOT — the shared `ConnectKind` medium vocabulary is closed at element/path/port and a space-to-surface boundary is none of the three, so a fourth medium row minted to reach the typed slot is the deleted phantom. Both keys name the same preserved STEP fragment in the one store. The census verdict rides the `Fin` result BOTH entrypoints already return, so a pin bump that renames an attribute, narrows a SET member type, or seals a setter refuses at the first roster touch with a rostered token — where a type-initializer throw died in a frame no caller reads and no diagnostic vocabulary owns.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections;
using System.Collections.Frozen;
using System.Reflection;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Projection;

// --- [MODELS] --------------------------------------------------------------------------
sealed record RelSlots(PropertyInfo Relating, PropertyInfo Related, Option<MethodInfo> Add) {
    public static Option<RelSlots> Of(IfcRelKind row) =>
        from shape in Optional(typeof(IfcRelationship).Assembly.GetType($"{typeof(IfcRelationship).Namespace}.{row.Key}"))
        from relating in Optional(shape.GetProperty(row.Relating)).Filter(static slot => slot.CanWrite)
        from related in Optional(shape.GetProperty(row.Related))
        from resolved in Adder(related.PropertyType) is { IsSome: true } add ? Some(new RelSlots(relating, related, add))
            : related.CanWrite ? Some(new RelSlots(relating, related, Option<MethodInfo>.None))
            : Option<RelSlots>.None
        select resolved;

    public Option<string> RelatingId(IfcRelationship rel) => Rooted(Relating.GetValue(rel));

    public Seq<Option<string>> RelatedIds(IfcRelationship rel) =>
        Add.IsSome
            ? toSeq(Optional(Related.GetValue(rel) as IEnumerable)
                .Map(static members => toSeq(members.Cast<object?>())).IfNone(Seq<object?>())).Map(Rooted)
            : Seq(Rooted(Related.GetValue(rel)));

    static Option<string> Rooted(object? value) => Optional(value as IfcRoot).Map(static root => root.GlobalId);

    static Option<MethodInfo> Adder(Type slotType) =>
        Optional(slotType.GetInterfaces().FirstOrDefault(static face =>
                face.IsGenericType && face.GetGenericTypeDefinition() == typeof(ICollection<>)))
            .Bind(face => Optional(slotType.GetMethod(nameof(ICollection<object>.Add), [face.GetGenericArguments()[0]])));
}

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class IfcRelKind {
    public static readonly IfcRelKind Aggregates             = new("IfcRelAggregates",                   "RelatingObject",            "RelatedObjects",              static (_, r, e) => new Relationship.Compose(r, e, ComposeKind.Aggregate));
    public static readonly IfcRelKind Nests                  = new("IfcRelNests",                        "RelatingObject",            "RelatedObjects",              static (_, r, e) => new Relationship.Compose(r, e, ComposeKind.Nest));
    public static readonly IfcRelKind ContainedInStructure   = new("IfcRelContainedInSpatialStructure",  "RelatingStructure",         "RelatedElements",             static (_, r, e) => new Relationship.Compose(r, e, ComposeKind.Contain));
    public static readonly IfcRelKind ReferencedInStructure  = new("IfcRelReferencedInSpatialStructure", "RelatingStructure",         "RelatedElements",             static (_, r, e) => new Relationship.Compose(r, e, ComposeKind.Reference));
    public static readonly IfcRelKind ConnectsElements       = new("IfcRelConnectsElements",             "RelatingElement",           "RelatedElement",              static (_, r, e) => new Relationship.Connect(r, e, ConnectKind.Element, Option<NodeId>.None, Option<UInt128>.None));
    public static readonly IfcRelKind ConnectsRealizing      = new("IfcRelConnectsWithRealizingElements","RelatingElement",           "RelatedElement",              static (_, r, e) => new Relationship.Connect(r, e, ConnectKind.Element, Option<NodeId>.None, Option<UInt128>.None));
    public static readonly IfcRelKind ConnectsPorts          = new("IfcRelConnectsPorts",                "RelatingPort",              "RelatedPort",                 static (_, r, e) => new Relationship.Connect(r, e, ConnectKind.Port, Option<NodeId>.None, Option<UInt128>.None));
    public static readonly IfcRelKind DefinesByType          = new("IfcRelDefinesByType",                "RelatingType",              "RelatedObjects",              static (_, r, e) => new Relationship.Assign(r, e, AssignKind.TypeDefinition));
    public static readonly IfcRelKind AssignsToGroup         = new("IfcRelAssignsToGroup",               "RelatingGroup",             "RelatedObjects",              static (_, r, e) => new Relationship.Assign(r, e, AssignKind.Group));
    public static readonly IfcRelKind Voids                  = new("IfcRelVoidsElement",                 "RelatingBuildingElement",   "RelatedOpeningElement",       static (_, r, e) => new Relationship.Void(r, e, VoidKind.Void));
    public static readonly IfcRelKind Fills                  = new("IfcRelFillsElement",                 "RelatingOpeningElement",    "RelatedBuildingElement",      static (_, r, e) => new Relationship.Void(r, e, VoidKind.Fill));
    public static readonly IfcRelKind ConnectsPortToElement  = new("IfcRelConnectsPortToElement",        "RelatingPort",              "RelatedElement",              Passthrough);
    public static readonly IfcRelKind ConnectsPathElements   = new("IfcRelConnectsPathElements",         "RelatingElement",           "RelatedElement",              Passthrough);
    public static readonly IfcRelKind ConnectsStructMember   = new("IfcRelConnectsStructuralMember",     "RelatingStructuralMember",  "RelatedStructuralConnection", Passthrough);
    public static readonly IfcRelKind ConnectsStructActivity = new("IfcRelConnectsStructuralActivity",   "RelatingElement",           "RelatedStructuralActivity",   Passthrough);
    public static readonly IfcRelKind ConnectsStructElement  = new("IfcRelConnectsStructuralElement",    "RelatingElement",           "RelatedStructuralMember",     Passthrough);
    public static readonly IfcRelKind SpaceBoundary          = new("IfcRelSpaceBoundary",                "RelatingSpace",             "RelatedBuildingElement",      Passthrough);
    public static readonly IfcRelKind Projects               = new("IfcRelProjectsElement",              "RelatingElement",           "RelatedFeatureElement",       Passthrough);
    public static readonly IfcRelKind Adheres                = new("IfcRelAdheresToElement",             "RelatingElement",           "RelatedSurfaceFeatures",      Passthrough);
    public static readonly IfcRelKind FlowControl            = new("IfcRelFlowControlElements",          "RelatingPort",              "RelatedElement",              Passthrough);
    public static readonly IfcRelKind Positions              = new("IfcRelPositions",                    "RelatingPositioningElement","RelatedProducts",             Passthrough);
    public static readonly IfcRelKind DefinesByObject        = new("IfcRelDefinesByObject",              "RelatingObject",            "RelatedObjects",              Passthrough);
    public static readonly IfcRelKind InterferesElements     = new("IfcRelInterferesElements",           "RelatingElement",           "RelatedElement",              Passthrough);
    public static readonly IfcRelKind Sequence               = new("IfcRelSequence",                     "RelatingProcess",           "RelatedProcess",              Passthrough);
    public static readonly IfcRelKind CoversElements         = new("IfcRelCoversBldgElements",           "RelatingBuildingElement",   "RelatedCoverings",            Passthrough);
    public static readonly IfcRelKind CoversSpaces           = new("IfcRelCoversSpaces",                 "RelatingSpace",             "RelatedCoverings",            Passthrough);
    public static readonly IfcRelKind AssignsToControl       = new("IfcRelAssignsToControl",             "RelatingControl",           "RelatedObjects",              Passthrough);
    public static readonly IfcRelKind AssignsToProcess       = new("IfcRelAssignsToProcess",             "RelatingProcess",           "RelatedObjects",              Passthrough);
    public static readonly IfcRelKind AssignsToProduct       = new("IfcRelAssignsToProduct",             "RelatingProduct",           "RelatedObjects",              Passthrough);
    public static readonly IfcRelKind AssignsToActor         = new("IfcRelAssignsToActor",               "RelatingActor",             "RelatedObjects",              Passthrough);
    public static readonly IfcRelKind Declares               = new("IfcRelDeclares",                     "RelatingContext",           "RelatedDefinitions",          Passthrough);
    public static readonly IfcRelKind ServicesBuildings      = new("IfcRelServicesBuildings",            "RelatingSystem",            "RelatedBuildings",            Passthrough);

    public string Relating { get; }
    public string Related { get; }

    [UseDelegateFromConstructor]
    public partial Relationship Lower(WireName wireName, NodeId relating, NodeId related);

    static Relationship Passthrough(WireName wireName, NodeId relating, NodeId related) =>
        new Relationship.Generic(wireName, relating, related, Map<PropertyName, PropertyValue>());

    public WireName Wire => WireName.Create(Key);

    public static Fin<IfcRelKind> Admit(WireName wireName, Op key) =>
        TryGet(wireName.Value, out IfcRelKind? row) && row is { } resolved
            ? Fin.Succ(resolved)
            : Fin.Fail<IfcRelKind>(new BimFault.Refused(key, BimScope.Projection, BimReason.Unmapped, string.Join(':', new object?[] { "rel-row-unbound", wireName.Value })));

    public Relationship Edge(NodeId relating, NodeId related) => Lower(Wire, relating, related);

    static readonly NodeId ProbeRelating = NodeId.Create("00000000000000000000000000000001");
    static readonly NodeId ProbeRelated = NodeId.Create("00000000000000000000000000000002");

    static readonly (FrozenSet<IfcRelKind> Inverted, FrozenDictionary<(RelationshipKind, string), IfcRelKind> ByNeutral) Probed = Probe();

    static (FrozenSet<IfcRelKind>, FrozenDictionary<(RelationshipKind, string), IfcRelKind>) Probe() {
        Seq<(IfcRelKind Row, Relationship Edge)> rows = toSeq(Items).Map(static row => (Row: row, Edge: row.Edge(ProbeRelating, ProbeRelated)));
        return (rows.Filter(static probe => probe.Edge is Relationship.Assign).Map(static probe => probe.Row).ToFrozenSet(),
            rows.Filter(static probe => probe.Row != ConnectsRealizing)
                .Choose(static probe => NeutralKey(probe.Edge).Map(neutral => (Neutral: neutral, probe.Row)))
                .ToFrozenDictionary(static entry => entry.Neutral, static entry => entry.Row));
    }

    public bool Inverted => Probed.Inverted.Contains(this);

    public static Option<IfcRelKind> ForNeutral(RelationshipKind kind, string subKind) =>
        Probed.ByNeutral.TryGetValue((kind, subKind), out IfcRelKind? row) && row is { } resolved ? Some(resolved) : None;

    static Option<(RelationshipKind Kind, string SubKind)> NeutralKey(Relationship edge) =>
        edge.Switch<Option<(RelationshipKind Kind, string SubKind)>>(
            compose:   static c => Some((RelationshipKind.Compose, c.SubKind.Key)),
            assign:    static a => Some((RelationshipKind.Assign, a.SubKind.Key)),
            connect:   static c => Some((RelationshipKind.Connect, c.SubKind.Key)),
            @void:     static v => Some((RelationshipKind.Void, v.SubKind.Key)),
            associate: static _ => Option<(RelationshipKind Kind, string SubKind)>.None,
            generic:   static _ => Option<(RelationshipKind Kind, string SubKind)>.None);

    static readonly Lazy<FrozenDictionary<IfcRelKind, RelSlots>> Census = new(static () =>
        toSeq(Items).Choose(static row => RelSlots.Of(row).Map(slots => (Row: row, Slots: slots)))
             .ToFrozenDictionary(static probe => probe.Row, static probe => probe.Slots));

    public static Fin<RelSlots> SlotsOf(IfcRelKind row, Op key) =>
        Census.Value.TryGetValue(row, out RelSlots? slots) && slots is { } resolved
            ? Fin.Succ(resolved)
            : Fin.Fail<RelSlots>(new BimFault.Refused(key, BimScope.Projection, BimReason.Unmapped, string.Join(':', new object?[] { "rel-row-unbound", row.Key })));

    public Fin<IfcRelationship> Author(DatabaseIfc db, IfcObjectDefinition relating, Seq<IfcObjectDefinition> related, Op key, Option<string> refined = default) =>
        related.IsEmpty
            ? Fin.Fail<IfcRelationship>(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "relation-related-empty", Key })))
            : from slots in SlotsOf(this, key)
              from rel in Optional(db.Factory.Construct(refined.IfNone(Key)) as IfcRelationship)
                  .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "relation-unconstructible", refined.IfNone(Key) })))
              select Filled(rel, slots, relating, related);

    static IfcRelationship Filled(IfcRelationship rel, RelSlots slots, IfcObjectDefinition relating, Seq<IfcObjectDefinition> related) {
        slots.Relating.SetValue(rel, relating);
        slots.Add.Match(
            Some: add => related.Iter(member => add.Invoke(slots.Related.GetValue(rel), [member])),
            None: () => slots.Related.SetValue(rel, related[0]));
        return rel;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EdgeProjection {
    public static WriterT<FidelityLog, Fin, Seq<Relationship>> All(
        IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScheme scale,
        Option<EurocodePolicy> eurocode, TemplateScope templates, IIfcProfileStore profiles, Op key) =>
        from rows in Fidelity.Lift((
            Decomposition(project, rooted, key)
                .Concat(Connections(project, rooted, profiles, key))
                .Concat(Generics(project, rooted, key))
                .Concat(DefinesProperties(project, rooted, key))
                .Concat(Structural(project, rooted, scale, eurocode, profiles, key))
                .Concat(SpatialBoundaries(project, rooted, profiles, key))
                .Traverse(identity).As().Map(static groups => toSeq(groups.Flatten()))).ToFin())
        from factors in GroupFactors(project)
        from materials in MaterialEdges(project, rooted, tolerance, scale, templates, profiles, key)
        select rows + materials;

    static WriterT<FidelityLog, Fin, Unit> GroupFactors(IfcProject project) =>
        toSeq(project.Extract<IfcRelAssignsToGroupByFactor>().AsIterable())
            .TraverseM(static rel => Fidelity.Drop(FidelityDrop.GroupFactor, Anchor(rel.RelatingGroup), unit)).As()
            .Map(static _ => unit);

    static string Anchor(IfcRoot? entity) => Optional(entity).Map(static root => root.GlobalId).IfNone("");

    // --- [ROW_FOLD]

    static Validation<Error, Seq<Relationship>> Rows(IEnumerable<IfcRelationship> rels, IfcRelKind kind, Map<string, NodeId> rooted, Op key) =>
        (IfcRelKind.SlotsOf(kind, key)).ToValidation().Bind(slots =>
            Landed(toSeq(rels).Bind(rel => slots.RelatedIds(rel).Map(related =>
                from source in Endpoint(rooted, slots.RelatingId(rel), kind.Key, kind.Relating, key)
                from target in Endpoint(rooted, related, kind.Key, kind.Related, key)
                select kind.Inverted ? kind.Edge(target, source) : kind.Edge(source, target)))));

    static Validation<Error, Seq<Relationship>> Landed(Seq<Validation<Error, Relationship>> rows) => rows.Traverse(identity).As();

    static Validation<Error, NodeId> Endpoint(Map<string, NodeId> rooted, Option<string> globalId, string relationship, string attribute, Op key) =>
        globalId
            .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Rejected, string.Join(':', new object?[] { "edge-endpoint-absent", relationship, attribute })))
            .Bind(id => rooted.Find(id).ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.DanglingReference, string.Join(':', new object?[] { "edge-endpoint-miss", relationship, attribute, id }))))
            .ToValidation();

    // --- [ARM_GROUPS]

    static Seq<Validation<Error, Seq<Relationship>>> Decomposition(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        Rows(project.Extract<IfcRelAggregates>(), IfcRelKind.Aggregates, rooted, key),
        Nests(project, rooted, key),
        Rows(project.Extract<IfcRelContainedInSpatialStructure>(), IfcRelKind.ContainedInStructure, rooted, key),
        Rows(project.Extract<IfcRelReferencedInSpatialStructure>(), IfcRelKind.ReferencedInStructure, rooted, key),
        Rows(project.Extract<IfcRelVoidsElement>(), IfcRelKind.Voids, rooted, key),
        Rows(project.Extract<IfcRelFillsElement>(), IfcRelKind.Fills, rooted, key),
        Rows(project.Extract<IfcRelProjectsElement>(), IfcRelKind.Projects, rooted, key),
        Rows(project.Extract<IfcRelAdheresToElement>(), IfcRelKind.Adheres, rooted, key));

    static Validation<Error, Seq<Relationship>> Nests(IfcProject project, Map<string, NodeId> rooted, Op key) =>
        Landed(toSeq(project.Extract<IfcRelNests>().AsIterable()
            .GroupBy(static rel => Anchor(rel.RelatingObject))
            .AsIterable()
            .SelectMany(group => group.SelectMany(static rel => rel.RelatedObjects).Select((child, ordinal) =>
                from parent in Endpoint(rooted, Stated(group.Key), IfcRelKind.Nests.Key, IfcRelKind.Nests.Relating, key)
                from part in Endpoint(rooted, Stated(child.GlobalId), IfcRelKind.Nests.Key, IfcRelKind.Nests.Related, key)
                select (Relationship)new Relationship.Generic(IfcRelKind.Nests.Wire, parent, part,
                    Map((SemanticProjector.NestOrdinal, (PropertyValue)new PropertyValue.Integer(ordinal))))))));

    static Option<string> Stated(string? value) => PropertyLowering.Stated(value);

    static Seq<Validation<Error, Seq<Relationship>>> Connections(IfcProject project, Map<string, NodeId> rooted, IIfcProfileStore profiles, Op key) => Seq(
        Landed(toSeq(project.Extract<IfcRelConnectsElements>().AsIterable()
            .Where(static rel => rel is not (IfcRelConnectsWithRealizingElements or IfcRelConnectsPathElements))
            .Select(rel =>
                from a in Endpoint(rooted, Stated(rel.RelatingElement?.GlobalId), IfcRelKind.ConnectsElements.Key, IfcRelKind.ConnectsElements.Relating, key)
                from b in Endpoint(rooted, Stated(rel.RelatedElement?.GlobalId), IfcRelKind.ConnectsElements.Key, IfcRelKind.ConnectsElements.Related, key)
                select (Relationship)new Relationship.Connect(a, b, ConnectKind.Element, Option<NodeId>.None,
                    PreserveInterface(rel.ConnectionGeometry, profiles, key))))),
        Landed(toSeq(project.Extract<IfcRelConnectsPorts>().AsIterable()).Map(rel =>
            from a in Endpoint(rooted, Stated(rel.RelatingPort?.GlobalId), IfcRelKind.ConnectsPorts.Key, IfcRelKind.ConnectsPorts.Relating, key)
            from b in Endpoint(rooted, Stated(rel.RelatedPort?.GlobalId), IfcRelKind.ConnectsPorts.Key, IfcRelKind.ConnectsPorts.Related, key)
            from r in Optional(rel.RealizingElement)
                .TraverseM(element => Endpoint(rooted, Stated(element.GlobalId), IfcRelKind.ConnectsPorts.Key, "RealizingElement", key))
                .As()
            select (Relationship)new Relationship.Connect(a, b, ConnectKind.Port, r, Option<UInt128>.None))),
        Rows(project.Extract<IfcRelConnectsPortToElement>(), IfcRelKind.ConnectsPortToElement, rooted, key),
        Rows(project.Extract<IfcRelConnectsPathElements>(), IfcRelKind.ConnectsPathElements, rooted, key),
        Rows(project.Extract<IfcRelInterferesElements>(), IfcRelKind.InterferesElements, rooted, key),
        Rows(project.Extract<IfcRelSequence>(), IfcRelKind.Sequence, rooted, key),
        Rows(project.Extract<IfcRelFlowControlElements>(), IfcRelKind.FlowControl, rooted, key),
        Rows(project.Extract<IfcRelConnectsStructuralElement>(), IfcRelKind.ConnectsStructElement, rooted, key),
        Realizing(project, rooted, profiles, key));

    static Validation<Error, Seq<Relationship>> Realizing(IfcProject project, Map<string, NodeId> rooted, IIfcProfileStore profiles, Op key) =>
        Landed(toSeq(project.Extract<IfcRelConnectsWithRealizingElements>().AsIterable().SelectMany(rel =>
            rel.RealizingElements.AsIterable().ToSeq() switch {
                { IsEmpty: true } => Seq((Fin.Fail<Relationship>(new BimFault.Refused(key, BimScope.Projection, BimReason.Rejected, string.Join(':', new object?[] { "realizing-elements-empty", rel.GlobalId })))).ToValidation()),
                var members => members.Map(member =>
                    from a in Endpoint(rooted, Stated(rel.RelatingElement?.GlobalId), IfcRelKind.ConnectsRealizing.Key, IfcRelKind.ConnectsRealizing.Relating, key)
                    from b in Endpoint(rooted, Stated(rel.RelatedElement?.GlobalId), IfcRelKind.ConnectsRealizing.Key, IfcRelKind.ConnectsRealizing.Related, key)
                    from r in Endpoint(rooted, Stated(member.GlobalId), IfcRelKind.ConnectsRealizing.Key, "RealizingElements", key)
                    select (Relationship)new Relationship.Connect(a, b, ConnectKind.Element, Some(r),
                        PreserveInterface(rel.ConnectionGeometry, profiles, key))),
            })));

    static Option<UInt128> PreserveInterface(IfcConnectionGeometry? geometry, IIfcProfileStore profiles, Op key) =>
        Optional(geometry).Map(surface => profiles.Preserve(surface, key));

    static Seq<Validation<Error, Seq<Relationship>>> Generics(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        Rows(project.Extract<IfcRelDefinesByType>(), IfcRelKind.DefinesByType, rooted, key),
        Rows(project.Extract<IfcRelAssignsToGroup>(), IfcRelKind.AssignsToGroup, rooted, key),
        Rows(project.Extract<IfcRelCoversBldgElements>(), IfcRelKind.CoversElements, rooted, key),
        Rows(project.Extract<IfcRelCoversSpaces>(), IfcRelKind.CoversSpaces, rooted, key),
        Rows(project.Extract<IfcRelAssignsToControl>(), IfcRelKind.AssignsToControl, rooted, key),
        Rows(project.Extract<IfcRelAssignsToProcess>(), IfcRelKind.AssignsToProcess, rooted, key),
        Rows(project.Extract<IfcRelAssignsToProduct>(), IfcRelKind.AssignsToProduct, rooted, key),
        Rows(project.Extract<IfcRelAssignsToActor>(), IfcRelKind.AssignsToActor, rooted, key),
        Rows(project.Extract<IfcRelDeclares>(), IfcRelKind.Declares, rooted, key),
        Rows(project.Extract<IfcRelServicesBuildings>(), IfcRelKind.ServicesBuildings, rooted, key),
        Rows(project.Extract<IfcRelPositions>(), IfcRelKind.Positions, rooted, key),
        Rows(project.Extract<IfcRelDefinesByObject>(), IfcRelKind.DefinesByObject, rooted, key));

    static Seq<Validation<Error, Seq<Relationship>>> DefinesProperties(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        Landed(toSeq(project.Extract<IfcRelDefinesByProperties>().AsIterable()
            .SelectMany(rel => rel.RelatedObjects.SelectMany(o => rel.RelatingPropertyDefinition.Select(definition =>
                from subject in Endpoint(rooted, Stated(o.GlobalId), IfcRelKind.DefinesByType.Key, "RelatedObjects", key)
                from bag in Endpoint(rooted, Stated(definition.GlobalId), IfcRelKind.DefinesByType.Key, "RelatingPropertyDefinition", key)
                select (Relationship)new Relationship.Assign(subject, bag, AssignKind.PropertyDefinition)))))));

    static Seq<Validation<Error, Seq<Relationship>>> Structural(
        IfcProject project, Map<string, NodeId> rooted, UnitScheme scale, Option<EurocodePolicy> eurocode, IIfcProfileStore profiles, Op key) => Seq(
        Landed(toSeq(project.Extract<IfcRelConnectsStructuralMember>().AsIterable().Select(rel =>
            from m in Endpoint(rooted, Stated(rel.RelatingStructuralMember?.GlobalId), IfcRelKind.ConnectsStructMember.Key, IfcRelKind.ConnectsStructMember.Relating, key)
            from c in Endpoint(rooted, Stated(rel.RelatedStructuralConnection?.GlobalId), IfcRelKind.ConnectsStructMember.Key, IfcRelKind.ConnectsStructMember.Related, key)
            from attrs in (StructuralProjection.Attrs(rel, scale, eurocode, profiles, key)).ToValidation()
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructMember.Wire, m, c, attrs)))),
        Landed(toSeq(project.Extract<IfcRelConnectsStructuralActivity>().AsIterable().Select(rel =>
            from item in Endpoint(rooted, Stated((rel.RelatingElement as IfcRoot)?.GlobalId), IfcRelKind.ConnectsStructActivity.Key, IfcRelKind.ConnectsStructActivity.Relating, key)
            from act in Endpoint(rooted, Stated(rel.RelatedStructuralActivity?.GlobalId), IfcRelKind.ConnectsStructActivity.Key, IfcRelKind.ConnectsStructActivity.Related, key)
            from attrs in (StructuralProjection.Attrs(rel, scale, eurocode, profiles, key)).ToValidation()
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructActivity.Wire, item, act, attrs)))));

    static Seq<Validation<Error, Seq<Relationship>>> SpatialBoundaries(IfcProject project, Map<string, NodeId> rooted, IIfcProfileStore profiles, Op key) => Seq(
        Landed(toSeq(project.Extract<IfcRelSpaceBoundary>().AsIterable().Select(rel =>
            from s in Endpoint(rooted, Stated((rel.RelatingSpace as IfcRoot)?.GlobalId), IfcRelKind.SpaceBoundary.Key, IfcRelKind.SpaceBoundary.Relating, key)
            from e in Endpoint(rooted, Stated(rel.RelatedBuildingElement?.GlobalId), IfcRelKind.SpaceBoundary.Key, IfcRelKind.SpaceBoundary.Related, key)
            select (Relationship)new Relationship.Generic(IfcRelKind.SpaceBoundary.Wire, s, e, BoundaryAttrs(rel, profiles, key))))));

    static Map<PropertyName, PropertyValue> BoundaryAttrs(IfcRelSpaceBoundary rel, IIfcProfileStore profiles, Op key) =>
        PreserveInterface(rel.ConnectionGeometry, profiles, key).Fold(
            Map((BoundaryRows.BoundaryLevel, (PropertyValue)new PropertyValue.Text(BoundaryLevelOf(rel)))),
            static (attrs, surface) => attrs.Add(SemanticProjector.InterfaceKey, new PropertyValue.Text(surface.ToString("X32"))));

    static string BoundaryLevelOf(IfcRelSpaceBoundary rel) =>
        rel is IfcRelSpaceBoundary2ndLevel ? "2nd" : rel is IfcRelSpaceBoundary1stLevel ? "1st" : "";

    // --- [MATERIAL_ARM]

    static WriterT<FidelityLog, Fin, Seq<Relationship>> MaterialEdges(
        IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScheme scale, TemplateScope templates,
        IIfcProfileStore profiles, Op key) =>
        toSeq(project.Extract<IfcRelAssociatesMaterial>().AsIterable())
            .Traverse(rel =>
                from relating in Fidelity.Lift(Optional(rel.RelatingMaterial).ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Rejected, string.Join(':', new object?[] { "material-relation-unbound", rel.GlobalId }))))
                from material in Fidelity.Lift(MaterialProjection.Project(relating, tolerance, profiles, scale, key).Map(static node => node.Id))
                from usage in Fidelity.Lift(UsageOf(relating, scale, key))
                from bags in SemanticProjector.DefinitionOf(relating)
                    .Map(definition => Fidelity.Lift(MaterialProjection.ImportedPsets(definition, rooted, scale, templates, key))
                        .Bind(narrowed => Fidelity.Told(narrowed.Log, narrowed.Value)))
                    .IfNone(Fidelity.Clean(Seq<PropertyBag>()))
                from elements in Fidelity.Lift((toSeq(rel.RelatedObjects.AsIterable())
                    .Map(o => Endpoint(rooted, Stated((o as IfcRoot)?.GlobalId), nameof(IfcRelAssociatesMaterial), "RelatedObjects", key))
                    .Traverse(identity).As()).ToFin())
                select elements.Map(element => (Relationship)new Relationship.Associate(element, material, usage))
                    .Concat(bags.Map(bag => (Relationship)new Relationship.Assign(
                        material, SemanticProjector.PropertySetNode(bag, tolerance).Id, AssignKind.PropertyDefinition))).ToSeq())
            .As()
            .Map(static rows => toSeq(rows.Flatten()));

    // --- [USAGE_ADMISSION]

    static Fin<MaterialUsage> UsageOf(IfcMaterialSelect select, UnitScheme scale, Op key) => select switch {
        IfcMaterialLayerSetUsage u =>
            from direction in Elected(LayerAxes, u.LayerSetDirection, key)
            from sense in Elected(LayerSenses, u.DirectionSense, key)
            from offset in Length(u.OffsetFromReferenceLine, scale, key)
            from extent in Length(u.ReferenceExtent, scale, key)
            from usage in MaterialUsage.LayerSet.Of(direction, sense, offset, extent, key)
            select usage,
        IfcMaterialProfileSetUsage u =>
            from extent in Length(u.ReferenceExtent, scale, key)
            from usage in MaterialUsage.ProfileSet.Of(OptionalCardinal(u.CardinalPoint), extent, key)
            select usage,
        _ => Fin.Succ<MaterialUsage>(new MaterialUsage.Unbound()),
    };

    static readonly FrozenDictionary<IfcLayerSetDirectionEnum, LayerSetDirection> LayerAxes =
        new Dictionary<IfcLayerSetDirectionEnum, LayerSetDirection> {
            [IfcLayerSetDirectionEnum.AXIS1] = LayerSetDirection.Axis1,
            [IfcLayerSetDirectionEnum.AXIS2] = LayerSetDirection.Axis2,
            [IfcLayerSetDirectionEnum.AXIS3] = LayerSetDirection.Axis3,
        }.ToFrozenDictionary();

    static readonly FrozenDictionary<IfcDirectionSenseEnum, DirectionSense> LayerSenses =
        new Dictionary<IfcDirectionSenseEnum, DirectionSense> {
            [IfcDirectionSenseEnum.POSITIVE] = DirectionSense.Positive,
            [IfcDirectionSenseEnum.NEGATIVE] = DirectionSense.Negative,
        }.ToFrozenDictionary();

    static Fin<TRow> Elected<TMember, TRow>(FrozenDictionary<TMember, TRow> rows, TMember member, Op key) where TMember : notnull =>
        rows.TryGetValue(member, out TRow? row) && row is { } elected
            ? Fin.Succ(elected)
            : Fin.Fail<TRow>(new BimFault.Refused(key, BimScope.Projection, BimReason.Unmapped, string.Join(':', new object?[] { "material-usage-axis-unmapped", typeof(TMember).Name, $"{member}" })));

    static Fin<Option<MeasureValue>> Length(double native, UnitScheme scale, Op key) =>
        double.IsFinite(native)
            ? MeasureValue.OfSi(Dimension.LengthDim, scale.Coerce(native, QuantityType.Length, Dimension.LengthDim), key).Map(Some)
            : Fin.Succ(Option<MeasureValue>.None);

    static Option<int> OptionalCardinal(IfcCardinalPointReference point) =>
        point == IfcCardinalPointReference.MID ? Option<int>.None : Some((int)point);
}
```

## [03]-[RESEARCH]

(none)

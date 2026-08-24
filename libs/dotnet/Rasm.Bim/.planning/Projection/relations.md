# [BIM_RELATION_ALGEBRA]

The IFC relationship vocabulary `Rasm.Bim` owns as the SOLE GeometryGym/IFC owner: the `IfcRelKind` `[SmartEnum<string>]` roster over the full concrete node-to-node `IfcRelationship` subtree — each row carrying the IFC relating/related inverse-attribute names and the neutral edge constructor it lowers through — and the `EdgeProjection` fold landing every relationship family on a NEUTRAL `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship` edge.

This page is the relationship half of the `Projection/semantic#SEMANTIC_PROJECTOR` ingress, and the SAME roster reverses at the `Projection/egress#IFC_EGRESS` re-author through `ForNeutral` and `Author`.

GeometryGym leaks no relationship case below the seam: the typed case carries only its `SubKind`, the IFC wire-name and inverse living on the ROW, and only `Generic` carries a wire-name and a per-edge attribute payload [NEUTRAL_EDGE_RULING].

The roster is the ADMISSION for that wire-name — a `WireName` mints from a row key and nowhere else — so a name no row declares cannot be constructed on the producing side.

## [01]-[INDEX]

- [02]-[RELATION_ALGEBRA]: `IfcRelKind` the full `IfcRel*` roster and its wire-name admission, `RelSlots` the reflected fill-and-read surface the census resolves once, and the `EdgeProjection` fold — the row-driven generic families, the ordinal-carrying nests, the realizing fan, the property attachment, and the structural/space-boundary/material payload arms.

## [02]-[RELATION_ALGEBRA]

- Owner: `IfcRelKind` the `[SmartEnum<string>]` roster keyed on the IFC relationship entity name, each row carrying its relating/related inverse-attribute names and its `[UseDelegateFromConstructor]` `Lower` arm; `RelSlots` the reflected relating/related/`Add` triple ONE census resolves per row, read by BOTH the ingest fold and the egress author; `EdgeProjection` the static fold lowering every family onto neutral edges.
- Cases: the typed families `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelReferencedInSpatialStructure` (`Compose`), `IfcRelConnectsElements`/`IfcRelConnectsWithRealizingElements`/`IfcRelConnectsPorts` (`Connect`), `IfcRelDefinesByType`/`IfcRelAssignsToGroup` (`Assign`), `IfcRelVoidsElement`/`IfcRelFillsElement` (`Void`), and the twenty-one families that lower to `Generic` because no neutral sub-kind fits (the port-to-element and path-element joins, the three structural bindings, the space boundary, interference, sequence, the two covering families, the four non-group assignments, declaration, building service, the additive projection, surface-feature adherence, flow control, positioning, and declaring-object typing). `IfcRelAssociatesMaterial`, `IfcRelDefinesByProperties`, the structural/space-boundary payloads, and the two realizer-carrying connects (`RealizingElements` set fan-out; the ports join's optional `RealizingElement`) ride DEDICATED folds, not the generic path.
- Law: the row's `Lower` arm is the SINGLE owner of the row's neutral identity — the lowering axis, the neutral `SubKind`, and the directionality inversion all READ OFF it through one sentinel probe, so no Bim-side axis enum stands beside the seam's own `RelationshipKind` and no hand-kept `Inverted` column can drift from the arm that built the edge.
- Law: the wire name is the ROW KEY, supplied at the ONE lowering site rather than repeated inside each passthrough arm. Twenty-one delegate literals restated column one of their own row and a census arm existed solely to prove they matched; the key supplied here makes that verdict unrepresentable rather than checked.
- Law: a census hit proves CALLABILITY, never a name — a row's related slot resolves the exact `Add(memberType)` overload the fan-out invokes when the slot is a SET and its setter when it is single-valued, and the census, the ingest READ, and the egress FILL are ONE resolution. A name-only `GetMethod("Add")` matches several arities and throws the ambiguous match before naming which one it meant.
- Law: an ABSENT endpoint attribute and an UNROOTED one are two verdicts. GeometryGym backs a missing endpoint with null; coalescing that to an empty string handed a malformed file and a projection gap to one fault whose detail was empty, and a federation manager reads no difference between a broken source and a missed projection.
- Entry: `EdgeProjection.All(project, rooted, tolerance, scale, eurocode, templates, profiles, key)` returns `WriterT<FidelityLog, Fin, Seq<Relationship>>` — the fold's own fidelity facts RETURNED beside the edges, never a ledger this page holds; `IfcRelKind.Admit(wireName, key)` re-admits a crossed wire name against the roster; `kind.Author(db, relating, related, key, refined)` is the egress fill.
- Auto: the generic families take ONE row-driven fold reading the census-resolved slots — the relating endpoint through its slot, the related endpoints through theirs (a SET slot yielding its members, a single-valued slot the one), every value admitted through the same `IfcRoot` read, and the row's own derived `Inverted` deciding the seam orientation. The arm groups are INDEPENDENT and ACCUMULATE, so one dangling endpoint no longer hides every other family's rejects. Bespoke arms survive only where a real extra law lives: the ordered nest's running ordinal, the realizing fan-out, the many-to-many property attachment, and the three payload folds.
- Auto: `IfcRelNests.RelatedObjects` is a schema `LIST`, so INGEST routes every nest through `Generic` stamping the egress-declared `SemanticProjector.NestOrdinal` attribute; ordinals are PER-PARENT CONTINUOUS across relations because `IsNestedBy` is a schema SET, so a parent with N nest relations collides per-relation zero-based indices and the egress per-parent merge interleaves nondeterministically. The typed `Compose{Nest}` case and its canonical bytes stay byte-identical for authored graphs.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new IFC relationship is one `IfcRelKind` row — its inverse-attribute names, and a `Lower` arm only where a typed seam case fits — with one dedicated `EdgeProjection` arm where it carries a payload. A relationship to a NON-node IFC resource is a SEPARATE Growth axis: a new seam `Node` case plus its row, never silently in scope of the covered set. A rider subtype's extra payload is either SOLVED or a NAMED bounded drop: the eccentricity, the connection interface, and the space-boundary level are solved, while `IfcRelAssignsToGroupByFactor.Factor` and the `IfcRelOverridesProperties` override semantics stay counted drops on the `NestOrdinal` precedent.
- Boundary: the seam `Relationship` is the NEUTRAL edge algebra plus `Generic` — re-introducing typed `IfcRel*` cases is the deleted form [NEUTRAL_EDGE_RULING]; the IFC names, directionality, and inverse live HERE, reconstructing at egress through the reverse index. The material occurrence-usage rides the `Associate` edge's typed `MaterialUsage` payload [OCCURRENCE_USAGE_RULING] and a parallel usage node is deleted; the structural and space-boundary connectivity ride the NEUTRAL `Generic` wire-name payload, so a space boundary's interface surface rides the `InterfaceKey` ATTR while an element connect's rides the typed `Connect.Interface` SLOT — the seam `ConnectKind` medium vocabulary is closed at element/path/port and a space-to-surface boundary is none of the three, so a fourth medium row minted to reach the typed slot is the deleted phantom. Both keys name the same preserved STEP fragment in the one store. The census verdict rides the `Fin` rail BOTH entrypoints already return, so a pin bump that renames an attribute, narrows a SET member type, or seals a setter refuses at the first roster touch with a rostered token — where a type-initializer throw died in a frame no caller reads and no diagnostic vocabulary owns.
- Boundary: the wire-name roster is the producer-side ADMISSION for `Relationship.Generic.WireName` and the peer decoders still type it as an unvalidated string — `tests/contracts/proto/rasm/contracts/element/graph.proto` `wire_name` and the TypeScript codec's `NonEmptyString` — so an unrostered name crossing INTO this runtime is refused only when `Admit` is composed at the decode edge. Both decoders now RE-QUOTE that admission at their own field — the proto `wire_name` comment and the TypeScript codec's predicate and edge landings each state the producer's roster as the gate and name `IfcRelKind.Admit` as the refusal on re-entry — so the open column is the wire's own enumeration, which stays unpublished DELIBERATELY: freezing thirty-two keys into the schema forks the roster the moment a row lands, where a re-quote keeps one authority and costs the peers a decode-edge call.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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

// --- [MODELS] -----------------------------------------------------------------------------
// The reflected surface of ONE IfcRelKind row: the relating slot, the related slot, and — when the related slot is
// a SET — the EXACT Add(memberType) overload the egress fill invokes. Of resolves all three off the DECLARED
// property types and answers None on any miss, so the census verdict, the ingest READ, and the egress FILL are one
// resolution and a census hit proves a call both directions can actually make.
sealed record RelSlots(PropertyInfo Relating, PropertyInfo Related, Option<MethodInfo> Add) {
    public static Option<RelSlots> Of(IfcRelKind row) =>
        from shape in Optional(typeof(IfcRelationship).Assembly.GetType($"{typeof(IfcRelationship).Namespace}.{row.Key}"))
        from relating in Optional(shape.GetProperty(row.Relating)).Filter(static slot => slot.CanWrite)
        from related in Optional(shape.GetProperty(row.Related))
        from resolved in Adder(related.PropertyType) is { IsSome: true } add ? Some(new RelSlots(relating, related, add))
            : related.CanWrite ? Some(new RelSlots(relating, related, Option<MethodInfo>.None))
            : Option<RelSlots>.None
        select resolved;

    // The relating endpoint read through the SAME slot the author fills. Every relating attribute is an IfcRoot or
    // a SELECT interface over one, so ONE cast serves all thirty-two rows where the hand accessors needed eight
    // separate `as IfcRoot` spellings and thirty-two `?.GlobalId` reads.
    public Option<string> RelatingId(IfcRelationship rel) => Rooted(Relating.GetValue(rel));

    // The related endpoints: a SET-valued slot yields its members, a single-valued slot the one. Add already
    // answered which at census time, so ARITY is a row fact rather than a caller's choice between two helpers that
    // differed in nothing else.
    public Seq<Option<string>> RelatedIds(IfcRelationship rel) =>
        Add.IsSome
            ? toSeq(Optional(Related.GetValue(rel) as IEnumerable)
                .Map(static members => toSeq(members.Cast<object?>())).IfNone(Seq<object?>())).Map(Rooted)
            : Seq(Rooted(Related.GetValue(rel)));

    // GeometryGym backs a missing endpoint with null, and every earlier read coalesced it to "". None keeps
    // absence and presence apart so the ONE admission below can give them two faults.
    static Option<string> Rooted(object? value) => Optional(value as IfcRoot).Map(static root => root.GlobalId);

    // A SET-valued slot is a GeometryGym SET<T> — an ICollection<T>, NEVER a System.Collections.IList — so the fill
    // resolves the TYPED Add(T) overload off the collection interface's own argument. The name-only
    // GetMethod("Add") this replaces matches several arities and throws AmbiguousMatchException before naming which
    // one it meant, so a row passed a census whose Add the author then failed to invoke.
    static Option<MethodInfo> Adder(Type slotType) =>
        Optional(slotType.GetInterfaces().FirstOrDefault(static face =>
                face.IsGenericType && face.GetGenericTypeDefinition() == typeof(ICollection<>)))
            .Bind(face => Optional(slotType.GetMethod(nameof(ICollection<object>.Add), [face.GetGenericArguments()[0]])));
}

// --- [TYPES] ------------------------------------------------------------------------------
// No axis enum stands beside the seam's own RelationshipKind: a Bim-side copy of the seam union's discriminant is
// the parallel-discriminant form the seam accessor law already deleted, and the row's lowering axis, sub-kind, and
// inversion all read off the ONE arm below.
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
    // GG deviates from the schema attribute pair here (no RelatingFlowElement/RelatedControlElements on the
    // public surface) — the decompiled members ARE RelatingPort/RelatedElement, so the row records the REAL wire.
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

    // The row's neutral lowering: the axis it folds onto and the SubKind the typed case takes are spelled once,
    // HERE, in the arm that builds the edge — never a column a separate dispatch has to keep in step with. Every
    // Connect arm spells the seam's full five-argument arity with both Options None, so no call site reads a
    // defaulted slot; a payload-bearing family rides a dedicated arm and never this one.
    [UseDelegateFromConstructor]
    public partial Relationship Lower(WireName wireName, NodeId relating, NodeId related);

    static Relationship Passthrough(WireName wireName, NodeId relating, NodeId related) =>
        new Relationship.Generic(wireName, relating, related, Map<PropertyName, PropertyValue>());

    // The wire-name ADMISSION: a Generic edge's name is a ROSTER KEY, minted here and nowhere else, so a name no
    // row declares cannot be constructed on the producing side. Every Generic construction in this folder — the
    // row lowering, the ordered nest, the structural and space-boundary arms — reads its name off this member.
    public WireName Wire => WireName.Create(Key);

    public static Fin<IfcRelKind> Admit(WireName wireName, Op key) =>
        TryGet(wireName.Value, out IfcRelKind? row) && row is { } resolved
            ? Fin.Succ(resolved)
            : Fin.Fail<IfcRelKind>(new BimFault.Refused(key, BimScope.Projection, BimReason.Unmapped, string.Join(':', new object?[] { "rel-row-unbound", wireName.Value })));

    // The row's own key IS the wire name, supplied at this ONE call site.
    public Relationship Edge(NodeId relating, NodeId related) => Lower(Wire, relating, related);

    // The two sentinel endpoints every derivation below probes a row's own arm with. They never reach a graph:
    // only the constructed case's discriminants are read, so the arm stays the ONE owner of the row's identity.
    static readonly NodeId ProbeRelating = NodeId.Create("00000000000000000000000000000001");
    static readonly NodeId ProbeRelated = NodeId.Create("00000000000000000000000000000002");

    // ONE probe pass over the roster yielding BOTH edge-derived answers, run eagerly because the pass is total:
    // invoking a row's own arm cannot fail. The two Lazy<> that each re-walked the roster after this one ran read
    // the same delegate for the same reason and are gone.
    static readonly (FrozenSet<IfcRelKind> Inverted, FrozenDictionary<(RelationshipKind, string), IfcRelKind> ByNeutral) Probed = Probe();

    static (FrozenSet<IfcRelKind>, FrozenDictionary<(RelationshipKind, string), IfcRelKind>) Probe() {
        Seq<(IfcRelKind Row, Relationship Edge)> rows = Items.AsIterable().ToSeq().Map(static row => (Row: row, Edge: row.Edge(ProbeRelating, ProbeRelated)));
        return (rows.Filter(static probe => probe.Edge is Relationship.Assign).Map(static probe => probe.Row).ToFrozenSet(),
            // The direct ToFrozenDictionary IS the uniqueness gate — a colliding (kind, sub-kind) pair fails at
            // type initialization (the FaultBand registry law), never a GroupBy/First mask electing a winner.
            rows.Filter(static probe => probe.Row != ConnectsRealizing)
                .Choose(static probe => NeutralKey(probe.Edge).Map(neutral => (Neutral: neutral, probe.Row)))
                .ToFrozenDictionary(static entry => entry.Neutral, static entry => entry.Row));
    }

    // Directionality inversion is DERIVED from the row's own arm, never a hand-kept column: the seam Assign reads
    // Subject(occurrence)->Definition(type/group) while every IFC assign relation reads
    // relating(definition)->related(occurrences), so an Assign-producing row is inverted BY CONSTRUCTION and every
    // other row already reads in IFC orientation. Both the ingest fold and the egress author re-orient on this.
    public bool Inverted => Probed.Inverted.Contains(this);

    // The egress reverse index, keyed on the SEAM's own (RelationshipKind, sub-kind) discriminant so Lower and
    // ForNeutral cannot drift. A Generic row yields no sub-kind and stays out (it resolves by wire-name through
    // Admit), as does ConnectsRealizing: it shares the Connect "element" sub-kind with ConnectsElements because
    // realization is the seam Connect.Realizing FIELD, never a sub-kind row.
    public static Option<IfcRelKind> ForNeutral(RelationshipKind kind, string subKind) =>
        Probed.ByNeutral.TryGetValue((kind, subKind), out IfcRelKind? row) && row is { } resolved ? Some(resolved) : None;

    // The generated TOTAL Switch over the seam union: a new seam edge case breaks the reverse index at COMPILE
    // time rather than falling into a `_ => None` that silently dropped its whole family from the egress.
    static Option<(RelationshipKind Kind, string SubKind)> NeutralKey(Relationship edge) =>
        edge.Switch<Option<(RelationshipKind Kind, string SubKind)>>(
            compose:   static c => Some((RelationshipKind.Compose, c.SubKind.Key)),
            assign:    static a => Some((RelationshipKind.Assign, a.SubKind.Key)),
            connect:   static c => Some((RelationshipKind.Connect, c.SubKind.Key)),
            @void:     static v => Some((RelationshipKind.Void, v.SubKind.Key)),
            associate: static _ => Option<(RelationshipKind Kind, string SubKind)>.None,
            generic:   static _ => Option<(RelationshipKind Kind, string SubKind)>.None);

    // Author registers the entity through Construct BEFORE either side binds, so a slot miss cannot be undone at the
    // call site: the writer serializes the half-bound IfcRel* regardless, and a schema-invalid relationship in a
    // delivered file is indistinguishable from an authoring choice. The FlowControl row proves a pin can move a pair.
    static readonly Lazy<FrozenDictionary<IfcRelKind, RelSlots>> Census = new(static () =>
        Items.AsIterable().Choose(static row => RelSlots.Of(row).Map(slots => (Row: row, Slots: slots)))
             .ToFrozenDictionary(static probe => probe.Row, static probe => probe.Slots));

    public static Fin<RelSlots> SlotsOf(IfcRelKind row, Op key) =>
        Census.Value.TryGetValue(row, out RelSlots? slots) && slots is { } resolved
            ? Fin.Succ(resolved)
            : Fin.Fail<RelSlots>(new BimFault.Refused(key, BimScope.Projection, BimReason.Unmapped, string.Join(':', new object?[] { "rel-row-unbound", row.Key })));

    // The EGRESS author; the caller feeds endpoints already in IFC orientation. Fin, never Option: an empty related
    // set, an unconstructible class, and an unbound row are three distinct authoring failures, where the silent None
    // left the caller unable to say whether a relationship was written at all. Endpoints are IfcObjectDefinition-wide
    // — a DefinesByType relating side is an IfcTypeObject and an AssignsToGroup relating side an IfcGroup, neither an
    // IfcProduct. The refined slot is the SUBTYPE-construct discriminant an edge attr carries and the row cannot; a
    // rider subtype shares its base's relating/related pair, so the row's names still fill the endpoints.
    public Fin<IfcRelationship> Author(DatabaseIfc db, IfcObjectDefinition relating, Seq<IfcObjectDefinition> related, Op key, Option<string> refined = default) =>
        related.IsEmpty
            ? Fin.Fail<IfcRelationship>(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "relation-related-empty", Key })))
            : from slots in SlotsOf(this, key)
              from rel in Optional(db.Factory.Construct(refined.IfNone(Key)) as IfcRelationship)
                  .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "relation-unconstructible", refined.IfNone(Key) })))
              select Filled(rel, slots, relating, related);

    // A SET-valued related side fills member-by-member through the census-resolved Add(memberType); a
    // single-valued side takes the head through its setter. Insertion order is the fill order, which is what makes
    // the egress ordered-nest ordinal sort survive into IfcRelNests.RelatedObjects.
    static IfcRelationship Filled(IfcRelationship rel, RelSlots slots, IfcObjectDefinition relating, Seq<IfcObjectDefinition> related) {
        slots.Relating.SetValue(rel, relating);
        slots.Add.Match(
            Some: add => related.Iter(member => add.Invoke(slots.Related.GetValue(rel), [member])),
            None: () => slots.Related.SetValue(rel, related[0]));
        return rel;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class EdgeProjection {
    // One fold over the whole roster: the row-driven generic families, the ordered nests, the realizing fan, and
    // the dedicated payload folds. The fold RETURNS its fidelity contribution on the writer carrier — the
    // group-factor riders as a pure tell, the material bag lowering as its own returned log — so no arm holds a
    // ledger and the ingest half lands one log at the SEMANTIC_PROJECTOR fold edge.
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
                .Traverse(identity).As().Map(static groups => groups.Flatten().ToSeq())).ToFin())
        from factors in GroupFactors(project)
        from materials in MaterialEdges(project, rooted, tolerance, scale, templates, profiles, key)
        select rows + materials;

    // The group-factor riders as a pure TELL over the same subtype the assignment arm already lands: the ByFactor
    // Factor has no seam slot on the typed Assign case, so the membership edge lands whole and the rider is a
    // returned fact on the NestOrdinal precedent, never silent.
    static WriterT<FidelityLog, Fin, Unit> GroupFactors(IfcProject project) =>
        toSeq(project.Extract<IfcRelAssignsToGroupByFactor>().AsIterable())
            .TraverseM(static rel => Fidelity.Drop(FidelityDrop.GroupFactor, Anchor(rel.RelatingGroup), unit)).As()
            .Map(static _ => unit);

    static string Anchor(IfcRoot? entity) => Optional(entity).Map(static root => root.GlobalId).IfNone("");

    // --- [ROW_FOLD]

    // The ONE row-driven fold every generic family takes: the row's census-resolved slots read BOTH endpoints off
    // the entity, arity is the ROW's (a SET-valued related slot fans one edge per member), and the row's derived
    // Inverted decides the seam orientation.
    static Validation<Error, Seq<Relationship>> Rows(IEnumerable<IfcRelationship> rels, IfcRelKind kind, Map<string, NodeId> rooted, Op key) =>
        (IfcRelKind.SlotsOf(kind, key)).ToValidation().Bind(slots =>
            Landed(toSeq(rels).Bind(rel => slots.RelatedIds(rel).Map(related =>
                from source in Endpoint(rooted, slots.RelatingId(rel), kind.Key, kind.Relating, key)
                from target in Endpoint(rooted, related, kind.Key, kind.Related, key)
                select kind.Inverted ? kind.Edge(target, source) : kind.Edge(source, target)))));

    // The ONE arm tail: independent rows accumulate into one group verdict.
    static Validation<Error, Seq<Relationship>> Landed(Seq<Validation<Error, Relationship>> rows) => rows.Traverse(identity).As();

    // The endpoint admission, TWO verdicts: an ABSENT attribute is a malformed file (GG backs a missing mandatory
    // endpoint with null) and an UNROOTED GlobalId is a projection gap. The anchor names the relationship and the
    // attribute, so a fault says WHICH endpoint of WHICH relationship failed rather than carrying an empty detail.
    static Validation<Error, NodeId> Endpoint(Map<string, NodeId> rooted, Option<string> globalId, string relationship, string attribute, Op key) =>
        (globalId.Match(
            None: () => Fin.Fail<NodeId>(new BimFault.Refused(key, BimScope.Projection, BimReason.Rejected, string.Join(':', new object?[] { "edge-endpoint-absent", relationship, attribute }))),
            Some: id => rooted.Find(id).ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.DanglingReference, string.Join(':', new object?[] { "edge-endpoint-miss", relationship, attribute, id }))))).ToValidation();

    // --- [ARM_GROUPS]

    static Seq<Validation<Error, Seq<Relationship>>> Decomposition(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        Rows(project.Extract<IfcRelAggregates>(), IfcRelKind.Aggregates, rooted, key),
        Nests(project, rooted, key),
        Rows(project.Extract<IfcRelContainedInSpatialStructure>(), IfcRelKind.ContainedInStructure, rooted, key),
        Rows(project.Extract<IfcRelReferencedInSpatialStructure>(), IfcRelKind.ReferencedInStructure, rooted, key),
        Rows(project.Extract<IfcRelVoidsElement>(), IfcRelKind.Voids, rooted, key),
        Rows(project.Extract<IfcRelFillsElement>(), IfcRelKind.Fills, rooted, key),
        // The feature-attachment tail of IfcRelDecomposes: the additive projection (the Voids counterpart) and the
        // IFC4.3 surface-feature adherence — both Generic rows, neither a new seam case.
        Rows(project.Extract<IfcRelProjectsElement>(), IfcRelKind.Projects, rooted, key),
        Rows(project.Extract<IfcRelAdheresToElement>(), IfcRelKind.Adheres, rooted, key));

    // The running index is PER-PARENT and continuous ACROSS relations, so the egress per-parent merge is total; the
    // typed Compose{Nest} case keeps its (Compose,"nest") reverse-index row for authored graphs.
    static Validation<Error, Seq<Relationship>> Nests(IfcProject project, Map<string, NodeId> rooted, Op key) =>
        Landed(toSeq(project.Extract<IfcRelNests>().AsIterable()
            .GroupBy(static rel => Anchor(rel.RelatingObject))
            .AsIterable()
            .SelectMany(group => group.SelectMany(static rel => rel.RelatedObjects).Select((child, ordinal) =>
                from parent in Endpoint(rooted, Stated(group.Key), IfcRelKind.Nests.Key, IfcRelKind.Nests.Relating, key)
                from part in Endpoint(rooted, Stated(child.GlobalId), IfcRelKind.Nests.Key, IfcRelKind.Nests.Related, key)
                select (Relationship)new Relationship.Generic(IfcRelKind.Nests.Wire, parent, part,
                    Map((SemanticProjector.NestOrdinal, (PropertyValue)new PropertyValue.Integer(ordinal))))))));

    // The ONE GG string admission on this page, composed from the Projection/value#PROPERTY_LOWERING owner: GG
    // backs an optional string with an EMPTY default rather than a null, so blank IS absence and lifts to None
    // here — a second admission spelling would put the same decision at a second site.
    static Option<string> Stated(string? value) => PropertyLowering.Stated(value);

    static Seq<Validation<Error, Seq<Relationship>>> Connections(IfcProject project, Map<string, NodeId> rooted, IIfcProfileStore profiles, Op key) => Seq(
        // Extract<IfcRelConnectsElements> returns its subclasses too — the realizing and path-element joins are
        // handled by their own arms — so they are excluded here. The arm leaves the row fold because the base
        // carries the OPTIONAL ConnectionGeometry, the joint's physical interface surface, which PreserveInterface
        // content-keys onto the seam Connect.Interface slot.
        Landed(toSeq(project.Extract<IfcRelConnectsElements>().AsIterable()
            .Where(static rel => rel is not (IfcRelConnectsWithRealizingElements or IfcRelConnectsPathElements))
            .Select(rel =>
                from a in Endpoint(rooted, Stated(rel.RelatingElement?.GlobalId), IfcRelKind.ConnectsElements.Key, IfcRelKind.ConnectsElements.Relating, key)
                from b in Endpoint(rooted, Stated(rel.RelatedElement?.GlobalId), IfcRelKind.ConnectsElements.Key, IfcRelKind.ConnectsElements.Related, key)
                select (Relationship)new Relationship.Connect(a, b, ConnectKind.Element, Option<NodeId>.None,
                    PreserveInterface(rel.ConnectionGeometry, profiles, key))))),
        // IfcRelConnectsPorts carries an OPTIONAL [0:1] RealizingElement (api-geometrygym-ifc :308) — the port join
        // leaves the row fold so a present realizer lands on the seam Connect.Realizing field; absence is lawful
        // None, a present-but-unrooted realizer is a projection gap and faults typed like any endpoint.
        Landed(toSeq(project.Extract<IfcRelConnectsPorts>().AsIterable()).Map(rel =>
            from a in Endpoint(rooted, Stated(rel.RelatingPort?.GlobalId), IfcRelKind.ConnectsPorts.Key, IfcRelKind.ConnectsPorts.Relating, key)
            from b in Endpoint(rooted, Stated(rel.RelatedPort?.GlobalId), IfcRelKind.ConnectsPorts.Key, IfcRelKind.ConnectsPorts.Related, key)
            from r in Optional(rel.RealizingElement).Match(
                Some: element => Endpoint(rooted, Stated(element.GlobalId), IfcRelKind.ConnectsPorts.Key, "RealizingElement", key).Map(Some),
                None: static () => (Fin.Succ(Option<NodeId>.None)).ToValidation())
            select (Relationship)new Relationship.Connect(a, b, ConnectKind.Port, r, Option<UInt128>.None))),
        Rows(project.Extract<IfcRelConnectsPortToElement>(), IfcRelKind.ConnectsPortToElement, rooted, key),
        Rows(project.Extract<IfcRelConnectsPathElements>(), IfcRelKind.ConnectsPathElements, rooted, key),
        Rows(project.Extract<IfcRelInterferesElements>(), IfcRelKind.InterferesElements, rooted, key),
        Rows(project.Extract<IfcRelSequence>(), IfcRelKind.Sequence, rooted, key),
        Rows(project.Extract<IfcRelFlowControlElements>(), IfcRelKind.FlowControl, rooted, key),
        // The 2x3 element-to-idealized-member binding; Extract<IfcRelConnectsStructuralMember> separately returns
        // IfcRelConnectsWithEccentricity, whose edge rides the Structural fold — its mandatory ConnectionConstraint
        // content-keys through the store's STEP-fragment lane onto its owner-stamped Eccentricity row [M2].
        Rows(project.Extract<IfcRelConnectsStructuralElement>(), IfcRelKind.ConnectsStructElement, rooted, key),
        Realizing(project, rooted, profiles, key));

    // Realization is the seam Connect.Realizing FIELD, never a sub-kind row [NEUTRAL_EDGE_RULING].
    // RealizingElements is a SET [1:?], so the fold FANS OUT one edge per member over the same (From, To) pair — a
    // moment connection realized by a plate AND its bolts lands N edges, where the .Head slice dropped every member
    // past the first. A schema-invalid EMPTY set faults typed and cannot masquerade as a base connect.
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

    // The connection-interface preservation: an IfcConnectionGeometry is the joint's physical interface surface (a
    // point, curve, or surface the inline prohibition keeps off the seam [M2]), so it PRESERVES as a STEP fragment
    // through the store's content-keyed lane and the key alone crosses. The geometry is OPTIONAL on both carriers,
    // so an absent one is plain topology; a dropped present one left every re-exported joint and every 2nd-level
    // energy boundary geometrically unlocated.
    static Option<UInt128> PreserveInterface(IfcConnectionGeometry? geometry, IIfcProfileStore profiles, Op key) =>
        Optional(geometry).Map(surface => profiles.Preserve(surface, key));

    // Extract<IfcRelAssignsToGroup> separately returns IfcRelAssignsToGroupByFactor — its membership edge lands here
    // unchanged and its unslotted Factor is the counted drop above.
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
        // IFC4.3 linear-referencing placement (alignment/grid to positioned products) and the declaring-object
        // typing family.
        Rows(project.Extract<IfcRelPositions>(), IfcRelKind.Positions, rooted, key),
        Rows(project.Extract<IfcRelDefinesByObject>(), IfcRelKind.DefinesByObject, rooted, key));

    // The property/quantity ATTACHMENT onto neutral Assign.PropertyDefinition edges the seam Bake reads. Both sides
    // are SETs — a many-to-many the row fold's single relating slot cannot express — so this arm stays its own:
    // one edge per (related occurrence, definition) pair, Subject the occurrence and Definition the bag node.
    // Extract<IfcRelDefinesByProperties> returns the 2x3 IfcRelOverridesProperties subtype too, so an override
    // binding lands its attachment edge here.
    static Seq<Validation<Error, Seq<Relationship>>> DefinesProperties(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        Landed(toSeq(project.Extract<IfcRelDefinesByProperties>().AsIterable()
            .SelectMany(rel => rel.RelatedObjects.SelectMany(o => rel.RelatingPropertyDefinition.Select(definition =>
                from subject in Endpoint(rooted, Stated(o.GlobalId), IfcRelKind.DefinesByType.Key, "RelatedObjects", key)
                from bag in Endpoint(rooted, Stated(definition.GlobalId), IfcRelKind.DefinesByType.Key, "RelatingPropertyDefinition", key)
                select (Relationship)new Relationship.Assign(subject, bag, AssignKind.PropertyDefinition)))))));

    // Both restraint families and the whole load family lower through the DEDICATED
    // Model/structural#STRUCTURAL_PROJECTION Attrs owner in ONE call, so a local two-step over the
    // connection/activity entity is the deleted form. `profiles` is the SAME fragment lane PreserveInterface takes,
    // so the eccentric subtype's MANDATORY ConnectionConstraint content-keys inside its OWN owner — a second
    // eccentricity read here would fork one payload across two writers.
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

    // The energy/spatial space-boundary graph onto neutral Generic edges [NEUTRAL_EDGE_RULING], the 1st/2nd-level
    // discriminant riding the attrs. RelatingSpace is an IfcSpaceBoundarySelect, so its GlobalId reads through the
    // IfcRoot cast the endpoint admission already performs.
    static Seq<Validation<Error, Seq<Relationship>>> SpatialBoundaries(IfcProject project, Map<string, NodeId> rooted, IIfcProfileStore profiles, Op key) => Seq(
        Landed(toSeq(project.Extract<IfcRelSpaceBoundary>().AsIterable().Select(rel =>
            from s in Endpoint(rooted, Stated((rel.RelatingSpace as IfcRoot)?.GlobalId), IfcRelKind.SpaceBoundary.Key, IfcRelKind.SpaceBoundary.Relating, key)
            from e in Endpoint(rooted, Stated(rel.RelatedBuildingElement?.GlobalId), IfcRelKind.SpaceBoundary.Key, IfcRelKind.SpaceBoundary.Related, key)
            select (Relationship)new Relationship.Generic(IfcRelKind.SpaceBoundary.Wire, s, e, BoundaryAttrs(rel, profiles, key))))));

    // THREE-valued because the runtime type is: 2ndLevel derives from 1stLevel, so the 2nd probe runs first and a
    // level is never upgraded onto a base instance. GeometryGym keeps PhysicalOrVirtualBoundary and
    // InternalOrExternalBoundary on internal fields with no public getter, so the level is the lone publicly
    // readable flag and the physical/virtual classification is never fabricated. The level attr is built ONCE and
    // the optional interface key FOLDS onto it, so the two arms of the geometry Option cannot disagree.
    static Map<PropertyName, PropertyValue> BoundaryAttrs(IfcRelSpaceBoundary rel, IIfcProfileStore profiles, Op key) =>
        PreserveInterface(rel.ConnectionGeometry, profiles, key).Fold(
            Map((BoundaryRows.BoundaryLevel, (PropertyValue)new PropertyValue.Text(BoundaryLevelOf(rel)))),
            static (attrs, surface) => attrs.Add(SemanticProjector.InterfaceKey, new PropertyValue.Text(surface.ToString("X32"))));

    static string BoundaryLevelOf(IfcRelSpaceBoundary rel) =>
        rel is IfcRelSpaceBoundary2ndLevel ? "2nd" : rel is IfcRelSpaceBoundary1stLevel ? "1st" : "";

    // --- [MATERIAL_ARM]

    // The material node and usage bind ONCE PER RELATION and the related objects fan over the pair — the
    // per-(rel, object) re-projection ran the whole composition fold N times per relation, the deleted quadratic
    // form. RelatingMaterial is schema-mandatory, so a null read is a malformed file faulting typed HERE: the
    // node-side Materials fold Optional-skips the same null, and this edge rail is the one fault site. The bag node
    // id re-derives through the SAME PropertySetNode content mint the node-side fold takes, so both ends key
    // identically with no shared table.
    static WriterT<FidelityLog, Fin, Seq<Relationship>> MaterialEdges(
        IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScheme scale, TemplateScope templates,
        IIfcProfileStore profiles, Op key) =>
        toSeq(project.Extract<IfcRelAssociatesMaterial>().AsIterable())
            .Traverse(rel =>
                from relating in Fidelity.Lift(Optional(rel.RelatingMaterial).ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Rejected, string.Join(':', new object?[] { "material-relation-unbound", rel.GlobalId }))))
                from material in Fidelity.Lift(MaterialProjection.Project(relating, tolerance, profiles, scale, key).Map(static node => node.Id))
                from usage in Fidelity.Lift(UsageOf(relating, scale, key))
                // The ONE crossing from the retired hand carrier: Semantics/composition#MATERIAL_COMPOSITION still
                // returns its narrowing facts beside its value, so its log enters through Told at this single site
                // instead of every consumer re-threading it — and the type never reaches a signature here.
                from bags in SemanticProjector.DefinitionOf(relating)
                    .Map(definition => Fidelity.Lift(MaterialProjection.ImportedPsets(definition, rooted, scale, templates, key))
                        .Bind(narrowed => Fidelity.Told(narrowed.Log, narrowed.Value)))
                    .IfNone(Fidelity.Clean(Seq<PropertyBag>()))
                // RelatedObjects is a SET<IfcDefinitionSelect> — a SELECT interface, not an IfcRoot — so the
                // endpoints read through the SAME admission every other arm takes.
                from elements in Fidelity.Lift((toSeq(rel.RelatedObjects.AsIterable())
                    .Map(o => Endpoint(rooted, Stated((o as IfcRoot)?.GlobalId), nameof(IfcRelAssociatesMaterial), "RelatedObjects", key))
                    .Traverse(identity).As()).ToFin())
                select elements.Map(element => (Relationship)new Relationship.Associate(element, material, usage))
                    .Concat(bags.Map(bag => (Relationship)new Relationship.Assign(
                        material, SemanticProjector.PropertySetNode(bag, tolerance).Id, AssignKind.PropertyDefinition))).ToSeq())
            .As()
            .Map(static rows => rows.Flatten().ToSeq());

    // --- [USAGE_ADMISSION]

    // The IFC occurrence material usage -> the seam's typed MaterialUsage [OCCURRENCE_USAGE_RULING]: a layer-set
    // usage lowers all four IFC occurrence parameters, a profile-set usage admits through the seam Of cardinal-point
    // gate, and a type-level set with no occurrence usage is Unbound. The ReferenceExtent (the layer-set size
    // perpendicular to the layers, decompile-confirmed at `.api/api-geometrygym-ifc` row 12) is the 4th seam ctor
    // arg — without it an asymmetric wall finish is dropped at ingest.
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

    // The two GG enums -> their seam rows, TOTAL with a typed refusal. The `_ => Axis3` catch-all mapped every
    // future GG member — and today's NOTDEFINED — onto the third axis, so a layer set that declared no direction
    // read as a vertically-stacked slab; the sense ternary did the same onto Negative. Both targets are SmartEnum
    // ROWS rather than enums, so the correspondence is a table and the refusal names the member it could not map.
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

    // MID is the GG "unset" sentinel, so it lifts to None at this boundary rather than crossing as a cardinal point.
    static Option<int> OptionalCardinal(IfcCardinalPointReference point) =>
        point == IfcCardinalPointReference.MID ? Option<int>.None : Some((int)point);
}
```

## [03]-[RESEARCH]

(none)

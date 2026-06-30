# [PERSISTENCE_VERSION_PROVENANCE]

Rasm.Persistence owns the causal lineage of every durable change as a W3C-PROV-O graph folded over the Marten changefeed plus a tamper-evident, KMS-attested, Merkle-audited ledger that is the one authenticity authority the time-travel checkpoint chain and the structural merge defer to. The lineage is the standards-conformant PROV-O vocabulary, not an ad-hoc edge list: `ProvNode` the closed entity/activity/agent family (an `Entity` a content-addressed `ElementGraph`/delta/snapshot/blob, refined by the `Collection`/`Bundle`/`Plan` PROV subclasses; an `Activity` a commit/merge/import/solve span; an `Agent` discriminated `Person`/`SoftwareAgent`/`Organization`), `ProvRelation` the closed influence vocabulary carrying its PROV-O term, directionality, and endpoint-kind law, and `CausalDag` the static surface that derives the lineage from the changefeed, walks bounded ancestry and descent through the `LineageWalk` frontier, and projects the W3C-PROV-JSON bundle a CDE consumer ingests. The derivation is exact in PROV semantics: a delta `Entity` `WasGeneratedBy` its commit `Activity`, the commit `Activity` `WasAssociatedWith` its signed `Agent` (an activity binds an agent through association, never attribution), the resulting graph `Entity` `WasAttributedTo` that agent, a revised graph `WasRevisionOf` its prior content key (the derivation subclass, not the generic relation), a merge `Activity` `WasInformedBy` the commits it merges, a retired `Entity` `WasInvalidatedBy` its retiring activity, and a software agent `ActedOnBehalfOf` the person who triggered it — so the PROV graph is a fold over the events the system already holds and the qualified `hadRole`/`hadPlan` influence rides the association without a parallel store.

`AttestedEntry` is the hash-chained, KMS-signed ledger row whose `Prior` back-link and rolling `Chain` address make any rewrite detectable, distinct from the non-cryptographic content chain the `Version/timetravel#TIME_TRAVEL` `Checkpoint` carries — that chain proves a checkpoint reproduces from the op stream, THIS chain proves the entry was authored by a verified actor and never reordered. Verification composes the `Element/identity#AUTHORITY` `Authority.Verify` over the resolved `SigningKeyring`, so the per-entry probe is the SAME KMS dispatch that gates every signed op (`Authentic`/`Forged`/`Unauthored`/`Unsigned` `AuthDecision`), never a hand-rolled boolean signature check; the chain folds those verdicts into one `AttestVerdict`. Beyond whole-chain re-fold the ledger seals a Merkle audit head over the rolling addresses so a third party verifies one entry's `InclusionProof` membership and a `ConsistencyProof` append-only extension without replaying the whole chain — the transparency-log discipline the "authenticity authority" prose names. The lineage rides the one changefeed (`Version/ledger#CHANGEFEED`) and the one content-address (`Element/codec#CONTENT_ADDRESS`), never a second store; `Principal` (the bundle authority and `SignedAuthorship.Actor`), `SignedAuthorship`, `OpDigest`, `SigningKeyring`, `Authority`, `AuthDecision` arrive from `Element/identity#AUTHORITY`; `Hlc`/`CommitNode` arrive from `Version/commits`; `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive from AppHost.

## [01]-[INDEX]

- [01]-[CAUSAL_DAG]: the W3C-PROV-O node/relation vocabulary, the exact lineage derivation from the changefeed, the bounded ancestry/descent frontier, and the PROV-JSON bundle egress.
- [02]-[ATTESTED_LEDGER]: the hash-chained KMS-signed ledger, `Authority`-composed verification, the Merkle inclusion/consistency audit proofs, and the tamper-evidence authority.

## [02]-[CAUSAL_DAG]

- Owner: `ProvKind` the `[SmartEnum<string>]` PROV-node-class axis carrying its PROV-O class IRI and the `AssociationRole` an agent associated with this activity kind plays; `AgentClass` the `[SmartEnum<string>]` PROV-O agent-subtype axis (`Person`/`SoftwareAgent`/`Organization`); `ProvRole` the `[SmartEnum<string>]` qualified-influence role vocabulary the association carries; `ProvNode` the `[Union]` causal node (`Entity`/`Activity`/`Agent`) whose `Entity` refines into the `EntitySubclass` PROV form (`Plain`/`Collection`/`Bundle`/`Plan`); `ProvRelation` the `[SmartEnum<string>]` influence vocabulary carrying its PROV-O term, its endpoint-property law (the W3C-PROV-JSON edge map property names), and the derivation-subclass parent it specializes; `ProvEndpoint` the `[SmartEnum<string>]` from/to property-name axis the JSON projection reads; `ProvEdge` the typed causal edge carrying its qualified `ProvRole`/plan; `LineageWalk` the bounded ancestry/descent frontier fold; `CausalDag` the static surface owning lineage derivation, the walks, and the PROV-JSON projection.
- Cases: `ProvNode` is `Entity(ContentAddress, ProvKind, EntitySubclass, Instant)`, `Activity(UInt128 Id, ProvKind Kind, Instant Started, Instant Ended)`, `Agent(string Actor, AgentClass Class, bool Signed)` — the agent identity is the stable actor SUBJECT string (the one identifier the changefeed `Actor` header and a `SignedAuthorship.Actor.Subject` both yield, never a request-transient `Principal` frozen into a durable node), the CLASS is the PROV-O subtype, the `Signed` flag the attestation fact, never a `bool`-as-type; `EntitySubclass` is `Plain | Collection | Bundle | Plan` (the PROV-O `Entity` subclasses — a model is a `Collection` of element entities, the lineage export a `Bundle`, a merge strategy a `Plan`); `ProvRelation` is the eleven W3C-PROV-O influence terms — `WasGeneratedBy | Used | WasInformedBy | WasDerivedFrom | WasRevisionOf | WasQuotedFrom | HadPrimarySource | WasInvalidatedBy | WasAttributedTo | WasAssociatedWith | ActedOnBehalfOf` — each a row carrying its `prov:` term, its endpoint-property law, and the derivation-subclass parent it specializes (`WasRevisionOf`/`WasQuotedFrom`/`HadPrimarySource` generalize to `WasDerivedFrom`), a twelfth being one row, never a parallel edge family; `AgentClass` is the three PROV-O agent subtypes; `ProvRole` is `Author | Reviser | Importer | Merger | Solver | Delegate` — the role an agent played in an association.
- Entry: `public static Seq<ProvEdge> Derive(Seq<OpLogEntry> changefeed, Func<UInt128, Option<CommitNode>> resolve)` projects the lineage graph from the changefeed plus the commit DAG with EXACT PROV-O endpoint typing — each delta entity `WasGeneratedBy` its commit activity, the commit activity `WasAssociatedWith` its signed agent under the agent's `ProvRole`, the delta entity `WasAttributedTo` that agent, a revised entity `WasRevisionOf` each parent commit's produced op-key entities (the derivation subclass, resolved one hop through `resolve` — never the geometry `Closure`), a retired entity `WasInvalidatedBy` its commit activity, and a software agent `ActedOnBehalfOf` its delegating principal; `public static Seq<ProvNode> Walk(LineageWalk walk, Func<ContentAddress, Seq<ProvEdge>> adjacency, Func<UInt128, (ProvKind, EntitySubclass)> kindOf)` is the one bounded breadth-first frontier fold the ancestry and descent directions both dial; `public static Seq<ProvNode> Derivations(ContentAddress root, int depth, Func<ContentAddress, Seq<ProvEdge>> incoming, Func<UInt128, (ProvKind, EntitySubclass)> kindOf)` composes `Walk` over a derivation-family-filtered adjacency for the transitive `wasDerivedFrom` closure; `public static ProvBundle Bundle(Seq<ProvEdge> lineage, Principal authority, Instant at)` names the lineage as a PROV `Bundle` with its own provenance-of-provenance header; `public static JsonElement ProvJson(ProvBundle bundle, Func<UInt128, ProvNode> resolve)` projects the standards-conformant W3C-PROV-JSON document.
- Auto: lineage is DERIVED from the changefeed plus the commit DAG, never a parallel provenance write — a delta IS the `WasGeneratedBy` evidence (delta entity → its commit activity), and the `Version/commits#COMMIT_DAG` parent commits' op-key entities (resolved one hop through `resolve`, the parent commit being an Activity whose produced entities are its `OpKeys`) ARE the `WasRevisionOf` sources, NEVER the `OpLogEntry.Closure` geometry-blob manifest, so the PROV graph is a fold over the events the system already holds; the association edge reads the `Element/identity#AUTHORITY` `SignedAuthorship` so a `WasAssociatedWith` names a verified `Person`/`Organization` `Agent` when the op was KMS-signed and a `SoftwareAgent` (a Compute solver activity, an IFC importer) when an automated activity produced the entity, the automated agent `ActedOnBehalfOf` the human principal that triggered the run; the activity span reads the commit cell `Hlc` so the PROV `startedAtTime`/`endedAtTime` ride the one causal clock; the qualified `hadRole`/`hadPlan` of an association reads the activity's `ProvKind` (an `Import` activity's agent plays `Importer`, a `Merge` activity's plays `Merger` and `hadPlan` the merge strategy) so the influence is qualified, never bare.
- Receipt: a lineage derivation rides `store.prov.derive` carrying the edge count by `ProvRelation`; an ancestry/descent walk rides `store.prov.walk` carrying the reached-node count, depth, and direction; a PROV-JSON bundle export rides `store.prov.export` carrying the bundle node and edge counts.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Text.Json (the `Element/codec#CODEC_AXIS` `ElementJson.Options` Thinktecture-converter set the PROV-JSON `SerializeToElement` egress composes, never a second converter registration), System.Runtime.InteropServices, BCL inbox.
- Growth: a new PROV relation is one `ProvRelation` row carrying its term + endpoint law + derivation-subclass parent; a new node class is one `ProvNode`/`EntitySubclass`/`ProvKind` row; a new agent subtype is one `AgentClass` row; a new association role is one `ProvRole` row; a new walk direction is one `LineageWalk` disposition over the one `Walk` fold; zero new surface — a parallel provenance store, a second lineage walker, an attribution edge mis-typed off an activity, or a free-string PROV term is the deleted form because the lineage is a fold over the changefeed and the PROV vocabulary is the closed W3C-PROV-O term set.
- Boundary: the causal DAG is DERIVED from the changefeed plus the commit DAG — a delta entity's `WasGeneratedBy`, its commit's `WasAssociatedWith` agent, and the `WasRevisionOf` predecessor (each parent commit's produced op-key entities, resolved one hop, NEVER the `OpLogEntry.Closure` descendant-geometry manifest which is a blob set and not a version predecessor — keying revision off `Closure.Head` is the deleted defect) are all reads off the events, never a write of record, so the lineage is reconstructible from the one op stream a replica folds; the PROV-O typing is EXACT — an `Activity` binds an `Agent` through `WasAssociatedWith` and an `Entity` through `WasAttributedTo`, so an attribution edge sourced off an activity is the deleted defect, the derivation of a revised graph is the `WasRevisionOf` subclass not the generic `WasDerivedFrom`, and the `WasInformedBy` activity-to-activity chain captures a merge informed by its parents; the agent is a PROV-O subtype (`Person`/`SoftwareAgent`/`Organization`) so a derived `Assessment` result names its `SoftwareAgent` solver `ActedOnBehalfOf` the human `Person` who triggered it, never an anonymous machine write; the W3C-PROV-JSON egress is a real standards document (top-level `prefix`/`entity`/`activity`/`agent`/`wasGeneratedBy`/`used`/`wasAssociatedWith`/`wasAttributedTo`/`wasDerivedFrom`/`actedOnBehalfOf`/`bundle` keyed maps with `prov:`-prefixed properties), never a flat `from→to` edge dictionary, so a CDE consumer ingests it through any PROV-O toolchain; the bundle is a PROV `Bundle` carrying its own provenance-of-provenance so the export is itself attributable; the lineage walk is bounded breadth-first over the typed adjacency so the cost is linear in the reachable-edge count within the depth bound; the attribution reconciles with the `Version/timetravel#TIME_TRAVEL` `BlameRow` (the same `(Hlc, origin)` winner the convergence selected) so blame and provenance never disagree.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProvRole {
    public static readonly ProvRole Author = new("author");
    public static readonly ProvRole Reviser = new("reviser");
    public static readonly ProvRole Importer = new("importer");
    public static readonly ProvRole Merger = new("merger");
    public static readonly ProvRole Solver = new("solver");
    public static readonly ProvRole Delegate = new("delegate");
}

// The PROV-O endpoint law per influence term — the from/to property NAMES the W3C-PROV-JSON edge map
// emits (an EntityActivity generation carries prov:entity/prov:activity, an AgentAgent delegation
// prov:delegate/prov:responsible), so the JSON projection reads the property names off the relation's
// own endpoint row rather than a per-term switch.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProvEndpoint {
    public static readonly ProvEndpoint EntityActivity = new("entity->activity", "entity", "activity");
    public static readonly ProvEndpoint ActivityEntity = new("activity->entity", "activity", "entity");
    public static readonly ProvEndpoint ActivityActivity = new("activity->activity", "informed", "informant");
    public static readonly ProvEndpoint EntityEntity = new("entity->entity", "generatedEntity", "usedEntity");
    public static readonly ProvEndpoint EntityAgent = new("entity->agent", "entity", "agent");
    public static readonly ProvEndpoint ActivityAgent = new("activity->agent", "activity", "agent");
    public static readonly ProvEndpoint AgentAgent = new("agent->agent", "delegate", "responsible");
    public string FromProperty { get; }
    public string ToProperty { get; }
    private ProvEndpoint(string key, string fromProperty, string toProperty) : this(key) => (FromProperty, ToProperty) = (fromProperty, toProperty);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProvKind {
    public static readonly ProvKind Graph = new("graph", "prov:Entity", ProvRole.Author);
    public static readonly ProvKind Delta = new("delta", "prov:Entity", ProvRole.Author);
    public static readonly ProvKind Snapshot = new("snapshot", "prov:Entity", ProvRole.Author);
    public static readonly ProvKind Blob = new("blob", "prov:Entity", ProvRole.Author);
    public static readonly ProvKind Commit = new("commit", "prov:Activity", ProvRole.Author);
    public static readonly ProvKind Merge = new("merge", "prov:Activity", ProvRole.Merger);
    public static readonly ProvKind Import = new("import", "prov:Activity", ProvRole.Importer);
    public static readonly ProvKind Solve = new("solve", "prov:Activity", ProvRole.Solver);
    public string ClassIri { get; }
    // The role an agent associated with this kind of activity played, qualifying the WasAssociatedWith
    // influence — an Import activity's agent is the Importer, a Merge's the Merger, a Solve's the Solver.
    public ProvRole AssociationRole { get; }
    // The PROV class discriminant a lineage-walk node mint reads (`prov:Activity` kinds are Activity nodes, the rest
    // Entity nodes) so a reached commit is typed Activity, never an activity-kinded Entity.
    public bool IsActivity => ClassIri == "prov:Activity";
    private ProvKind(string key, string classIri, ProvRole role) : this(key) => (ClassIri, AssociationRole) = (classIri, role);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AgentClass {
    public static readonly AgentClass Person = new("person", "prov:Person");
    public static readonly AgentClass SoftwareAgent = new("software", "prov:SoftwareAgent");
    public static readonly AgentClass Organization = new("organization", "prov:Organization");
    public string ClassIri { get; }
    private AgentClass(string key, string classIri) : this(key) => ClassIri = classIri;
}

[SmartEnum]
public sealed partial class EntitySubclass {
    public static readonly EntitySubclass Plain = new("prov:Entity");
    public static readonly EntitySubclass Collection = new("prov:Collection");
    public static readonly EntitySubclass Bundle = new("prov:Bundle");
    public static readonly EntitySubclass Plan = new("prov:Plan");
    public string ClassIri { get; }
    private EntitySubclass(string classIri) => ClassIri = classIri;
}

// The closed W3C-PROV-O influence vocabulary: each row carries its prov: term, its endpoint law, and
// the derivation-subclass parent it specializes (so a WasRevisionOf ancestry resolves to the generic
// wasDerivedFrom family — `Derivations` reads `GeneralizesTo` to bound a derivation-only lineage).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProvRelation {
    public static readonly ProvRelation WasGeneratedBy = new("wasGeneratedBy", "prov:wasGeneratedBy", ProvEndpoint.EntityActivity, generic: None);
    public static readonly ProvRelation Used = new("used", "prov:used", ProvEndpoint.ActivityEntity, generic: None);
    public static readonly ProvRelation WasInformedBy = new("wasInformedBy", "prov:wasInformedBy", ProvEndpoint.ActivityActivity, generic: None);
    public static readonly ProvRelation WasDerivedFrom = new("wasDerivedFrom", "prov:wasDerivedFrom", ProvEndpoint.EntityEntity, generic: None);
    public static readonly ProvRelation WasRevisionOf = new("wasRevisionOf", "prov:wasRevisionOf", ProvEndpoint.EntityEntity, generic: Some(WasDerivedFrom));
    public static readonly ProvRelation WasQuotedFrom = new("wasQuotedFrom", "prov:wasQuotedFrom", ProvEndpoint.EntityEntity, generic: Some(WasDerivedFrom));
    public static readonly ProvRelation HadPrimarySource = new("hadPrimarySource", "prov:hadPrimarySource", ProvEndpoint.EntityEntity, generic: Some(WasDerivedFrom));
    public static readonly ProvRelation WasInvalidatedBy = new("wasInvalidatedBy", "prov:wasInvalidatedBy", ProvEndpoint.EntityActivity, generic: None);
    public static readonly ProvRelation WasAttributedTo = new("wasAttributedTo", "prov:wasAttributedTo", ProvEndpoint.EntityAgent, generic: None);
    public static readonly ProvRelation WasAssociatedWith = new("wasAssociatedWith", "prov:wasAssociatedWith", ProvEndpoint.ActivityAgent, generic: None);
    public static readonly ProvRelation ActedOnBehalfOf = new("actedOnBehalfOf", "prov:actedOnBehalfOf", ProvEndpoint.AgentAgent, generic: None);
    public string Term { get; }
    public ProvEndpoint Endpoint { get; }
    public Option<ProvRelation> GeneralizesTo { get; }
    // True iff this relation OR its generalization is the derivation family — the `Derivations` ancestry
    // bound, so a revision/quotation/primary-source edge counts as a derivation step.
    public bool IsDerivation => this == WasDerivedFrom || GeneralizesTo == Some(WasDerivedFrom);
    private ProvRelation(string key, string term, ProvEndpoint endpoint, Option<ProvRelation> generic) : this(key) => (Term, Endpoint, GeneralizesTo) = (term, endpoint, generic);
}

[SmartEnum]
public sealed partial class WalkDirection {
    public static readonly WalkDirection Ancestry = new(forward: false);
    public static readonly WalkDirection Descent = new(forward: true);
    public bool Forward { get; }
    // Ancestry steps to the influencing end (To), descent to the influenced end (From).
    public UInt128 Step(ProvEdge edge) => Forward ? edge.From : edge.To;
    private WalkDirection(bool forward) => Forward = forward;
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvNode {
    private ProvNode() { }
    public sealed record Entity(ContentAddress Address, ProvKind Kind, EntitySubclass Subclass, Instant At) : ProvNode;
    public sealed record Activity(UInt128 Id, ProvKind Kind, Instant Started, Instant Ended) : ProvNode;
    public sealed record Agent(string Actor, AgentClass Class, bool Signed) : ProvNode;

    // Mint the CORRECT PROV class from the kind: an `IsActivity` kind (Commit/Merge/Import/Solve, `prov:Activity`) is an
    // Activity node, an entity kind (Graph/Delta/Snapshot/Blob, `prov:Entity`) an Entity — so a reached COMMIT in a
    // lineage walk is never mis-minted as an Entity (the prior `new ProvNode.Entity(...)`-for-everything tangle would
    // emit a `prov:Activity`-kinded Entity, a PROV-O typing contradiction the JSON projection's class map would mis-file).
    public static ProvNode Of(ContentAddress address, ProvKind kind, EntitySubclass subclass) =>
        kind.IsActivity ? new Activity(address.Value, kind, Instant.MinValue, Instant.MinValue) : new Entity(address, kind, subclass, Instant.MinValue);

    public UInt128 Identity => this switch { Entity e => e.Address.Value, Activity a => a.Id, Agent g => CausalDag.AgentKey(g.Actor) };
    public string ClassIri => this switch { Entity e => e.Subclass.ClassIri, Activity a => a.Kind.ClassIri, Agent g => g.Class.ClassIri };
}

// The qualified causal edge — the PROV term, the endpoints, the HLC cell, plus the qualified-influence
// role (hadRole) and the optional plan (hadPlan) an association carries.
public readonly record struct ProvEdge(ProvRelation Relation, UInt128 From, UInt128 To, Hlc Cell, Option<ProvRole> Role, Option<UInt128> Plan) {
    public static ProvEdge Of(ProvRelation relation, UInt128 from, UInt128 to, Hlc cell) => new(relation, from, to, cell, None, None);
    public ProvEdge Qualified(ProvRole role, Option<UInt128> plan) => this with { Role = Some(role), Plan = plan };
}

// A bounded ancestry/descent request — root, direction, and the depth ceiling the frontier fold respects.
public readonly record struct LineageWalk(ContentAddress Root, WalkDirection Direction, int Depth) {
    public static LineageWalk Ancestry(ContentAddress root, int depth) => new(root, WalkDirection.Ancestry, depth);
    public static LineageWalk Descent(ContentAddress root, int depth) => new(root, WalkDirection.Descent, depth);
}

// A named PROV Bundle — a set of provenance descriptions with its own provenance-of-provenance (who
// asserted the bundle, when), so the lineage export is itself an attributable PROV Entity.
public readonly record struct ProvBundle(UInt128 Id, Seq<ProvEdge> Lineage, Principal Authority, Instant At);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CausalDag {
    public static Seq<ProvEdge> Derive(Seq<OpLogEntry> changefeed, Func<UInt128, Option<CommitNode>> resolve) =>
        changefeed.Bind(entry => {
            Option<CommitNode> node = resolve(entry.ContentKey);
            Option<UInt128> commit = node.Map(static c => c.ContentKey);
            // The COMMIT (activity) nature decides the association role: a multi-parent commit IS a merge
            // (CommitNode.IsMerge), so its agent plays Merger and the activity hadPlan the merge; an
            // ordinary commit's agent plays Author. The role is read off the activity, never the entity kind.
            ProvKind activity = node.Map(static c => c.IsMerge ? ProvKind.Merge : ProvKind.Commit).IfNone(ProvKind.Commit);
            UInt128 agent = AgentKey(entry.Actor);
            // EXACT PROV-O: the delta/snapshot Entity WasGeneratedBy its commit Activity; the Activity
            // WasAssociatedWith its Agent qualified by the activity's role + plan; the produced Entity
            // WasAttributedTo that Agent; a merge Activity WasInformedBy each parent commit Activity; a
            // revised Entity WasRevisionOf its prior (the derivation subclass), a retired one WasInvalidatedBy
            // its activity. No attribution is ever sourced off an activity (that is association).
            Seq<ProvEdge> generated = commit.ToSeq().Map(c => ProvEdge.Of(ProvRelation.WasGeneratedBy, entry.ContentKey, c, entry.Stamp));
            Seq<ProvEdge> associated = commit.ToSeq().Map(c => ProvEdge.Of(ProvRelation.WasAssociatedWith, c, agent, entry.Stamp).Qualified(activity.AssociationRole, commit));
            Seq<ProvEdge> attributed = Seq(ProvEdge.Of(ProvRelation.WasAttributedTo, entry.ContentKey, agent, entry.Stamp));
            Seq<ProvEdge> informed = node.ToSeq().Bind(c => commit.ToSeq().Bind(activityKey => c.Parents.Map(parent => ProvEdge.Of(ProvRelation.WasInformedBy, activityKey, parent, entry.Stamp))));
            // Lineage rides the COMMIT-DAG, NEVER the `OpLogEntry.Closure` (which is the DESCENDANT GEOMETRY content-key
            // manifest — a blob set, not a predecessor). PROV-O endpoint typing is exact: a RETIRED entity `WasInvalidatedBy`
            // its retiring commit ACTIVITY (EntityActivity, entity->activity, so the target is the commit key); a REVISED
            // entity `WasRevisionOf` each PARENT-COMMIT'S delta ENTITY (EntityEntity, entity->entity — the parent commit is
            // an Activity, so the predecessor is its produced op-key entities, resolved one hop through `resolve`, NEVER the
            // parent commit key itself which would mistype an activity as the used entity). A root commit (no parents) emits
            // no revision edge — the genesis is generation-only.
            Seq<ProvEdge> lineage = entry.Kind.Tombstone
                ? commit.ToSeq().Map(activityKey => ProvEdge.Of(ProvRelation.WasInvalidatedBy, entry.ContentKey, activityKey, entry.Stamp))
                : node.ToSeq().Bind(c => c.Parents.Bind(parent => resolve(parent).ToSeq()
                    .Bind(p => p.OpKeys.Map(priorEntity => ProvEdge.Of(ProvRelation.WasRevisionOf, entry.ContentKey, priorEntity, entry.Stamp)))));
            return generated + associated + attributed + informed + lineage;
        });

    public static Seq<ProvNode> Walk(LineageWalk walk, Func<ContentAddress, Seq<ProvEdge>> adjacency, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf) {
        // Bounded breadth-first frontier fold: each level expands the current frontier's adjacency to the
        // next, records the freshly-seen nodes, and recurses on ONLY the new frontier until the depth
        // ceiling or quiescence — never re-descending an already-visited node (the prior re-walk-every-
        // reached-node tangle is the deleted form).
        Seq<ProvNode> Expand(Seq<ContentAddress> frontier, LanguageExt.HashSet<UInt128> seen, Seq<ProvNode> reached, int depth) {
            if (depth <= 0 || frontier.IsEmpty) return reached;
            (LanguageExt.HashSet<UInt128> Seen, Seq<ContentAddress> Next, Seq<ProvNode> Reached) level = frontier
                .Bind(node => adjacency(node).Map(walk.Direction.Step))
                .Distinct()
                .Fold((Seen: seen, Next: Seq<ContentAddress>(), Reached: reached), (acc, id) => acc.Seen.Contains(id)
                    ? acc
                    : kindOf(id) is var (kind, subclass) && ContentAddress.Of(id) is var address
                        ? (acc.Seen.Add(id), acc.Next.Add(address), acc.Reached.Add(ProvNode.Of(address, kind, subclass)))
                        : acc);
            return Expand(level.Next, level.Seen, level.Reached, depth - 1);
        }
        return Expand(Seq(walk.Root), HashSet(walk.Root.Value), Seq<ProvNode>(), walk.Depth);
    }

    // Derivation-only ancestry — the transitive wasDerivedFrom/Revision/Quotation/PrimarySource closure
    // (the PROV "what did this derive from" query) by composing `Walk` over an adjacency pre-filtered to
    // the derivation family through `ProvRelation.IsDerivation`, never a second walker.
    public static Seq<ProvNode> Derivations(ContentAddress root, int depth, Func<ContentAddress, Seq<ProvEdge>> incoming, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf) =>
        Walk(LineageWalk.Ancestry(root, depth), address => incoming(address).Filter(static e => e.Relation.IsDerivation), kindOf);

    public static ProvBundle Bundle(Seq<ProvEdge> lineage, Principal authority, Instant at) {
        var rolling = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        foreach (var edge in lineage.OrderBy(static e => (e.From, e.To, e.Relation.Key))) {
            BinaryPrimitives.WriteUInt128LittleEndian(word, edge.From); rolling.Append(word);
            BinaryPrimitives.WriteUInt128LittleEndian(word, edge.To); rolling.Append(word);
            rolling.Append(MemoryMarshal.AsBytes(edge.Relation.Key.AsSpan()));
        }
        return new ProvBundle(rolling.GetCurrentHashAsUInt128(), lineage, authority, at);
    }

    // A standards-conformant W3C-PROV-JSON document: top-level prefix + per-class node maps (entity/
    // activity/agent) + per-relation influence maps, every node and edge keyed by its prov:-namespaced
    // id, every influence carrying its endpoint properties plus the qualified prov:hadRole/prov:hadPlan
    // — ingestible by any PROV-O toolchain, never a flat from->to edge dictionary.
    public static JsonElement ProvJson(ProvBundle bundle, Func<UInt128, ProvNode> resolve) {
        static string Iri(UInt128 id) => $"rasm:{id:x32}";
        // The bundle is itself an attributable PROV `Bundle` entity: it registers under `entity` as a `prov:Bundle`,
        // its asserting authority under `agent` keyed by the authority's SUBJECT (the same actor-key discipline
        // `AgentKey` uses), and a bundle->authority `wasAttributedTo` edge is emitted so the export carries its own
        // provenance-of-provenance — the prose's "the export is itself attributable" realized, not a captured-but-
        // unemitted field. The lineage nodes group by PROV class and merge into the same three class maps.
        var authorityKey = AgentKey(bundle.Authority.Subject);
        var byClass = bundle.Lineage.Bind(static e => Seq(e.From, e.To)).Distinct().Map(resolve)
            .GroupBy(static n => n is ProvNode.Entity ? "entity" : n is ProvNode.Activity ? "activity" : "agent")
            .ToDictionary(static g => g.Key, static g => g.ToDictionary(n => Iri(n.Identity), NodeMembers));
        var entities = byClass.TryGetValue("entity", out var es) ? es : [];
        var agents = byClass.TryGetValue("agent", out var gs) ? gs : [];
        entities[Iri(bundle.Id)] = new Dictionary<string, object?> { ["prov:type"] = EntitySubclass.Bundle.ClassIri, ["prov:generatedAtTime"] = bundle.At.ToString() };
        agents[Iri(authorityKey)] = new Dictionary<string, object?> { ["prov:type"] = AgentClass.Person.ClassIri, ["rasm:id"] = bundle.Authority.Subject, ["rasm:signed"] = false };
        var document = new Dictionary<string, object> {
            ["prefix"] = new Dictionary<string, string> { ["prov"] = "http://www.w3.org/ns/prov#", ["rasm"] = "urn:rasm:prov:" },
            ["bundle"] = Iri(bundle.Id), ["entity"] = entities, ["agent"] = agents,
            ["wasAttributedTo"] = new Dictionary<string, object> { ["_:b0"] = new Dictionary<string, object?> { ["prov:entity"] = Iri(bundle.Id), ["prov:agent"] = Iri(authorityKey) } },
        };
        if (byClass.TryGetValue("activity", out var acts)) document["activity"] = acts;
        foreach (var byRelation in bundle.Lineage.GroupBy(static e => e.Relation.Term))
            document[byRelation.Key] = byRelation.Select(static (e, i) => (Key: $"_:e{i}", Edge: e)).ToDictionary(static p => p.Key, p => EdgeMembers(p.Edge));
        return JsonSerializer.SerializeToElement(document, ElementJson.Options);

        static object NodeMembers(ProvNode node) => node switch {
            ProvNode.Entity e => new Dictionary<string, object?> { ["prov:type"] = e.ClassIri, ["rasm:kind"] = e.Kind.Key, ["prov:generatedAtTime"] = e.At.ToString() },
            ProvNode.Activity a => new Dictionary<string, object?> { ["prov:type"] = a.ClassIri, ["prov:startedAtTime"] = a.Started.ToString(), ["prov:endedAtTime"] = a.Ended.ToString() },
            ProvNode.Agent g => new Dictionary<string, object?> { ["prov:type"] = g.ClassIri, ["rasm:id"] = g.Actor, ["rasm:signed"] = g.Signed },
            _ => new Dictionary<string, object?>(),
        };

        static object EdgeMembers(ProvEdge edge) {
            var members = new Dictionary<string, object?> {
                [$"prov:{edge.Relation.Endpoint.FromProperty}"] = Iri(edge.From),
                [$"prov:{edge.Relation.Endpoint.ToProperty}"] = Iri(edge.To),
                ["rasm:atTime"] = edge.Cell.Physical.ToString(),
            };
            edge.Role.Iter(role => members["prov:hadRole"] = role.Key);
            edge.Plan.Iter(plan => members["prov:hadPlan"] = Iri(plan));
            return members;
        }
    }

    // The agent key is the XxHash128 of the actor SUBJECT string — the one stable actor identifier reconstructible
    // from BOTH the changefeed `OpLogEntry.Actor` header and a `SignedAuthorship.Actor.Subject`, never a full
    // `Principal` (a request-transient validated-JWT record carrying a live `ClaimsIdentity`/scopes/expiry that has
    // no place frozen in a durable PROV node, and that the bare changefeed actor string cannot reconstruct).
    internal static UInt128 AgentKey(string actor) => XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(actor));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                          | [BINDING]                                                  |
| :-----: | :------------------ | :----------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | lineage source      | derived from the changefeed + commit DAG         | never a parallel provenance write; never the geometry `Closure` |
|  [02]   | activity↔agent      | `WasAssociatedWith` (never `WasAttributedTo`)    | entity→agent is attribution, activity→agent is association |
|  [03]   | revision source     | each parent commit's `OpKeys` entities (one hop) | `WasRevisionOf` (EntityEntity); never `Closure.Head` (a blob set) |
|  [04]   | agent typing        | `Person`/`SoftwareAgent`/`Organization`          | a solver `ActedOnBehalfOf` its human principal            |
|  [05]   | walk node typing    | `ProvNode.Of` discriminates on `kind.IsActivity` | a reached commit is an Activity, never an activity-kinded Entity |
|  [06]   | egress              | W3C-PROV-JSON bundle (`entity`/`activity`/`agent`) | a standards CDE artifact, never a flat edge dictionary  |
|  [07]   | walk cost           | one bounded breadth-first frontier fold          | linear in reachable edges within the depth bound          |

## [03]-[ATTESTED_LEDGER]

- Owner: `AttestedEntry` the hash-chained, KMS-signed ledger row; `MerkleAudit` the per-head Merkle tree over the rolling addresses; `InclusionProof`/`ConsistencyProof` the third-party audit paths; `AttestVerdict` the closed chain-validity verdict; `AttestedLedger` the static surface owning the chain append, the rolling-address fold, the Merkle head seal, the audit-proof projections, and the `Authority`-composed chain verification that is the one tamper-evidence authority.
- Cases: an `AttestedEntry` carries the entry content key, the `Prior` back-link, the rolling `Chain` address (`XxHash128` over `Prior ++ ContentKey`), and the optional `SignedAuthorship`; `AttestVerdict` is `Authentic | Broken(at) | Unsigned | Unauthored(at) | Forged(at)` — `Authentic` the verified chain, `Broken` a back-link/rolling-address discontinuity, `Unsigned` a local-tier chain with no KMS signature, `Unauthored` a signed entry whose `OpDigest` does not bind its content (the `AuthDecision.Unauthored` arm), `Forged` a signature that fails KMS verification (the `AuthDecision.Forged` arm); the verdict cases mirror the `Element/identity#AUTHORITY` `AuthDecision` cryptographic arms so verification never re-classifies what `Authority.Verify` already decided.
- Entry: `public static AttestedEntry Append(Option<AttestedEntry> prior, UInt128 contentKey, Option<SignedAuthorship> authorship)` extends the chain with the new rolling address; `public static IO<AttestVerdict> Verify(Seq<AttestedEntry> chain, Func<SignedAuthorship, SigningKeyring> keyringFor, Func<AttestedEntry, OpDigest> digestOf)` re-folds the chain, confirms every back-link and rolling address, and runs `Authority.Verify` over the per-entry resolved keyring with the INDEPENDENTLY recomputed expected digest (so `Unauthored` — a signature over a digest that does not bind the entry's content — is actually reachable, never self-compared against the stored digest); `public static MerkleAudit Seal(Seq<AttestedEntry> chain)` folds the rolling addresses into a balanced Merkle tree whose root is the audit head; `public static Option<InclusionProof> Prove(MerkleAudit audit, int leaf)` projects the sibling-hash path proving one entry's membership; `public static bool Includes(InclusionProof proof, UInt128 leaf, UInt128 root)` re-folds the path to the root; `public static ConsistencyProof Extend(MerkleAudit older, MerkleAudit newer)` issues the proof the newer head append-only-extends the older; `public static bool Consistent(ConsistencyProof proof, MerkleAudit newer)` confirms it by re-sealing the newer's leaf prefix to the old root.
- Auto: the ledger is the AUTHENTICITY authority distinct from the reproducibility chain — the `Version/timetravel#TIME_TRAVEL` `Checkpoint.Hash` is a non-cryptographic content chain that proves a checkpoint reproduces from the op stream, while THIS chain's `SignedAuthorship` proves the entry was authored by a verified actor and not rewritten; verification re-folds the rolling address over the back-links and routes each signed entry through `Authority.Verify` so the cryptographic verdict is the SAME KMS dispatch (`Authentic`/`Forged`/`Unauthored`/`Unsigned`) that gates every signed op, never a hand-rolled boolean; the Merkle head seals the rolling addresses so a third party verifies one entry's `InclusionProof` and an append-only `ConsistencyProof` between two heads without replaying the whole chain.
- Receipt: a chain append rides `store.attest.append`; a verification rides `store.attest.verify` carrying the verdict and the break locus when broken; an audit-proof projection rides `store.attest.prove` carrying the leaf index and the proof path length.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (the KMS verify rides `Element/identity#AUTHORITY` `Authority.Verify` over the resolved `SigningKeyring`, never a direct provider call here).
- Growth: a new verdict is one `AttestVerdict` case mirroring an `AuthDecision` arm; a richer audit proof is one projection over `MerkleAudit`; zero new surface — a second tamper-evidence scheme, a hand-rolled signature check beside `Authority.Verify`, a Merkle-tree audit log built on a second hasher, or a content chain claiming authenticity is the deleted form because this ledger owns authenticity and the checkpoint chain owns reproducibility, two distinct concerns.
- Boundary: the attested ledger is the ONE tamper-evidence authority — the `Version/timetravel#TIME_TRAVEL` `Checkpoint` hash chain explicitly defers here and carries no authenticity claim, so a content chain standing in for tamper-evidence is the deleted form; the chain is hash-chained off the prior rolling address so any inserted, deleted, or reordered entry breaks every downstream address (a `Broken` verdict naming the discontinuity); the per-entry cryptographic verdict COMPOSES `Element/identity#AUTHORITY` `Authority.Verify(authorship, digest, keyring)` with the `digest` being the INDEPENDENTLY recomputed expected digest off the entry's content (`digestOf`, never the stored `authorship.Digest` self-compared — that self-comparison makes `Unauthored` unreachable, the illusory-verify deleted form) — a verified `AuthDecision.Authentic` proves the actor and the order, an `Unsigned` chain (the local/Personal tier on `KmsProvider.None`) proves order only, an `Unauthored` names a signed entry whose digest does not bind its content (the recomputed digest differs from the signed one), and a `Forged` names the entry whose KMS signature fails, so a second boolean signature predicate beside the one `Authority` verifier is the deleted form; the Merkle audit head is the transparency-log discipline — `InclusionProof` lets an external auditor confirm one entry is in the ledger from its sibling path and root alone, and `ConsistencyProof` confirms a later head only appended (never rewrote) so a regulator audits a slice without the whole history; the Merkle tree composes the one `XxHash128` the rolling chain and the content address already use (the `Version/commits#COMMIT_DAG` `MerkleRange` is the peer anti-entropy digest, this the per-entry authenticity audit — two altitudes of the one Merkle discipline, never a second hasher).

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AttestedEntry(UInt128 ContentKey, Option<UInt128> Prior, UInt128 Chain, Option<SignedAuthorship> Authorship, Instant At);

// The balanced Merkle tree over the chain's rolling addresses — Levels[0] the leaves (one per entry's
// Chain address), each higher level the pairwise XxHash128 of its children (a lone right child carried
// up), Levels[^1] the single audit-head root. The transparency-log structure the inclusion/consistency
// proofs descend, composing the one XxHash128 the rolling chain already uses.
public readonly record struct MerkleAudit(Seq<Seq<UInt128>> Levels, int Leaves) {
    public UInt128 Root => Levels.Last.Bind(static top => top.Head).IfNone(UInt128.Zero);
}

// The sibling-hash path proving one leaf's membership in the audit head — the auditor re-folds the
// (sibling, isLeftSibling) pairs from the leaf to the root and compares against the published head.
public readonly record struct InclusionProof(int Leaf, int Size, Seq<(UInt128 Sibling, bool Left)> Path);

// Proof that a newer head append-only-extends an older one — the older root plus the frontier nodes the
// auditor folds to confirm the older root is a prefix of the newer (no rewrite of any sealed entry).
public readonly record struct ConsistencyProof(int OldSize, int NewSize, UInt128 OldRoot, UInt128 NewRoot, Seq<UInt128> Frontier);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttestVerdict {
    private AttestVerdict() { }
    public sealed record Authentic(int Entries) : AttestVerdict;
    public sealed record Broken(int At, UInt128 Expected, UInt128 Found) : AttestVerdict;
    public sealed record Unsigned(int Entries) : AttestVerdict;
    public sealed record Unauthored(int At, OpDigest Expected, OpDigest Found) : AttestVerdict;
    public sealed record Forged(int At, Principal Actor) : AttestVerdict;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AttestedLedger {
    public static AttestedEntry Append(Option<AttestedEntry> prior, UInt128 contentKey, Option<SignedAuthorship> authorship) {
        var rolling = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        prior.Iter(p => { BinaryPrimitives.WriteUInt128LittleEndian(word, p.Chain); rolling.Append(word); });
        BinaryPrimitives.WriteUInt128LittleEndian(word, contentKey);
        rolling.Append(word);
        return new AttestedEntry(contentKey, prior.Map(static p => p.Chain), rolling.GetCurrentHashAsUInt128(), authorship, authorship.Map(static a => a.At).IfNone(Instant.MinValue));
    }

    // Verification COMPOSES Element/identity#AUTHORITY Authority.Verify over the resolved keyring — the
    // SAME KMS dispatch that gates every signed op, so the chain folds the AuthDecision arms (Authentic/
    // Forged/Unauthored/Unsigned) it returns into one AttestVerdict, never a hand-rolled bool predicate.
    // `digestOf` RE-DERIVES the expected OpDigest from the entry's actual content (the bytes the ContentKey
    // addresses, re-hashed under the authorship's SigningAlgorithm) so `Authority.Verify` compares the SIGNED
    // digest against an INDEPENDENT recomputation — passing `authorship.Digest` as both sides would make the
    // Unauthored arm (digest-does-not-bind-content) structurally unreachable, the illusory-verify deleted form.
    public static IO<AttestVerdict> Verify(Seq<AttestedEntry> chain, Func<SignedAuthorship, SigningKeyring> keyringFor, Func<AttestedEntry, OpDigest> digestOf) =>
        chain.FoldM(
            (State: Option<AttestedEntry>.None, Verdict: (AttestVerdict)new AttestVerdict.Authentic(0), Index: 0, Signed: 0),
            (acc, entry) => {
                var recomputed = Append(acc.State, entry.ContentKey, entry.Authorship);
                return recomputed.Chain != entry.Chain || acc.State.Map(static s => s.Chain) != entry.Prior
                    ? IO.pure((Some(entry), (AttestVerdict)new AttestVerdict.Broken(acc.Index, recomputed.Chain, entry.Chain), acc.Index + 1, acc.Signed))
                    : entry.Authorship.Match(
                        Some: authorship => Authority.Verify(authorship, digestOf(entry), keyringFor(authorship)).Map(decision => decision switch {
                            AuthDecision.Authentic => (Some(entry), acc.Verdict, acc.Index + 1, acc.Signed + 1),
                            AuthDecision.Unauthored u => (Some(entry), (AttestVerdict)new AttestVerdict.Unauthored(acc.Index, u.Expected, u.Found), acc.Index + 1, acc.Signed),
                            AuthDecision.Forged f => (Some(entry), new AttestVerdict.Forged(acc.Index, f.Actor), acc.Index + 1, acc.Signed),
                            _ => (Some(entry), acc.Verdict, acc.Index + 1, acc.Signed),
                        }),
                        None: () => IO.pure((Some(entry), acc.Verdict, acc.Index + 1, acc.Signed)));
            })
            .Map(final => final.Verdict is AttestVerdict.Authentic ? (final.Signed == 0 && chain.Count > 0 ? new AttestVerdict.Unsigned(chain.Count) : new AttestVerdict.Authentic(chain.Count)) : final.Verdict).As();

    public static MerkleAudit Seal(Seq<AttestedEntry> chain) {
        Seq<UInt128> leaves = chain.Map(static e => e.Chain);
        Seq<Seq<UInt128>> levels = Seq(leaves);
        for (var level = leaves; level.Count > 1; level = levels.Last.IfNone(level))
            levels = levels.Add(toSeq(level.AsEnumerable().Chunk(2).Select(static pair => pair.Length == 2 ? Pair(pair[0], pair[1]) : pair[0])));
        return new MerkleAudit(leaves.IsEmpty ? Seq(Seq<UInt128>()) : levels, chain.Count);
    }

    public static Option<InclusionProof> Prove(MerkleAudit audit, int leaf) =>
        leaf < 0 || leaf >= audit.Leaves ? None : Some(audit.Levels.Take(audit.Levels.Count - 1).Fold(
            (Index: leaf, Path: Seq<(UInt128, bool)>()),
            (acc, level) => (acc.Index >> 1, (acc.Index ^ 1) < level.Count
                ? acc.Path.Add((level[acc.Index ^ 1], (acc.Index & 1) == 1))
                : acc.Path)) is var folded ? new InclusionProof(leaf, audit.Leaves, folded.Path) : default);

    public static bool Includes(InclusionProof proof, UInt128 leaf, UInt128 root) =>
        proof.Path.Fold(leaf, static (acc, step) => step.Left ? Pair(step.Sibling, acc) : Pair(acc, step.Sibling)) == root;

    public static ConsistencyProof Extend(MerkleAudit older, MerkleAudit newer) =>
        new(older.Leaves, newer.Leaves, older.Root, newer.Root, older.Levels.Bind(static level => level.Last.ToSeq()));

    // The append-only check (the ledger is a hash chain, so consistency reduces to leaf-prefix equality):
    // re-sealing the newer audit's first OldSize leaves must reproduce OldRoot AND the newer audit must
    // re-seal to NewRoot — a newer tree shorter than the older, or a prefix that does not reproduce the
    // old root, is a rewrite the proof rejects. `Frontier` is the older tree's right-edge witness the
    // operator publishes and an offline auditor compares against its cached head, never folded here.
    public static bool Consistent(ConsistencyProof proof, MerkleAudit newer) =>
        proof.NewSize >= proof.OldSize
        && newer.Root == proof.NewRoot
        && (proof.OldSize == 0 || Reseal(newer.Levels.Head.IfNone(Seq<UInt128>()).Take(proof.OldSize)).Root == proof.OldRoot);

    // Re-seal a leaf prefix into its Merkle audit — the inverse the consistency check folds the prefix
    // through, composing the one `Seal` pairing over synthetic chain-address leaves.
    static MerkleAudit Reseal(Seq<UInt128> leaves) => Seal(leaves.Map(static leaf => new AttestedEntry(default, None, leaf, None, Instant.MinValue)));

    static UInt128 Pair(UInt128 left, UInt128 right) {
        var node = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128LittleEndian(word, left); node.Append(word);
        BinaryPrimitives.WriteUInt128LittleEndian(word, right); node.Append(word);
        return node.GetCurrentHashAsUInt128();
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | tamper-evidence     | hash-chained + KMS-signed              | the one authenticity authority; checkpoint chain defers   |
|  [02]   | chain break         | rolling-address/back-link discontinuity | any insert/delete/reorder breaks every downstream address |
|  [03]   | signature verdict   | `Authority.Verify` over `digestOf(entry)` → `AuthDecision` | the one KMS dispatch; `Unauthored` reachable (recomputed vs signed digest), never self-compared |
|  [04]   | audit proof         | Merkle `InclusionProof`/`ConsistencyProof` | a third party audits one entry without the whole chain |
|  [05]   | Merkle hasher       | the one `XxHash128`                    | the `MerkleRange` peer digest is the other altitude       |

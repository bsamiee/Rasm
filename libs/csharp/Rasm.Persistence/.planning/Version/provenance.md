# [PERSISTENCE_VERSION_PROVENANCE]

`CausalDag` derives W3C-PROV-O lineage from the changefeed and commit DAG, using separate op-key containment and commit-key resolution. `ProvNode`, `ProvClass`, `ProvRelation`, and `ProvRole` close the standards vocabulary; the derived edges seat in ONE `BidirectionalGraph` the bounded walks read; `ProvJson` owns the wire projection. `AttestedLedger` binds content order, KMS authorship, Merkle inclusion, append-only consistency, and externally witnessed heads under one authenticity surface.

Lineage rides the one `Version/ledger#CHANGEFEED` and the one `Element/codec#CONTENT_ADDRESS`, never a second store. `Element/graph#STORE_RAIL` supplies `StoreActor` (the bundle asserter and `SignedAuthorship.Actor`); `Element/identity#KMS_CUSTODY` supplies `SignedAuthorship`, `OpDigest`, `SigningKeyring`, and the `Custody`/`CustodyVerdict` probe; `Version/commits` supplies `Hlc` and `CommitNode`; the injected `ProjectionContext` frame supplies clock, correlation, and tenant; and the sidecar projects each completed Pollination run as a `CloudRunFact` value.

## [01]-[INDEX]

- [02]-[CAUSAL_DAG]: `CausalDag` derives exact lineage from the changefeed and the cloud-run seam over the closed W3C-PROV-O node/relation vocabulary, seats it in one QuikGraph container the bounded ancestry/descent walks read, and projects the PROV-JSON bundle egress.
- [03]-[ATTESTED_LEDGER]: `AttestedLedger` hash-chains and KMS-signs every entry, verifies through `Custody`, seals the Merkle inclusion/consistency audit proofs, and owns tamper evidence.

## [02]-[CAUSAL_DAG]

- Owner: `ProvClass` the `[SmartEnum<string>]` PROV-O base class every node carries — the top-level PROV-JSON map key beside the class IRI, so one row answers both; `ProvKind` the `[SmartEnum<string>]` node-kind axis carrying its `ProvClass` and the `AssociationRole` an agent associated with this activity kind plays; `AgentClass` the `[SmartEnum<string>]` PROV-O agent-subtype axis (`Person`/`SoftwareAgent`/`Organization`); `ProvRole` the `[SmartEnum<string>]` qualified-influence role vocabulary the association carries; `ProvNode` the `[Union]` causal node (`Entity`/`Activity`/`Agent`) whose `Entity` refines into the `EntitySubclass` PROV form (`Plain`/`Collection`/`Bundle`/`Plan`); `ProvRelation` the `[SmartEnum<string>]` influence vocabulary carrying its PROV-O term, its endpoint-property law (the W3C-PROV-JSON edge map property names), and the derivation-subclass parent it specializes; `ProvEndpoint` the `[SmartEnum<string>]` from/to property-name axis the JSON projection reads; `ProvEdge` the typed causal edge, itself the container's `IEdge<UInt128>`, carrying its qualified `ProvRole`/plan; `WalkDirection` the two-row incidence axis the container projection dials; `LineageWalk` the bounded walk request; `CloudRunFact` the sidecar-projected completed-cloud-run value and `AcquireFact` the neural-acquisition value (model card + licence + artefact digest as the plan triple) the second and third `Derive` modalities fold; `CausalDag` the static surface owning lineage derivation (changefeed, cloud-run, AND acquisition modalities), the lineage container, the walks, and the PROV-JSON projection.
- Cases: `ProvNode` is `Entity(ContentAddress, ProvKind, EntitySubclass, Instant)`, `Activity(UInt128 Id, ProvKind Kind, Instant Started, Instant Ended)`, `Agent(string Actor, AgentClass Class, Option<UInt128> Attested)` — the agent identity is the stable actor SUBJECT string (the one identifier the changefeed `Actor` header and a `SignedAuthorship.Actor.Subject` both yield, never a full `StoreActor` frozen into a durable node — role claims are session facts), the CLASS is the PROV-O subtype derived through `AgentClass.Of(StoreActor)` off the actor's role claims, and the attestation rides ABSENCE carrying the attesting signature's own key, so a reader learns WHICH attestation stands rather than only that one did; `EntitySubclass` is `Plain | Collection | Bundle | Plan` (the PROV-O `Entity` subclasses — a model is a `Collection` of element entities, the lineage export a `Bundle`, a merge strategy a `Plan`); `ProvRelation` is the eleven W3C-PROV-O influence terms — `WasGeneratedBy | Used | WasInformedBy | WasDerivedFrom | WasRevisionOf | WasQuotedFrom | HadPrimarySource | WasInvalidatedBy | WasAttributedTo | WasAssociatedWith | ActedOnBehalfOf` — each a row carrying its `prov:` term, its endpoint-property law, and the derivation-subclass parent it specializes (`WasRevisionOf`/`WasQuotedFrom`/`HadPrimarySource` generalize to `WasDerivedFrom`), a twelfth being one row, never a parallel edge family; `AgentClass` is the three PROV-O agent subtypes; `ProvRole` is `Author | Reviser | Importer | Merger | Solver | Delegate` — the role an agent played in an association; `ProvKind` closes at ten rows — the four entity kinds (`Graph`/`Delta`/`Snapshot`/`Blob`) and the six activity kinds (`Commit`/`Merge`/`Import`/`Solve`/`CloudRun`/`Acquire`), the `CloudRun` and `Acquire` rows REUSING `ProvRole.Solver` because a cloud solver, a local solver, and a neural acquirer play the one role, and the `Acquire` row landing the model-card/licence/provider evidence the acquired-texture `Blob` retention class rests on.
- Entry: `public static Seq<ProvEdge> Derive(Seq<OpLogEntry> changefeed, Func<UInt128, Option<CommitNode>> containing, Func<UInt128, Option<CommitNode>> resolve)` projects the lineage graph from the changefeed and the commit DAG with EXACT PROV-O endpoint typing — `containing` resolves an entry's content key (an OP key under `CommitNode.OpKeys`, never the commit key) to its RECORDING commit activity while `resolve` walks commit keys to parents, two resolvers because the two key spaces are disjoint — each delta entity `WasGeneratedBy` its commit activity, the commit activity `WasAssociatedWith` its signed agent under the agent's `ProvRole`, the delta entity `WasAttributedTo` that agent, a revised entity `WasRevisionOf` each parent commit's produced op-key entities (the derivation subclass, resolved one hop through `resolve` — never the geometry `Closure`), a retired entity `WasInvalidatedBy` its commit activity, and a software agent `ActedOnBehalfOf` its delegating principal; `public static Seq<ProvEdge> Derive(CloudRunFact run)` is the SAME entry's cloud-run modality (input shape discriminates) — the completed run is a W3C-PROV `Activity`: `Used` each input-asset content key, each output-asset entity `WasGeneratedBy` the run activity and `WasAttributedTo` the service agent, the activity `WasAssociatedWith` the `SoftwareAgent` behind `Configuration.AccessToken` (`TokenRepo`) qualified `ProvRole.Solver` with `hadPlan` the recipe reference (`owner/name:tag` + the registry `PackageVersion.Digest`), and the service agent `ActedOnBehalfOf` the human subject who submitted, with `public static Seq<ProvEdge> Derive(AcquireFact run)` the acquisition modality of that same fold under `ProvKind.Acquire`; `public static BidirectionalGraph<UInt128, ProvEdge> Graph(Seq<ProvEdge> lineage)` seats the derived edges in the one lineage container; `public static Seq<ProvNode> Walk(LineageWalk walk, IBidirectionalGraph<UInt128, ProvEdge> lineage, Func<UInt128, (ProvKind, EntitySubclass)> kindOf)` runs ONE `BreadthFirstSearchAlgorithm` under a `VertexDistanceRecorderObserver`, the direction selecting the container view rather than branching per step; `public static Seq<ProvNode> Derivations(ContentAddress root, int depth, IBidirectionalGraph<UInt128, ProvEdge> lineage, Func<UInt128, (ProvKind, EntitySubclass)> kindOf)` composes `Walk` over a `FilteredBidirectionalGraph` scoped to the derivation family for the transitive `wasDerivedFrom` closure; `public static ProvBundle Bundle(Seq<ProvEdge> lineage, StoreActor authority, Option<SignedAuthorship> attestation, Instant at)` names the lineage as a PROV `Bundle` whose asserting `ProvNode.Agent` DERIVES its `AgentClass` off the actor's role claims (`AgentClass.Of`) and its attestation off the signature's own content key — the provenance-of-provenance header, never a hardcoded `Person`/unsigned pair — and mints the bundle id through `CanonicalWriter.Sorted`, which owns the canonical order; `public static JsonElement ProvJson(ProvBundle bundle, Func<UInt128, ProvNode> resolve)` projects the standards-conformant W3C-PROV-JSON document.
- Auto: lineage is DERIVED from the changefeed and the commit DAG, never a parallel provenance write — a delta IS the `WasGeneratedBy` evidence (delta entity → its RECORDING commit activity, resolved through the op-key→commit `containing` index), and the `Version/commits#COMMIT_DAG` parent commits' op-key entities (resolved one hop through `resolve`, the parent commit being an Activity whose produced entities are its `OpKeys`) ARE the `WasRevisionOf` sources, NEVER the `OpLogEntry.Closure` geometry-blob manifest, so the PROV graph is a fold over the events the system already holds; the association edge reads the `Element/identity#KMS_CUSTODY` `SignedAuthorship` so a `WasAssociatedWith` names a verified `Person`/`Organization` `Agent` when the op was KMS-signed and a `SoftwareAgent` (a Compute solver activity, an IFC importer, a Pollination cloud run) when an automated activity produced the entity, the automated agent `ActedOnBehalfOf` the human principal that triggered the run; the activity span reads the commit cell `Hlc` so the PROV `startedAtTime`/`endedAtTime` ride the one causal clock; the qualified `hadRole`/`hadPlan` of an association reads the activity's `ProvKind` (an `Import` activity's agent plays `Importer`, a `Merge` activity's plays `Merger`, a `CloudRun` activity's plays `Solver` with `hadPlan` the recipe-reference `Plan` entity) so the influence is role-qualified, never bare — `hadPlan` rides ONLY a modality that supplies a real `Plan` entity (`CommitNode` records no strategy entity, so the changefeed association carries no plan; an activity key cited as its own plan is a PROV typing contradiction, the deleted form); the cloud-run modality folds the SAME edge vocabulary from `CloudRunFact` values the sidecar projects — the SDK fork closure (`LBT.RestSharp`/`LBT.Newtonsoft.Json`) never loads here, and the attested verify makes the externally-computed result tamper-evident locally, the federation-wide verify template this page owns.
- Receipt: a lineage derivation rides `store.prov.derive` carrying the edge count by `ProvRelation`; an ancestry/descent walk rides `store.prov.walk` carrying the reached-node count, depth, and direction; a PROV-JSON bundle export rides `store.prov.export` carrying the bundle node and edge counts.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of<TState>` + `CanonicalWriter.String`/`U128`/`Sorted` — every durable key on this page mints through the one framed preimage, so no delimiter join and no caller-side sort sits beside the writer, [B]), QuikGraph (`GraphExtensions.ToBidirectionalGraph`, `BidirectionalGraph`, `ReversedBidirectionalGraph`, `FilteredBidirectionalGraph`, `IEdge<TVertex>`, `BreadthFirstSearchAlgorithm`, `VertexDistanceRecorderObserver`, `DistanceRelaxers.ShortestDistance`, `AlgorithmBase.Abort`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Text.Json (the `Element/codec#CODEC_AXIS` `ElementJson.Options` Thinktecture-converter set the PROV-JSON `SerializeToElement` egress composes, never a second converter registration), System.Collections.Frozen, PollinationSDK (sidecar seam only — `RunsApi.GetRunAsync` → `Run`/`RunStatusEnum`, `Configuration.AccessToken` → `TokenRepo`, `ArtifactsApi` asset landings project the `CloudRunFact` VALUES; no fence references the SDK), BCL inbox.
- Growth: a new PROV relation is one `ProvRelation` row carrying its term + endpoint law + derivation-subclass parent; a new node class is one `ProvNode`/`EntitySubclass`/`ProvKind` row; a new activity source is one `ProvKind` activity row with one `Derive` input modality (as `CloudRun`/`CloudRunFact` is); a new agent subtype is one `AgentClass` row; a new association role is one `ProvRole` row; a new walk direction is one `WalkDirection` row projecting the one container; zero new surface — a parallel provenance store, a hand frontier beside the container, a second incidence structure, an attribution edge mis-typed off an activity, or a free-string PROV term is the deleted form because the lineage is a fold over the changefeed and the PROV vocabulary is the closed W3C-PROV-O term set.
- Boundary: the causal DAG is DERIVED from the changefeed and the commit DAG — a delta entity's `WasGeneratedBy`, its commit's `WasAssociatedWith` agent, and the `WasRevisionOf` predecessor (each parent commit's produced op-key entities, resolved one hop, NEVER the `OpLogEntry.Closure` descendant-geometry manifest which is a blob set and not a version predecessor — keying revision off `Closure.Head` is the deleted defect) are all reads off the events, never a write of record, so the lineage is reconstructible from the one op stream a replica folds; the PROV-O typing is EXACT — an `Activity` binds an `Agent` through `WasAssociatedWith` and an `Entity` through `WasAttributedTo`, so an attribution edge sourced off an activity is the deleted defect, the derivation of a revised graph is the `WasRevisionOf` subclass not the generic `WasDerivedFrom`, and the `WasInformedBy` activity-to-activity chain captures a merge informed by its parents; the agent is a PROV-O subtype (`Person`/`SoftwareAgent`/`Organization`) so a derived `Assessment` result names its `SoftwareAgent` solver `ActedOnBehalfOf` the human `Person` who triggered it, never an anonymous machine write; a completed Pollination run is the SAME law at the cloud seam — the service principal behind the access token is the `SoftwareAgent`, the input/output asset content keys the blobstore landing minted are the `Used`/`WasGeneratedBy` entities, and the recipe reference is the `hadPlan` `Plan` entity, so a cloud result is attributable, plan-bound, and locally tamper-evident through the one attested ledger, never a loose file download; the W3C-PROV-JSON egress is a real standards document (top-level `prefix`/`entity`/`activity`/`agent`/`wasGeneratedBy`/`used`/`wasAssociatedWith`/`wasAttributedTo`/`wasDerivedFrom`/`actedOnBehalfOf` keyed maps with `prov:`-prefixed properties, the bundle registered under `entity` as a `prov:Bundle` — a top-level `bundle` STRING key is a schema violation, the deleted form), never a flat `from→to` edge dictionary, so a CDE consumer ingests it through any PROV-O toolchain; the bundle is a PROV `Bundle` carrying its own provenance-of-provenance so the export is itself attributable; the lineage graph is THIS page's container and answers version-causality over content addresses under the `ProvRelation` influence vocabulary, where `Query/topology#TOPOLOGY_VIEW` keys `NodeId` over `RelationshipKind` element relations inside one model version — two vertex spaces and two edge vocabularies, so one incidence structure asked both questions returns a reachability neither caller meant; the walk is bounded breadth-first over that container so the cost is linear in the reachable-edge count within the depth bound, the ceiling tripping the search's own `Abort()` at the first over-depth dequeue rather than filtering a fully expanded reachable set; the attribution reconciles with the `Version/timetravel#TIME_TRAVEL` `BlameRow` (the same `(Hlc, origin)` winner the convergence selected) so blame and provenance never disagree.

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

// `ProvEndpoint` rows the PROV-O endpoint law per influence term — the from/to property NAMES the W3C-PROV-JSON
// edge map emits (an EntityActivity generation carries prov:entity/prov:activity, an AgentAgent delegation
// prov:delegate/prov:responsible), so the JSON projection reads the property names off the relation's own
// endpoint row rather than a per-term switch.
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

// `ProvClass` owns the PROV-O base class: the top-level PROV-JSON map key AND the class IRI on one row. A kind's
// class was a `"prov:Activity"` string column compared as text to discriminate, and the JSON projection re-derived
// the same partition from node shape through a second literal set — one row now answers both reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProvClass {
    public static readonly ProvClass Entity = new("entity", "prov:Entity");
    public static readonly ProvClass Activity = new("activity", "prov:Activity");
    public static readonly ProvClass Agent = new("agent", "prov:Agent");
    public string ClassIri { get; }
    private ProvClass(string key, string classIri) : this(key) => ClassIri = classIri;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProvKind {
    public static readonly ProvKind Graph = new("graph", ProvClass.Entity, ProvRole.Author);
    public static readonly ProvKind Delta = new("delta", ProvClass.Entity, ProvRole.Author);
    public static readonly ProvKind Snapshot = new("snapshot", ProvClass.Entity, ProvRole.Author);
    public static readonly ProvKind Blob = new("blob", ProvClass.Entity, ProvRole.Author);
    public static readonly ProvKind Commit = new("commit", ProvClass.Activity, ProvRole.Author);
    public static readonly ProvKind Merge = new("merge", ProvClass.Activity, ProvRole.Merger);
    public static readonly ProvKind Import = new("import", ProvClass.Activity, ProvRole.Importer);
    public static readonly ProvKind Solve = new("solve", ProvClass.Activity, ProvRole.Solver);
    // `CloudRun` types a completed Pollination cloud run under the same Solver role a local solve plays — the
    // cloud/local split is deployment, never a second role vocabulary.
    public static readonly ProvKind CloudRun = new("cloud-run", ProvClass.Activity, ProvRole.Solver);
    // `Acquire` types a neural acquisition run whose recorded model card, licence class, and execution provider carry
    // every bit of evidence an acquired texture's Blob retention class rests on — a retired card or a drifted
    // provider makes those bytes unreproducible, and this row lands that fact durably.
    public static readonly ProvKind Acquire = new("acquire", ProvClass.Activity, ProvRole.Solver);
    // The PROV class discriminant a lineage-walk node mint reads, so a reached commit types Activity and never an
    // activity-kinded Entity the JSON class map would mis-file.
    public ProvClass Class { get; }
    // `AssociationRole` names the role an agent associated with this activity kind played, qualifying the
    // WasAssociatedWith influence — an Import activity's agent is the Importer, a Merge's the Merger, and a
    // Solve's the Solver.
    public ProvRole AssociationRole { get; }
    private ProvKind(string key, ProvClass cls, ProvRole role) : this(key) => (Class, AssociationRole) = (cls, role);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AgentClass {
    public static readonly AgentClass Person = new("person", "prov:Person");
    public static readonly AgentClass SoftwareAgent = new("software", "prov:SoftwareAgent");
    public static readonly AgentClass Organization = new("organization", "prov:Organization");
    public string ClassIri { get; }
    // `Of` DERIVES the agent class off the actor's role claims — the AppHost port maps its principal kind onto a
    // role claim spelled as an `AgentClass` key ("software"/"organization"), so a service principal or an org
    // asserter classes correctly and the unclaimed default is `Person`; a hardcoded class is the deleted form.
    public static AgentClass Of(StoreActor actor) => Items.Find(cls => actor.Roles.Contains(cls.Key)).IfNone(Person);
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

// `ProvRelation` closes the W3C-PROV-O influence vocabulary: each row carries its prov: term, its endpoint law,
// and the derivation-subclass parent it specializes (so a WasRevisionOf ancestry resolves to the generic
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
    // `Family` is the influence family HEAD — the generic term a specialization rolls up to, itself where none. A
    // derivation-scoped lineage filters on `Family == WasDerivedFrom`, so a twelfth specialization joins by its own
    // `generic` column and no predicate anywhere names it; a per-family bool would need one member per family.
    public ProvRelation Family => GeneralizesTo.IfNone(this);
    private ProvRelation(string key, string term, ProvEndpoint endpoint, Option<ProvRelation> generic) : this(key) => (Term, Endpoint, GeneralizesTo) = (term, endpoint, generic);
}

// Direction is a CONTAINER decision, never a per-step branch: descent reads the lineage graph's own incidence and
// ancestry the zero-copy `ReversedBidirectionalGraph` view over that same container, so one frontier body serves
// both and no fold re-derives which endpoint is the influencing one.
[SmartEnum]
public sealed partial class WalkDirection {
    public static readonly WalkDirection Ancestry = new();
    public static readonly WalkDirection Descent = new();
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvNode {
    private ProvNode() { }
    public sealed record Entity(ContentAddress Address, ProvKind Kind, EntitySubclass Subclass, Instant At) : ProvNode;
    public sealed record Activity(UInt128 Id, ProvKind Kind, Instant Started, Instant Ended) : ProvNode;
    // `Attested` names WHICH attestation stands, absence spelling unsigned — a bare flag separated neither the two
    // states nor the two attestations a re-signed entry carries.
    public sealed record Agent(string Actor, AgentClass Class, Option<UInt128> Attested) : ProvNode;

    // Mint the CORRECT PROV class from the kind's own row: an Activity kind (Commit/Merge/Import/Solve/CloudRun/
    // Acquire) is an Activity node, an entity kind (Graph/Delta/Snapshot/Blob) an Entity — so a reached COMMIT in a
    // lineage walk is never mis-minted as an activity-kinded Entity, a PROV-O typing contradiction the JSON class
    // map would mis-file.
    // The parameter IS the `kindOf` resolver's own return shape, so a reached vertex resolves once and the pair
    // travels intact rather than being split at the call and re-paired here.
    public static ProvNode Of(ContentAddress address, (ProvKind Kind, EntitySubclass Subclass) row) =>
        row.Kind.Class == ProvClass.Activity
            ? new Activity(address.Value, row.Kind, Instant.MinValue, Instant.MinValue)
            : new Entity(address, row.Kind, row.Subclass, Instant.MinValue);

    public UInt128 Identity => Switch(
        entity: static e => e.Address.Value,
        activity: static a => a.Id,
        agent: static g => CausalDag.AgentKey(g.Actor));

    // The PROV-JSON top-level map key: read off the node's own class row, never a second literal set beside it.
    public ProvClass Class => Switch(
        entity: static _ => ProvClass.Entity,
        activity: static _ => ProvClass.Activity,
        agent: static _ => ProvClass.Agent);

    // The REFINED class IRI a node publishes — a Collection, Bundle, or Plan entity and a Person or Organization
    // agent each narrow their base class, so the member reads the refinement and the base row only where none exists.
    public string ClassIri => Switch(
        entity: static e => e.Subclass.ClassIri,
        activity: static a => a.Kind.Class.ClassIri,
        agent: static g => g.Class.ClassIri);
}

// `ProvEdge` carries one qualified causal edge — the PROV term, the endpoints, the HLC cell, the qualified-influence
// role (hadRole), and the optional plan (hadPlan) an association carries. The influence endpoints ARE the graph
// endpoints, so this value IS the lineage container's edge and no second edge type mirrors it: `Source`/`Target`
// name QuikGraph's contract while `From`/`To` name the PROV endpoint law the JSON projection reads.
public readonly record struct ProvEdge(ProvRelation Relation, UInt128 From, UInt128 To, Hlc Cell, Option<ProvRole> Role, Option<UInt128> Plan) : IEdge<UInt128> {
    public UInt128 Source => From;
    public UInt128 Target => To;
    public static ProvEdge Of(ProvRelation relation, UInt128 from, UInt128 to, Hlc cell) => new(relation, from, to, cell, None, None);
    public ProvEdge Qualified(ProvRole role, Option<UInt128> plan) => this with { Role = Some(role), Plan = plan };
}

// `LineageWalk` requests one bounded ancestry or descent — root, direction, and the depth ceiling the search respects.
public readonly record struct LineageWalk(ContentAddress Root, WalkDirection Direction, int Depth) {
    public static LineageWalk Ancestry(ContentAddress root, int depth) => new(root, WalkDirection.Ancestry, depth);
    public static LineageWalk Descent(ContentAddress root, int depth) => new(root, WalkDirection.Descent, depth);
}

// `ProvBundle` names a PROV Bundle — a set of provenance descriptions with its own provenance-of-provenance (who
// asserted the bundle, when), so the lineage export is itself an attributable PROV Entity. Its asserter is the
// DERIVED Agent node (class off the actor's role claims, attestation off the signature's own key), never a raw
// actor value the projection re-classifies.
public readonly record struct ProvBundle(UInt128 Id, Seq<ProvEdge> Lineage, ProvNode.Agent Asserter, Instant At);

// `CloudRunFact` carries the sidecar-projected completed cloud run: the SDK DTOs never cross this seam — the sidecar reads
// `RunsApi.GetRunAsync` -> `Run`/`RunStatusEnum`, resolves the service principal behind
// `Configuration.AccessToken` (`TokenRepo`), and hands over VALUES: the run id, the recipe plan reference
// (`owner/name:tag`) with its registry `PackageVersion.Digest`, and the input/output asset content keys the
// `Store/blobstore` landing minted.
public readonly record struct CloudRunFact(
    string RunId, string ServicePrincipal, string OnBehalfOf, string RecipeRef, string RecipeDigest,
    Seq<ContentAddress> Used, Seq<ContentAddress> Generated, Hlc Started, Hlc Ended);

// `AcquireFact` carries a completed neural-acquisition run as VALUES the composing seam projects off the producer's
// own provenance receipt (the Materials wire carries ModelCard/License/ModelArtefact as its Key(12)/(13)/(16)
// columns) — model card, licence, and artefact digest triple as the hadPlan Plan, the execution provider naming the
// software agent, and no Materials type crossing the S2 seam. This fact grounds an acquired texture's Blob retention
// class: the catalog's "unreproducible bytes" claim cites a landed activity.
public readonly record struct AcquireFact(
    string RunId, string Provider, string OnBehalfOf, string ModelCard, string License, string ModelArtefact,
    Seq<ContentAddress> Used, Seq<ContentAddress> Generated, Hlc Started, Hlc Ended);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CausalDag {
    // TWO resolvers with DISTINCT key spaces: `containing` maps an entry's content key (an OP key recorded under
    // CommitNode.OpKeys — the commit's own key only on the `commit` lane) to the commit that RECORDED it (the
    // op-key→commit index the composition root maintains off the commit lane); `resolve` maps a COMMIT content
    // key to its node (the parent walk). Resolving an op key through the commit-key resolver returns None for
    // every committed delta and silently drops its generation/association/revision edges — the deleted defect.
    public static Seq<ProvEdge> Derive(Seq<OpLogEntry> changefeed, Func<UInt128, Option<CommitNode>> containing, Func<UInt128, Option<CommitNode>> resolve) =>
        changefeed.Bind(entry => {
            Option<CommitNode> node = containing(entry.ContentKey);
            Option<UInt128> commit = node.Map(static c => c.ContentKey);
            // `CommitNode.IsMerge` decides the association role off the COMMIT activity: a multi-parent commit IS a
            // merge, so its agent plays Merger; an ordinary commit's plays Author. The role reads
            // off the activity, never the entity kind; no plan rides here — CommitNode records no Plan entity.
            ProvKind activity = node.Map(static c => c.IsMerge ? ProvKind.Merge : ProvKind.Commit).IfNone(ProvKind.Commit);
            UInt128 agent = AgentKey(entry.Actor);
            // EXACT PROV-O: the delta/snapshot Entity WasGeneratedBy its commit Activity; the Activity
            // WasAssociatedWith its Agent qualified by the activity's role; the produced Entity
            // WasAttributedTo that Agent; a merge Activity WasInformedBy each parent commit Activity; a
            // revised Entity WasRevisionOf its prior (the derivation subclass), a retired one WasInvalidatedBy
            // its activity. No attribution is ever sourced off an activity (that is association).
            Seq<ProvEdge> generated = commit.ToSeq().Map(c => ProvEdge.Of(ProvRelation.WasGeneratedBy, entry.ContentKey, c, entry.Stamp));
            Seq<ProvEdge> associated = commit.ToSeq().Map(c => ProvEdge.Of(ProvRelation.WasAssociatedWith, c, agent, entry.Stamp).Qualified(activity.AssociationRole, None));
            Seq<ProvEdge> attributed = Seq(ProvEdge.Of(ProvRelation.WasAttributedTo, entry.ContentKey, agent, entry.Stamp));
            // WasInformedBy is the MERGE chain only — ordinary succession already rides the entity-level WasRevisionOf,
            // so an activity-level edge per single-parent commit would restate it as a parallel lineage.
            Seq<ProvEdge> informed =
                from current in node.ToSeq()
                where current.IsMerge
                from activityKey in commit.ToSeq()
                from parent in current.Parents
                select ProvEdge.Of(ProvRelation.WasInformedBy, activityKey, parent, entry.Stamp);
            // Lineage rides the COMMIT-DAG, NEVER the `OpLogEntry.Closure` (which is the DESCENDANT GEOMETRY content-key
            // manifest — a blob set, not a predecessor). PROV-O endpoint typing is exact: a RETIRED entity `WasInvalidatedBy`
            // its retiring commit ACTIVITY (EntityActivity, entity->activity, so the target is the commit key); a REVISED
            // entity `WasRevisionOf` each PARENT-COMMIT'S delta ENTITY (EntityEntity, entity->entity — the parent commit is
            // an Activity, so the predecessor is its produced op-key entities, resolved one hop through `resolve`, NEVER the
            // parent commit key itself which would mistype an activity as the used entity). A root commit (no parents) emits
            // no revision edge — the genesis is generation-only.
            Seq<ProvEdge> lineage = entry.Kind.Tombstone
                ? commit.ToSeq().Map(activityKey => ProvEdge.Of(ProvRelation.WasInvalidatedBy, entry.ContentKey, activityKey, entry.Stamp))
                : from current in node.ToSeq()
                  from parent in current.Parents
                  from predecessor in resolve(parent).ToSeq()
                  from priorEntity in predecessor.OpKeys
                  select ProvEdge.Of(ProvRelation.WasRevisionOf, entry.ContentKey, priorEntity, entry.Stamp);
            return generated + associated + attributed + informed + lineage;
        });

    // `Derive(CloudRunFact)` and `Derive(AcquireFact)` are the RUN modalities of the ONE lineage derivation — input
    // shape discriminates, never a sibling name. Each run is a PROV Activity keyed off its run id; the SoftwareAgent
    // behind the credential associates qualified Solver with hadPlan the plan entity its own evidence keys; the asset
    // content keys the blobstore landing minted are the Used/WasGeneratedBy entities; the agent delegates to the
    // submitter. Both modalities emit the SAME five edge families, so the shape folds once and each overload supplies
    // only what its seam knows: the activity kind, the agent's identifier, and the plan's evidence segments.
    public static Seq<ProvEdge> Derive(CloudRunFact run) =>
        Run(ProvKind.CloudRun, run.RunId, run.ServicePrincipal, run.OnBehalfOf,
            Seq(run.RecipeRef, run.RecipeDigest), run.Used, run.Generated, run.Started, run.Ended);

    // Model card, licence class, and artefact digest key the hadPlan Plan entity, so "which card produced these
    // bytes" is a lineage read, and the execution provider names the software agent delegating to the human
    // submitter — the evidence fold the acquired-texture Blob retention row rests on.
    public static Seq<ProvEdge> Derive(AcquireFact run) =>
        Run(ProvKind.Acquire, run.RunId, run.Provider, run.OnBehalfOf,
            Seq(run.ModelCard, run.License, run.ModelArtefact), run.Used, run.Generated, run.Started, run.Ended);

    // The plan key mints through the kernel writer's COUNT-FRAMED row stream, so a two-segment recipe reference and a
    // three-segment model triple share one preimage law and no delimiter can collide two different segmentations
    // into one key — the defect a `$"{a}@{b}"` join carries by construction.
    private static Seq<ProvEdge> Run(
        ProvKind kind, string runId, string actor, string principal, Seq<string> plan,
        Seq<ContentAddress> used, Seq<ContentAddress> generated, Hlc started, Hlc ended) {
        UInt128 activity = ContentHash.Of(runId, static (id, writer) => writer.String(id));
        UInt128 agent = AgentKey(actor);
        UInt128 planKey = ContentHash.Of(plan, static (segments, writer) => writer.Rows(segments, static (segment, w) => w.String(segment)));
        return used.Map(key => ProvEdge.Of(ProvRelation.Used, activity, key.Value, started))
            + generated.Map(key => ProvEdge.Of(ProvRelation.WasGeneratedBy, key.Value, activity, ended))
            + generated.Map(key => ProvEdge.Of(ProvRelation.WasAttributedTo, key.Value, agent, ended))
            + Seq(
                ProvEdge.Of(ProvRelation.WasAssociatedWith, activity, agent, ended).Qualified(kind.AssociationRole, Some(planKey)),
                ProvEdge.Of(ProvRelation.ActedOnBehalfOf, agent, AgentKey(principal), ended));
    }

    // `Graph` is the ONE lineage container. `BidirectionalGraph` because ancestry reads predecessors and descent
    // successors off the same incidence, and `allowParallelEdges` because one ordered pair legitimately carries
    // several influence terms — a revised entity holds both its revision edge and any quotation or primary-source
    // edge to the same predecessor, and dropping the second silently narrows a derivation closure.
    // DISCRIMINANT against `Query/topology#TOPOLOGY_VIEW`: that container keys `NodeId` over `RelationshipKind`
    // element relations INSIDE one model version; this one keys content addresses over `ProvRelation` influence
    // ACROSS versions. Two vertex spaces, two edge vocabularies, so neither is the other's view.
    public static BidirectionalGraph<UInt128, ProvEdge> Graph(Seq<ProvEdge> lineage) =>
        lineage.ToBidirectionalGraph<UInt128, ProvEdge>(allowParallelEdges: true);

    // ONE walk, one observer, one product. Direction rides the CONTAINER — ancestry reads the zero-copy reversed
    // view, descent the container itself — so the frontier body is written once over whichever incidence it holds.
    public static Seq<ProvNode> Walk(LineageWalk walk, IBidirectionalGraph<UInt128, ProvEdge> lineage, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf) =>
        walk.Direction == WalkDirection.Ancestry
            ? Frontier(new ReversedBidirectionalGraph<UInt128, ProvEdge>(lineage), walk, kindOf)
            : Frontier(lineage, walk, kindOf);

    // The distance recorder supplies the hop depth off the SAME run that supplies the order, so the reached roster IS
    // the recorded distance map and no second traversal produces it. The ceiling trips the search's own `Abort()` at
    // the first over-depth DEQUEUE: breadth order discovers every node at depth d while dequeuing depth d-1, so
    // everything inside the bound is already recorded when the first depth-d vertex is examined — the bound stays a
    // COST bound rather than a filter over a fully expanded reachable set.
    private static Seq<ProvNode> Frontier<TEdge>(
        IVertexListGraph<UInt128, TEdge> graph, LineageWalk walk, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf)
        where TEdge : IEdge<UInt128> {
        BreadthFirstSearchAlgorithm<UInt128, TEdge> search = new(graph);
        VertexDistanceRecorderObserver<UInt128, TEdge> depths = new(DistanceRelaxers.ShortestDistance);
        // Exemption: an event handler is a `void` seam no expression can inhabit, and the abort is the whole body.
        search.ExamineVertex += examined => {
            if (depths.Distances.TryGetValue(examined, out double hops) && (hops >= walk.Depth)) { search.Abort(); }
        };
        using (depths.Attach(search)) { search.Compute(walk.Root.Value); }
        return toSeq(depths.Distances)
            .Filter(reached => (reached.Value <= walk.Depth) && (reached.Key != walk.Root.Value))
            .Map(reached => ProvNode.Of(ContentAddress.Of(reached.Key), kindOf(reached.Key)));
    }

    // Derivation-only ancestry — the transitive wasDerivedFrom/Revision/Quotation/PrimarySource closure (the PROV
    // "what did this derive from" query) scopes the SAME container through a filtered VIEW holding it by reference,
    // so nothing is copied and no second walker exists. The predicate reads the relation's family head, so a new
    // derivation specialization joins the closure with no edit here.
    public static Seq<ProvNode> Derivations(ContentAddress root, int depth, IBidirectionalGraph<UInt128, ProvEdge> lineage, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf) =>
        Walk(LineageWalk.Ancestry(root, depth),
            new FilteredBidirectionalGraph<UInt128, ProvEdge, IBidirectionalGraph<UInt128, ProvEdge>>(
                lineage, static _ => true, static edge => edge.Relation.Family == ProvRelation.WasDerivedFrom),
            kindOf);

    // `Bundle` derives the asserting Agent node HERE — class off the actor's role claims (`AgentClass.Of`),
    // attestation off the signature's own key — so the JSON projection reads a settled node and never re-classifies.
    // The bundle id mints through the kernel writer's `Sorted`, which OWNS the canonical order for a hash-keyed
    // container: a caller-side `OrderBy` beside a hand hasher both forked the framing and, on the string term,
    // ordered by the culture-sensitive default comparer, so two runtimes in different locales computed two ids for
    // one lineage. The order is stated ordinally here and the terms are length-framed by the writer.
    public static ProvBundle Bundle(Seq<ProvEdge> lineage, StoreActor authority, Option<SignedAuthorship> attestation, Instant at) =>
        new(Id: ContentHash.Of(lineage, static (edges, writer) => writer.Sorted(
                rows: edges,
                key: static edge => (edge.From, edge.To, edge.Relation.Key),
                order: EdgeOrder,
                field: static (edge, w) => w.U128(edge.From).U128(edge.To).String(edge.Relation.Key))),
            Lineage: lineage,
            Asserter: new ProvNode.Agent(
                authority.Subject,
                AgentClass.Of(authority),
                attestation.Map(static signed => ContentHash.Of(signed.Signature.Span))),
            At: at);

    private static readonly IComparer<(UInt128 From, UInt128 To, string Relation)> EdgeOrder =
        Comparer<(UInt128, UInt128, string)>.Create(static (left, right) =>
            left.Item1 != right.Item1 ? left.Item1.CompareTo(right.Item1)
            : left.Item2 != right.Item2 ? left.Item2.CompareTo(right.Item2)
            : string.CompareOrdinal(left.Item3, right.Item3));

    // `ProvJson` projects a standards-conformant W3C-PROV-JSON document: top-level prefix, per-class node maps
    // (entity/activity/agent), and per-relation influence maps, every node and edge keyed by its prov:-namespaced
    // id, every influence carrying its endpoint properties with the qualified prov:hadRole/prov:hadPlan — ingestible
    // by any PROV-O toolchain, never a flat from->to edge dictionary.
    public static JsonElement ProvJson(ProvBundle bundle, Func<UInt128, ProvNode> resolve) {
        static string Iri(UInt128 id) => $"rasm:{id:x32}";
        // `ProvBundle` is itself an attributable PROV `Bundle` entity, so it and its asserting Agent enter the SAME
        // node stream every lineage node takes rather than being patched into two already-built maps afterwards —
        // the patch form left `entity` and `agent` mandatory while `activity` was conditional, three shapes for one
        // fact. The bundle->asserter attribution rides one ordinary `WasAttributedTo` edge, never a top-level literal
        // a lineage attribution group would clobber. Top-level influence keys are the UNPREFIXED `ProvRelation.Key`
        // names the PROV-JSON schema fixes; the `prov:` prefix belongs to member properties alone.
        UInt128 authorityKey = AgentKey(bundle.Asserter.Actor);
        Seq<ProvEdge> edges = bundle.Lineage.Add(ProvEdge.Of(ProvRelation.WasAttributedTo, bundle.Id, authorityKey, new Hlc(bundle.At, 0UL)));
        Seq<(ProvClass Class, string Iri, object Members)> nodes =
            bundle.Lineage.Bind(static edge => Seq(edge.From, edge.To)).Distinct().Map(resolve)
                .Map(node => (node.Class, Iri(node.Identity), NodeMembers(node)))
                .Add((ProvClass.Entity, Iri(bundle.Id), BundleMembers(bundle)))
                .Add((ProvClass.Agent, Iri(authorityKey), NodeMembers(bundle.Asserter)));
        return JsonSerializer.SerializeToElement(
            nodes.GroupBy(static node => node.Class.Key)
                .Map(static byClass => (byClass.Key, Value: (object)byClass.ToFrozenDictionary(static n => n.Iri, static n => n.Members)))
                .Append(edges.GroupBy(static edge => edge.Relation.Key)
                    .Map(static byRelation => (byRelation.Key, Value: (object)byRelation
                        .Select(static (edge, ordinal) => (Key: $"_:e{ordinal}", Members: EdgeMembers(edge)))
                        .ToFrozenDictionary(static influence => influence.Key, static influence => influence.Members))))
                .Append(Seq((Key: "prefix", Value: (object)Prefixes)))
                .ToFrozenDictionary(static pair => pair.Key, static pair => pair.Value),
            ElementJson.Options);

        static object BundleMembers(ProvBundle bundle) => new Dictionary<string, object?> {
            ["prov:type"] = EntitySubclass.Bundle.ClassIri, ["prov:generatedAtTime"] = bundle.At.ToString(),
        };

        // `rasm:attestation` names WHICH attestation stands and its absence IS the unsigned fact, so the projection
        // publishes the evidence rather than a flag a consumer cannot trace back to a signature.
        static object NodeMembers(ProvNode node) => node.Switch(
            entity: static e => new Dictionary<string, object?> { ["prov:type"] = e.ClassIri, ["rasm:kind"] = e.Kind.Key, ["prov:generatedAtTime"] = e.At.ToString() },
            activity: static a => new Dictionary<string, object?> { ["prov:type"] = a.ClassIri, ["prov:startedAtTime"] = a.Started.ToString(), ["prov:endedAtTime"] = a.Ended.ToString() },
            agent: static g => new Dictionary<string, object?> {
                ["prov:type"] = g.ClassIri, ["rasm:id"] = g.Actor,
                ["rasm:attestation"] = g.Attested.Map(Iri).IfNoneUnsafe(() => null),
            });

        static object EdgeMembers(ProvEdge edge) => new Dictionary<string, object?> {
            [$"prov:{edge.Relation.Endpoint.FromProperty}"] = Iri(edge.From),
            [$"prov:{edge.Relation.Endpoint.ToProperty}"] = Iri(edge.To),
            ["rasm:atTime"] = edge.Cell.Physical.ToString(),
            ["prov:hadRole"] = edge.Role.Map(static role => role.Key).IfNoneUnsafe(() => null),
            ["prov:hadPlan"] = edge.Plan.Map(Iri).IfNoneUnsafe(() => null),
        };
    }

    // The two PROV-JSON namespace bindings the document fixes — one frozen row set, never re-minted per export.
    private static readonly FrozenDictionary<string, string> Prefixes = new Dictionary<string, string> {
        ["prov"] = "http://www.w3.org/ns/prov#", ["rasm"] = "urn:rasm:prov:",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    // `AgentKey` mints the agent key as the kernel seed-zero digest over the length-framed actor SUBJECT string
    // ([B] — a durable PROV node identity is a content-key mint, never a raw hasher call) — one stable actor identifier
    // reconstructible from BOTH the changefeed `OpLogEntry.Actor` header and a `SignedAuthorship.Actor.Subject`,
    // never a full `StoreActor` (role claims are session facts the bare changefeed actor string cannot reconstruct).
    internal static UInt128 AgentKey(string actor) => ContentHash.Of(actor, static (subject, writer) => writer.String(subject));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                            | [BINDING]                                                     |
| :-----: | :---------------- | :------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | lineage source    | derived from the changefeed + commit DAG           | never a parallel provenance write; never geometry `Closure`   |
|  [02]   | activity↔agent    | `WasAssociatedWith` (never `WasAttributedTo`)      | entity→agent is attribution, activity→agent is association    |
|  [03]   | revision source   | each parent commit's `OpKeys` entities (one hop)   | `WasRevisionOf` (EntityEntity); never `Closure.Head`          |
|  [04]   | agent typing      | `Person`/`SoftwareAgent`/`Organization`            | a solver `ActedOnBehalfOf` its human principal                |
|  [05]   | walk node typing  | `ProvNode.Of` on `kind.Class`                      | a reached commit is an Activity, never an Entity              |
|  [06]   | egress            | W3C-PROV-JSON bundle (`entity`/`activity`/`agent`) | a standards CDE artifact, never a flat edge dictionary        |
|  [07]   | lineage container | one `BidirectionalGraph` over content addresses    | direction is a container view; never an element-graph twin    |
|  [08]   | walk cost         | one BFS, one distance observer, `Abort` at ceiling | linear in reachable edges within the depth bound              |
|  [09]   | run lineage       | `Derive(CloudRunFact)`/`Derive(AcquireFact)`       | one edge fold; plan segments count-framed, never joined       |
|  [10]   | bundle asserter   | `AgentClass.Of(StoreActor)` + attestation key      | class/attestation DERIVED, never a hardcoded `Person`/`false` |
|  [11]   | durable key mints | `ContentHash.Of` through `CanonicalWriter`         | length-framed segments; `Sorted` owns the container order     |

- [09]-[RUN_LINEAGE]: `Used`/`Generated` = asset content keys; `hadPlan` = the run's own evidence segments.

## [03]-[ATTESTED_LEDGER]

- Owner: `AttestedEntry` the hash-chained, KMS-signed ledger row; `MerkleAudit` the per-head Merkle tree over the rolling addresses; `InclusionProof`/`ConsistencyProof` the third-party audit paths; `WitnessedHead` the KMS-signed tree-head publication row an EXTERNAL witness caches; `AttestVerdict` the closed chain-validity verdict; `AttestedLedger` the static surface owning the chain append, the rolling-address fold, the Merkle head seal, the audit-proof projections, the `Witness`/`Corroborate` external-witness pair, and the `Custody`-composed chain verification that is the one tamper-evidence authority.
- Cases: an `AttestedEntry` carries the entry content key, the `Prior` back-link, the rolling `Chain` address (`XxHash128` over `Prior ++ ContentKey ++ authorship signature ++ attestation instant` — authorship BINDS the address, so a signature or time rewrite moves every downstream address and the audit root), and the optional `SignedAuthorship`; `AttestVerdict` is `Authentic | Broken(at) | Unsigned | Mixed(signed, unsigned) | Unauthored(at) | Forged(at) | CustodyRejected(at, cause)` — `Authentic` the verified chain, `Broken` a back-link/rolling-address discontinuity, `Unsigned` a local-tier chain with no KMS signature, `Unauthored` a signed entry whose `OpDigest` does not bind its content (the `CustodyVerdict.Unauthored` arm), `Forged` a signature that fails KMS verification (the `CustodyVerdict.Forged` arm), `CustodyRejected` every remaining non-authentic custody arm (`DigestWidth`/`UnsupportedAlgorithm`/`AlgorithmMismatch` — a custody rejection can NEVER finalize `Authentic`), `Mixed` a chain carrying both signed and unsigned entries (partial custody is its own verdict, never an `Authentic` masquerade); the verdict cases mirror the `Element/identity#KMS_CUSTODY` `CustodyVerdict` cryptographic arms so verification never re-classifies what `Custody.Verify` already decided.
- Entry: `public static AttestedEntry Append(Option<AttestedEntry> prior, UInt128 contentKey, Option<SignedAuthorship> authorship)` extends the chain with the new rolling address; `public static IO<AttestVerdict> Verify(Seq<AttestedEntry> chain, Func<SignedAuthorship, SigningKeyring> keyringFor, Func<AttestedEntry, OpDigest> digestOf)` re-folds the chain, confirms every back-link and rolling address, and runs `Custody.Verify` over the per-entry resolved keyring with the INDEPENDENTLY recomputed expected digest (so `Unauthored` — a signature over a digest that does not bind the entry's content — is reachable, never self-compared against the stored digest); `public static MerkleAudit Seal(Seq<AttestedEntry> chain)` folds the rolling addresses into a balanced Merkle tree whose root is the audit head; `public static Option<InclusionProof> Prove(MerkleAudit audit, int leaf)` projects the sibling-hash path proving one entry's membership; `public static bool Includes(InclusionProof proof, UInt128 leaf, UInt128 root)` re-folds the path to the root; `public static ConsistencyProof Extend(MerkleAudit older, MerkleAudit newer)` issues the proof the newer head append-only-extends the older; `public static bool Consistent(ConsistencyProof proof, MerkleAudit newer)` confirms it by re-sealing the newer's leaf prefix to the old root; `public static IO<WitnessedHead> Witness(MerkleAudit audit, Func<ReadOnlyMemory<byte>, IO<Option<SignedAuthorship>>> sign, Func<WitnessedHead, IO<OpLogEntry>> stamp, Instant at)` seals the KMS-signed tree head for publication BEYOND the store it audits — the threaded `stamp` (the `OpLog.Stamp` partial application) lands the head as one `attest`-lane `Version/ledger#CHANGEFEED` `OpLogEntry` (`Payload` the lane-codec-encoded `WitnessedHead`, `ContentKey` the kernel `ContentHash.Of` over the `WitnessedHead.Canonical` bytes) so the ordinary `Version/egress` pump drains it to the witness's sink at cadence with zero bespoke message envelope — and `public static bool Corroborate(WitnessedHead cached, MerkleAudit newer)` is the external witness's probe — a newly published audit must append-only-extend the head the witness cached, composed wholly over the one `Consistent` check.
- Auto: the ledger is the AUTHENTICITY authority distinct from the reproducibility chain — the `Version/timetravel#TIME_TRAVEL` `Checkpoint.Hash` is a non-cryptographic content chain that proves a checkpoint reproduces from the op stream, while THIS chain's `SignedAuthorship` proves the entry was authored by a verified actor and not rewritten; verification re-folds the rolling address over the back-links and routes each signed entry through `Custody.Verify` so the cryptographic verdict is the SAME KMS dispatch (`Authentic`/`Forged`/`Unauthored`/`Unsigned`) that gates every signed op, never a hand-rolled boolean; the Merkle head seals the rolling addresses so a third party verifies one entry's `InclusionProof` and an append-only `ConsistencyProof` between two heads without replaying the whole chain.
- Receipt: a chain append rides `store.attest.append`; a verification rides `store.attest.verify` carrying the verdict and the break locus when broken; an audit-proof projection rides `store.attest.prove` carrying the leaf index and the proof path length; a witnessed-head publication rides `store.attest.witness` carrying the root, the leaf count, and the delivering sink key.
- Packages: System.IO.Hashing (`XxHash128.Append`/`Clone`/`GetCurrentHashAsUInt128`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (the KMS verify rides `Element/identity#KMS_CUSTODY` `Custody.Verify` over the resolved `SigningKeyring`, never a direct provider call here).
- Growth: a new verdict is one `AttestVerdict` case mirroring a `CustodyVerdict` arm; a richer audit proof is one projection over `MerkleAudit`; zero new surface — a second tamper-evidence scheme, a hand-rolled signature check beside `Custody.Verify`, a Merkle-tree audit log built on a second hasher, or a content chain claiming authenticity is the deleted form because this ledger owns authenticity and the checkpoint chain owns reproducibility, two distinct concerns.
- Boundary: the attested ledger is the ONE tamper-evidence authority — the `Version/timetravel#TIME_TRAVEL` `Checkpoint` hash chain explicitly defers here and carries no authenticity claim, so a content chain standing in for tamper-evidence is the deleted form; the chain is hash-chained off the prior rolling address so any inserted, deleted, or reordered entry breaks every downstream address (a `Broken` verdict naming the discontinuity); the per-entry cryptographic verdict COMPOSES `Element/identity#KMS_CUSTODY` `Custody.Verify(authorship, digest, keyring)` with the `digest` being the INDEPENDENTLY recomputed expected digest off the entry's content (`digestOf`, never the stored `authorship.Digest` self-compared — that self-comparison makes `Unauthored` unreachable, the illusory-verify deleted form) — a verified `CustodyVerdict.Authentic` proves the actor and the order, an `Unsigned` chain (the local/Personal tier on `KmsProvider.None`) proves order only, an `Unauthored` names a signed entry whose digest does not bind its content (the recomputed digest differs from the signed one), and a `Forged` names the entry whose KMS signature fails, so a second boolean signature predicate beside the one `Custody` verifier is the deleted form; the Merkle audit head is the transparency-log discipline — `InclusionProof` lets an external auditor confirm one entry is in the ledger from its sibling path and root alone, and `ConsistencyProof` confirms a later head only appended (never rewrote) so a regulator audits a slice without the whole history; the Merkle tree composes the one `XxHash128` the rolling chain and the content address already use (the `Version/commits#COMMIT_DAG` `MerkleRange` is the peer anti-entropy digest, this the per-entry authenticity audit — two altitudes of the one Merkle discipline, never a second hasher); a signed head held ONLY inside the store it audits proves nothing against a compromised operator, so `Witness` publishes the KMS-signed head at cadence through one `Version/egress` sink to an independent residence (a peer store, a second cloud, a notarization endpoint — riding the `attest` changefeed lane, never a bespoke message envelope beside the one pump) and `Corroborate` lets that witness reject a rewritten history from its cached head alone — tamper evidence that holds against the store's own operator, the difference between self-audit and counterparty audit a multi-party construction contract demands.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AttestedEntry(UInt128 ContentKey, Option<UInt128> Prior, UInt128 Chain, Option<SignedAuthorship> Authorship, Instant At);

// `MerkleAudit` balances the Merkle tree over the chain's rolling addresses — Levels[0] the leaves (one per entry's
// Chain address), each higher level the pairwise XxHash128 of its children (a lone right child carried up),
// Levels[^1] the single audit-head root: the transparency-log structure the inclusion/consistency proofs descend,
// composing the one XxHash128 the rolling chain already uses.
public readonly record struct MerkleAudit(Seq<Seq<UInt128>> Levels, int Leaves) {
    public UInt128 Root => Levels.Last.Bind(static top => top.Head).IfNone(UInt128.Zero);
}

// `InclusionProof` carries the sibling-hash path proving one leaf's membership in the audit head — the auditor
// re-folds the siblings from the leaf to the root and compares against the published head. The side of each step is
// bit `level` of `Leaf` and is DERIVED from the level the sibling came from, never carried beside it: a stored side
// could disagree with the index it accompanies, and such a proof was malformed in every case the pair could express.
// The level also states WHICH rung a skipped odd tail omitted, which a flat sibling list could not.
public readonly record struct InclusionProof(int Leaf, int Size, Seq<(int Level, UInt128 Sibling)> Path);

// Proof coordinates for a full newer audit: the verifier re-seals its old-size leaf prefix and compares both roots.
public readonly record struct ConsistencyProof(int OldSize, int NewSize, UInt128 OldRoot, UInt128 NewRoot);

// `WitnessedHead` carries the externally witnessed tree head: root, leaf count, and the KMS signature over the
// canonical head bytes — published beyond the store so an independent witness caches it and rejects any rewrite. `Signature`
// is None on the local KmsProvider.None tier (order-only witness, the same Unsigned stance the chain carries).
public readonly record struct WitnessedHead(UInt128 Root, int Leaves, Option<SignedAuthorship> Signature, Instant At) {
    public static ReadOnlyMemory<byte> Canonical(UInt128 root, int leaves, Instant at) {
        ArrayBufferWriter<byte> buffer = new(28);
        Span<byte> span = buffer.GetSpan(28);
        BinaryPrimitives.WriteUInt128LittleEndian(span[..16], root);
        BinaryPrimitives.WriteInt32LittleEndian(span[16..20], leaves);
        BinaryPrimitives.WriteInt64LittleEndian(span[20..28], at.ToUnixTimeTicks());
        buffer.Advance(28);
        return buffer.WrittenMemory;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttestVerdict {
    private AttestVerdict() { }
    public sealed record Authentic(int Entries) : AttestVerdict;
    public sealed record Broken(int At, UInt128 Expected, UInt128 Found) : AttestVerdict;
    public sealed record Unsigned(int Entries) : AttestVerdict;
    public sealed record Unauthored(int At, OpDigest Expected, OpDigest Found) : AttestVerdict;
    public sealed record Forged(int At, StoreActor Actor) : AttestVerdict;
    public sealed record CustodyRejected(int At, CustodyVerdict Cause) : AttestVerdict;
    public sealed record Mixed(int SignedEntries, int UnsignedEntries) : AttestVerdict;

    public string Key => Map(
        authentic: static _ => "authentic", broken: static _ => "broken", unsigned: static _ => "unsigned",
        unauthored: static _ => "unauthored", forged: static _ => "forged",
        custodyRejected: static _ => "custody-rejected", mixed: static _ => "mixed");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AttestedLedger {
    public static AttestedEntry Append(Option<AttestedEntry> prior, UInt128 contentKey, Option<SignedAuthorship> authorship) {
        using XxHash128 rolling = new();
        Span<byte> word = stackalloc byte[16];
        prior.Iter(p => { BinaryPrimitives.WriteUInt128LittleEndian(word, p.Chain); rolling.Append(word); });
        using XxHash128 next = rolling.Clone();
        BinaryPrimitives.WriteUInt128LittleEndian(word, contentKey);
        next.Append(word);
        // Authorship BINDS the address: the signature bytes and the attestation instant fold into the rolling
        // hash, so a valid authorship or attestation-time rewrite moves every downstream chain address and the
        // Merkle audit root — a chain bound only to (prior, content key) is the rewritable deleted form.
        authorship.Iter(a => {
            next.Append(a.Signature.Span);
            BinaryPrimitives.WriteInt64LittleEndian(word[..8], a.At.ToUnixTimeTicks());
            next.Append(word[..8]);
        });
        return new AttestedEntry(contentKey, prior.Map(static p => p.Chain), next.GetCurrentHashAsUInt128(), authorship, authorship.Map(static a => a.At).IfNone(Instant.MinValue));
    }

    // Verification COMPOSES Element/identity#KMS_CUSTODY Custody.Verify over the resolved keyring — the
    // SAME KMS dispatch that gates every signed op, so the chain folds the CustodyVerdict arms (Authentic/
    // Forged/Unauthored/Unsigned) it returns into one AttestVerdict, never a hand-rolled bool predicate.
    // `digestOf` RE-DERIVES the expected OpDigest from the entry's actual content (the bytes the ContentKey
    // addresses, re-hashed under the authorship's SigningAlgorithm) so `Custody.Verify` compares the SIGNED
    // digest against an INDEPENDENT recomputation — passing `authorship.Digest` as both sides would make the
    // Unauthored arm (digest-does-not-bind-content) structurally unreachable, the illusory-verify deleted form.
    public static IO<AttestVerdict> Verify(Seq<AttestedEntry> chain, Func<SignedAuthorship, SigningKeyring> keyringFor, Func<AttestedEntry, OpDigest> digestOf) =>
        chain.FoldM(
            (State: Option<AttestedEntry>.None, Verdict: (AttestVerdict)new AttestVerdict.Authentic(0), Index: 0, Signed: 0, Unsigned: 0),
            (acc, entry) => {
                // FIRST-DEFECT-WINS: a later mismatch never overwrites the earliest break locus — the verdict slot
                // assigns only while still `Authentic`, so the receipt names the discontinuity the auditor replays from.
                AttestedEntry recomputed = Append(acc.State, entry.ContentKey, entry.Authorship);
                return (recomputed.Chain != entry.Chain) || (acc.State.Map(static s => s.Chain) != entry.Prior)
                    ? IO.pure((Some(entry), acc.Verdict is AttestVerdict.Authentic ? new AttestVerdict.Broken(acc.Index, recomputed.Chain, entry.Chain) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned))
                    : entry.Authorship.Match(
                        Some: authorship => Custody.Verify(authorship, digestOf(entry), keyringFor(authorship)).Map(decision => decision switch {
                            CustodyVerdict.Authentic => (Some(entry), acc.Verdict, acc.Index + 1, acc.Signed + 1, acc.Unsigned),
                            CustodyVerdict.Unauthored u => (Some(entry), acc.Verdict is AttestVerdict.Authentic ? (AttestVerdict)new AttestVerdict.Unauthored(acc.Index, u.Expected, u.Found) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned),
                            CustodyVerdict.Forged f => (Some(entry), acc.Verdict is AttestVerdict.Authentic ? new AttestVerdict.Forged(acc.Index, f.Actor) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned),
                            // EVERY remaining custody arm (DigestWidth, UnsupportedAlgorithm, AlgorithmMismatch, and any
                            // future case) is a non-authentic consequence — a custody rejection can never finalize Authentic.
                            _ => (Some(entry), acc.Verdict is AttestVerdict.Authentic ? new AttestVerdict.CustodyRejected(acc.Index, decision) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned),
                        }),
                        None: () => IO.pure((Some(entry), acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned + 1)));
            })
            // `Mixed` verdicts a partly signed chain on its own — partial custody never masquerades as Authentic.
            .Map(final => final.Verdict is AttestVerdict.Authentic
                ? (final.Signed == 0) && (chain.Count > 0) ? (AttestVerdict)new AttestVerdict.Unsigned(chain.Count)
                    : (final.Signed > 0) && (final.Unsigned > 0) ? new AttestVerdict.Mixed(final.Signed, final.Unsigned)
                    : new AttestVerdict.Authentic(chain.Count)
                : final.Verdict).As();

    public static MerkleAudit Seal(Seq<AttestedEntry> chain) {
        Seq<UInt128> leaves = chain.Map(static e => e.Chain);
        Seq<Seq<UInt128>> levels = Seq(leaves);
        for (Seq<UInt128> level = leaves; level.Count > 1; level = levels.Last.IfNone(level))
            levels = levels.Add(toSeq(level.AsEnumerable().Chunk(2).Select(static pair => pair.Length == 2 ? Pair(pair[0], pair[1]) : pair[0])));
        return new MerkleAudit(leaves.IsEmpty ? Seq(Seq<UInt128>()) : levels, chain.Count);
    }

    public static Option<InclusionProof> Prove(MerkleAudit audit, int leaf) =>
        (leaf < 0) || (leaf >= audit.Leaves)
            ? None
            : Some(new InclusionProof(leaf, audit.Leaves, audit.Levels.Take(audit.Levels.Count - 1)
                .Map((level, rung) => (Rung: rung, Index: leaf >> rung, Level: level))
                .Filter(static step => (step.Index ^ 1) < step.Level.Count)
                .Map(static step => (step.Rung, step.Level[step.Index ^ 1]))));

    // The side of each fold step is bit `Level` of the proof's own leaf index, so the verifier reconstructs the
    // pairing from the index it was handed instead of trusting a side the prover supplied.
    public static bool Includes(InclusionProof proof, UInt128 leaf, UInt128 root) =>
        proof.Path.Fold(leaf, (acc, step) => ((proof.Leaf >> step.Level) & 1) == 1
            ? Pair(step.Sibling, acc)
            : Pair(acc, step.Sibling)) == root;

    public static ConsistencyProof Extend(MerkleAudit older, MerkleAudit newer) =>
        new(older.Leaves, newer.Leaves, older.Root, newer.Root);

    // `Witness`/`Corroborate` pair the external witness. Witness signs the canonical head bytes through the SAME Element/identity
    // KMS custody lane that signs every op (the `sign` delegate; None on the local tier — order-only witness);
    // Corroborate is the witness's own probe over its CACHED head — it needs no stored older audit, because
    // consistency reduces to re-sealing the newer's leaf prefix against the cached root through the one
    // Consistent check.
    // `stamp` is the ledger `OpLog.Stamp` partial application: it lands the lane-codec-encoded head as ONE
    // `ColumnFamily.Attest` `OpLogEntry` (`ContentKey` the kernel `ContentHash.Of` over the canonical bytes),
    // so the changefeed and the ordinary egress pump observe and drain the publication — a Witness that only
    // returns the head has no durable producer, the deleted form.
    public static IO<WitnessedHead> Witness(MerkleAudit audit, Func<ReadOnlyMemory<byte>, IO<Option<SignedAuthorship>>> sign, Func<WitnessedHead, IO<OpLogEntry>> stamp, Instant at) =>
        sign(WitnessedHead.Canonical(audit.Root, audit.Leaves, at))
            .Map(signature => new WitnessedHead(audit.Root, audit.Leaves, signature, at))
            .Bind(head => stamp(head).Map(_ => head));

    public static bool Corroborate(WitnessedHead cached, MerkleAudit newer) =>
        Consistent(new ConsistencyProof(cached.Leaves, newer.Leaves, cached.Root, newer.Root), newer);

    // `Consistent` runs the append-only check (the ledger is a hash chain, so consistency reduces to leaf-prefix
    // equality): re-sealing the newer audit's first OldSize leaves must reproduce OldRoot AND the newer audit must
    // re-seal to NewRoot — a newer tree shorter than the older, or a prefix that does not reproduce the old root, is
    // a rewrite the proof rejects. `ConsistencyProof` carries only values this verifier consumes, and the newer
    // audit supplies the prefix leaves reproducing the cached root.
    public static bool Consistent(ConsistencyProof proof, MerkleAudit newer) =>
        (proof.OldSize >= 0)
        && (proof.NewSize == newer.Leaves)
        && (proof.NewSize >= proof.OldSize)
        && (newer.Root == proof.NewRoot)
        && ((proof.OldSize == 0) || (Reseal(newer.Levels.Head.IfNone(Seq<UInt128>()).Take(proof.OldSize)).Root == proof.OldRoot));

    // Re-seal a leaf prefix into its Merkle audit — the inverse the consistency check folds the prefix
    // through, composing the one `Seal` pairing over synthetic chain-address leaves.
    static MerkleAudit Reseal(Seq<UInt128> leaves) => Seal(leaves.Map(static leaf => new AttestedEntry(default, None, leaf, None, Instant.MinValue)));

    static UInt128 Pair(UInt128 left, UInt128 right) {
        using XxHash128 node = new();
        Span<byte> word = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128LittleEndian(word, left); node.Append(word);
        BinaryPrimitives.WriteUInt128LittleEndian(word, right); node.Append(word);
        return node.GetCurrentHashAsUInt128();
    }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                              | [BINDING]                                                 |
| :-----: | :---------------- | :--------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | tamper-evidence   | hash-chained + KMS-signed                            | the one authenticity authority; checkpoint chain defers   |
|  [02]   | chain break       | rolling-address/back-link discontinuity              | any insert/delete/reorder breaks every downstream address |
|  [03]   | signature verdict | `Custody.Verify(digestOf(entry))` → `CustodyVerdict` | one KMS dispatch; `Unauthored` (recompute vs signed)      |
|  [04]   | audit proof       | Merkle `InclusionProof`/`ConsistencyProof`           | a third party audits one entry without the whole chain    |
|  [05]   | Merkle hasher     | the one `XxHash128`                                  | the `MerkleRange` peer digest is the other altitude       |
|  [06]   | external witness  | `Witness` signed head + `Corroborate` probe          | published via one egress sink; holds against the operator |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

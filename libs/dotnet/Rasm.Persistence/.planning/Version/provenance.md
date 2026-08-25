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
    public static readonly ProvKind CloudRun = new("cloud-run", ProvClass.Activity, ProvRole.Solver);
    public static readonly ProvKind Acquire = new("acquire", ProvClass.Activity, ProvRole.Solver);
    public ProvClass Class { get; }
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
    public ProvRelation Family => GeneralizesTo.IfNone(this);
    private ProvRelation(string key, string term, ProvEndpoint endpoint, Option<ProvRelation> generic) : this(key) => (Term, Endpoint, GeneralizesTo) = (term, endpoint, generic);
}

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
    public sealed record Agent(string Actor, AgentClass Class, Option<UInt128> Attested) : ProvNode;

    public static ProvNode Of(ContentAddress address, (ProvKind Kind, EntitySubclass Subclass) row) =>
        row.Kind.Class == ProvClass.Activity
            ? new Activity(address.Value, row.Kind, Instant.MinValue, Instant.MinValue)
            : new Entity(address, row.Kind, row.Subclass, Instant.MinValue);

    public UInt128 Identity => Switch(
        entity: static e => e.Address.Value,
        activity: static a => a.Id,
        agent: static g => CausalDag.AgentKey(g.Actor));

    public ProvClass Class => Switch(
        entity: static _ => ProvClass.Entity,
        activity: static _ => ProvClass.Activity,
        agent: static _ => ProvClass.Agent);

    public string ClassIri => Switch(
        entity: static e => e.Subclass.ClassIri,
        activity: static a => a.Kind.Class.ClassIri,
        agent: static g => g.Class.ClassIri);
}

public readonly record struct ProvEdge(ProvRelation Relation, UInt128 From, UInt128 To, Hlc Cell, Option<ProvRole> Role, Option<UInt128> Plan) : IEdge<UInt128> {
    public UInt128 Source => From;
    public UInt128 Target => To;
    public static ProvEdge Of(ProvRelation relation, UInt128 from, UInt128 to, Hlc cell) => new(relation, from, to, cell, None, None);
    public ProvEdge Qualified(ProvRole role, Option<UInt128> plan) => this with { Role = Some(role), Plan = plan };
}

public readonly record struct LineageWalk(ContentAddress Root, WalkDirection Direction, int Depth) {
    public static LineageWalk Ancestry(ContentAddress root, int depth) => new(root, WalkDirection.Ancestry, depth);
    public static LineageWalk Descent(ContentAddress root, int depth) => new(root, WalkDirection.Descent, depth);
}

public readonly record struct ProvBundle(UInt128 Id, Seq<ProvEdge> Lineage, ProvNode.Agent Asserter, Instant At);

public readonly record struct CloudRunFact(
    string RunId, string ServicePrincipal, string OnBehalfOf, string RecipeRef, string RecipeDigest,
    Seq<ContentAddress> Used, Seq<ContentAddress> Generated, Hlc Started, Hlc Ended);

public readonly record struct AcquireFact(
    string RunId, string Provider, string OnBehalfOf, string ModelCard, string License, string ModelArtefact,
    Seq<ContentAddress> Used, Seq<ContentAddress> Generated, Hlc Started, Hlc Ended);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CausalDag {
    public static Seq<ProvEdge> Derive(Seq<OpLogEntry> changefeed, Func<UInt128, Option<CommitNode>> containing, Func<UInt128, Option<CommitNode>> resolve) =>
        changefeed.Bind(entry => {
            Option<CommitNode> node = containing(entry.ContentKey);
            Option<UInt128> commit = node.Map(static c => c.ContentKey);
            ProvKind activity = node.Map(static c => c.IsMerge ? ProvKind.Merge : ProvKind.Commit).IfNone(ProvKind.Commit);
            UInt128 agent = AgentKey(entry.Actor);
            Seq<ProvEdge> generated = commit.ToSeq().Map(c => ProvEdge.Of(ProvRelation.WasGeneratedBy, entry.ContentKey, c, entry.Stamp));
            Seq<ProvEdge> associated = commit.ToSeq().Map(c => ProvEdge.Of(ProvRelation.WasAssociatedWith, c, agent, entry.Stamp).Qualified(activity.AssociationRole, None));
            Seq<ProvEdge> attributed = Seq(ProvEdge.Of(ProvRelation.WasAttributedTo, entry.ContentKey, agent, entry.Stamp));
            Seq<ProvEdge> informed =
                from current in node.ToSeq()
                where current.IsMerge
                from activityKey in commit.ToSeq()
                from parent in current.Parents
                select ProvEdge.Of(ProvRelation.WasInformedBy, activityKey, parent, entry.Stamp);
            Seq<ProvEdge> lineage = entry.Kind.Ops.Admits(SyncCapability.Tombstone)
                ? commit.ToSeq().Map(activityKey => ProvEdge.Of(ProvRelation.WasInvalidatedBy, entry.ContentKey, activityKey, entry.Stamp))
                : from current in node.ToSeq()
                  from parent in current.Parents
                  from predecessor in resolve(parent).ToSeq()
                  from priorEntity in predecessor.OpKeys
                  select ProvEdge.Of(ProvRelation.WasRevisionOf, entry.ContentKey, priorEntity, entry.Stamp);
            return generated + associated + attributed + informed + lineage;
        });

    public static Seq<ProvEdge> Derive(CloudRunFact run) =>
        Run(ProvKind.CloudRun, run.RunId, run.ServicePrincipal, run.OnBehalfOf,
            Seq(run.RecipeRef, run.RecipeDigest), run.Used, run.Generated, run.Started, run.Ended);

    public static Seq<ProvEdge> Derive(AcquireFact run) =>
        Run(ProvKind.Acquire, run.RunId, run.Provider, run.OnBehalfOf,
            Seq(run.ModelCard, run.License, run.ModelArtefact), run.Used, run.Generated, run.Started, run.Ended);

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

    public static BidirectionalGraph<UInt128, ProvEdge> Graph(Seq<ProvEdge> lineage) =>
        lineage.ToBidirectionalGraph<UInt128, ProvEdge>(allowParallelEdges: true);

    public static Seq<ProvNode> Walk(LineageWalk walk, IBidirectionalGraph<UInt128, ProvEdge> lineage, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf) =>
        walk.Direction == WalkDirection.Ancestry
            ? Frontier(new ReversedBidirectionalGraph<UInt128, ProvEdge>(lineage), walk, kindOf)
            : Frontier(lineage, walk, kindOf);

    private static Seq<ProvNode> Frontier<TEdge>(
        IVertexListGraph<UInt128, TEdge> graph, LineageWalk walk, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf)
        where TEdge : IEdge<UInt128> {
        BreadthFirstSearchAlgorithm<UInt128, TEdge> search = new(graph);
        VertexDistanceRecorderObserver<UInt128, TEdge> depths = new(DistanceRelaxers.ShortestDistance);
        search.ExamineVertex += examined => {
            if (depths.Distances.TryGetValue(examined, out double hops) && (hops >= walk.Depth)) { search.Abort(); }
        };
        using (depths.Attach(search)) { search.Compute(walk.Root.Value); }
        return toSeq(depths.Distances)
            .Filter(reached => (reached.Value <= walk.Depth) && (reached.Key != walk.Root.Value))
            .Map(reached => ProvNode.Of(ContentAddress.Of(reached.Key), kindOf(reached.Key)));
    }

    public static Seq<ProvNode> Derivations(ContentAddress root, int depth, IBidirectionalGraph<UInt128, ProvEdge> lineage, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf) =>
        Walk(LineageWalk.Ancestry(root, depth),
            new FilteredBidirectionalGraph<UInt128, ProvEdge, IBidirectionalGraph<UInt128, ProvEdge>>(
                lineage, static _ => true, static edge => edge.Relation.Family == ProvRelation.WasDerivedFrom),
            kindOf);

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

    public static JsonElement ProvJson(ProvBundle bundle, Func<UInt128, ProvNode> resolve) {
        static string Iri(UInt128 id) => $"rasm:{id:x32}";
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

    private static readonly FrozenDictionary<string, string> Prefixes = new Dictionary<string, string> {
        ["prov"] = "http://www.w3.org/ns/prov#", ["rasm"] = "urn:rasm:prov:",
    }.ToFrozenDictionary(StringComparer.Ordinal);

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
- Entry: `Append` extends the rolling chain; `Verify` re-folds it and composes `Custody.Verify` over independently recomputed digests; `Seal`, `Prove`, `Includes`, `Extend`, and `Consistent` own the Merkle audit proofs. `Witness(audit, sign, at)` seals and signs a head, while `Corroborate(cached, newer)` proves append-only extension. Publication belongs to a real application binding; this owner claims no `attest`-lane producer.
- Auto: the ledger is the AUTHENTICITY authority distinct from the reproducibility chain — the `Version/timetravel#TIME_TRAVEL` `Checkpoint.Hash` is a non-cryptographic content chain that proves a checkpoint reproduces from the op stream, while THIS chain's `SignedAuthorship` proves the entry was authored by a verified actor and not rewritten; verification re-folds the rolling address over the back-links and routes each signed entry through `Custody.Verify` so the cryptographic verdict is the SAME KMS dispatch (`Authentic`/`Forged`/`Unauthored`/`Unsigned`) that gates every signed op, never a hand-rolled boolean; the Merkle head seals the rolling addresses so a third party verifies one entry's `InclusionProof` and an append-only `ConsistencyProof` between two heads without replaying the whole chain.
- Receipt: a chain append rides `store.attest.append`; verification rides `store.attest.verify`; proof projection rides `store.attest.prove`. No publication receipt is claimed without a bound publisher.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of<TState>` + `CanonicalWriter.Optional`/`U128`/`Ordinal`/`I64`/`Raw` + `CanonicalWriter.Retaining`/`ToBytes` — the rolling chain, the Merkle pair, and the witnessed-head canonical bytes all on the one alphabet; `XxHash128` reaches no call site), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (the KMS verify rides `Element/identity#KMS_CUSTODY` `Custody.Verify` over the resolved `SigningKeyring`, never a direct provider call here).
- Growth: a new verdict is one `AttestVerdict` case mirroring a `CustodyVerdict` arm; a richer audit proof is one projection over `MerkleAudit`; zero new surface — a second tamper-evidence scheme, a hand-rolled signature check beside `Custody.Verify`, a Merkle-tree audit log built on a second hasher, or a content chain claiming authenticity is the deleted form because this ledger owns authenticity and the checkpoint chain owns reproducibility, two distinct concerns.
- Boundary: the attested ledger is the ONE tamper-evidence authority; the reproducibility checkpoint carries no authenticity claim. The rolling address exposes insertion, deletion, and reorder, while `Custody.Verify` alone distinguishes authentic, unsigned, unauthored, forged, and rejected custody. Inclusion and consistency proofs let an external auditor verify membership and append-only extension without replaying the chain. A signed head held only inside its own store proves nothing against that operator, so an application must bind publication to an independent residence; this package does not fabricate that binding or claim an `attest` op-log producer.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AttestedEntry(UInt128 ContentKey, Option<UInt128> Prior, UInt128 Chain, Option<SignedAuthorship> Authorship, Instant At);

public readonly record struct MerkleAudit(Seq<Seq<UInt128>> Levels, int Leaves) {
    public UInt128 Root => Levels.Last.Bind(static top => top.Head).IfNone(UInt128.Zero);
}

public readonly record struct InclusionProof(int Leaf, int Size, Seq<(int Level, UInt128 Sibling)> Path);

public readonly record struct ConsistencyProof(int OldSize, int NewSize, UInt128 OldRoot, UInt128 NewRoot);

public readonly record struct WitnessedHead(UInt128 Root, int Leaves, Option<SignedAuthorship> Signature, Instant At) {
    public static Fin<ReadOnlyMemory<byte>> Canonical(UInt128 root, int leaves, Instant at, Op key) =>
        CanonicalWriter.Retaining(EpsilonPolicy.ZeroTolerance).U128(root).Ordinal(leaves).I64(at.ToUnixTimeTicks()).ToBytes(key);
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
    public static AttestedEntry Append(Option<AttestedEntry> prior, UInt128 contentKey, Option<SignedAuthorship> authorship) =>
        new(contentKey, prior.Map(static p => p.Chain),
            ContentHash.Of((Prior: prior, Key: contentKey, Authorship: authorship), static (link, w) =>
                w.Optional(link.Prior, static (p, x) => { x.U128(p.Chain); })
                 .U128(link.Key)
                 .Optional(link.Authorship, static (a, x) => { x.Ordinal(a.Signature.Length).Raw(a.Signature.Span).I64(a.At.ToUnixTimeTicks()); })),
            authorship, authorship.Map(static a => a.At).IfNone(Instant.MinValue));

    public static IO<AttestVerdict> Verify(Seq<AttestedEntry> chain, Func<SignedAuthorship, SigningKeyring> keyringFor, Func<AttestedEntry, OpDigest> digestOf) =>
        chain.FoldM(
            (State: Option<AttestedEntry>.None, Verdict: (AttestVerdict)new AttestVerdict.Authentic(0), Index: 0, Signed: 0, Unsigned: 0),
            (acc, entry) => {
                AttestedEntry recomputed = Append(acc.State, entry.ContentKey, entry.Authorship);
                return (recomputed.Chain != entry.Chain) || (acc.State.Map(static s => s.Chain) != entry.Prior)
                    ? IO.pure((Some(entry), acc.Verdict is AttestVerdict.Authentic ? new AttestVerdict.Broken(acc.Index, recomputed.Chain, entry.Chain) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned))
                    : entry.Authorship.Match(
                        Some: authorship => Custody.Verify(authorship, digestOf(entry), keyringFor(authorship)).Map(decision => decision switch {
                            CustodyVerdict.Authentic => (Some(entry), acc.Verdict, acc.Index + 1, acc.Signed + 1, acc.Unsigned),
                            CustodyVerdict.Unauthored u => (Some(entry), acc.Verdict is AttestVerdict.Authentic ? (AttestVerdict)new AttestVerdict.Unauthored(acc.Index, u.Expected, u.Found) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned),
                            CustodyVerdict.Forged f => (Some(entry), acc.Verdict is AttestVerdict.Authentic ? new AttestVerdict.Forged(acc.Index, f.Actor) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned),
                            _ => (Some(entry), acc.Verdict is AttestVerdict.Authentic ? new AttestVerdict.CustodyRejected(acc.Index, decision) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned),
                        }),
                        None: () => IO.pure((Some(entry), acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned + 1)));
            })
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

    public static bool Includes(InclusionProof proof, UInt128 leaf, UInt128 root) =>
        proof.Path.Fold(leaf, (acc, step) => ((proof.Leaf >> step.Level) & 1) == 1
            ? Pair(step.Sibling, acc)
            : Pair(acc, step.Sibling)) == root;

    public static ConsistencyProof Extend(MerkleAudit older, MerkleAudit newer) =>
        new(older.Leaves, newer.Leaves, older.Root, newer.Root);

    public static IO<WitnessedHead> Witness(MerkleAudit audit, Func<ReadOnlyMemory<byte>, IO<Option<SignedAuthorship>>> sign, Instant at) =>
        IO.liftFin(WitnessedHead.Canonical(audit.Root, audit.Leaves, at, Op.Of()))
            .Bind(sign)
            .Map(signature => new WitnessedHead(audit.Root, audit.Leaves, signature, at));

    public static bool Corroborate(WitnessedHead cached, MerkleAudit newer) =>
        Consistent(new ConsistencyProof(cached.Leaves, newer.Leaves, cached.Root, newer.Root), newer);

    public static bool Consistent(ConsistencyProof proof, MerkleAudit newer) =>
        (proof.OldSize >= 0)
        && (proof.NewSize == newer.Leaves)
        && (proof.NewSize >= proof.OldSize)
        && (newer.Root == proof.NewRoot)
        && ((proof.OldSize == 0) || (Reseal(newer.Levels.Head.IfNone(Seq<UInt128>()).Take(proof.OldSize)).Root == proof.OldRoot));

    static MerkleAudit Reseal(Seq<UInt128> leaves) => Seal(leaves.Map(static leaf => new AttestedEntry(default, None, leaf, None, Instant.MinValue)));

    static UInt128 Pair(UInt128 left, UInt128 right) =>
        ContentHash.Of((Left: left, Right: right), static (pair, w) => { w.U128(pair.Left).U128(pair.Right); });
}
```

| [INDEX] | [POLICY]          | [VALUE]                                              | [BINDING]                                                 |
| :-----: | :---------------- | :--------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | tamper-evidence   | hash-chained + KMS-signed                            | the one authenticity authority; checkpoint chain defers   |
|  [02]   | chain break       | rolling-address/back-link discontinuity              | any insert/delete/reorder breaks every downstream address |
|  [03]   | signature verdict | `Custody.Verify(digestOf(entry))` → `CustodyVerdict` | one KMS dispatch; `Unauthored` (recompute vs signed)      |
|  [04]   | audit proof       | Merkle `InclusionProof`/`ConsistencyProof`           | a third party audits one entry without the whole chain    |
|  [05]   | Merkle hasher     | kernel `ContentHash.Of` on `CanonicalWriter`         | the `MerkleRange` peer digest is the other altitude       |
|  [06]   | external witness  | `Witness` signed head + `Corroborate` probe          | published via one egress sink; holds against the operator |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

# [PERSISTENCE_VERSION_PROVENANCE]

`CausalDag` derives W3C-PROV-O lineage from the changefeed and commit DAG, using separate op-key containment and commit-key resolution. `ProvNode`, `ProvRelation`, and `ProvRole` close the standards vocabulary; `Walk` owns bounded traversal; `ProvJson` owns the wire projection. `AttestedLedger` binds content order, KMS authorship, Merkle inclusion, append-only consistency, and externally witnessed heads under one authenticity surface.

`AttestedEntry` is the hash-chained, KMS-signed ledger row whose `Prior` back-link and rolling `Chain` address make any rewrite detectable, distinct from the non-cryptographic content chain the `Version/timetravel#TIME_TRAVEL` `Checkpoint` carries — that chain proves a checkpoint reproduces from the op stream, THIS chain proves the entry was authored by a verified actor and never reordered. Verification composes the `Element/identity#KMS_CUSTODY` `Custody.Verify` over the resolved `SigningKeyring`, so the per-entry probe is the SAME KMS dispatch that gates every signed op (`Authentic`/`Forged`/`Unauthored`/`Unsigned` `CustodyVerdict`), never a hand-rolled boolean signature check; the chain folds those verdicts into one `AttestVerdict`. Beyond whole-chain re-fold the ledger seals a Merkle audit head over the rolling addresses so a third party verifies one entry's `InclusionProof` membership and a `ConsistencyProof` append-only extension without replaying the whole chain — the transparency-log discipline the "authenticity authority" prose names. Lineage rides the one changefeed (`Version/ledger#CHANGEFEED`) and the one content-address (`Element/codec#CONTENT_ADDRESS`), never a second store; `StoreActor` (the bundle asserter and `SignedAuthorship.Actor`) arrives from `Element/graph#STORE_RAIL`; `SignedAuthorship`, `OpDigest`, `SigningKeyring`, `Custody`, `CustodyVerdict` arrive from `Element/identity#KMS_CUSTODY`; `Hlc`/`CommitNode` arrive from `Version/commits`; the wall clock, correlation, and tenant ride the injected `ProjectionContext` frame values — a `ClockPolicy`/`CorrelationId`/`Principal` parameter on any signature here is the deleted strata inversion; a completed Pollination cloud run enters as a `CloudRunFact` VALUE the sidecar projects off `RunsApi.GetRunAsync` (`Run`/`RunStatusEnum`), never the SDK DTOs crossing the seam.

## [01]-[INDEX]

- [01]-[CAUSAL_DAG]: the W3C-PROV-O node/relation vocabulary, the exact lineage derivation from the changefeed and the cloud-run seam, the bounded ancestry/descent frontier, and the PROV-JSON bundle egress.
- [02]-[ATTESTED_LEDGER]: the hash-chained KMS-signed ledger, `Custody`-composed verification, the Merkle inclusion/consistency audit proofs, and the tamper-evidence authority.

## [02]-[CAUSAL_DAG]

- Owner: `ProvKind` the `[SmartEnum<string>]` PROV-node-class axis carrying its PROV-O class IRI and the `AssociationRole` an agent associated with this activity kind plays; `AgentClass` the `[SmartEnum<string>]` PROV-O agent-subtype axis (`Person`/`SoftwareAgent`/`Organization`); `ProvRole` the `[SmartEnum<string>]` qualified-influence role vocabulary the association carries; `ProvNode` the `[Union]` causal node (`Entity`/`Activity`/`Agent`) whose `Entity` refines into the `EntitySubclass` PROV form (`Plain`/`Collection`/`Bundle`/`Plan`); `ProvRelation` the `[SmartEnum<string>]` influence vocabulary carrying its PROV-O term, its endpoint-property law (the W3C-PROV-JSON edge map property names), and the derivation-subclass parent it specializes; `ProvEndpoint` the `[SmartEnum<string>]` from/to property-name axis the JSON projection reads; `ProvEdge` the typed causal edge carrying its qualified `ProvRole`/plan; `LineageWalk` the bounded ancestry/descent frontier fold; `CloudRunFact` the sidecar-projected completed-cloud-run value the second `Derive` modality folds; `CausalDag` the static surface owning lineage derivation (changefeed AND cloud-run modalities), the walks, and the PROV-JSON projection.
- Cases: `ProvNode` is `Entity(ContentAddress, ProvKind, EntitySubclass, Instant)`, `Activity(UInt128 Id, ProvKind Kind, Instant Started, Instant Ended)`, `Agent(string Actor, AgentClass Class, bool Signed)` — the agent identity is the stable actor SUBJECT string (the one identifier the changefeed `Actor` header and a `SignedAuthorship.Actor.Subject` both yield, never a full `StoreActor` frozen into a durable node — role claims are session facts), the CLASS is the PROV-O subtype derived through `AgentClass.Of(StoreActor)` off the actor's role claims, the `Signed` flag the attestation fact, never a `bool`-as-type; `EntitySubclass` is `Plain | Collection | Bundle | Plan` (the PROV-O `Entity` subclasses — a model is a `Collection` of element entities, the lineage export a `Bundle`, a merge strategy a `Plan`); `ProvRelation` is the eleven W3C-PROV-O influence terms — `WasGeneratedBy | Used | WasInformedBy | WasDerivedFrom | WasRevisionOf | WasQuotedFrom | HadPrimarySource | WasInvalidatedBy | WasAttributedTo | WasAssociatedWith | ActedOnBehalfOf` — each a row carrying its `prov:` term, its endpoint-property law, and the derivation-subclass parent it specializes (`WasRevisionOf`/`WasQuotedFrom`/`HadPrimarySource` generalize to `WasDerivedFrom`), a twelfth being one row, never a parallel edge family; `AgentClass` is the three PROV-O agent subtypes; `ProvRole` is `Author | Reviser | Importer | Merger | Solver | Delegate` — the role an agent played in an association; `ProvKind` closes at nine rows — the four entity kinds (`Graph`/`Delta`/`Snapshot`/`Blob`) and the five activity kinds (`Commit`/`Merge`/`Import`/`Solve`/`CloudRun`), the `CloudRun` row REUSING `ProvRole.Solver` because a cloud solver and a local solver play the one role.
- Entry: `public static Seq<ProvEdge> Derive(Seq<OpLogEntry> changefeed, Func<UInt128, Option<CommitNode>> containing, Func<UInt128, Option<CommitNode>> resolve)` projects the lineage graph from the changefeed plus the commit DAG with EXACT PROV-O endpoint typing — `containing` resolves an entry's content key (an OP key under `CommitNode.OpKeys`, never the commit key) to its RECORDING commit activity while `resolve` walks commit keys to parents, two resolvers because the two key spaces are disjoint — each delta entity `WasGeneratedBy` its commit activity, the commit activity `WasAssociatedWith` its signed agent under the agent's `ProvRole`, the delta entity `WasAttributedTo` that agent, a revised entity `WasRevisionOf` each parent commit's produced op-key entities (the derivation subclass, resolved one hop through `resolve` — never the geometry `Closure`), a retired entity `WasInvalidatedBy` its commit activity, and a software agent `ActedOnBehalfOf` its delegating principal; `public static Seq<ProvEdge> Derive(CloudRunFact run)` is the SAME entry's cloud-run modality (input shape discriminates) — the completed run is a W3C-PROV `Activity`: `Used` each input-asset content key, each output-asset entity `WasGeneratedBy` the run activity and `WasAttributedTo` the service agent, the activity `WasAssociatedWith` the `SoftwareAgent` behind `Configuration.AccessToken` (`TokenRepo`) qualified `ProvRole.Solver` with `hadPlan` the recipe reference (`owner/name:tag` + the registry `PackageVersion.Digest`), and the service agent `ActedOnBehalfOf` the human subject who submitted; `public static Seq<ProvNode> Walk(LineageWalk walk, Func<ContentAddress, Seq<ProvEdge>> adjacency, Func<UInt128, (ProvKind, EntitySubclass)> kindOf)` is the one bounded breadth-first frontier fold the ancestry and descent directions both dial; `public static Seq<ProvNode> Derivations(ContentAddress root, int depth, Func<ContentAddress, Seq<ProvEdge>> incoming, Func<UInt128, (ProvKind, EntitySubclass)> kindOf)` composes `Walk` over a derivation-family-filtered adjacency for the transitive `wasDerivedFrom` closure; `public static ProvBundle Bundle(Seq<ProvEdge> lineage, StoreActor authority, Option<SignedAuthorship> attestation, Instant at)` names the lineage as a PROV `Bundle` whose asserting `ProvNode.Agent` DERIVES its `AgentClass` off the actor's role claims (`AgentClass.Of`) and its `Signed` fact off the attestation presence — the provenance-of-provenance header, never a hardcoded `Person`/`signed=false` pair; `public static JsonElement ProvJson(ProvBundle bundle, Func<UInt128, ProvNode> resolve)` projects the standards-conformant W3C-PROV-JSON document.
- Auto: lineage is DERIVED from the changefeed plus the commit DAG, never a parallel provenance write — a delta IS the `WasGeneratedBy` evidence (delta entity → its RECORDING commit activity, resolved through the op-key→commit `containing` index), and the `Version/commits#COMMIT_DAG` parent commits' op-key entities (resolved one hop through `resolve`, the parent commit being an Activity whose produced entities are its `OpKeys`) ARE the `WasRevisionOf` sources, NEVER the `OpLogEntry.Closure` geometry-blob manifest, so the PROV graph is a fold over the events the system already holds; the association edge reads the `Element/identity#KMS_CUSTODY` `SignedAuthorship` so a `WasAssociatedWith` names a verified `Person`/`Organization` `Agent` when the op was KMS-signed and a `SoftwareAgent` (a Compute solver activity, an IFC importer, a Pollination cloud run) when an automated activity produced the entity, the automated agent `ActedOnBehalfOf` the human principal that triggered the run; the activity span reads the commit cell `Hlc` so the PROV `startedAtTime`/`endedAtTime` ride the one causal clock; the qualified `hadRole`/`hadPlan` of an association reads the activity's `ProvKind` (an `Import` activity's agent plays `Importer`, a `Merge` activity's plays `Merger`, a `CloudRun` activity's plays `Solver` with `hadPlan` the recipe-reference `Plan` entity) so the influence is role-qualified, never bare — `hadPlan` rides ONLY a modality that supplies a real `Plan` entity (`CommitNode` records no strategy entity, so the changefeed association carries no plan; an activity key cited as its own plan is a PROV typing contradiction, the deleted form); the cloud-run modality folds the SAME edge vocabulary from `CloudRunFact` values the sidecar projects — the SDK fork closure (`LBT.RestSharp`/`LBT.Newtonsoft.Json`) never loads here, and the attested verify makes the externally-computed result tamper-evident locally, the federation-wide verify template this page owns.
- Receipt: a lineage derivation rides `store.prov.derive` carrying the edge count by `ProvRelation`; an ancestry/descent walk rides `store.prov.walk` carrying the reached-node count, depth, and direction; a PROV-JSON bundle export rides `store.prov.export` carrying the bundle node and edge counts.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the agent/plan/activity durable key mints, [B]), System.IO.Hashing (`XxHash128.Append`/`Clone`/`GetCurrentHashAsUInt128` — the bundle-id rolling digest ONLY, a transparency-log construction the [B] ruling keeps local), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Text.Json (the `Element/codec#CODEC_AXIS` `ElementJson.Options` Thinktecture-converter set the PROV-JSON `SerializeToElement` egress composes, never a second converter registration), System.Runtime.InteropServices, PollinationSDK (sidecar seam only — `RunsApi.GetRunAsync` → `Run`/`RunStatusEnum`, `Configuration.AccessToken` → `TokenRepo`, `ArtifactsApi` asset landings project the `CloudRunFact` VALUES; no fence references the SDK), BCL inbox.
- Growth: a new PROV relation is one `ProvRelation` row carrying its term + endpoint law + derivation-subclass parent; a new node class is one `ProvNode`/`EntitySubclass`/`ProvKind` row; a new activity source is one `ProvKind` activity row plus one `Derive` input modality (as `CloudRun`/`CloudRunFact` is); a new agent subtype is one `AgentClass` row; a new association role is one `ProvRole` row; a new walk direction is one `LineageWalk` disposition over the one `Walk` fold; zero new surface — a parallel provenance store, a second lineage walker, an attribution edge mis-typed off an activity, or a free-string PROV term is the deleted form because the lineage is a fold over the changefeed and the PROV vocabulary is the closed W3C-PROV-O term set.
- Boundary: the causal DAG is DERIVED from the changefeed plus the commit DAG — a delta entity's `WasGeneratedBy`, its commit's `WasAssociatedWith` agent, and the `WasRevisionOf` predecessor (each parent commit's produced op-key entities, resolved one hop, NEVER the `OpLogEntry.Closure` descendant-geometry manifest which is a blob set and not a version predecessor — keying revision off `Closure.Head` is the deleted defect) are all reads off the events, never a write of record, so the lineage is reconstructible from the one op stream a replica folds; the PROV-O typing is EXACT — an `Activity` binds an `Agent` through `WasAssociatedWith` and an `Entity` through `WasAttributedTo`, so an attribution edge sourced off an activity is the deleted defect, the derivation of a revised graph is the `WasRevisionOf` subclass not the generic `WasDerivedFrom`, and the `WasInformedBy` activity-to-activity chain captures a merge informed by its parents; the agent is a PROV-O subtype (`Person`/`SoftwareAgent`/`Organization`) so a derived `Assessment` result names its `SoftwareAgent` solver `ActedOnBehalfOf` the human `Person` who triggered it, never an anonymous machine write; a completed Pollination run is the SAME law at the cloud seam — the service principal behind the access token is the `SoftwareAgent`, the input/output asset content keys the blobstore landing minted are the `Used`/`WasGeneratedBy` entities, and the recipe reference is the `hadPlan` `Plan` entity, so a cloud result is attributable, plan-bound, and locally tamper-evident through the one attested ledger, never a loose file download; the W3C-PROV-JSON egress is a real standards document (top-level `prefix`/`entity`/`activity`/`agent`/`wasGeneratedBy`/`used`/`wasAssociatedWith`/`wasAttributedTo`/`wasDerivedFrom`/`actedOnBehalfOf` keyed maps with `prov:`-prefixed properties, the bundle registered under `entity` as a `prov:Bundle` — a top-level `bundle` STRING key is a schema violation, the deleted form), never a flat `from→to` edge dictionary, so a CDE consumer ingests it through any PROV-O toolchain; the bundle is a PROV `Bundle` carrying its own provenance-of-provenance so the export is itself attributable; the lineage walk is bounded breadth-first over the typed adjacency so the cost is linear in the reachable-edge count within the depth bound; the attribution reconciles with the `Version/timetravel#TIME_TRAVEL` `BlameRow` (the same `(Hlc, origin)` winner the convergence selected) so blame and provenance never disagree.

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
    // A completed Pollination cloud run: the same Solver role a local solve plays — the cloud/local split is
    // deployment, never a second role vocabulary.
    public static readonly ProvKind CloudRun = new("cloud-run", "prov:Activity", ProvRole.Solver);
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
    // The agent class DERIVES off the actor's role claims — the AppHost port maps its principal kind onto a
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
    public bool IsDerivation => (this == WasDerivedFrom) || (GeneralizesTo == Some(WasDerivedFrom));
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

    // Mint the CORRECT PROV class from the kind: an `IsActivity` kind (Commit/Merge/Import/Solve/CloudRun, `prov:Activity`) is an
    // Activity node, an entity kind (Graph/Delta/Snapshot/Blob, `prov:Entity`) an Entity — so a reached COMMIT in a
    // lineage walk is never mis-minted as an Entity (the prior `new ProvNode.Entity(...)`-for-everything tangle would
    // emit a `prov:Activity`-kinded Entity, a PROV-O typing contradiction the JSON projection's class map would mis-file).
    public static ProvNode Of(ContentAddress address, ProvKind kind, EntitySubclass subclass) =>
        kind.IsActivity ? new Activity(address.Value, kind, Instant.MinValue, Instant.MinValue) : new Entity(address, kind, subclass, Instant.MinValue);

    public UInt128 Identity => Switch(
        entity: static e => e.Address.Value,
        activity: static a => a.Id,
        agent: static g => CausalDag.AgentKey(g.Actor));

    public string ClassIri => Switch(
        entity: static e => e.Subclass.ClassIri,
        activity: static a => a.Kind.ClassIri,
        agent: static g => g.Class.ClassIri);
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
// asserted the bundle, when), so the lineage export is itself an attributable PROV Entity. The asserter is
// the DERIVED Agent node (class off the actor's role claims, Signed off the attestation presence), never a
// raw actor value the projection would have to re-classify.
public readonly record struct ProvBundle(UInt128 Id, Seq<ProvEdge> Lineage, ProvNode.Agent Asserter, Instant At);

// The sidecar-projected completed-cloud-run fact: the SDK DTOs never cross this seam — the sidecar reads
// `RunsApi.GetRunAsync` -> `Run`/`RunStatusEnum`, resolves the service principal behind
// `Configuration.AccessToken` (`TokenRepo`), and hands over VALUES: the run id, the recipe plan reference
// (`owner/name:tag`) with its registry `PackageVersion.Digest`, and the input/output asset content keys the
// `Store/blobstore` landing minted.
public readonly record struct CloudRunFact(
    string RunId, string ServicePrincipal, string OnBehalfOf, string RecipeRef, string RecipeDigest,
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
            // The COMMIT (activity) nature decides the association role: a multi-parent commit IS a merge
            // (CommitNode.IsMerge), so its agent plays Merger; an ordinary commit's plays Author. The role reads
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

    // The cloud-run modality of the ONE lineage derivation — input shape discriminates, never a sibling name.
    // The run is a PROV Activity keyed off its run id; the service SoftwareAgent behind the access token is
    // associated qualified Solver with hadPlan the recipe-reference Plan entity; the asset content keys the
    // blobstore landing minted are the Used/WasGeneratedBy entities; the agent delegates to the submitter.
    public static Seq<ProvEdge> Derive(CloudRunFact run) {
        UInt128 activity = ContentHash.Of(Encoding.UTF8.GetBytes(run.RunId));
        UInt128 agent = AgentKey(run.ServicePrincipal);
        UInt128 plan = ContentHash.Of(Encoding.UTF8.GetBytes($"{run.RecipeRef}@{run.RecipeDigest}"));
        return run.Used.Map(key => ProvEdge.Of(ProvRelation.Used, activity, key.Value, run.Started))
            + run.Generated.Map(key => ProvEdge.Of(ProvRelation.WasGeneratedBy, key.Value, activity, run.Ended))
            + run.Generated.Map(key => ProvEdge.Of(ProvRelation.WasAttributedTo, key.Value, agent, run.Ended))
            + Seq(
                ProvEdge.Of(ProvRelation.WasAssociatedWith, activity, agent, run.Ended).Qualified(ProvKind.CloudRun.AssociationRole, Some(plan)),
                ProvEdge.Of(ProvRelation.ActedOnBehalfOf, agent, AgentKey(run.OnBehalfOf), run.Ended));
    }

    public static Seq<ProvNode> Walk(LineageWalk walk, Func<ContentAddress, Seq<ProvEdge>> adjacency, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf) {
        // Bounded breadth-first frontier fold: each level expands the current frontier's adjacency to the
        // next, records the freshly-seen nodes, and recurses on ONLY the new frontier until the depth
        // ceiling or quiescence — never re-descending an already-visited node (the prior re-walk-every-
        // reached-node tangle is the deleted form).
        Seq<ProvNode> Expand(Seq<ContentAddress> frontier, LanguageExt.HashSet<UInt128> seen, Seq<ProvNode> reached, int depth) {
            if ((depth <= 0) || frontier.IsEmpty) return reached;
            (LanguageExt.HashSet<UInt128> Seen, Seq<ContentAddress> Next, Seq<ProvNode> Reached) level = frontier
                .Bind(node => adjacency(node).Map(walk.Direction.Step))
                .Distinct()
                .Fold((Seen: seen, Next: Seq<ContentAddress>(), Reached: reached), (acc, id) => {
                    if (acc.Seen.Contains(id)) { return acc; }
                    (ProvKind kind, EntitySubclass subclass) = kindOf(id);
                    ContentAddress address = ContentAddress.Of(id);
                    return (acc.Seen.Add(id), acc.Next.Add(address), acc.Reached.Add(ProvNode.Of(address, kind, subclass)));
                });
            return Expand(level.Next, level.Seen, level.Reached, depth - 1);
        }
        return Expand(Seq(walk.Root), HashSet(walk.Root.Value), Seq<ProvNode>(), walk.Depth);
    }

    // Derivation-only ancestry — the transitive wasDerivedFrom/Revision/Quotation/PrimarySource closure
    // (the PROV "what did this derive from" query) by composing `Walk` over an adjacency pre-filtered to
    // the derivation family through `ProvRelation.IsDerivation`, never a second walker.
    public static Seq<ProvNode> Derivations(ContentAddress root, int depth, Func<ContentAddress, Seq<ProvEdge>> incoming, Func<UInt128, (ProvKind Kind, EntitySubclass Subclass)> kindOf) =>
        Walk(LineageWalk.Ancestry(root, depth), address => incoming(address).Filter(static e => e.Relation.IsDerivation), kindOf);

    // The asserting Agent node derives HERE — class off the actor's role claims (`AgentClass.Of`), Signed off
    // the attestation presence — so the JSON projection reads a settled node and never re-classifies.
    public static ProvBundle Bundle(Seq<ProvEdge> lineage, StoreActor authority, Option<SignedAuthorship> attestation, Instant at) {
        using XxHash128 rolling = new();
        Span<byte> word = stackalloc byte[16];
        foreach (ProvEdge edge in lineage.OrderBy(static e => (e.From, e.To, e.Relation.Key))) {
            BinaryPrimitives.WriteUInt128LittleEndian(word, edge.From); rolling.Append(word);
            BinaryPrimitives.WriteUInt128LittleEndian(word, edge.To); rolling.Append(word);
            rolling.Append(MemoryMarshal.AsBytes(edge.Relation.Key.AsSpan()));
        }
        using XxHash128 sealedState = rolling.Clone();
        return new ProvBundle(sealedState.GetCurrentHashAsUInt128(), lineage, new ProvNode.Agent(authority.Subject, AgentClass.Of(authority), attestation.IsSome), at);
    }

    // A standards-conformant W3C-PROV-JSON document: top-level prefix + per-class node maps (entity/
    // activity/agent) + per-relation influence maps, every node and edge keyed by its prov:-namespaced
    // id, every influence carrying its endpoint properties plus the qualified prov:hadRole/prov:hadPlan
    // — ingestible by any PROV-O toolchain, never a flat from->to edge dictionary.
    public static JsonElement ProvJson(ProvBundle bundle, Func<UInt128, ProvNode> resolve) {
        static string Iri(UInt128 id) => $"rasm:{id:x32}";
        // The bundle is itself an attributable PROV `Bundle` entity: it registers under `entity` as a `prov:Bundle`,
        // its asserting Agent node under `agent` through the SAME `NodeMembers` projection every lineage agent takes
        // (class + signed DERIVED at `Bundle`, never re-hardcoded here), and the bundle->asserter attribution rides
        // the ONE edge grouping as a `WasAttributedTo` `ProvEdge` — never a hand-set top-level literal a lineage
        // attribution group would clobber. Top-level influence keys are the UNPREFIXED `ProvRelation.Key` names the
        // PROV-JSON schema fixes; the `prov:` prefix belongs to member properties, never the top-level map key.
        UInt128 authorityKey = AgentKey(bundle.Asserter.Actor);
        Seq<ProvEdge> edges = bundle.Lineage.Add(ProvEdge.Of(ProvRelation.WasAttributedTo, bundle.Id, authorityKey, new Hlc(bundle.At, 0UL)));
        Dictionary<string, Dictionary<string, object>> byClass = bundle.Lineage.Bind(static e => Seq(e.From, e.To)).Distinct().Map(resolve)
            .GroupBy(static n => n.Switch(entity: static _ => "entity", activity: static _ => "activity", agent: static _ => "agent"))
            .ToDictionary(static g => g.Key, static g => g.ToDictionary(static n => Iri(n.Identity), NodeMembers));
        Dictionary<string, object> entities = byClass.TryGetValue("entity", out Dictionary<string, object> es) ? es : new Dictionary<string, object>();
        Dictionary<string, object> agents = byClass.TryGetValue("agent", out Dictionary<string, object> gs) ? gs : new Dictionary<string, object>();
        entities[Iri(bundle.Id)] = new Dictionary<string, object?> { ["prov:type"] = EntitySubclass.Bundle.ClassIri, ["prov:generatedAtTime"] = bundle.At.ToString() };
        agents[Iri(authorityKey)] = NodeMembers(bundle.Asserter);
        Dictionary<string, object> document = new() {
            ["prefix"] = new Dictionary<string, string> { ["prov"] = "http://www.w3.org/ns/prov#", ["rasm"] = "urn:rasm:prov:" },
            ["entity"] = entities, ["agent"] = agents,
        };
        if (byClass.TryGetValue("activity", out Dictionary<string, object> acts)) { document["activity"] = acts; }
        foreach (IGrouping<string, ProvEdge> byRelation in edges.GroupBy(static e => e.Relation.Key))
            document[byRelation.Key] = byRelation.Select(static (e, i) => (Key: $"_:e{i}", Edge: e)).ToDictionary(static p => p.Key, static p => EdgeMembers(p.Edge));
        return JsonSerializer.SerializeToElement(document, ElementJson.Options);

        static object NodeMembers(ProvNode node) => node.Switch(
            entity: static e => new Dictionary<string, object?> { ["prov:type"] = e.ClassIri, ["rasm:kind"] = e.Kind.Key, ["prov:generatedAtTime"] = e.At.ToString() },
            activity: static a => new Dictionary<string, object?> { ["prov:type"] = a.ClassIri, ["prov:startedAtTime"] = a.Started.ToString(), ["prov:endedAtTime"] = a.Ended.ToString() },
            agent: static g => new Dictionary<string, object?> { ["prov:type"] = g.ClassIri, ["rasm:id"] = g.Actor, ["rasm:signed"] = g.Signed });

        static object EdgeMembers(ProvEdge edge) {
            Dictionary<string, object?> members = new() {
                [$"prov:{edge.Relation.Endpoint.FromProperty}"] = Iri(edge.From),
                [$"prov:{edge.Relation.Endpoint.ToProperty}"] = Iri(edge.To),
                ["rasm:atTime"] = edge.Cell.Physical.ToString(),
            };
            edge.Role.Iter(role => members["prov:hadRole"] = role.Key);
            edge.Plan.Iter(plan => members["prov:hadPlan"] = Iri(plan));
            return members;
        }
    }

    // The agent key is the kernel `ContentHash.Of` seed-zero digest of the actor SUBJECT string ([B] — a durable
    // PROV node identity is a content-key mint, never a raw hasher call) — the one stable actor identifier
    // reconstructible from BOTH the changefeed `OpLogEntry.Actor` header and a `SignedAuthorship.Actor.Subject`,
    // never a full `StoreActor` (role claims are session facts the bare changefeed actor string cannot reconstruct).
    internal static UInt128 AgentKey(string actor) => ContentHash.Of(Encoding.UTF8.GetBytes(actor));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                            | [BINDING]                                                     |
| :-----: | :---------------- | :------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | lineage source    | derived from the changefeed + commit DAG           | never a parallel provenance write; never geometry `Closure`   |
|  [02]   | activity↔agent    | `WasAssociatedWith` (never `WasAttributedTo`)      | entity→agent is attribution, activity→agent is association    |
|  [03]   | revision source   | each parent commit's `OpKeys` entities (one hop)   | `WasRevisionOf` (EntityEntity); never `Closure.Head`          |
|  [04]   | agent typing      | `Person`/`SoftwareAgent`/`Organization`            | a solver `ActedOnBehalfOf` its human principal                |
|  [05]   | walk node typing  | `ProvNode.Of` on `kind.IsActivity`                 | a reached commit is an Activity, never an Entity              |
|  [06]   | egress            | W3C-PROV-JSON bundle (`entity`/`activity`/`agent`) | a standards CDE artifact, never a flat edge dictionary        |
|  [07]   | walk cost         | one bounded breadth-first frontier fold            | linear in reachable edges within the depth bound              |
|  [08]   | cloud-run lineage | `Derive(CloudRunFact)` second modality             | `ProvKind.CloudRun` reuses `ProvRole.Solver`                  |
|  [09]   | bundle asserter   | `AgentClass.Of(StoreActor)` + attestation presence | class/signed DERIVED, never hardcoded `Person`/`signed=false` |

- [08]-[cloud-run lineage]: `Used`/`Generated` = asset content keys; `hadPlan` = recipe ref + digest.

## [03]-[ATTESTED_LEDGER]

- Owner: `AttestedEntry` the hash-chained, KMS-signed ledger row; `MerkleAudit` the per-head Merkle tree over the rolling addresses; `InclusionProof`/`ConsistencyProof` the third-party audit paths; `WitnessedHead` the KMS-signed tree-head publication row an EXTERNAL witness caches; `AttestVerdict` the closed chain-validity verdict; `AttestedLedger` the static surface owning the chain append, the rolling-address fold, the Merkle head seal, the audit-proof projections, the `Witness`/`Corroborate` external-witness pair, and the `Custody`-composed chain verification that is the one tamper-evidence authority.
- Cases: an `AttestedEntry` carries the entry content key, the `Prior` back-link, the rolling `Chain` address (`XxHash128` over `Prior ++ ContentKey ++ authorship signature ++ attestation instant` — authorship BINDS the address, so a signature or time rewrite moves every downstream address and the audit root), and the optional `SignedAuthorship`; `AttestVerdict` is `Authentic | Broken(at) | Unsigned | Mixed(signed, unsigned) | Unauthored(at) | Forged(at) | CustodyRejected(at, cause)` — `Authentic` the verified chain, `Broken` a back-link/rolling-address discontinuity, `Unsigned` a local-tier chain with no KMS signature, `Unauthored` a signed entry whose `OpDigest` does not bind its content (the `CustodyVerdict.Unauthored` arm), `Forged` a signature that fails KMS verification (the `CustodyVerdict.Forged` arm), `CustodyRejected` every remaining non-authentic custody arm (`DigestWidth`/`UnsupportedAlgorithm`/`AlgorithmMismatch` — a custody rejection can NEVER finalize `Authentic`), `Mixed` a chain carrying both signed and unsigned entries (partial custody is its own verdict, never an `Authentic` masquerade); the verdict cases mirror the `Element/identity#KMS_CUSTODY` `CustodyVerdict` cryptographic arms so verification never re-classifies what `Custody.Verify` already decided.
- Entry: `public static AttestedEntry Append(Option<AttestedEntry> prior, UInt128 contentKey, Option<SignedAuthorship> authorship)` extends the chain with the new rolling address; `public static IO<AttestVerdict> Verify(Seq<AttestedEntry> chain, Func<SignedAuthorship, SigningKeyring> keyringFor, Func<AttestedEntry, OpDigest> digestOf)` re-folds the chain, confirms every back-link and rolling address, and runs `Custody.Verify` over the per-entry resolved keyring with the INDEPENDENTLY recomputed expected digest (so `Unauthored` — a signature over a digest that does not bind the entry's content — is actually reachable, never self-compared against the stored digest); `public static MerkleAudit Seal(Seq<AttestedEntry> chain)` folds the rolling addresses into a balanced Merkle tree whose root is the audit head; `public static Option<InclusionProof> Prove(MerkleAudit audit, int leaf)` projects the sibling-hash path proving one entry's membership; `public static bool Includes(InclusionProof proof, UInt128 leaf, UInt128 root)` re-folds the path to the root; `public static ConsistencyProof Extend(MerkleAudit older, MerkleAudit newer)` issues the proof the newer head append-only-extends the older; `public static bool Consistent(ConsistencyProof proof, MerkleAudit newer)` confirms it by re-sealing the newer's leaf prefix to the old root; `public static IO<WitnessedHead> Witness(MerkleAudit audit, Func<ReadOnlyMemory<byte>, IO<Option<SignedAuthorship>>> sign, Func<WitnessedHead, IO<OpLogEntry>> stamp, Instant at)` seals the KMS-signed tree head for publication BEYOND the store it audits — the threaded `stamp` (the `OpLog.Stamp` partial application) lands the head as one `attest`-lane `Version/ledger#CHANGEFEED` `OpLogEntry` (`Payload` the lane-codec-encoded `WitnessedHead`, `ContentKey` the kernel `ContentHash.Of` over the `WitnessedHead.Canonical` bytes) so the ordinary `Version/egress` pump drains it to the witness's sink at cadence with zero bespoke envelope — and `public static bool Corroborate(WitnessedHead cached, MerkleAudit newer)` is the external witness's probe — a newly published audit must append-only-extend the head the witness cached, composed wholly over the one `Consistent` check.
- Auto: the ledger is the AUTHENTICITY authority distinct from the reproducibility chain — the `Version/timetravel#TIME_TRAVEL` `Checkpoint.Hash` is a non-cryptographic content chain that proves a checkpoint reproduces from the op stream, while THIS chain's `SignedAuthorship` proves the entry was authored by a verified actor and not rewritten; verification re-folds the rolling address over the back-links and routes each signed entry through `Custody.Verify` so the cryptographic verdict is the SAME KMS dispatch (`Authentic`/`Forged`/`Unauthored`/`Unsigned`) that gates every signed op, never a hand-rolled boolean; the Merkle head seals the rolling addresses so a third party verifies one entry's `InclusionProof` and an append-only `ConsistencyProof` between two heads without replaying the whole chain.
- Receipt: a chain append rides `store.attest.append`; a verification rides `store.attest.verify` carrying the verdict and the break locus when broken; an audit-proof projection rides `store.attest.prove` carrying the leaf index and the proof path length; a witnessed-head publication rides `store.attest.witness` carrying the root, the leaf count, and the delivering sink key.
- Packages: System.IO.Hashing (`XxHash128.Append`/`Clone`/`GetCurrentHashAsUInt128`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (the KMS verify rides `Element/identity#KMS_CUSTODY` `Custody.Verify` over the resolved `SigningKeyring`, never a direct provider call here).
- Growth: a new verdict is one `AttestVerdict` case mirroring a `CustodyVerdict` arm; a richer audit proof is one projection over `MerkleAudit`; zero new surface — a second tamper-evidence scheme, a hand-rolled signature check beside `Custody.Verify`, a Merkle-tree audit log built on a second hasher, or a content chain claiming authenticity is the deleted form because this ledger owns authenticity and the checkpoint chain owns reproducibility, two distinct concerns.
- Boundary: the attested ledger is the ONE tamper-evidence authority — the `Version/timetravel#TIME_TRAVEL` `Checkpoint` hash chain explicitly defers here and carries no authenticity claim, so a content chain standing in for tamper-evidence is the deleted form; the chain is hash-chained off the prior rolling address so any inserted, deleted, or reordered entry breaks every downstream address (a `Broken` verdict naming the discontinuity); the per-entry cryptographic verdict COMPOSES `Element/identity#KMS_CUSTODY` `Custody.Verify(authorship, digest, keyring)` with the `digest` being the INDEPENDENTLY recomputed expected digest off the entry's content (`digestOf`, never the stored `authorship.Digest` self-compared — that self-comparison makes `Unauthored` unreachable, the illusory-verify deleted form) — a verified `CustodyVerdict.Authentic` proves the actor and the order, an `Unsigned` chain (the local/Personal tier on `KmsProvider.None`) proves order only, an `Unauthored` names a signed entry whose digest does not bind its content (the recomputed digest differs from the signed one), and a `Forged` names the entry whose KMS signature fails, so a second boolean signature predicate beside the one `Custody` verifier is the deleted form; the Merkle audit head is the transparency-log discipline — `InclusionProof` lets an external auditor confirm one entry is in the ledger from its sibling path and root alone, and `ConsistencyProof` confirms a later head only appended (never rewrote) so a regulator audits a slice without the whole history; the Merkle tree composes the one `XxHash128` the rolling chain and the content address already use (the `Version/commits#COMMIT_DAG` `MerkleRange` is the peer anti-entropy digest, this the per-entry authenticity audit — two altitudes of the one Merkle discipline, never a second hasher); a signed head held ONLY inside the store it audits proves nothing against a compromised operator, so `Witness` publishes the KMS-signed head at cadence through one `Version/egress` sink to an independent residence (a peer store, a second cloud, a notarization endpoint — riding the `attest` changefeed lane, never a bespoke envelope beside the one pump) and `Corroborate` lets that witness reject a rewritten history from its cached head alone — tamper evidence that holds against the store's own operator, the difference between self-audit and counterparty audit a multi-party construction contract demands.

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

// Proof coordinates for a full newer audit: the verifier re-seals its old-size leaf prefix and compares both roots.
public readonly record struct ConsistencyProof(int OldSize, int NewSize, UInt128 OldRoot, UInt128 NewRoot);

// The externally witnessed tree head: root, leaf count, and the KMS signature over the canonical head bytes —
// published beyond the store so an independent witness caches it and later rejects any rewrite. `Signature`
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
    public sealed record CustodyRejected(int At, string Cause) : AttestVerdict;
    public sealed record Mixed(int SignedEntries, int UnsignedEntries) : AttestVerdict;
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
                            _ => (Some(entry), acc.Verdict is AttestVerdict.Authentic ? new AttestVerdict.CustodyRejected(acc.Index, decision.GetType().Name) : acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned),
                        }),
                        None: () => IO.pure((Some(entry), acc.Verdict, acc.Index + 1, acc.Signed, acc.Unsigned + 1)));
            })
            // A mixed signed/unsigned chain is its own verdict — partial custody never masquerades as Authentic.
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

    public static Option<InclusionProof> Prove(MerkleAudit audit, int leaf) {
        if ((leaf < 0) || (leaf >= audit.Leaves)) { return None; }
        (int Index, Seq<(UInt128, bool)> Path) folded = audit.Levels.Take(audit.Levels.Count - 1).Fold(
            (Index: leaf, Path: Seq<(UInt128, bool)>()),
            (acc, level) => (acc.Index >> 1, (acc.Index ^ 1) < level.Count
                ? acc.Path.Add((level[acc.Index ^ 1], (acc.Index & 1) == 1))
                : acc.Path));
        return Some(new InclusionProof(leaf, audit.Leaves, folded.Path));
    }

    public static bool Includes(InclusionProof proof, UInt128 leaf, UInt128 root) =>
        proof.Path.Fold(leaf, static (acc, step) => step.Left ? Pair(step.Sibling, acc) : Pair(acc, step.Sibling)) == root;

    public static ConsistencyProof Extend(MerkleAudit older, MerkleAudit newer) =>
        new(older.Leaves, newer.Leaves, older.Root, newer.Root);

    // The external-witness pair. Witness signs the canonical head bytes through the SAME Element/identity
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

    // The append-only check (the ledger is a hash chain, so consistency reduces to leaf-prefix equality):
    // re-sealing the newer audit's first OldSize leaves must reproduce OldRoot AND the newer audit must
    // re-seal to NewRoot — a newer tree shorter than the older, or a prefix that does not reproduce the
    // old root, is a rewrite the proof rejects. `ConsistencyProof` carries only values this verifier consumes;
    // the newer audit supplies the prefix leaves needed to reproduce the cached root.
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

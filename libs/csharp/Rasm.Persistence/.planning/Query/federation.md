# [PERSISTENCE_FEDERATION]

Rasm.Persistence owns the source-agnostic federated entity graph and the universal BIM query currency that rides it: `FederatedEntity` keys every element by a stable composite identity (geometry refs, property sets, classification, spatial containment, provenance) across every source document, carrying the spatial `Envelope`, the `EmbeddingIdentity`, the search corpus, and the `TenantId` the fusion and lane consumers join on; `ElementSet` is the polymorphic, composable, stable selection receipt every clash/IDS/MVD/QTO surface consumes; `SetPredicate` is the closed leaf-predicate algebra that spatial, document, and classification selection lower through so no `SetExpr` leaf carries a raw stringly-typed predicate; `CrossDocLink` resolves typed inter-document references with pin-versus-float semantics and transitive impact analysis; `RulePlan` lowers a declarative rule DSL — including the buildingSMART IDS 1.0 value-restriction grammar — into a predicate/query plan emitting typed pass/fail/element-set/viewpoint receipts; `FusionRank` fuses the world-class pgvector HNSW, PostGIS GiST, and pg_search/FTS index branches into one reciprocal-rank result with per-hit lineage through the one n-ary RRF fold the `Query/lanes#SEARCH_LANES` `HybridRetrieve` two-branch leaf also rides; and `FederatedPlan` pushes one selection AST across engines with cost-based engine selection and partial-pushdown, admitting a portable `python:data` Substrait/ibis plan at the wire through `Admit` into the one `RuleAst` algebra. Every durable mutation and resolution is a `Query/rail#OPERATION_ALGEBRA` `StoreOp` case run through `StoreRail.Run`, never an injected `persist`/`resolve` delegate; every fault is the closed `FederationFault` `[Union]` deriving from `Expected`, never a bare `Error.New`. The federated graph rides the existing PostGIS GiST + jsonb + ltree substrate on the `PostgresServer` profile, the content-addressed identity (`XxHash128`), the op-log changefeed, and the structural-diff node identity; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive settled.

## [01]-[INDEX]

- [01]-[ENTITY_GRAPH]: source-agnostic federated entity keyed by stable composite identity; spatial/vector/lexical/tenant join columns; `StoreOp`-routed upsert; closed fault family.
- [02]-[ELEMENT_SET_ALGEBRA]: polymorphic composable selection over the graph; `SetPredicate` typed leaf algebra; stable set receipts.
- [03]-[CROSS_DOC_LINKS]: typed inter-doc links, pin/float, and transitive impact propagation as one immutable fold.
- [04]-[RULE_PLAN]: declarative rule DSL to predicate/query plan; the IDS value-restriction grammar; typed result receipts.
- [05]-[FUSION_RANK]: HNSW + GiST + FTS fused through one n-ary RRF fold with per-branch lineage.
- [06]-[FEDERATED_PLAN]: one selection AST across engines; cost-based engine and partial-pushdown; the `PlanWire` Substrait/portable-SQL ingress decoding into the one `SetExpr` algebra.

## [02]-[ENTITY_GRAPH]

- Owner: `FederatedEntity` the source-agnostic element record keyed by stable composite identity and carrying the spatial/vector/lexical/tenant join columns the fusion and lane consumers read; `EntityIdentity` the five-axis identity value; `SourceRef` the originating-document pointer; `FederationFault` the closed `[Union]` fault family deriving from `Expected` and satisfying `IValidationError<FederationFault>` so it lifts into the `Fin`/`Validation` rail unbridged; `EntityGraph` the static surface owning identity derivation, the tenant-gated source-projection merge, the `StoreOp`-routed upsert-into-the-graph, the spatial-containment fold, and the source-agnostic merge of one element across documents.
- Cases: identity composes geometry-ref hash, property-set hash, classification key, spatial-containment ltree path, and provenance origin; a federated entity merges every `SourceRef` projecting the same `EntityIdentity` so one element appears once regardless of source-document count; a `Sync/collaboration#TRANSPORT_AXIS` `SpeckleReceive` graph enters as one `SourceRef` of `SourceKind.ExternalImport` so a Speckle `DataObject` mapped to closed Rasm types at the marshal seam federates as one source projection, never a Speckle-specific entity family. `FederationFault` is `IdentityConflict | TenantMismatch | PlanUnpushable | IdsParse` over `Expected` in the 8270 band — no `SourceUnmergeable` case because the property fold is the total join-semilattice `Crdt.Merge` over the typed `CrdtField` slots, so a property never fails to converge and an unmergeable-source case would be an unreachable decorative arm.
- Entry: `public static EntityIdentity Identify(UInt128 geometryRef, UInt128 psetHash, string classification, string containmentPath, Guid origin)` — pure composite identity; `public static Fin<FederatedEntity> Merge(FederatedEntity prior, SourceRef source, EntityIdentity identity, UInt128 tenant, HashMap<string, CrdtField> incoming)` is the invariant-gated source-projection fold that rejects a cross-tenant source as `FederationFault.TenantMismatch` and a key aliasing two physically distinct elements as `FederationFault.IdentityConflict` before the convergent property fold runs, and `public static Fin<StoreOp<FederatedEntity>> Upsert(FederatedEntity prior, SourceRef source, EntityIdentity identity, UInt128 tenant, HashMap<string, CrdtField> incoming)` lifts a successful merge into a `StoreOp<FederatedEntity>.Upsert` the caller runs through `StoreRail.Run` — the arm's own `SaveChangesAsync` persists the change-tracked `db.Set<FederatedEntity>().Update(merged)` so the durable write rides the one operation algebra and self-emits its cache invalidation, never an injected `persist` delegate and never a nonexistent `DbSet.Upsert`.
- Auto: the entity table is a `PostgresServer` row carrying a `geometry`/`geography` GiST column for spatial containment plus the stored `Envelope` bbox the spatial fusion branch joins on, a jsonb column for the merged property sets, an `ltree` column for the spatial hierarchy path, a `tsvector` generated column over the `Corpus` search text, and the `EmbeddingIdentity` reference keying the HNSW vector branch — so spatial, document, full-text, and vector lanes (`Query/lanes#GEO_LANES`, `#DOCUMENT_LANE`, `#SEARCH_LANES`) all index the one federated table and `#FUSION_RANK` fuses their branches over one row; the stable composite identity is `XxHash128` over the canonical five-axis tuple so a re-ingested model whose source ids changed still federates to the same entity by geometry/pset/containment signature — the same `GraphNode` identity the structural diff matches on; spatial containment derives from the GiST `ST_Contains` fold against the spatial-hierarchy table so an element's room/level/building path is computed once at ingest, not stored per-source.
- Receipt: an upsert rides `store.federation.upsert` carrying the source count merged; the spatial-containment fold rides the `geo.diff.kind` stream.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new identity axis is one column on `EntityIdentity` plus one term in the canonical tuple; a new source kind is one `SourceRef.Kind` value; a new spatial-containment relation is one ltree path policy; a new fault cause is one `FederationFault` case in the closed family; zero new surface — a per-source entity table, a second graph store, an IFC-specific entity family, or an injected-`persist`-delegate write rail is the deleted form because the federated entity rides the one PostGIS substrate, the one `StoreOp` operation algebra, and the `IfcSemantic` model graph (`Query/cache#ARTIFACT_BLOB_INDEX`) projects into it as one source.
- Boundary: identity is source-agnostic by construction — the composite key reads geometry ref, property-set hash, classification, containment path, and provenance origin so the same physical element ingested from a Rhino model, an IFC file, and a gh2 solve federates to one `FederatedEntity` with three `SourceRef` projections; the property sets merge jsonb-deep so a later source's psets union into the entity rather than overwrite, and a value conflict folds through the `Version/commits#CRDT_ALGEBRA` `Crdt.Merge` join-semilattice (each named property is a `CrdtField`, `OrSet`/`LwwRegister`/`MvRegister` per stance) so the merge is convergent; spatial containment is the ltree path the GiST fold computes, never a stored per-source path, so an element moved between rooms updates one path; the federated graph is the substrate `ElementSet`, `RulePlan`, `FusionRank`, and `FederatedPlan` all ride, so a parallel selection table or a second BIM model is the deleted form; the row carries four join columns the consumers demand and no projection-time recompute can supply — the stored `Envelope Bounds` is the bbox the `#FUSION_RANK` spatial branch and the `Query/lanes#GEO_LANES` `SpatialOp` GiST predicate join on (a bare `GeometryRef` hash cannot seed a spatial query), the `Option<EmbeddingIdentity> Embedding` keys the `Query/lanes#SEARCH_LANES` HNSW vector branch (the fusion vector arm is empty without it), the `string Corpus` is the `tsvector`-generated BM25/trigram text the lexical branch matches, and the `UInt128 Tenant` is the `Store/tenancy#TENANCY_RLS` `current_setting('rasm.tenant')::uuid` RLS column every federated read partitions on — so a multi-model hub isolates by tenant by construction rather than by a query-time filter, and the `CrsReconcile` per-model transform-to-world (`Query/lanes#GEO_LANES`) writes `Bounds` in the one world frame so two models in different CRSs federate into one spatial index; the durable write is a `StoreOp<FederatedEntity>.Upsert` through `StoreRail.Run` whose arm calls `SaveChangesAsync` over a change-tracked `Update`, so provider failure converts to `StoreFault` at the one rail site and the op-log row commits with the entity row in one transaction, while a federation-domain rejection (a cross-tenant source surfacing as `FederationFault.TenantMismatch`, an `EntityIdentity` colliding two physically distinct elements under one key as `IdentityConflict`) is the typed `FederationFault` the `Fin<FederatedEntity>` merge returns BEFORE any rail runs — the property fold never adds a third rejection because the join-semilattice `Crdt.Merge` is total over the typed `CrdtField` slots — an injected `Func<FederatedEntity, IO<Unit>>` persist lambda, a phantom `DbSet.Upsert(...).RunAsync()` (no such EF member), and a bare `Error.New` are all the deleted form; the provenance origin axis ties the federated entity to the `Version/provenance#CAUSAL_DAG` so a federated query joins lineage as a dimension; a Speckle import is the `SourceKind.ExternalImport` source projection — the `Sync/collaboration#TRANSPORT_AXIS` `SpeckleReceive` marshal maps the inbound `Base`/`DataObject` graph to closed Rasm op-log entries at the seam and `EntityGraph.Upsert` folds it into the federated entity under one `SourceRef`, so a Speckle-sourced element federates by its geometry/pset/containment signature exactly like a Rhino, IFC, or gh2 source with zero Speckle-specific entity surface; a C2PA-signed artifact is the `SourceKind.SignedArtifact` source projection — the `python:artifacts provenance/credential` lane hard-binds a `ContentIdentity` over the one C#-owned XxHash128 content-key seed and projects it onto the wire, and Persistence is the sole durable identity store that decodes the wire-projected content-key binding and federates the signed artifact by its XxHash128 key (the Python session lane holds no durable store and never re-mints the identity), so a signed, content-addressed artifact resolves against the durable federation table from the one shared seed with zero second hash; the federated entity table is RLS-scoped per tenant through `Store/tenancy#TENANCY_RLS` so a multi-model hub partitions by tenant.

```csharp signature
public sealed class FederationKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<FederationKeyPolicy, string>]
[KeyMemberComparer<FederationKeyPolicy, string>]
public sealed partial class SourceKind {
    public static readonly SourceKind RhinoDocument = new("rhino-doc");
    public static readonly SourceKind IfcModel = new("ifc-model");
    public static readonly SourceKind Gh2Solve = new("gh2-solve");
    public static readonly SourceKind ExternalImport = new("external-import");
    public static readonly SourceKind SignedArtifact = new("signed-artifact");
}

[Union]
public abstract partial record FederationFault : Expected, IValidationError<FederationFault> {
    private FederationFault(string detail, int code) : base(detail, code, None) { }

    public static FederationFault Create(string message) => new IdsParse(message);

    public sealed record IdentityConflict(UInt128 Key, Guid Left, Guid Right) : FederationFault($"<federation-identity-conflict:{Key:x32}>", 8270);
    public sealed record TenantMismatch(UInt128 Key, UInt128 Held, UInt128 Incoming) : FederationFault($"<federation-tenant-mismatch:{Held:x32}!={Incoming:x32}>", 8272);
    public sealed record PlanUnpushable(string Op) : FederationFault($"<federation-plan-unpushable:{Op}>", 8273);
    public sealed record IdsParse(string Detail) : FederationFault($"<ids-parse:{Detail}>", 8274);
}

[ComplexValueObject]
public sealed partial class EntityIdentity {
    public UInt128 GeometryRef { get; }
    public UInt128 PsetHash { get; }
    public string Classification { get; }
    public string ContainmentPath { get; }
    public Guid Origin { get; }

    // The stable composite key — `XxHash128` over the canonical five-axis byte tuple, the value the
    // stored `Key` PK denormalizes and the `Version/commits#CRDT_WIRE` corpus pins by reference.
    public UInt128 Key {
        get {
            var buffer = new ArrayBufferWriter<byte>();
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16), GeometryRef);
            buffer.Advance(16);
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16), PsetHash);
            buffer.Advance(16);
            Encoding.UTF8.GetBytes($"{Classification}|{ContainmentPath}|{Origin:N}", buffer);
            return XxHash128.HashToUInt128(buffer.WrittenSpan);
        }
    }
}

public sealed record SourceRef(SourceKind Kind, Guid Document, string SourceElementId, Instant At);

// `Identity` is the `[ComplexValueObject]` owning the five identity axes (geometry ref, pset hash,
// classification, containment path, origin); EF flattens its members to the `geometry`/`ltree`/relational
// columns the residence table maps, so `GeometryRef` and `ContainmentPath` live ONCE inside `Identity`,
// never duplicated top-level where a drift could split the join column from the identity. `Key` is the
// stored PK (`XxHash128` of the identity), the one denormalization a content-addressed PK needs.
public sealed record FederatedEntity(
    UInt128 Key,
    EntityIdentity Identity,
    Seq<SourceRef> Sources,
    HashMap<string, CrdtField> PropertySets,
    Envelope Bounds,
    Option<EmbeddingIdentity> Embedding,
    string Corpus,
    UInt128 Tenant,
    Option<UInt128> ProvenanceHead,
    DataClassification Classification,
    Instant At);

public static class EntityGraph {
    public static EntityIdentity Identify(UInt128 geometryRef, UInt128 psetHash, string classification, string containmentPath, Guid origin) =>
        EntityIdentity.Create(geometryRef, psetHash, classification, containmentPath, origin);

    // Two invariants gate the merge before the convergent property fold runs, so the interior never
    // sees a foreign tenant or a key aliasing two physically distinct elements: a cross-tenant source
    // is `TenantMismatch`, and a held key carrying a different geometry ref than the incoming identity
    // (the same `XxHash128` key over distinct geometry) is `IdentityConflict`. Property convergence is
    // the join-semilattice `Crdt.Merge`, which is total, so a divergence is structural, not a fault arm.
    public static Fin<FederatedEntity> Merge(FederatedEntity prior, SourceRef source, EntityIdentity identity, UInt128 tenant, HashMap<string, CrdtField> incoming) =>
        tenant != prior.Tenant
            ? Fin.Fail<FederatedEntity>(new FederationFault.TenantMismatch(prior.Key, prior.Tenant, tenant))
            : identity.Key == prior.Key && identity.GeometryRef != prior.Identity.GeometryRef
                ? Fin.Fail<FederatedEntity>(new FederationFault.IdentityConflict(prior.Key, prior.Identity.Origin, identity.Origin))
                : prior with {
                    Sources = prior.Sources.Exists(s => s.Document == source.Document && s.SourceElementId == source.SourceElementId)
                        ? prior.Sources
                        : prior.Sources.Add(source),
                    PropertySets = incoming.Fold(prior.PropertySets, static (acc, slot) =>
                        acc.AddOrUpdate(slot.Key, existing => Crdt.Merge(existing, slot.Value), slot.Value)),
                };

    public static Fin<StoreOp<FederatedEntity>> Upsert(FederatedEntity prior, SourceRef source, EntityIdentity identity, UInt128 tenant, HashMap<string, CrdtField> incoming) =>
        Merge(prior, source, identity, tenant, incoming).Map(static merged =>
            (StoreOp<FederatedEntity>)new StoreOp<FederatedEntity>.Upsert((db, _) => {
                _ = db.Set<FederatedEntity>().Update(merged);
                return ValueTask.FromResult(merged);
            }));

    public static Seq<FederatedEntity> Contained(Seq<FederatedEntity> graph, string ancestorPath) =>
        graph.Filter(entity => entity.Identity.ContainmentPath.StartsWith(ancestorPath, StringComparison.Ordinal));
}
```

| [INDEX] | [AXIS]              | [RESIDENCE]                              | [INDEX_ROUTE]                                          |
| :-----: | :------------------ | :--------------------------------------- | :---------------------------------------------------- |
|  [01]   | geometry ref        | `Identity.GeometryRef` content-key btree column | identity dedup; re-ingest matches by geometry signature |
|  [02]   | spatial geometry    | `geometry`/`Envelope` GiST column        | `Query/lanes#GEO_LANES` `SpatialOp`/`Clash` GiST predicates; `#FUSION_RANK` spatial branch |
|  [03]   | property sets       | merged jsonb column                      | `Query/lanes#DOCUMENT_LANE` `@?`/`@@` predicates      |
|  [04]   | classification      | `tsvector` generated + ltree key         | `Query/lanes#SEARCH_LANES` + classification catalog   |
|  [05]   | search corpus       | `Corpus` text → `tsvector` generated     | `#FUSION_RANK` lexical branch; BM25/trigram match     |
|  [06]   | embedding ref       | `EmbeddingIdentity` → `EmbeddingArity` col | `#FUSION_RANK` vector branch; HNSW/diskann probe     |
|  [07]   | spatial containment | `ltree` hierarchy path                   | `lquery` ancestor/descendant predicates               |
|  [08]   | tenant              | `Tenant` `uuid` RLS column               | `Store/tenancy#TENANCY_RLS` `current_setting` policy  |
|  [09]   | provenance          | `Version/provenance#CAUSAL_DAG` head ref | lineage join dimension                                |

## [03]-[ELEMENT_SET_ALGEBRA]

- Owner: `ElementSet` the polymorphic composable selection record carrying a stable content-addressed set receipt; `SetPredicate` `[Union]` the closed leaf-predicate algebra spatial/document/classification/containment leaves carry; `SetExpr` `[Union]` the selection-tree algebra; `ElementSetAlgebra` the static surface owning literal selection, the boolean/spatial/property/classification combinators, and the stable-receipt fold.
- Cases: `Spatial | Jsonpath | Classification | Containment | Material | Exists` on `SetPredicate`; `Literal | Predicate | ByRule | Union | Intersect | Difference | Closure` on `SetExpr`.
- Entry: `public static ElementSet Evaluate(SetExpr expr, Func<SetExpr, Seq<UInt128>> resolve)` — folds the expression tree into a stable element-key set; `public static UInt128 Receipt(Seq<UInt128> sortedKeys)` derives the content-addressed set identity over the distinct-sorted key set `ElementSet.Of` supplies so the same selection produces the same receipt across runs and peers.
- Auto: an element set is the universal BIM currency — clash, IDS, MVD, QTO, and rule surfaces all consume and produce `ElementSet` values, so a clash result is an `ElementSet`, an IDS pass-set is an `ElementSet`, and a QTO subject is an `ElementSet`; the set receipt is `XxHash128` over the distinct-sorted `UInt128LE`-packed element-key set so two selections yielding the same elements share one receipt and a cached selection result keys on it through the `Query/cache#MODEL_RESULT_INDEX`; the boolean combinators (`Union`/`Intersect`/`Difference`) fold over evaluated leaf sets, and the one `Predicate` leaf carries a `SetPredicate` — `Spatial` carries the `Query/lanes#GEO_LANES` `SpatialOp` row plus its `Geometry` operand and lowers to that row's `ST_*` GiST predicate, `Jsonpath` carries a typed `JsonComparison` and lowers to a jsonb path predicate, `Classification` lowers to a tsvector/classification-catalog predicate, `Containment(Ancestor, Subtree)` lowers to the `ltree` `<@` ancestry predicate or the `~ <lquery>` subtree form over the spatial-hierarchy `ContainmentPath` column so a "every element under room X" selection is a typed leaf rather than an abused `Classification`, and `Material`/`Exists` lower to their jsonb existence forms — so one algebra composes every selection concern with no leaf carrying a raw stringly-typed predicate.
- Receipt: an evaluation rides `store.elementset.eval` carrying the leaf count and the result cardinality; the stable receipt is the cache key.
- Packages: System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, NodaTime, BCL inbox.
- Growth: a new selection primitive is one `SetPredicate` case (lowered by the `Predicate` leaf) or one `SetExpr` tree case; a new combinator is one fold arm; zero new surface — a per-discipline selection class, a saved-search table per concern, a string-query selection DSL, or a `(string GeometryPredicate, byte[] Operand)` raw-string leaf is the deleted form because the algebra is one composable tree the planner lowers, every leaf predicate is a typed `SetPredicate` case, and the receipt is content-addressed; an `ElementSet` is the input and output type of every analysis surface so a clash engine, an IDS validator, and a QTO rollup never mint a parallel selection shape.
- Boundary: `ElementSet` is the one composable currency — every analysis surface (clash, IDS, MVD, QTO, rule) takes an `ElementSet` and yields an `ElementSet` so results compose: a clash result intersected with a classification selection is one `SetExpr.Intersect`, never a join in application code; the receipt is content-addressed over the distinct-sorted key set so it is stable across runs, peers, and tenants — a positional or timestamp-keyed selection id is the deleted form; the `Closure` combinator reads the same `Closure` content-key manifest the op-log carries so a transitive selection (every element referenced by a selection) rides the existing graph closure, never a second reachability walk; every leaf predicate is a typed `SetPredicate` case rather than a string — `Spatial(SpatialOp Op, Geometry Operand)` reuses the `Query/lanes#GEO_LANES` DE-9IM owner so a spatial leaf lowers to that row's translated `ST_*` predicate and a raw `byte[]` WKB operand never crosses the algebra, `Jsonpath(string Path, JsonComparison Cmp)` carries a typed comparison (`Exists | Equals | Contains | GreaterThan | LessThan | Matches`) so the predicate spelling that decides whether the GIN index serves (`Query/domain/postgres#SQL_LAW`) is a closed vocabulary, and `Classification`/`Material`/`Exists` carry their typed forms — so a selection that promised a spatial intersection actually carries the operator and the geometry, never a paraphrase; selection evaluation pushes through `FederatedPlan` so a `Spatial` leaf executes on the GiST index and a `Jsonpath` leaf on the jsonb index in the store, never client-side; an empty `Literal` and a `Difference` to empty both yield the stable empty-set receipt; the algebra rides the federated entity keys (`#ENTITY_GRAPH`) so a selection spanning multiple source documents is one set over the one federated graph; the `ElementSetAlgebra.Receipt` distinct-sorted `UInt128LE`-packed preimage is the byte shape the `Version/commits#CRDT_WIRE` `ContentParityCorpus` freezes as the `elementset.empty` parity vector — the preimage writes one `BinaryPrimitives.WriteUInt128LittleEndian` plus `Advance(16)` per distinct-sorted key (never a `GetSpan` without `Advance`, which leaves `WrittenSpan` empty and silently hashes nothing), so the cross-runtime golden corpus pins this receipt encoding by reference and a sort-order, packing, or advance drift fails one corpus assertion.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<FederationKeyPolicy, string>]
public sealed partial class JsonComparison {
    public static readonly JsonComparison Exists = new("exists");
    public static readonly JsonComparison Equals = new("eq");
    public static readonly JsonComparison Contains = new("contains");
    public static readonly JsonComparison GreaterThan = new("gt");
    public static readonly JsonComparison GreaterOrEqual = new("gte");
    public static readonly JsonComparison LessThan = new("lt");
    public static readonly JsonComparison LessOrEqual = new("lte");
    public static readonly JsonComparison Matches = new("matches");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetPredicate {
    private SetPredicate() { }

    public sealed record Spatial(SpatialOp Op, Geometry Operand) : SetPredicate;
    public sealed record Jsonpath(string Path, JsonComparison Cmp, Option<string> Value) : SetPredicate;
    public sealed record Classification(string SystemPath, Option<string> Value) : SetPredicate;
    public sealed record Containment(string Ancestor, bool Subtree) : SetPredicate;
    public sealed record Material(Option<string> Value) : SetPredicate;
    public sealed record Exists(string Path) : SetPredicate;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetExpr {
    private SetExpr() { }

    public sealed record Literal(Seq<UInt128> Keys) : SetExpr;
    public sealed record Predicate(SetPredicate Leaf) : SetExpr;
    public sealed record ByRule(string RuleId) : SetExpr;
    public sealed record Union(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Intersect(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Difference(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Closure(SetExpr Seed, int Depth) : SetExpr;
}

public readonly record struct ElementSet(UInt128 Receipt, Seq<UInt128> Keys, int Count) {
    public static readonly ElementSet Empty = Of(Seq<UInt128>());

    public static ElementSet Of(Seq<UInt128> keys) {
        var sorted = toSeq(keys.Distinct().OrderBy(static k => k));
        return new ElementSet(ElementSetAlgebra.Receipt(sorted), sorted, sorted.Count);
    }
}

public static class ElementSetAlgebra {
    public static UInt128 Receipt(Seq<UInt128> sortedKeys) {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var key in sortedKeys) {
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16), key);
            buffer.Advance(16);
        }
        return XxHash128.HashToUInt128(buffer.WrittenSpan);
    }

    public static ElementSet Evaluate(SetExpr expr, Func<SetExpr, Seq<UInt128>> resolve) =>
        expr.Switch(
            state: resolve,
            literal:    static (_, lit) => ElementSet.Of(lit.Keys),
            predicate:  static (r, e) => ElementSet.Of(r(e)),
            byRule:     static (r, e) => ElementSet.Of(r(e)),
            closure:    static (r, e) => ElementSet.Of(r(e)),
            union:      static (r, u) => ElementSet.Of(Evaluate(u.Left, r).Keys + Evaluate(u.Right, r).Keys),
            intersect:  static (r, i) => ElementSet.Of(toSeq(Evaluate(i.Left, r).Keys.Intersect(Evaluate(i.Right, r).Keys))),
            difference: static (r, d) => ElementSet.Of(toSeq(Evaluate(d.Left, r).Keys.Except(Evaluate(d.Right, r).Keys))));
}
```

## [04]-[CROSS_DOC_LINKS]

- Owner: `CrossDocLink` the typed inter-document reference record carrying pin-versus-float semantics; `LinkKind` the reference-type vocabulary; `ImpactNode` a transitive-impact-analysis node; `LinkStore` the static surface owning the `StoreOp`-routed link upsert, pin/float resolution, transitive forward/backward impact as one immutable fold, and the change-propagation fold.
- Cases: `Reference | Override | Aggregation | Constraint | Derivation` on `LinkKind`; the `Option<UInt128> PinCommit` is the one reference-stability discriminant — `Some` freezes the link at that source commit (a pinned link), `None` floats it onto the source head, and `CrossDocLink.Pinned => PinCommit.IsSome` derives the stance rather than a parallel `bool`; a change to a pinned source emits a stale-pin impact while a change to a floated source propagates transitively.
- Entry: `public static StoreOp<CrossDocLink> Link(UInt128 from, UInt128 to, LinkKind kind, Option<UInt128> pinCommit, ClockPolicy clocks)` — lifts the link row into a `StoreOp<CrossDocLink>.Upsert` the caller runs through `StoreRail.Run` (the arm's own `SaveChangesAsync` persists the change-tracked `Update`, never an injected `persist` delegate and never a nonexistent `DbSet.Upsert`), so the link write commits with its op-log row in one transaction; `public static Seq<ImpactNode> Impact(Func<UInt128, Seq<CrossDocLink>> adjacency, UInt128 changed, int depth)` walks the link graph to every transitively affected element as a tail-recursive frontier fold, total over both the outbound (forward impact) and inbound (backward impact) adjacency.
- Auto: a link is one `OpLogEntry` of `SyncOpKind.Upsert` on the `link` `ColumnFamily` so the link graph rides the changefeed and converges per the CRDT algebra; pin/float is the one `Option<UInt128> PinCommit` column — `Some` carries the frozen source-commit content key and `None` tracks the source's branch head, so the stability stance reads off the option's inhabitedness rather than a redundant `bool`; transitive impact is a bounded breadth-first walk expressed as an immutable `(frontier, seen, impact)` fold rather than a mutable `HashSet`/`while` loop, so when a source element changes the impact fold yields every downstream element within the depth bound with its link path; change-propagation re-evaluates every floated link's `ElementSet` against the new source state and emits an impact receipt per affected element.
- Receipt: a link mutation rides `store.link.upsert`; an impact analysis rides `store.link.impact` carrying the affected count and max depth; a stale-pin detection rides `store.link.stale-pin`.
- Packages: System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), BCL inbox.
- Growth: a new link semantic is one `LinkKind` row; a new propagation policy is one column on `CrossDocLink`; zero new surface — a per-link-type table, a second reference resolver, or a foreign-key-only model is the deleted form because the link is one typed row on the federated graph and impact is one bounded walk over its adjacency.
- Boundary: pin-versus-float is the one reference-stability axis the `Option<UInt128> PinCommit` discriminant carries — a `Some` link freezes at that commit so a source change never silently propagates and surfaces as a `store.link.stale-pin` impact the operator resolves by re-pinning, while a `None` link tracks the source head so a change propagates transitively through `Impact`, and a parallel `bool Pinned` field beside the option is the deleted form because the stance is recoverable from the value itself; transitive impact is a bounded breadth-first walk over the outbound adjacency so the cost is linear in the reachable-link count within the depth bound, never an unbounded graph traversal; backward impact (every source a changed element depends on) is the same walk over the inbound adjacency; the link graph rides the op-log so links converge like any other op and a link removed on one peer and re-added on another resolves add-wins through the CRDT `OrSet`; a `Derivation` link carries the rule that produced the target so a change-propagation re-runs the `RulePlan` rather than copying a stale value; the link endpoints are federated entity keys (`#ENTITY_GRAPH`) so a cross-document link is one edge over the one federated graph, never a per-document foreign key.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<FederationKeyPolicy, string>]
[KeyMemberComparer<FederationKeyPolicy, string>]
public sealed partial class LinkKind {
    public static readonly LinkKind Reference = new("reference");
    public static readonly LinkKind Override = new("override");
    public static readonly LinkKind Aggregation = new("aggregation");
    public static readonly LinkKind Constraint = new("constraint");
    public static readonly LinkKind Derivation = new("derivation");
}

public sealed record CrossDocLink(
    UInt128 From,
    UInt128 To,
    LinkKind Kind,
    Option<UInt128> PinCommit,
    Option<string> DerivationRule,
    Instant At) {
    public bool Pinned => PinCommit.IsSome;
}

public readonly record struct ImpactNode(UInt128 Element, int Depth, Seq<UInt128> Path, LinkKind Via);

public static class LinkStore {
    public const int MaxImpactDepth = 32;

    public static StoreOp<CrossDocLink> Link(UInt128 from, UInt128 to, LinkKind kind, Option<UInt128> pinCommit, ClockPolicy clocks) {
        var link = new CrossDocLink(from, to, kind, pinCommit, None, clocks.Now);
        return new StoreOp<CrossDocLink>.Upsert((db, _) => {
            _ = db.Set<CrossDocLink>().Update(link);
            return ValueTask.FromResult(link);
        });
    }

    private readonly record struct Frontier(UInt128 Element, int Depth, Seq<UInt128> Path);

    public static Seq<ImpactNode> Impact(Func<UInt128, Seq<CrossDocLink>> adjacency, UInt128 changed, int depth) =>
        Walk(adjacency, int.Min(depth, MaxImpactDepth), HashSet(changed), Seq<ImpactNode>(),
            Seq(new Frontier(changed, 0, Seq<UInt128>())));

    // The dedup membership is the FOLD accumulator, not the frontier-entry snapshot, so two links
    // expanded from one frontier node to the same target enqueue once — reading the outer `seen` would
    // re-admit the second within the same batch. One fold over a node's adjacency yields the next
    // `(seen, impact, frontier)` state the tail-recursion continues from; an empty frontier returns.
    private static Seq<ImpactNode> Walk(
        Func<UInt128, Seq<CrossDocLink>> adjacency, int bound,
        HashSet<UInt128> seen, Seq<ImpactNode> impact,
        Seq<Frontier> frontier) =>
        frontier.HeadOrNone() is { IsSome: true, Case: Frontier head }
            ? head.Depth >= bound
                ? Walk(adjacency, bound, seen, impact, frontier.Tail)
                : Step(adjacency, bound, head, frontier.Tail, seen, impact)
            : impact;

    private static Seq<ImpactNode> Step(
        Func<UInt128, Seq<CrossDocLink>> adjacency, int bound, Frontier head,
        Seq<Frontier> rest, HashSet<UInt128> seen, Seq<ImpactNode> impact) {
        var next = adjacency(head.Element).Fold((Seen: seen, Impact: impact, Frontier: rest), (acc, link) =>
            acc.Seen.Contains(link.To)
                ? acc
                : (acc.Seen.Add(link.To),
                   acc.Impact.Add(new ImpactNode(link.To, head.Depth + 1, head.Path.Add(head.Element), link.Kind)),
                   acc.Frontier.Add(new Frontier(link.To, head.Depth + 1, head.Path.Add(head.Element)))));
        return Walk(adjacency, bound, next.Seen, next.Impact, next.Frontier);
    }

    public static Seq<CrossDocLink> StalePins(Seq<CrossDocLink> links, Func<UInt128, UInt128> sourceHead) =>
        links.Filter(link => link.PinCommit.Map(pin => sourceHead(link.From) != pin).IfNone(false));
}
```

## [05]-[RULE_PLAN]

- Owner: `RuleAst` `[Union]` the declarative rule DSL parsed shape; `RuleResult` `[Union]` the typed result family; `RuleAggregate` the QTO fold vocabulary and `QuantityMeasure` the closed buildingSMART base-quantity vocabulary carrying each measure's jsonb path and canonical unit; `RulePlan` the static surface lowering a rule AST into a predicate/query plan over the federated graph and the element-set algebra, emitting the typed result; `IdsValue` `[Union]` the buildingSMART IDS 1.0 value-restriction grammar; `IdsSpecification`/`IdsFacet`/`IdsRequirement`/`IdsImport` the IDS 1.0 importer projecting a published IDS document's facets into `RuleAst` rows and emitting `IdsConformance` audit receipts.
- Cases: `Select | Where | Requires | Forbids | Clash | Quantity | Property` on `RuleAst` (`Requires`/`Forbids` carry a `SetExpr` requirement selection, not a single `SetPredicate`, so a two-sided bound or a composite `partOf` requirement lowers as an `Intersect` of leaves); `Pass | Fail | Selection | Viewpoint | Quantity` on `RuleResult`; `Count | Sum | Min | Max | Mean` on `RuleAggregate`; `Count | Length | Area | GrossArea | Volume | GrossVolume | Weight | Perimeter` on `QuantityMeasure`; `Simple | Enumeration | Pattern | Bounds | Length` on `IdsValue`; `Entity | Attribute | Classification | Property | Material | PartOf` on `IdsFacet`; `Required | Prohibited | Optional` on `IdsCardinality`.
- Entry: `public static SetExpr Lower(RuleAst rule)` — pure lowering of the rule DSL into an `ElementSet` plan over the typed `SetPredicate` leaves; `public static StoreOp<RuleResult> Evaluate(RuleAst rule, Func<SetExpr, ElementSet> evaluate)` runs the plan as a `StoreOp<RuleResult>` so the clash GiST spatial self-join (`Clash`) and the QTO aggregate (`Aggregate`) execute server-side through `StoreRail.Run` over the GiST/jsonb indexes, folds the typed result, and never takes an injected `clash`/`quantity` delegate; the QTO aggregate binds the `RuleAggregate.Projection` closed-vocabulary body into the statement text and parameterizes the `QuantityMeasure.JsonPath` and the key set through a `FormattableString`, the clash binds both subject key-sets and the `Clash.Tolerance` clearance bound through the same `FormattableString`, so the dynamic identifier rides the one declared seam and no caller input concatenates into SQL.
- Auto: the rule DSL covers clash (two element sets whose geometries overlap in space), IDS/MVD (every element of a selection requires/forbids a property or classification), compliance (a predicate over a selection), and QTO (a `RuleAggregate` over a selection) — so one rule engine serves every validation and takeoff concern; a rule lowers to an `ElementSet` plan over typed `SetPredicate` leaves, so a clash rule selects its two candidate sets and the `Evaluate` `Clash` arm runs the GiST `ST_3DIntersects`/`ST_3DDWithin(tolerance)` spatial self-join between them (`a.key <> b.key`, the offending keys either side of any clash pair) yielding a `Fail` with the offending set and pair count, an IDS rule lowers to a `SetPredicate.Classification` selection plus a typed `SetPredicate.Jsonpath`/`Exists` requirement yielding `Pass`/`Fail` with the violating element set, and a QTO rule lowers to a selection plus a `RuleAggregate` yielding a `Quantity` result; every `Fail` carries the offending `ElementSet` and a typed `RuleViolation` so a downstream surface selects the failures and dispatches on the violation, and a clash `Fail` carries a viewpoint receipt the annotation surface anchors.
- Receipt: an evaluation rides `store.rule.eval` carrying the rule kind, the pass/fail verdict, and the result cardinality; a clash result carries the pair count.
- Packages: System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.Xml.Linq (BCL inbox), BCL inbox.
- Growth: a new rule primitive is one `RuleAst` case; a new result shape is one `RuleResult` case; a new takeoff fold is one `RuleAggregate` row and a new base quantity is one `QuantityMeasure` row carrying its jsonb path and unit; a new IDS facet kind is one `IdsFacet` case plus one `Selection`/`Require`/`Forbid` arm; a new IDS value-restriction is one `IdsValue` case lowering to one `SetExpr` form, and an unsupported facet is the typed `FederationFault.IdsParse` rejection captured at the one parse seam, never a second checker; zero new surface — a per-standard validator (a hard-coded IDS checker, a separate clash engine, a separate QTO calculator) is the deleted form because one DSL lowers to the element-set algebra and emits a typed result, the IDS specification is data parsed into `RuleAst` rows rather than C# predicate code, and the rule itself is a `ByRule` selection leaf so a rule's pass-set composes with any other selection.
- Boundary: the rule DSL lowers to the element-set algebra so a rule is a query plan, never imperative validation code — a hard-coded clash loop, a string-templated IDS check, or a per-discipline QTO calculator is the deleted form; the buildingSMART IDS 1.0 importer is the standards-conformant face of this engine — `IdsImport.Parse` reads an IDS XSD-shaped document through `System.Xml.Linq` into `IdsSpecification` rows inside one `Try.lift` kernel that captures BOTH the `XDocument.Load` throw AND every facet-walk throw, so an unsupported facet rejoins the rail as the typed `FederationFault.IdsParse` it carries through the boundary `IdsParseException` (never a bare `Error.New` and never an exception escaping the capture seam past the projection), `Scope` folds the applicability facets into one `SetExpr` intersection over each facet's `Selection`, and `Lower` projects each requirement facet under its `IdsCardinality` into a `RuleAst.Requires`/`Forbids`/pass-through carrying the facet's full `SetExpr` selection over that scope, so a published IDS check runs against the federated entity graph through the settled `RulePlan.Evaluate` lowering and the `IdsConformance` receipt aligns with `Sync/annotation.md#BCF_PROTOCOL` as an exportable CDE audit artifact, never a parallel checker; the IDS value grammar is a closed `IdsValue` `[Union]` so a facet value is never the magic `"exists"` string the prior fence carried — `Simple` lowers to `JsonComparison.Equals`, `Enumeration` to a key-in-set predicate, `Pattern` to `JsonComparison.Matches` (the IDS `xs:pattern` restriction the GIN-expression-index regex serves), `Bounds` lowers a two-sided `xs:minInclusive`/`maxInclusive` restriction to the `Intersect` of a `GreaterThan` AND a `LessThan` leaf so BOTH bounds survive (collapsing to one comparison and silently dropping the upper bound is the deleted form), `Length` lowers `xs:length`/min/max to the `path.length()` jsonpath bound pair, and a `partOf` facet checks the relation presence AND the related whole's `Entity` selection (never the relation alone), so an IDS restriction-typed facet (`xs:restriction` with `xs:enumeration`/`xs:pattern`/bounds children) lowers to the richer `SetExpr` the RESEARCH gate named rather than the flat `simpleValue` equality, and an unsupported facet maps to one `IdsFacet` case or the typed `FederationFault.IdsParse` rejection, never C# predicate code or a second validation surface; every rule result is typed — `Pass`/`Fail` carry the verdict, the offending `ElementSet`, and a typed `RuleViolation` (`MissingProperty | ForbiddenProperty | Clash | OutOfBounds | PatternMismatch`), `Selection` carries the matched set, `Viewpoint` carries the wire camera blob and the highlighted set the `Sync/annotation#ANCHOR_ALGEBRA` anchors, and `Quantity` carries the typed `QuantityMeasure` (its canonical `Unit` derived from the row, never a parallel `string Unit`), the `RuleAggregate` fold, and the aggregated value — so a downstream surface dispatches on the result and the violation type, never parses a string verdict; the durable `Evaluate` is a `StoreOp<RuleResult>` so the clash runs the GiST `ST_3DIntersects`/`ST_3DDWithin` spatial self-join (`a.key <> b.key` over the two candidate key-sets, the `Clash.Tolerance` clearance bound bound as a parameter) through `StoreRail.Run` (the same rail every store op rides, with provider failure converting to `StoreFault` once) over the spatial index — a managed key-set `Intersect` of the two scopes is the named-rejected form because distinct elements never share a key so it can never surface a clash, and a client-side bounding-box loop or an injected delegate is equally rejected — and an IDS/MVD property requirement lowers to a jsonb path predicate so the validation rides the document index; the rule result `ElementSet` is content-addressed so a re-run of an unchanged rule against an unchanged graph reuses the cached result through `Query/cache#MODEL_RESULT_INDEX`, and an incremental re-test (only the changed elements) rides the structural diff's changed-node set so a clash re-runs over the delta, never the whole graph.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<FederationKeyPolicy, string>]
public sealed partial class RuleAggregate {
    public static readonly RuleAggregate Count = new("count", aggFn: "count", overMeasure: false);
    public static readonly RuleAggregate Sum = new("sum", aggFn: "sum", overMeasure: true);
    public static readonly RuleAggregate Min = new("min", aggFn: "min", overMeasure: true);
    public static readonly RuleAggregate Max = new("max", aggFn: "max", overMeasure: true);
    public static readonly RuleAggregate Mean = new("mean", aggFn: "avg", overMeasure: true);

    public string AggFn { get; }
    public bool OverMeasure { get; }

    // Renders the SQL aggregate body over the measure expression: a `Count` projects `count(*)` (the
    // element cardinality, never `count(<measure>)` which counts present paths), every other row projects
    // `fn(<measure>)`. The closed-vocabulary `AggFn` is the only literal — the measure expression is the
    // already-parameterized jsonb path the caller threads, so no caller input concatenates here.
    public string Projection(string measureExpr) => OverMeasure ? $"{AggFn}({measureExpr})" : $"{AggFn}(*)";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<FederationKeyPolicy, string>]
public sealed partial class QuantityMeasure {
    public static readonly QuantityMeasure Count = new("count", "$.quantity.count", "ea");
    public static readonly QuantityMeasure Length = new("length", "$.quantity.length", "m");
    public static readonly QuantityMeasure Area = new("area", "$.quantity.netArea", "m2");
    public static readonly QuantityMeasure GrossArea = new("gross-area", "$.quantity.grossArea", "m2");
    public static readonly QuantityMeasure Volume = new("volume", "$.quantity.netVolume", "m3");
    public static readonly QuantityMeasure GrossVolume = new("gross-volume", "$.quantity.grossVolume", "m3");
    public static readonly QuantityMeasure Weight = new("weight", "$.quantity.weight", "kg");
    public static readonly QuantityMeasure Perimeter = new("perimeter", "$.quantity.perimeter", "m");

    public string JsonPath { get; }
    public string Unit { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record RuleViolation {
    private RuleViolation() { }

    public sealed record MissingProperty(string Path) : RuleViolation;
    public sealed record ForbiddenProperty(string Path) : RuleViolation;
    public sealed record Clash(int Pairs) : RuleViolation;
    public sealed record OutOfBounds(string Path, Option<string> Lower, Option<string> Upper) : RuleViolation;
    public sealed record PatternMismatch(string Path, string Pattern) : RuleViolation;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record RuleAst {
    private RuleAst() { }

    public sealed record Select(SetExpr Scope) : RuleAst;
    public sealed record Where(RuleAst Source, SetPredicate Leaf) : RuleAst;
    public sealed record Requires(RuleAst Scope, SetExpr Required) : RuleAst;
    public sealed record Forbids(RuleAst Scope, SetExpr Forbidden) : RuleAst;
    public sealed record Clash(RuleAst Left, RuleAst Right, double Tolerance) : RuleAst;
    public sealed record Quantity(RuleAst Scope, QuantityMeasure Measure, RuleAggregate Aggregate) : RuleAst;
    public sealed record Property(RuleAst Scope, string Path) : RuleAst;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuleResult {
    private RuleResult() { }

    public sealed record Pass(ElementSet Subject) : RuleResult;
    public sealed record Fail(ElementSet Subject, ElementSet Offending, RuleViolation Violation) : RuleResult;
    public sealed record Selection(ElementSet Matched) : RuleResult;
    public sealed record Viewpoint(ElementSet Highlighted, ReadOnlyMemory<byte> Camera, ElementSet Offending) : RuleResult;
    public sealed record Quantity(ElementSet Subject, QuantityMeasure Measure, RuleAggregate Aggregate, double Value) : RuleResult;
}

// The keyless `(Key, Directed)` row the clash self-join CTE projects — `SqlQuery<ClashHit>` maps the
// scalar projection a keyed entity type cannot; `Directed` is the windowed total directed-pair count
// carried on every row, halved into the unordered pair count the `Clash` violation reports.
public readonly record struct ClashHit(UInt128 Key, long Directed);

public static class RulePlan {
    // `clash` lowers to the UNION of both scopes — the candidate universe the GiST `ST_3DIntersects`
    // self-join ranges over — not their key `Intersect`: two clashing elements are distinct entities with
    // distinct keys, so a membership intersection is empty and the spatial overlap lives in the `Evaluate`
    // `Clash` SQL kernel, not the set algebra. `Lower` projects the selection plan; the verdict is `Evaluate`.
    public static SetExpr Lower(RuleAst rule) =>
        rule.Switch(
            select:   static s => s.Scope,
            where:    static w => new SetExpr.Intersect(Lower(w.Source), new SetExpr.Predicate(w.Leaf)),
            requires: static r => Lower(r.Scope),
            forbids:  static f => Lower(f.Scope),
            clash:    static c => new SetExpr.Union(Lower(c.Left), Lower(c.Right)),
            quantity: static q => Lower(q.Scope),
            property: static p => Lower(p.Scope));

    // One total generated `Switch` over the seven `RuleAst` arms — a new case breaks every dispatch site
    // at compile time, never a runtime-silent `_` arm wrapping a second switch. The `quantity` and `clash`
    // arms await server-side SQL (`Aggregate` the jsonb measure fold, `Clash` the GiST `ST_3DIntersects`/
    // `ST_3DDWithin` spatial self-join), the verdict arms are synchronous over the already-resolved
    // `evaluate`, and every arm yields one `ValueTask<RuleResult>` so the rail's `Query` shape carries the
    // whole family without a statement-level dispatch beside the generated one. A clash is NEVER a key-set
    // `Intersect` — two clashing elements are DISTINCT entities with distinct keys, so a membership
    // intersection is always empty; the spatial overlap is a `(a.geom, b.geom) WHERE a.key <> b.key`
    // self-join that only the GiST index can answer, the named-rejected client-side bounding-box loop.
    public static StoreOp<RuleResult> Evaluate(RuleAst rule, Func<SetExpr, ElementSet> evaluate) =>
        new StoreOp<RuleResult>.Query((db, token) => rule.Switch(
            state: (Db: db, Eval: evaluate, Token: token),
            quantity: static (ctx, q) => Aggregate(ctx.Db, ctx.Eval(Lower(q.Scope)), q, ctx.Token),
            clash: static (ctx, c) => Clash(ctx.Db, ctx.Eval(Lower(c.Left)), ctx.Eval(Lower(c.Right)), c.Tolerance, ctx.Token),
            requires: static (ctx, r) => ValueTask.FromResult(Verdict(
                ctx.Eval(Lower(r.Scope)),
                ctx.Eval(new SetExpr.Difference(Lower(r.Scope), r.Required)),
                _ => Violated(r.Required, required: true))),
            forbids: static (ctx, f) => ValueTask.FromResult(Verdict(
                ctx.Eval(Lower(f.Scope)),
                ctx.Eval(new SetExpr.Intersect(Lower(f.Scope), f.Forbidden)),
                _ => Violated(f.Forbidden, required: false))),
            select:   static (ctx, s) => ValueTask.FromResult((RuleResult)new RuleResult.Selection(ctx.Eval(s.Scope))),
            where:    static (ctx, w) => ValueTask.FromResult((RuleResult)new RuleResult.Selection(ctx.Eval(Lower(w)))),
            property: static (ctx, p) => ValueTask.FromResult((RuleResult)new RuleResult.Selection(ctx.Eval(Lower(p.Scope))))));

    private static async ValueTask<RuleResult> Aggregate(DbContext db, ElementSet subject, RuleAst.Quantity rule, CancellationToken token) {
        // `Projection` is a closed `RuleAggregate` vocabulary literal baked into the statement text (the
        // `count(*)`/`sum`/`min`/`max`/`avg` body over the `double precision`-cast measure, never caller
        // input) so the dynamic identifier rides the one declared seam, while the jsonb `path` and the
        // `key` set bind as parameters through the `FormattableString`. The whole aggregate is wrapped in
        // `coalesce(...::float8, 0)` so it maps to the keyless `double` projection on every row — `count`
        // returns `bigint` and `min`/`max`/`sum` over `numeric` return `numeric`, both of which fail or
        // truncate against `SqlQuery<double>` if returned bare, and an all-null measure yields 0 rather
        // than a `NULL` the non-nullable `double` projection cannot bind. `Count` is `count(*)` over the
        // subject rows, never `count(jsonb_path_query_first(...))` (which counts present paths, not
        // elements), so a takeoff count is the element cardinality.
        // `{predicate}`/`{Projection}` interpolate verbatim (their `{0}` is plain-literal text); the directly
        // written `{{1}}` doubles its braces so the `$"""..."""` emits a literal `{1}` placeholder for
        // `Create` to bind, never the interpolation hole `{1}` that would render the integer 1.
        FormattableString query = FormattableStringFactory.Create(
            $"""
            SELECT coalesce({rule.Aggregate.Projection("jsonb_path_query_first(property_sets, {0}::jsonpath)::float8")}, 0)::float8
            FROM federated_entity WHERE key = ANY({{1}})
            """,
            rule.Measure.JsonPath, subject.Keys.ToArray());
        var value = await db.Database.SqlQuery<double>(query).SingleAsync(token).ConfigureAwait(false);
        return new RuleResult.Quantity(subject, rule.Measure, rule.Aggregate, value);
    }

    // The clash arm IS the one verdict that needs server geometry: a GiST spatial self-join over the
    // provisioned `geom` column (the full `geometry`/`geometryz` the `Auto` table carries and the GiST
    // index covers — the narrow-phase exact geometry, never the `Envelope Bounds` bbox which over-reports),
    // hard `ST_3DIntersects` when `tolerance` is 0 and the `ST_3DDWithin(a, b, tolerance)` clearance form
    // otherwise (so the decorative `Clash.Tolerance` becomes the load-bearing clearance bound), with
    // `a.key <> b.key` excluding self-pairs. Raw SQL reads the `geom` column directly — the
    // `FederatedEntity` record maps the identity/bbox/lane columns, the lane owner owns the `Geometry`
    // value chain, and the two meet at this one wire column rather than a duplicated managed geometry. The CTE projects one
    // row per offending element key (either side of any clash pair) carrying the windowed total directed
    // count; the unordered pair count is that total halved (each pair appears as `(a,b)` and `(b,a)`). Both
    // key-sets and the tolerance bind as parameters, the `3d` predicate name is the only closed literal.
    private static async ValueTask<RuleResult> Clash(DbContext db, ElementSet left, ElementSet right, double tolerance, CancellationToken token) {
        // `predicate` is a plain-literal string so its `{2}` is literal text inserted verbatim by the outer
        // interpolation; the directly written `{{0}}`/`{{1}}` double their braces so the `$"""..."""` emits
        // literal `{0}`/`{1}` placeholders for `Create` to bind, never the integer-rendering holes.
        var predicate = tolerance > 0 ? "ST_3DDWithin(a.geom, b.geom, {2})" : "ST_3DIntersects(a.geom, b.geom)";
        FormattableString query = FormattableStringFactory.Create(
            $"""
            WITH pairs AS (
                SELECT a.key AS key FROM federated_entity a JOIN federated_entity b
                ON a.key <> b.key AND {predicate}
                WHERE a.key = ANY({{0}}) AND b.key = ANY({{1}}))
            SELECT DISTINCT key AS "Key", count(*) OVER () AS "Directed" FROM pairs
            """,
            left.Keys.ToArray(), right.Keys.ToArray(), tolerance);
        var hits = toSeq(await db.Database.SqlQuery<ClashHit>(query).ToListAsync(token).ConfigureAwait(false));
        var offending = ElementSet.Of(hits.Map(static h => h.Key));
        var subject = ElementSet.Of(left.Keys + right.Keys);
        return offending.Count == 0
            ? new RuleResult.Pass(subject)
            : new RuleResult.Fail(subject, offending, new RuleViolation.Clash((int)(hits.Head.Map(static h => h.Directed).IfNone(0L) / 2)));
    }

    private static RuleResult Verdict(ElementSet subject, ElementSet offending, Func<ElementSet, RuleViolation> violation) =>
        offending.Count == 0
            ? new RuleResult.Pass(subject)
            : new RuleResult.Fail(subject, offending, violation(offending));

    private static RuleViolation Violated(SetExpr requirement, bool required) =>
        Governing(requirement) switch {
            SetPredicate.Jsonpath { Cmp: var cmp, Path: var path, Value: var value } when cmp == JsonComparison.Matches =>
                new RuleViolation.PatternMismatch(path, value.IfNone("")),
            SetPredicate.Jsonpath { Cmp: var cmp, Path: var path, Value: var value } when cmp == JsonComparison.GreaterThan || cmp == JsonComparison.LessThan =>
                new RuleViolation.OutOfBounds(path, cmp == JsonComparison.GreaterThan ? value : None, cmp == JsonComparison.LessThan ? value : None),
            SetPredicate.Exists e => required ? new RuleViolation.MissingProperty(e.Path) : new RuleViolation.ForbiddenProperty(e.Path),
            var leaf => required ? new RuleViolation.MissingProperty(PathOf(leaf)) : new RuleViolation.ForbiddenProperty(PathOf(leaf)),
        };

    // The leftmost leaf of a requirement selection names the violated facet; a two-sided bound or a
    // composite `partOf` reports its governing predicate, the diagnostic anchor for the violation row.
    private static SetPredicate Governing(SetExpr expr) =>
        expr.Switch(
            predicate:  static p => p.Leaf,
            intersect:  static i => Governing(i.Left),
            union:      static u => Governing(u.Left),
            difference: static d => Governing(d.Left),
            literal:    static _ => new SetPredicate.Exists("$"),
            byRule:     static _ => new SetPredicate.Exists("$"),
            closure:    static c => Governing(c.Seed));

    private static string PathOf(SetPredicate predicate) =>
        predicate.Switch(
            jsonpath:       static j => j.Path,
            exists:         static e => e.Path,
            classification: static c => c.SystemPath,
            containment:    static c => c.Ancestor,
            material:       static _ => "$.material",
            spatial:        static _ => "$.geometry");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<FederationKeyPolicy, string>]
public sealed partial class IdsCardinality {
    public static readonly IdsCardinality Required = new("required");
    public static readonly IdsCardinality Prohibited = new("prohibited");
    public static readonly IdsCardinality Optional = new("optional");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record IdsValue {
    private IdsValue() { }

    public sealed record Simple(string Value) : IdsValue;
    public sealed record Enumeration(Seq<string> Allowed) : IdsValue;
    public sealed record Pattern(string Regex) : IdsValue;
    // The `xs:restriction` numeric-bound grammar in full: inclusive AND exclusive on both sides, each
    // lowering to its own comparison so an `xs:minInclusive` keeps the boundary value (`>=`) while an
    // `xs:minExclusive` excludes it (`>`) — folding inclusive onto a strict `>` silently drops the
    // boundary, and dropping the exclusive forms makes a half the restriction grammar unrepresentable.
    public sealed record Bounds(Option<string> MinInclusive, Option<string> MaxInclusive, Option<string> MinExclusive, Option<string> MaxExclusive) : IdsValue;
    public sealed record Length(Option<int> Exact, Option<int> Min, Option<int> Max) : IdsValue;

    public SetExpr Lower(string path) =>
        this switch {
            Simple s => Leaf(new SetPredicate.Jsonpath(path, JsonComparison.Equals, Some(s.Value))),
            Enumeration e => Leaf(new SetPredicate.Jsonpath(path, JsonComparison.Contains, Some(string.Join('|', e.Allowed)))),
            Pattern p => Leaf(new SetPredicate.Jsonpath(path, JsonComparison.Matches, Some(p.Regex))),
            // Every present bound intersects as its own leaf so a multi-sided restriction keeps all sides;
            // inclusive lowers to `>=`/`<=`, exclusive to `>`/`<`, and collapsing any to one comparison
            // silently drops a side.
            Bounds b => Conjoin(path,
                b.MinInclusive.Map(v => new SetPredicate.Jsonpath(path, JsonComparison.GreaterOrEqual, Some(v))),
                b.MaxInclusive.Map(v => new SetPredicate.Jsonpath(path, JsonComparison.LessOrEqual, Some(v))),
                b.MinExclusive.Map(v => new SetPredicate.Jsonpath(path, JsonComparison.GreaterThan, Some(v))),
                b.MaxExclusive.Map(v => new SetPredicate.Jsonpath(path, JsonComparison.LessThan, Some(v)))),
            // `xs:length` is an exact equality on `.length()`; `xs:minLength`/`xs:maxLength` are inclusive.
            Length l => Conjoin($"{path}.length()",
                l.Exact.Map(n => new SetPredicate.Jsonpath($"{path}.length()", JsonComparison.Equals, Some($"{n}"))),
                l.Min.Map(n => new SetPredicate.Jsonpath($"{path}.length()", JsonComparison.GreaterOrEqual, Some($"{n}"))),
                l.Max.Map(n => new SetPredicate.Jsonpath($"{path}.length()", JsonComparison.LessOrEqual, Some($"{n}")))),
        };

    private static SetExpr Leaf(SetPredicate leaf) => new SetExpr.Predicate(leaf);

    // Intersects every present bound leaf into one selection; an all-empty restriction degrades to an
    // existence check on the path so the facet still constrains presence rather than vanishing.
    private static SetExpr Conjoin(string path, params ReadOnlySpan<Option<SetPredicate>> leaves) =>
        toSeq(leaves.ToArray()).Choose(static o => o.Map(Leaf)) is { IsEmpty: false } present
            ? present.Tail.Fold(present.Head.IfNone(() => Leaf(new SetPredicate.Exists(path))),
                static (acc, leaf) => new SetExpr.Intersect(acc, leaf))
            : Leaf(new SetPredicate.Exists(path));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record IdsFacet {
    private IdsFacet() { }

    public sealed record Entity(string Name, Option<string> PredefinedType) : IdsFacet;
    public sealed record Attribute(string Name, Option<IdsValue> Value) : IdsFacet;
    public sealed record Classification(string System, Option<IdsValue> Value) : IdsFacet;
    public sealed record Property(string PropertySet, string BaseName, Option<IdsValue> Value, Option<string> DataType) : IdsFacet;
    public sealed record Material(Option<IdsValue> Value) : IdsFacet;
    public sealed record PartOf(string Relation, Entity Whole) : IdsFacet;

    public string Path =>
        this switch {
            Entity => "$.entity",
            Attribute a => $"$.{a.Name}",
            Classification c => $"$.classification.{c.System}",
            Property p => $"$.{p.PropertySet}.{p.BaseName}",
            Material => "$.material",
            PartOf po => $"$.partOf.{po.Relation}",
        };

    public SetExpr Selection =>
        this switch {
            Entity e => e.PredefinedType.Match(
                Some: pt => new SetExpr.Intersect(
                    new SetExpr.Predicate(new SetPredicate.Jsonpath("$.entity", JsonComparison.Equals, Some(e.Name))),
                    new SetExpr.Predicate(new SetPredicate.Jsonpath("$.predefinedType", JsonComparison.Equals, Some(pt)))),
                None: () => new SetExpr.Predicate(new SetPredicate.Jsonpath("$.entity", JsonComparison.Equals, Some(e.Name)))),
            // A classification/material facet carries a full `IdsValue` restriction (an `xs:enumeration`
            // of allowed systems, an `xs:pattern` reference, a bound) — the typed `Classification`/
            // `Material` leaf anchors the system/medium membership while the restriction lowers onto the
            // facet path so an enumeration or pattern restriction survives; collapsing the value to its
            // `Simple` case (the prior `(v as IdsValue.Simple)?.Value` cast) silently dropped every
            // enumeration/pattern/bounds restriction on a classification facet.
            Classification c => Constrain(
                new SetExpr.Predicate(new SetPredicate.Classification(c.System, Simple(c.Value))),
                c.Value, $"$.classification.{c.System}"),
            Material m => Constrain(
                new SetExpr.Predicate(new SetPredicate.Material(Simple(m.Value))),
                m.Value, "$.material"),
            // PartOf checks the relation presence AND the related whole's entity, never the relation alone.
            PartOf po => new SetExpr.Intersect(
                new SetExpr.Predicate(new SetPredicate.Exists($"$.partOf.{po.Relation}")),
                po.Whole.Selection),
            _ => Value.Match(Some: v => v.Lower(Path), None: () => new SetExpr.Predicate(new SetPredicate.Exists(Path))),
        };

    // The simple-value face of a restriction (the `Classification`/`Material` leaf reads the system or
    // medium literal directly); a richer restriction keeps `None` here and survives via `Constrain`.
    private static Option<string> Simple(Option<IdsValue> value) =>
        value.Bind(static v => v is IdsValue.Simple s ? Some(s.Value) : None);

    // The typed leaf alone for a simple/absent value; the typed leaf intersected with the lowered
    // restriction when the value is an enumeration/pattern/bounds form, so no restriction is dropped.
    private static SetExpr Constrain(SetExpr leaf, Option<IdsValue> value, string path) =>
        value.Match(
            Some: v => v is IdsValue.Simple ? leaf : new SetExpr.Intersect(leaf, v.Lower(path)),
            None: () => leaf);

    private Option<IdsValue> Value =>
        this switch { Attribute a => a.Value, Property p => p.Value, _ => None };
}

public sealed record IdsRequirement(IdsFacet Facet, IdsCardinality Cardinality);

public sealed record IdsSpecification(
    string Name,
    Option<string> Identifier,
    Seq<string> ApplicableSchemas,
    Seq<IdsFacet> Applicability,
    Seq<IdsRequirement> Requirements);

public sealed record IdsConformance(
    string Specification,
    RuleResult Result,
    int Checked,
    int Passing,
    int Failing,
    Instant At);

// XSD-walk carrier: the only throw inside the captured `Parse` kernel, so an unsupported facet
// rejoins the rail as the typed `FederationFault.IdsParse` at the one boundary that captures it,
// never escaping as an unstructured exception past the `Try.lift` seam.
public sealed class IdsParseException(FederationFault.IdsParse fault) : Exception(fault.Message) {
    public FederationFault.IdsParse Fault { get; } = fault;
}

public static class IdsImport {
    private static readonly XNamespace Ids = "http://standards.buildingsmart.org/IDS";

    public static Fin<Seq<IdsSpecification>> Parse(Stream document) =>
        Try.lift(() => toSeq(XDocument.Load(document)
                .Root!.Element(Ids + "specifications")!.Elements(Ids + "specification").Map(Specification)))
            .Run()
            .MapFail(static error => error.Exception.Case is IdsParseException carried
                ? carried.Fault
                : new FederationFault.IdsParse(error.Message));

    public static SetExpr Scope(IdsSpecification spec) =>
        spec.Applicability.Fold(
            (SetExpr)new SetExpr.Literal(Seq<UInt128>()),
            static (acc, facet) => acc is SetExpr.Literal { Keys.IsEmpty: true }
                ? facet.Selection
                : new SetExpr.Intersect(acc, facet.Selection));

    public static Seq<RuleAst> Lower(IdsSpecification spec) {
        var scope = new RuleAst.Select(Scope(spec));
        return spec.Requirements.Map(req => req.Cardinality.Switch(
            state: (Scope: (RuleAst)scope, Req: req),
            required:   static ctx => new RuleAst.Requires(ctx.Scope, ctx.Req.Facet.Selection),
            prohibited: static ctx => new RuleAst.Forbids(ctx.Scope, ctx.Req.Facet.Selection),
            optional:   static ctx => ctx.Scope));
    }

    private static IdsSpecification Specification(XElement element) =>
        new(
            element.Attribute("name")?.Value ?? "",
            Optional(element.Attribute("identifier")?.Value),
            toSeq((element.Attribute("ifcVersion")?.Value ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)),
            toSeq(element.Element(Ids + "applicability")!.Elements().Map(Facet)),
            toSeq(element.Element(Ids + "requirements")!.Elements().Map(req =>
                new IdsRequirement(Facet(req), IdsCardinality.Get(req.Attribute("cardinality")?.Value ?? "required")))));

    private static IdsFacet Facet(XElement node) =>
        node.Name.LocalName switch {
            "entity" => new IdsFacet.Entity(Simple(node, "name"), Optional(Simple(node, "predefinedType")).Filter(static v => v.Length > 0)),
            "attribute" => new IdsFacet.Attribute(Simple(node, "name"), Value(node, "value")),
            "classification" => new IdsFacet.Classification(Simple(node, "system"), Value(node, "value")),
            "property" => new IdsFacet.Property(Simple(node, "propertySet"), Simple(node, "baseName"), Value(node, "value"), Optional(node.Attribute("dataType")?.Value)),
            "material" => new IdsFacet.Material(Value(node, "value")),
            "partOf" => new IdsFacet.PartOf(node.Attribute("relation")?.Value ?? "", (IdsFacet.Entity)Facet(node.Element(Ids + "entity")!)),
            var other => throw new IdsParseException(new FederationFault.IdsParse($"<ids-facet-unsupported:{other}>")),
        };

    private static Option<IdsValue> Value(XElement node, string child) =>
        node.Element(Ids + child) is { } element
            ? element.Element(Ids + "restriction") is { } restriction
                ? Some(Restriction(restriction))
                : Optional(element.Element(Ids + "simpleValue")?.Value).Filter(static v => v.Length > 0).Map(static v => (IdsValue)new IdsValue.Simple(v))
            : None;

    // The `xs:restriction` facet dispatch: enumeration and pattern are exclusive shapes; a `length`/
    // `minLength`/`maxLength` child marks the `Length` form, otherwise the inclusive/exclusive numeric
    // bounds compose `Bounds`. Each facet maps to its `IdsValue` case so no restriction child is dropped
    // (the prior reader produced neither `Length` nor the exclusive bounds, leaving both unreachable).
    private static IdsValue Restriction(XElement restriction) =>
        restriction.Elements(Ids + "enumeration").ToArray() is { Length: > 0 } enums
            ? new IdsValue.Enumeration(toSeq(enums.Map(static e => e.Attribute("value")?.Value ?? "")))
            : restriction.Element(Ids + "pattern")?.Attribute("value")?.Value is { } pattern
                ? new IdsValue.Pattern(pattern)
                // `is not null` binds tighter than `??`, so a `length ?? minLength ?? maxLength is not null`
                // spelling parses as `length ?? minLength ?? (maxLength is not null)` — an `XElement ?? bool`
                // type error that also mis-detects the facet; the length family is present iff ANY of the
                // three length children exists.
                : restriction.Element(Ids + "length") is not null || restriction.Element(Ids + "minLength") is not null || restriction.Element(Ids + "maxLength") is not null
                    ? new IdsValue.Length(
                        FacetInt(restriction, "length"),
                        FacetInt(restriction, "minLength"),
                        FacetInt(restriction, "maxLength"))
                    : new IdsValue.Bounds(
                        Facet(restriction, "minInclusive"),
                        Facet(restriction, "maxInclusive"),
                        Facet(restriction, "minExclusive"),
                        Facet(restriction, "maxExclusive"));

    private static Option<string> Facet(XElement restriction, string facet) =>
        Optional(restriction.Element(Ids + facet)?.Attribute("value")?.Value);

    private static Option<int> FacetInt(XElement restriction, string facet) =>
        Facet(restriction, facet).Bind(static v => parseInt(v));

    private static string Simple(XElement node, string child) =>
        node.Element(Ids + child)?.Element(Ids + "simpleValue")?.Value ?? "";
}
```

## [06]-[FUSION_RANK]

- Owner: `FusionBranch` the one ranked-retrieval-branch row carrying its lane name, RRF weight, and SQL projection; `FusionCandidate` a scored hit carrying its per-branch rank map and lineage; `FusionRank` the static surface fusing an arbitrary set of `FusionBranch` rows — the pgvector HNSW, PostGIS GiST, and pg_search/FTS branches — into one reciprocal-rank result with per-hit provenance through one n-ary RRF fold.
- Cases: a `FusionBranch` row per ranked retrieval lane (`vector`, `spatial`, `lexical`, and any future lane); each candidate carries its rank in every branch that returned it (absent where it did not) plus the federated entity key and its provenance head.
- Entry: `public static Seq<FusionCandidate> Fuse(Seq<FusionBranch> branches, Seq<Seq<(UInt128 Key, int Rank)>> ranked, int k, int rrfConstant = DefaultRrfConstant)` — folds N ranked candidate sets, one per branch, into one reciprocal-rank-fused top-k with the full per-branch contribution map, so the two-branch `HybridRetrieve` (vector + BM25) and the three-branch spatial-vector-lexical fusion are the same fold at different arity; `public static (string Sql, Seq<NpgsqlParameter> Parameters) FuseSql(Seq<FusionBranch> branches, int k, int rrfConstant)` projects the N-way RRF CTE by unioning each branch's `ROW_NUMBER()`-ranked SQL and returns the composed statement paired with its bound `NpgsqlParameter` list for `db.Database.SqlQueryRaw<FusionHit>(Sql, [.. Parameters])` — the dynamically composed CTE text is the raw door (the lane keys interpolate the CTE names over a closed vocabulary), the `(key, fused)` projection is the keyless `FusionHit` row (`SqlQueryRaw<TResult>` carries a keyless projection, where `FromSqlRaw<TEntity>` is entity-only and cannot map a CTE scalar), and the branch operands, the per-branch weights, the `rrfConstant`, and the `k` limit all bind as parameters, never as interpolated literals.
- Auto: the indexes exist and are world-class individually — pgvector HNSW/diskann on the `EmbeddingArity` column (`Query/lanes#SEARCH_LANES`), PostGIS GiST on the `geometry`/`Envelope` column (`Query/lanes#GEO_LANES`), and pg_search BM25 / native tsvector on the `Corpus` (`Query/lanes#SEARCH_LANES`) — so this owner is purely the fusion-ranking layer: it sums `weight / (rrfConstant + rank)` across every branch that returned a candidate and re-ranks, never a fourth index or a learned reranker; each fused candidate carries its federated entity key so the result is a stable `ElementSet`, and each carries its provenance head so a fused hit joins lineage as a per-hit dimension; the `Query/lanes#SEARCH_LANES` two-branch RRF (vector + BM25) is not a separate implementation but the same `FusionRank.Fuse` fold over a two-row `FusionBranch` set, so a geometry-aware find-similar adds the spatial `FusionBranch` row and the fold is unchanged.
- Receipt: a fusion rides `store.fusion.rank` carrying the per-branch hit counts and the fused top-k cardinality; each hit's per-branch ranks ride the `search.*` fact family.
- Packages: Pgvector.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, Npgsql, System.IO.Hashing, LanguageExt.Core, NodaTime.
- Growth: a new fusable index is one `FusionBranch` row carrying its weight and SQL projection — the fold and the CTE generalize over the branch set with zero new arm; a new weighting policy is one `Weight` value on a `FusionBranch`; zero new surface — a learned cross-encoder reranker, a fourth parallel index, a per-modality result class, or a second hand-written `Score`/`FuseSql`/`DefaultRrfConstant` beside the `HybridRetrieve` two-way one is the deleted form because the branches are world-class, only the fusion is net-new, and the fusion is one n-ary RRF fold the two-way and three-way uses both ride.
- Boundary: fusion is reciprocal-rank summation over the branch set so it needs no learned model and no training — a candidate's score is `Σ weight_b / (rrfConstant + rank_b)` over the branches that returned it; the fold and the CTE are parameterized by `Seq<FusionBranch>` so the two-branch `Query/lanes#SEARCH_LANES` `HybridRetrieve.Fuse` and the three-branch federated fusion are literally one `FusionRank.Fuse`/`FusionRank.FuseSql` at arity 2 and 3 — a parallel two-way implementation with its own `Score`, its own CTE string, and its own `DefaultRrfConstant` is the deleted form, so the lanes owner delegates its two-branch fusion to this fold rather than re-deriving RRF; the fused result is a stable `ElementSet` keyed on federated entity ids so it composes with the element-set algebra and the rule engine; each hit carries its provenance head so a federated search joins lineage per-hit — a search result whose source lineage is unknown is the deleted form; a branch that returns no candidate for a key contributes zero, so a hit strong in one modality still surfaces; the `rrfConstant` defaults to 60 per the standard RRF damping and a `FusionBranch.Weight` defaults to 1.0 so an unweighted fusion is the branch mean; the fusion projects identities then re-queries the federated graph by the surviving keys so it never materializes N full candidate payloads; the SQL form is one CTE unioning each branch's `ROW_NUMBER()`-ranked projection executed through `SqlQueryRaw<FusionHit>(Sql, [.. Parameters])` (the raw door, because the CTE text is composed dynamically over the closed lane-key vocabulary and the keyless `(key, fused)` projection cannot ride the entity-only `FromSqlRaw<TEntity>`) — the per-query probe vector, query point, radius, and term string ride the branch's `NpgsqlParameter` `Operands` bound through `FusionBranch.Bind`, and the per-branch weight, the `rrfConstant`, and the `k` limit bind as `@w_*`/`@rrf`/`@k` parameters, so a `$probe`/`$terms` token spliced as a raw literal (which binds against nothing) and a `{k}`/`{weight}` numeric concatenation are both the deleted form; the spatial branch is one more `FusionBranch` row, never a special case.

```csharp signature
// A branch is its lane key, its RRF weight, the `RankSql` whose value-holes are `@lane_*` named
// markers, and the ordered `NpgsqlParameter` set that binds them — the probe vector, query point,
// radius, and term string are bound values, never interpolated literals or dangling `$probe` tokens
// the binder cannot reach (the two-door raw-SQL leg with mechanical parameter binding, never a
// hand-spliced literal). The static rows are the canonical column spellings the binder clones and
// re-binds per query through `Bind`; the lane name namespaces each branch's parameters so an N-way
// CTE never collides two `@probe` markers.
public sealed record FusionBranch(string Lane, double Weight, string RankSql, Seq<NpgsqlParameter> Operands) {
    public static readonly FusionBranch Vector = new("vector", 1.0,
        "SELECT key, ROW_NUMBER() OVER (ORDER BY embedding <=> @vector_probe) AS rank FROM federated_entity ORDER BY embedding <=> @vector_probe", Empty);
    public static readonly FusionBranch Spatial = new("spatial", 1.0,
        "SELECT key, ROW_NUMBER() OVER (ORDER BY bounds <-> @spatial_point) AS rank FROM federated_entity WHERE ST_DWithin(geom, @spatial_point, @spatial_radius)", Empty);
    public static readonly FusionBranch Lexical = new("lexical", 1.0,
        "SELECT key, ROW_NUMBER() OVER (ORDER BY pdb.score(key) DESC) AS rank FROM federated_entity WHERE corpus @@@ pdb.parse(@lexical_terms) ORDER BY pdb.score(key) DESC", Empty);

    public FusionBranch Bind(params ReadOnlySpan<NpgsqlParameter> operands) => this with { Operands = toSeq(operands) };
}

public readonly record struct FusionCandidate(
    UInt128 Key,
    HashMap<string, int> BranchRanks,
    double FusedScore,
    Option<UInt128> ProvenanceHead);

// The keyless `(key, fused)` row shape the N-way RRF CTE projects — `SqlQueryRaw<FusionHit>` maps a CTE
// scalar projection a keyed entity type cannot, so the fused identities re-query the graph by `Key`.
public readonly record struct FusionHit(UInt128 Key, double Fused);

public static class FusionRank {
    public const int DefaultRrfConstant = 60;

    public static Seq<FusionCandidate> Fuse(Seq<FusionBranch> branches, Seq<Seq<(UInt128 Key, int Rank)>> ranked, int k, int rrfConstant = DefaultRrfConstant) {
        var lanes = branches.Zip(ranked, static (branch, hits) =>
            (branch.Lane, branch.Weight, Hits: hits.ToHashMap(static hit => hit.Key, static hit => hit.Rank)));
        var keys = toSeq(lanes.Bind(static lane => lane.Hits.Keys).Distinct());
        return toSeq(keys
            .Map(key => lanes.Fold(
                new FusionCandidate(key, HashMap<string, int>(), 0.0, None),
                (candidate, lane) => lane.Hits.Find(key).Match(
                    Some: rank => candidate with {
                        BranchRanks = candidate.BranchRanks.Add(lane.Lane, rank),
                        FusedScore = candidate.FusedScore + lane.Weight / (rrfConstant + rank),
                    },
                    None: () => candidate)))
            .OrderByDescending(static c => c.FusedScore)
            .Take(k));
    }

    // The N-way RRF CTE is one parameterized statement for `db.Database.SqlQueryRaw<FusionHit>(Sql, [.. Parameters])`:
    // the per-branch `Weight`, the `rrfConstant` damping, and the `k` limit bind as `@w_*`/`@rrf`/`@k`
    // parameters, the branch operands flow through unchanged, and the only interpolated tokens are the
    // closed lane keys that name the CTEs — so no caller string, score, or numeric literal concatenates
    // into SQL; the keyless `(key, fused)` projection rides `SqlQueryRaw`, never the entity-only `FromSqlRaw`.
    public static (string Sql, Seq<NpgsqlParameter> Parameters) FuseSql(Seq<FusionBranch> branches, int k, int rrfConstant) {
        var ctes = string.Join(",\n", branches.Map(b => $"{b.Lane}_branch AS ({b.RankSql} LIMIT @k)"));
        var unions = string.Join("\nUNION ALL ", branches.Map(b => $"SELECT key, rank, @w_{b.Lane} AS weight FROM {b.Lane}_branch"));
        var scalars = branches.Map(b => new NpgsqlParameter($"w_{b.Lane}", b.Weight))
            .Append(new NpgsqlParameter("rrf", rrfConstant))
            .Append(new NpgsqlParameter("k", k));
        return ($"""
            WITH {ctes}
            SELECT key, SUM(weight / (@rrf + rank)) AS fused
            FROM ({unions}) ranked
            GROUP BY key ORDER BY fused DESC LIMIT @k
            """,
            branches.Bind(static b => b.Operands) + scalars);
    }
}
```

## [07]-[FEDERATED_PLAN]

- Owner: `PlanNode` `[Union]` the cross-store query AST; `EngineCost` the per-engine cost estimate row; `FederatedPlan` the static surface owning predicate pushdown across stores, cost-based engine selection, partial-pushdown splitting, and the cross-store join fold.
- Cases: `Scan | Filter | Join | Aggregate | Pushdown | Materialize` on `PlanNode`, each node carrying typed operands — `Filter` the `SetPredicate` leaf, `Join` the `SetCombine` (`Union | Intersect | Difference`) discriminant, `Aggregate` the `QuantityMeasure` plus `RuleAggregate` fold — never a pre-rendered SQL string; cost-based selection ranks every candidate engine a leaf admits (a spatial leaf serves from GiST or seqscan, a jsonb leaf from the GIN index or seqscan) through the live `EngineCost` — `EngineCost.Pushable` gates a candidate out and `EngineCost.Estimate` ranks the survivors, so the leaf engine is data from `pg_stat_statements`/route facts rather than a static map and a GIN-unpushable jsonb leaf falls to the relational seqscan — and partial-pushdown splits a node into a pushed sub-plan and a residual local fold, the non-pushable filter landing in the residual rather than a forced push.
- Entry: `public static PlanNode Plan(SetExpr selection, Func<PlanNode, EngineCost> cost)` — lowers an element-set selection into a cost-optimal cross-store plan; `public static (PlanNode Pushed, PlanNode Residual) Split(PlanNode node, FrozenSet<string> pushableOps)` partitions a plan into the store-pushable sub-plan and the local residual; `public static Fin<RuleAst> Admit(PlanWire wire)` is the wire-ingress that decodes a portable `python:data` plan — a Substrait `rel` tree or an ibis-emitted SQL string — into the one canonical `RuleAst` (so an `AggregateRel` keeps its measure/fold a bare-`SetExpr` target would drop) that `RulePlan.Lower`/`Evaluate` then drives against the federated entity graph through the same `Plan`/`Split` lowering, rejecting a relation no rule arm expresses as `FederationFault.PlanUnpushable` at the one decode seam rather than fanning to a second SQL engine.
- Auto: the federation is single-source-per-residence by design — one element's relational, spatial, document, vector, and analytical facets live in one `PostgresServer` row set (plus the embedded `DuckDB` analytical lane), so the planner pushes each predicate to the engine that owns its index rather than fanning across separate stores: a `SetPredicate.Spatial` leaf pushes to the GiST index, a `SetPredicate.Jsonpath`/`Material`/`Exists` leaf to the jsonb GIN index, a `SetPredicate.Classification` leaf to the ltree/relational index, and an analytical aggregate to the DuckDB lane through the live attach; cost-based selection reads the index-route facts (`search.vector.route`, `search.spatial.route`) and the `pg_stat_statements` evidence so a node routes to the cheapest engine; partial-pushdown splits a node when only part of it is pushable — the pushable predicate executes in the store and the residual (a cross-engine join or a managed fold) executes locally over the pushed result, never client-filtering a full scan.
- Receipt: a plan rides `store.plan.federated` carrying the engine selection per node and the pushdown ratio; a partial-pushdown rides `store.plan.partial`.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, DuckDB.NET.Data.Full, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new plan operator is one `PlanNode` case plus one `Split` arm; a new selection lowering is one `SetExpr` arm on `Plan`; a new engine is one `EngineCost` row; a new pushdown rule is one entry in the pushable-op set; a new portable-plan dialect is one `PlanWire` case lowering to `SetExpr` in `Admit`; zero new surface — `Plan`, `Split`, and `Admit` are total generated `Switch` folds so a new `SetExpr`, `PlanNode`, or `PlanWire` case breaks the build rather than routing to a silent default, and a generic federated query engine fanning a query across heterogeneous remote stores is deliberately not built (the suite is single-source-per-residence), so this owner is the within-store cross-engine planner over the one federated table, never a multi-database query federator.
- Boundary: federation here is cross-engine within one residence, never cross-store across heterogeneous databases — the deliberate single-source-per-residence posture means one element's facets live in one PG row set so the planner pushes each predicate to the engine (relational, GiST, jsonb GIN, HNSW, DuckDB) that owns its index, and a true multi-database query federator (postgres_fdw fan-out, a cross-store join engine) stays the rejected form because the per-store rail is world-class and a generic federator re-derives it; cost-based selection reads the live index-route and `pg_stat_statements` evidence so engine choice is data, never a hardcoded route; partial-pushdown is the one mechanism that keeps a non-fully-pushable plan efficient — the pushable predicate runs in the store and only the irreducible residual (a managed cross-engine join or a fold the translator cannot express) runs locally over the already-filtered result, so a client-side filter over a full scan is the deleted form; the plan AST stays typed end-to-end — `PlanNode.Filter` carries the `SetPredicate` leaf, `PlanNode.Aggregate` the `QuantityMeasure`, `PlanNode.Join` the `SetCombine` discriminant — so SQL translation rides the existing `Query/lanes#GEO_LANES` `SpatialOp.Predicate`, the jsonb-lane `@?`/`@@` path owner, and the parameterized `Aggregate` `SqlQuery` at execution, never a `Clause` that concatenates a raw path string into a SQL literal (`detail @? '<path>'`) — an interpolated-identifier-or-value plan-render is the deleted SQL-injection form because the parameterizing translator already owns the leaf; the plan operands are `SetExpr` selections so the federated planner is the lowering target of the element-set algebra and the rule engine, one plan surface every selection rides; a portable `python:data` `[SUBSTRAIT_FEDERATION_SEAM]` Substrait/ibis plan admits at the wire through `Admit`, which decodes the `PlanWire.Substrait` `rel` tree or the `PlanWire.PortableSql` ibis-`to_sql` rule under one `Fin` rail — a Substrait `ReadRel`/`FilterRel`/`SetRel`/`AggregateRel` maps to the `Select(Literal)`/`Where`/`Select(Union·Intersect·Difference)`/`Quantity` `RuleAst` shapes the planner already lowers (the `RuleAst` target keeps the `AggregateRel` measure/fold a bare-`SetExpr` decode would drop), and a relation no arm expresses (an arbitrary `ExtensionRel`, a join the algebra does not model) rejoins the rail as `FederationFault.PlanUnpushable` rather than admitting a partial plan, so the wire-projected plan rides the one PostGIS/DuckDB substrate with zero second SQL engine and a `Substrait.Protobuf.Plan`-walking `IConsumer` re-implementation beside the `RuleAst` decode is the deleted form.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<FederationKeyPolicy, string>]
public sealed partial class PlanEngine {
    public static readonly PlanEngine Relational = new("pg-relational");
    public static readonly PlanEngine Spatial = new("pg-gist");
    public static readonly PlanEngine Document = new("pg-jsonb-gin");
    public static readonly PlanEngine Vector = new("pgvector-hnsw");
    public static readonly PlanEngine Analytical = new("duckdb-attach");
}

public readonly record struct EngineCost(PlanEngine Engine, double Estimate, bool Pushable);

[SmartEnum]
public sealed partial class SetCombine {
    public static readonly SetCombine Union = new();
    public static readonly SetCombine Intersect = new();
    public static readonly SetCombine Difference = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record PlanNode {
    private PlanNode() { }

    public sealed record Scan(PlanEngine Engine) : PlanNode;
    public sealed record Filter(PlanNode Source, SetPredicate Leaf, PlanEngine Engine) : PlanNode;
    public sealed record Join(PlanNode Left, PlanNode Right, SetCombine On, PlanEngine Engine) : PlanNode;
    public sealed record Aggregate(PlanNode Source, QuantityMeasure Measure, RuleAggregate Fold, PlanEngine Engine) : PlanNode;
    public sealed record Pushdown(PlanNode Inner, PlanEngine Target) : PlanNode;
    public sealed record Materialize(PlanNode Source) : PlanNode;
}

// The closed mirror of the Substrait relational shapes `Admit` lowers — `ReadRel` (a named-table scan
// carrying its key literals), `FilterRel` (a condition over a typed `SetPredicate`), `SetRel` (the
// union/intersect/minus set operation), `AggregateRel` (a measure-plus-fold) — so the decode is a total
// fold over the relations the federated graph models rather than a partial walk of the full Substrait
// IR; a relation outside this family (an arbitrary `ExtensionRel`, an unmodeled join) is the
// `PlanUnpushable` rejection. The wire decoder hands the protobuf `Substrait.Protobuf.Rel` tree in as
// this mirror at the marshal seam, never the raw proto into the algebra.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SubstraitRel {
    private SubstraitRel() { }

    public sealed record Read(Seq<UInt128> Keys) : SubstraitRel;
    public sealed record Filter(SubstraitRel Input, SetPredicate Condition) : SubstraitRel;
    public sealed record Set(SubstraitRel Left, SubstraitRel Right, SetCombine Op) : SubstraitRel;
    public sealed record Aggregate(SubstraitRel Input, QuantityMeasure Measure, RuleAggregate Fold) : SubstraitRel;
    public sealed record Extension(string Uri) : SubstraitRel;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record PlanWire {
    private PlanWire() { }

    public sealed record Substrait(SubstraitRel Root) : PlanWire;
    public sealed record PortableSql(string Dialect, RuleAst Lowered) : PlanWire;
}

public static class FederatedPlan {
    // The leaf predicate's candidate engines, index-owner first then the always-available relational
    // sequential scan — a spatial leaf can serve from GiST or seqscan, a jsonb leaf from the GIN index or
    // seqscan, so the planner ranks the live `EngineCost` per candidate rather than hardcoding one route.
    private static readonly FrozenDictionary<string, Seq<PlanEngine>> LeafCandidates =
        new Dictionary<string, Seq<PlanEngine>> {
            [SpatialKind] = Seq(PlanEngine.Spatial, PlanEngine.Relational),
            [DocumentKind] = Seq(PlanEngine.Document, PlanEngine.Relational),
            [RelationalKind] = Seq(PlanEngine.Relational),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static PlanNode Plan(SetExpr selection, Func<PlanNode, EngineCost> cost) =>
        selection.Switch(
            state: cost,
            // Cost-based: rank every candidate engine the leaf admits through the live `EngineCost` (the
            // `Pushable` flag gates a candidate out and `Estimate` ranks the survivors), so the leaf engine
            // is data from `pg_stat_statements`/route facts, never a static map — a `jsonpath` leaf the cost
            // model marks GIN-unpushable falls to the relational seqscan rather than a phantom index route.
            predicate:  static (c, p) => Filter(p.Leaf, Cheapest(p.Leaf, c)),
            union:      static (c, u) => Combine(c, Plan(u.Left, c), Plan(u.Right, c), SetCombine.Union),
            intersect:  static (c, i) => Combine(c, Plan(i.Left, c), Plan(i.Right, c), SetCombine.Intersect),
            difference: static (c, d) => new PlanNode.Join(Plan(d.Left, c), Plan(d.Right, c), SetCombine.Difference, PlanEngine.Relational),
            literal:    static (_, _) => new PlanNode.Scan(PlanEngine.Relational),
            byRule:     static (_, _) => new PlanNode.Scan(PlanEngine.Relational),
            closure:    static (_, _) => new PlanNode.Scan(PlanEngine.Relational));

    // The portable-plan ingress lowers to `RuleAst`, not bare `SetExpr`, so a Substrait `AggregateRel`
    // keeps its `QuantityMeasure`/`RuleAggregate` (a selection-only target would silently drop the fold);
    // an ibis-`to_sql` plan arrives already lowered by the data lane's dialect onto this same rule AST,
    // and an `Extension`/untranslatable relation rejoins the rail as `PlanUnpushable` so a partial plan
    // never admits. The result is the `RulePlan.Lower`/`Evaluate` lowering target every selection rides.
    public static Fin<RuleAst> Admit(PlanWire wire) =>
        wire.Switch(
            substrait:   static s => Lower(s.Root),
            portableSql: static q => Fin.Succ(q.Lowered));

    // `Fin` is the abort rail, so the `set` arm sequences the two sub-relation lowerings monadically
    // (`Bind` then `Map`): the first `PlanUnpushable` short-circuits the whole plan rather than admitting a
    // one-sided fragment — a partial relational plan is never half-decoded. A new `SubstraitRel` case breaks
    // this generated `Switch` at compile time.
    private static Fin<RuleAst> Lower(SubstraitRel rel) =>
        rel.Switch(
            read:      static r => Fin.Succ((RuleAst)new RuleAst.Select(new SetExpr.Literal(r.Keys))),
            filter:    static f => Lower(f.Input).Map(inner => (RuleAst)new RuleAst.Where(inner, f.Condition)),
            set:       static s => Lower(s.Left).Bind(l => Lower(s.Right).Map(r =>
                          (RuleAst)new RuleAst.Select(Compose(RulePlan.Lower(l), RulePlan.Lower(r), s.Op)))),
            aggregate: static a => Lower(a.Input).Map(inner => (RuleAst)new RuleAst.Quantity(inner, a.Measure, a.Fold)),
            extension: static e => Fin.Fail<RuleAst>(new FederationFault.PlanUnpushable(e.Uri)));

    private static SetExpr Compose(SetExpr left, SetExpr right, SetCombine op) =>
        op.Switch(
            union:      () => (SetExpr)new SetExpr.Union(left, right),
            intersect:  () => new SetExpr.Intersect(left, right),
            difference: () => new SetExpr.Difference(left, right));

    // A non-pushable filter lands in the Residual local fold, never a forced push to relational — the
    // false branch keeping a `Pushdown` defeats partial-pushdown by client-side-filtering a full scan,
    // the named-rejected form; the pushed sub-plan is the bare scan and the leaf predicate refines locally.
    public static (PlanNode Pushed, PlanNode Residual) Split(PlanNode node, FrozenSet<string> pushableOps) =>
        node.Switch(
            state: pushableOps,
            filter:      static (ops, f) => ops.Contains(f.Engine.Key)
                ? (Pushed: (PlanNode)new PlanNode.Pushdown(f, f.Engine), Residual: (PlanNode)new PlanNode.Materialize(f))
                : (Pushed: (PlanNode)new PlanNode.Pushdown(f.Source, PlanEngine.Relational), Residual: f),
            join:        static (ops, j) => (Pushed: (PlanNode)new PlanNode.Pushdown(j.Left, j.Engine), Residual: j),
            scan:        static (ops, s) => (Pushed: (PlanNode)new PlanNode.Pushdown(s, PlanEngine.Relational), Residual: new PlanNode.Materialize(s)),
            aggregate:   static (ops, a) => (Pushed: (PlanNode)new PlanNode.Pushdown(a, a.Engine), Residual: new PlanNode.Materialize(a)),
            pushdown:    static (ops, p) => (Pushed: (PlanNode)p, Residual: new PlanNode.Materialize(p)),
            materialize: static (ops, m) => (Pushed: (PlanNode)new PlanNode.Pushdown(m, PlanEngine.Relational), Residual: m));

    private static PlanNode.Join Combine(Func<PlanNode, EngineCost> cost, PlanNode left, PlanNode right, SetCombine on) =>
        new(left, right, on, cost(left).Estimate <= cost(right).Estimate ? cost(left).Engine : cost(right).Engine);

    private static PlanNode.Filter Filter(SetPredicate leaf, PlanEngine engine) => new(new PlanNode.Scan(engine), leaf, engine);

    // The cheapest pushable engine the leaf admits — the candidate set folds through the live `EngineCost`,
    // a non-pushable candidate drops, and the survivors rank by `Estimate`; with no pushable candidate the
    // leaf's index-owning engine (the head of its candidate row) is the static fallback.
    private static PlanEngine Cheapest(SetPredicate leaf, Func<PlanNode, EngineCost> cost) {
        var candidates = LeafCandidates[KindOf(leaf)];
        return candidates
            .Map(engine => cost(new PlanNode.Scan(engine)))
            .Filter(static c => c.Pushable)
            .OrderBy(static c => c.Estimate)
            .HeadOrNone()
            .Map(static c => c.Engine)
            .IfNone(() => candidates.HeadOrNone().IfNone(PlanEngine.Relational));
    }

    private const string SpatialKind = "spatial";
    private const string DocumentKind = "document";
    private const string RelationalKind = "relational";

    private static string KindOf(SetPredicate leaf) =>
        leaf.Switch(
            spatial:        static _ => SpatialKind,
            jsonpath:       static _ => DocumentKind,
            material:       static _ => DocumentKind,
            exists:         static _ => DocumentKind,
            classification: static _ => RelationalKind,
            containment:    static _ => RelationalKind);
}
```

## [08]-[RESEARCH]

- [IDS_FACET_SCHEMA]: the buildingSMART IDS 1.0 XSD element/attribute shape the `IdsImport.Facet` reader projects — the `specification` `name`/`identifier`/`ifcVersion` attributes, the `applicability`/`requirements` element split, the per-facet `simpleValue`/`restriction` child grammar (`entity.name`, `attribute.name`/`value`, `classification.system`/`value`, `property.propertySet`/`baseName`/`dataType`/`value`, `material.value`, `partOf.relation`/`entity`), and the `requirement` `cardinality` attribute (`required`/`prohibited`/`optional`) — confirmed against the published IDS 1.0 schema before the `Facet` element-name dispatch and the `IdsConformance` receipt shape pin; the `xs:restriction` child grammar (`xs:enumeration`, `xs:pattern`, `xs:minInclusive`/`xs:maxInclusive`, `xs:length`) is modeled as the closed `IdsValue` `[Union]` lowering to a `SetExpr` over `SetPredicate.Jsonpath` `JsonComparison` leaves — a two-sided inclusive bound or a min/max length intersects both leaves so neither side is dropped — so the open item is the exact `xs:pattern` regex-flavor mapping onto the PG `~` operator and the GIN-expression-index that serves it for a high-cardinality pattern facet, plus the `jsonb` `.length()`-path serving form for the `xs:length` facet.
- [SPATIAL_CLASH_PUSHDOWN]: the `Clash` self-join SQL form is settled — the `ST_3DIntersects` (hard) / `ST_3DDWithin(tolerance)` (clearance) `a.key <> b.key` join over the GiST-indexed `geom` column with the windowed directed-pair count is the realized kernel; the held probe is the live PG18 GiST index-route the planner observes for that two-set self-join over a `geometryz` column (whether the planner selects the GiST index for both join sides under the `key = ANY(...)` prefilter) and the incremental-clash form re-running the join with one side bound to only the structural-diff changed-node set rather than the full candidate cross-product.
- [FUSION_PLANNER_COST]: the cost-model inputs the `FederatedPlan` reads to rank the GiST, jsonb-GIN, HNSW, and DuckDB-attach engines for a node — whether `pg_stat_statements` plan-cost evidence and the `search.*.route` facts are sufficient to drive cost-based engine selection, and the N-way `FusionRank.FuseSql` CTE planner route (the same fold at arity 2 for the `Query/lanes#SEARCH_LANES` `HybridRetrieve` vector+BM25 case and arity 3 for the spatial-vector-lexical case) on a live PG18 server carrying pgvectorscale and pg_search.
- [SUBSTRAIT_REL_MAPPING]: the field-level projection of the published Substrait `Rel` proto (`ReadRel.named_table`/`base_schema`, `FilterRel.condition` `Expression`, `SetRel.op` `SetOp`, `AggregateRel.measures`) onto the closed `SubstraitRel` mirror `FederatedPlan.Admit` lowers — the decode of a `FilterRel.condition` scalar-function `Expression` into a typed `SetPredicate` leaf (an `ST_*` function reference to `SetPredicate.Spatial`, a `jsonb_path` reference to `SetPredicate.Jsonpath`) and of an `AggregateRel` measure function into the `QuantityMeasure`/`RuleAggregate` pair is the one marshal-seam detail held with the `python:data` query owner that emits the plan, the algebra-side lowering (`Read`→`Select(Literal)`, `Filter`→`Where`, `Set`→`Select(Union`/`Intersect`/`Difference)`, `Aggregate`→`Quantity`) being settled; the `python:data` `[SUBSTRAIT_PORTABILITY]`/`[QUERY_IR_AND_SQLGATE]` leg owns the proto-to-mirror marshal and the ibis-`to_sql` dialect that targets this `RuleAst` algebra.

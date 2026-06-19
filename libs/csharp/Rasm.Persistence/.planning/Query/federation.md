# [PERSISTENCE_FEDERATION]

Rasm.Persistence owns the source-agnostic federated entity graph and the universal BIM query currency that rides it: `FederatedEntity` keys every element by a stable composite identity (geometry refs, property sets, classification, spatial containment, provenance) across every source document; `ElementSet` is the polymorphic, composable, stable selection receipt every clash/IDS/MVD/QTO surface consumes; `CrossDocLink` resolves typed inter-document references with pin-versus-float semantics and transitive impact analysis; `RulePlan` lowers a declarative rule DSL into a predicate/query plan emitting typed pass/fail/element-set/viewpoint receipts; `FusionRank` fuses the world-class pgvector HNSW, PostGIS GiST, and pg_search/FTS indexes into one scored result with per-hit lineage; and `FederatedPlan` pushes one selection AST across multiple stores with cost-based engine selection and partial-pushdown. The federated graph rides the existing PostGIS GiST + jsonb + ltree substrate on the `PostgresServer` profile, the content-addressed identity (`XxHash128`), the op-log changefeed, and the structural-diff node identity; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive settled.

## [1]-[INDEX]

- [1]-[ENTITY_GRAPH]: source-agnostic federated entity keyed by stable composite identity.
- [2]-[ELEMENT_SET_ALGEBRA]: polymorphic composable selection over the graph; stable set receipts.
- [3]-[CROSS_DOC_LINKS]: typed inter-doc links, pin/float, and transitive impact propagation.
- [4]-[RULE_PLAN]: declarative rule DSL to predicate/query plan; typed result receipts.
- [5]-[FUSION_RANK]: HNSW + GiST + FTS fused into one scored result with lineage.
- [6]-[FEDERATED_PLAN]: one selection AST across stores; cost-based engine and partial-pushdown.

## [2]-[ENTITY_GRAPH]

- Owner: `FederatedEntity` the source-agnostic element record keyed by stable composite identity; `EntityIdentity` the five-axis identity value; `SourceRef` the originating-document pointer; `EntityGraph` the static surface owning identity derivation, upsert-into-the-graph, spatial-containment fold, and the source-agnostic merge of one element across documents.
- Cases: identity composes geometry-ref hash, property-set hash, classification key, spatial-containment ltree path, and provenance origin; a federated entity merges every `SourceRef` projecting the same `EntityIdentity` so one element appears once regardless of source-document count; a `Sync/collaboration#TRANSPORT_AXIS` `SpeckleReceive` graph enters as one `SourceRef` of `SourceKind.ExternalImport` so a Speckle `DataObject` mapped to closed Rasm types at the marshal seam federates as one source projection, never a Speckle-specific entity family.
- Entry: `public static EntityIdentity Identify(UInt128 geometryRef, UInt128 psetHash, string classification, string containmentPath, Guid origin)` — pure composite identity; `public static IO<FederatedEntity> Upsert(FederatedEntity prior, SourceRef source, Func<FederatedEntity, IO<Unit>> persist)` folds a new source projection into the federated entity.
- Auto: the entity table is a `PostgresServer` row carrying a `geometry`/`geography` GiST column for spatial containment, a jsonb column for the merged property sets, an `ltree` column for the spatial hierarchy path, and a `tsvector` generated column for the classification and name corpus — so spatial, document, and full-text lanes (`Query/lanes#GEO_LANES`, `#DOCUMENT_LANE`, `#SEARCH_LANES`) all index the one federated table; the stable composite identity is `XxHash128` over the canonical five-axis tuple so a re-ingested model whose source ids changed still federates to the same entity by geometry/pset/containment signature — the same `GraphNode` identity the structural diff matches on; spatial containment derives from the GiST `ST_Contains` fold against the spatial-hierarchy table so an element's room/level/building path is computed once at ingest, not stored per-source.
- Receipt: an upsert rides `store.federation.upsert` carrying the source count merged; the spatial-containment fold rides the `geo.diff.kind` stream.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new identity axis is one column on `EntityIdentity` plus one term in the canonical tuple; a new source kind is one `SourceRef.Kind` value; a new spatial-containment relation is one ltree path policy; zero new surface — a per-source entity table, a second graph store, or an IFC-specific entity family is the deleted form because the federated entity rides the one PostGIS substrate and the `IfcSemantic` model graph (`Query/cache#ARTIFACT_BLOB_INDEX`) projects into it as one source.
- Boundary: identity is source-agnostic by construction — the composite key reads geometry ref, property-set hash, classification, containment path, and provenance origin so the same physical element ingested from a Rhino model, an IFC file, and a gh2 solve federates to one `FederatedEntity` with three `SourceRef` projections; the property sets merge jsonb-deep so a later source's psets union into the entity rather than overwrite, and a value conflict folds through the CRDT `OrSet`/`LwwRegister` so the merge is convergent; spatial containment is the ltree path the GiST fold computes, never a stored per-source path, so an element moved between rooms updates one path; the federated graph is the substrate `ElementSet`, `RulePlan`, `FusionRank`, and `FederatedPlan` all ride, so a parallel selection table or a second BIM model is the deleted form; the provenance origin axis ties the federated entity to the `Version/provenance#CAUSAL_DAG` so a federated query joins lineage as a dimension; a Speckle import is the `SourceKind.ExternalImport` source projection — the `Sync/collaboration#TRANSPORT_AXIS` `SpeckleReceive` marshal maps the inbound `Base`/`DataObject` graph to closed Rasm op-log entries at the seam and `EntityGraph.Upsert` folds it into the federated entity under one `SourceRef`, so a Speckle-sourced element federates by its geometry/pset/containment signature exactly like a Rhino, IFC, or gh2 source with zero Speckle-specific entity surface; the federated entity table is RLS-scoped per tenant through `Store/server#TENANCY_RLS` so a multi-model hub partitions by tenant.

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
}

public readonly record struct EntityIdentity(UInt128 GeometryRef, UInt128 PsetHash, string Classification, string ContainmentPath, Guid Origin) {
    public UInt128 Key {
        get {
            var buffer = new ArrayBufferWriter<byte>();
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], GeometryRef);
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], PsetHash);
            Encoding.UTF8.GetBytes($"{Classification}|{ContainmentPath}|{Origin:N}", buffer);
            return XxHash128.HashToUInt128(buffer.WrittenSpan);
        }
    }
}

public sealed record SourceRef(SourceKind Kind, Guid Document, string SourceElementId, Instant At);

public sealed record FederatedEntity(
    UInt128 Key,
    EntityIdentity Identity,
    Seq<SourceRef> Sources,
    HashMap<string, CrdtField> PropertySets,
    string ContainmentPath,
    UInt128 GeometryRef,
    Option<UInt128> ProvenanceHead,
    DataClassification Classification,
    Instant At);

public static class EntityGraph {
    public static EntityIdentity Identify(UInt128 geometryRef, UInt128 psetHash, string classification, string containmentPath, Guid origin) =>
        new(geometryRef, psetHash, classification, containmentPath, origin);

    public static IO<FederatedEntity> Upsert(FederatedEntity prior, SourceRef source, HashMap<string, CrdtField> incoming, Func<FederatedEntity, IO<Unit>> persist) {
        var merged = prior with {
            Sources = prior.Sources.Exists(s => s.Document == source.Document && s.SourceElementId == source.SourceElementId)
                ? prior.Sources
                : prior.Sources.Add(source),
            PropertySets = incoming.Fold(prior.PropertySets, static (acc, slot) =>
                acc.AddOrUpdate(slot.Key, existing => Crdt.Merge(existing, slot.Value), slot.Value)),
        };
        return persist(merged).Map(_ => merged);
    }

    public static Seq<FederatedEntity> Contained(Seq<FederatedEntity> graph, string ancestorPath) =>
        graph.Filter(entity => entity.ContainmentPath.StartsWith(ancestorPath, StringComparison.Ordinal));
}
```

| [INDEX] | [AXIS]              | [RESIDENCE]                      | [INDEX_ROUTE]                                      |
| :-----: | :------------------ | :------------------------------- | :------------------------------------------------- |
|   [1]   | geometry ref        | `geometry` GiST column           | `Query/lanes#GEO_LANES` spatial predicates          |
|   [2]   | property sets       | merged jsonb column              | `Query/lanes#DOCUMENT_LANE` `@?`/`@@` predicates    |
|   [3]   | classification      | `tsvector` generated + ltree key | `Query/lanes#SEARCH_LANES` + classification catalog |
|   [4]   | spatial containment | `ltree` hierarchy path           | `lquery` ancestor/descendant predicates            |
|   [5]   | provenance          | `Version/provenance#CAUSAL_DAG` head ref | lineage join dimension                             |

## [3]-[ELEMENT_SET_ALGEBRA]

- Owner: `ElementSet` the polymorphic composable selection record carrying a stable content-addressed set receipt; `SetExpr` `[Union]` the selection algebra; `ElementSetAlgebra` the static surface owning literal selection, the boolean/spatial/property/classification combinators, and the stable-receipt fold.
- Cases: `Literal | ByClassification | BySpatial | ByProperty | ByRule | Union | Intersect | Difference | Closure` on `SetExpr`.
- Entry: `public static ElementSet Evaluate(SetExpr expr, Func<SetExpr, Seq<UInt128>> resolve)` — folds the expression tree into a stable element-key set; `public static UInt128 Receipt(Seq<UInt128> keys)` derives the content-addressed set identity so the same selection produces the same receipt across runs and peers.
- Auto: an element set is the universal BIM currency — clash, IDS, MVD, QTO, and rule surfaces all consume and produce `ElementSet` values, so a clash result is an `ElementSet`, an IDS pass-set is an `ElementSet`, and a QTO subject is an `ElementSet`; the set receipt is `XxHash128` over the sorted element-key set so two selections yielding the same elements share one receipt and a cached selection result keys on it through the `Query/cache#MODEL_RESULT_INDEX`; the boolean combinators (`Union`/`Intersect`/`Difference`) fold over evaluated leaf sets, the spatial combinator (`BySpatial`) lowers to a GiST predicate, the property combinator (`ByProperty`) lowers to a jsonb path predicate, and the classification combinator lowers to an ltree/tsvector predicate, so one algebra composes every selection concern.
- Receipt: an evaluation rides `store.elementset.eval` carrying the leaf count and the result cardinality; the stable receipt is the cache key.
- Packages: System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new selection primitive is one `SetExpr` case; a new combinator is one fold arm; zero new surface — a per-discipline selection class, a saved-search table per concern, or a string-query selection DSL is the deleted form because the algebra is one composable tree the planner lowers and the receipt is content-addressed; an `ElementSet` is the input and output type of every analysis surface so a clash engine, an IDS validator, and a QTO rollup never mint a parallel selection shape.
- Boundary: `ElementSet` is the one composable currency — every analysis surface (clash, IDS, MVD, QTO, rule) takes an `ElementSet` and yields an `ElementSet` so results compose: a clash result intersected with a classification selection is one `SetExpr.Intersect`, never a join in application code; the receipt is content-addressed over the sorted key set so it is stable across runs, peers, and tenants — a positional or timestamp-keyed selection id is the deleted form; the `Closure` combinator reads the same `Closure` content-key manifest the op-log carries so a transitive selection (every element referenced by a selection) rides the existing graph closure, never a second reachability walk; selection evaluation pushes through `FederatedPlan` so a `BySpatial` leaf executes on the GiST index and a `ByProperty` leaf on the jsonb index in the store, never client-side; an empty `Literal` and a `Difference` to empty both yield the stable empty-set receipt; the algebra rides the federated entity keys (`#ENTITY_GRAPH`) so a selection spanning multiple source documents is one set over the one federated graph; the `ElementSetAlgebra.Receipt` distinct-sorted `UInt128LE`-packed preimage is the byte shape the `Version/commits#CRDT_WIRE` `ContentParityCorpus` freezes as the `elementset.empty` parity vector, so the cross-runtime golden corpus pins this receipt encoding by reference and a sort-order or packing drift fails one corpus assertion.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetExpr {
    private SetExpr() { }

    public sealed record Literal(Seq<UInt128> Keys) : SetExpr;
    public sealed record ByClassification(string ClassificationPath) : SetExpr;
    public sealed record BySpatial(string GeometryPredicate, byte[] Operand) : SetExpr;
    public sealed record ByProperty(string JsonPath, string Predicate) : SetExpr;
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
        foreach (var key in sortedKeys)
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], key);
        return XxHash128.HashToUInt128(buffer.WrittenSpan);
    }

    public static ElementSet Evaluate(SetExpr expr, Func<SetExpr, Seq<UInt128>> resolve) =>
        expr.Switch(
            state: resolve,
            literal:          static (_, lit) => ElementSet.Of(lit.Keys),
            byClassification: static (r, e) => ElementSet.Of(r(e)),
            bySpatial:        static (r, e) => ElementSet.Of(r(e)),
            byProperty:       static (r, e) => ElementSet.Of(r(e)),
            byRule:           static (r, e) => ElementSet.Of(r(e)),
            closure:          static (r, e) => ElementSet.Of(r(e)),
            union:            static (r, u) => ElementSet.Of(Evaluate(u.Left, r).Keys + Evaluate(u.Right, r).Keys),
            intersect:        static (r, i) => ElementSet.Of(toSeq(Evaluate(i.Left, r).Keys.Intersect(Evaluate(i.Right, r).Keys))),
            difference:       static (r, d) => ElementSet.Of(toSeq(Evaluate(d.Left, r).Keys.Except(Evaluate(d.Right, r).Keys))));
}
```

## [4]-[CROSS_DOC_LINKS]

- Owner: `CrossDocLink` the typed inter-document reference record carrying pin-versus-float semantics; `LinkKind` the reference-type vocabulary; `ImpactNode` a transitive-impact-analysis node; `LinkStore` the static surface owning link upsert, pin/float resolution, transitive forward/backward impact, and change-propagation fold.
- Cases: `Reference | Override | Aggregation | Constraint | Derivation` on `LinkKind`; pin freezes the link at a specific source commit while float tracks the source head; a change to a pinned source emits a stale-pin impact while a change to a floated source propagates transitively.
- Entry: `public static IO<CrossDocLink> Link(UInt128 from, UInt128 to, LinkKind kind, bool pinned, Option<UInt128> pinCommit, Func<CrossDocLink, IO<Unit>> persist)` — `IO` persists the link row; `public static Seq<ImpactNode> Impact(Func<UInt128, Seq<CrossDocLink>> outbound, UInt128 changed, int depth)` walks the link graph forward to every transitively affected element.
- Auto: a link is one `OpLogEntry` of `SyncOpKind.Upsert` on the `link` column family so the link graph rides the changefeed and converges per the CRDT algebra; pin/float is a column on the link — a pinned link carries the frozen source commit content key and a floated link carries `None` so it tracks the source's branch head; transitive impact is a bounded breadth-first walk over the outbound link adjacency, so when a source element changes the impact fold yields every downstream element within the depth bound with its link path; change-propagation re-evaluates every floated link's `ElementSet` against the new source state and emits an impact receipt per affected element.
- Receipt: a link mutation rides `store.link.upsert`; an impact analysis rides `store.link.impact` carrying the affected count and max depth; a stale-pin detection rides `store.link.stale-pin`.
- Packages: System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), BCL inbox.
- Growth: a new link semantic is one `LinkKind` row; a new propagation policy is one column on `CrossDocLink`; zero new surface — a per-link-type table, a second reference resolver, or a foreign-key-only model is the deleted form because the link is one typed row on the federated graph and impact is one bounded walk over its adjacency.
- Boundary: pin-versus-float is the one reference-stability axis — a pinned link freezes at `pinCommit` so a source change never silently propagates and surfaces as a `store.link.stale-pin` impact the operator resolves by re-pinning, while a floated link tracks the source head so a change propagates transitively through `Impact`; transitive impact is a bounded breadth-first walk over the outbound adjacency so the cost is linear in the reachable-link count within the depth bound, never an unbounded graph traversal; backward impact (every source a changed element depends on) is the same walk over the inbound adjacency; the link graph rides the op-log so links converge like any other op and a link removed on one peer and re-added on another resolves add-wins through the CRDT `OrSet`; a `Derivation` link carries the rule that produced the target so a change-propagation re-runs the `RulePlan` rather than copying a stale value; the link endpoints are federated entity keys (`#ENTITY_GRAPH`) so a cross-document link is one edge over the one federated graph, never a per-document foreign key.

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
    bool Pinned,
    Option<UInt128> PinCommit,
    Option<string> DerivationRule,
    Instant At);

public readonly record struct ImpactNode(UInt128 Element, int Depth, Seq<UInt128> Path, LinkKind Via);

public static class LinkStore {
    public const int MaxImpactDepth = 32;

    public static IO<CrossDocLink> Link(UInt128 from, UInt128 to, LinkKind kind, bool pinned, Option<UInt128> pinCommit, Func<CrossDocLink, IO<Unit>> persist, ClockPolicy clocks) {
        var link = new CrossDocLink(from, to, kind, pinned, pinCommit, None, clocks.Now);
        return persist(link).Map(_ => link);
    }

    public static Seq<ImpactNode> Impact(Func<UInt128, Seq<CrossDocLink>> outbound, UInt128 changed, int depth) {
        var seen = new System.Collections.Generic.HashSet<UInt128> { changed };
        var frontier = Seq((Element: changed, Depth: 0, Path: Seq<UInt128>(), Via: LinkKind.Reference));
        var impact = Seq<ImpactNode>();
        while (frontier.HeadOrNone() is { IsSome: true, Case: var head } && frontier is { IsEmpty: false }) {
            frontier = frontier.Tail;
            if (head.Depth >= int.Min(depth, MaxImpactDepth))
                continue;
            foreach (var link in outbound(head.Element))
                if (seen.Add(link.To)) {
                    var node = new ImpactNode(link.To, head.Depth + 1, head.Path.Add(head.Element), link.Kind);
                    impact = impact.Add(node);
                    frontier = frontier.Add((link.To, head.Depth + 1, node.Path, link.Kind));
                }
        }
        return impact;
    }

    public static Seq<CrossDocLink> StalePins(Seq<CrossDocLink> links, Func<UInt128, UInt128> sourceHead) =>
        links.Filter(link => link.Pinned && link.PinCommit.Map(pin => sourceHead(link.From) != pin).IfNone(false));
}
```

## [5]-[RULE_PLAN]

- Owner: `RuleAst` `[Union]` the declarative rule DSL parsed shape; `RuleResult` `[Union]` the typed result family; `RulePlan` the static surface lowering a rule AST into a predicate/query plan over the federated graph and the element-set algebra, emitting the typed result; `IdsSpecification`/`IdsFacet`/`IdsRequirement`/`IdsImport` the buildingSMART IDS 1.0 importer projecting a published IDS document's facets into `RuleAst` rows and emitting `IdsConformance` audit receipts.
- Cases: `Select | Where | Requires | Forbids | Clash | Quantity | Property` on `RuleAst`; `Pass | Fail | Selection | Viewpoint | Quantity` on `RuleResult`; `Entity | Attribute | Classification | Property | Material | PartOf` on `IdsFacet`; `Required | Prohibited | Optional` on `IdsCardinality`.
- Entry: `public static (SetExpr Plan, RuleResult.Kind Shape) Lower(RuleAst rule)` — pure lowering of the rule DSL into an `ElementSet` plan and the result shape; `public static IO<RuleResult> Evaluate(RuleAst rule, Func<SetExpr, ElementSet> evaluate, Func<ElementSet, ElementSet, IO<Seq<(UInt128, UInt128)>>> clash)` runs the plan and folds the typed result.
- Auto: the rule DSL covers clash (two element sets intersecting in space), IDS/MVD (every element of a selection requires/forbids a property or classification), compliance (a predicate over a selection), and QTO (a quantity aggregate over a selection) — so one rule engine serves every validation and takeoff concern; a rule lowers to an `ElementSet` plan plus a result shape, so a clash rule lowers to two `BySpatial` selections plus a spatial-intersection fold yielding a `Fail` with the clashing pairs, an IDS rule lowers to a `ByClassification` selection plus a property predicate yielding `Pass`/`Fail` with the violating element set, and a QTO rule lowers to a selection plus a quantity aggregate yielding a `Quantity` result; every `Fail` carries the offending `ElementSet` so a downstream surface selects the failures, and a clash `Fail` carries a viewpoint receipt the annotation surface anchors.
- Receipt: an evaluation rides `store.rule.eval` carrying the rule kind, the pass/fail verdict, and the result cardinality; a clash result carries the pair count.
- Packages: System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.Xml.Linq (BCL inbox), BCL inbox.
- Growth: a new rule primitive is one `RuleAst` case; a new result shape is one `RuleResult` case; a new takeoff aggregate is one `Quantity` arm; a new IDS facet kind is one `IdsFacet` case plus one `Predicate`/`Require`/`Forbid` arm and an unsupported facet is the typed `NotSupportedException` rejection at the parse seam, never a second checker; zero new surface — a per-standard validator (a hard-coded IDS checker, a separate clash engine, a separate QTO calculator) is the deleted form because one DSL lowers to the element-set algebra and emits a typed result, the IDS specification is data parsed into `RuleAst` rows rather than C# predicate code, and the rule itself is a `ByRule` selection leaf so a rule's pass-set composes with any other selection.
- Boundary: the rule DSL lowers to the element-set algebra so a rule is a query plan, never imperative validation code — a hard-coded clash loop, a string-templated IDS check, or a per-discipline QTO calculator is the deleted form; the buildingSMART IDS 1.0 importer is the standards-conformant face of this engine — `IdsImport.Parse` reads an IDS XSD-shaped document through `System.Xml.Linq` into `IdsSpecification` rows, `Scope` folds the applicability facets into one `SetExpr` intersection, and `Lower` projects each requirement facet under its `IdsCardinality` into a `RuleAst.Requires`/`Forbids`/pass-through over that scope, so a published IDS check runs against the federated entity graph through the settled `RulePlan.Evaluate` lowering and the `IdsConformance` receipt aligns with `Sync/annotation.md#BCF_PROTOCOL` as an exportable CDE audit artifact, never a parallel checker — the IDS specification is data authored alongside the descriptor catalog and an unsupported facet maps to one `IdsFacet` case or a typed `<ids-facet-unsupported>` rejection, never C# predicate code or a second validation surface; every rule result is typed — `Pass`/`Fail` carry the verdict and the offending `ElementSet`, `Selection` carries the matched set, `Viewpoint` carries the camera and the highlighted set the `Sync/annotation#ANCHOR_ALGEBRA` anchors, and `Quantity` carries the aggregated measure — so a downstream surface dispatches on the result type, never parses a string verdict; a clash rule's spatial intersection lowers to a GiST `ST_3DIntersects` predicate pushed into the store through `FederatedPlan` so the clash executes server-side over the spatial index, never a client-side bounding-box loop; an IDS/MVD rule's property requirement lowers to a jsonb path predicate so the validation rides the document index; the rule result `ElementSet` is content-addressed so a re-run of an unchanged rule against an unchanged graph reuses the cached result through `Query/cache#MODEL_RESULT_INDEX`, and an incremental re-test (only the changed elements) rides the structural diff's changed-node set so a clash re-runs over the delta, never the whole graph.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record RuleAst {
    private RuleAst() { }

    public sealed record Select(SetExpr Scope) : RuleAst;
    public sealed record Where(RuleAst Source, string JsonPath, string Predicate) : RuleAst;
    public sealed record Requires(RuleAst Scope, string PropertyPath) : RuleAst;
    public sealed record Forbids(RuleAst Scope, string PropertyPath) : RuleAst;
    public sealed record Clash(RuleAst Left, RuleAst Right, double Tolerance) : RuleAst;
    public sealed record Quantity(RuleAst Scope, string Measure, string Aggregate) : RuleAst;
    public sealed record Property(RuleAst Scope, string Path) : RuleAst;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuleResult {
    private RuleResult() { }

    public sealed record Pass(ElementSet Subject) : RuleResult;
    public sealed record Fail(ElementSet Subject, ElementSet Offending, string Reason) : RuleResult;
    public sealed record Selection(ElementSet Matched) : RuleResult;
    public sealed record Viewpoint(ElementSet Highlighted, byte[] Camera, ElementSet Offending) : RuleResult;
    public sealed record Quantity(ElementSet Subject, string Measure, double Value, string Unit) : RuleResult;
}

public static class RulePlan {
    public static SetExpr Lower(RuleAst rule) =>
        rule.Switch(
            select:   static s => s.Scope,
            where:    static w => new SetExpr.Intersect(Lower(w.Source), new SetExpr.ByProperty(w.JsonPath, w.Predicate)),
            requires: static r => Lower(r.Scope),
            forbids:  static f => Lower(f.Scope),
            clash:    static c => new SetExpr.Intersect(Lower(c.Left), Lower(c.Right)),
            quantity: static q => Lower(q.Scope),
            property: static p => Lower(p.Scope));

    public static IO<RuleResult> Evaluate(
        RuleAst rule,
        Func<SetExpr, ElementSet> evaluate,
        Func<ElementSet, ElementSet, double, IO<Seq<(UInt128 Left, UInt128 Right)>>> clash,
        Func<ElementSet, string, string, IO<(double Value, string Unit)>> quantity) =>
        rule.Switch(
            state: (Evaluate: evaluate, Clash: clash, Quantity: quantity),
            clash: static (ctx, c) =>
                ctx.Clash(ctx.Evaluate(Lower(c.Left)), ctx.Evaluate(Lower(c.Right)), c.Tolerance).Map(pairs =>
                    pairs.IsEmpty
                        ? new RuleResult.Pass(ctx.Evaluate(Lower(c.Left)))
                        : (RuleResult)new RuleResult.Fail(
                            ctx.Evaluate(Lower(c.Left)),
                            ElementSet.Of(pairs.Map(static p => p.Left) + pairs.Map(static p => p.Right)),
                            $"{pairs.Count} clashes")),
            requires: static (ctx, r) =>
                IO.pure(ctx.Evaluate(Lower(r.Scope)) is var scope
                    && ctx.Evaluate(new SetExpr.Difference(Lower(r.Scope), new SetExpr.ByProperty(r.PropertyPath, "exists"))) is var missing
                    ? missing.Count == 0 ? new RuleResult.Pass(scope) : (RuleResult)new RuleResult.Fail(scope, missing, $"{missing.Count} missing {r.PropertyPath}")
                    : new RuleResult.Pass(scope)),
            forbids: static (ctx, f) =>
                IO.pure(ctx.Evaluate(new SetExpr.Intersect(Lower(f.Scope), new SetExpr.ByProperty(f.PropertyPath, "exists"))) is var present
                    ? present.Count == 0 ? new RuleResult.Pass(ctx.Evaluate(Lower(f.Scope))) : (RuleResult)new RuleResult.Fail(ctx.Evaluate(Lower(f.Scope)), present, $"{present.Count} forbidden {f.PropertyPath}")
                    : new RuleResult.Pass(ctx.Evaluate(Lower(f.Scope)))),
            quantity: static (ctx, q) =>
                ctx.Quantity(ctx.Evaluate(Lower(q.Scope)), q.Measure, q.Aggregate).Map(measure =>
                    (RuleResult)new RuleResult.Quantity(ctx.Evaluate(Lower(q.Scope)), q.Measure, measure.Value, measure.Unit)),
            select:   static (ctx, s) => IO.pure((RuleResult)new RuleResult.Selection(ctx.Evaluate(s.Scope))),
            where:    static (ctx, w) => IO.pure((RuleResult)new RuleResult.Selection(ctx.Evaluate(Lower(w)))),
            property: static (ctx, p) => IO.pure((RuleResult)new RuleResult.Selection(ctx.Evaluate(Lower(p.Scope)))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<FederationKeyPolicy, string>]
public sealed partial class IdsCardinality {
    public static readonly IdsCardinality Required = new("required");
    public static readonly IdsCardinality Prohibited = new("prohibited");
    public static readonly IdsCardinality Optional = new("optional");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record IdsFacet {
    private IdsFacet() { }

    public sealed record Entity(string Name, Option<string> PredefinedType) : IdsFacet;
    public sealed record Attribute(string Name, Option<string> Value) : IdsFacet;
    public sealed record Classification(string System, Option<string> Value) : IdsFacet;
    public sealed record Property(string PropertySet, string BaseName, Option<string> Value, Option<string> DataType) : IdsFacet;
    public sealed record Material(Option<string> Value) : IdsFacet;
    public sealed record PartOf(string Relation, Entity Whole) : IdsFacet;
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

public static class IdsImport {
    private static readonly XNamespace Ids = "http://standards.buildingsmart.org/IDS";

    public static Fin<Seq<IdsSpecification>> Parse(Stream document) =>
        Try.lift(() => XDocument.Load(document))
            .Run()
            .Map(static doc => toSeq(doc.Root!.Element(Ids + "specifications")!.Elements(Ids + "specification").Map(Specification)))
            .MapFail(static error => Error.New(8271, $"<ids-parse:{error.Message}>"));

    public static SetExpr Scope(IdsSpecification spec) =>
        spec.Applicability.Fold(
            (SetExpr)new SetExpr.Literal(Seq<UInt128>()),
            static (acc, facet) => acc is SetExpr.Literal { Keys.IsEmpty: true }
                ? Predicate(facet)
                : new SetExpr.Intersect(acc, Predicate(facet)));

    public static Seq<RuleAst> Lower(IdsSpecification spec) {
        var scope = new RuleAst.Select(Scope(spec));
        return spec.Requirements.Map(req => req.Cardinality.Switch(
            state: (Scope: (RuleAst)scope, Req: req),
            required:   static ctx => Require(ctx.Scope, ctx.Req.Facet),
            prohibited: static ctx => Forbid(ctx.Scope, ctx.Req.Facet),
            optional:   static ctx => ctx.Scope));
    }

    private static SetExpr Predicate(IdsFacet facet) =>
        facet.Switch(
            entity:         static e => new SetExpr.ByProperty("$.entity", e.Name),
            attribute:      static a => new SetExpr.ByProperty($"$.{a.Name}", a.Value.IfNone("exists")),
            classification: static c => new SetExpr.ByClassification(c.Value.Map(v => $"{c.System}.{v}").IfNone(c.System)),
            property:       static p => new SetExpr.ByProperty($"$.{p.PropertySet}.{p.BaseName}", p.Value.IfNone("exists")),
            material:       static m => new SetExpr.ByProperty("$.material", m.Value.IfNone("exists")),
            partOf:         static po => new SetExpr.ByProperty("$.partOf", po.Relation));

    private static RuleAst Require(RuleAst scope, IdsFacet facet) =>
        facet.Switch(
            entity:         _ => scope,
            attribute:      a => new RuleAst.Requires(scope, $"$.{a.Name}"),
            classification: c => new RuleAst.Requires(scope, $"$.classification.{c.System}"),
            property:       p => new RuleAst.Requires(scope, $"$.{p.PropertySet}.{p.BaseName}"),
            material:       _ => new RuleAst.Requires(scope, "$.material"),
            partOf:         po => new RuleAst.Requires(scope, $"$.partOf.{po.Relation}"));

    private static RuleAst Forbid(RuleAst scope, IdsFacet facet) =>
        facet.Switch(
            entity:         _ => scope,
            attribute:      a => new RuleAst.Forbids(scope, $"$.{a.Name}"),
            classification: c => new RuleAst.Forbids(scope, $"$.classification.{c.System}"),
            property:       p => new RuleAst.Forbids(scope, $"$.{p.PropertySet}.{p.BaseName}"),
            material:       _ => new RuleAst.Forbids(scope, "$.material"),
            partOf:         po => new RuleAst.Forbids(scope, $"$.partOf.{po.Relation}"));

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
            "attribute" => new IdsFacet.Attribute(Simple(node, "name"), Optional(Simple(node, "value")).Filter(static v => v.Length > 0)),
            "classification" => new IdsFacet.Classification(Simple(node, "system"), Optional(Simple(node, "value")).Filter(static v => v.Length > 0)),
            "property" => new IdsFacet.Property(Simple(node, "propertySet"), Simple(node, "baseName"), Optional(Simple(node, "value")).Filter(static v => v.Length > 0), Optional(node.Attribute("dataType")?.Value)),
            "material" => new IdsFacet.Material(Optional(Simple(node, "value")).Filter(static v => v.Length > 0)),
            "partOf" => new IdsFacet.PartOf(node.Attribute("relation")?.Value ?? "", (IdsFacet.Entity)Facet(node.Element(Ids + "entity")!)),
            var other => throw new NotSupportedException($"<ids-facet-unsupported:{other}>"),
        };

    private static string Simple(XElement node, string child) =>
        node.Element(Ids + child)?.Element(Ids + "simpleValue")?.Value ?? "";
}
```

## [6]-[FUSION_RANK]

- Owner: `FusionCandidate` a scored hit carrying its per-index rank and lineage; `FusionWeights` the per-index reciprocal-rank-fusion weight policy; `FusionRank` the static surface fusing the pgvector HNSW, PostGIS GiST, and pg_search/FTS index results into one scored result with per-hit provenance.
- Cases: each candidate carries its rank in the vector branch, the spatial branch, and the lexical branch (absent where the branch did not return it) plus the federated entity key and its provenance head.
- Entry: `public static Seq<FusionCandidate> Fuse(Seq<(UInt128 Key, int Rank)> vector, Seq<(UInt128 Key, int Rank)> spatial, Seq<(UInt128 Key, int Rank)> lexical, FusionWeights weights, int k)` — folds the three ranked candidate sets into one reciprocal-rank-fused top-k with per-branch contribution; `public static string FuseSql(FusionWeights weights, int k, int rrfConstant)` projects the three-way RRF CTE.
- Auto: the three indexes exist and are world-class individually — pgvector HNSW/diskann on the `EmbeddingArity` column (`Query/lanes#SEARCH_LANES`), PostGIS GiST on the `geometry` column (`Query/lanes#GEO_LANES`), and pg_search BM25 / native tsvector on the corpus (`Query/lanes#SEARCH_LANES`) — so this owner is purely the fusion-ranking layer: it sums `weight / (rrfConstant + rank)` across the three branches per candidate and re-ranks, never a fourth index or a learned reranker; each fused candidate carries its federated entity key so the result is a stable `ElementSet`, and each carries its provenance head so a fused hit joins lineage as a per-hit dimension; the two-way RRF on `Query/lanes#SEARCH_LANES` (vector + BM25) is the leaf this generalizes to the three-way spatial-vector-lexical fusion, so a geometry-aware find-similar fuses spatial proximity with embedding similarity and text relevance in one scored result.
- Receipt: a fusion rides `store.fusion.rank` carrying the per-branch hit counts and the fused top-k cardinality; each hit's three ranks ride the `search.*` fact family.
- Packages: Pgvector.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, System.IO.Hashing, LanguageExt.Core, NodaTime.
- Growth: a new fusable index is one branch arm plus one `FusionWeights` column; a new weighting policy is one `FusionWeights` row; zero new surface — a learned cross-encoder reranker, a fourth parallel index, or a per-modality result class is the deleted form because the three indexes are world-class and only the fusion is net-new, and the fusion is the standard RRF the `HybridRetrieve.Fuse` two-way leaf already proves.
- Boundary: fusion is reciprocal-rank summation over the three existing index branches so it needs no learned model and no training — a candidate's score is `Σ weight_b / (rrfConstant + rank_b)` over the branches that returned it; the fused result is a stable `ElementSet` keyed on federated entity ids so it composes with the element-set algebra and the rule engine; each hit carries its provenance head so a federated search joins lineage per-hit — a search result whose source lineage is unknown is the deleted form; a branch that returns no candidate for a key contributes zero, so a hit strong in one modality still surfaces; the `rrfConstant` defaults to 60 per the standard RRF damping and the weights default to equal so an unweighted fusion is the spatial-vector-lexical mean; the fusion projects identities then re-queries the federated graph by the surviving keys so it never materializes three full candidate payloads; the SQL form is one CTE unioning the three `ROW_NUMBER()`-ranked branches, the same shape `HybridRetrieve.Fuse` proves, extended with the spatial branch — so the two-way and three-way fusions are one algebra, never parallel implementations.

```csharp signature
public sealed record FusionWeights(double Vector, double Spatial, double Lexical) {
    public static readonly FusionWeights Equal = new(1.0, 1.0, 1.0);
}

public readonly record struct FusionCandidate(
    UInt128 Key,
    Option<int> VectorRank,
    Option<int> SpatialRank,
    Option<int> LexicalRank,
    double FusedScore,
    Option<UInt128> ProvenanceHead);

public static class FusionRank {
    public const int DefaultRrfConstant = 60;

    public static Seq<FusionCandidate> Fuse(
        Seq<(UInt128 Key, int Rank)> vector,
        Seq<(UInt128 Key, int Rank)> spatial,
        Seq<(UInt128 Key, int Rank)> lexical,
        FusionWeights weights,
        int k,
        int rrfConstant = DefaultRrfConstant) {
        var v = vector.ToHashMap(static hit => hit.Key, static hit => hit.Rank);
        var s = spatial.ToHashMap(static hit => hit.Key, static hit => hit.Rank);
        var l = lexical.ToHashMap(static hit => hit.Key, static hit => hit.Rank);
        var keys = toSeq(v.Keys.Append(s.Keys).Append(l.Keys).Distinct());
        return toSeq(keys
            .Map(key => new FusionCandidate(
                key,
                v.Find(key).ToOption(),
                s.Find(key).ToOption(),
                l.Find(key).ToOption(),
                Score(weights.Vector, v.Find(key), rrfConstant) + Score(weights.Spatial, s.Find(key), rrfConstant) + Score(weights.Lexical, l.Find(key), rrfConstant),
                None))
            .OrderByDescending(static c => c.FusedScore)
            .Take(k));
    }

    public static string FuseSql(FusionWeights weights, int k, int rrfConstant) =>
        $"""
        WITH vector_branch AS (SELECT key, ROW_NUMBER() OVER (ORDER BY embedding <=> $probe) AS rank FROM federated_entity ORDER BY embedding <=> $probe LIMIT {k}),
        spatial_branch AS (SELECT key, ROW_NUMBER() OVER (ORDER BY geom <-> $point) AS rank FROM federated_entity WHERE ST_DWithin(geom, $point, $radius) LIMIT {k}),
        lexical_branch AS (SELECT key, ROW_NUMBER() OVER (ORDER BY pdb.score(key) DESC) AS rank FROM federated_entity WHERE corpus @@@ $terms LIMIT {k})
        SELECT key, SUM(weight / ({rrfConstant} + rank)) AS fused FROM (
            SELECT key, rank, {weights.Vector} AS weight FROM vector_branch
            UNION ALL SELECT key, rank, {weights.Spatial} FROM spatial_branch
            UNION ALL SELECT key, rank, {weights.Lexical} FROM lexical_branch) ranked
        GROUP BY key ORDER BY fused DESC LIMIT {k}
        """;

    private static double Score(double weight, Option<int> rank, int rrfConstant) =>
        rank.Map(r => weight / (rrfConstant + r)).IfNone(0.0);
}
```

## [7]-[FEDERATED_PLAN]

- Owner: `PlanNode` `[Union]` the cross-store query AST; `EngineCost` the per-engine cost estimate row; `FederatedPlan` the static surface owning predicate pushdown across stores, cost-based engine selection, partial-pushdown splitting, and the cross-store join fold.
- Cases: `Scan | Filter | Join | Aggregate | Pushdown | Materialize` on `PlanNode`; cost-based selection picks the engine (pg relational, pg GiST, pg jsonb, DuckDB analytical, pgvector) whose estimated cost is lowest for a node, and partial-pushdown splits a node into a pushed sub-plan and a residual local fold.
- Entry: `public static PlanNode Plan(SetExpr selection, Func<PlanNode, EngineCost> cost)` — lowers an element-set selection into a cost-optimal cross-store plan; `public static (PlanNode Pushed, PlanNode Residual) Split(PlanNode node, FrozenSet<string> pushableOps)` partitions a plan into the store-pushable sub-plan and the local residual.
- Auto: the federation is single-source-per-residence by design — one element's relational, spatial, document, vector, and analytical facets live in one `PostgresServer` row set (plus the embedded `DuckDB` analytical lane), so the planner pushes each predicate to the engine that owns its index rather than fanning across separate stores: a `BySpatial` leaf pushes to the GiST index, a `ByProperty` leaf to the jsonb GIN index, a vector probe to the HNSW/diskann index, and an analytical aggregate to the DuckDB lane through the live attach; cost-based selection reads the index-route facts (`search.vector.route`, `search.spatial.route`) and the `pg_stat_statements` evidence so a node routes to the cheapest engine; partial-pushdown splits a node when only part of it is pushable — the pushable predicate executes in the store and the residual (a cross-engine join or a managed fold) executes locally over the pushed result, never client-filtering a full scan.
- Receipt: a plan rides `store.plan.federated` carrying the engine selection per node and the pushdown ratio; a partial-pushdown rides `store.plan.partial`.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, DuckDB.NET.Data.Full, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new plan operator is one `PlanNode` case plus one `Split` arm; a new selection lowering is one `SetExpr` arm on `Plan`; a new engine is one `EngineCost` row; a new pushdown rule is one entry in the pushable-op set; zero new surface — `Plan` and `Split` are total generated `Switch` folds so a new `SetExpr` or `PlanNode` case breaks the build rather than routing to a silent default, and a generic federated query engine fanning a query across heterogeneous remote stores is deliberately not built (the suite is single-source-per-residence), so this owner is the within-store cross-engine planner over the one federated table, never a multi-database query federator.
- Boundary: federation here is cross-engine within one residence, never cross-store across heterogeneous databases — the deliberate single-source-per-residence posture means one element's facets live in one PG row set so the planner pushes each predicate to the engine (relational, GiST, jsonb GIN, HNSW, DuckDB) that owns its index, and a true multi-database query federator (postgres_fdw fan-out, a cross-store join engine) stays the rejected form because the per-store rail is world-class and a generic federator re-derives it; cost-based selection reads the live index-route and `pg_stat_statements` evidence so engine choice is data, never a hardcoded route; partial-pushdown is the one mechanism that keeps a non-fully-pushable plan efficient — the pushable predicate runs in the store and only the irreducible residual (a managed cross-engine join or a fold the translator cannot express) runs locally over the already-filtered result, so a client-side filter over a full scan is the deleted form; the plan operands are `SetExpr` selections so the federated planner is the lowering target of the element-set algebra and the rule engine, one plan surface every selection rides.

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record PlanNode {
    private PlanNode() { }

    public sealed record Scan(string Table, PlanEngine Engine) : PlanNode;
    public sealed record Filter(PlanNode Source, string Predicate, PlanEngine Engine) : PlanNode;
    public sealed record Join(PlanNode Left, PlanNode Right, string On, PlanEngine Engine) : PlanNode;
    public sealed record Aggregate(PlanNode Source, string Measure, PlanEngine Engine) : PlanNode;
    public sealed record Pushdown(PlanNode Inner, PlanEngine Target) : PlanNode;
    public sealed record Materialize(PlanNode Source) : PlanNode;
}

public static class FederatedPlan {
    public static PlanNode Plan(SetExpr selection, Func<PlanNode, EngineCost> cost) =>
        selection.Switch(
            state: cost,
            bySpatial:        static (c, sp) => new PlanNode.Filter(new PlanNode.Scan("federated_entity", PlanEngine.Spatial), sp.GeometryPredicate, PlanEngine.Spatial),
            byProperty:       static (c, pr) => new PlanNode.Filter(new PlanNode.Scan("federated_entity", PlanEngine.Document), pr.JsonPath, PlanEngine.Document),
            byClassification: static (c, cl) => new PlanNode.Filter(new PlanNode.Scan("federated_entity", PlanEngine.Relational), cl.ClassificationPath, PlanEngine.Relational),
            union:            static (c, u) => new PlanNode.Join(Plan(u.Left, c), Plan(u.Right, c), "union", Cheapest(c, Plan(u.Left, c), Plan(u.Right, c))),
            intersect:        static (c, i) => new PlanNode.Join(Plan(i.Left, c), Plan(i.Right, c), "intersect", Cheapest(c, Plan(i.Left, c), Plan(i.Right, c))),
            difference:       static (c, d) => new PlanNode.Join(Plan(d.Left, c), Plan(d.Right, c), "difference", PlanEngine.Relational),
            literal:          static (c, _) => new PlanNode.Scan("federated_entity", PlanEngine.Relational),
            byRule:           static (c, _) => new PlanNode.Scan("federated_entity", PlanEngine.Relational),
            closure:          static (c, _) => new PlanNode.Scan("federated_entity", PlanEngine.Relational));

    public static (PlanNode Pushed, PlanNode Residual) Split(PlanNode node, FrozenSet<string> pushableOps) =>
        node.Switch(
            state: pushableOps,
            filter:      static (ops, f) => ops.Contains(f.Engine.Key)
                ? (Pushed: (PlanNode)new PlanNode.Pushdown(f, f.Engine), Residual: (PlanNode)new PlanNode.Materialize(f))
                : (Pushed: new PlanNode.Pushdown(f, PlanEngine.Relational), Residual: new PlanNode.Materialize(f)),
            join:        static (ops, j) => (Pushed: (PlanNode)new PlanNode.Pushdown(j.Left, j.Engine), Residual: j),
            scan:        static (ops, s) => (Pushed: (PlanNode)new PlanNode.Pushdown(s, PlanEngine.Relational), Residual: new PlanNode.Materialize(s)),
            aggregate:   static (ops, a) => (Pushed: (PlanNode)new PlanNode.Pushdown(a, a.Engine), Residual: new PlanNode.Materialize(a)),
            pushdown:    static (ops, p) => (Pushed: (PlanNode)p, Residual: new PlanNode.Materialize(p)),
            materialize: static (ops, m) => (Pushed: (PlanNode)new PlanNode.Pushdown(m, PlanEngine.Relational), Residual: m));

    private static PlanEngine Cheapest(Func<PlanNode, EngineCost> cost, PlanNode left, PlanNode right) =>
        cost(left).Estimate <= cost(right).Estimate ? cost(left).Engine : cost(right).Engine;
}
```

## [8]-[RESEARCH]

- [IDS_FACET_SCHEMA]: the buildingSMART IDS 1.0 XSD element/attribute shape the `IdsImport.Facet` reader projects — the `specification` `name`/`identifier`/`ifcVersion` attributes, the `applicability`/`requirements` element split, the per-facet `simpleValue`/`restriction` child grammar (`entity.name`, `attribute.name`/`value`, `classification.system`/`value`, `property.propertySet`/`baseName`/`dataType`/`value`, `material.value`, `partOf.relation`/`entity`), and the `requirement` `cardinality` attribute (`required`/`prohibited`/`optional`) — confirmed against the published IDS 1.0 schema before the `Facet` element-name dispatch and the `IdsConformance` receipt shape pin; the `restriction`-based pattern/enumeration facet values lower to a richer `SetExpr.ByProperty` predicate than the `simpleValue` equality the fence carries today.
- [SPATIAL_CLASH_PUSHDOWN]: the PostGIS `ST_3DIntersects`/`ST_3DDWithin` pushdown for a `Clash` rule over a GiST-indexed `geometry`/`geometryz` column on PG18 — the index-route the planner observes for a two-set spatial intersection and the incremental-clash form re-testing only the structural-diff changed-node set against the GiST index rather than the full cross-product.
- [FUSION_PLANNER_COST]: the cost-model inputs the `FederatedPlan` reads to rank the GiST, jsonb-GIN, HNSW, and DuckDB-attach engines for a node — whether `pg_stat_statements` plan-cost evidence and the `search.*.route` facts are sufficient to drive cost-based engine selection, and the three-way `FusionRank.FuseSql` CTE planner route fusing the spatial branch beside the vector and BM25 branches on a live PG18 server carrying pgvectorscale and pg_search.

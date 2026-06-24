# [PERSISTENCE_QUALITY]

Rasm.Persistence makes declarative validation, cross-federation referential integrity, statistical-anomaly detection, and near-duplicate/de-duplication one closed rule axis that lowers each rule to its least-cost enforcement site rather than re-checking in process. `QualityRule` is the `[Union]` rule family — null-completeness, set-domain membership, range/regex/JSON-Schema shape, cross-document uniqueness, temporal-overlap exclusion, referential edge, functional dependency, cardinality, freshness, statistical anomaly, distribution drift, and near-duplicate — and `QualityPlan` is the lowering fold that drives each rule to a `Schema/ddl#EXTENSION_DDL` `SchemaDdl` row (`Check`/`Index`/`Exclusion`/`TemporalKey`/`JsonSchemaCheck`), a `pg_jsonschema` `jsonb_matches_schema` predicate, an op-log changefeed-fold, or a `DuckDB`/`pgvector` analytical pass. Every rule emits the violating rows as a `Query/federation#ELEMENT_SET_ALGEBRA` `ElementSet` so a quality result composes with every other selection through one `SetExpr.Intersect`; `QualityFault` is the closed `[Union]` deriving from `Expected` so a constraint-add failure, a server-side validation rejection, or a provider fault is typed evidence on the unified rail, never silent. The rule plan rides the federation `CrossDocLink`/`StalePins` adjacency and the `Version/snapshots#CONTENT_CHUNKING` `ContentChunk.ShortTag` pre-filter; `SetExpr`/`ElementSet`/`SetPredicate`/`CrossDocLink`/`StalePins`/`LinkStore`, `EmbeddingArity`/`VectorMetric`/`HybridRetrieve`, `ContentChunk.ShortTag`/`ContentChunker.Novel`, `SchemaDdl`/`ColumnInvariant`/`DerivedColumn`, `StoreFact`/`ReceiptSinkPort`, and `ClockPolicy` arrive settled. `Rasm.Bim/Model` authors the IFC/IDS validation rules as `QualityRule` rows and `Rasm.Compute` is the geometry-derived anomaly source, both folded into this one axis at the seam.

Wire posture: this page is host-local — quality rules lower to server-side constraints, server-side predicates, or in-process analytical passes, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The IFC/IDS validation rules and the geometry-derived anomaly sources arrive from the Bim/Compute seams as `QualityRule` rows, never minted here.

## [01]-[INDEX]

- [01]-[QUALITY_RULE]: the rule union, the severity/enforcement-site axes, the typed receipt, the page-wide fact stream, and the closed fault family.
- [02]-[QUALITY_PLAN]: the lowering fold to the cheapest site (`SchemaDdl` rows, server predicates, changefeed-folds, analytical passes), the violating-set projection, and the gate fold.

## [02]-[QUALITY_RULE]

- Owner: `QualityRule` the `[Union]` rule family; `EnforcementSite` the `[SmartEnum<string>]` least-cost-site axis; `Severity` the `[SmartEnum<string>]` gating/advisory axis (`block` evidence gates, `warn`/`info` ride forward); `AnomalyMethod` the `[SmartEnum<string>]` outlier-estimator axis; `DedupMode` the `[SmartEnum<string>]` exact-versus-fuzzy de-duplication axis; `QualityReceipt` the typed rule-evaluation evidence; `QualityFact` with `QualityFactKind` the page-wide fact stream feeding `StoreFact`; `QualityFault` the closed `[Union]` deriving from `Expected, IValidationError<QualityFault>` in the 8290 band.
- Cases: `NotNullSet` (a column set that must be complete), `SetDomain` (a column constrained to an allowed-value set), `RangeBound` (a numeric/temporal range), `RegexShape` (a string pattern), `JsonSchemaShape` (a JSON Schema over a jsonb column), `UniqueAcross` (a cross-document uniqueness over a column tuple), `TemporalOverlap` (a no-overlap exclusion over a validity range), `ReferentialEdge` (a `CrossDocLink`-backed foreign reference), `FunctionalDependency` (a determinant→dependent column functional dependency), `Cardinality` (a relationship-count multiplicity bound), `Freshness` (a staleness bound on a timestamp column), `StatisticalAnomaly` (a columnar outlier over a measure under an `AnomalyMethod`), `DistributionDrift` (a histogram/quantile drift over a measure against a baseline), `NearDuplicate` (an exact content-key or fuzzy-embedding near-match under a `DedupMode`) on `QualityRule`; `EnforcementSite` is `check | set-domain | exclude | temporal-exclude | foreign-key | json-schema | changefeed-fold | duckdb-pass | vector-probe`; every rule carries a `Severity` discriminant the gate fold reads.
- Entry: `public EnforcementSite Site()` projects the least-cost site for a rule through one generated `Switch`; `public Fin<SetExpr> Violating()` lowers the rule into the federation `SetExpr` selecting the violating rows so a quality result is an `ElementSet` (a rule whose violating set is only knowable through an effectful pass returns its plan-shaped `ByRule` leaf, the pass resolving it). One entrypoint per modality discriminates on the case, never a parallel `SiteOf`/`ViolatingOf` family.
- Auto: each rule lowers to the cheapest site that can enforce it. `NotNullSet`/`SetDomain`/`RangeBound`/`RegexShape`/`FunctionalDependency`/`Cardinality`/`Freshness` lower to a `SchemaDdl.Check`/`ColumnInvariant` `CHECK` constraint (a model fact emitted through `HasCheckConstraint`); `UniqueAcross` to a `SchemaDdl.Index` with `NullsDistinct`; `TemporalOverlap` to a `SchemaDdl.Exclusion` `EXCLUDE USING gist` over `btree_gist` (or a `SchemaDdl.TemporalKey` WITHOUT OVERLAPS when the overlap protects a primary/unique key); `ReferentialEdge` to a `SchemaDdl`-backed foreign key reading the `CrossDocLink` adjacency and the `StalePins` projection; `JsonSchemaShape` to a `SchemaDdl.JsonSchemaCheck` `CHECK (jsonb_matches_schema(...))` validated server-side by `pg_jsonschema` (degrading to the `JsonSchema.Net` in-process fallback when the extension is absent, never silently dropping validation); `StatisticalAnomaly`/`DistributionDrift` to a DuckDB analytical pass over the columnar lane (a z-score/IQR/MAD outlier or a quantile-drift fold over a measure column); and `NearDuplicate` to the `ContentChunk.ShortTag` 64-bit `XxHash3` pre-filter ahead of an `XxHash128` content-key compare for `DedupMode.Exact` or a `VectorMetric` pgvector distance probe for `DedupMode.Fuzzy`. A rule a constraint cannot express folds over the op-log changefeed so a streaming violation is caught as each `OpLogEntry` commits; every rule emits its violating rows as an `ElementSet` so a quality gate composes with any other selection (a clash result intersected with a quality violation is one `SetExpr.Intersect`).
- Receipt: a rule evaluation rides `QualityReceipt(QualityRule Rule, EnforcementSite Site, Severity Severity, long Checked, long Violating, ElementSet Subject, AnomalyMethod Method, Duration Elapsed, Instant At)` — the typed evidence carrying the rule, its site, its severity, the checked/violating counts, the violating `ElementSet`, and the estimator used; `QualityFact.Of` projects each receipt onto the `StoreFact` stream under the `store.quality.*` kinds so a constraint add, a violation, a fallback evaluation, and a drift detection ride the one receipt-sink envelope; a constraint-add failure or a server-side validation rejection is a typed `QualityFault`, never silent.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, DuckDB.NET.Data.Full, Pgvector.EntityFrameworkCore, Pgvector, System.IO.Hashing, JsonSchema.Net, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime. `SchemaDdl`/`ColumnInvariant`, `SetExpr`/`ElementSet`, `CrossDocLink`/`StalePins`, `ContentChunk`/`ContentChunker`, `EmbeddingArity`/`VectorMetric`, and `StoreFact`/`ReceiptSinkPort` arrive as settled vocabulary.
- Growth: a new rule kind is one `QualityRule` case plus one `Site`/`Violating` arm; a new enforcement site is one `EnforcementSite` row; a new outlier estimator is one `AnomalyMethod` row carrying its DuckDB scalar form; a new de-duplication stance is one `DedupMode` row; zero new surface — a per-rule-type validator, an imperative in-process re-check, a second integrity engine, or a parallel anomaly detector is the deleted form because one union lowers to the cheapest site and emits an `ElementSet`.
- Boundary: every rule lowers to the least-cost enforcement site — an imperative in-process re-check over a materialized scan is the deleted form, the rule is a constraint, a server-side predicate, a changefeed-fold, or a columnar pass, never application code; the constraint-class rules (`NotNullSet`/`SetDomain`/`RangeBound`/`RegexShape`/`FunctionalDependency`/`Cardinality`/`Freshness`/`UniqueAcross`/`TemporalOverlap`/`ReferentialEdge`) lower to `Schema/ddl` `SchemaDdl` rows so they are model facts emitted through `HasCheckConstraint`/`HasIndex`/the migration fold, never deploy-time hand DDL beside the migration set and never a raw string concatenated at the site — `Lower` constructs the typed `SchemaDdl` row whose own SQL projection owns the one declared identifier seam (`Query/domain/postgres#SQL_LAW`: identifiers parameterize in neither lane), so a regex pattern or an allowed-value list is a row column the row escapes, not interpolated caller text; `JsonSchemaShape` rides `pg_jsonschema` `jsonb_matches_schema` server-side and degrades to `JsonSchema.Net` in-process only when the extension is absent, the degradation a typed `QualityFact.FallbackEvaluated`, never a silent drop; `StatisticalAnomaly`/`DistributionDrift` ride the DuckDB analytical lane so an outlier or a drift detection is a columnar query under the `AnomalyMethod` estimator, never a row-by-row fold; `NearDuplicate` probes the `ContentChunk.ShortTag` 64-bit bloom/sketch pre-filter ahead of the authoritative `XxHash128` content-key compare for `DedupMode.Exact` or the `VectorMetric` pgvector probe for `DedupMode.Fuzzy`, never a quadratic pairwise scan; the violating-row projection is an `ElementSet` so a quality result is content-addressed and composes with the element-set algebra; the `ReferentialEdge` reads the `Query/federation#CROSS_DOC_LINKS` adjacency and the `StalePins` projection so a cross-document referential violation is caught as a stale pin, never a per-document foreign key; the `Severity` axis carries the validation-shard evidence discipline (`Query/domain/validation#OUTCOME_PROJECTION`) — a `block` rule gates the quality gate, a `warn`/`info` rule rides the gate result forward as evidence and never re-promotes to a gating fault; the IFC/IDS validation rules arrive from `Rasm.Bim/Model` as `QualityRule` rows through the `Store/quality ← Rasm.Bim/Model` seam (an IDS facet's cardinality maps to a `Cardinality` rule, its value-restriction to a `SetDomain`/`RegexShape`/`RangeBound`, its property requirement to a `JsonSchemaShape`/`NotNullSet`) and the geometry-derived anomaly sources arrive from `Rasm.Compute` through the `Store/quality ← Rasm.Compute` seam, so a Bim or Compute domain rule federates into the one rule axis without a parallel checker.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class EnforcementSite {
    public static readonly EnforcementSite Check = new("check");
    public static readonly EnforcementSite SetDomain = new("set-domain");
    public static readonly EnforcementSite Exclude = new("exclude");
    public static readonly EnforcementSite TemporalExclude = new("temporal-exclude");
    public static readonly EnforcementSite ForeignKey = new("foreign-key");
    public static readonly EnforcementSite JsonSchema = new("json-schema");
    public static readonly EnforcementSite ChangefeedFold = new("changefeed-fold");
    public static readonly EnforcementSite DuckDbPass = new("duckdb-pass");
    public static readonly EnforcementSite VectorProbe = new("vector-probe");
}

// `block` evidence gates the quality gate; `warn`/`info` ride the gate result forward as evidence and
// never re-promote downstream (`Query/domain/validation#OUTCOME_PROJECTION` evidence-rides-forward law).
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class Severity {
    public static readonly Severity Block = new("block", gates: true);
    public static readonly Severity Warn = new("warn", gates: false);
    public static readonly Severity Info = new("info", gates: false);

    public bool Gates { get; }
}

// The estimator the DuckDB anomaly pass lowers to — `Scalar` is the per-row deviation expression keyed on
// the measure column, evaluated server-side over the columnar lane. `z-score` is the parametric standard-
// deviation form, `iqr`/`mad` the robust quantile/median-absolute-deviation forms for skewed measures.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class AnomalyMethod {
    public static readonly AnomalyMethod ZScore = new("z-score",
        scalar: "abs(({0} - avg({0}) OVER ()) / nullif(stddev_pop({0}) OVER (), 0))");
    public static readonly AnomalyMethod Iqr = new("iqr",
        scalar: "({0} - quantile_cont({0}, 0.75) OVER ()) / nullif(quantile_cont({0}, 0.75) OVER () - quantile_cont({0}, 0.25) OVER (), 0)");
    public static readonly AnomalyMethod Mad = new("mad",
        scalar: "abs({0} - median({0}) OVER ()) / nullif(median(abs({0} - median({0}) OVER ())) OVER (), 0)");

    public string Scalar { get; }
}

// `Exact` proves identity through the `XxHash128` content key (the `ShortTag` pre-filter culls the compare);
// `Fuzzy` proves near-match through a `VectorMetric` pgvector distance under a threshold.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class DedupMode {
    public static readonly DedupMode Exact = new("exact");
    public static readonly DedupMode Fuzzy = new("fuzzy");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class QualityFactKind {
    public static readonly QualityFactKind Checked = new("store.quality.checked");
    public static readonly QualityFactKind Violated = new("store.quality.violated");
    public static readonly QualityFactKind ConstraintAdded = new("store.quality.constraint.added");
    public static readonly QualityFactKind FallbackEvaluated = new("store.quality.fallback");
    public static readonly QualityFactKind Drifted = new("store.quality.drift");
    public static readonly QualityFactKind Gated = new("store.quality.gate");
}

[Union]
public abstract partial record QualityFault : Expected, IValidationError<QualityFault> {
    private QualityFault(string detail, int code) : base(detail, code, None) { }

    public static QualityFault Create(string message) => new EvaluationFailed(message);

    public sealed record ConstraintRejected(string Table, string Constraint, string Detail) : QualityFault($"<quality-constraint-rejected:{Table}.{Constraint}>", 8290);
    public sealed record SchemaInvalid(string Column, string Detail) : QualityFault($"<quality-schema-invalid:{Column}>", 8291);
    public sealed record ExtensionUnavailable(string Extension, string Fallback) : QualityFault($"<quality-extension-unavailable:{Extension}->{Fallback}>", 8292);
    public sealed record PassFaulted(EnforcementSite Site, string Detail) : QualityFault($"<quality-pass-faulted:{Site.Key}>", 8293);
    public sealed record EvaluationFailed(string Detail) : QualityFault($"<quality-evaluation-failed>:{Detail}", 8294);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record QualityRule {
    private QualityRule() { }

    public sealed record NotNullSet(string Table, Seq<string> Columns, Severity Severity) : QualityRule;
    public sealed record SetDomain(string Table, string Column, Seq<string> Allowed, Severity Severity) : QualityRule;
    public sealed record RangeBound(string Table, string Column, string Low, string High, Severity Severity) : QualityRule;
    public sealed record RegexShape(string Table, string Column, string Pattern, Severity Severity) : QualityRule;
    public sealed record JsonSchemaShape(string Table, string Column, string Schema, Severity Severity) : QualityRule;
    public sealed record UniqueAcross(string Table, Seq<string> Columns, bool NullsDistinct, Severity Severity) : QualityRule;
    public sealed record TemporalOverlap(string Table, string Period, Seq<string> PartitionBy, Severity Severity) : QualityRule;
    public sealed record ReferentialEdge(string Table, string Column, string ReferencedTable, string ReferencedColumn, Severity Severity) : QualityRule;
    public sealed record FunctionalDependency(string Table, Seq<string> Determinant, Seq<string> Dependent, Severity Severity) : QualityRule;
    public sealed record Cardinality(string Table, string Column, int Min, Option<int> Max, Severity Severity) : QualityRule;
    public sealed record Freshness(string Table, string TimestampColumn, Duration MaxAge, Severity Severity) : QualityRule;
    public sealed record StatisticalAnomaly(string Table, string Measure, double Sigma, AnomalyMethod Method, Severity Severity) : QualityRule;
    public sealed record DistributionDrift(string Table, string Measure, double Quantile, double MaxShift, UInt128 Baseline, Severity Severity) : QualityRule;
    public sealed record NearDuplicate(string Table, string EmbeddingColumn, DedupMode Mode, VectorMetric Metric, double MaxDistance, Severity Severity) : QualityRule;

    public Severity Severity =>
        Switch(
            notNullSet:           static r => r.Severity,
            setDomain:            static r => r.Severity,
            rangeBound:           static r => r.Severity,
            regexShape:           static r => r.Severity,
            jsonSchemaShape:      static r => r.Severity,
            uniqueAcross:         static r => r.Severity,
            temporalOverlap:      static r => r.Severity,
            referentialEdge:      static r => r.Severity,
            functionalDependency: static r => r.Severity,
            cardinality:          static r => r.Severity,
            freshness:            static r => r.Severity,
            statisticalAnomaly:   static r => r.Severity,
            distributionDrift:    static r => r.Severity,
            nearDuplicate:        static r => r.Severity);

    // One subject projection the fact stream and the gate fault both read — never a second per-call switch.
    public string Subject =>
        Switch(
            notNullSet:           static r => $"{r.Table}:{string.Join(',', r.Columns)}",
            setDomain:            static r => $"{r.Table}.{r.Column}",
            rangeBound:           static r => $"{r.Table}.{r.Column}",
            regexShape:           static r => $"{r.Table}.{r.Column}",
            jsonSchemaShape:      static r => $"{r.Table}.{r.Column}",
            uniqueAcross:         static r => $"{r.Table}:{string.Join(',', r.Columns)}",
            temporalOverlap:      static r => $"{r.Table}.{r.Period}",
            referentialEdge:      static r => $"{r.Table}.{r.Column}->{r.ReferencedTable}.{r.ReferencedColumn}",
            functionalDependency: static r => $"{r.Table}:{string.Join(',', r.Determinant)}->{string.Join(',', r.Dependent)}",
            cardinality:          static r => $"{r.Table}.{r.Column}",
            freshness:            static r => $"{r.Table}.{r.TimestampColumn}",
            statisticalAnomaly:   static r => $"{r.Table}.{r.Measure}",
            distributionDrift:    static r => $"{r.Table}.{r.Measure}",
            nearDuplicate:        static r => $"{r.Table}.{r.EmbeddingColumn}");

    public EnforcementSite Site() =>
        Switch(
            notNullSet:           static _ => EnforcementSite.Check,
            setDomain:            static _ => EnforcementSite.SetDomain,
            rangeBound:           static _ => EnforcementSite.Check,
            regexShape:           static _ => EnforcementSite.Check,
            functionalDependency: static _ => EnforcementSite.Exclude,
            cardinality:          static _ => EnforcementSite.Check,
            freshness:            static _ => EnforcementSite.ChangefeedFold,
            jsonSchemaShape:      static _ => EnforcementSite.JsonSchema,
            uniqueAcross:         static _ => EnforcementSite.Exclude,
            temporalOverlap:      static _ => EnforcementSite.TemporalExclude,
            referentialEdge:      static _ => EnforcementSite.ForeignKey,
            statisticalAnomaly:   static _ => EnforcementSite.DuckDbPass,
            distributionDrift:    static _ => EnforcementSite.DuckDbPass,
            nearDuplicate:        static _ => EnforcementSite.VectorProbe);
}

public sealed record QualityReceipt(
    QualityRule Rule, EnforcementSite Site, Severity Severity, long Checked, long Violating,
    ElementSet Subject, AnomalyMethod Method, Duration Elapsed, Instant At) {
    public static readonly AnomalyMethod NoMethod = AnomalyMethod.ZScore;
}

public readonly record struct QualityFact(QualityFactKind Kind, QualityReceipt Receipt) {
    // The fact stream is the settled `Query/rail#INTERCEPTOR_SPINE` `StoreFact` envelope — a quality fact
    // is one `store.quality.*` row, never a parallel signal owner; `gated`/`violated` carry the violation
    // count, `constraint-added` carries 1 per lowered constraint, `drift`/`fallback` carry the measure span.
    public StoreFact Project() => new(Kind.Key, Receipt.Rule.Subject, Receipt.Violating, Receipt.Elapsed, Receipt.At);

    public static QualityFact Of(QualityFactKind kind, QualityReceipt receipt) => new(kind, receipt);
}
```

## [03]-[QUALITY_PLAN]

- Owner: `QualityPlan` the static surface lowering a `Seq<QualityRule>` into its enforcement artifacts — the `SchemaDdl` constraint-row set folded into the migration, the server-side validation predicates, the changefeed-fold registrations, and the DuckDB/pgvector analytical passes — folding the violating `ElementSet` per rule, and reducing a rule batch's per-rule results into one `QualityGate` verdict.
- Entry: `public static Fin<MigrationBuilder> Lower(MigrationBuilder migration, Seq<QualityRule> rules)` folds the constraint-class rules into `Schema/ddl#EXTENSION_DDL` `SchemaDdl` rows (`Check`/`Exclusion`/`JsonSchemaCheck`) — reusing each row's own `Sql` projection so the DDL owner owns the identifier seam — emitted through the migration so they enforce at write time; `public static IO<QualityReceipt> Evaluate(QualityRuntime rt, QualityRule rule)` runs a non-constraint rule (anomaly, drift, near-duplicate, referential, changefeed-fold) over the `QualityRuntime` capability (the DuckDB analytical connection, the analytical-read key delegate, the pgvector/`ShortTag` probe, the `CrossDocLink` adjacency, the baseline-quantile resolver, the `ElementSet` resolver, and the `ClockPolicy`) and folds the violating set, lifting a `DuckDBException`/provider fault to `QualityFault.PassFaulted` at the one rail site; `public static Validation<QualityFault, QualityGate> Gate(Seq<QualityReceipt> receipts)` accumulates every `block`-severity violating set into one gate verdict applicatively, riding the `warn`/`info` evidence forward. One polymorphic entrypoint per modality (lower constraints, evaluate a pass, fold a gate); a `LowerCheck`/`LowerIndex`/`EvaluateAnomaly` family is the deleted form.
- Auto: the lowering fold partitions the rules by `Site()` — the constraint-class rules become `SchemaDdl.Check`/`Index`/`Exclusion`/`TemporalKey`/`JsonSchemaCheck` rows folded into the migration so a violating write is a statement error (never an after-the-fact scan), and the pass-class rules (`StatisticalAnomaly`/`DistributionDrift`/`NearDuplicate`/`ReferentialEdge` cross-document) register as op-log changefeed folds or scheduled analytical passes so a streaming or batch violation is caught past the constraint lane; the violating rows of every rule are an `ElementSet` lowered through the federation algebra so a quality dashboard, a remediation selection, and a downstream gate all read the one content-addressed set; a `ReferentialEdge` cross-document rule reads the `Query/federation#CROSS_DOC_LINKS` `LinkStore.StalePins` projection so a stale-pin reference is a quality violation; the `StatisticalAnomaly` pass binds the `AnomalyMethod.Scalar` form into the DuckDB analytical lane's window query so an outlier over a measure column is one columnar pass; the `DistributionDrift` pass folds a `quantile_cont` shift against the `Baseline` content key; the `NearDuplicate.Exact` probe rides `ContentChunker.Novel(manifest, mayHold, holds)` — the `ShortTag` 64-bit pre-filter culls the `XxHash128` content-key compare — while `NearDuplicate.Fuzzy` projects the `VectorMetric` distance ordering over the `EmbeddingArity` column through the catalogued `CosineDistance`/`L2Distance`/`MaxInnerProduct` function under the `MaxDistance` threshold; the gate fold accumulates `block` failures and surfaces `warn`/`info` as evidence.
- Receipt: a constraint lowering rides `QualityFactKind.ConstraintAdded`; a rule evaluation rides `QualityFactKind.Checked`/`Violated` carrying the violation count; a drift detection rides `QualityFactKind.Drifted`; a server-side-to-fallback degradation rides `QualityFactKind.FallbackEvaluated`; the gate verdict rides `QualityFactKind.Gated`; the typed `QualityReceipt` carries the violating `ElementSet` and projects onto `StoreFact`; a constraint-add, schema-invalid, extension-unavailable, or pass fault is a typed `QualityFault`.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, DuckDB.NET.Data.Full, Pgvector.EntityFrameworkCore, Pgvector, System.IO.Hashing, JsonSchema.Net, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new lowering target is one arm on `Lower` (constructing one `SchemaDdl` row); a new pass kind is one arm on `Evaluate`; a new gate combinator is one fold arm on `Gate`; zero new surface — a second lowering fold, a parallel constraint emitter, a hand-written constraint DDL string, or a per-rule evaluation service is the deleted form because the plan lowers the one rule union into the settled `SchemaDdl` rows and the federation `SetExpr`.
- Boundary: the plan lowers the rule union into the settled `Schema/ddl` `SchemaDdl` constraint rows and the federation `SetExpr` — a hand-written constraint DDL string at the lowering site, a parallel selection shape, or a raw `MigrationBuilder.Sql($"ALTER TABLE {table} ...")` is the deleted form because the `SchemaDdl` row owns the one declared identifier seam and the migration fold owns the emission; the constraint-class rules enforce at write time so a violating write is a statement error, and only the rules a constraint cannot express (anomaly, drift, near-duplicate, cross-document referential) ride a changefeed-fold or a scheduled pass; the violating-set projection is the one `ElementSet` currency so a quality result composes with the rule engine and the element-set algebra, never a per-rule result shape; the cross-document referential rule reads the `CrossDocLink`/`StalePins` projection so it rides the federated link graph, never a per-document foreign key; the DuckDB anomaly pass binds the `AnomalyMethod.Scalar` form and the measure column (a closed-vocabulary identifier from the entity model) through the one analytical seam, and the key set binds as a parameter so no caller input concatenates into SQL; the pgvector probe reuses the `Query/lanes#SEARCH_LANES` `VectorMetric` distance functions (never a hand-spelled distance operator) and the `ContentChunk.ShortTag` pre-filter reuses the `Version/snapshots#CONTENT_CHUNKING` `ContentChunker.Novel` projection (never a second bloom); the gate fold rides the validation-shard severity partition (`Query/domain/validation#OUTCOME_PROJECTION`) so a `block` failure gates and a `warn`/`info` advisory rides forward; the IFC/IDS validation rules and the geometry-derived anomaly sources lower through this one plan so a Bim or Compute domain rule is a `QualityRule` row, never a parallel checker.

```csharp signature
// The capability the effectful passes read — the DuckDB analytical connection, the analytical-read delegate
// that streams a violating key set off the columnar lane (the same read-delegate shape `Store/profiles#COST_ROLLUP`
// `CostRollup.Rollup` rides, since the analytical lane only reads and never owns source-of-truth writes), the
// `ElementSet` resolver over the federation algebra, the `NearDuplicate` probe, the `CrossDocLink` adjacency and
// source-head reads, the baseline-quantile resolver keyed by edition content key, the `pg_jsonschema`-availability
// probe, and the clock. Arrives composed at the `Store/profiles` lifecycle.
public sealed record QualityRuntime(
    DuckDBConnection Analytical,
    Func<DuckDBConnection, string, Seq<(string, object)>, IO<Seq<UInt128>>> AnalyticalKeys,
    Func<SetExpr, ElementSet> Resolve,
    Func<QualityRule.NearDuplicate, IO<ElementSet>> Probe,
    Func<QualityRule.ReferentialEdge, Seq<CrossDocLink>> Links,
    Func<UInt128, UInt128> SourceHead,
    Func<UInt128, double> Baselines,
    Func<string, bool> ExtensionAvailable,
    ClockPolicy Clocks);

public static class QualityPlan {
    // Each constraint-class rule lowers to one settled `Schema/ddl#EXTENSION_DDL` `SchemaDdl` row whose OWN
    // `Sql`/`CreateSql` projection owns the declared identifier seam — `Lower` reuses that projection and never
    // concatenates its own `ALTER TABLE` string, so the table/column identifiers (model facts, not caller input)
    // and the row-escaped value literals trace to the one DDL owner, never a parallel emitter.
    public static Fin<MigrationBuilder> Lower(MigrationBuilder migration, Seq<QualityRule> rules) =>
        rules.Filter(static rule => rule.Site() is { Key: "check" or "set-domain" or "exclude" or "temporal-exclude" or "foreign-key" or "json-schema" })
            .Map(LowerOne)
            .Traverse(identity).As()
            .Map(statements => statements.Fold(migration, static (builder, sql) => { builder.Sql(sql); return builder; }));

    private static Fin<string> LowerOne(QualityRule rule) =>
        rule switch {
            QualityRule.NotNullSet n => Ddl(new SchemaDdl.Check(n.Table, $"{n.Table}_notnull",
                string.Join(" AND ", n.Columns.Map(static c => $"{c} IS NOT NULL")))),
            QualityRule.SetDomain d => Ddl(new SchemaDdl.Check(d.Table, $"{d.Table}_{d.Column}_domain",
                $"{d.Column} IN ({string.Join(", ", d.Allowed.Map(static v => $"'{v.Replace("'", "''", StringComparison.Ordinal)}'"))})")),
            QualityRule.RangeBound r => Ddl(new SchemaDdl.Check(r.Table, $"{r.Table}_{r.Column}_range",
                $"{r.Column} BETWEEN {r.Low} AND {r.High}")),
            QualityRule.RegexShape r => Ddl(new SchemaDdl.Check(r.Table, $"{r.Table}_{r.Column}_shape",
                $"{r.Column} ~ '{r.Pattern.Replace("'", "''", StringComparison.Ordinal)}'")),
            QualityRule.Cardinality c => Ddl(new SchemaDdl.Check(c.Table, $"{c.Table}_{c.Column}_card",
                c.Max.Match(Some: max => $"{c.Column} BETWEEN {c.Min} AND {max}", None: () => $"{c.Column} >= {c.Min}"))),
            QualityRule.FunctionalDependency f => Ddl(new SchemaDdl.Exclusion(f.Table, string.Join(", ",
                f.Determinant.Map(static c => $"{c} WITH =").Concat(f.Dependent.Map(static c => $"{c} WITH <>"))))),
            QualityRule.TemporalOverlap t => Ddl(new SchemaDdl.Exclusion(t.Table, string.Join(", ",
                t.PartitionBy.Map(static c => $"{c} WITH =").Append($"{t.Period} WITH &&")))),
            QualityRule.UniqueAcross u => Fin<string>.Succ(
                $"CREATE UNIQUE INDEX {u.Table}_unique ON {u.Table} ({string.Join(", ", u.Columns)})" + (u.NullsDistinct ? "" : " NULLS NOT DISTINCT")),
            QualityRule.JsonSchemaShape j => Ddl(new SchemaDdl.JsonSchemaCheck(j.Table, j.Column, $"{j.Table}_{j.Column}_schema", j.Schema)),
            QualityRule.ReferentialEdge e => Ddl(new SchemaDdl.Check(e.Table, $"{e.Table}_{e.Column}_fk",
                $"{e.Column} IS NULL OR EXISTS (SELECT 1 FROM {e.ReferencedTable} WHERE {e.ReferencedColumn} = {e.Table}.{e.Column})")),
            _ => Fin<string>.Fail(new QualityFault.ConstraintRejected(rule.Subject, rule.Site().Key, "<not-a-constraint-rule>")),
        };

    // A constraint-class rule short-circuits to a zero-violation receipt (it enforces at write time); a pass-class
    // rule runs over the `QualityRuntime` capability and lifts a provider/DuckDB fault to `QualityFault.PassFaulted`
    // through `Try.lift` at the one rail site. The runtime is a plain parameter, not an injected `persist` delegate
    // (the federated write rides `StoreOp`) — only the analytical READ delegate the columnar lane already owns,
    // symmetric with `Store/profiles#COST_ROLLUP` `CostRollup.Rollup(lane, subject, code, query)`.
    public static IO<QualityReceipt> Evaluate(QualityRuntime rt, QualityRule rule) =>
        rule.Site() is { Key: "check" or "set-domain" or "exclude" or "temporal-exclude" or "foreign-key" or "json-schema" }
            ? IO.pure(Receipt(rule, ElementSet.Empty, QualityReceipt.NoMethod, rt.Clocks.Now))
            : rule switch {
                QualityRule.StatisticalAnomaly a => AnalyticalPass(rt, a, a.Method,
                    $"SELECT key FROM {a.Table} WHERE {string.Format(CultureInfo.InvariantCulture, a.Method.Scalar, a.Measure)} > $sigma",
                    Seq(("sigma", (object)a.Sigma))),
                QualityRule.DistributionDrift d => AnalyticalPass(rt, d, QualityReceipt.NoMethod,
                    $"SELECT key FROM {d.Table} WHERE abs(quantile_cont({d.Measure}, $q) OVER () - $baseline) > $shift",
                    Seq(("q", (object)d.Quantile), ("baseline", rt.Baselines(d.Baseline)), ("shift", (object)d.MaxShift))),
                QualityRule.NearDuplicate n => rt.Probe(n).Map(set => Receipt(n, set, QualityReceipt.NoMethod, rt.Clocks.Now)),
                QualityRule.ReferentialEdge e => IO.pure(
                    Receipt(e, rt.Resolve(Stale(LinkStore.StalePins(rt.Links(e), rt.SourceHead))), QualityReceipt.NoMethod, rt.Clocks.Now)),
                QualityRule.Freshness f => IO.pure(
                    Receipt(f, rt.Resolve(new SetExpr.ByRule($"freshness:{f.Table}.{f.TimestampColumn}")), QualityReceipt.NoMethod, rt.Clocks.Now)),
                _ => IO.pure(Receipt(rule, ElementSet.Empty, QualityReceipt.NoMethod, rt.Clocks.Now)),
            };

    // The gate accumulates every `block`-severity violating set applicatively through `Error.Combine` so a batch
    // reports ALL blocking violations, never the first; `warn`/`info` violations ride forward as evidence on the
    // success value (`Query/domain/validation#OUTCOME_PROJECTION`: evidence rides forward, never re-promoted).
    public static Validation<QualityFault, QualityGate> Gate(Seq<QualityReceipt> receipts) =>
        receipts.Filter(static r => r.Severity.Gates && r.Violating > 0)
            .Map(static r => Fail<QualityFault, ElementSet>(new QualityFault.ConstraintRejected(r.Rule.Subject, "gate", $"<gate-blocked:{r.Violating}>")))
            .Traverse(identity).As()
            .Map(_ => new QualityGate(Passed: true, Blocking: ElementSet.Empty,
                Evidence: receipts.Filter(static r => !r.Severity.Gates && r.Violating > 0)));

    // The anomaly/drift passes bind the closed-vocabulary `AnomalyMethod.Scalar` form and the measure identifier
    // (a model fact) into the analytical text; the threshold, quantile, and resolved baseline bind as parameters so
    // no caller input concatenates, and the violating keys stream off the columnar lane through the runtime read
    // delegate; a `DuckDBException` lifts to `QualityFault.PassFaulted` at this one site.
    private static IO<QualityReceipt> AnalyticalPass(QualityRuntime rt, QualityRule rule, AnomalyMethod method, string sql, Seq<(string, object)> binds) {
        var mark = rt.Clocks.Mark();
        return rt.AnalyticalKeys(rt.Analytical, sql, binds)
            .Map(keys => Receipt(rule, rt.Resolve(new SetExpr.Literal(keys)), method, rt.Clocks.Now) with { Elapsed = rt.Clocks.Elapsed(mark) });
    }

    private static QualityReceipt Receipt(QualityRule rule, ElementSet violating, AnomalyMethod method, Instant at) =>
        new(rule, rule.Site(), rule.Severity, violating.Count, violating.Count, violating, method, Duration.Zero, at);

    private static SetExpr Stale(Seq<CrossDocLink> stale) => new SetExpr.Literal(stale.Map(static link => link.From));

    private static Fin<string> Ddl(SchemaDdl.Check check) => Fin<string>.Succ(check.Sql);
    private static Fin<string> Ddl(SchemaDdl.Exclusion exclusion) => Fin<string>.Succ(exclusion.Sql);
    private static Fin<string> Ddl(SchemaDdl.JsonSchemaCheck schema) => Fin<string>.Succ(schema.Sql);
}

public readonly record struct QualityGate(bool Passed, ElementSet Blocking, Seq<QualityReceipt> Evidence);
```

| [INDEX] | [RULE]                | [SITE]                                                  | [MECHANISM]                                                          |
| :-----: | :-------------------- | :----------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | not-null / range / cardinality | `SchemaDdl.Check` `CHECK`                      | model fact via `HasCheckConstraint`, write-time statement error      |
|  [02]   | set-domain            | `SchemaDdl.Check` `IN (...)`                            | allowed-value membership, row-escaped literal list                   |
|  [03]   | regex-shape           | `SchemaDdl.Check` `~`                                   | server-side pattern, expression-GIN serves the hot path              |
|  [04]   | functional-dependency | `SchemaDdl.Exclusion` `= WITH <>`                       | determinant equal forbids dependent unequal                          |
|  [05]   | unique-across         | `SchemaDdl.Index` unique / `NullsDistinct`             | single-null `NULLS NOT DISTINCT` law                                 |
|  [06]   | temporal-overlap      | `SchemaDdl.Exclusion` `EXCLUDE USING gist` (`btree_gist`) | partition `=` plus range `&&` non-overlap                         |
|  [07]   | referential-edge      | foreign key + `CrossDocLink`/`StalePins`               | federated link graph, stale-pin violation                           |
|  [08]   | json-schema-shape     | `SchemaDdl.JsonSchemaCheck` (`pg_jsonschema`)          | server-side, `JsonSchema.Net` in-process fallback                    |
|  [09]   | freshness             | op-log changefeed-fold                                  | staleness bound caught as each `OpLogEntry` commits                  |
|  [10]   | statistical-anomaly / distribution-drift | DuckDB analytical pass                | columnar z-score/IQR/MAD outlier or `quantile_cont` drift           |
|  [11]   | near-duplicate        | `ShortTag` pre-filter + `XxHash128` / `VectorMetric` probe | exact content-key (`DedupMode.Exact`) or fuzzy embedding (`Fuzzy`) |

## [04]-[RESEARCH]

- [JSONB_SCHEMA_VALIDATE]: the `pg_jsonschema` `jsonb_matches_schema` server-side CHECK against a real JSON Schema document and the `JsonSchema.Net` `JsonSchema.Evaluate` in-process degradation when the pgrx extension is absent, confirming the document-shape rule lowers to the settled `SchemaDdl.JsonSchemaCheck` row before the `JsonSchemaShape` fence pins.
- [ANOMALY_PASS_PUSHDOWN]: the DuckDB columnar `AnomalyMethod` passes (z-score `stddev_pop`, IQR/MAD `quantile_cont`/`median` windows over a measure column) and the `quantile_cont` distribution-drift fold against a baseline content key on a live in-process engine — whether each estimator lowers to one columnar window query and the violating keys project as a `SetExpr.Literal`, measured before the pass fences pin.
- [DEDUP_PREFILTER]: the `ContentChunker.Novel(manifest, mayHold, holds)` `ShortTag` 64-bit pre-filter ahead of the authoritative `XxHash128` content-key compare for `DedupMode.Exact` and the `VectorMetric` pgvector distance probe over the `EmbeddingArity` column under a `MaxDistance` threshold for `DedupMode.Fuzzy` — whether the exact path reuses the settled chunker projection and the fuzzy path reuses the catalogued distance functions, measured before the `NearDuplicate` fence pins.
- [CROSS_DOC_REFERENTIAL]: the `CrossDocLink`/`LinkStore.StalePins` cross-document referential-integrity fold — whether a stale-pin reference surfaces as a `ReferentialEdge` violation through the federated link graph rather than a per-document foreign key, confirmed against the federation adjacency before the referential fence pins.

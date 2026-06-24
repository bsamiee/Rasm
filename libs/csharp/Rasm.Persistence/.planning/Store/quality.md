# [PERSISTENCE_QUALITY]

Rasm.Persistence makes declarative data integrity, statistical-anomaly detection, distribution drift, and near-duplicate/de-duplication one closed rule axis that lowers each rule to its least-cost enforcement site rather than re-checking in process. `QualityRule` is the `[Union]` rule family — null-completeness, conditional completeness, completeness-ratio, set-domain membership, single-column range, cross-column consistency, regex/JSON-Schema shape, cross-document uniqueness, temporal-overlap exclusion, referential edge, functional dependency, relationship cardinality, freshness, statistical anomaly, distribution drift, and near-duplicate — and `QualityPlan` is the lowering fold that drives each rule to a `Schema/ddl#EXTENSION_DDL` `SchemaDdl` row (`Check`/`Index`/`Exclusion`/`TemporalKey`/`JsonSchemaCheck`), an op-log changefeed-fold, or a `DuckDB`/`pgvector` analytical pass. Every rule emits the violating rows as a `Query/federation#ELEMENT_SET_ALGEBRA` `ElementSet` so a quality result composes with every other selection through one `SetExpr.Intersect`; `QualityFault` is the closed `[Union]` deriving from `Expected` so a constraint-add failure, a server-side validation rejection, or a provider fault is typed evidence on the unified rail, never silent.

`Severity` is NOT re-minted here — the gating axis is the `Query/domain/validation#OUTCOME_PROJECTION` FluentValidation `Severity` partition (`Error` gates, `Warning`/`Info` ride the gate result forward as evidence) every admission seam in the suite already reads, so a quality verdict and a boundary-validation verdict speak one severity vocabulary and a `block`/`warn`/`info` `[SmartEnum]` beside it is the deleted form. The rule plan rides the federation `CrossDocLink`/`StalePins` adjacency and de-duplicates exact rows by grouping on the `Version/snapshots#COMPRESSION_HASHING` `HashPolicy.Identity` `XxHash128` content-key column; `SetExpr`/`ElementSet`/`SetPredicate`/`CrossDocLink`/`StalePins`/`LinkStore`, `EmbeddingArity`/`VectorMetric`, `HashPolicy.Identity`, `SchemaDdl`/`ColumnInvariant`/`DerivedColumn`, `StoreFact`/`ReceiptSinkPort`, `Severity`, and `ClockPolicy` arrive settled.

Two altitude seams keep this axis disjoint from the BIM-semantic rule engine and from the boundary validator. `Query/federation#RULE_PLAN` owns the buildingSMART IDS 1.0 / MVD / clash / QTO grammar — its `IdsImport.Parse` lowers an IDS facet to a `SetExpr` and emits a typed `RuleViolation`, so an IFC/IDS semantic facet is `RuleAst`, never a `QualityRule`. This axis owns the orthogonal concern: declarative DATA-INTEGRITY, STATISTICAL, and DE-DUPLICATION rules that lower to DB constraints and columnar passes. `Rasm.Bim/Model` and `Rasm.Compute` federate at the seam only the rules that are data-integrity or statistical in kind (a per-element completeness invariant, a geometry-derived measure anomaly), each as a `QualityRule` row folded into this one axis — never a parallel checker, and never the IDS grammar `RULE_PLAN` already owns.

Wire posture: this page is host-local — quality rules lower to server-side constraints, server-side predicates, or in-process analytical passes, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster.

## [01]-[INDEX]

- [01]-[QUALITY_RULE]: the rule union, the enforcement-site axis, the outlier-estimator and de-duplication-mode vocabularies, the typed receipt, the page-wide fact stream, and the closed fault family.
- [02]-[QUALITY_PLAN]: the lowering fold to the cheapest site (`SchemaDdl` rows, server predicates, changefeed-folds, analytical passes), the violating-set projection, and the gate fold.

## [02]-[QUALITY_RULE]

- Owner: `QualityRule` the `[Union]` rule family; `EnforcementSite` the `[SmartEnum<string>]` least-cost-site axis carrying its constraint-versus-pass `Lowered` predicate; `AnomalyMethod` the `[SmartEnum<string>]` outlier-estimator axis carrying its DuckDB scalar form (with the `None` non-estimator row so a non-anomaly receipt never claims a fictitious estimator); `DedupMode` the `[SmartEnum<string>]` exact-content-key-versus-fuzzy-embedding axis; `QualityReceipt` the typed rule-evaluation evidence; `QualityFact` with `QualityFactKind` the page-wide fact stream feeding `StoreFact`; `QualityFault` the closed `[Union]` deriving from `Expected, IValidationError<QualityFault>, Semigroup<QualityFault>` in the 8290 band with the per-case `IValidationError<TCase>`/`Create` dual-tier contract every suite fault carries, the `Semigroup` `Combine` folding accumulated blocks into one `Aggregate` case so `Validation<QualityFault, _>` does not throw `TypeLoadException`.
- Cases: `NotNullSet` (a column set that must be complete), `ConditionalNotNull` (a column required only when a guard predicate holds — a partial-completeness invariant), `CompletenessRatio` (a non-null fraction at or above a threshold over a measure, a statistical completeness gate beyond the row-level not-null floor), `SetDomain` (a column constrained to an allowed-value set), `RangeBound` (a numeric/temporal range over one column), `Consistency` (a cross-column relational invariant — a free server-side boolean over a column tuple, `end_date >= start_date` / `discount <= price`, the canonical multi-column data-integrity CHECK no single-column range or determinant→dependent rule can state), `RegexShape` (a string pattern), `JsonSchemaShape` (a JSON Schema over a jsonb column), `UniqueAcross` (a cross-document uniqueness over a column tuple), `TemporalOverlap` (a no-overlap exclusion over a validity range), `ReferentialEdge` (a `CrossDocLink`-backed foreign reference), `FunctionalDependency` (a determinant→dependent column functional dependency), `Cardinality` (a relationship-multiplicity bound on the per-determinant dependent count, not a column-value range), `Freshness` (a staleness bound on a timestamp column), `StatisticalAnomaly` (a columnar outlier over a measure, optionally per partition, under an `AnomalyMethod`), `DistributionDrift` (a quantile drift over a measure against a baseline content key), `NearDuplicate` (an exact content-key match on a `ContentKeyColumn` or a fuzzy-embedding near-match on an `EmbeddingColumn` under a `DedupMode`) on `QualityRule`; every rule carries a `Severity` discriminant the gate fold reads. `EnforcementSite` is `check | set-domain | exclude | temporal-exclude | foreign-key | json-schema | changefeed-fold | duckdb-pass | vector-probe`, its `Lowered` column splitting the constraint-class sites (folded into the migration) from the pass-class sites (run over the runtime).
- Entry: `public EnforcementSite Site()` projects the least-cost site for a rule through one generated `Switch`; `public Severity Severity` and `public string Subject` are the two other generated-`Switch` projections the fact stream and gate read. The violating-row set is NOT a rule method — a constraint-class rule enforces inline so its violating set is the stable empty `SetExpr.Literal`, while a pass-class rule's violating set is only knowable through the effectful columnar/probe pass `QualityPlan.Evaluate` runs and resolves into an `ElementSet`, so the lowering and the resolution live on `QualityPlan`, never a per-rule `Violating()` the rule could only answer trivially. One projection per modality discriminates on the case, never a parallel `SiteOf`/`SeverityOf` family.
- Auto: each rule lowers to the cheapest site that can enforce it. `NotNullSet`/`ConditionalNotNull`/`SetDomain`/`RangeBound`/`Consistency`/`RegexShape`/`FunctionalDependency` lower to a `SchemaDdl.Check` `CHECK` constraint (a model fact emitted through `HasCheckConstraint`) — `Consistency` is the free cross-column relational predicate (`{a} <= {b}`) over a column tuple, the multi-column invariant a single-column `RangeBound` cannot state; `UniqueAcross` to a `SchemaDdl.Index` with `NullsDistinct`; `TemporalOverlap` to a `SchemaDdl.Exclusion` `EXCLUDE USING gist` over `btree_gist` (or a `SchemaDdl.TemporalKey` WITHOUT OVERLAPS when the overlap protects a primary/unique key); `ReferentialEdge` is a PASS-class cross-document rule the `LinkStore.StalePins` link-graph fold resolves (a server FK or a `CHECK` subquery cannot span documents — Postgres forbids subqueries in `CHECK`), reading the `CrossDocLink` adjacency and the `StalePins` projection; `JsonSchemaShape` to a `SchemaDdl.JsonSchemaCheck` `CHECK (jsonb_matches_schema(...))` validated server-side by `pg_jsonschema` when present, else (the `extensionAvailable` probe `Schema/ddl#EXTENSION_DDL` `SchemaDdl.Validate` reads reporting it absent) a typed `FallbackEvaluated` receipt carrying the `Json.Schema.JsonSchema.Evaluate` in-process degradation, never a silent drop; `Cardinality`/`CompletenessRatio`/`StatisticalAnomaly`/`DistributionDrift` to a DuckDB analytical pass over the columnar lane; `Freshness` to an op-log changefeed-fold caught as each `OpLogEntry` commits; and `NearDuplicate` to a DuckDB `GROUP BY ContentKeyColumn HAVING count(*) > 1` duplicate-group pass over the `ContentKeyColumn` (the materialized `Version/snapshots#COMPRESSION_HASHING` `HashPolicy.Identity` `XxHash128` content address — grouping on the identity column is the complete in-column duplicate test, distinct from the snapshot chunker's cross-set `ShortTag` membership pre-filter, which answers which chunks of one payload a peer lacks) for `DedupMode.Exact`, or a `VectorMetric` pgvector distance probe over the `EmbeddingArity` column for `DedupMode.Fuzzy`. Every rule emits its violating rows as an `ElementSet` so a quality gate composes with any other selection (a clash result intersected with a quality violation is one `SetExpr.Intersect`).
- Receipt: a rule evaluation rides `QualityReceipt(QualityRule Rule, EnforcementSite Site, Severity Severity, long Checked, ElementSet Violating, Option<AnomalyMethod> Method, Duration Elapsed, Instant At)` — the typed evidence carrying the rule, its site, its severity, the real scanned cardinality, the violating `ElementSet` (its own `Count` the violation total, never a duplicated field), and the estimator the pass used when one applies; `QualityFact.Of` projects each receipt onto the `StoreFact` stream under the `store.quality.*` kinds so a constraint add, a violation, a fallback evaluation, and a drift detection ride the one receipt-sink envelope; a constraint-add failure or a server-side validation rejection is a typed `QualityFault`, never silent.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, DuckDB.NET.Data.Full, Pgvector.EntityFrameworkCore, Pgvector, FluentValidation, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime. `SchemaDdl`/`ColumnInvariant`, `SetExpr`/`ElementSet`/`ElementSetAlgebra`, `CrossDocLink`/`StalePins`/`LinkStore`, `HashPolicy.Identity` (the materialized `XxHash128` content-key column the `Exact` pass groups on — no C# hash is computed here), `EmbeddingArity`/`VectorMetric`, `Severity`, and `StoreFact`/`ReceiptSinkPort` arrive as settled vocabulary; `pg_jsonschema`'s `jsonb_matches_schema` and its `Json.Schema.JsonSchema.Evaluate` fallback are `Schema/ddl` capability composed at the lowering seam, not re-minted here.
- Growth: a new rule kind is one `QualityRule` case plus one arm on each of the three generated projections (`Site`/`Severity`/`Subject`) and one `LowerOne`/`Evaluate` arm by site class; a new enforcement site is one `EnforcementSite` row carrying its `Lowered` stance; a new outlier estimator is one `AnomalyMethod` row carrying its DuckDB scalar form; a new de-duplication stance is one `DedupMode` row; zero new surface — a per-rule-type validator, an imperative in-process re-check, a second integrity engine, or a parallel anomaly detector is the deleted form because one union lowers to the cheapest site and emits an `ElementSet`.
- Boundary: every rule lowers to the least-cost enforcement site — an imperative in-process re-check over a materialized scan is the deleted form; the constraint-class rules (`NotNullSet`/`ConditionalNotNull`/`SetDomain`/`RangeBound`/`Consistency`/`RegexShape`/`FunctionalDependency`/`UniqueAcross`/`TemporalOverlap`) lower to `Schema/ddl` `SchemaDdl` rows so they are model facts emitted through `HasCheckConstraint`/`HasIndex`/the migration fold, never deploy-time hand DDL beside the migration set — `Lower` constructs the typed `SchemaDdl` row that owns the `ALTER TABLE … CHECK/EXCLUDE` envelope and the table/constraint identifier seam through its own `Sql` projection, never a parallel `ALTER TABLE` emitter; the CHECK PREDICATE body is migration-artifact DDL (`Query/domain/postgres#SQL_LAW`: DDL lives in migration artifacts, identifiers parameterize in neither lane), so `LowerOne` assembles the predicate from model-fact identifiers and single-quote-doubles every embedded value literal at the lowering site — the `SetDomain` allowed-value list and the `RegexShape` pattern escape, the `RangeBound` bound is a closed model fact not caller free-text, so `UniqueAcross` becomes a `SchemaDdl.Index` row not a hand-spelled `CREATE UNIQUE INDEX`, and `TemporalOverlap`/`FunctionalDependency` become `SchemaDdl.Exclusion` rows; `JsonSchemaShape` lowers to a `SchemaDdl.JsonSchemaCheck` row whose `Sql` rides `pg_jsonschema` server-side and whose extension-absent fallback is `SchemaDdl.Validate`'s `Json.Schema.JsonSchema.Evaluate` degradation surfaced as `QualityFact.FallbackEvaluated`; `Cardinality` is the per-determinant dependent-count GROUP BY bound the DuckDB pass enforces (a `{Column} BETWEEN {Min} AND {Max}` row-value `CHECK` is the deleted illusion — relationship multiplicity is a count over a partition, not a per-row value bound), `CompletenessRatio` the non-null-fraction-over-threshold pass, and `StatisticalAnomaly`/`DistributionDrift` ride the DuckDB analytical lane (`Query/lanes#ANALYTICAL_LANE`) so an outlier or a drift detection is a columnar query under the `AnomalyMethod` estimator, never a row-by-row fold; `NearDuplicate` `DedupMode.Exact` is a DuckDB `GROUP BY ContentKeyColumn HAVING count(*) > 1` duplicate-group pass over the `ContentKeyColumn` (the materialized `Version/snapshots#COMPRESSION_HASHING` `HashPolicy.Identity` `XxHash128` content address — the identity-column group-by is the complete in-column duplicate test; the `HashPolicy.Content` `XxHash3` `ShortTag` is the snapshot chunker's cross-set membership pre-filter, a different question with no column here) while `DedupMode.Fuzzy` is the `VectorMetric` pgvector probe over its `EmbeddingColumn`, never a quadratic pairwise scan; the violating-row projection is an `ElementSet` so a quality result is content-addressed and composes with the element-set algebra; the `ReferentialEdge` reads the `Query/federation#CROSS_DOC_LINKS` adjacency and the `StalePins` projection so a cross-document referential violation is caught as a stale pin, never a per-document foreign key; the `Severity` axis is the validation-shard partition (`Query/domain/validation#OUTCOME_PROJECTION`) — `Severity.Error` gates the quality gate, `Severity.Warning`/`Severity.Info` ride the gate result forward as evidence and never re-promote to a gating fault; the data-integrity and statistical rules `Rasm.Bim/Model` and `Rasm.Compute` author federate into this one rule axis as `QualityRule` rows at the seam, while their IDS/MVD/clash facets stay `Query/federation#RULE_PLAN` `RuleAst` — so a Bim or Compute domain rule federates without a parallel checker and without duplicating the IDS grammar.

```csharp signature
using FluentValidation;          // Severity
using DuckDB.NET.Native;         // DuckDBErrorType

// The least-cost site a rule lowers to; `Lowered` splits the write-time constraint sites (folded into
// the migration) from the runtime pass sites so `QualityPlan.Lower` and `Evaluate` partition off one row,
// never a duplicated string-match in two places.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class EnforcementSite {
    public static readonly EnforcementSite Check = new("check", lowered: true);
    public static readonly EnforcementSite SetDomain = new("set-domain", lowered: true);
    public static readonly EnforcementSite Exclude = new("exclude", lowered: true);
    public static readonly EnforcementSite TemporalExclude = new("temporal-exclude", lowered: true);
    // A CROSS-DOCUMENT referential edge has no server foreign key (Postgres FKs and CHECK subqueries cannot
    // span documents/tables this way), so its site is a PASS class: `Evaluate` resolves the violation through
    // the `Query/federation#CROSS_DOC_LINKS` `LinkStore.StalePins` link-graph fold, never a lowered constraint.
    public static readonly EnforcementSite ForeignKey = new("foreign-key", lowered: false);
    public static readonly EnforcementSite JsonSchema = new("json-schema", lowered: true);
    public static readonly EnforcementSite ChangefeedFold = new("changefeed-fold", lowered: false);
    public static readonly EnforcementSite DuckDbPass = new("duckdb-pass", lowered: false);
    public static readonly EnforcementSite VectorProbe = new("vector-probe", lowered: false);

    // A constraint-class site enforces at write time; a pass-class site runs over the `QualityRuntime`.
    public bool Lowered { get; }
    private EnforcementSite(string key, bool lowered) : this(key) => Lowered = lowered;
}

// The estimator the DuckDB anomaly pass lowers to — `Scalar` is the per-row deviation expression keyed on
// the measure column `{0}` and the window frame `{1}`, a single-pass window aggregate evaluated server-side
// over the columnar lane (DuckDB forbids nested window functions, so each form computes its center and scale
// in ONE window pass — a true median-of-absolute-deviations needs a two-pass CTE and is a research-gated
// refinement, never the illegal nested `median(... median() OVER w) OVER w`). `{1}` binds the empty whole-table
// frame (`OVER ()`) or the `OVER (PARTITION BY col, …)` frame the rule's `PartitionBy` names, so a per-partition
// outlier is a real partitioned window. `z-score` is the parametric mean-centered standard-deviation form, `iqr`
// the median-centered IQR-scaled robust form for skewed measures, `mad` the median-centered `0.6745`-scaled
// modified-z robust form, and `None` the non-estimator row a non-anomaly receipt carries so `Method` never
// claims a fictitious form. `Frame(partitionBy)` renders the window clause off the rule axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class AnomalyMethod {
    public static readonly AnomalyMethod ZScore = new("z-score",
        scalar: "abs({0} - avg({0}) OVER ({1})) / nullif(stddev_pop({0}) OVER ({1}), 0)");
    public static readonly AnomalyMethod Iqr = new("iqr",
        scalar: "abs({0} - median({0}) OVER ({1})) / nullif(quantile_cont({0}, 0.75) OVER ({1}) - quantile_cont({0}, 0.25) OVER ({1}), 0)");
    public static readonly AnomalyMethod Mad = new("mad",
        scalar: "0.6745 * abs({0} - median({0}) OVER ({1})) / nullif(stddev_pop({0}) OVER ({1}), 0)");
    public static readonly AnomalyMethod None = new("none", scalar: "");

    public string Scalar { get; }
    private AnomalyMethod(string key, string scalar) : this(key) => Scalar = scalar;

    // The window frame off the rule's partition columns (closed-vocabulary model-fact identifiers, never
    // caller input): empty columns render the whole-table frame, a named set the partitioned frame.
    public static string Frame(Seq<string> partitionBy) =>
        partitionBy.IsEmpty ? "" : $"PARTITION BY {string.Join(", ", partitionBy)}";
}

// `Exact` proves duplication through a `GROUP BY ContentKeyColumn HAVING count(*) > 1` collision over the
// `HashPolicy.Identity` `XxHash128` content-key column — grouping on the identity column is the complete
// in-column duplicate test (the chunker's `XxHash3` `ShortTag` is a cross-set membership pre-filter, not a
// column here); `Fuzzy` proves near-match through a `VectorMetric` pgvector distance under a threshold over an
// embedding column — each mode reads its own column (`Subject` projects the active one), the other riding as the
// inactive discriminant payload.
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

// Closed fault family deriving from `Expected` for the 8290 band, `IValidationError<QualityFault>` for the
// generated `[ValidationError]` admission, AND `Semigroup<QualityFault>` because LanguageExt v5 resolves the
// `Validation<F,_>` combination trait ad-hoc at run time and throws `TypeLoadException` unless the CONCRETE `F`
// derives from `Semigroup<F>` (`Error`'s own `Monoid<Error>` does not satisfy `Semigroup<QualityFault>` — the
// trait is on the concrete type, the same law the `Store/tenancy#TENANCY_RLS` `TenancyFault` carries). So
// the gate's per-rule blocking pass accumulates every `GateBlocked` under one `Aggregate` case rather than the
// first, and a per-case fault still keys on its own type for code-keyed recovery (`Is`/`HasCode`/`IsType<E>`,
// never `==`). The 8290 band locates the owner without a lookup.
[Union]
public abstract partial record QualityFault : Expected, IValidationError<QualityFault>, Semigroup<QualityFault> {
    private QualityFault(string detail, int code) : base(detail, code, None) { }

    public static QualityFault Create(string message) => new EvaluationFailed(message);

    public sealed record ConstraintRejected(string Table, string Constraint, string Detail)
        : QualityFault($"<quality-constraint-rejected:{Table}.{Constraint}>", 8290), IValidationError<ConstraintRejected> {
        public static ConstraintRejected Create(string detail) => new("", "", detail);
    }
    public sealed record SchemaInvalid(string Column, string Detail)
        : QualityFault($"<quality-schema-invalid:{Column}>", 8291), IValidationError<SchemaInvalid> {
        public static SchemaInvalid Create(string detail) => new("", detail);
    }
    public sealed record ExtensionUnavailable(string Extension, string Fallback)
        : QualityFault($"<quality-extension-unavailable:{Extension}->{Fallback}>", 8292), IValidationError<ExtensionUnavailable> {
        public static ExtensionUnavailable Create(string detail) => new(detail, "");
    }
    // A columnar pass fault carries DuckDB's native `DuckDBErrorType` class verbatim — symmetric with
    // `Query/rail#OPERATION_ALGEBRA` `StoreFault.Analytical`, so a Catalog/Conversion/Serialization rejection on
    // the analytical lane stays typed evidence (`Kind`) and its transient subset derives (`IsRetryable`) rather
    // than collapsing to an opaque string; `Site` records the rule's actual pass site for the receipt.
    public sealed record PassFaulted(EnforcementSite Site, DuckDBErrorType Kind, string Detail)
        : QualityFault($"<quality-pass-faulted:{Site.Key}:{Kind}>", 8293), IValidationError<PassFaulted> {
        public bool IsRetryable => Kind is DuckDBErrorType.Transaction or DuckDBErrorType.Interrupt;
        public static PassFaulted Of(EnforcementSite site, DuckDBException ex) => new(site, ex.ErrorType, ex.Message);
        public static PassFaulted Create(string detail) => new(EnforcementSite.DuckDbPass, DuckDBErrorType.Invalid, detail);
    }
    public sealed record NotAConstraint(string Subject)
        : QualityFault($"<quality-not-a-constraint:{Subject}>", 8294), IValidationError<NotAConstraint> {
        public static NotAConstraint Create(string detail) => new(detail);
    }
    public sealed record EvaluationFailed(string Detail)
        : QualityFault($"<quality-evaluation-failed>:{Detail}", 8295), IValidationError<EvaluationFailed> {
        public static EvaluationFailed Create(string detail) => new(detail);
    }
    // A `Severity.Error` violating set blocks the gate — `Set` is the content-addressed blocking-set receipt
    // the remediation selection re-resolves, `Count` the violation total; one per blocking receipt accumulates
    // under `Aggregate` so the rail surfaces every blocked rule, not the first.
    public sealed record GateBlocked(string Subject, UInt128 Set, int Count)
        : QualityFault($"<quality-gate-blocked:{Subject}:{Count}>", 8296), IValidationError<GateBlocked> {
        public static GateBlocked Create(string detail) => new(detail, default, 0);
    }
    // The accumulation carrier — `Validation<QualityFault, _>.Apply` folds gate blocks (and any applicative
    // batch admission) into one `Aggregate` rather than flattening to a string, so the rail reports the full
    // blocked roster with each typed cause intact.
    public sealed record Aggregate(Seq<QualityFault> Faults)
        : QualityFault($"{Faults.Count} quality faults", 8299), IValidationError<Aggregate> {
        public static Aggregate Create(string detail) => new(Seq<QualityFault>(new EvaluationFailed(detail)));
    }

    public QualityFault Combine(QualityFault rhs) => (this, rhs) switch {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _) => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r) => new Aggregate(this.Cons(r.Faults)),
        _ => new Aggregate(Seq(this, rhs)),
    };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record QualityRule {
    private QualityRule() { }

    public sealed record NotNullSet(string Table, Seq<string> Columns, Severity Severity) : QualityRule;
    public sealed record ConditionalNotNull(string Table, Seq<string> Columns, string When, Severity Severity) : QualityRule;
    public sealed record CompletenessRatio(string Table, string Column, double MinFraction, Severity Severity) : QualityRule;
    public sealed record SetDomain(string Table, string Column, Seq<string> Allowed, Severity Severity) : QualityRule;
    public sealed record RangeBound(string Table, string Column, string Low, string High, Severity Severity) : QualityRule;
    public sealed record Consistency(string Table, Seq<string> Columns, string Predicate, Severity Severity) : QualityRule;
    public sealed record RegexShape(string Table, string Column, string Pattern, Severity Severity) : QualityRule;
    public sealed record JsonSchemaShape(string Table, string Column, string Schema, Severity Severity) : QualityRule;
    public sealed record UniqueAcross(string Table, Seq<string> Columns, bool NullsDistinct, Severity Severity) : QualityRule;
    public sealed record TemporalOverlap(string Table, string Period, Seq<string> PartitionBy, Severity Severity) : QualityRule;
    public sealed record ReferentialEdge(string Table, string Column, string ReferencedTable, string ReferencedColumn, Severity Severity) : QualityRule;
    public sealed record FunctionalDependency(string Table, Seq<string> Determinant, Seq<string> Dependent, Severity Severity) : QualityRule;
    public sealed record Cardinality(string Table, Seq<string> Determinant, int Min, Option<int> Max, Severity Severity) : QualityRule;
    public sealed record Freshness(string Table, string TimestampColumn, Duration MaxAge, Severity Severity) : QualityRule;
    public sealed record StatisticalAnomaly(string Table, string Measure, double Sigma, AnomalyMethod Method, Seq<string> PartitionBy, Severity Severity) : QualityRule;
    public sealed record DistributionDrift(string Table, string Measure, double Quantile, double MaxShift, UInt128 Baseline, Severity Severity) : QualityRule;
    public sealed record NearDuplicate(string Table, DedupMode Mode, string ContentKeyColumn, string EmbeddingColumn, VectorMetric Metric, double MaxDistance, Severity Severity) : QualityRule;

    public Severity Severity =>
        Switch(
            notNullSet:           static r => r.Severity,
            conditionalNotNull:   static r => r.Severity,
            completenessRatio:    static r => r.Severity,
            setDomain:            static r => r.Severity,
            rangeBound:           static r => r.Severity,
            consistency:          static r => r.Severity,
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
            conditionalNotNull:   static r => $"{r.Table}:{string.Join(',', r.Columns)}?",
            completenessRatio:    static r => $"{r.Table}.{r.Column}~{r.MinFraction.ToString(CultureInfo.InvariantCulture)}",
            setDomain:            static r => $"{r.Table}.{r.Column}",
            rangeBound:           static r => $"{r.Table}.{r.Column}",
            consistency:          static r => $"{r.Table}:{string.Join(',', r.Columns)}",
            regexShape:           static r => $"{r.Table}.{r.Column}",
            jsonSchemaShape:      static r => $"{r.Table}.{r.Column}",
            uniqueAcross:         static r => $"{r.Table}:{string.Join(',', r.Columns)}",
            temporalOverlap:      static r => $"{r.Table}.{r.Period}",
            referentialEdge:      static r => $"{r.Table}.{r.Column}->{r.ReferencedTable}.{r.ReferencedColumn}",
            functionalDependency: static r => $"{r.Table}:{string.Join(',', r.Determinant)}->{string.Join(',', r.Dependent)}",
            cardinality:          static r => $"{r.Table}:{string.Join(',', r.Determinant)}[{r.Min}..]",
            freshness:            static r => $"{r.Table}.{r.TimestampColumn}",
            statisticalAnomaly:   static r => $"{r.Table}.{r.Measure}",
            distributionDrift:    static r => $"{r.Table}.{r.Measure}",
            nearDuplicate:        static r => r.Mode == DedupMode.Exact ? $"{r.Table}.{r.ContentKeyColumn}" : $"{r.Table}.{r.EmbeddingColumn}");

    public EnforcementSite Site() =>
        Switch(
            notNullSet:           static _ => EnforcementSite.Check,
            conditionalNotNull:   static _ => EnforcementSite.Check,
            completenessRatio:    static _ => EnforcementSite.DuckDbPass,
            setDomain:            static _ => EnforcementSite.SetDomain,
            rangeBound:           static _ => EnforcementSite.Check,
            consistency:          static _ => EnforcementSite.Check,
            regexShape:           static _ => EnforcementSite.Check,
            functionalDependency: static _ => EnforcementSite.Exclude,
            cardinality:          static _ => EnforcementSite.DuckDbPass,
            freshness:            static _ => EnforcementSite.ChangefeedFold,
            jsonSchemaShape:      static _ => EnforcementSite.JsonSchema,
            uniqueAcross:         static _ => EnforcementSite.Exclude,
            temporalOverlap:      static _ => EnforcementSite.TemporalExclude,
            referentialEdge:      static _ => EnforcementSite.ForeignKey,
            statisticalAnomaly:   static _ => EnforcementSite.DuckDbPass,
            distributionDrift:    static _ => EnforcementSite.DuckDbPass,
            nearDuplicate:        static r => r.Mode == DedupMode.Exact ? EnforcementSite.DuckDbPass : EnforcementSite.VectorProbe);
}

// `Violating.Count` IS the violation total — no parallel count field; `Method` is `Some` only when an
// anomaly estimator ran; `Checked` is the real scanned cardinality the pass reports, not a copy of the
// violation count (a constraint-class receipt carries `Checked: 0` because the constraint enforces inline).
public sealed record QualityReceipt(
    QualityRule Rule, EnforcementSite Site, Severity Severity, long Checked, ElementSet Violating,
    Option<AnomalyMethod> Method, Duration Elapsed, Instant At);

public readonly record struct QualityFact(QualityFactKind Kind, QualityReceipt Receipt) {
    // The fact stream is the settled `Query/rail#INTERCEPTOR_SPINE` `StoreFact` envelope — a quality fact is one
    // `store.quality.*` row, never a parallel signal owner; `checked` carries the scanned cardinality, `constraint-
    // added`/`fallback` carry 1 per lowered constraint or degraded rule, and every violation-bearing kind
    // (`violated`/`gated`/`drift`) carries the violation total off `Violating.Count` — the `Count` projection is a
    // closed fold over the kind, never a per-call-site count knob.
    public StoreFact Project() =>
        new(Kind.Key, Receipt.Rule.Subject, Count, Receipt.Elapsed, Receipt.At);

    private long Count =>
        Kind == QualityFactKind.Checked ? Receipt.Checked
        : Kind == QualityFactKind.ConstraintAdded || Kind == QualityFactKind.FallbackEvaluated ? 1L
        : Receipt.Violating.Count;

    public static QualityFact Of(QualityFactKind kind, QualityReceipt receipt) => new(kind, receipt);
}
```

## [03]-[QUALITY_PLAN]

- Owner: `QualityPlan` the static surface lowering a `Seq<QualityRule>` into its enforcement artifacts — the `SchemaDdl` constraint-row set folded into the migration, the changefeed-fold registrations, and the DuckDB/pgvector analytical passes — folding the violating `ElementSet` per rule, and reducing a rule batch's per-rule results into one `QualityGate` verdict; `QualityRuntime` the capability the effectful passes read; `QualityGate` the verdict carrying the union of blocking sets and the riding evidence.
- Entry: `public static Fin<(MigrationBuilder Migration, Seq<SchemaDdl.Index> Indexes, Seq<QualityReceipt> Fallbacks)> Lower(MigrationBuilder migration, Seq<QualityRule> rules, Func<string, bool> extensionAvailable, ClockPolicy clocks)` folds the constraint-class rules into `Schema/ddl#EXTENSION_DDL` `SchemaDdl` rows (`Check`/`Exclusion`/`JsonSchemaCheck` emitting raw `Sql` through the migration, `Index` rows handed back for the model-build `SchemaDdl.Configure(entity, indexes)` fold) — reusing each row's own `Sql`/`Configure` projection so the DDL owner owns the identifier seam — so the constraints enforce at write time, while a `JsonSchemaShape` whose `pg_jsonschema` the `extensionAvailable` probe reports absent emits no server `CHECK` and rides back as a `FallbackEvaluated` receipt (the in-process `Json.Schema.JsonSchema.Evaluate` degradation), never a silent drop; `public static IO<QualityReceipt> Evaluate(QualityRuntime rt, QualityRule rule)` composes a pass-class rule (completeness-ratio, cardinality, anomaly, drift, near-duplicate, referential, freshness) over the `QualityRuntime` substrate and folds the violating set, converting a `DuckDBException` to `QualityFault.PassFaulted` (carrying the native `DuckDBErrorType`) at the columnar read's one `Bracket(Catch:)` edge; `public static QualityGate Gate(Seq<QualityReceipt> receipts)` folds the receipt batch into the one total `QualityGate.Of` verdict — `Blocking` the union of every `Severity.Error` violating set in BOTH outcomes, `Passed` derived from its emptiness, `Evidence` riding the `Warning`/`Info` violations forward — whose `Rail` projection accumulates one `GateBlocked` per blocking receipt onto `Validation<QualityFault, QualityGate>` so a save gate binds the rail while a dashboard reads the bare verdict. One polymorphic entrypoint per modality (lower constraints, evaluate a pass, fold a gate); a `LowerCheck`/`LowerIndex`/`EvaluateAnomaly` family and a second hand-rolled severity partition at the plan site are the deleted forms.
- Auto: the lowering fold partitions the rules by `Site().Lowered` — the constraint-class rules become `SchemaDdl.Check`/`Index`/`Exclusion`/`JsonSchemaCheck` rows folded into the migration so a violating write is a statement error (never an after-the-fact scan), and the pass-class rules register as op-log changefeed folds or scheduled analytical passes so a streaming or batch violation is caught past the constraint lane; the violating rows of every rule are an `ElementSet` lowered through the federation algebra so a quality dashboard, a remediation selection, and a downstream gate all read the one content-addressed set; a `ReferentialEdge` cross-document rule reads the `Query/federation#CROSS_DOC_LINKS` `LinkStore.StalePins` projection so a stale-pin reference is a quality violation; the `StatisticalAnomaly` pass binds the `AnomalyMethod.Scalar` form into the analytical-lane window query so an outlier over a measure column (optionally per `PartitionBy`) is one columnar pass; the `CompletenessRatio` pass folds a non-null fraction against `MinFraction`; the `Cardinality` pass folds a per-`Determinant` `GROUP BY count(*)` against the `[Min, Max]` multiplicity bound; the `DistributionDrift` pass folds a `quantile_cont` shift against the `Baseline` content key as one table-level verdict (a single drift key when the shift breaches `MaxShift`, none otherwise — never a per-key window predicate that yields all rows or none); the `NearDuplicate.Exact` probe is a DuckDB columnar `GROUP BY ContentKeyColumn HAVING count(*) > 1` duplicate-group pass over the `ContentKeyColumn` materializing the authoritative `HashPolicy.Identity` `XxHash128` content address, so the violating rows are every member of a content-key group of size > 1 — grouping on the identity column is the complete in-column duplicate test, distinct from the snapshot chunker's `ShortTag` cross-set membership pre-filter (`ContentChunker.Novel`, which answers which chunks of one payload a peer lacks, not which rows of a column collide); `NearDuplicate.Fuzzy` projects the `VectorMetric` distance ordering over the `EmbeddingArity` column through the catalogued `CosineDistance`/`L2Distance`/`MaxInnerProduct` function under the `MaxDistance` threshold; the gate fold accumulates `Severity.Error` failures, unions their violating sets into `QualityGate.Blocking`, and surfaces `Warning`/`Info` as evidence.
- Receipt: a constraint lowering rides `QualityFactKind.ConstraintAdded`; a rule evaluation rides `QualityFactKind.Checked`/`Violated` carrying the scanned and violation counts; a drift detection rides `QualityFactKind.Drifted`; a server-side-to-fallback degradation rides `QualityFactKind.FallbackEvaluated`; the gate verdict rides `QualityFactKind.Gated`; the typed `QualityReceipt` carries the violating `ElementSet` and projects onto `StoreFact`; a constraint-add, schema-invalid, extension-unavailable, not-a-constraint, pass, or gate-blocked fault is a typed `QualityFault`.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, DuckDB.NET.Data.Full, Pgvector.EntityFrameworkCore, Pgvector, FluentValidation, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new lowering target is one arm on `Lower` (constructing one `SchemaDdl` row); a new pass kind is one arm on `Evaluate`; a new gate disposition is one field on the total `QualityGate` verdict or one arm on its `Rail` projection; zero new surface — a second lowering fold, a parallel constraint emitter, a hand-written constraint DDL string, a per-rule evaluation service, or a second severity partition beside the `QualityGate` verdict is the deleted form because the plan lowers the one rule union into the settled `SchemaDdl` rows and the federation `SetExpr`.
- Boundary: the plan lowers the rule union into the settled `Schema/ddl` `SchemaDdl` constraint rows and the federation `SetExpr` — a parallel `ALTER TABLE`/`CREATE UNIQUE INDEX` envelope at the lowering site or a parallel selection shape is the deleted form because the `SchemaDdl` row owns the `ALTER TABLE … CHECK/EXCLUDE` envelope and the table/constraint identifier seam through its own `Sql` projection while the migration fold owns the emission; the CHECK predicate body is migration-artifact DDL (`Query/domain/postgres#SQL_LAW`: DDL lives in migration artifacts), assembled from model-fact identifiers with every embedded value literal single-quote-doubled at the lowering site; `Lower` routes `UniqueAcross` through `SchemaDdl.Index.Configure(entity)` (the model-translatable index path, never a raw `CREATE INDEX` string), `TemporalOverlap`/`FunctionalDependency` through `SchemaDdl.Exclusion`, and `JsonSchemaShape` through `SchemaDdl.JsonSchemaCheck`, so every constraint traces to a DDL-owner row; the constraint-class rules enforce at write time so a violating write is a statement error, and only the rules a constraint cannot express (completeness-ratio, cardinality, anomaly, drift, near-duplicate, cross-document referential, freshness) ride a changefeed-fold or a scheduled pass; the violating-set projection is the one `ElementSet` currency so a quality result composes with the rule engine and the element-set algebra, never a per-rule result shape; the cross-document referential rule reads the `CrossDocLink`/`StalePins` projection so it rides the federated link graph, never a per-document foreign key; the DuckDB anomaly/cardinality/completeness/drift passes bind the closed-vocabulary `AnomalyMethod.Scalar` form and the measure/determinant identifiers (model facts from the entity model) through the `Query/lanes#ANALYTICAL_LANE` analytical seam — the same read-only analytical lane that owns DuckDB reads and never source-of-truth writes — and the threshold/quantile/baseline bind as parameters so no caller input concatenates into SQL; the pgvector probe reuses the `Query/lanes#SEARCH_LANES` `VectorMetric` distance functions (never a hand-spelled distance operator) and the `Exact` duplicate-group pass groups on the `ContentKeyColumn` materializing the `Version/snapshots#COMPRESSION_HASHING` `HashPolicy.Identity` `XxHash128` content address (the identity-column group-by is the complete in-column duplicate test; the `HashPolicy.Content` `XxHash3` `ShortTag` is the snapshot chunker's `ContentChunker.Novel` cross-set membership pre-filter, a different question with no column on this pass); the gate fold rides the validation-shard severity partition (`Query/domain/validation#OUTCOME_PROJECTION`) so a `Severity.Error` failure gates with its violating set unioned into `Blocking` and a `Warning`/`Info` advisory rides forward; the data-integrity and statistical rules `Rasm.Bim/Model` and `Rasm.Compute` author lower through this one plan so a Bim or Compute domain rule is a `QualityRule` row, never a parallel checker, while their IDS/clash/QTO facets stay `Query/federation#RULE_PLAN`.

```csharp signature
using System.Numerics;           // BigInteger ↔ HUGEINT key transcription
using DuckDB.NET.Data;           // DuckDBConnection, DuckDBException, DuckDBParameter

// The capability the effectful passes compose — NOT a bag of opaque pre-baked delegates (the
// `Query/federation#RULE_PLAN` law names an injected evaluator the rejected form): the runtime carries the
// concrete substrate surfaces and the page composes the real reads over them. `Analytical` is the
// `Store/profiles#STORE_LIFECYCLE` `StoreProfile.DuckDbAnalytical` anchor `DuckDBConnection` every columnar pass
// brackets in-page through the `Query/lanes#ANALYTICAL_LANE` read kernel (`CreateCommand`/`UseStreamingMode`/
// `DuckDBParameter`/`ExecuteReaderAsync`, read-only — the lane owns DuckDB reads and never source-of-truth
// writes); `Resolve` is the federation `Func<SetExpr, Seq<UInt128>>` seam EXACTLY as `ElementSetAlgebra.Evaluate`
// consumes it, so the page lowers every violating-key set through the real `Evaluate` fold rather than a
// paraphrase `Func<SetExpr, ElementSet>` that hides the algebra; `FuzzyProbe` is the `DedupMode.Fuzzy` pgvector
// route — a Postgres-tier `VectorMetric.Fn` distance read whose engine (pgvector over `vector`) differs from the
// DuckDB columnar lane, so it stays a distinct surface yielding raw keys the same `Resolve` lowers; `Links`,
// `SourceHead`, and `Baselines` are point reads into the `Query/federation#CROSS_DOC_LINKS` adjacency and the
// edition-keyed baseline-quantile store. Arrives composed at the `Store/profiles#STORE_LIFECYCLE` lifecycle.
public sealed record QualityRuntime(
    DuckDBConnection Analytical,
    Func<SetExpr, Seq<UInt128>> Resolve,
    Func<QualityRule.NearDuplicate, IO<(long Checked, Seq<UInt128> Keys)>> FuzzyProbe,
    Func<QualityRule.ReferentialEdge, Seq<CrossDocLink>> Links,
    Func<UInt128, UInt128> SourceHead,
    Func<UInt128, double> Baselines,
    ClockPolicy Clocks) {
    // The one resolution seam every pass funnels its violating keys through — the real `ElementSetAlgebra.Evaluate`
    // fold over the federation resolver, never a parallel set shape, so a quality result is content-addressed and
    // composes with the element-set algebra (`a clash result ∩ a quality violation` is one `SetExpr.Intersect`).
    public ElementSet Eval(SetExpr expr) => ElementSetAlgebra.Evaluate(expr, Resolve);
    public ElementSet Keys(Seq<UInt128> keys) => Eval(new SetExpr.Literal(keys));
}

// The total gate verdict — `Of` folds the receipt batch ONCE into the union of every `Severity.Error`
// violating set (`Blocking`), the non-gating `Warning`/`Info` riders (`Evidence`), and `Passed` derived
// from `Blocking.Count == 0`, so the verdict is total: a blocked batch still carries its content-addressed
// blocking set a remediation selection reads, and `Blocking` is the real union in BOTH outcomes, never a
// placeholder reachable only when nothing blocks. The verdict is the one gate owner; `Rail` is its typed-
// rail projection a save gate binds, a dashboard reads the bare verdict — one verdict, two egress shapes.
public readonly record struct QualityGate(bool Passed, ElementSet Blocking, Seq<QualityReceipt> Evidence) {
    public static QualityGate Of(Seq<QualityReceipt> receipts) {
        var blocking = receipts.Filter(static r => r.Severity is Severity.Error && r.Violating.Count > 0);
        return new QualityGate(
            blocking.IsEmpty,
            ElementSet.Of(blocking.Bind(static r => r.Violating.Keys)),
            receipts.Filter(static r => r.Severity is not Severity.Error && r.Violating.Count > 0));
    }

    // Each blocking receipt accumulates its own `GateBlocked` on `Validation<QualityFault, _>` so a batch
    // reports EVERY blocking violation (never the first), the `Apply` semigroup unions their faults, and a
    // passing gate carries the bare verdict (riding the `Warning`/`Info` evidence forward, never re-promoted).
    public Validation<QualityFault, QualityGate> Rail(Seq<QualityReceipt> receipts) =>
        receipts.Filter(static r => r.Severity is Severity.Error && r.Violating.Count > 0)
            .Map(static r => Fail<QualityFault, Unit>(new QualityFault.GateBlocked(r.Rule.Subject, r.Violating.Receipt, r.Violating.Count)))
            .Traverse(identity).As()
            .Map(_ => this);
}

public static class QualityPlan {
    // Each constraint-class rule lowers to one settled `Schema/ddl#EXTENSION_DDL` `SchemaDdl` row that owns
    // the `ALTER TABLE … CHECK/EXCLUDE/…` envelope and the table/constraint identifier seam through its own
    // `Sql`/`Configure` projection — `Lower` never emits its own `ALTER TABLE` envelope, never a parallel
    // emitter. The CHECK PREDICATE body is migration-artifact DDL (`Query/domain/postgres#SQL_LAW`: DDL lives
    // in migration artifacts, identifiers parameterize in neither lane), so `LowerOne` assembles the predicate
    // from model-fact identifiers and escapes every embedded value literal at the lowering site (`SetDomain`
    // allowed values and the `RegexShape` pattern single-quote-double, the `RangeBound` bound is a closed model
    // fact, never caller free-text). The split is honest:
    // `Check`/`Exclusion`/`JsonSchemaCheck` rows emit their `Sql` through `MigrationBuilder`, while a
    // `UniqueAcross` `Index` row is model-translatable and hands off to `SchemaDdl.Configure(entity, indexes)`
    // at model-build time — so `Lower` returns the threaded migration AND the index rows, never a no-op emit.
    // `extensionAvailable` is the same deploy-image probe `Schema/ddl#EXTENSION_DDL` `SchemaDdl.Validate` reads —
    // a `JsonSchemaShape` whose `pg_jsonschema` is absent emits NO server-side `CHECK` and instead surfaces a
    // typed `FallbackEvaluated` receipt (the quality plan OWNS reporting that its own JSON-schema rule degraded
    // to the `Json.Schema.JsonSchema.Evaluate` in-process path), so the degradation is evidence on the fact
    // stream, never the silent drop the prior fence's unconditional emit hid behind a decorative fact kind.
    public static Fin<(MigrationBuilder Migration, Seq<SchemaDdl.Index> Indexes, Seq<QualityReceipt> Fallbacks)> Lower(
        MigrationBuilder migration, Seq<QualityRule> rules, Func<string, bool> extensionAvailable, ClockPolicy clocks) =>
        rules.Filter(rule => rule.Site().Lowered && !Degraded(rule, extensionAvailable))
            .Map(LowerOne)
            .Traverse(identity).As()
            .Map(lowered => (
                lowered.Bind(static l => l.Emit).Fold(migration, static (builder, emit) => emit(builder)),
                lowered.Bind(static l => l.Index),
                rules.Filter(rule => Degraded(rule, extensionAvailable))
                    .Map(rule => Receipt(rule, 0L, ElementSet.Empty, None, clocks))));

    // A JSON-schema rule degrades exactly when its server-side extension is absent — the one fallback gate.
    private static bool Degraded(QualityRule rule, Func<string, bool> extensionAvailable) =>
        rule is QualityRule.JsonSchemaShape && !extensionAvailable("pg_jsonschema");

    private static Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>> Emit, Seq<SchemaDdl.Index> Index)> LowerOne(QualityRule rule) =>
        rule switch {
            QualityRule.NotNullSet n => Sql(new SchemaDdl.Check(n.Table, $"{n.Table}_notnull",
                string.Join(" AND ", n.Columns.Map(static c => $"{c} IS NOT NULL")))),
            QualityRule.ConditionalNotNull c => Sql(new SchemaDdl.Check(c.Table, $"{c.Table}_cnotnull",
                $"NOT ({c.When}) OR ({string.Join(" AND ", c.Columns.Map(static col => $"{col} IS NOT NULL"))})")),
            QualityRule.SetDomain d => Sql(new SchemaDdl.Check(d.Table, $"{d.Table}_{d.Column}_domain",
                $"{d.Column} IN ({string.Join(", ", d.Allowed.Map(static v => $"'{v.Replace("'", "''", StringComparison.Ordinal)}'"))})")),
            QualityRule.RangeBound r => Sql(new SchemaDdl.Check(r.Table, $"{r.Table}_{r.Column}_range",
                $"{r.Column} BETWEEN {r.Low} AND {r.High}")),
            QualityRule.Consistency x => Sql(new SchemaDdl.Check(x.Table, $"{x.Table}_{string.Join('_', x.Columns)}_consistency", x.Predicate)),
            QualityRule.RegexShape r => Sql(new SchemaDdl.Check(r.Table, $"{r.Table}_{r.Column}_shape",
                $"{r.Column} ~ '{r.Pattern.Replace("'", "''", StringComparison.Ordinal)}'")),
            QualityRule.FunctionalDependency f => Sql(new SchemaDdl.Exclusion(f.Table, string.Join(", ",
                f.Determinant.Map(static c => $"{c} WITH =").Concat(f.Dependent.Map(static c => $"{c} WITH <>"))))),
            QualityRule.TemporalOverlap t => Sql(new SchemaDdl.Exclusion(t.Table, string.Join(", ",
                t.PartitionBy.Map(static c => $"{c} WITH =").Append($"{t.Period} WITH &&")))),
            QualityRule.UniqueAcross u => Configured(new SchemaDdl.Index(u.Table, u.Columns, IndexMethod.BTree,
                NullsDistinct: Some(u.NullsDistinct))),
            QualityRule.JsonSchemaShape j => Sql(new SchemaDdl.JsonSchemaCheck(j.Table, j.Column, $"{j.Table}_{j.Column}_schema", j.Schema)),
            // `ReferentialEdge` is NOT here — a cross-document reference cannot be a `CHECK` (Postgres forbids
            // subqueries in CHECK) or a server FK across documents, so it is a pass-class rule the `StalePins`
            // link-graph fold resolves in `Evaluate`, filtered out of `Lower` by its non-lowered site.
            _ => Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>>, Seq<SchemaDdl.Index>)>.Fail(new QualityFault.NotAConstraint(rule.Subject)),
        };

    // A constraint-class rule short-circuits to a zero-violation receipt (it enforces at write time); a pass-
    // class rule composes the real substrate read over the `QualityRuntime` and converts a `DuckDBException` to
    // the typed `QualityFault.PassFaulted` (carrying the native `DuckDBErrorType`) at the bracket's one `Catch`
    // edge — never the `@catch`-operator-plus-effect form (that recovery is `@catchM`; the analytical lane and
    // the store rail both convert provider faults at a `Bracket(Catch:)`/`.MapFail` edge, the idiom this reuses).
    // The runtime is a plain capability parameter, not an injected evaluator (the `Query/federation#RULE_PLAN`
    // law: an injected delegate is the rejected form) — the columnar passes bracket the `DuckDBConnection`
    // in-page and the `Fuzzy` probe is the distinct Postgres-tier pgvector route, both resolving keys through the
    // one `rt.Eval`/`rt.Keys` federation seam.
    public static IO<QualityReceipt> Evaluate(QualityRuntime rt, QualityRule rule) =>
        rule.Site().Lowered
            ? IO.pure(Receipt(rule, 0L, ElementSet.Empty, None, rt.Clocks))
            : rule switch {
                QualityRule.StatisticalAnomaly a => AnalyticalPass(rt, a, Some(a.Method),
                    $"SELECT key FROM {a.Table} WHERE {string.Format(CultureInfo.InvariantCulture, a.Method.Scalar, a.Measure, AnomalyMethod.Frame(a.PartitionBy))} > $sigma",
                    Seq(("sigma", (object)a.Sigma))),
                QualityRule.CompletenessRatio c => AnalyticalPass(rt, c, None,
                    $"SELECT key FROM {c.Table} WHERE (count({c.Column}) OVER ())::double / nullif(count(*) OVER (), 0) < $min AND {c.Column} IS NULL",
                    Seq(("min", (object)c.MinFraction))),
                QualityRule.Cardinality k => AnalyticalPass(rt, k, None,
                    $"SELECT any_value(key) AS key FROM {k.Table} GROUP BY {string.Join(", ", k.Determinant)} " +
                    k.Max.Match(Some: max => "HAVING count(*) NOT BETWEEN $min AND $max", None: () => "HAVING count(*) < $min"),
                    k.Max.Match(Some: max => Seq(("min", (object)k.Min), ("max", (object)max)), None: () => Seq(("min", (object)k.Min)))),
                QualityRule.DistributionDrift d => AnalyticalPass(rt, d, None,
                    $"SELECT $drift::hugeint AS key FROM {d.Table} GROUP BY ALL HAVING abs(quantile_cont({d.Measure}, $q) - $baseline) > $shift",
                    Seq(("q", (object)d.Quantile), ("baseline", rt.Baselines(d.Baseline)), ("shift", (object)d.MaxShift), ("drift", (object)d.Baseline))),
                // `Exact` groups on the `ContentKeyColumn` (the `Version/snapshots#COMPRESSION_HASHING`
                // `HashPolicy.Identity` `XxHash128` content address) — `GROUP BY ... HAVING count(*) > 1` is the
                // complete in-column duplicate test; `Fuzzy` is the distinct Postgres-tier pgvector route.
                QualityRule.NearDuplicate n when n.Mode == DedupMode.Exact => AnalyticalPass(rt, n, None,
                    $"SELECT key FROM {n.Table} WHERE {n.ContentKeyColumn} IN " +
                    $"(SELECT {n.ContentKeyColumn} FROM {n.Table} GROUP BY {n.ContentKeyColumn} HAVING count(*) > 1)",
                    Seq<(string, object)>()),
                QualityRule.NearDuplicate n => FuzzyPass(rt, n),
                QualityRule.ReferentialEdge e => IO.pure(
                    Receipt(e, 0L, rt.Eval(Stale(LinkStore.StalePins(rt.Links(e), rt.SourceHead))), None, rt.Clocks)),
                QualityRule.Freshness f => IO.pure(
                    Receipt(f, 0L, rt.Eval(new SetExpr.ByRule($"freshness:{f.Table}.{f.TimestampColumn}")), None, rt.Clocks)),
                _ => IO.pure(Receipt(rule, 0L, ElementSet.Empty, None, rt.Clocks)),
            };

    // The gate is the total `QualityGate.Of` verdict — `Blocking` is the union of every `Severity.Error`
    // violating set in BOTH outcomes, `Evidence` rides the `Warning`/`Info` violations forward, and `Passed`
    // derives from the empty blocking set; `Rail` accumulates one `GateBlocked` per blocking receipt onto the
    // typed `Validation<QualityFault, _>` so a save gate binds the rail while a dashboard reads the verdict
    // (`Query/domain/validation#OUTCOME_PROJECTION`: evidence rides forward, never re-promoted). One verdict
    // owner — re-deriving the severity partition by hand at the plan site is the deleted duplicate fold.
    public static QualityGate Gate(Seq<QualityReceipt> receipts) => QualityGate.Of(receipts);

    // The columnar pass binds the closed-vocabulary `AnomalyMethod.Scalar` form and the measure/determinant
    // identifiers (model facts) into the analytical text while the threshold/quantile/baseline bind as
    // `DuckDBParameter` values so no caller input concatenates. The read is the `Query/lanes#ANALYTICAL_LANE`
    // kernel composed in-page over the `Analytical` anchor — `CreateCommand`/`UseStreamingMode`/`DuckDBParameter`/
    // `ExecuteReaderAsync` streaming the violating `key` column WITH its row tally — bracketed so the reader and
    // command dispose deterministically. A `DuckDBException` converts to `QualityFault.PassFaulted.Of(site, ex)`
    // (carrying the native `DuckDBErrorType`, symmetric with `StoreFault.Analytical`) at the bracket's one
    // `Catch` edge, the only seam columnar faults cross; the keys lower through the one `rt.Keys` federation seam.
    private static IO<QualityReceipt> AnalyticalPass(QualityRuntime rt, QualityRule rule, Option<AnomalyMethod> method, string sql, Seq<(string, object)> binds) =>
        IO.lift(rt.Clocks.Mark).Bind(mark =>
            IO.lift(() => {
                var command = rt.Analytical.CreateCommand();
                command.CommandText = sql;
                command.UseStreamingMode = true;
                binds.Iter(b => command.Parameters.Add(new DuckDBParameter(b.Item1, b.Item2)));
                return command;
            }).Bracket(
                Use: command => IO.liftVAsync(async env => {
                    await using var reader = await command.ExecuteReaderAsync(env.Token);
                    var keys = Seq<UInt128>();
                    var scanned = 0L;
                    while (await reader.ReadAsync(env.Token)) {
                        scanned++;
                        if (!reader.IsDBNull(0)) keys = keys.Add(KeyOf(reader.GetFieldValue<BigInteger>(0)));
                    }
                    return Receipt(rule, scanned, rt.Keys(keys), method, rt.Clocks) with { Elapsed = rt.Clocks.Elapsed(mark) };
                }),
                Catch: error => error.Exception.Case is DuckDBException duck
                    ? IO.fail<QualityReceipt>(QualityFault.PassFaulted.Of(rule.Site(), duck))
                    : IO.fail<QualityReceipt>(QualityFault.PassFaulted.Create(error.Message)),
                Fin: static command => IO.liftVAsync<Unit>(async _ => { await command.DisposeAsync(); return unit; })));

    // The `DedupMode.Fuzzy` route is the Postgres-tier pgvector probe — distinct from the DuckDB columnar lane
    // because the `vector`/`halfvec` column lives in pg — projecting the `VectorMetric.Order` distance under
    // `MaxDistance` over the `EmbeddingColumn` and returning the near-match keys the same `rt.Keys` seam lowers;
    // a DuckDB fault is impossible here, so a provider fault rides the probe's own `IO` rail.
    private static IO<QualityReceipt> FuzzyPass(QualityRuntime rt, QualityRule.NearDuplicate n) =>
        IO.lift(rt.Clocks.Mark).Bind(mark =>
            rt.FuzzyProbe(n).Map(hit => Receipt(n, hit.Checked, rt.Keys(hit.Keys), None, rt.Clocks) with { Elapsed = rt.Clocks.Elapsed(mark) }));

    private static QualityReceipt Receipt(QualityRule rule, long checkedRows, ElementSet violating, Option<AnomalyMethod> method, ClockPolicy clocks) =>
        new(rule, rule.Site(), rule.Severity, checkedRows, violating, method, Duration.Zero, clocks.Now);

    private static SetExpr Stale(Seq<CrossDocLink> stale) => new SetExpr.Literal(stale.Map(static link => link.From));

    // DuckDB surfaces the federated content key's `HUGEINT` column as a signed-128-bit `BigInteger` (the same
    // `BigInteger`↔`HUGEINT` transcription `Query/lanes#ANALYTICAL_LANE` writes for the unsigned `UInt128`
    // address), so a top-bit-set key reads back negative — `KeyOf` reinterprets the two's-complement bits as the
    // unsigned `UInt128` the federation algebra keys on, never a checked `(UInt128)` cast that overflows.
    private static UInt128 KeyOf(BigInteger huge) =>
        (UInt128)(huge.Sign < 0 ? huge + (BigInteger.One << 128) : huge);

    private static Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>>, Seq<SchemaDdl.Index>)> Sql(SchemaDdl.Check check) => Sql(check.Sql);
    private static Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>>, Seq<SchemaDdl.Index>)> Sql(SchemaDdl.Exclusion exclusion) => Sql(exclusion.Sql);
    private static Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>>, Seq<SchemaDdl.Index>)> Sql(SchemaDdl.JsonSchemaCheck schema) => Sql(schema.Sql);
    private static Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>>, Seq<SchemaDdl.Index>)> Sql(string sql) =>
        Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>>, Seq<SchemaDdl.Index>)>.Succ(
            (Seq<Func<MigrationBuilder, MigrationBuilder>>(builder => { builder.Sql(sql); return builder; }), Seq<SchemaDdl.Index>()));

    // `UniqueAcross` is a model-translatable index — it folds through the row's `Configure(EntityTypeBuilder)`
    // at model-build time, never a raw `CREATE UNIQUE INDEX` string, so it rides the `Index` channel `Lower`
    // returns rather than a migration emit; the caller hands the rows to `SchemaDdl.Configure(entity, indexes)`.
    private static Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>>, Seq<SchemaDdl.Index>)> Configured(SchemaDdl.Index index) =>
        Fin<(Seq<Func<MigrationBuilder, MigrationBuilder>>, Seq<SchemaDdl.Index>)>.Succ(
            (Seq<Func<MigrationBuilder, MigrationBuilder>>(), Seq(index)));
}
```

| [INDEX] | [RULE]                | [SITE]                                                  | [MECHANISM]                                                          |
| :-----: | :-------------------- | :----------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | not-null / range      | `SchemaDdl.Check` `CHECK`                               | model fact via `HasCheckConstraint`, write-time statement error      |
|  [02]   | conditional-not-null  | `SchemaDdl.Check` `NOT(guard) OR (...)`                | partial completeness, required only when the guard holds             |
|  [15]   | consistency           | `SchemaDdl.Check` cross-column predicate                | free relational invariant over a column tuple (`a <= b`), write-time |
|  [03]   | set-domain            | `SchemaDdl.Check` `IN (...)`                            | allowed-value membership, row-escaped literal list                   |
|  [04]   | regex-shape           | `SchemaDdl.Check` `~`                                   | server-side pattern, expression-GIN serves the hot path              |
|  [05]   | functional-dependency | `SchemaDdl.Exclusion` `= WITH <>`                       | determinant equal forbids dependent unequal                          |
|  [06]   | unique-across         | `SchemaDdl.Index` unique / `NullsDistinct` `Configure` | model-translatable index, single-null `NULLS NOT DISTINCT` law       |
|  [07]   | temporal-overlap      | `SchemaDdl.Exclusion` `EXCLUDE USING gist` (`btree_gist`) | partition `=` plus range `&&` non-overlap                         |
|  [08]   | referential-edge      | foreign reference + `CrossDocLink`/`StalePins`         | federated link graph, stale-pin violation                           |
|  [09]   | json-schema-shape     | `SchemaDdl.JsonSchemaCheck` (`pg_jsonschema`)          | server-side, `Schema/ddl` `JsonSchema.Net` in-process fallback       |
|  [10]   | cardinality           | DuckDB `GROUP BY count(*)` bound                       | per-determinant dependent-count multiplicity, not a value range      |
|  [11]   | completeness-ratio    | DuckDB non-null fraction                               | `count(col)/count(*)` over threshold, statistical completeness gate  |
|  [12]   | freshness             | op-log changefeed-fold                                  | staleness bound caught as each `OpLogEntry` commits                  |
|  [13]   | statistical-anomaly / distribution-drift | DuckDB analytical pass                | columnar z-score/IQR/MAD outlier or `quantile_cont` table-level drift |
|  [14]   | near-duplicate        | DuckDB `GROUP BY ContentKeyColumn HAVING count(*) > 1` (`Exact`) / `VectorMetric` probe (`Fuzzy`) | group on the `XxHash128` content-key column (`HashPolicy.Identity`); fuzzy embedding distance under `MaxDistance` |

## [04]-[RESEARCH]

- [ANOMALY_PASS_PUSHDOWN]: the DuckDB columnar `AnomalyMethod` passes — each a SINGLE-pass window scalar (z-score mean-centered `stddev_pop`, IQR median-centered `quantile_cont`-scaled, MAD median-centered `0.6745`-`stddev_pop`-scaled), DuckDB forbidding the nested-window `median(... median() OVER w) OVER w` a true median-of-absolute-deviations needs — over a measure column, optionally per `PartitionBy`, and the `quantile_cont` distribution-drift fold against a baseline content key as a single table-level verdict on a live in-process engine — whether each estimator lowers to one columnar window query (and whether the true two-pass-CTE MAD denominator is worth its second scan), drift yields one drift key or none (never a per-row window predicate that returns all rows or none), and the violating keys project as a `SetExpr.Literal` carrying the scanned cardinality, measured before the pass fences pin.
- [COUNT_BOUND_PASS]: the `Cardinality` per-determinant `GROUP BY count(*)` multiplicity bound and the `CompletenessRatio` non-null-fraction-over-threshold fold over the columnar lane — whether a relationship-multiplicity violation and a statistical-completeness gap each lower to one columnar aggregate yielding the offending determinant keys, distinct from the row-level `NotNullSet`/`RangeBound` `CHECK` floor, measured before the count-bound fences pin.
- [DEDUP_PREFILTER]: the `DedupMode.Exact` columnar `GROUP BY ContentKeyColumn HAVING count(*) > 1` duplicate-group pass over the `Version/snapshots#COMPRESSION_HASHING` `HashPolicy.Identity` `XxHash128` content-key column (the identity-column group-by is the complete in-column duplicate test — the chunker's `HashPolicy.Content` `XxHash3` `ShortTag` cross-set membership pre-filter and `ContentChunker.Novel` answer a different question and carry no column here) and the `DedupMode.Fuzzy` `VectorMetric` pgvector distance probe over the `EmbeddingColumn` under a `MaxDistance` threshold — whether the exact path's content-key group-by yields the colliding row set and the fuzzy path reuses the catalogued distance functions, measured before the `NearDuplicate` fence pins.
- [CROSS_DOC_REFERENTIAL]: the `CrossDocLink`/`LinkStore.StalePins` cross-document referential-integrity fold — whether a stale-pin reference surfaces as a `ReferentialEdge` violation through the federated link graph rather than a per-document foreign key, confirmed against the federation adjacency before the referential fence pins.

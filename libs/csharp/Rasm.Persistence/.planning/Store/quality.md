# [PERSISTENCE_QUALITY]

Rasm.Persistence makes declarative validation rules, cross-federation referential integrity, and anomaly/near-duplicate detection one closed rule axis that lowers each rule to the least-cost enforcement site rather than re-checking in process. `QualityRule` is the `[Union]` rule family — null-completeness, cross-document uniqueness, referential edges, range and regex shape, JSON-schema shape, statistical anomaly, and near-duplicate — and `QualityPlan` is the lowering fold that drives each rule to a `CHECK`/`EXCLUDE`/`FK` constraint, a `jsonb_matches_schema` document predicate, an op-log changefeed-fold, or a DuckDB analytical pass. The rule plan rides the federation `CrossDocLink`/`StalePins` surface and emits an `ElementSet` of the violating rows so a quality result composes with every other selection. The admitted `pg_jsonschema` carries the schema-shape validation, `DuckDB.NET.Data.Full` the columnar-anomaly pass, `Pgvector.EntityFrameworkCore` the near-duplicate probe, and `btree_gist` the `EXCLUDE` constraint; `SetExpr`/`ElementSet`/`RulePlan`/`CrossDocLink`/`StalePins`, `ContentChunk.ShortTag`, `ClockPolicy`, and `ReceiptSinkPort` arrive settled. `Rasm.Bim/Model` authors the IFC validation rules as `QualityRule` rows and `Rasm.Compute` is the geometry-derived anomaly rule source, both consumed at the seam.

Wire posture: this page is host-local — quality rules lower to server-side constraints or in-process passes, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The IFC validation rules and the geometry-derived anomaly sources arrive from the Bim/Compute seams as `QualityRule` rows, never minted here.

## [01]-[INDEX]

- [01]-[QUALITY_RULE]: the rule union, the enforcement-site axis, and the lowering fold.
- [02]-[QUALITY_PLAN]: the lowering fold to the cheapest site and the violating-set projection.

## [02]-[QUALITY_RULE]

- Owner: `QualityRule` the `[Union]` rule family; `EnforcementSite` the `[SmartEnum<string>]` least-cost-site axis; `QualityReceipt` the typed rule-evaluation evidence; `QualityFact` with `QualityFactKind` the page-wide fact stream.
- Cases: `NotNullSet` (a column set that must be complete), `UniqueAcross` (a cross-document uniqueness over a column tuple), `ReferentialEdge` (a `CrossDocLink`-backed foreign reference), `RangeBound` (a numeric/temporal range), `RegexShape` (a string pattern), `JsonSchemaShape` (a JSON Schema over a jsonb column), `StatisticalAnomaly` (a columnar outlier over a measure), `NearDuplicate` (a content/embedding near-match) on `QualityRule`; `EnforcementSite` check | exclude | foreign-key | json-schema | changefeed-fold | duckdb-pass | vector-probe.
- Entry: `public static EnforcementSite Site(QualityRule rule)` projects the least-cost enforcement site for a rule; `public static SetExpr Violating(QualityRule rule)` lowers the rule into the `Query/federation#ELEMENT_SET_ALGEBRA` `SetExpr` selecting the violating rows so a quality result is an `ElementSet`.
- Auto: each rule lowers to the cheapest site that can enforce it — `NotNullSet`/`RangeBound`/`RegexShape` lower to a `Schema/ddl#EXTENSION_DDL` `Check` constraint (a model fact emitted through `HasCheckConstraint`), `UniqueAcross` to an `Index` with `NullsDistinct` or an `Exclusion` over `btree_gist` for a range-overlap rule, `ReferentialEdge` to a foreign-key constraint backed by the `CrossDocLink` adjacency, `JsonSchemaShape` to a `JsonSchemaCheck` `CHECK (jsonb_matches_schema(...))` validated server-side by `pg_jsonschema` (degrading to the JsonSchema.Net in-process fallback when the extension is absent), `StatisticalAnomaly` to a DuckDB analytical pass over the columnar lane (a z-score/IQR outlier over a measure column), and `NearDuplicate` to a `ContentChunk.ShortTag` 64-bit pre-filter ahead of an `XxHash128` content-key compare for exact dedup or a pgvector cosine-distance probe for fuzzy near-match; a rule that cannot lower to a constraint folds over the op-log changefeed so a streaming violation is caught as each `OpLogEntry` commits; every rule emits its violating rows as an `ElementSet` so a quality gate composes with any other selection (a clash result intersected with a quality violation is one `SetExpr.Intersect`).
- Receipt: a rule evaluation rides `QualityFact` carrying the rule kind, the enforcement site, the violation count, and the violating `ElementSet` receipt; `QualityReceipt(RuleKind, Site, Checked, Violating, ElementSet Subject, Instant At)` is the typed evidence feeding `StoreFact`/`StoreEvidence`; a constraint-add failure or a server-side validation rejection is a typed quality fault, never silent.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, DuckDB.NET.Data.Full, Pgvector.EntityFrameworkCore, Pgvector, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new rule kind is one `QualityRule` case plus one `Site`/`Violating` arm; a new enforcement site is one `EnforcementSite` row; zero new surface — a per-rule-type validator, an imperative in-process re-check, or a second integrity engine is the deleted form because one union lowers to the cheapest site and emits an `ElementSet`.
- Boundary: every rule lowers to the least-cost enforcement site — an imperative in-process re-check over a materialized scan is the deleted form, the rule is a constraint, a server-side predicate, a changefeed-fold, or a columnar pass, never application code; `NotNullSet`/`RangeBound`/`RegexShape`/`UniqueAcross`/`ReferentialEdge` are schema-level constraints emitted through the `Schema/ddl` `SchemaDdl` rows so they are model facts, never deploy-time hand DDL; `JsonSchemaShape` rides `pg_jsonschema` `jsonb_matches_schema` server-side and degrades to JsonSchema.Net in-process only when the extension is absent, never silently dropping validation; `StatisticalAnomaly` rides the DuckDB analytical lane so an outlier detection is a columnar query, never a row-by-row fold; `NearDuplicate` probes the `ContentChunk.ShortTag` 64-bit bloom/sketch pre-filter ahead of the authoritative `XxHash128` content-key compare for exact dedup or the pgvector cosine probe for fuzzy match, never a quadratic pairwise scan; the violating-row projection is an `ElementSet` so a quality result is content-addressed and composes with the element-set algebra; the `ReferentialEdge` reads the `Query/federation#CROSS_DOC_LINKS` adjacency and the `StalePins` projection so a cross-document referential violation is caught as a stale pin, never a per-document foreign key; the IFC validation rules arrive from `Rasm.Bim/Model` as `QualityRule` rows through the `Store/quality ← Rasm.Bim/Model` seam and the geometry-derived anomaly sources arrive from `Rasm.Compute` through the `Store/quality ← Rasm.Compute` seam, so a Bim or Compute domain rule federates into the one rule axis without a parallel checker.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class QualityFactKind {
    public static readonly QualityFactKind Checked = new("checked");
    public static readonly QualityFactKind Violated = new("violated");
    public static readonly QualityFactKind ConstraintAdded = new("constraint-added");
    public static readonly QualityFactKind FallbackEvaluated = new("fallback-evaluated");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class EnforcementSite {
    public static readonly EnforcementSite Check = new("check");
    public static readonly EnforcementSite Exclude = new("exclude");
    public static readonly EnforcementSite ForeignKey = new("foreign-key");
    public static readonly EnforcementSite JsonSchema = new("json-schema");
    public static readonly EnforcementSite ChangefeedFold = new("changefeed-fold");
    public static readonly EnforcementSite DuckDbPass = new("duckdb-pass");
    public static readonly EnforcementSite VectorProbe = new("vector-probe");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record QualityRule {
    private QualityRule() { }

    public sealed record NotNullSet(string Table, Seq<string> Columns) : QualityRule;
    public sealed record UniqueAcross(string Table, Seq<string> Columns, bool NullsDistinct) : QualityRule;
    public sealed record ReferentialEdge(string Table, string Column, string ReferencedTable, string ReferencedColumn) : QualityRule;
    public sealed record RangeBound(string Table, string Column, string Low, string High) : QualityRule;
    public sealed record RegexShape(string Table, string Column, string Pattern) : QualityRule;
    public sealed record JsonSchemaShape(string Table, string Column, string Schema) : QualityRule;
    public sealed record StatisticalAnomaly(string Table, string Measure, double Sigma) : QualityRule;
    public sealed record NearDuplicate(string Table, string EmbeddingColumn, double MaxCosineDistance) : QualityRule;

    public EnforcementSite Site() =>
        Switch(
            notNullSet:         static _ => EnforcementSite.Check,
            rangeBound:         static _ => EnforcementSite.Check,
            regexShape:         static _ => EnforcementSite.Check,
            uniqueAcross:       static _ => EnforcementSite.Exclude,
            referentialEdge:    static _ => EnforcementSite.ForeignKey,
            jsonSchemaShape:    static _ => EnforcementSite.JsonSchema,
            statisticalAnomaly: static _ => EnforcementSite.DuckDbPass,
            nearDuplicate:      static _ => EnforcementSite.VectorProbe);
}

public sealed record QualityReceipt(
    string RuleKind, EnforcementSite Site, long Checked, long Violating, ElementSet Subject, Instant At);
```

## [03]-[QUALITY_PLAN]

- Owner: `QualityPlan` the static surface lowering a `Seq<QualityRule>` into its enforcement artifacts — the schema-constraint DDL set, the server-side validation predicates, the changefeed-fold registrations, and the analytical passes — and folding the violating `ElementSet` per rule.
- Entry: `public static Fin<MigrationBuilder> Lower(MigrationBuilder migration, Seq<QualityRule> rules)` folds the constraint-class rules (`NotNullSet`/`UniqueAcross`/`ReferentialEdge`/`RangeBound`/`RegexShape`/`JsonSchemaShape`) into `Schema/ddl#EXTENSION_DDL` `SchemaDdl` rows emitted through the migration; `public static IO<QualityReceipt> Evaluate(QualityRule rule, Func<SetExpr, ElementSet> evaluate, Func<QualityRule, IO<ElementSet>> pass)` runs a non-constraint rule (anomaly, near-duplicate, changefeed-fold) and folds the violating set.
- Auto: the lowering fold partitions the rules by enforcement site — the constraint-class rules become `SchemaDdl.Check`/`Index`/`Exclusion`/`JsonSchemaCheck` rows folded into the migration so they enforce at write time (a violating write is a statement error, never an after-the-fact scan), and the pass-class rules (`StatisticalAnomaly`/`NearDuplicate`/`ReferentialEdge` cross-document) register as op-log changefeed folds or scheduled DuckDB/vector passes so a streaming or batch violation is caught past the constraint lane; the violating rows of every rule are an `ElementSet` lowered through the federation algebra so a quality dashboard, a remediation selection, and a downstream gate all read the one content-addressed set; a `ReferentialEdge` cross-document rule reads the `Query/federation#CROSS_DOC_LINKS` `StalePins` projection so a stale-pin reference is a quality violation; the anomaly pass rides the DuckDB analytical lane's `GROUP BY`/window so an outlier over a measure column is a columnar query.
- Receipt: a constraint lowering rides `quality.constraint.added`; a rule evaluation rides `quality.rule.eval` carrying the violation count; the typed `QualityReceipt` carries the violating `ElementSet` and feeds `StoreFact`/`StoreEvidence`; a constraint-add or validation failure is a typed quality fault.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, DuckDB.NET.Data.Full, Pgvector.EntityFrameworkCore, LanguageExt.Core, NodaTime.
- Growth: a new lowering target is one arm on `Lower`; a new pass kind is one arm on `Evaluate`; zero new surface — a second lowering fold, a parallel constraint emitter, or a per-rule evaluation service is the deleted form because the plan lowers the one rule union into the settled `SchemaDdl` rows and the federation `SetExpr`.
- Boundary: the plan lowers the rule union into the settled `Schema/ddl` `SchemaDdl` constraint rows and the federation `SetExpr` — a hand-written constraint DDL beside the `SchemaDdl` set or a parallel selection shape is the deleted form; the constraint-class rules enforce at write time so a violating write is a statement error, never an after-the-fact scan, and only the rules a constraint cannot express (anomaly, near-duplicate, cross-document referential) ride a changefeed-fold or a scheduled pass; the violating-set projection is the one `ElementSet` currency so a quality result composes with the rule engine and the element-set algebra, never a per-rule result shape; the cross-document referential rule reads the `CrossDocLink`/`StalePins` projection so it rides the federated link graph, never a per-document foreign key; the IFC validation rules and the geometry-derived anomaly sources lower through this one plan so a Bim or Compute domain rule is a `QualityRule` row, never a parallel checker.

```csharp signature
public static class QualityPlan {
    public static Fin<MigrationBuilder> Lower(MigrationBuilder migration, Seq<QualityRule> rules) =>
        rules.Filter(static rule => rule.Site() is { Key: "check" or "exclude" or "foreign-key" or "json-schema" })
            .Fold(
                Fin.Succ(migration),
                static (acc, rule) => acc.Map(builder => { builder.Sql(ConstraintSql(rule)); return builder; }));

    public static IO<QualityReceipt> Evaluate(
        QualityRule rule, Func<SetExpr, ElementSet> evaluate, Func<QualityRule, IO<ElementSet>> pass, ClockPolicy clocks) =>
        rule.Site() is { Key: "check" or "exclude" or "foreign-key" or "json-schema" }
            ? IO.pure(new QualityReceipt(rule.GetType().Name, rule.Site(), 0L, 0L, ElementSet.Empty, clocks.Now))
            : pass(rule).Map(violating =>
                new QualityReceipt(rule.GetType().Name, rule.Site(), violating.Count, violating.Count, violating, clocks.Now));

    private static string ConstraintSql(QualityRule rule) =>
        rule.Switch(
            notNullSet:    static n => $"ALTER TABLE {n.Table} ADD CONSTRAINT {n.Table}_notnull CHECK ({string.Join(" AND ", n.Columns.Map(c => $"{c} IS NOT NULL"))})",
            rangeBound:    static r => $"ALTER TABLE {r.Table} ADD CONSTRAINT {r.Table}_{r.Column}_range CHECK ({r.Column} BETWEEN {r.Low} AND {r.High})",
            regexShape:    static r => $"ALTER TABLE {r.Table} ADD CONSTRAINT {r.Table}_{r.Column}_shape CHECK ({r.Column} ~ '{r.Pattern}')",
            uniqueAcross:  static u => $"CREATE UNIQUE INDEX {u.Table}_unique ON {u.Table} ({string.Join(", ", u.Columns)}){(u.NullsDistinct ? "" : " NULLS NOT DISTINCT")}",
            referentialEdge: static e => $"ALTER TABLE {e.Table} ADD CONSTRAINT {e.Table}_{e.Column}_fk FOREIGN KEY ({e.Column}) REFERENCES {e.ReferencedTable} ({e.ReferencedColumn})",
            jsonSchemaShape: static j => $"ALTER TABLE {j.Table} ADD CONSTRAINT {j.Table}_{j.Column}_schema CHECK (jsonb_matches_schema('{j.Schema}', {j.Column}))",
            statisticalAnomaly: static _ => string.Empty,
            nearDuplicate:      static _ => string.Empty);
}
```

| [INDEX] | [RULE]              | [SITE]                                       | [MECHANISM]                                              |
| :-----: | :------------------ | :------------------------------------------- | :------------------------------------------------------- |
|  [01]   | not-null / range    | `CHECK` constraint                           | model fact, write-time statement error                  |
|  [02]   | unique-across       | unique index / `EXCLUDE` over `btree_gist`   | single-null `NULLS NOT DISTINCT` or range non-overlap   |
|  [03]   | referential-edge    | foreign key + `CrossDocLink`/`StalePins`     | federated link graph, stale-pin violation               |
|  [04]   | json-schema-shape   | `jsonb_matches_schema` (`pg_jsonschema`)     | server-side, JsonSchema.Net in-process fallback          |
|  [05]   | statistical-anomaly | DuckDB analytical pass                       | columnar z-score/IQR over a measure column              |
|  [06]   | near-duplicate      | `ShortTag` pre-filter + pgvector cosine probe | 64-bit sketch ahead of `XxHash128` / fuzzy embedding match |

## [04]-[RESEARCH]

- [JSONB_SCHEMA_VALIDATE]: the `pg_jsonschema` `jsonb_matches_schema` server-side CHECK against a real JSON Schema document and the JsonSchema.Net in-process degradation when the pgrx extension is absent, confirming the document-shape rule lowers to a server-side predicate before the `JsonSchemaShape` fence pins.
- [ANOMALY_PASS_PUSHDOWN]: the DuckDB columnar outlier pass (z-score/IQR window over a measure column) and the pgvector cosine near-duplicate probe over an embedding column on a live in-process engine — whether the anomaly detection lowers to one columnar query and the near-match probe rides the `ShortTag` pre-filter ahead of the authoritative compare, measured before the pass fences pin.
- [CROSS_DOC_REFERENTIAL]: the `CrossDocLink`/`StalePins` cross-document referential-integrity fold — whether a stale-pin reference surfaces as a `ReferentialEdge` violation through the federated link graph rather than a per-document foreign key, confirmed against the federation adjacency before the referential fence pins.

# [PERSISTENCE_SCHEMA_DDL]

Generated-column and declarative DDL surface for every store the suite opens. `DerivedColumn` rows decide stored-versus-virtual generated columns and route each through `HasComputedColumnSql(sql, stored)` — or `IsGeneratedTsVectorColumn(config, sources)` for a full-text search column — plus their co-located CHECK constraints. `SchemaDdl` is one `[Union]` over the whole declarative-DDL family — extension, index, exclusion, temporal-key, partition, check, json-schema, composite, and native-enum — folded by one `Declare`/`Configure`/`Migrate`/`Validate`/`Sql` surface, so the schema-provisioning set is one owner, not a scatter of per-feature emitters. The split that matters is translator ownership: an `Index` row whose access method the Npgsql EF provider translates composes `NpgsqlIndexBuilderExtensions` at model time through `Configure(EntityTypeBuilder)`, never a hand-spelled `CREATE INDEX` string; only the access methods with no first-party translator (`diskann`, `bm25`) and the server-tier policy DDL with no translator at all (TimescaleDB, declarative partitioning) project a `Fin<string>` through `ProvisionSql` and ride `MigrationBuilder.Sql` through the one `Sql` traverse. The TimescaleDB hypertable/continuous-aggregate/retention/columnstore steps, the PG18 native range/list/hash partition steps, and the `vectorscale` diskann / `pg_search` bm25 index builds are all arms on this one union — the deploy-side raw-SQL provisioning set is one owner with the declarative DDL set, never a second `Provision` static surface, and `ProvisionSql` emits the catalogued `SELECT`-function-versus-`CALL`-procedure verb per step so a policy adder never mis-spells its invocation.

## [01]-[INDEX]

- [01]-[GENERATED_COLUMNS]: stored-versus-virtual decision, computed-column emission, and co-located check constraints.
- [02]-[EXTENSION_DDL]: extension, model-composed index, exclusion, temporal-key, declarative partition, check, json-schema, composite, native-enum, and server-tier provisioning DDL (timescale hypertable/cagg/retention/columnstore + diskann/bm25 index build).

## [02]-[GENERATED_COLUMNS]

- Owner: `DerivedColumn` row record carrying the stored-versus-virtual law, its optional `TsVectorSpec` full-text derivation, and its co-located CHECK constraint; `ColumnInvariant` row record for table-level check constraints that are not column-derived.
- Entry: `public bool Stored` — derived; `Replicated || Indexed || TsVector.IsSome` is the whole decision; `public PropertyBuilder<T> Apply(PropertyBuilder<T> property)` dispatches the `TsVector` arm through `IsGeneratedTsVectorColumn(config, [..sources])` and every other derivation through `HasComputedColumnSql(Sql, stored: Stored)`, and `public EntityTypeBuilder<T> Constrain(EntityTypeBuilder<T> entity)` folds each `ColumnInvariant` into `ToTable(t => t.HasCheckConstraint(name, sql))`.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Sqlite
- Growth: one `DerivedColumn` row per derived projection — a full-text search column is the same row carrying `TsVector: Some(...)`; one `ColumnInvariant` row per table-level check; zero new surface.
- Boundary: the pg provider emits VIRTUAL generated columns by default at the Npgsql-v10 provider floor over PG18 — `HasComputedColumnSql(sql, stored: false)` emits VIRTUAL when `Stored` is false and PG18 is the target — and the sqlite provider emits its own VIRTUAL/STORED form. A STORED column exists only where the `Stored` flag derives true — a hot predicate on a virtual column is the promotion signal to stored-plus-index. A replicated row is STORED so the logical-replication publication's `publish_generated_columns` field carries it; an indexed row is STORED so the index reads materialized bytes; a tsvector search column is a `DerivedColumn` carrying `TsVector: Some(TsVectorSpec(config, sources))` so `Apply` routes through `IsGeneratedTsVectorColumn(config, [..sources])` — the language `Config` and the source-column set are model facts the provider emits as `GENERATED ALWAYS AS (to_tsvector('config', …)) STORED`, never a hand-spelled `to_tsvector` `Sql` string, and the spec forces STORED so the search GIN reads materialized lexemes. A derived projection computed in application code beside a store-computed twin is the deleted pattern — one `Sql` expression (or one `TsVectorSpec`) owns the derivation. Document-shape invariants and range/sign bounds are `ColumnInvariant` rows so a `CHECK` lands as a model fact emitted through `HasCheckConstraint`, never deploy-time hand DDL beside the migration set; a geodesic-planar twin rides the generated-column law as a stored planar projection feeding the hot planar predicate while the geography column holds geodesic truth.

```csharp signature
public sealed record ColumnInvariant(string Name, string Sql);

// A tsvector search column is the language `Config` plus the source columns it concatenates, not a
// hand-spelled `to_tsvector` string — `IsGeneratedTsVectorColumn` bakes the config as a model fact and
// forces STORED so the GIN row reads materialized lexemes (doctrine full-text spine: stored tsvector + GIN).
public readonly record struct TsVectorSpec(string Config, Seq<string> Sources);

public sealed record DerivedColumn(
    string Table,
    string Column,
    string Sql,
    bool Replicated,
    bool Indexed,
    Option<TsVectorSpec> TsVector = default,
    Seq<ColumnInvariant> Checks = default) {
    // A tsvector column is materialized by construction; the search GIN reads its stored bytes.
    public bool Stored => Replicated || Indexed || TsVector.IsSome;

    // The tsvector arm routes through the provider's `IsGeneratedTsVectorColumn(config, sources)` so the
    // language config and source set are model facts; every other derived projection rides one `Sql`
    // expression through `HasComputedColumnSql` — an application-side twin of a store-computed column is
    // the deleted pattern either way.
    public PropertyBuilder<T> Apply<T>(PropertyBuilder<T> property) =>
        TsVector.Match(
            Some: ts => (PropertyBuilder<T>)(object)((PropertyBuilder)property).IsGeneratedTsVectorColumn(ts.Config, [.. ts.Sources]),
            None: () => property.HasComputedColumnSql(Sql, stored: Stored));

    public EntityTypeBuilder<TEntity> Constrain<TEntity>(EntityTypeBuilder<TEntity> entity) where TEntity : class =>
        Checks.Fold(entity, static (builder, check) =>
            builder.ToTable(table => table.HasCheckConstraint(check.Name, check.Sql)));
}
```


## [03]-[EXTENSION_DDL]

- Owner: `SchemaDdl` `[Union]` declaration-row family with the frozen `Extensions` row set. The `Extension` case carries `AccessMethod`, `PreloadGated`, `Cascade`, and `Fallback` columns so one row owns the extension's install DDL, its index access method, its `shared_preload_libraries` gate, and its app-side degradation path. The `Index` case carries an `IndexMethod` access-method row, operator classes, `Include` covering columns, `NullsDistinct`, `NullSort` ordering, `Collation`, `Concurrent`, a `Predicate` partial-index clause, and a `Storage` build-option map — and composes `NpgsqlIndexBuilderExtensions` at model time through `Configure(EntityTypeBuilder)` because `IndexMethod` is the translatable-by-construction `HasMethod` vocabulary; the untranslatable `diskann`/`bm25` builds are the dedicated `DiskAnn`/`Bm25` raw-DDL cases, never an `IndexMethod`. `TemporalShape` is the temporal-key shape vocabulary and `PartitionStrategy` the range/list/hash partition vocabulary. The server-tier provisioning cases — `Hypertable`, `ContinuousAggregate`, `RetentionPolicy`, `ColumnstorePolicy`, `Partition`, `AttachPartition`, `DiskAnn`, `Bm25` — fold into the same union so the TimescaleDB steps, the PG18 native partition steps, and the diskann/bm25 index builds are arms on this one declaration owner, never a parallel `Provision` static surface.
- Cases: Extension, Index, Exclusion, TemporalKey, Partition, AttachPartition, Check, JsonSchemaCheck, Composite, Enum, Hypertable, ContinuousAggregate, RetentionPolicy, ColumnstorePolicy, DiskAnn, Bm25. The `Index` case folds the model-translatable methods (`btree`/`hash`/`gin`/`gist`/`brin`/`hnsw`/`ivfflat`) through `Configure(EntityTypeBuilder)` while the untranslatable `diskann`/`bm25` builds are the dedicated `DiskAnn`/`Bm25` cases through `ProvisionSql`; `Exclusion` rides btree_gist range non-overlap; `TemporalKey` rides PG18 WITHOUT OVERLAPS primary/unique/foreign keys with a lock-light two-step on a populated table; `Partition`/`AttachPartition` declare native `CREATE TABLE ... PARTITION BY`/`PARTITION OF` with no EF translator; `Check`/`JsonSchemaCheck` declare free-form and pg_jsonschema `jsonb_matches_schema` table invariants; `Composite`/`Enum` declare native PostgreSQL composite and enum types; and the eight server-tier provisioning steps project `ProvisionSql` `Fin<string>` carrying the catalogued `SELECT`-function-versus-`CALL`-procedure verb so the `DiskAnn` `<#>`-on-`plain` reject arm and the `ColumnstorePolicy` `CALL` spelling both survive.
- Entry: `public static ModelBuilder Declare(ModelBuilder model)` folds the non-preload extension and enum rows into model annotations; `public static EntityTypeBuilder Configure(EntityTypeBuilder entity, Seq<Index> indexes)` folds each model-translatable `Index` row onto `HasIndex(params string[]).HasMethod(...).HasOperators(...).IncludeProperties(...).AreNullsDistinct(...).HasNullSortOrder(...).UseCollation(...).IsCreatedConcurrently(...).HasFilter(...).HasStorageParameter(...)` so a translatable index is model data driven entirely by the row's string columns, never a hand-spelled `CREATE INDEX`; `public static MigrationBuilder Migrate(MigrationBuilder migration)` emits composite `CREATE TYPE` DDL, preload-gated `CREATE EXTENSION` DDL, and the temporal-key two-step, check, and JSON-schema SQL; `public static Fin<MigrationBuilder> Validate(MigrationBuilder migration, Func<string, bool> extensionAvailable)` degrades each fallback-bearing extension to its app-side path when the deploy image lacks the pgrx-compiled extension and fails typed otherwise; `public static Fin<MigrationBuilder> Sql(MigrationBuilder migration, Seq<SchemaDdl> steps)` is the one server-tier raw-SQL provisioning fold — it traverses each provisioning case's `ProvisionSql` `Fin<string>` projection, threads the `SuppressTransaction` flag onto `MigrationBuilder.Sql` for the non-transactional `CREATE INDEX CONCURRENTLY` diskann build, and surfaces a rejected `DiskAnn` as `Fin.Fail` to the deploy-gate caller before any `Sql()` lands.
- Auto: `HasPostgresExtension` annotations flow through `AlterDatabaseOperation` into generated migration DDL — `CreatePostgresExtensionOperation` is the deleted phantom spelling. `HasPostgresEnum` annotations flow through `PostgresEnum` into the same `AlterDatabaseOperation` so a native enum column emits a `CREATE TYPE ... AS ENUM` step rather than a check-constrained text column. A model-translatable `Index` row never reaches `MigrationBuilder.Sql` — its `Configure` fold lands `CREATE INDEX ... USING <method> (...) INCLUDE (...) [WITH (...)] [WHERE ...]` through the provider's `NpgsqlMigrationsSqlGenerator`, so a covering, partial, concurrent, or operator-class index is generated migration DDL not deploy-time hand SQL. A `TemporalKey`, `Partition`, `AttachPartition`, `Check`, `JsonSchemaCheck`, and `Composite` row emit through `MigrationBuilder.Sql` because the WITHOUT OVERLAPS clause, the `CREATE TABLE ... PARTITION BY`/`PARTITION OF` form, the `jsonb_matches_schema` predicate, and the `CREATE TYPE ... AS (...)` composite (which has no model-annotation analogue to `HasPostgresEnum`) have no first-party EF translator; preload-gated rows (`pg_search`) emit their `CreateSql` through `Migrate` because `HasPostgresExtension` cannot encode the `shared_preload_libraries` prerequisite, and `CASCADE` rows (`vectorscale`) emit `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE` so the `vector` dependency installs in one step.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, Pgvector.EntityFrameworkCore, Pgvector, Npgsql, JsonSchema.Net, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `SchemaDdl` row — a new extension is one `Extension` entry (access method, preload gate, cascade flag, fallback path are columns on the same row), a new index family is one `Index` row carrying its `IndexMethod`, operator classes, `Include` covering columns, `NullsDistinct`, ordering, `Collation`, `Predicate`, `Concurrent`, and `Storage` build-option map, a temporal-versioned table is one `TemporalKey` row, a partitioned table is one `Partition` row plus its `AttachPartition` rows, a server-validated document lane is one `JsonSchemaCheck` row, a table invariant is one `Check` row, a native pg enum is one `Enum` entry, a native pg composite type is one `Composite` entry (its `Attributes` field DDL and round-trip registration on the same row), a new TimescaleDB provisioning concern is one of the four timescale cases, and a new access-method index build is one of the two raw-DDL index cases — all arms on the one union, the model-translatable `Index` rows folded by `Configure` and the raw-DDL rows by `Sql`; a new index access method the provider learns to translate is one `IndexMethod` row consumed by `Configure` with zero case churn; zero new surface.
- Boundary: extension capability is one `Extension` row with its full DDL story in columns — `vectorscale` rides `AccessMethod: "diskann"` and `Cascade: true` so `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE` pulls the `vector` dependency, and the query-time `diskann.query_search_list_size`/`diskann.query_rescore` GUCs belong to the search lane not this declaration row; `pg_search` rides `AccessMethod: "bm25"` and `PreloadGated: true` so it never enters `HasPostgresExtension` (`shared_preload_libraries = 'pg_search'` must precede `CREATE EXTENSION pg_search`, so `Migrate` emits it), and the `pdb.*` query builders, `|||`/`&&&`/`===`/`###` column operators, `pdb.score`/`pdb.snippet` projections, and `::pdb.fuzzy`/`::pdb.boost` casts belong to the search lane — the removed `paradedb.*` namespace is the deleted phantom spelling, every search predicate uses the `pdb.*` namespace. The self-provisioned non-preload `pg_jsonschema` carries `Fallback: "Json.Schema.JsonSchema.Evaluate"` so `Validate` degrades the document lane to JsonSchema.Net in-process evaluation when the deploy image lacks the pgrx-compiled extension, never silently dropping validation. The `Surface` column states the driver-native cost of each row and built-in ranges and multiranges map to `NpgsqlRange<T>` with zero extension entry.
- Boundary: index design is model data, not deploy-time hand SQL — the `Index` case folds `NpgsqlIndexBuilderExtensions` through `Configure(EntityTypeBuilder)` for every access method the provider translates (`btree`/`hash`/`gin`/`gist`/`brin`/`hnsw`/`ivfflat`), so `HasMethod`, `HasOperators(params string[])`, `IncludeProperties`, `AreNullsDistinct`, `HasNullSortOrder(params NullSortOrder[])`, `UseCollation`, `IsCreatedConcurrently`, the `HasFilter` partial predicate, and `HasStorageParameter` build options each ride a column on the row and emit through the provider's `NpgsqlMigrationsSqlGenerator` — a hand-spelled `CREATE INDEX` string for a translatable method is the deleted form. An hnsw vector index rides `IndexMethod.Hnsw` with `m`/`ef_construction` in the `Storage` map and `vector_cosine_ops` in `Operators`; an ivfflat index rides `IndexMethod.IvfFlat` with `lists`; a brin index rides `IndexMethod.Brin` with `pages_per_range` for an append-only time column. The `diskann`/`bm25` access methods have no `HasMethod` translation, so they are the dedicated `DiskAnn`/`Bm25` cases routing their build DDL through `ProvisionSql`, never an `Index` row. A compound `Index` row leading with the tenant/partition discriminant serves both keyset cursors and single-column filters through the PG18 automatic B-tree skip scan so a redundant single-column index is the deleted form; `Include` columns leave the key as covering payload, `NullsDistinct: false` is the single-null uniqueness law deleting the partial-index workaround for that one pathology, and a true `Predicate` clause is the partial index for a hot row subset.
- Boundary: `Exclusion` rows ride `EXCLUDE USING gist` for range non-overlap with a typed `IndexMethod.Gist` access method — GiST is the canonical non-referencable exclusion form (PostgreSQL admits only `gist`/`spgist` under `EXCLUDE`, never `btree`/`hash`/`brin`/`gin`), so `Method` is constrained to `Gist` at construction and the scalar equality parts ride the `btree_gist` extension; `spgist` enters the `IndexMethod` vocabulary as one row the day a real SP-GiST exclusion landmark exists, not as a present spelling. `TemporalKey` rows ride the PG18 WITHOUT OVERLAPS shape over a `tstzrange` valid-time column GiST-backed by btree_gist for scalar equality — `PRIMARY KEY (id, valid_period WITHOUT OVERLAPS)`, `UNIQUE (... WITHOUT OVERLAPS)`, and `FOREIGN KEY (cust_id, PERIOD valid_period) REFERENCES parent (id, PERIOD parent_period)` are the bitemporal-versioning structural fence for geospatial-sync and multi-tenant history; a `TemporalKey` added to a populated table rides the `Schema/migration#LOCK_LIGHT` `LockLightStep.TwoStep` (`ADD ... NOT VALID` then `VALIDATE CONSTRAINT`) carried through `Populated: true` so the validate scan takes no full-table AccessExclusiveLock, and a destructive temporal-key change rides the `Version/retention#DESTRUCTIVE_GATE` `DestructiveUnapproved` retention gate. `Partition` declares the PG18 native `CREATE TABLE ... PARTITION BY RANGE|LIST|HASH` parent (the partitioned table is created whole because `ALTER TABLE ... PARTITION BY` does not exist) and `AttachPartition` declares the `CREATE TABLE ... PARTITION OF ... FOR VALUES` child (or `... DEFAULT` for the catch-all) over the `OpLogEntry` time axis — neither has an EF translator, so they ride `MigrationBuilder.Sql`, the parent's per-partition lifecycle advance is the `pg_partman` companion's idempotent `run_maintenance` owned at `Store/profiles#PROVISIONING_ROWS`, and a standalone child table followed by `ALTER TABLE ... ATTACH PARTITION` is the deleted form because the declarative `PARTITION OF` creates the child already attached. `Check` rows declare free-form table invariants and `JsonSchemaCheck` rows declare the `CHECK (jsonb_matches_schema(<schema>, <column>))` document-lane invariant so the document lane validates server-side, with `Validate` carrying the in-process fallback when the server-side extension is absent.
- Boundary: the postgis row makes NetTopologySuite the pg boundary projection of the canonical proto wire geometry; earthdistance is rejected — the postgis row owns geodesy. `Composite` rows own a native PostgreSQL composite type end-to-end from one row — `Migrate` emits its `CREATE TYPE {Name} AS ({Attributes})` (no Npgsql EF composite-type model annotation exists, so the type DDL rides raw `MigrationBuilder.Sql` exactly as `Enum` would absent `HasPostgresEnum`) and `MapComposites` folds the round-trip registration through `MapComposite(Type, name)`, so the type declaration and its reader trace to one row, never a per-type hand-written reader beside a hand-spelled `CREATE TYPE`. `Enum` rows declare native PostgreSQL enum types symmetric with `Composite` — `MapEnums` folds each onto the data-source builder through the generic `MapEnum<TEnum>(pgName, translator)` resolver while `Declare` folds `HasPostgresEnum<TEnum>(name)` (the enum's type DDL flows through `PostgresEnum`/`AlterDatabaseOperation`, so it needs no raw `Sql` arm) so the model annotation and the type registration trace to one row, never a per-enum hand-written `MapEnum` call beside a hand-written check constraint, and the `EnableUnmappedTypes` builder column on the pg profile row admits enum-as-text round-trips without an `Enum` entry where a native type is unwarranted. The composite and enum sets are empty until a real landmark exists, the cases are shaped for the family they absorb.
- Boundary: the server-tier provisioning cases (`Hypertable`/`ContinuousAggregate`/`RetentionPolicy`/`ColumnstorePolicy`/`Partition`/`AttachPartition`/`DiskAnn`/`Bm25`) ride `MigrationBuilder.Sql` through the one `Sql` fold because none carries a first-party EF translator. The TimescaleDB steps project the catalogued `SELECT`-function-versus-`CALL`-procedure verb (`api-timescaledb.md` `[EMISSION_LAW]`): `create_hypertable`/`add_continuous_aggregate_policy`/`add_retention_policy` are `SELECT` functions returning a job_id, but `add_columnstore_policy` is a `CALL` procedure — a `ColumnstorePolicy` emitting `SELECT add_columnstore_policy(...)` is the faulted spelling, so the `<ALTER> ... SET (timescaledb.enable_columnstore=true, segmentby, orderby)` enable arm precedes the `CALL add_columnstore_policy(...)` schedule arm, the enable keyword carried by the `ColumnstoreRelation` row (`ALTER TABLE` for a raw hypertable, `ALTER MATERIALIZED VIEW` for a continuous-aggregate rollup) so compressing a cagg tile source emits the cagg-correct keyword rather than the table-only `ALTER TABLE`; the `ContinuousAggregate` carries its bucketed `Measures` projection (not a frozen `count(*)`) and every optional argument binds by `=>` name with `if_not_exists => TRUE` for idempotent re-run. The `DiskAnn` step projects `CREATE INDEX CONCURRENTLY ... USING diskann` carrying the full `DiskAnnOptions` build axis against `vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`, rejects the `<#>` inner-product operator on a `storage_layout=plain` build as `Fin.Fail` (SBQ requires `memory_optimized`), and carries `SuppressTransaction: true` for the non-transactional concurrent build; the `Bm25` step projects the one-per-table `CREATE INDEX ... USING bm25` keyed by the `key_field` UNIQUE/primary column listed first. The `Bm25Predicate` `@@@`/`pdb.*` column-operator-and-builder query-projection axis stays its own owner consumed at `Store/provisioning#SCHEMA_DDL_FOLD` and the search lane and never folds into this declaration union — only the index-build DDL collapses here, never the query-time predicate builders, and the preload-gated companions verify through the `Store/profiles#PROVISIONING_ROWS` `PreloadProbe` before these steps run.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class TemporalShape {
    public static readonly TemporalShape PrimaryKey = new("primary-key");
    public static readonly TemporalShape Unique = new("unique");
    public static readonly TemporalShape ForeignKey = new("foreign-key");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class PartitionStrategy {
    public static readonly PartitionStrategy Range = new("RANGE");
    public static readonly PartitionStrategy List = new("LIST");
    public static readonly PartitionStrategy Hash = new("HASH");
}

// --- [INDEX_METHOD] ----------------------------------------------------------------------
// The access methods `NpgsqlIndexBuilderExtensions.HasMethod` translates — the `Index` row composes
// each at model time. The untranslatable `diskann`/`bm25` builds are the dedicated `DiskAnn`/`Bm25`
// cases, never an `IndexMethod`, so this vocabulary is translatable by construction.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class IndexMethod {
    public static readonly IndexMethod BTree = new("btree");
    public static readonly IndexMethod Hash = new("hash");
    public static readonly IndexMethod Gin = new("gin");
    public static readonly IndexMethod Gist = new("gist");
    public static readonly IndexMethod Brin = new("brin");
    public static readonly IndexMethod Hnsw = new("hnsw");
    public static readonly IndexMethod IvfFlat = new("ivfflat");
}

[SmartEnum<string>]
public sealed partial class DiskAnnOps {
    public static readonly DiskAnnOps Cosine = new("vector_cosine_ops", "<=>");
    public static readonly DiskAnnOps L2 = new("vector_l2_ops", "<->");
    public static readonly DiskAnnOps InnerProduct = new("vector_ip_ops", "<#>");
    public string Operator { get; }
    private DiskAnnOps(string key, string @operator) : this(key) => Operator = @operator;
}

[SmartEnum<string>]
public sealed partial class DiskAnnLayout {
    public static readonly DiskAnnLayout MemoryOptimized = new("memory_optimized");
    public static readonly DiskAnnLayout Plain = new("plain");
}

// The relation kind a `ColumnstorePolicy` enables: a raw hypertable rides `ALTER TABLE`, a
// continuous-aggregate rollup rides `ALTER MATERIALIZED VIEW` — the `Alter` column is the keyword.
[SmartEnum<string>]
public sealed partial class ColumnstoreRelation {
    public static readonly ColumnstoreRelation Hypertable = new("table", "ALTER TABLE");
    public static readonly ColumnstoreRelation Aggregate = new("materialized-view", "ALTER MATERIALIZED VIEW");
    public string Alter { get; }
    private ColumnstoreRelation(string key, string alter) : this(key) => Alter = alter;
}

public sealed record DiskAnnOptions(
    DiskAnnLayout StorageLayout, int NumNeighbors, int SearchListSize,
    double MaxAlpha, int NumDimensions, int NumBitsPerDimension) {
    public static readonly DiskAnnOptions Default =
        new(DiskAnnLayout.MemoryOptimized, NumNeighbors: 50, SearchListSize: 100,
            MaxAlpha: 1.2, NumDimensions: 0, NumBitsPerDimension: 0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SchemaDdl {
    private SchemaDdl() { }

    public sealed record Extension(
        string Name,
        string Surface,
        Option<string> AccessMethod = default,
        bool PreloadGated = false,
        bool Cascade = false,
        Option<string> Fallback = default) : SchemaDdl {
        public string CreateSql => Cascade
            ? $"CREATE EXTENSION IF NOT EXISTS {Name} CASCADE"
            : $"CREATE EXTENSION IF NOT EXISTS {Name}";
    }

    // Every `IndexMethod` is `HasMethod`-translatable, so `Configure` composes the whole row through
    // `NpgsqlIndexBuilderExtensions` — no raw `CREATE INDEX` string. `Storage` carries
    // `m`/`ef_construction` (hnsw), `lists` (ivfflat), `pages_per_range` (brin), or `fillfactor`.
    public sealed record Index(
        string Table,
        Seq<string> Columns,
        IndexMethod Method,
        Seq<string> Operators = default,
        Seq<string> Include = default,
        Option<bool> NullsDistinct = default,
        Seq<NullSortOrder> NullSort = default,
        Option<string> Predicate = default,
        bool Concurrent = false,
        Seq<string> Collation = default,
        Map<string, object> Storage = default) : SchemaDdl {
        // `Columns` names the index columns by string, so `HasIndex(params string[])` keeps the row
        // fully data-driven — no per-row property-expression selector.
        public IndexBuilder Configure(EntityTypeBuilder entity) {
            var index = entity.HasIndex([.. Columns]).HasMethod(Method.Key);
            _ = Operators.IsEmpty ? index : index.HasOperators([.. Operators]);
            _ = Include.IsEmpty ? index : index.IncludeProperties([.. Include]);
            _ = NullsDistinct.Match(Some: distinct => index.AreNullsDistinct(distinct), None: () => index);
            _ = Collation.IsEmpty ? index : index.UseCollation([.. Collation]);
            _ = NullSort.IsEmpty ? index : index.HasNullSortOrder([.. NullSort]);
            _ = Predicate.Match(Some: clause => index.HasFilter(clause), None: () => index);
            _ = Concurrent ? index.IsCreatedConcurrently() : index;
            return Storage.Fold(index, static (built, kv) => built.HasStorageParameter(kv.Key, kv.Value));
        }
    }
    // `EXCLUDE` admits only `gist`/`spgist` access methods; `Gist` is the operative form and the only one
    // in the vocabulary, so `Method` defaults to it and a `gin`/`brin`/`btree` exclusion is unspellable.
    public sealed record Exclusion(string Table, string Predicate, IndexMethod? Method = null) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD EXCLUDE USING {(Method ?? IndexMethod.Gist).Key} ({Predicate})";
    }
    // A temporal constraint on a populated table rides the `LockLight` two-step so the validate scan
    // skips the full-table AccessExclusiveLock; an empty table validates in one `ADD` step.
    public sealed record TemporalKey(
        string Table,
        Seq<string> Columns,
        string Period,
        TemporalShape Shape,
        Option<(string Table, Seq<string> Columns, string Period)> References,
        bool Populated = false,
        Option<string> Constraint = default) : SchemaDdl {
        private string Definition => Shape.Switch(
            state: this,
            primaryKey: static self => $"PRIMARY KEY ({string.Join(", ", self.Columns)}, {self.Period} WITHOUT OVERLAPS)",
            unique: static self => $"UNIQUE ({string.Join(", ", self.Columns)}, {self.Period} WITHOUT OVERLAPS)",
            foreignKey: static self => self.References.Match(
                Some: parent => $"FOREIGN KEY ({string.Join(", ", self.Columns)}, PERIOD {self.Period}) REFERENCES {parent.Table} ({string.Join(", ", parent.Columns)}, PERIOD {parent.Period})",
                None: () => string.Empty));
        private string Named => Constraint.Match(Some: name => $"CONSTRAINT {name} {Definition}", None: () => Definition);
        public string Sql => Definition.Length == 0 ? string.Empty : $"ALTER TABLE {Table} ADD {Named}";
        // The `WITHOUT OVERLAPS` body is a primary/unique/foreign-key constraint, not a `CHECK` predicate,
        // so the populated-table two-step mints the pre-rendered `Schema/migration#LOCK_LIGHT`
        // `LockLightStep.TwoStep(AddSql, ValidateSql)` directly — the `NotValidConstraint` arm renders its
        // own `CHECK (predicate)` ADD and cannot carry this constraint body.
        public Option<LockLightStep> LockLight =>
            Populated && Constraint.Case is string name && Definition.Length > 0
                ? Some<LockLightStep>(new LockLightStep.TwoStep($"ALTER TABLE {Table} ADD {Named} NOT VALID", $"ALTER TABLE {Table} VALIDATE CONSTRAINT {name}"))
                : None;
    }
    // PG18 native declarative partitioning has no EF translator and cannot be added by `ALTER TABLE`,
    // so the partitioned parent is created whole by raw DDL (`Definition` is its column body), each
    // child attaches its value bound, and per-partition lifecycle is the `pg_partman` companion.
    public sealed record Partition(string Table, PartitionStrategy Strategy, Seq<string> Keys, string Definition) : SchemaDdl {
        public string Sql => $"CREATE TABLE IF NOT EXISTS {Table} ({Definition}) PARTITION BY {Strategy.Key} ({string.Join(", ", Keys)})";
    }
    public sealed record AttachPartition(string Parent, string Child, string Bounds) : SchemaDdl {
        public string Sql => Bounds.Length == 0
            ? $"CREATE TABLE IF NOT EXISTS {Child} PARTITION OF {Parent} DEFAULT"
            : $"CREATE TABLE IF NOT EXISTS {Child} PARTITION OF {Parent} FOR VALUES {Bounds}";
    }
    public sealed record Check(string Table, string Constraint, string Predicate) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD CONSTRAINT {Constraint} CHECK ({Predicate})";
    }
    public sealed record JsonSchemaCheck(string Table, string Column, string Constraint, string Schema) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD CONSTRAINT {Constraint} CHECK (jsonb_matches_schema('{Schema}', {Column}))";
    }
    // Npgsql EF carries no composite-type model annotation (`HasPostgresEnum`/`HasPostgresRange` exist,
    // a composite analogue does not), so the `CREATE TYPE … AS (…)` rides raw `MigrationBuilder.Sql` from
    // the `Attributes` field-DDL while `MapComposite` registers the round-trip — symmetric with `Enum`.
    public sealed record Composite(string Name, Type ClrType, Seq<(string Field, string PgType)> Attributes = default) : SchemaDdl {
        public string Sql => $"CREATE TYPE {Name} AS ({string.Join(", ", Attributes.Map(static a => $"{a.Field} {a.PgType}"))})";
    }
    public sealed record Enum(string Name, Type ClrType, Func<NpgsqlDataSourceBuilder, string, INpgsqlNameTranslator?, NpgsqlDataSourceBuilder> Map, Func<ModelBuilder, ModelBuilder> Annotate) : SchemaDdl {
        public static Enum Of<TEnum>(string name) where TEnum : struct, System.Enum =>
            new(name, typeof(TEnum),
                static (builder, pgName, translator) => builder.MapEnum<TEnum>(pgName, translator),
                static model => model.HasPostgresEnum<TEnum>(name));
        public static Enum Labelled(string name, params string[] labels) =>
            new(name, typeof(string),
                (builder, _, _) => builder,
                model => model.HasPostgresEnum(name, labels));
    }

    public sealed record Hypertable(string Table, string TimeColumn, string Interval) : SchemaDdl;
    public sealed record ContinuousAggregate(
        string View, string Source, string Bucket, string TimeColumn, string Measures,
        string StartOffset, string EndOffset, string Schedule) : SchemaDdl;
    public sealed record RetentionPolicy(string Table, string DropAfter) : SchemaDdl;
    // `Relation` selects the columnstore-enable keyword: a raw hypertable enables through `ALTER TABLE`,
    // a continuous-aggregate rollup through `ALTER MATERIALIZED VIEW` (`api-timescaledb.md` [04]); the
    // `add_columnstore_policy` CALL is identical for both because it targets the relation by name.
    public sealed record ColumnstorePolicy(string Table, string SegmentBy, string OrderBy, string After, ColumnstoreRelation Relation) : SchemaDdl;
    public sealed record DiskAnn(string Index, string Table, string Column, DiskAnnOps Ops, DiskAnnOptions Options) : SchemaDdl;
    public sealed record Bm25(string Index, string Table, string KeyField, Seq<string> TextColumns) : SchemaDdl;

    // `CREATE INDEX CONCURRENTLY` cannot run in a migration transaction block, so the diskann build
    // carries `SuppressTransaction` for the `Sql` fold to thread; every other step is transactional.
    public bool SuppressTransaction => this is DiskAnn;

    public Fin<string> ProvisionSql() =>
        this switch {
            Hypertable h => Fin<string>.Succ(
                $"SELECT create_hypertable('{h.Table}', by_range('{h.TimeColumn}', INTERVAL '{h.Interval}'), create_default_indexes => TRUE, migrate_data => FALSE, if_not_exists => TRUE)"),
            ContinuousAggregate c => Fin<string>.Succ(
                $"CREATE MATERIALIZED VIEW {c.View} WITH (timescaledb.continuous) AS SELECT time_bucket('{c.Bucket}', {c.TimeColumn}) AS bucket, {c.Measures} FROM {c.Source} GROUP BY 1 WITH NO DATA; " +
                $"SELECT add_continuous_aggregate_policy('{c.View}', start_offset => INTERVAL '{c.StartOffset}', end_offset => INTERVAL '{c.EndOffset}', schedule_interval => INTERVAL '{c.Schedule}', if_not_exists => TRUE)"),
            RetentionPolicy r => Fin<string>.Succ(
                $"SELECT add_retention_policy('{r.Table}', drop_after => INTERVAL '{r.DropAfter}', if_not_exists => TRUE)"),
            // `add_columnstore_policy` is a CALL procedure, never a SELECT function — the enable arm
            // (ALTER ... SET) precedes the schedule arm (api-timescaledb.md [EMISSION_LAW]); the enable
            // keyword is `ALTER TABLE` for a hypertable and `ALTER MATERIALIZED VIEW` for a cagg rollup.
            ColumnstorePolicy c => Fin<string>.Succ(
                $"{c.Relation.Alter} {c.Table} SET (timescaledb.enable_columnstore = true, timescaledb.segmentby = '{c.SegmentBy}', timescaledb.orderby = '{c.OrderBy}'); " +
                $"CALL add_columnstore_policy('{c.Table}', after => INTERVAL '{c.After}', if_not_exists => TRUE)"),
            Partition p => Fin<string>.Succ(p.Sql),
            AttachPartition a => Fin<string>.Succ(a.Sql),
            DiskAnn d => d is { Ops.Key: "vector_ip_ops", Options.StorageLayout.Key: "plain" }
                ? Fin<string>.Fail(Error.New($"<diskann-ip-on-plain-layout:{d.Index}>"))
                : Fin<string>.Succ(
                    $"CREATE INDEX CONCURRENTLY {d.Index} ON {d.Table} USING diskann ({d.Column} {d.Ops.Key}) " +
                    $"WITH (storage_layout = '{d.Options.StorageLayout.Key}', num_neighbors = {d.Options.NumNeighbors}, " +
                    $"search_list_size = {d.Options.SearchListSize}, max_alpha = {d.Options.MaxAlpha.ToString(CultureInfo.InvariantCulture)}, " +
                    $"num_dimensions = {d.Options.NumDimensions}, num_bits_per_dimension = {d.Options.NumBitsPerDimension})"),
            Bm25 b => Fin<string>.Succ(
                $"CREATE INDEX {b.Index} ON {b.Table} USING bm25 ({string.Join(", ", b.TextColumns.Prepend(b.KeyField))}) " +
                $"WITH (key_field = '{b.KeyField}')"),
            _ => Fin<string>.Fail(Error.New("<schema-ddl-not-a-provisioning-step>")),
        };

    public static readonly Seq<Composite> Composites = Seq<Composite>();

    public static readonly Seq<Enum> Enums = Seq<Enum>();

    public static readonly Seq<Extension> Extensions = Seq(
        new Extension("pg_trgm", Surface: "sql-functions"),
        new Extension("btree_gin", Surface: "ddl-only"),
        new Extension("btree_gist", Surface: "ddl-only"),
        new Extension("citext", Surface: "string"),
        new Extension("hstore", Surface: "Dictionary<string,string>"),
        new Extension("ltree", Surface: "LTree"),
        new Extension("intarray", Surface: "int[]"),
        new Extension("pgcrypto", Surface: "sql-functions"),
        new Extension("fuzzystrmatch", Surface: "sql-functions"),
        new Extension("unaccent", Surface: "sql-functions"),
        new Extension("cube", Surface: "NpgsqlCube"),
        new Extension("tablefunc", Surface: "sql-functions"),
        new Extension("amcheck", Surface: "sql-functions"),
        new Extension("pg_prewarm", Surface: "sql-functions"),
        new Extension("pg_visibility", Surface: "sql-functions"),
        new Extension("pg_logicalinspect", Surface: "sql-functions"),
        new Extension("postgres_fdw", Surface: "ddl-only"),
        new Extension("pg_jsonschema", Surface: "sql-functions", Fallback: "Json.Schema.JsonSchema.Evaluate"),
        new Extension("vector", Surface: "Pgvector.Vector"),
        new Extension("postgis", Surface: "NetTopologySuite.Geometry"),
        new Extension("vectorscale", Surface: "Pgvector.Vector", AccessMethod: "diskann", Cascade: true),
        new Extension("pg_search", Surface: "bm25-search", AccessMethod: "bm25", PreloadGated: true));

    public static readonly Seq<TemporalKey> TemporalKeys = Seq<TemporalKey>();

    public static readonly Seq<Check> Checks = Seq<Check>();

    public static readonly Seq<JsonSchemaCheck> JsonSchemaChecks = Seq<JsonSchemaCheck>();

    public static NpgsqlDataSourceBuilder MapComposites(NpgsqlDataSourceBuilder builder) =>
        Composites.Fold(builder, static (mapper, row) => mapper.MapComposite(row.ClrType, row.Name));

    public static NpgsqlDataSourceBuilder MapEnums(NpgsqlDataSourceBuilder builder) =>
        Enums.Fold(builder, static (mapper, row) => row.Map(mapper, row.Name, null));

    public static ModelBuilder Declare(ModelBuilder model) =>
        Enums.Fold(
            Extensions.Filter(static row => !row.PreloadGated).Fold(model, static (builder, row) => builder.HasPostgresExtension(row.Name)),
            static (builder, row) => row.Annotate(builder));

    public static EntityTypeBuilder Configure(EntityTypeBuilder entity, Seq<Index> indexes) =>
        indexes.Fold(entity, static (built, row) => { row.Configure(built); return built; });

    // Composite `CREATE TYPE` emits first (no EF annotation exists), then preload-gated `CREATE EXTENSION`,
    // then the populated-table temporal-key two-steps, then free-form and JSON-schema checks.
    public static MigrationBuilder Migrate(MigrationBuilder migration) =>
        JsonSchemaChecks.Fold(
            Checks.Fold(
                TemporalKeys.Filter(static row => row.Sql.Length > 0).Fold(
                    Extensions.Filter(static row => row.PreloadGated).Fold(
                        Composites.Fold(migration, static (builder, row) => { builder.Sql(row.Sql); return builder; }),
                        static (builder, row) => { builder.Sql(row.CreateSql); return builder; }),
                    static (builder, row) => row.LockLight.Match(
                        Some: step => step.Emit.Fold(builder, static (b, emit) => { b.Sql(emit.Sql, suppressTransaction: emit.SuppressTransaction); return b; }),
                        None: () => { builder.Sql(row.Sql); return builder; })),
                static (builder, row) => { builder.Sql(row.Sql); return builder; }),
            static (builder, row) => { builder.Sql(row.Sql); return builder; });

    public static Fin<MigrationBuilder> Validate(MigrationBuilder migration, Func<string, bool> extensionAvailable) =>
        Extensions
            .Filter(static row => row.Fallback.IsSome)
            .Fold(
                Fin.Succ(migration),
                (acc, row) => acc.Bind(builder =>
                    extensionAvailable(row.Name)
                        ? Fin.Succ(builder)
                        : row.Fallback.Match(
                            Some: _ => Fin.Succ(builder),
                            None: () => Fin.Fail<MigrationBuilder>(SchemaFault.Create($"<extension-unavailable:{row.Name}>")))));

    public static Fin<MigrationBuilder> Sql(MigrationBuilder migration, Seq<SchemaDdl> steps) =>
        steps.Map(static step => step.ProvisionSql().Map(sql => (Sql: sql, Suppress: step.SuppressTransaction)))
            .Traverse(identity).As()
            .Map(rendered => rendered.Fold(migration, static (builder, step) => { builder.Sql(step.Sql, suppressTransaction: step.Suppress); return builder; }));
}
```


# [PERSISTENCE_SCHEMA_DDL]

Generated-column and declarative DDL surface for every store the suite opens. `DerivedColumn` rows decide stored-versus-virtual generated columns and emit `HasComputedColumnSql(sql, stored)` plus their co-located CHECK constraints. `SchemaDdl` rows declare the PostgreSQL extension, index, exclusion, temporal-key, check, json-schema, composite, and native-enum surface as one `[Union]` over one `Declare`/`Migrate`/`Validate` fold, so the native-type DDL set is one owner, not a scatter of per-feature emitters.

## [1]-[INDEX]

- [1]-[GENERATED_COLUMNS]: stored-versus-virtual decision, computed-column emission, and co-located check constraints.
- [2]-[EXTENSION_DDL]: extension, index, exclusion, temporal-key, check, json-schema, composite, and native-enum DDL.

## [2]-[GENERATED_COLUMNS]

- Owner: `DerivedColumn` row record carrying the stored-versus-virtual law plus its co-located CHECK constraint; `ColumnInvariant` row record for table-level check constraints that are not column-derived.
- Entry: `public bool Stored` — derived; `Replicated || Indexed` is the whole decision; `public PropertyBuilder<T> Apply(PropertyBuilder<T> property)` emits `HasComputedColumnSql(Sql, stored: Stored)` and `public EntityTypeBuilder<T> Constrain(EntityTypeBuilder<T> entity)` folds each `ColumnInvariant` into `ToTable(t => t.HasCheckConstraint(name, sql))`.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Sqlite
- Growth: one `DerivedColumn` row per derived projection; one `ColumnInvariant` row per table-level check; zero new surface.
- Boundary: the pg provider emits VIRTUAL generated columns by default at the Npgsql-v10 provider floor over PG18 — `HasComputedColumnSql(sql, stored: false)` emits VIRTUAL when `Stored` is false and PG18 is the target — and the sqlite provider emits its own VIRTUAL/STORED form. A STORED column exists only where the `Stored` flag derives true — a hot predicate on a virtual column is the promotion signal to stored-plus-index. A replicated row is STORED so the logical-replication publication's `publish_generated_columns` field carries it; an indexed row is STORED so the index reads materialized bytes; tsvector search columns are `DerivedColumn` rows whose instances land on their owning lane through `HasGeneratedTsVectorColumn` rather than a hand-spelled `to_tsvector` computed column where the language config is a model fact. A derived projection computed in application code beside a store-computed twin is the deleted pattern — one `Sql` expression owns the derivation. Document-shape invariants and range/sign bounds are `ColumnInvariant` rows so a `CHECK` lands as a model fact emitted through `HasCheckConstraint`, never deploy-time hand DDL beside the migration set; a geodesic-planar twin rides the generated-column law as a stored planar projection feeding the hot planar predicate while the geography column holds geodesic truth.

```csharp signature
public sealed record ColumnInvariant(string Name, string Sql);

public sealed record DerivedColumn(string Table, string Column, string Sql, bool Replicated, bool Indexed, Seq<ColumnInvariant> Checks = default) {
    public bool Stored => Replicated || Indexed;

    public PropertyBuilder<T> Apply<T>(PropertyBuilder<T> property) =>
        property.HasComputedColumnSql(Sql, stored: Stored);

    public EntityTypeBuilder<TEntity> Constrain<TEntity>(EntityTypeBuilder<TEntity> entity) where TEntity : class =>
        Checks.Fold(entity, static (builder, check) =>
            builder.ToTable(table => table.HasCheckConstraint(check.Name, check.Sql)));
}
```


## [3]-[EXTENSION_DDL]

- Owner: `SchemaDdl` `[Union]` declaration-row family with the frozen `Extensions` row set; the `Extension` case carries `AccessMethod`, `PreloadGated`, `Cascade`, and `Fallback` columns so one row owns the extension's install DDL, its index access method, its `shared_preload_libraries` gate, and its app-side degradation path; the `Index` case carries method, operator-class, `Include`, `NullsDistinct`, and a `With` build-option map; `TemporalShape` is the temporal-key shape vocabulary.
- Cases: Extension, Index, Exclusion, TemporalKey, Check, JsonSchemaCheck, Composite, Enum — extension declarations with access-method/preload/cascade/fallback metadata, method-and-operator-class index rows carrying a `With` option map for `diskann`/`bm25`/`hnsw` build parameters plus `Include` covering columns and `NullsDistinct` single-null uniqueness, btree_gist exclusion-constraint rows, PG18 WITHOUT OVERLAPS temporal primary-key and foreign-key rows, free-form CHECK rows, pg_jsonschema document-validation CHECK rows, PostgreSQL composite-type declarations, native PostgreSQL enum-type declarations.
- Entry: `public static ModelBuilder Declare(ModelBuilder model)` is the total fold of the non-preload extension and enum rows into model annotations; `public static MigrationBuilder Migrate(MigrationBuilder migration)` emits preload-gated `CREATE EXTENSION` DDL plus temporal-key, check, and JSON-schema SQL; `public static Fin<MigrationBuilder> Validate(MigrationBuilder migration, Func<string, bool> extensionAvailable)` folds each fallback-bearing extension, degrading to its app-side path when the deploy image lacks the pgrx-compiled extension and failing typed otherwise.
- Auto: `HasPostgresExtension` annotations flow through `AlterDatabaseOperation` into generated migration DDL — `CreatePostgresExtensionOperation` is the deleted phantom spelling. `HasPostgresEnum` annotations flow through `PostgresEnum` into the same `AlterDatabaseOperation` so a native enum column emits a `CREATE TYPE ... AS ENUM` step rather than a check-constrained text column. A `TemporalKey` row, a `Check` row, and a `JsonSchemaCheck` row emit through `MigrationBuilder.Sql` because the WITHOUT OVERLAPS clause and the `jsonb_matches_schema` predicate have no first-party EF translator; preload-gated rows (`pg_search`) emit their `CreateSql` through `Migrate` because `HasPostgresExtension` cannot encode the `shared_preload_libraries` prerequisite, and `CASCADE` rows (`vectorscale`) emit `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE` so the `vector` dependency installs in one step.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, Pgvector.EntityFrameworkCore, Pgvector, Npgsql, JsonSchema.Net, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `SchemaDdl` row — a new extension is one `Extension` entry (its access method, preload gate, cascade flag, and fallback path are columns on the same row, never a sibling provisioning type), a new index family is one `Index` row carrying its operator class, `Include` covering columns, `NullsDistinct`, and `With` build-option map, a temporal-versioned table is one `TemporalKey` row, a server-validated document lane is one `JsonSchemaCheck` row, a table invariant is one `Check` row, a native pg enum is one `Enum` entry; zero new surface.
- Boundary: extension capability is one `Extension` row with its full DDL story in columns — `vectorscale` rides `AccessMethod: "diskann"` and `Cascade: true` so `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE` pulls the `vector` dependency, its diskann index lands as an `Index` row whose `With` map carries `storage_layout`/`num_neighbors`/`search_list_size`/`max_alpha`/`num_dimensions`/`num_bits_per_dimension` against `vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`, and the query-time `diskann.query_search_list_size`/`diskann.query_rescore` GUCs belong to the search lane not this declaration row; `pg_search` rides `AccessMethod: "bm25"` and `PreloadGated: true` so it never enters `HasPostgresExtension` (`shared_preload_libraries = 'pg_search'` must precede `CREATE EXTENSION pg_search`, so `Migrate` emits it), its one-per-table bm25 index lands as an `Index` row keyed by the `key_field` UNIQUE/primary column listed first in `With`, and the `pdb.*` query builders, `|||`/`&&&`/`===`/`###` column operators, `pdb.score`/`pdb.snippet` projections, and `::pdb.fuzzy`/`::pdb.boost` casts belong to the search lane — the removed `paradedb.*` namespace is the deleted phantom spelling, every search predicate uses the `pdb.*` namespace; an hnsw vector index rides `Method: "hnsw"` with `m`/`ef_construction` in the `With` map; the self-provisioned non-preload `pg_jsonschema` carries `Fallback: "Json.Schema.JsonSchema.Evaluate"` so `Validate` degrades the document lane to JsonSchema.Net in-process evaluation when the deploy image lacks the pgrx-compiled extension, never silently dropping validation. The `Surface` column states the driver-native cost of each row and built-in ranges and multiranges map to `NpgsqlRange<T>` with zero extension entry. `Index` rows carry the method, operator-class, `Include`, `NullsDistinct`, and `With` columns that the per-column lane policies instantiate, and a compound `Index` row leading with the tenant/partition discriminant serves both keyset cursors and single-column filters through the PG18 automatic B-tree skip scan so a redundant single-column index is the deleted form; `Include` columns leave the key as covering payload and `NullsDistinct: false` is the single-null uniqueness law deleting the partial-index workaround. `Exclusion` rows ride btree_gist for range non-overlap, and `TemporalKey` rows ride the PG18 WITHOUT OVERLAPS shape over a `tstzrange` valid-time column GiST-backed by btree_gist for scalar equality — `PRIMARY KEY (id, valid_period WITHOUT OVERLAPS)`, `UNIQUE (... WITHOUT OVERLAPS)`, and `FOREIGN KEY (cust_id, PERIOD valid_period) REFERENCES parent (id, PERIOD parent_period)` are the bitemporal-versioning structural fence for geospatial-sync and multi-tenant history, the migration emits the constraint and a destructive temporal-key change rides the `DestructiveUnapproved` retention gate. `Check` rows declare free-form table invariants and `JsonSchemaCheck` rows declare the `CHECK (jsonb_matches_schema(<schema>, <column>))` document-lane invariant so the document lane validates server-side, with `Validate` carrying the in-process fallback when the server-side extension is absent. The postgis row makes NetTopologySuite the pg boundary projection of the canonical proto wire geometry; earthdistance is rejected — the postgis row owns geodesy. `Composite` rows declare PostgreSQL composite types — `MapComposites` folds each onto the data-source builder through `MapComposite(Type, name)` so the round-trip type registration is one row, never a per-type hand-written reader. `Enum` rows declare native PostgreSQL enum types symmetric with `Composite` — `MapEnums` folds each onto the data-source builder through the generic `MapEnum<TEnum>(pgName, translator)` resolver while `Declare` folds `HasPostgresEnum<TEnum>(name)` so the model annotation and the type registration trace to one row, never a per-enum hand-written `MapEnum` call beside a hand-written check constraint, and the `EnableUnmappedTypes` builder column on the pg profile row admits enum-as-text round-trips without an `Enum` entry where a native type is unwarranted. The composite and enum sets are empty until a real landmark exists, the cases are shaped for the family they absorb.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class TemporalShape {
    public static readonly TemporalShape PrimaryKey = new("primary-key");
    public static readonly TemporalShape Unique = new("unique");
    public static readonly TemporalShape ForeignKey = new("foreign-key");
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
    public sealed record Index(string Table, Seq<string> Columns, string Method, Option<string> Operators, Seq<string> Include = default, Option<bool> NullsDistinct = default, Map<string, string> With = default) : SchemaDdl {
        private string ColumnList => string.Join(", ", Columns.Map(c => Operators.Match(Some: ops => $"{c} {ops}", None: () => c)));
        private string IncludeClause => Include.IsEmpty ? "" : $" INCLUDE ({string.Join(", ", Include)})";
        private string NullsClause => NullsDistinct.Match(Some: static distinct => distinct ? "" : " NULLS NOT DISTINCT", None: static () => "");
        private string WithClause => With.IsEmpty ? "" : $" WITH ({string.Join(", ", With.AsEnumerable().Map(static kv => $"{kv.Key} = {kv.Value}"))})";
        public string Sql => $"CREATE INDEX ON {Table} USING {Method} ({ColumnList}){IncludeClause}{NullsClause}{WithClause}";
    }
    public sealed record Exclusion(string Table, string Predicate, string Method) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD EXCLUDE USING {Method} ({Predicate})";
    }
    public sealed record TemporalKey(string Table, Seq<string> Columns, string Period, TemporalShape Shape, Option<(string Table, Seq<string> Columns, string Period)> References) : SchemaDdl {
        public string Sql => Shape.Switch(
            state: this,
            primaryKey: static self => $"ALTER TABLE {self.Table} ADD PRIMARY KEY ({string.Join(", ", self.Columns)}, {self.Period} WITHOUT OVERLAPS)",
            unique: static self => $"ALTER TABLE {self.Table} ADD UNIQUE ({string.Join(", ", self.Columns)}, {self.Period} WITHOUT OVERLAPS)",
            foreignKey: static self => self.References.Match(
                Some: parent => $"ALTER TABLE {self.Table} ADD FOREIGN KEY ({string.Join(", ", self.Columns)}, PERIOD {self.Period}) REFERENCES {parent.Table} ({string.Join(", ", parent.Columns)}, PERIOD {parent.Period})",
                None: () => string.Empty));
    }
    public sealed record Check(string Table, string Constraint, string Predicate) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD CONSTRAINT {Constraint} CHECK ({Predicate})";
    }
    public sealed record JsonSchemaCheck(string Table, string Column, string Constraint, string Schema) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD CONSTRAINT {Constraint} CHECK (jsonb_matches_schema('{Schema}', {Column}))";
    }
    public sealed record Composite(string Name, Type ClrType) : SchemaDdl;
    public sealed record Enum(string Name, Type ClrType, Func<NpgsqlDataSourceBuilder, string, INpgsqlNameTranslator?, NpgsqlDataSourceBuilder> Map, Func<ModelBuilder, ModelBuilder> Annotate) : SchemaDdl {
        public static Enum Of<TEnum>(string name) where TEnum : struct, System.Enum =>
            new(name, typeof(TEnum),
                static (builder, pgName, translator) => builder.MapEnum<TEnum>(pgName, translator),
                static model => model.HasPostgresEnum<TEnum>(name));
    }

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

    public static MigrationBuilder Migrate(MigrationBuilder migration) =>
        JsonSchemaChecks.Fold(
            Checks.Fold(
                TemporalKeys.Filter(static row => row.Sql.Length > 0).Fold(
                    Extensions.Filter(static row => row.PreloadGated).Fold(migration, static (builder, row) => { builder.Sql(row.CreateSql); return builder; }),
                    static (builder, row) => { builder.Sql(row.Sql); return builder; }),
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
}
```


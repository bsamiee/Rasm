# [PERSISTENCE_QUERY_DATASETS]

Rasm.Persistence declares the Series hypertable roster, the Fleet op-log row vocabulary, and the assessment-fact plane. Each dataset is one `AnalyticsSchema` value driving DDL, landing, reads, and cold-tail batches.

Producer-handed DECLARATIONS cross elsewhere and stay elsewhere — `Rasm.Element` hands its catalogue datasets across the `[WIRE]: AnalyticsSchema` boundary and `Rasm.Materials` hands its catalogue and texture generations across the `[WIRE]: MaterialsDataset` boundary, two named wires over ONE admitted vocabulary `Query/backend#SCHEMA_ADMISSION` gates. Producer-handed ROWS are the other crossing and land on a dataset declared here, which is why this page declares no boundary of its own: a custodian-owned dataset is admitted by construction and a producer that hands rows onto it hands data, never a declaration.

## [01]-[INDEX]

- [02]-[SERIES_ROSTER]: `SeriesKind` derives each hypertable family's whole provisioning set from one row, `SeriesPoint` is the ingest value, `SeriesSelector` names a stream by key or by facet, and `SeriesLane` is the family's landing arm beside its three reads.
- [03]-[WAREHOUSE_OPLOG]: `WarehouseSchema` declares the Fleet op-log dataset and `WarehouseOpRow` is the row both ends of the egress boundary read.
- [04]-[ASSESSMENT_ROWS]: `AssessmentDataset` declares the analysis pipeline's typed fact plane and `AssessmentLane` derives its provisioning, lands its rows and its cold-tail batch, and serves the content-addressed read.
- [05]-[RESEARCH]: open verification debts and their routes.

## [02]-[SERIES_ROSTER]

- Owner: `SeriesKind` is the `[SmartEnum<string>]` hypertable family roster — one row per Series dataset carrying its relation name, the ordered text facets it keys by beyond the shared spine, and the `BackendPolicy` its whole provisioning set derives from; `SeriesPoint` is the ingest value a producer hands; `SeriesSelector` closes how a caller names a stream; `SeriesLane` is the family's provisioning derivation, its landing arm, and its three reads; `SeriesWeight`, `SeriesBucket`, and `SeriesJobHealth` are the three read shapes.
- Cases: `SeriesKind` carries assessment and sensor rows; `SeriesSelector` names a stream by content key or ordered facets.
- Entry: `SeriesLane.Provision(SeriesKind)` derives the whole ordered step set through the one backend emitter; `Ingest(NpgsqlDataSource, SeriesKind, Seq<SeriesPoint>, ProjectionContext)` is the family's arm of the one relational landing; `Weighted(BackendReach, SeriesKind, SeriesSelector, BackendWindow, ProjectionContext)` reads the time-weighted mean off raw chunks and `Bucketed(…, double quantile, …)` reads the accessor projection off the materialised rollup, both through the one plan builder and the one query entry; `Jobs(NpgsqlDataSource, SeriesKind)` reads the Timescale bgworker run history.
- Auto: a new series family is ONE row carrying its relation, its facets, and its policy — the hypertable, the columnstore, the retention window, and the continuous aggregate all derive from `SeriesKind.Schema` through `BackendDdl.Provision`, and a facet added to a row moves the schema projection, the ingest cell fold, and both reads' narrowings together. Both reads compose `BackendPlan` builders and `BackendRead.Read`, so this page assembles no relation, spells no extension name, and writes no SQL; the raw-chunk fold and the rollup accessors answer ONE statistic because `SeriesBackend` materialises the summary the accessors read.
- Packages: Npgsql (`NpgsqlDataSource.CreateCommand`/`NpgsqlCommand.ExecuteReaderAsync`/`NpgsqlDataReader`/`NpgsqlException`), timescaledb (`timescaledb_information.jobs`/`job_stats`), Rasm (`Domain/identity#CONTENT_KEY` `ContentHash.Hex` — the key text a narrowing carries, `Domain/stats#SCALAR_CARRIER` `QuantileRule`), Rasm.Persistence (`Query/backend#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnType`/`ColumnCell`, `#BACKEND_FAMILY` `Backend`/`BackendPolicy`/`BackendFault`, `#PROVISIONING` `BackendDdl`/`SeriesBackend`, `Query/serving#READ_PLAN` `BackendPlan`/`BackendFold`/`BackendScope`, `#SERVING_PLANE` `BackendRead`/`BackendLanding`/`BackendRow`, `Element/graph#PROJECTION_FRAME` `ProjectionContext`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new family is one `SeriesKind` row; a new way to name a stream is one `SeriesSelector` case breaking both narrowing folds at compile time; a new read shape is one record beside the fold roster its plan names; zero new surface — a per-family provisioning path, a second importer body, a hand-written `SELECT`, a per-selector read entry, or a duration column beside the policy that already carries it is the deleted form.
- Law: the spine travels as SCHEMA COLUMNS, never as statics a provisioning arm reaches for, which is what lets a producer-handed dataset with its own instant name provision through the identical emitter. Static field initializers run in DECLARATION order, so every facet and spine `Identifier` leads the rows — a row reading an identifier declared below it captures an uninitialized value and mounts a nameless column. Each read groups by series because a `time_weight` summary combines across DISJOINT windows alone: folding two live streams sharing a window is a mean no algebra defines, so a facet selection matching several streams answers one weighted mean EACH.
- Boundary: tenancy is NOT a point column — the whole COPY batch lands under the ingesting frame's tenant and every read scopes by it, so equal series keys under distinct tenants never share rows. Absence stays absence across an empty window, where a collapsed `0d` reads as a measurement a board renders indistinguishably from a measured floor. `Jobs` is Timescale-ONLY and named for what it is — `BackendRead.Health` measures the expiry OUTCOME every backend answers, while this transcribes a bgworker run history only one engine publishes, over a catalog relation carrying no tenant column and no time spine, which is why it cannot ride the family entry and why its sibling backends carry no counterpart.

```csharp
using Npgsql;
using NodaTime;
using Rasm.Domain;
using Rasm.Persistence.Element;
using System.Globalization;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeriesKind {
    public static readonly Identifier SeriesColumn = Identifier.Create("series_key");
    public static readonly Identifier AtColumn = Identifier.Create("at");
    public static readonly Identifier ValueColumn = Identifier.Create("value");
    public static readonly SeriesKind Assessment = new("assessment", "assessment_series",
        Tuned("assessment_series", Duration.FromDays(365), Duration.FromHours(1), Duration.FromDays(1), Duration.FromDays(3)),
        Seq<Identifier>());
    public static readonly SeriesKind Sensor = new("sensor", "sensor_series",
        Tuned("sensor_series", Duration.FromDays(90), Duration.FromMinutes(15), Duration.FromDays(1), Duration.FromDays(3)),
        Seq<Identifier>());
    public string Table { get; }
    public BackendPolicy Policy { get; }
    public Seq<Identifier> Facets { get; }

    private SeriesKind(string key, string table, BackendPolicy policy, Seq<Identifier> facets) : this(key) =>
        (Table, Policy, Facets) = (table, policy, facets);

    static BackendPolicy Tuned(string table, Duration retain, Duration grain, Duration chunk, Duration backfill) =>
        new(retain, grain, chunk, backfill, StorePath.Create(table));

    public AnalyticsSchema Schema => new(Table,
        Seq(SeriesColumn) + Facets,
        Seq(new ColumnRow(SeriesColumn, ColumnType.KeyHex, Nullable: false))
            + Facets.Map(static facet => new ColumnRow(facet, ColumnType.Utf8, Nullable: false))
            + Seq(new ColumnRow(AtColumn, ColumnType.Timestamp, Nullable: false),
                  new ColumnRow(ValueColumn, ColumnType.Float64, Nullable: false)),
        Time: AtColumn, Spine: TimeSpine.Event, Measure: Some(ValueColumn));

    public AnalyticsSchema Rollup => new((string)SeriesBackend.Rollup(Schema),
        Seq(SeriesColumn) + Facets,
        Seq(new ColumnRow(SeriesColumn, ColumnType.KeyHex, Nullable: false))
            + Facets.Map(static facet => new ColumnRow(facet, ColumnType.Utf8, Nullable: false))
            + Seq(new ColumnRow(SeriesBackend.Bucket, ColumnType.Timestamp, Nullable: false),
                  new ColumnRow(SeriesBackend.Low, ColumnType.Float64, Nullable: false),
                  new ColumnRow(SeriesBackend.High, ColumnType.Float64, Nullable: false),
                  new ColumnRow(SeriesBackend.Samples, ColumnType.Int64, Nullable: false)),
        Time: SeriesBackend.Bucket, Spine: TimeSpine.Event, Measure: None);
}

public readonly record struct SeriesPoint(UInt128 Series, Instant At, double Value, Seq<string> Facets);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SeriesSelector {
    private SeriesSelector() { }
    public sealed record Key(UInt128 Series) : SeriesSelector;
    public sealed record Facets(Seq<string> Values) : SeriesSelector;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SeriesWeight(UInt128 Series, double Weighted);

public readonly record struct SeriesBucket(Instant Bucket, UInt128 Series, double Mean, double Tail, double Low, double High, long Samples);

public readonly record struct SeriesJobHealth(string Hypertable, string Status, Option<Instant> LastSuccessfulFinish, long TotalFailures);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SeriesLane {
    public static Fin<Seq<ProvisionStep>> Provision(SeriesKind kind) =>
        BackendDdl.Provision(Backend.Series, kind.Schema, kind.Policy);

    public static IO<Fin<BackendWrite>> Ingest(NpgsqlDataSource store, SeriesKind kind, Seq<SeriesPoint> points, ProjectionContext frame) =>
        BackendLanding.Stage(store, kind.Schema, points.Map(Cells), frame);

    static Seq<ColumnCell> Cells(SeriesPoint point) =>
        Seq<ColumnCell>(new ColumnCell.Key(point.Series))
        + point.Facets.Map(static facet => (ColumnCell)new ColumnCell.Text(facet))
        + Seq<ColumnCell>(new ColumnCell.Moment(point.At), new ColumnCell.Real(point.Value));

    public static IO<Fin<BackendResult<SeriesWeight>>> Weighted(
        BackendReach reach, SeriesKind kind, SeriesSelector selector, BackendWindow window, ProjectionContext frame) =>
        Served(kind, kind.Schema, selector, window, frame, BackendProjection.Aggregate,
            (schema, matches) => BackendPlan.Aggregate(schema, schema.Table, matches,
                Seq(SeriesKind.SeriesColumn),
                Seq((WeightedColumn, (BackendFold)new BackendFold.Weighted(SeriesKind.ValueColumn)))),
            reach, static (backend, row) =>
                (row.Key(backend, 0).ToValidation(), row.Real(backend, 1).ToValidation())
                .Apply(static (series, weighted) => new SeriesWeight(series, weighted)).As().ToFin());

    public static IO<Fin<BackendResult<SeriesBucket>>> Bucketed(
        BackendReach reach, SeriesKind kind, SeriesSelector selector, BackendWindow window,
        double quantile, QuantileRule rule, ProjectionContext frame) =>
        Served(kind, kind.Rollup, selector, window, frame, BackendProjection.Quantile,
            (schema, matches) => BackendPlan.Project(schema, schema.Table, matches,
                Seq((SeriesBackend.Bucket, (BackendFold)new BackendFold.Plain(SeriesBackend.Bucket)),
                    (SeriesKind.SeriesColumn, new BackendFold.Plain(SeriesKind.SeriesColumn)),
                    (SeriesBackend.Weight, new BackendFold.Mean()),
                    (SeriesBackend.Sketch, new BackendFold.Tail(quantile, rule)),
                    (SeriesBackend.Low, new BackendFold.Plain(SeriesBackend.Low)),
                    (SeriesBackend.High, new BackendFold.Plain(SeriesBackend.High)),
                    (SeriesBackend.Samples, new BackendFold.Plain(SeriesBackend.Samples))),
                Seq(SeriesBackend.Bucket)),
            reach, static (backend, row) =>
                (row.At(backend, 0).ToValidation(), row.Key(backend, 1).ToValidation(),
                 row.Real(backend, 2).ToValidation(), row.Real(backend, 3).ToValidation(),
                 row.Real(backend, 4).ToValidation(), row.Real(backend, 5).ToValidation(),
                 row.Whole(backend, 6).ToValidation())
                .Apply(static (bucket, series, mean, tail, low, high, samples) =>
                    new SeriesBucket(bucket, series, mean, tail, low, high, samples)).As().ToFin());

    static IO<Fin<BackendResult<T>>> Served<T>(
        SeriesKind kind, AnalyticsSchema schema, SeriesSelector selector, BackendWindow window, ProjectionContext frame,
        BackendProjection projection, Func<AnalyticsSchema, Seq<(Identifier Column, string Value)>, Fin<Plan>> build,
        BackendReach reach, Func<Backend, BackendRow, Fin<T>> shape) =>
        Narrowings(kind, selector).Bind(matches => build(schema, matches)).Match(
            Succ: plan => BackendRead.Read(reach, plan,
                new BackendScope(Backend.Series, schema, window, frame), projection,
                row => shape(Backend.Series, row)),
            Fail: error => IO.pure(Fin<BackendResult<T>>.Fail(error)));

    static Fin<Seq<(Identifier Column, string Value)>> Narrowings(SeriesKind kind, SeriesSelector selector) =>
        selector.Switch(
            state: kind,
            key:    static (_, row) => Fin.Succ(Seq((SeriesKind.SeriesColumn, ContentHash.Hex(row.Series)))),
            facets: static (family, row) => row.Values.Count == family.Facets.Count
                ? Fin.Succ(family.Facets.Zip(row.Values).Map(static pair => (pair.Item1, pair.Item2)).Strict())
                : Fin.Fail<Seq<(Identifier, string)>>(new BackendFault.ReadRefused(
                    Backend.Series.Key, new EngineFault("<facet-arity>", family.Key))));

    public static IO<Fin<Seq<SeriesJobHealth>>> Jobs(NpgsqlDataSource store, SeriesKind kind) =>
        IO.liftAsync(async () => (await Try.lift(async token => {
            await using NpgsqlCommand command = store.CreateCommand(
                "SELECT j.hypertable_name, s.job_status, s.last_successful_finish, s.total_failures " +
                "FROM timescaledb_information.jobs j JOIN timescaledb_information.job_stats s ON s.job_id = j.job_id " +
                "WHERE j.hypertable_name = @table");
            _ = command.Parameters.AddWithValue("table", (string)kind.Schema.Table);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
            List<SeriesJobHealth> rows = [];
            while (await reader.ReadAsync(token).ConfigureAwait(false)) {
                rows.Add(new SeriesJobHealth(reader.GetString(0), reader.GetString(1),
                    reader.IsDBNull(2) ? Option<Instant>.None : Some(reader.GetFieldValue<Instant>(2)),
                    reader.GetInt64(3)));
            }
            return Fin<Seq<SeriesJobHealth>>.Succ(toSeq(rows));
        }).Run().Bind(static inner => inner).ConfigureAwait(false)).MapFail(Backend.Series.ReadRefused));

    public static readonly Identifier WeightedColumn = Identifier.Create("weighted");
}
```

## [03]-[WAREHOUSE_OPLOG]

- Owner: `WarehouseSchema` declares the Fleet backend's op-log dataset as ONE `AnalyticsSchema` value; `WarehouseOpRow` is the typed row a fleet question composes over.
- Entry: `WarehouseSchema.Dataset` is the declaration the backend DDL, the egress sink's insert column list, and every reader's ordinals derive from; `Shape(Backend, BackendRow)` is the reader over the one row surface every reach yields.
- Auto: the sink lands EXACTLY these columns projected from `Egress.Envelope` — `id` the content key, `source`/`type`/`time` the message-envelope attributes, `partition_key`/`sequence` the partitioning extensions, `data` the redacted payload — so writer and reader cannot drift while naming one relation. Every column declares `Nullable: false`, so the seven reads compose as ONE applicative product and a backend answering an empty cell names every offending column at once.
- Packages: Rasm.Persistence (`Query/backend#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnType`, `#BACKEND_FAMILY` `Backend`/`BackendFault`, `Query/serving#SERVING_PLANE` `BackendRow`), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op-log column is one `ColumnRow` beside one reader ordinal moving together; zero new surface — a second row vocabulary, a hand-spelled insert column list, or a per-end declaration is the deleted form.
- Law: op-log rows spell their instant `time`, not `at` — exactly why the backend spine travels as a schema column: this dataset provisions its MergeTree partition, sort key, and TTL through the same arm every `at`-spelled series relation rides, with no dialect arm branching on a column name.
- Boundary: the Fleet leg is READ-side only. `Version/egress`'s ClickHouse sink owns landing under `insert_deduplication_token` dedup, and the two ends meet at this declaration rather than at a table name two sites spell; ClickHouse carries no transaction, so every fleet read is a convergence-consistent view whose staleness the egress cursor bounds.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record WarehouseOpRow(string Id, string Source, string Type, Instant Time, string PartitionKey, long Sequence, ReadOnlyMemory<byte> Data);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class WarehouseSchema {
    public static readonly Identifier TimeColumn = Identifier.Create("time");

    public static readonly AnalyticsSchema Dataset = new("rasm.oplog",
        Seq(Identifier.Create("source"), Identifier.Create("type"), TimeColumn),
        Seq(new ColumnRow(Identifier.Create("id"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("source"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("type"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(TimeColumn, ColumnType.Timestamp, Nullable: false),
            new ColumnRow(Identifier.Create("partition_key"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("sequence"), ColumnType.Int64, Nullable: false),
            new ColumnRow(Identifier.Create("data"), ColumnType.Utf8, Nullable: false)),
        Time: TimeColumn, Spine: TimeSpine.Event, Measure: None);

    public static string Table => (string)Dataset.Table;
    public static string Columns => string.Join(", ", Dataset.Columns.Map(static column => (string)column.Name));

    public static Fin<WarehouseOpRow> Shape(Backend backend, BackendRow row) =>
        (row.Text(backend, 0).ToValidation(), row.Text(backend, 1).ToValidation(),
         row.Text(backend, 2).ToValidation(), row.At(backend, 3).ToValidation(),
         row.Text(backend, 4).ToValidation(), row.Whole(backend, 5).ToValidation(),
         row.Text(backend, 6).ToValidation())
        .Apply(static (id, source, type, time, partition, sequence, data) =>
            new WarehouseOpRow(id, source, type, time, partition, sequence, Encoding.UTF8.GetBytes(data))).As().ToFin();
}
```

## [04]-[ASSESSMENT_ROWS]

- Owner: `AssessmentDataset` is the ONE `AnalyticsSchema` value the analysis pipeline's typed result stream lands on, beside the `BackendPolicy` its whole provisioning set derives from and the column identifiers every derivation reads; `AssessmentLane` is that dataset's provisioning derivation, its two landing arms, its named plan, and its resident read, with `Cells` the ONE cell fold both landings share and `Shape` its reader inverse.
- Cases: the fourteen `PropertyValue` arms are ONE `kind` column carrying the union's own case token, and each arm answers the scalar face the `Face` projection seats — a `Measure` its SI magnitude and unit, a `Number` its magnitude, an `Integer` its magnitude when the double holds the value exactly, a `Boolean`/present `Logical` its flag, a `Bounded` its three SI bounds under the one unit its members share, a `Text`/`Enumerated` its canonical render, and the remaining arms the all-absent face the `value` column already carries whole.
- Entry: `AssessmentLane.Provision()` derives the whole ordered statement set through the one backend emitter; `Ingest(NpgsqlDataSource, Seq<TRow>, Func<TRow, FactRow>, ProjectionContext)` is the relational arm of the one landing and `Batch(Seq<TRow>, Func<TRow, FactRow>, ProjectionContext, metadata)` its cold-tail record batch, both over the producer's OWN row type through one projection; `Scan(UInt128, Option<Discipline>)` is the named plan and `Resident(reach, scope, key, discipline, mint)` the durable read, handing each row's five coordinates to a mint the caller supplies.
- Auto: a fact kind gaining a scalar face is ONE `ColumnRow` beside its slot in the `Face` projection, and a kind with no scalar face still lands whole because `value` carries the entire fact through the wire's own codec; the DDL, the COPY roster, the record batch, the plan's projected names, and the reader's ordinals all derive from `AssessmentDataset.Schema`, so a column insert moves every one of them together and no literal index or hand column list survives beside it. The dataset declares NO measure, so the Series arm provisions hypertable, columnstore, and retention and emits no continuous aggregate — a fact stream is not a scalar series, and the numeric rollup a discipline wants rides `SeriesKind.Assessment` through the producer's own temporal leg.
- Packages: Npgsql (`NpgsqlDataSource`), Apache.Arrow (`RecordBatch`), Rasm (`Domain/identity#CONTENT_KEY` `ContentHash.Hex` — the key text a content-addressed narrowing carries, `FactoryBridge.Accept` admission), Rasm.Element (`Classification/classification#DISCIPLINE_AXIS` `Discipline`, `Properties/property#PROPERTY_VALUE` `PropertyName`/`PropertyValue`, `Properties/quantity#MEASURE_VALUE` `MeasureValue`, `Graph/wire#NODE_CODEC` `ElementWire` — the one public door onto the wire's `PropertyValue` codec), Rasm.Persistence (`Query/backend#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnRow`/`ColumnShape`/`ColumnType`/`ColumnCell`/`TimeSpine`/`ArrowLanding`, `#BACKEND_FAMILY` `Backend`/`BackendPolicy`, `#SCHEMA_ADMISSION` `AnalyticsGate.LandedColumn`, `#PROVISIONING` `BackendDdl`/`ProvisionStep`, `Query/serving#READ_PLAN` `BackendPlan`/`BackendFold`/`BackendScope`, `#SERVING_PLANE` `BackendRead`/`BackendLanding`/`BackendRow`/`BackendReach`, `Element/graph#PROJECTION_FRAME` `ProjectionContext`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new scalar face is one `ColumnRow` beside one `Face` slot; a new consumer of the plane is one `Resident` mint at that consumer; zero new surface — a per-discipline relation, a second cell fold, a hand `SELECT`, a literal reader ordinal, or a record mirroring the producer's row is the deleted form.
- Law: facet arity is ROW DATA, never schema — a discipline's facet path rides the `ColumnShape.List` container the vocabulary already generates, so an energy row's `(measure, fuel, end-use)` triple and a daylight row's single sensor id land in one column and one relation. A per-discipline table would be a backend per producer, which is the custodian law this page exists to hold. The `value` column is the TRUTH and every scalar column its projection: the whole fact crosses through the wire's one canonical `PropertyValue` codec, so a case with no scalar face rehydrates losslessly and a scalar column is a query accelerator a read never inverts. Tenancy is ROUTING, not a column — the whole batch lands under the ingesting frame's tenant and every read scopes by it. The retention extent matches `SeriesKind.Assessment`'s, so a board resolving a temporal point to its typed rows never lands on rows already dropped.
- Boundary: `Rasm.Compute` sits ABOVE this custodian and references it, so the producer's `AssessmentRow` record is unnameable here and a mirror of it would be a strata inversion wearing a convenience — the arms take the producer's row type as a TYPE PARAMETER beside one projection onto the five coordinates a fact carries, every one of them `Rasm.Element` or BCL vocabulary this package already references, so neither end holds the other's record. Producer-handed rows LAND: this custodian derives nothing from the fact, re-measures nothing, and admits by construction, while a JSON-only row that no filter can narrow and a scalar-only row that silently drops a `Table`, a `Complex`, or a `Binary` payload are both the deleted form. An empty facet path is an EMPTY RUN, never absence — `ColumnRow.Admits` refuses an absent cell on a container by declaration, and a discipline emitting one unfaceted fact per assessment is the ordinary case.

```csharp
using Apache.Arrow;
using LanguageExt;
using NodaTime;
using Npgsql;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using System.Numerics;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

using FactRow = (System.UInt128 Key, Rasm.Element.Classification.Discipline Discipline,
    LanguageExt.Seq<string> Facets, Rasm.Element.Properties.PropertyName Name,
    Rasm.Element.Properties.PropertyValue Value);

// --- [TYPES] ---------------------------------------------------------------------------
public static class AssessmentDataset {
    public static readonly Identifier KeyColumn = Identifier.Create("key");
    public static readonly Identifier DisciplineColumn = Identifier.Create("discipline");
    public static readonly Identifier FacetsColumn = Identifier.Create("facets");
    public static readonly Identifier NameColumn = Identifier.Create("name");
    public static readonly Identifier KindColumn = Identifier.Create("kind");
    public static readonly Identifier MagnitudeColumn = Identifier.Create("magnitude");
    public static readonly Identifier UnitColumn = Identifier.Create("unit");
    public static readonly Identifier LowerColumn = Identifier.Create("lower");
    public static readonly Identifier UpperColumn = Identifier.Create("upper");
    public static readonly Identifier SetPointColumn = Identifier.Create("setpoint");
    public static readonly Identifier FlagColumn = Identifier.Create("flag");
    public static readonly Identifier TextColumn = Identifier.Create("text");
    public static readonly Identifier ValueColumn = Identifier.Create("value");

    public static readonly BackendPolicy Policy = new(
        Retain: Duration.FromDays(365), Grain: Duration.FromDays(1), Chunk: Duration.FromDays(7),
        Backfill: Duration.FromDays(30), Root: StorePath.Create("assessment_rows"));

    public static readonly AnalyticsSchema Schema = new("assessment_rows",
        Seq(KeyColumn, DisciplineColumn, NameColumn, FacetsColumn),
        Seq(new ColumnRow(KeyColumn, ColumnType.KeyHex, Nullable: false),
            new ColumnRow(DisciplineColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(FacetsColumn, new ColumnShape.List(ColumnType.Utf8), Nullable: false),
            new ColumnRow(NameColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(KindColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(MagnitudeColumn, ColumnType.Float64, Nullable: true),
            new ColumnRow(UnitColumn, ColumnType.Utf8, Nullable: true),
            new ColumnRow(LowerColumn, ColumnType.Float64, Nullable: true),
            new ColumnRow(UpperColumn, ColumnType.Float64, Nullable: true),
            new ColumnRow(SetPointColumn, ColumnType.Float64, Nullable: true),
            new ColumnRow(FlagColumn, ColumnType.Bool, Nullable: true),
            new ColumnRow(TextColumn, ColumnType.Utf8, Nullable: true),
            new ColumnRow(ValueColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(AnalyticsGate.LandedColumn, ColumnType.Timestamp, Nullable: false)),
        Time: AnalyticsGate.LandedColumn, Spine: TimeSpine.Landing, Measure: None);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AssessmentLane {
    public static Fin<Seq<ProvisionStep>> Provision() =>
        BackendDdl.Provision(Backend.Series, AssessmentDataset.Schema, AssessmentDataset.Policy);

    public static IO<Fin<BackendWrite>> Ingest<TRow>(
        NpgsqlDataSource store, Seq<TRow> rows, Func<TRow, FactRow> fact, ProjectionContext frame) =>
        Staged(rows, fact).Match(
            Succ: staged => BackendLanding.Stage(store, AssessmentDataset.Schema, staged, frame),
            Fail: error => IO.pure(Fin<BackendWrite>.Fail(error)));

    public static Fin<RecordBatch> Batch<TRow>(
        Seq<TRow> rows, Func<TRow, FactRow> fact, ProjectionContext frame, Seq<(string Key, string Value)> metadata) {
        ColumnCell stamp = new ColumnCell.Moment(frame.Now());
        return Staged(rows, fact).Bind(staged =>
            ArrowLanding.Build(AssessmentDataset.Schema, staged, cells => cells + Seq(stamp), metadata));
    }

    public static Fin<Plan> Scan(UInt128 key, Option<Discipline> discipline) =>
        BackendPlan.Scan(AssessmentDataset.Schema,
            Seq((AssessmentDataset.KeyColumn, ContentHash.Hex()))
            + discipline.Map(static row => (AssessmentDataset.DisciplineColumn, row.Key)).ToSeq());

    public static IO<Fin<BackendResult<T>>> Resident<T>(
        BackendReach reach, BackendScope scope, UInt128 key, Option<Discipline> discipline, Func<FactRow, T> mint) =>
        Scan(discipline).Match(
            Succ: plan => BackendRead.Read(reach, plan, scope, BackendProjection.Point,
                row => Shape(scope, row).Map(mint)),
            Fail: error => IO.pure(Fin<BackendResult<T>>.Fail(error)));

    // --- [FACT_PROJECTION]
    public static Validation<Error, Seq<ColumnCell>> Cells(FactRow row) =>
        ElementWire.Encode(row.Value).ToValidation().Map(json =>
            Seq<ColumnCell>(new ColumnCell.Key(row.Key),
                new ColumnCell.Text(row.Discipline.Key),
                new ColumnCell.Items(ColumnType.Utf8, row.Facets),
                new ColumnCell.Text(row.Name.ToValue()),
                new ColumnCell.Text(row.Value.Kind))
            + Face(row.Value)
            + Seq<ColumnCell>(new ColumnCell.Text(json)));

    static Fin<Seq<Seq<ColumnCell>>> Staged<TRow>(Seq<TRow> rows, Func<TRow, FactRow> fact) =>
        rows.Traverse(row => Cells(fact(row))).As().ToFin();

    static Seq<ColumnCell> Face(PropertyValue value) => value.Switch(
        text:       static v => Scalars(text: Some(v.Render())),
        measure:    static v => Scalars(magnitude: Some(v.Value.Si), unit: v.Value.CanonicalUnit),
        boolean:    static v => Scalars(flag: Some(v.Value)),
        logical:    static v => Scalars(flag: v.Value),
        integer:    static v => Scalars(magnitude: Exact(v.Value)),
        number:     static v => Scalars(magnitude: Some(v.Value)),
        binary:     static _ => Scalars(),
        enumerated: static v => Scalars(text: Some(v.Render())),
        reference:  static _ => Scalars(),
        bounded:    static v => Scalars(
            unit: Seq(v.Lower, v.Upper, v.SetPoint).Choose(static bound => bound).Head.Bind(static m => m.CanonicalUnit),
            lower: v.Lower.Map(static m => m.Si), upper: v.Upper.Map(static m => m.Si),
            setpoint: v.SetPoint.Map(static m => m.Si)),
        list:       static _ => Scalars(),
        table:      static _ => Scalars(),
        complex:    static _ => Scalars(),
        temporal:   static _ => Scalars());

    static Seq<ColumnCell> Scalars(
        Option<double> magnitude = default, Option<string> unit = default,
        Option<double> lower = default, Option<double> upper = default, Option<double> setpoint = default,
        Option<bool> flag = default, Option<string> text = default) =>
        Seq(Cell(magnitude, static held => new ColumnCell.Real(held)),
            Cell(unit, static held => new ColumnCell.Text(held)),
            Cell(lower, static held => new ColumnCell.Real(held)),
            Cell(upper, static held => new ColumnCell.Real(held)),
            Cell(setpoint, static held => new ColumnCell.Real(held)),
            Cell(flag, static held => new ColumnCell.Flag(held)),
            Cell(text, static held => new ColumnCell.Text(held)));

    static ColumnCell Cell<T>(Option<T> value, Func<T, ColumnCell> present) =>
        value.Match(Some: present, None: static () => (ColumnCell)new ColumnCell.Absent());

    static Option<double> Exact(BigInteger value) =>
        (double)value is var magnitude && double.IsFinite(magnitude) && new BigInteger(magnitude) == value
            ? Some(magnitude)
            : None;

    // --- [FACT_INVERSE]
    public static Fin<FactRow> Shape(BackendScope scope, BackendRow row) {
                AnalyticsSchema declaration = AssessmentDataset.Schema;
        return (row.Key(scope.Backend, declaration.Ordinal(AssessmentDataset.KeyColumn)).ToValidation(),
                row.Text(scope.Backend, declaration.Ordinal(AssessmentDataset.DisciplineColumn))
                    .Bind(token => FactoryBridge.Accept<Discipline>(token)).ToValidation(),
                row.Items(scope.Backend, declaration.Ordinal(AssessmentDataset.FacetsColumn)).ToValidation(),
                row.Text(scope.Backend, declaration.Ordinal(AssessmentDataset.NameColumn))
                    .Bind(token => FactoryBridge.Accept<PropertyName>(token)).ToValidation(),
                row.Text(scope.Backend, declaration.Ordinal(AssessmentDataset.ValueColumn))
                    .Bind(json => ElementWire.Decode(json)).ToValidation())
            .Apply(static (content, discipline, facets, name, value) =>
                (Key: content, Discipline: discipline, Facets: facets, Name: name, Value: value))
            .As().ToFin();
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                   | [BINDING]                                                         |
| :-----: | :------------------ | :---------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | series provisioning | derived from the `SeriesKind` row         | one emitter, generation-carried, verdict-gated                    |
|  [02]   | policy cadence      | each backend's own scheduler              | never AppHost-scheduled; `job_stats` is the Series proof row      |
|  [03]   | irregular timesteps | the `Weighted` fold off the plan builder  | a naive `avg` over-counts dense bursts                            |
|  [04]   | rollup statistic    | materialised summary + read-time accessor | one statistic per caption; never `avg` accelerating `time_weight` |
|  [05]   | series identity     | a `CanonicalWriter`-framed preimage       | length-framed fields; a concatenated key merges two streams       |
|  [06]   | measure projection  | numeric leaves under a facet triple       | text facets name a stream; a hash names nothing                   |
|  [07]   | batch handoff       | `ArrowLanding.Build` over the declaration | metadata is required; never a schema built beside the dataset     |
|  [08]   | fleet leg           | READ row over one declared op-log dataset | the egress sink owns landing; never a second SoR                  |
|  [09]   | facet path          | one `List(Utf8)` column, position-bearing | arity is row data; never a relation per discipline                |
|  [10]   | fact truth          | the whole value through the wire codec    | scalars are the projection; never a second, lossy truth           |
|  [11]   | producer row        | a type parameter beside one projection    | Compute sits above; never a record mirrored across the strata     |

## [05]-[RESEARCH]

(none)

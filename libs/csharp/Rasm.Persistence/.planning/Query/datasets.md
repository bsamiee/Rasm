# [PERSISTENCE_QUERY_DATASETS]

Rasm.Persistence declares THREE datasets for itself here, distinct from every dataset a producer hands across a seam: the Series hypertable roster this custodian provisions and reads, the Fleet op-log row vocabulary both ends of the `Version/egress` seam read, and the receipt evidence plane the kernel's message-envelope stream lands in. Each is one `AnalyticsSchema` value, so the DDL that plants its relation, the plan that reads it, the COPY that lands it, and the `RecordBatch` a cold-tail generation carries all derive from one declaration.

Producer-handed datasets cross elsewhere and stay elsewhere — `Rasm.Element` hands its catalogue datasets across the `[WIRE]: AnalyticsSchema` seam and `Rasm.Materials` hands its catalogue and texture generations across the `[WIRE]: MaterialsDataset` seam, two named wires over ONE admitted vocabulary `Query/residence#SEAM_ADMISSION` gates. This page declares no seam of its own: a custodian-owned dataset is admitted by construction.

## [01]-[INDEX]

- [02]-[SERIES_ROSTER]: `SeriesKind` derives each hypertable family's whole provisioning set from one row, `SeriesPoint` is the ingest value, `SeriesSelector` names a stream by key or by facet, and `SeriesLane` is the family's landing arm beside its three reads.
- [03]-[WAREHOUSE_OPLOG]: `WarehouseSchema` declares the Fleet op-log dataset and `WarehouseOpRow` is the row both ends of the egress seam read.
- [04]-[RECEIPT_EVIDENCE]: `ReceiptResidence` folds the kernel receipt stream into wide-event rows, projects its numeric leaves onto the Telemetry hypertable, builds the cold-tail batch, and serves the resident message-envelope read.
- [05]-[RESEARCH]: open verification debts and their routes.

## [02]-[SERIES_ROSTER]

- Owner: `SeriesKind` is the `[SmartEnum<string>]` hypertable family roster — one row per Series dataset carrying its relation name, the ordered text facets it keys by beyond the shared spine, and the `ResidencePolicy` its whole provisioning set derives from; `SeriesPoint` is the ingest value a producer hands; `SeriesSelector` closes how a caller names a stream; `SeriesLane` is the family's provisioning derivation, its landing arm, and its three reads; `SeriesWeight`, `SeriesBucket`, and `SeriesJobHealth` are the three read shapes.
- Cases: `SeriesKind` is `Assessment` (discipline-assessment streams — energy, thermal, daylight — one-hour grain over a year of retention, no facets), `Sensor` (BMS and operational streams — fifteen-minute grain over ninety days, no facets), and `Telemetry` (the receipt-stream measure projection — one-minute grain over ninety days, keyed by `domain`, `slot`, and `measure` so a board names a stream in TEXT); `SeriesSelector` is `Key` (the source artifact's own content key) and `Facets` (the ordered text values a board spells), so one narrowing serves a content-addressed read and a dashboard read alike.
- Entry: `SeriesLane.Provision(SeriesKind)` derives the whole ordered step set through the one residence emitter; `Ingest(NpgsqlDataSource, SeriesKind, Seq<SeriesPoint>, ProjectionContext)` is the family's arm of the one relational landing; `Weighted(ResidenceReach, SeriesKind, SeriesSelector, ResidenceWindow, ProjectionContext)` reads the time-weighted mean off raw chunks and `Bucketed(…, double quantile, …)` reads the accessor projection off the materialised rollup, both through the one plan builder and the one query entry; `Jobs(NpgsqlDataSource, SeriesKind)` reads the Timescale bgworker run history.
- Auto: a new series family is ONE row carrying its relation, its facets, and its policy — the hypertable, the columnstore, the retention window, and the continuous aggregate all derive from `SeriesKind.Schema` through `ResidenceDdl.Provision`, and a facet added to a row moves the schema projection, the ingest cell fold, and both reads' narrowings together. Both reads compose `ResidencePlan` builders and `ResidenceRead.Read`, so this page assembles no relation, spells no extension name, and writes no SQL; the raw-chunk fold and the rollup accessors answer ONE statistic because `SeriesResidence` materialises the summary the accessors read.
- Receipt: provisioning, ingest, and both reads ride the residence slots `Query/residence#RESIDENCE_FAMILY` declares — `store.columnar.residence.provision`, `.ingest`, and `.read`; the bgworker probe rides `store.columnar.series.jobs` carrying the relation beside the failure counter.
- Packages: Npgsql (`NpgsqlDataSource.CreateCommand`/`NpgsqlCommand.ExecuteReaderAsync`/`NpgsqlDataReader`/`NpgsqlException`), timescaledb (`timescaledb_information.jobs`/`job_stats`), Rasm (`Domain/identity#CONTENT_KEY` `ContentHash.Hex` — the key text a narrowing carries, `Domain/stats#SCALAR_CARRIER` `QuantileRule`), Rasm.Persistence (`Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnType`/`ColumnCell`, `#RESIDENCE_FAMILY` `Residence`/`ResidencePolicy`/`ResidenceFault`, `#PROVISIONING` `ResidenceDdl`/`SeriesResidence`, `Query/serving#READ_PLAN` `ResidencePlan`/`ResidenceFold`/`ResidenceScope`, `#SERVING_PLANE` `ResidenceRead`/`ResidenceLanding`/`ResidenceRow`, `Element/graph#PROJECTION_FRAME` `ProjectionContext`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new family is one `SeriesKind` row; a new way to name a stream is one `SeriesSelector` case breaking both narrowing folds at compile time; a new read shape is one record beside the fold roster its plan names; zero new surface — a per-family provisioning path, a second importer body, a hand-written `SELECT`, a per-selector read entry, or a duration column beside the policy that already carries it is the deleted form.
- Law: the spine travels as SCHEMA COLUMNS, never as statics a provisioning arm reaches for, which is what lets a producer-handed dataset with its own instant name provision through the identical emitter. Static field initializers run in DECLARATION order, so every facet and spine `Identifier` leads the rows — a row reading an identifier declared below it captures an uninitialized value and mounts a nameless column. Each read groups by series because a `time_weight` summary combines across DISJOINT windows alone: folding two live streams sharing a window is a mean no algebra defines, so a facet selection matching several streams answers one weighted mean EACH.
- Boundary: tenancy is NOT a point column — the whole COPY batch lands under the ingesting frame's tenant and every read scopes by it, so equal series keys under distinct tenants never share rows. Absence stays absence across an empty window, where a collapsed `0d` reads as a measurement a board renders indistinguishably from a measured floor. `Jobs` is Timescale-ONLY and named for what it is — `ResidenceRead.Health` measures the expiry OUTCOME every residence answers, while this transcribes a bgworker run history only one engine publishes, over a catalog relation carrying no tenant column and no time spine, which is why it cannot ride the family entry and why its sibling residences carry no counterpart.

```csharp signature
using Npgsql;
using NodaTime;
using Rasm.Domain;                                // ContentHash.Hex — the key text a content-addressed narrowing carries
using Rasm.Persistence.Element;                   // ProjectionContext — the ruled time-and-causal frame
using System.Globalization;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// One row per Series family; the WHOLE hypertable provisioning set derives from these columns. `Facets` names the
// ordered text columns a family carries beyond the shared `(series_key, at, value)` spine, so the telemetry stream
// keys by domain, slot, and measure without a second table and the two AEC families carry none.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeriesKind {
    // Spine and facet identifiers lead the rows: a static field initializer runs in DECLARATION order, so a row
    // reading a facet declared below it would capture an uninitialized `Identifier` and mount a nameless column.
    public static readonly Identifier SeriesColumn = Identifier.Create("series_key");
    public static readonly Identifier AtColumn = Identifier.Create("at");
    public static readonly Identifier ValueColumn = Identifier.Create("value");
    public static readonly Identifier DomainFacet = Identifier.Create("domain");
    public static readonly Identifier SlotFacet = Identifier.Create("slot");
    public static readonly Identifier MeasureFacet = Identifier.Create("measure");

    public static readonly SeriesKind Assessment = new("assessment", "assessment_series",
        Tuned("assessment_series", Duration.FromDays(365), Duration.FromHours(1), Duration.FromDays(1), Duration.FromDays(3)),
        Seq<Identifier>());
    public static readonly SeriesKind Sensor = new("sensor", "sensor_series",
        Tuned("sensor_series", Duration.FromDays(90), Duration.FromMinutes(15), Duration.FromDays(1), Duration.FromDays(3)),
        Seq<Identifier>());
    // Receipt-stream measures: the fan projects each numeric receipt field as one point under its domain, slot, and
    // measure path, so a board tile reads a one-minute continuous aggregate instead of scanning the evidence plane
    // while the receipt itself stays the truth this projection derives from. The measure facet is what makes a tile
    // expressible in TEXT — a series key is a content hash no dashboard can spell.
    public static readonly SeriesKind Telemetry = new("telemetry", "telemetry_series",
        Tuned("telemetry_series", Duration.FromDays(90), Duration.FromMinutes(1), Duration.FromHours(6), Duration.FromDays(1)),
        Seq(DomainFacet, SlotFacet, MeasureFacet));

    public string Table { get; }
    // Retention, grain, chunk, and backfill ride the residence family's OWN policy value rather than four columns a
    // property then re-assembles: the emitter takes a `ResidencePolicy`, so the row that answers it holds one.
    public ResidencePolicy Policy { get; }
    public Seq<Identifier> Facets { get; }

    private SeriesKind(string key, string table, ResidencePolicy policy, Seq<Identifier> facets) : this(key) =>
        (Table, Policy, Facets) = (table, policy, facets);

    // Relational tiers rest in their own table, so this residence root IS the relation name and the Lake arm alone
    // reads the column as a hive generation directory. The mint is named apart from the `Policy` property because a
    // member group sharing a property's name does not compile.
    static ResidencePolicy Tuned(string table, Duration retain, Duration grain, Duration chunk, Duration backfill) =>
        new(retain, grain, chunk, backfill, StorePath.Create(table));

    // One projection into the residence family's own schema shape, so ONE provisioning emitter serves the hypertable
    // roster and every producer-handed dataset alike and no second DDL path exists.
    public AnalyticsSchema Schema => new(Table,
        Seq(SeriesColumn) + Facets,
        Seq(new ColumnRow(SeriesColumn, ColumnType.KeyHex, Nullable: false))
            + Facets.Map(static facet => new ColumnRow(facet, ColumnType.Utf8, Nullable: false))
            + Seq(new ColumnRow(AtColumn, ColumnType.Timestamp, Nullable: false),
                  new ColumnRow(ValueColumn, ColumnType.Float64, Nullable: false)),
        Time: AtColumn, Spine: TimeSpine.Event, Measure: Some(ValueColumn));

    // Declared shape of the relation `SeriesResidence` materialises, so the rollup read resolves its narrowings and
    // its ordering against a roster rather than against the raw table's. Weight and sketch columns stay ABSENT by
    // construction: each holds a toolkit summary the neutral vocabulary carries no token for, which is exactly why
    // both accessor folds that read them name no column, reaching the provisioning arm's own declaration instead.
    public AnalyticsSchema Rollup => new((string)SeriesResidence.Rollup(Schema),
        Seq(SeriesColumn) + Facets,
        Seq(new ColumnRow(SeriesColumn, ColumnType.KeyHex, Nullable: false))
            + Facets.Map(static facet => new ColumnRow(facet, ColumnType.Utf8, Nullable: false))
            + Seq(new ColumnRow(SeriesResidence.Bucket, ColumnType.Timestamp, Nullable: false),
                  new ColumnRow(SeriesResidence.Low, ColumnType.Float64, Nullable: false),
                  new ColumnRow(SeriesResidence.High, ColumnType.Float64, Nullable: false),
                  new ColumnRow(SeriesResidence.Samples, ColumnType.Int64, Nullable: false)),
        Time: SeriesResidence.Bucket, Spine: TimeSpine.Event, Measure: None);
}

// Ingest row: `Series` is the content-key identity the source artifact already carries, `At` the sample instant,
// `Value` the measure, and `Facets` the ordered text values binding positionally against the kind's own roster.
public readonly record struct SeriesPoint(UInt128 Series, Instant At, double Value, Seq<string> Facets);

// Series selection discriminates on the VALUE's own shape: a caller holding the source artifact's content key
// names that key, and a board naming its stream names the facet values. Without the facet arm the whole telemetry
// projection is unreadable from a dashboard, so the two arms are one narrowing roster and neither read forks.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SeriesSelector {
    private SeriesSelector() { }
    public sealed record Key(UInt128 Series) : SeriesSelector;
    public sealed record Facets(Seq<string> Values) : SeriesSelector;
}

// --- [MODELS] -----------------------------------------------------------------------------
// One raw-chunk weighted mean per matching series; `Series` is the identity a facet selection resolves to, so a
// caller reading a single stream reads one row and a caller reading a family reads each stream apart.
public readonly record struct SeriesWeight(UInt128 Series, double Weighted);

// One rollup row per (bucket, series): `Mean` is the time-weighted accessor over the materialised summary, `Tail`
// names the sketch quantile the read asked for, so a tile's caption and the statistic behind it are one choice.
public readonly record struct SeriesBucket(Instant Bucket, UInt128 Series, double Mean, double Tail, double Low, double High, long Samples);

// One `job_stats` row per Timescale background job on the kind's hypertable — the WHICH-policy-stalled detail the
// family `ResidenceHealth` outcome probe cannot name.
public readonly record struct SeriesJobHealth(string Hypertable, string Status, Option<Instant> LastSuccessfulFinish, long TotalFailures);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SeriesLane {
    // Provisioning rides the one residence emitter over the kind's own schema projection, and derived steps ride
    // that same reviewed generation rail every `Store/provisioning#SERVER_EXTENSIONS` admission rides, gated on the
    // verdict holding the `timescaledb` lane.
    public static Fin<Seq<ProvisionStep>> Provision(SeriesKind kind) =>
        ResidenceDdl.Provision(Residence.Series, kind.Schema, kind.Policy);

    // Hypertable-family ARM of the one residence landing: this projects points into the kind's own declared column
    // order and `ResidenceLanding.Stage` owns the copy loop, the tenancy lead, and the wire types, so a spine column
    // add moves the schema projection alone. Facet arity refuses inside the shared conformance gate.
    public static IO<Fin<ResidenceIngestReceipt>> Ingest(NpgsqlDataSource store, SeriesKind kind, Seq<SeriesPoint> points, ProjectionContext frame) =>
        ResidenceLanding.Stage(store, kind.Schema, points.Map(Cells), frame);

    // Declaration order IS the projection order — key, facets, instant, measure — read off `SeriesKind.Schema`
    // rather than re-spelled, so the DDL, the COPY roster, and this fold cannot disagree on position.
    static Seq<ColumnCell> Cells(SeriesPoint point) =>
        Seq<ColumnCell>(new ColumnCell.Key(point.Series))
        + point.Facets.Map(static facet => (ColumnCell)new ColumnCell.Text(facet))
        + Seq<ColumnCell>(new ColumnCell.Moment(point.At), new ColumnCell.Real(point.Value));

    // Toolkit time-weighted read over RAW chunks, the grain below the rollup's bucket: each sample weighs by its
    // holding interval, the honest mean for irregular timesteps a naive `avg` over-counts. This fold rides the plan
    // builder's own `Weighted` row, so the `time_weight` text lives at the lowering that owns it while this arm
    // names the question alone.
    public static IO<Fin<ResidenceResult<SeriesWeight>>> Weighted(
        ResidenceReach reach, SeriesKind kind, SeriesSelector selector, ResidenceWindow window, ProjectionContext frame) =>
        Served(kind, kind.Schema, selector, window, frame, ResidenceProjection.Aggregate,
            (schema, matches) => ResidencePlan.Aggregate(schema, schema.Table, matches,
                Seq(SeriesKind.SeriesColumn),
                Seq((WeightedColumn, (ResidenceFold)new ResidenceFold.Weighted(SeriesKind.ValueColumn)))),
            reach, static (residence, row) =>
                (row.Key(residence, 0).ToValidation<Error>(), row.Real(residence, 1).ToValidation<Error>())
                .Apply(static (series, weighted) => new SeriesWeight(series, weighted)).As().ToFin());

    // Pre-bucketed read off the continuous aggregate the Series arm emitted: the relation, the bucket axis, and the
    // two accessor folds all read that arm's own declaration, so the reader binds by an ordinal the projected name
    // list owns rather than by a column list two sites spell. Accessors run over the SAME materialised summaries
    // that the raw-chunk read folds live, so cheap tile and expensive investigation answer one statistic.
    public static IO<Fin<ResidenceResult<SeriesBucket>>> Bucketed(
        ResidenceReach reach, SeriesKind kind, SeriesSelector selector, ResidenceWindow window,
        double quantile, QuantileRule rule, ProjectionContext frame) =>
        Served(kind, kind.Rollup, selector, window, frame, ResidenceProjection.Quantile,
            (schema, matches) => ResidencePlan.Project(schema, schema.Table, matches,
                Seq((SeriesResidence.Bucket, (ResidenceFold)new ResidenceFold.Plain(SeriesResidence.Bucket)),
                    (SeriesKind.SeriesColumn, new ResidenceFold.Plain(SeriesKind.SeriesColumn)),
                    (SeriesResidence.Weight, new ResidenceFold.Mean()),
                    (SeriesResidence.Sketch, new ResidenceFold.Tail(quantile, rule)),
                    (SeriesResidence.Low, new ResidenceFold.Plain(SeriesResidence.Low)),
                    (SeriesResidence.High, new ResidenceFold.Plain(SeriesResidence.High)),
                    (SeriesResidence.Samples, new ResidenceFold.Plain(SeriesResidence.Samples))),
                Seq(SeriesResidence.Bucket)),
            reach, static (residence, row) =>
                (row.At(residence, 0).ToValidation<Error>(), row.Key(residence, 1).ToValidation<Error>(),
                 row.Real(residence, 2).ToValidation<Error>(), row.Real(residence, 3).ToValidation<Error>(),
                 row.Real(residence, 4).ToValidation<Error>(), row.Real(residence, 5).ToValidation<Error>(),
                 row.Whole(residence, 6).ToValidation<Error>())
                .Apply(static (bucket, series, mean, tail, low, high, samples) =>
                    new SeriesBucket(bucket, series, mean, tail, low, high, samples)).As().ToFin());

    // ONE serving order both reads share: prove the selector's narrowings against the roster, build the plan, and
    // hand it to the one query entry under the scope that carries tenant and window. Scope is the frame's and the
    // window's — never a predicate this page writes — so neither read can express an unbounded or cross-tenant scan.
    static IO<Fin<ResidenceResult<T>>> Served<T>(
        SeriesKind kind, AnalyticsSchema schema, SeriesSelector selector, ResidenceWindow window, ProjectionContext frame,
        ResidenceProjection projection, Func<AnalyticsSchema, Seq<(Identifier Column, string Value)>, Fin<Plan>> build,
        ResidenceReach reach, Func<Residence, ResidenceRow, Fin<T>> shape) =>
        Narrowings(kind, selector).Bind(matches => build(schema, matches)).Match(
            Succ: plan => ResidenceRead.Read(reach, plan,
                new ResidenceScope(Residence.Series, schema, window, frame), projection,
                row => shape(Residence.Series, row)),
            Fail: error => IO.pure(Fin<ResidenceResult<T>>.Fail(error)));

    // Selector narrowings as ROWS the plan builder renders, so a facet added to a kind moves the roster and both
    // reads together. Facet arity the roster does not match refuses BEFORE any plan assembles, where a short row
    // set lowers a predicate naming fewer columns than the family declares and reads every sibling stream.
    // Keys cross as hex TEXT because a `KeyHex` column carries no Substrait literal at all — the builder's
    // own key narrowing then renders it through the residence's `bytea` spelling.
    static Fin<Seq<(Identifier Column, string Value)>> Narrowings(SeriesKind kind, SeriesSelector selector) =>
        selector.Switch(
            state: kind,
            key:    static (_, row) => Fin.Succ(Seq((SeriesKind.SeriesColumn, ContentHash.Hex(row.Series)))),
            facets: static (family, row) => row.Values.Count == family.Facets.Count
                ? Fin.Succ(family.Facets.Zip(row.Values).Map(static pair => (pair.Item1, pair.Item2)).Strict())
                : Fin.Fail<Seq<(Identifier, string)>>(new ResidenceFault.ReadRefused(
                    Residence.Series.Key, new EngineFault("<facet-arity>", family.Key))));

    // Timescale-ONLY enrichment beside the family probe: `ResidenceRead.Health` measures the expiry OUTCOME every
    // residence answers, while this reads the bgworker run history only the Timescale catalog publishes — a failed
    // status, or a `last_successful_finish` older than twice the job's schedule interval, names WHICH policy stalled
    // where the outcome probe reports only that residue survived. The catalog relation carries no tenant column and
    // no time spine, so it has no `ResidenceScope` and rides the ordinal reader the boundary carve admits.
    public static IO<Fin<Seq<SeriesJobHealth>>> Jobs(NpgsqlDataSource store, SeriesKind kind) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async token => {
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
        }).ConfigureAwait(false)).MapFail(Residence.Series.ReadRefused));

    // Projected names the two folds carrying no source column bind under, so the reader's ordinals and the plan's
    // root relation read one roster.
    public static readonly Identifier WeightedColumn = Identifier.Create("weighted");
}
```

## [03]-[WAREHOUSE_OPLOG]

- Owner: `WarehouseSchema` declares the Fleet residence's op-log dataset as ONE `AnalyticsSchema` value; `WarehouseOpRow` is the typed row a fleet question composes over.
- Entry: `WarehouseSchema.Dataset` is the declaration the residence DDL, the egress sink's insert column list, and every reader's ordinals derive from; `Shape(Residence, ResidenceRow)` is the reader over the one row surface every reach yields.
- Auto: the sink lands EXACTLY these columns projected from `Egress.Envelope` — `id` the content key, `source`/`type`/`time` the message-envelope attributes, `partition_key`/`sequence` the partitioning extensions, `data` the redacted payload — so writer and reader cannot drift while naming one relation. Every column declares `Nullable: false`, so the seven reads compose as ONE applicative product and a residence answering an empty cell names every offending column at once.
- Receipt: this dataset lands through `Version/egress`'s sink and reads through `store.columnar.residence.read`; it declares no slot of its own.
- Packages: Rasm.Persistence (`Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnType`, `#RESIDENCE_FAMILY` `Residence`/`ResidenceFault`, `Query/serving#SERVING_PLANE` `ResidenceRow`), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op-log column is one `ColumnRow` beside one reader ordinal moving together; zero new surface — a second row vocabulary, a hand-spelled insert column list, or a per-end declaration is the deleted form.
- Law: op-log rows spell their instant `time`, not `at` — exactly why the residence spine travels as a schema column: this dataset provisions its MergeTree partition, sort key, and TTL through the same arm every `at`-spelled series relation rides, with no dialect arm branching on a column name.
- Boundary: the Fleet leg is READ-side only. `Version/egress`'s ClickHouse sink owns landing under `insert_deduplication_token` dedup, and the two ends meet at this declaration rather than at a table name two sites spell; ClickHouse carries no transaction, so every fleet read is a convergence-consistent view whose staleness the egress cursor bounds.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// ONE warehouse row vocabulary both ends of the Fleet seam read.
public sealed record WarehouseOpRow(string Id, string Source, string Type, Instant Time, string PartitionKey, long Sequence, ReadOnlyMemory<byte> Data);

// --- [OPERATIONS] -------------------------------------------------------------------------
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

    // Ordinals read off the declaration above, so a column insert moves the reader and the DDL together. The seven
    // reads ACCUMULATE: a row whose id and payload are both empty names both, where a first-defect product would
    // send the fleet leg back for a second round trip over a batch it already scanned.
    public static Fin<WarehouseOpRow> Shape(Residence residence, ResidenceRow row) =>
        (row.Text(residence, 0).ToValidation<Error>(), row.Text(residence, 1).ToValidation<Error>(),
         row.Text(residence, 2).ToValidation<Error>(), row.At(residence, 3).ToValidation<Error>(),
         row.Text(residence, 4).ToValidation<Error>(), row.Whole(residence, 5).ToValidation<Error>(),
         row.Text(residence, 6).ToValidation<Error>())
        .Apply(static (id, source, type, time, partition, sequence, data) =>
            new WarehouseOpRow(id, source, type, time, partition, sequence, Encoding.UTF8.GetBytes(data))).As().ToFin();
}
```

## [04]-[RECEIPT_EVIDENCE]

- Owner: `ReceiptFactRow` is the kernel `ReceiptEnvelope` flattened to one wide-event row per emission; `ReceiptResidence` declares one dataset per capability domain and owns the five projections over it — the wide-event fold, the numeric measure fan, the cold-tail record batch, the named scan plan, and the resident message-envelope read.
- Entry: `ReceiptResidence.Dataset(string domain)` is the declaration; `Facts(Seq<ReceiptEnvelope>)` folds the wide-event rows; `Points(Seq<ReceiptEnvelope>)` projects the numeric leaves onto `SeriesKind.Telemetry`; `Batch(string domain, Seq<ReceiptEnvelope>, Seq<(string Key, string Value)> metadata)` is the cold-tail record batch; `Scan(string domain, Option<CorrelationId>)` is the named plan; `Resident(ResidenceReach, ResidenceScope, string domain, Option<CorrelationId>)` is the durable read; `Publish(…)` is the artifact generation.
- Auto: every column derives from the message envelope the sink already sealed, so the residence carries envelope-grade provenance and re-measures nothing; the batch derives from `Dataset(domain)` through `ArrowLanding.Build`, so field order, physical type, and every reader's ordinals come from one declaration and no positional column list or per-type builder helper survives beside it. Any receipt payload gaining a numeric field gains its own series with ZERO rows here.
- Receipt: this plane declares no slot of its own — it is the residence the whole receipt stream lands in, and its reads ride `store.columnar.residence.read`.
- Packages: Apache.Arrow (`RecordBatch`), Rasm (`Domain/identity#CONTENT_KEY` `CanonicalWriter`/`ContentHash` — the framed preimage every series key derives from, `Domain/receipt#RECEIPT_ENVELOPE` `ReceiptEnvelope`/`CorrelationId`), Rasm.Persistence (`Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnCell`/`ArrowLanding`, `Query/serving#READ_PLAN` `ResidencePlan`/`ResidenceScope`, `#SERVING_PLANE` `ResidenceRead`/`ResidenceRow`, `Query/columnar#ARTIFACT_EGRESS` `ArtifactClass`/`CopyBody`/`ArtifactEgress`), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new evidence column is one `ColumnRow` beside one reader ordinal; a new capability domain is a `Dataset(domain)` call and nothing else; zero new surface — a hand-built `Schema`, a per-type Arrow builder, a second flat-row twin of the causal frame, or a second ordinal roster is the deleted form.
- Law: `Tenant` is the ROUTING key, not a stored column — each residence owns its tenant column at the one key type every tenancy predicate compares against and every scan is tenant-scoped by frame, so a per-row tenant column duplicates that key at a second physical type. Wide events declare NO measure: a message envelope carries a payload, not a scalar, so this dataset provisions hypertable, columnstore, and retention and emits no continuous aggregate — numeric rollup rides the `SeriesKind.Telemetry` projection `Points` derives.
- Boundary: every identity part of a series key crosses the kernel `CanonicalWriter`, which length-frames each variable-width field — a raw concatenation of package, kind, and measure path mints ONE key for `("rasm.store", "a", "b")` and `("rasm.store", "ab", "")`, folding two streams into one series no reader can separate. Arrays stay off the measure plane — a per-row collection is evidence the wide event carries whole, never a scalar a time bucket averages — and the walk is depth-bounded so a nested payload cannot fan unbounded series out of one message envelope. Payloads CLONE onto their own buffer at the envelope inverse: `JsonElement` outlives no `JsonDocument`, so an element handed out past the parse scope reads returned pooled memory.

```csharp signature
using Apache.Arrow;
using Rasm.Domain;                                // CanonicalWriter/ContentHash — the framed preimage and its digest
using System.Text.Json;

// --- [MODELS] -----------------------------------------------------------------------------
// Receipt EVIDENCE plane: the kernel `ReceiptEnvelope` flattened to one wide-event row per emission. The payload
// crosses as its own JSON text, so a residence scan never re-decodes a foreign package's shape.
public readonly record struct ReceiptFactRow(
    string Package, string Kind, string Domain, string Correlation, string Tenant,
    Instant At, long Logical, long SkewNanos, string Payload);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ReceiptResidence {
    // One dataset per CAPABILITY DOMAIN under the `telemetry.<domain>` grammar, so a residence query joins on the
    // same domain segment a metric name carries and a scan never crosses two subjects.
    public static AnalyticsSchema Dataset(string domain) => new($"telemetry.{domain}",
        Seq(PackageColumn, KindColumn, AtColumn),
        Seq(new ColumnRow(PackageColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(KindColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(DomainColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(CorrelationColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(AtColumn, ColumnType.Timestamp, Nullable: false),
            new ColumnRow(LogicalColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(SkewColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(PayloadColumn, ColumnType.Utf8, Nullable: false)),
        Time: AtColumn, Spine: TimeSpine.Event, Measure: None);

    // Total domain projection: a dotted slot carries its capability domain in the second segment exactly as the
    // `store.<domain>.<verb>` grammar declares, and a flat kind literal falls to the emitting package id — so every
    // message envelope resolves a domain and none lands under an empty partition key a scan would then read whole.
    public static string Domain(ReceiptEnvelope envelope) =>
        envelope.Kind.Split('.') is [_, var domain, ..] && domain.Length > 0 ? domain : envelope.Package;

    // One pure fold — every column derives from the message envelope the sink already sealed.
    public static Seq<ReceiptFactRow> Facts(Seq<ReceiptEnvelope> envelopes) =>
        envelopes.Map(static envelope => new ReceiptFactRow(
            envelope.Package, envelope.Kind, Domain(envelope),
            envelope.Correlation.ToString(), envelope.Tenant.Entry,
            envelope.Physical, (long)envelope.Logical,
            (long)envelope.SkewBound.ToInt64Nanoseconds(),
            envelope.Payload.GetRawText()));

    // MEASURE PROJECTION: every numeric leaf of a receipt payload is one point on the `SeriesKind.Telemetry`
    // hypertable, keyed by the dotted path it sits at, so a board tile filters `(domain, slot, measure)` in TEXT
    // rather than a content hash no dashboard can spell.
    const int MeasureDepth = 4;

    // Identity FRAMES rather than concatenates: the writer emits each field's UTF-8 byte count ahead of its bytes,
    // which is the whole reason a dotted measure path cannot merge with the kind segment ahead of it.
    public static Seq<SeriesPoint> Points(Seq<ReceiptEnvelope> envelopes) =>
        envelopes.Bind(static envelope => Measures(envelope.Payload, string.Empty, MeasureDepth)
            .Map(measure => new SeriesPoint(
                Series: ContentHash.Of((envelope.Package, envelope.Kind, measure.Path),
                    static (parts, writer) => writer.String(parts.Package).String(parts.Kind).String(parts.Path)),
                At: envelope.Physical,
                Value: measure.Value,
                Facets: Seq(Domain(envelope), envelope.Kind, measure.Path))));

    static Seq<(string Path, double Value)> Measures(JsonElement node, string path, int depth) =>
        node.ValueKind switch {
            JsonValueKind.Number when path.Length > 0 => Seq((path, node.GetDouble())),
            JsonValueKind.Object when depth > 0 => toSeq(node.EnumerateObject()).Bind(property =>
                Measures(property.Value, path.Length == 0 ? property.Name : $"{path}.{property.Name}", depth - 1)),
            _ => Seq<(string, double)>(),
        };

    // COLD-TAIL handoff: `Query/lakehouse#FLAT_TABLE_EGRESS`'s `LandingArm.Receipt` generation lands Arrow batches,
    // so the custodian projects its own evidence rows through the ONE record-batch fold. Column order, field types,
    // and every conformance proof derive from `Dataset(domain)`, and `metadata` carries the producer's receipt facts
    // onto the schema — the one seat Arrow's builders expose no property for.
    public static Fin<RecordBatch> Batch(string domain, Seq<ReceiptEnvelope> envelopes, Seq<(string Key, string Value)> metadata) =>
        ArrowLanding.Build(Dataset(domain), Facts(envelopes), Cells, metadata);

    // Declaration order IS the cell order, so the fold that lands a batch and the fold that stages a COPY read one
    // roster and no per-type builder helper survives beside the row set.
    static Seq<ColumnCell> Cells(ReceiptFactRow fact) => Seq<ColumnCell>(
        new ColumnCell.Text(fact.Package), new ColumnCell.Text(fact.Kind), new ColumnCell.Text(fact.Domain),
        new ColumnCell.Text(fact.Correlation), new ColumnCell.Moment(fact.At), new ColumnCell.Whole(fact.Logical),
        new ColumnCell.Whole(fact.SkewNanos), new ColumnCell.Text(fact.Payload));

    // ONE named question over the evidence plane, composed on the family's own plan builder: consumers name whichever
    // correlation they reconstruct and take the plan, so no page assembles Substrait relations. Scope — tenant and
    // window — rides the read frame, so a correlation-free call is exactly the whole-window scan a usage fold reads.
    public static Fin<Plan> Scan(string domain, Option<CorrelationId> correlation) =>
        ResidencePlan.Scan(Dataset(domain), correlation.Map(id => (CorrelationColumn, id.ToString())).ToSeq());

    // DURABLE counterpart to the in-process sink, and the producing half of the `[RECEIPT]: resident ReceiptEnvelope`
    // seam: the read hands back the SAME message-envelope values the live sink held, so
    // `Rasm.AppUi/Diagnostics/evidence#CORRELATION_JOIN`'s `EvidenceSource.Resident` arrow binds here and an incident
    // reconstructs after the process that emitted it is gone.
    public static IO<Fin<ResidenceResult<ReceiptEnvelope>>> Resident(
        ResidenceReach reach, ResidenceScope scope, string domain, Option<CorrelationId> correlation) =>
        Scan(domain, correlation).Match(
            Succ: plan => ResidenceRead.Read(reach, plan, scope, ResidenceProjection.Point,
                row => Envelope(scope, row)),
            Fail: error => IO.pure(Fin<ResidenceResult<ReceiptEnvelope>>.Fail(error)));

    // Egress rides the ONE `COPY (SELECT) TO` rail: the domain partition key is the artifact class's own row and the
    // stamp is the caller's content address, so an evidence generation carries its identity in the Parquet footer.
    public static IO<Fin<Unit>> Publish(ColumnarSession session, string domain, StorePath destination, UInt128 stamp) =>
        ArtifactEgress.Publish(session, ArtifactClass.TelemetryEvidence,
            new CopyBody(Dataset(domain).Table, Dataset(domain).Columns.Map(static column => column.Name)),
            destination, stamp);

    public static readonly Identifier PackageColumn = Identifier.Create("package");
    public static readonly Identifier KindColumn = Identifier.Create("kind");
    public static readonly Identifier DomainColumn = Identifier.Create("domain");
    public static readonly Identifier CorrelationColumn = Identifier.Create("correlation");
    public static readonly Identifier AtColumn = Identifier.Create("at");
    public static readonly Identifier LogicalColumn = Identifier.Create("logical");
    public static readonly Identifier SkewColumn = Identifier.Create("skew_nanos");
    public static readonly Identifier PayloadColumn = Identifier.Create("payload");

    // Reader inverse over the one row surface: ordinals read off `Dataset`'s own declaration through the root
    // relation's projected names, so physical residence column order never reaches a consumer. The eight reads
    // ACCUMULATE, so a corrupt row names every empty column at once. Tenant returns from the frame each read scoped
    // with — that being the only tenant a tenant-scoped scan can have returned.
    public static Fin<ReceiptFactRow> Shape(ResidenceScope scope, ResidenceRow row) =>
        (row.Text(scope.Residence, 0).ToValidation<Error>(), row.Text(scope.Residence, 1).ToValidation<Error>(),
         row.Text(scope.Residence, 2).ToValidation<Error>(), row.Text(scope.Residence, 3).ToValidation<Error>(),
         row.At(scope.Residence, 4).ToValidation<Error>(), row.Whole(scope.Residence, 5).ToValidation<Error>(),
         row.Whole(scope.Residence, 6).ToValidation<Error>(), row.Text(scope.Residence, 7).ToValidation<Error>())
        .Apply((package, kind, domain, correlation, at, logical, skew, payload) =>
            new ReceiptFactRow(package, kind, domain, correlation, scope.Frame.Tenant.Entry, at, logical, skew, payload))
        .As().ToFin();

    // `Envelope` inverts the flat row rather than the ordinals a second time — the shape a consuming fold declared
    // against the live sink, so a durable read reaches those folds with no second decode and no ordinal roster that
    // can drift from the one above it.
    public static Fin<ReceiptEnvelope> Envelope(ResidenceScope scope, ResidenceRow row) =>
        Shape(scope, row).Map(fact => {
            using JsonDocument payload = JsonDocument.Parse(fact.Payload);
            return new ReceiptEnvelope(
                CorrelationId.Create(Guid.Parse(fact.Correlation)), scope.Frame.Tenant, fact.Package, fact.Kind,
                payload.RootElement.Clone(), fact.At, (ulong)fact.Logical, Duration.FromNanoseconds(fact.SkewNanos));
        });
}
```

| [INDEX] | [POLICY]            | [VALUE]                                   | [BINDING]                                                         |
| :-----: | :------------------ | :---------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | series provisioning | derived from the `SeriesKind` row         | one emitter, generation-carried, verdict-gated                    |
|  [02]   | policy cadence      | each residence's own scheduler            | never AppHost-scheduled; `job_stats` is the Series proof row      |
|  [03]   | irregular timesteps | the `Weighted` fold off the plan builder  | a naive `avg` over-counts dense bursts                            |
|  [04]   | rollup statistic    | materialised summary + read-time accessor | one statistic per caption; never `avg` accelerating `time_weight` |
|  [05]   | series identity     | a `CanonicalWriter`-framed preimage       | length-framed fields; a concatenated key merges two streams       |
|  [06]   | measure projection  | numeric leaves under a facet triple       | text facets name a stream; a hash names nothing                   |
|  [07]   | batch handoff       | `ArrowLanding.Build` over the declaration | metadata is required; never a schema built beside the dataset     |
|  [08]   | fleet leg           | READ row over one declared op-log dataset | the egress sink owns landing; never a second SoR                  |

## [05]-[RESEARCH]

(none)

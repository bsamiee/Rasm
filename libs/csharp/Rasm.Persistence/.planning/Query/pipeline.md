# [PERSISTENCE_PIPELINE]

Rasm.Persistence drives large import, export, and transform across Arrow, Parquet, CSV, IFC, and Speckle as one back-pressured fold over the `Query/rail#ARROW_PLANE` `ArrowChunk` zero-copy carrier — never an ad hoc per-format batch loop. `PipelineStage` is the closed `[Union]` stage family (`Extract`, `Stage`, `Validate`, `Transform`, `Load`, `Reconcile`) whose `Transform` carries a five-leg `TransformSpec` (`Analytical`/`Scalar`/`Relation` over DuckDB, `Flight`/`Adbc` over Arrow), `StageStep` the `[Union]` per-chunk outcome carrying its own typed receipt (`Drained`/`Staged`/`Resumed`/`Validated`/`Transformed`/`Loaded`/`Reconciled`/`Poisoned`/`Shed`) projected through one generated total `Switch` so a new outcome breaks every dispatch site at compile time, and `BulkPipeline` folds the ladder one chunk wide so peak managed memory is one vector quantum regardless of source size, sealing the typed run evidence with the per-chunk poison roster a re-drive replays. Idempotency reuses the settled `Version/snapshots#CONTENT_CHUNKING` `ContentChunker.Novel(manifest, mayHold, holds)` projection over the FastCDC `ContentChunk.ShortTag` 64-bit bloom pre-filter ahead of the `XxHash128` content compare, dedup keyed against the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow` lane; a poison chunk routes to the `DeadLetter` quarantine carrying a typed `PipelineFault` and a `Schedule`-cadenced retry budget that only re-drains a `IsTransient` fault rather than aborting the run; back-pressure rides the `Query/rail#BULK_LANE` `BulkShed` `StoreFact.BulkShed` fact off the existing stream; reconcile folds source against target through `MergeWithOutputAsync` RETURNING into a `ReconcileLedger` whose conservation identity is the closed `PipelineFault.Unconserved` on breach. `PipelineFault` is the closed `[Union]` fault family deriving from `Expected` in the 7300 band (`ExtractMissing` | `TransformFaulted` | `Unconserved` | `CapacityBreached` | `PoisonExhausted`) so every distinct pipeline cause carries its own recovery identity rather than a bare untyped `Error`. `DuckDB.NET.Data.Full` carries the analytical, UDF-scalar, and registered-relation transform legs and the lakehouse export, `Apache.Arrow.Flight`/`Apache.Arrow.Adbc` the remote-engine Flight-ticket and ADBC-pull transform legs (driving the settled `ArrowEgress` surface, never a second client), and `linq2db.EntityFrameworkCore` the `MergeWithOutputAsync` reconcile RETURNING image; `ArrowChunk`/`ArrowPlane`/`ArrowEgress`, `BulkRoute`/`BulkReceipt`/`BulkDelta`/`BulkShed`, `TabularExportSpec`/`LakehouseCatalog`/`AnalyticalTraversal`, `ContentChunker`/`ContentChunk`/`ChunkManifest` (with the `FastCDC.Net` cut and the `System.IO.Hashing` `XxHash128`/`XxHash3` content keys it owns), `ClockPolicy`, and `ReceiptSinkPort` arrive settled — the chunker and the hashers are reached through the settled `Version/snapshots#CONTENT_CHUNKING` owner, never re-bound here. The `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIo` parse-to-canonical-bytes — content-addressed onto the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Interchange` row through `InterchangeIdentity.Admit` — is the one `Extract` source this owner reads; the IFC/Speckle/Parquet/CSV parse is the Compute concern, never minted here.

Wire posture: this page is host-local — the pipeline stages execute over the in-process columnar carrier and the op-log, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The `Extract` source arrives from the `Query/pipeline ⇄ Rasm.Compute/Runtime/codecs # [PORT]: parse-to-canonical-bytes (Extract)` seam as settled canonical bytes the Compute interchange lane already content-addressed.

## [01]-[INDEX]

- [01]-[PIPELINE_STAGE]: the stage union, the typed per-chunk outcome, the closed `PipelineFault` family, the idempotency address, the dead-letter quarantine, and the reconcile ledger.
- [02]-[BULK_PIPELINE]: the `ArrowChunk` chunk-wide fold, the five `TransformSpec` legs (DuckDB analytical/UDF/relation, Arrow Flight/ADBC), the back-pressure shed, the poison roster, and the op-log/parquet terminators.

## [02]-[PIPELINE_STAGE]

- Owner: `PipelineStage` the closed `[Union]` stage family threading the source descriptor, the staging cursor, the validation rule set, the transform spec, the load route, and the reconcile target, projecting one canonical `Label` per case through its generated total `Switch`; `TransformSpec` the closed `[Union]` transform leg (`Analytical` in-engine SQL | `Flight` remote ticket | `Adbc` driver pull | `Relation` registered-carrier join | `Scalar` UDF) carrying no `bool`/`default` ghost; `ReconcilePolicy` the `[SmartEnum<string>]` row carrying the `InsertsNew`/`UpdatesMatched`/`DeletesAbsent` merge-clause triple that drives the `MergeWithOutputAsync` `UpdateWhenMatched`/`InsertWhenNotMatched`/`DeleteWhenNotMatchedBySource` arms (`Append`/`Upsert`/`Mirror`/`Prune`); `StageStep` the `[Union]` per-chunk outcome carrying the stage-specific typed receipt rather than a flat row count, every projection (`Kind`/`At`/`RowCount`) through the generated total `Switch` so a new case breaks every site at compile time; `PipelineFault` the closed `[Union]` fault family deriving from `Expected, IValidationError<PipelineFault>` in the 7300 band whose `From(stage, error)` is the one boundary that converts a provider `Error` into a typed cause; `ChunkGate` the idempotency seam folding the `Version/snapshots#CONTENT_CHUNKING` `ContentChunker.Novel` `(mayHold, holds)` probe against the `Query/cache#ARTIFACT_BLOB_INDEX` dedup lane; `DeadLetter` the typed quarantine record carrying a `PipelineFault` and a `Schedule`-cadenced retry budget; `ReconcileLedger` the per-verdict source-against-target merge accounting whose generic `Fold<TRow>` classifies the `BulkDelta<TRow>` image — comparing the `Old`/`New` `IEquatable<TRow>` pair so a no-op merge increments `Unchanged` rather than mis-counting `Updated` — and whose conservation identity is `PipelineFault.Unconserved` on breach; `StagingCursor` the resumable crash-resume mark; `PipelineFact` with `PipelineFactKind` the page-wide fact stream; `PipelineReceipt` the typed run evidence carrying the per-stage counts and the `Seq<DeadLetter>` poison roster a re-drive reads.
- Cases: `Extract(SourceFormat, ArtifactIndexRow Source)` (parse-to-canonical-bytes the Compute lane content-addressed), `Stage(StagingCursor Cursor)` (land canonical bytes into resumable staging keyed by chunk index), `Validate(Seq<QualityRule> Rules)` (in-flight quality-rule check), `Transform(TransformSpec Spec)` (a DuckDB analytical/UDF/relation or Arrow Flight/ADBC columnar transform), `Load(string Target, BulkRoute Route)` (a `BulkRoute` set/copy/merge into the target), `Reconcile(string Target, string SourceKey, ReconcilePolicy Policy)` (source-against-target merge with verdicts) on `PipelineStage`; `Drained | Staged | Resumed | Validated | Transformed | Loaded | Reconciled | Poisoned | Shed` on `StageStep`, where `Staged` carries the resumable cursor and `Resumed` carries the skipped-chunk count a crash-resume yields; `Extracted | Staged | Resumed | Validated | Transformed | Loaded | Reconciled | Quarantined | Shed` on `PipelineFactKind`, each fact kind backed by a `StageStep` producer through `StageStep.Kind`; `ExtractMissing(7300) | TransformFaulted(7301) | Unconserved(7302) | CapacityBreached(7303) | PoisonExhausted(7304)` on `PipelineFault`, each carrying its `IsTransient` recovery discriminant.
- Entry: `public Seq<ContentChunk> Admit(ChunkManifest manifest)` on `ChunkGate` is the idempotency gate — it folds the settled `Version/snapshots#CONTENT_CHUNKING` `ContentChunker.Novel(manifest, MayHold, Holds)` projection so only the chunks the dedup lane lacks survive, probing the cheap 64-bit `ShortTag` bloom (`MayHold`) ahead of the authoritative `XxHash128` content-key compare (`Holds`) against the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow` lane so a tag-miss proves a chunk novel without the full lookup and a re-run skips an already-applied chunk; `public PipelineStage Relink(PipelineStage authored, StageStep step)` threads the prior step's resolved cursor, target, and schema onto the caller-authored next stage so no rule set, SQL, or route is fabricated — the ladder carries the authored config and `Relink` only advances the dynamic fields the prior outcome resolves; `DeadLetter.Capture` quarantines a failing chunk with its stage, typed `PipelineFault`, and retry budget so a dead-letter capture is one record.
- Auto: the stages form one ladder over the `ArrowChunk` carrier — `Extract` reads the Compute exchange parse-to-canonical-bytes off the `Source` `ArtifactIndexRow` (the IFC/Speckle/Parquet/CSV parse is the Compute concern), `Stage` lands the canonical bytes into resumable staging keyed by `StagingCursor.ChunkIndex` so a crash resumes from the last staged chunk through the persisted `Watermark`, `Validate` runs the `Store/quality#QUALITY_RULE` rules in-flight so a violation is caught before load, `Transform` runs the `TransformSpec` leg — the DuckDB analytical lane (`Analytical` `GROUP BY`/window SQL), a registered scalar/window UDF (`Scalar`, `RegisterScalarFunction`), an in-process registered-relation join (`Relation`, `ArrowPlane.RegisterCarrier`), the remote Flight ticket (`Flight`, `ArrowEgress.Serve`), or the ADBC driver pull (`Adbc`, `ArrowEgress.Pull`) — all over the zero-copy `ArrowChunk` carrier, `Load` rides the `Query/rail#BULK_LANE` `BulkRoute` set/copy/merge so the load self-emits its changefeed and invalidation in one transaction, and `Reconcile` runs a source-against-target `MergeWithOutputAsync` per-row merge folding the OLD/NEW `BulkDelta<TRow>` image into the `ReconcileLedger` per-verdict counts through its generic `Fold<TRow>`; idempotency reuses the settled `Version/snapshots#CONTENT_CHUNKING` `ContentChunker.Novel(manifest, mayHold, holds)` projection over the FastCDC `ContentChunk` cut — `ChunkGate` binds `MayHold` to the cheap 64-bit `ShortTag` bloom probe and `Holds` to the authoritative `XxHash128` content-key compare against the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow` dedup lane, so a re-run is skip-not-duplicate, a tag false-positive only costs one fall-through `Holds` compare, and the content-defined boundary survives an interior insertion so an edited source re-stages only the changed chunks; back-pressure rides the existing `BulkShed` — a stage exceeding the `CapacityBound` per-shape quantum budget emits a `StageStep.Shed` outcome projecting the `StoreFact.BulkShed` fact and throttles its producer pull-shaped, never a thrown capacity exception; a stage failure routes the failing chunk to the `DeadLetter` quarantine carrying the stage, the typed `PipelineFault`, and the decremented `RetryBudget`, the fold continues, and a quarantine entry re-drains on the next pass only when `Fault.IsTransient` and the budget survives — a terminal `ExtractMissing`/`Unconserved`/`PoisonExhausted` is `Exhausted` immediately so a non-recoverable poison never burns the budget, while a transient fault backs off through the `DeadLetter.Backoff(Schedule, attempt)` cadence so the re-drain spacing is `Schedule` policy, not a tight loop.
- Receipt: each stage rides a `PipelineFact` carrying its `PipelineFactKind` slot, the chunk count, the row count, and the elapsed `Duration`; `StageStep` carries the stage-specific typed receipt — `Drained(long Rows, long Novel)` (the rows drained plus the novel-after-dedup count), `Staged(long Rows, StagingCursor Cursor)` (the staged rows and the advanced cursor), `Resumed(long SkippedChunks, StagingCursor From)` (the crash-resume skip), `Validated(long Passed, long Failed)`, `Transformed(long Rows, string Target)`, `Loaded(BulkReceipt Receipt)` (the settled `Query/rail#BULK_LANE` receipt), `Reconciled(ReconcileLedger Ledger)`, `Poisoned(Seq<DeadLetter> Letters)` (the stage's per-chunk poison set), `Shed(long ShedRows)` — so the run-level fold discriminates the case through the generated `Switch` and sums the stage-typed evidence rather than a generic row count, folding `Validated.Failed` into the receipt `Rejected` count so a quality violation reaches the run evidence rather than vanishing, and a non-zero `Rejected` aborts the run as `PipelineFault.TransformFaulted` before any `Conserve()` mints (violation caught before load); `PipelineReceipt(Stages, Extracted, Staged, Resumed, Validated, Rejected, Transformed, Loaded, Reconciled, Shed, ReconcileLedger Net, Seq<DeadLetter> Quarantine, Duration Elapsed, Instant At)` keys `Stages` by `PipelineFactKind` (never a reflection `GetType().Name` string), carries the `Quarantine` poison roster (each `DeadLetter` its stage, content key, typed fault, and surviving budget) so a re-drive retries the exact poison set rather than re-scanning, and is the typed run evidence feeding `StoreFact`/`StoreEvidence`; a dead-letter capture rides `pipeline.quarantine` carrying the failing stage, typed fault, and surviving budget; a back-pressure shed rides the existing `store.bulk.shed`, never a second signal owner.
- Packages: DuckDB.NET.Data.Full, Apache.Arrow.Flight, Apache.Arrow.Adbc, linq2db.EntityFrameworkCore, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime. The `ArrowChunk` carrier, the `ContentChunker` FastCDC cut, and the `System.IO.Hashing` content keys arrive as settled `Query/rail`/`Version/snapshots` vocabulary, not direct page assets.
- Growth: a new stage is one `PipelineStage` case plus one `StageStep` arm (and the generated `Switch` breaks `Kind`/`At`/`RowCount`/`Absorb` until the new case is handled); a new outcome dimension is one `StageStep` case carrying its typed receipt; a new evidence bucket is one `PipelineFactKind` row backed by a `StageStep` producer; a new reconcile mode is one `ReconcilePolicy` row toggling its `InsertsNew`/`UpdatesMatched`/`DeletesAbsent` triple; a new reconcile verdict is one column on `ReconcileLedger`; a new dead-letter dimension is one column on `DeadLetter`; a new failure cause is one `PipelineFault` case in the 7300 band; a new transform leg is one `TransformSpec` case; zero new surface — a per-format import loop, a second staging table, a parallel dead-letter channel, or a flat reconcile counter is the deleted form because the pipeline is one chunk-wide fold over the one `ArrowChunk` carrier reusing the `ArtifactIndexRow` idempotency, the `BulkRoute` op-log, and the `MergeWithOutputAsync` reconcile.
- Boundary: the pipeline is one back-pressured fold over the existing `ArrowChunk` carrier — an ad hoc batch loop or a managed-array result buffer is the deleted form, the fold threads the engine's zero-copy vector quantum and bounds peak memory to one chunk via the `CapacityBound`; the `Extract` stage reads the Compute interchange-lane parse-to-canonical-bytes through the `Query/pipeline ⇄ Rasm.Compute/Runtime/codecs # [PORT]: parse-to-canonical-bytes (Extract)` seam — `InterchangeIo` parse plus `InterchangeIdentity.Admit` lands the `ExportArtifact` content-addressed on the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Interchange` row this stage's `Source` reads — so Persistence owns staging, validation, transform, load, and reconcile, never a second parser; idempotency reuses the settled `ContentChunker.Novel(manifest, mayHold, holds)` projection over the FastCDC `ContentChunk` cut where `ChunkGate` probes the `ShortTag` bloom ahead of the `XxHash128` content compare against the `ArtifactIndexRow` dedup lane so a re-run is skip-not-duplicate, a second idempotency table or a `ShortTag`-as-content-address is the deleted form, and the chunk boundaries survive an interior insertion so an edited source re-stages only the chunks that changed; the `Stage` cursor persists a `Watermark` so a crash resumes from the last staged chunk index, an unbounded in-memory accumulation is the deleted form; back-pressure rides the existing `BulkShed` `StoreFact.BulkShed` fact off the existing stream so a downstream lane throttles pull-shaped, a thrown capacity exception or a parallel channel is the deleted form; the `DeadLetter` is the typed quarantine with a retry budget so a failing chunk is captured with its stage, fault, and surviving budget and re-drains on the next pass rather than aborting the run, a silent drop or a poison-aborts-run is the deleted form; the `Load` stage rides the `BulkRoute` op-log so the load self-emits its changefeed and invalidation, a trigger-based second write path is the deleted form; the `Transform` stage runs over the DuckDB analytical lane or the Arrow Flight C-data path so a columnar transform is zero-copy, a row-oriented carrier is the deleted form; the `Validate` stage runs the `Store/quality` rules in-flight so a violation is caught before load, an after-the-fact scan is the deleted form; the `Reconcile` stage folds the `MergeWithOutputAsync` OLD/NEW `BulkDelta<TRow>` image into the `ReconcileLedger` through its generic `Fold<TRow>` whose conservation identity (`Total == Inserted + Updated + Deleted + Unchanged`) is the typed `PipelineFault.Unconserved` on breach (`Conserve()`) so a lost row is impossible by construction — the conservation law is the same `Sync/collaboration#MERGE_LAW` `SyncApplyReceipt.Conserves` and `Version/retention#RETENTION_SWEEPS` `SweepReceipt.Conserves` discipline applied to the bulk reconcile, and a flat reconcile counter without conservation is the deleted form.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class PipelineFactKind {
    public static readonly PipelineFactKind Extracted = new("extracted");
    public static readonly PipelineFactKind Staged = new("staged");
    public static readonly PipelineFactKind Validated = new("validated");
    public static readonly PipelineFactKind Transformed = new("transformed");
    public static readonly PipelineFactKind Loaded = new("loaded");
    public static readonly PipelineFactKind Reconciled = new("reconciled");
    public static readonly PipelineFactKind Quarantined = new("quarantined");
    public static readonly PipelineFactKind Shed = new("shed");
    public static readonly PipelineFactKind Resumed = new("resumed");
}

public readonly record struct ChunkGate(Func<ulong, bool> MayHold, Func<UInt128, bool> Holds) {
    public Seq<ContentChunk> Admit(ChunkManifest manifest) => ContentChunker.Novel(manifest, MayHold, Holds);
}

public readonly record struct StagingCursor(long ChunkIndex, Watermark Resume, Instant At) {
    public static readonly StagingCursor Genesis = new(0L, Watermark.Start, Instant.MinValue);

    public StagingCursor Advance(long chunkRows, Instant at) =>
        new(ChunkIndex + 1, Resume.Advance(at, chunkRows), at);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record TransformSpec {
    private TransformSpec() { }

    public sealed record Analytical(string Sql) : TransformSpec;
    public sealed record Flight(FlightTicket Ticket) : TransformSpec;
    public sealed record Adbc(string Sql) : TransformSpec;
    public sealed record Relation(string Name, string Sql) : TransformSpec;
    public sealed record Scalar(string Name, string Sql) : TransformSpec;
}

[SmartEnum<string>]
public sealed partial class ReconcilePolicy {
    public static readonly ReconcilePolicy Append = new("append", BulkRoute.Copy, insertsNew: true, updatesMatched: false, deletesAbsent: false);
    public static readonly ReconcilePolicy Upsert = new("upsert", BulkRoute.Merge, insertsNew: true, updatesMatched: true, deletesAbsent: false);
    public static readonly ReconcilePolicy Mirror = new("mirror", BulkRoute.Merge, insertsNew: true, updatesMatched: true, deletesAbsent: true);
    public static readonly ReconcilePolicy Prune = new("prune", BulkRoute.Merge, insertsNew: false, updatesMatched: false, deletesAbsent: true);

    public BulkRoute Route { get; }
    public bool InsertsNew { get; }
    public bool UpdatesMatched { get; }
    public bool DeletesAbsent { get; }
}

[Union]
public abstract partial record PipelineFault : Expected, IValidationError<PipelineFault> {
    private PipelineFault(string detail, int code) : base(detail, code, None) { }

    public static PipelineFault Create(string message) => new ExtractMissing(message);

    public static PipelineFault From(string stage, Error error) =>
        error switch {
            StoreFault.Transient t => new TransformFaulted(stage, t.Message, recoverable: true),
            { Exception.Case: DbException { IsTransient: true } ex } => new TransformFaulted(stage, ex.Message, recoverable: true),
            _ => new TransformFaulted(stage, error.Message, recoverable: false),
        };

    public abstract bool IsTransient { get; }

    public sealed record ExtractMissing : PipelineFault {
        public ExtractMissing(string sourceKey) : base($"<extract-source-missing:{sourceKey}>", 7300) => SourceKey = sourceKey;
        public string SourceKey { get; }
        public override bool IsTransient => false;
    }
    public sealed record TransformFaulted : PipelineFault {
        public TransformFaulted(string stage, string detail, bool recoverable) : base($"<transform-faulted:{stage}:{detail}>", 7301) => (Stage, Detail, Recoverable) = (stage, detail, recoverable);
        public string Stage { get; }
        public string Detail { get; }
        public bool Recoverable { get; }
        public override bool IsTransient => Recoverable;
    }
    public sealed record Unconserved : PipelineFault {
        public Unconserved(long total, long parts) : base($"<reconcile-unconserved:{total}!={parts}>", 7302) => (Total, Parts) = (total, parts);
        public long Total { get; }
        public long Parts { get; }
        public override bool IsTransient => false;
        public static Unconserved Of(long total, long parts) => new(total, parts);
    }
    public sealed record CapacityBreached : PipelineFault {
        public CapacityBreached(string stage, long chunks, long rows) : base($"<capacity-breached:{stage}:{chunks}c/{rows}r>", 7303) => (Stage, Chunks, Rows) = (stage, chunks, rows);
        public string Stage { get; }
        public long Chunks { get; }
        public long Rows { get; }
        public override bool IsTransient => true;
    }
    public sealed record PoisonExhausted : PipelineFault {
        public PoisonExhausted(long chunkIndex, UInt128 contentKey, string cause) : base($"<poison-exhausted:{chunkIndex}:{contentKey:x32}:{cause}>", 7304) => (ChunkIndex, ContentKey, Cause) = (chunkIndex, contentKey, cause);
        public long ChunkIndex { get; }
        public UInt128 ContentKey { get; }
        public string Cause { get; }
        public override bool IsTransient => false;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record PipelineStage {
    private PipelineStage() { }

    public sealed record Extract(string SourceFormat, ArtifactIndexRow Source) : PipelineStage;
    public sealed record Stage(StagingCursor Cursor) : PipelineStage;
    public sealed record Validate(Seq<QualityRule> Rules) : PipelineStage;
    public sealed record Transform(TransformSpec Spec) : PipelineStage;
    public sealed record Load(string Target, BulkRoute Route) : PipelineStage;
    public sealed record Reconcile(string Target, string SourceKey, ReconcilePolicy Policy) : PipelineStage;

    public string Label => Switch(
        extract:   static _ => "extract",   stage:     static _ => "stage",
        validate:  static _ => "validate",  transform: static _ => "transform",
        load:      static _ => "load",      reconcile: static _ => "reconcile");

    public PipelineStage Relink(PipelineStage authored, StageStep step) =>
        (authored, this, step) switch {
            (Stage next, Stage prior, StageStep.Drained d) => next with { Cursor = prior.Cursor.Advance(d.Rows, step.At) },
            (Load next, Transform, StageStep.Transformed x) => next with { Target = x.Target },
            (Reconcile next, Load prior, _) => next with { Target = prior.Target },
            _ => authored,
        };
}

public sealed record DeadLetter(PipelineStage Stage, long ChunkIndex, UInt128 ContentKey, PipelineFault Fault, int RetryBudget, Instant At) {
    public const int MaxRetries = 3;

    public static DeadLetter Capture(PipelineStage stage, long chunkIndex, UInt128 contentKey, PipelineFault fault, Instant at) =>
        new(stage, chunkIndex, contentKey, fault, MaxRetries, at);

    public static DeadLetter Capture(PipelineStage stage, long chunkIndex, UInt128 contentKey, Error error, Instant at) =>
        Capture(stage, chunkIndex, contentKey, PipelineFault.From(stage.Label, error), at);

    public Option<DeadLetter> Reattempt(Instant at) =>
        Fault.IsTransient && RetryBudget > 0 ? Some(this with { RetryBudget = RetryBudget - 1, At = at }) : None;

    public Duration Backoff(Schedule cadence) =>
        cadence.AsEnumerable().Skip(MaxRetries - RetryBudget).HeadOrNone().IfNone(Duration.Zero);

    public bool Exhausted => !Fault.IsTransient || RetryBudget <= 0;
}

public readonly record struct ReconcileLedger(long Total, long Inserted, long Updated, long Deleted, long Unchanged) {
    public static readonly ReconcileLedger Empty = default;

    public ReconcileLedger Fold<TRow>(BulkDelta<TRow> delta) where TRow : class, IEquatable<TRow> =>
        (delta.Old.IsSome, delta.New.IsSome) switch {
            (false, true) => this with { Total = Total + 1, Inserted = Inserted + 1 },
            (true, false) => this with { Total = Total + 1, Deleted = Deleted + 1 },
            (true, true) when !delta.Old.Equals(delta.New) => this with { Total = Total + 1, Updated = Updated + 1 },
            _ => this with { Total = Total + 1, Unchanged = Unchanged + 1 },
        };

    public ReconcileLedger Merge(ReconcileLedger other) =>
        new(Total + other.Total, Inserted + other.Inserted, Updated + other.Updated, Deleted + other.Deleted, Unchanged + other.Unchanged);

    public Fin<ReconcileLedger> Conserve() =>
        Total == Inserted + Updated + Deleted + Unchanged
            ? Fin<ReconcileLedger>.Succ(this)
            : Fin<ReconcileLedger>.Fail(PipelineFault.Unconserved.Of(Total, Inserted + Updated + Deleted + Unchanged));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record StageStep {
    private StageStep() { }

    public sealed record Drained(long Rows, long Novel, Instant At) : StageStep;
    public sealed record Staged(long Rows, StagingCursor Cursor, Instant At) : StageStep;
    public sealed record Resumed(long SkippedChunks, StagingCursor From, Instant At) : StageStep;
    public sealed record Validated(long Passed, long Failed, Instant At) : StageStep;
    public sealed record Transformed(long Rows, string Target, Instant At) : StageStep;
    public sealed record Loaded(BulkReceipt Receipt, Instant At) : StageStep;
    public sealed record Reconciled(ReconcileLedger Ledger, Instant At) : StageStep;
    public sealed record Poisoned(Seq<DeadLetter> Letters, Instant At) : StageStep;
    public sealed record Shed(long ShedRows, Instant At) : StageStep;

    public Seq<DeadLetter> Quarantine => this is Poisoned p ? p.Letters : Seq<DeadLetter>();
    public long ShedCount => this is Shed s ? s.ShedRows : 0L;
    public long RejectedCount => this is Validated v ? v.Failed : 0L;

    public PipelineFactKind Kind => Switch(
        drained:     static _ => PipelineFactKind.Extracted,
        staged:      static _ => PipelineFactKind.Staged,
        resumed:     static _ => PipelineFactKind.Resumed,
        validated:   static _ => PipelineFactKind.Validated,
        transformed: static _ => PipelineFactKind.Transformed,
        loaded:      static _ => PipelineFactKind.Loaded,
        reconciled:  static _ => PipelineFactKind.Reconciled,
        poisoned:    static _ => PipelineFactKind.Quarantined,
        shed:        static _ => PipelineFactKind.Shed);

    public Instant At => Switch(
        drained:     static s => s.At, staged:    static s => s.At, resumed: static s => s.At,
        validated:   static s => s.At, transformed: static s => s.At, loaded: static s => s.At,
        reconciled:  static s => s.At, poisoned:  static s => s.At, shed: static s => s.At);

    public long RowCount => Switch(
        drained:     static s => s.Rows,
        staged:      static s => s.Rows,
        resumed:     static _ => 0L,
        validated:   static s => s.Passed,
        transformed: static s => s.Rows,
        loaded:      static s => s.Receipt.Rows,
        reconciled:  static s => s.Ledger.Total,
        poisoned:    static _ => 0L,
        shed:        static _ => 0L);
}

public readonly record struct PipelineFact(PipelineFactKind Kind, string Stage, long ChunkCount, long Rows, Duration Elapsed, Instant At);

public sealed record PipelineReceipt(
    Seq<PipelineFactKind> Stages,
    long Extracted,
    long Staged,
    long Resumed,
    long Validated,
    long Rejected,
    long Transformed,
    long Loaded,
    long Reconciled,
    long Shed,
    ReconcileLedger Net,
    Seq<DeadLetter> Quarantine,
    Duration Elapsed,
    Instant At) {
    public long Quarantined => Quarantine.Count;
}
```

The `StageStep` outcome is the one place stage-specific evidence lives — `Drained` carries the drained-versus-novel chunk split the `ChunkGate` projection yields, `Validated` splits passed from failed, `Loaded` re-projects the settled `Query/rail#BULK_LANE` `BulkReceipt` so the load's self-emitted fact and invalidation surface unchanged, `Reconciled` carries the conserved `ReconcileLedger`, and `Poisoned`/`Shed` carry the quarantine and shed slots the run fold accumulates. The run-level fold never re-derives counts from a flat row total: it discriminates the `StageStep` case and sums the typed receipt, so a `Loaded` step's `BulkReceipt` fires `Facts(receipt.Fact)` at the bulk lane while a `Reconciled` step's ledger `Conserve()` breach surfaces `PipelineFault.Unconserved` before the receipt mints, and a `Validated` step's `Failed` split aborts as `PipelineFault.TransformFaulted` so a quality violation is caught before load rather than counted post-hoc.

## [03]-[BULK_PIPELINE]

- Owner: `BulkPipeline` the static surface driving the stage ladder as a back-pressured chunk-wide fold over the `ArrowChunk` carrier, the five `TransformSpec` legs (DuckDB analytical/UDF/relation, Arrow Flight/ADBC), the `BulkRoute`/`MergeWithOutputAsync` load+reconcile, and the `ArrowEgress`/`AnalyticalTraversal` terminators; `CapacityBound` the per-shape quantum budget the shed gate reads; `ChunkGate` the idempotency `(mayHold, holds)` probe over the `ArtifactIndexRow` lane.
- Entry: `public static IO<PipelineReceipt> Run(Seq<PipelineStage> ladder, ChunkSource source, ChunkGate gate, CapacityBound bound, Func<DeadLetter, IO<Unit>> quarantine, ClockPolicy clocks)` — folds the stage ladder, drives each stage over the `ArrowChunk` carrier one quantum at a time through the `source` callback, gates each chunk through `gate.Admit` ahead of the stage body, sheds a chunk that exceeds the `CapacityBound` quantum budget, routes a `Poisoned` step's `DeadLetter` to the quarantine sink and continues, folds each `StageStep` outcome's typed receipt into the running counts and the net `ReconcileLedger`, and mints the typed run receipt only after the net ledger's `Conserve()` passes so a reconcile that lost a row fails the run rather than minting a silently-incomplete receipt; the `ladder` is the authoritative stage sequence the caller authors with real per-stage config, and `PipelineStage.Relink(authored, step)` threads each stage's resolved cursor/target onto the next authored stage so no config is fabricated; the idempotency gate, the shed gate, and the dead-letter route all live inside the `source` chunk loop so the run-level fold consumes the already-deduped, already-bounded `StageStep` outcome.
- Auto: the fold drives each stage over the `ArrowChunk` carrier so the same columnar memory threads extract → stage → validate → transform → load → reconcile with zero managed-array copy and one-chunk-wide peak memory bounded by the `CapacityBound`; the `Transform` stage discriminates the `TransformSpec` leg through its generated `Switch` — `Analytical` runs the DuckDB analytical lane (a `GROUP BY`/window transform through the `Query/lanes#ANALYTICAL_LANE` lane), `Scalar` registers a DuckDB scalar/window UDF (`DuckDBConnection.RegisterScalarFunction`) the query invokes vector-wide, `Relation` registers an in-process managed sequence as a queryable relation (`ArrowPlane.RegisterCarrier`) the SQL joins, `Flight` redeems a remote Flight ticket (`ArrowEgress.Serve` over `TransformSpec.Flight.Ticket`), and `Adbc` pulls an ADBC driver result (`ArrowEgress.Pull`) — every leg over the zero-copy `ArrowChunk` carrier so a columnar transform never materializes a managed row; the `Load` stage terminates on the `BulkRoute` op-log so the load self-emits its changefeed and the `StageStep.Loaded` re-projects the `BulkReceipt`; the `Reconcile` stage terminates on a `MergeWithOutputAsync` so the source-against-target OLD/NEW verdicts ride the RETURNING image folding into the `ReconcileLedger` per-verdict counts, the conservation identity guarding against a lost row; each stage's chunk loop gates each chunk through `gate.Admit` over the `ContentChunk.ShortTag` bloom pre-filter ahead of the `XxHash128` content compare so a re-run skips an applied chunk cheaply and the surviving rows surface as the `StageStep` typed count; back-pressure folds the `StageStep.Shed` count into the receipt and projects the `StoreFact.BulkShed` fact so a downstream lane throttles; a per-chunk failure inside a stage's chunk loop captures a `DeadLetter` through `DeadLetter.Capture(stage, chunkIndex, contentKey, error, at)` — the one boundary that converts a provider `Error` into a typed `PipelineFault` through `PipelineFault.From(stage.Label, error)` — and the stage surfaces one `StageStep.Poisoned` carrying the per-chunk `Seq<DeadLetter>` it accumulated, the run-level fold `Traverse`-routes every entry to the quarantine sink and appends it to the `PipelineReceipt.Quarantine` roster and continues so a poison chunk never aborts the whole run, and a `Fault.IsTransient` entry re-drains the chunk on the next pass under the `DeadLetter.Backoff` `Schedule` cadence while a terminal fault is `Exhausted` at capture; the parquet/lakehouse export leg rides the `Query/lanes#ANALYTICAL_LANE` `AnalyticalTraversal.Materialize` or `Lakehouse.Snapshot` so a columnar egress is one HLC-ordered drain, never a second pipeline.
- Receipt: the run rides the typed `PipelineReceipt` carrying the per-stage counts, the quarantined count, the shed count, and the net conserved `ReconcileLedger`; each stage's `PipelineFact` rides the interceptor stream; the export leg rides the `Query/rail#ARROW_EGRESS` Flight/parquet receipt or the `Query/lanes#ANALYTICAL_LANE` `ParquetSchemaStamp`, never a second egress.
- Packages: DuckDB.NET.Data.Full, Apache.Arrow.Flight, Apache.Arrow.Adbc, linq2db.EntityFrameworkCore, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new transform leg is one `TransformSpec` case the `ChunkSource` discriminates; a new terminator is one `BulkRoute` case (owned at the rail) or one `AnalyticalTraversal` row (owned at the lane); a new shed dimension is one column on `CapacityBound`; zero new surface — a second pipeline driver, a parallel transform engine, or a per-stage service is the deleted form because the pipeline drives the one stage union over the one `ArrowChunk` carrier and terminates on the one `BulkRoute` op-log and the one analytical export.
- Boundary: the pipeline driver folds the one stage union over the `ArrowChunk` carrier and terminates on the `BulkRoute` op-log and the `AnalyticalTraversal` export — a second driver, a parallel transform engine, or a row-oriented carrier is the deleted form; the `Transform` stage rides the DuckDB analytical lane or the Arrow Flight C-data path so a columnar transform is zero-copy and never a managed fold; the `Load` and `Reconcile` stages ride the `BulkRoute`/`MergeWithOutputAsync` so the load self-emits its changefeed and the reconcile verdicts ride RETURNING into the conserved `ReconcileLedger`, never a trigger-based second write path or a flat counter; the idempotency gate is the `ShortTag` bloom pre-filter ahead of the content-key compare against the `ArtifactIndexRow` dedup lane so a re-run skip is cheap and a false-positive tag only costs one fall-through compare; the dead-letter folds a failing chunk into the `DeadLetter` quarantine carrying its typed `PipelineFault` and a `Schedule`-cadenced retry budget and continues so the run is resumable past a poison chunk, a transient poison re-drains on a capability upgrade while a terminal fault (`ExtractMissing`/`Unconserved`/`PoisonExhausted`) is `Exhausted` at capture and never burns the budget — a bare untyped `Error` whose recovery identity collapses every cause to one key is the deleted form; back-pressure folds the `BulkShed` count off the existing fact stream when a chunk exceeds the `CapacityBound` quantum budget so the throttle is observable pull-shaped, never silent; the export leg is the settled `ArrowEgress`/`AnalyticalTraversal`/`Lakehouse` so a Flight/parquet/lakehouse egress rides the one columnar export, never a second pipeline; the chunk loop is the platform-forced statement seam — the `await foreach` drain over the `ChunkSource` and the per-chunk `ChunkSink` consumption carry language-owned statement forms, while the surrounding ladder fold stays expression-shaped over `IO.lift`/`Bind`/`Fold`.

```csharp signature
public readonly record struct CapacityBound(long ChunkQuota, long RowQuota) {
    public static readonly CapacityBound Default = new(ChunkQuota: 4096, RowQuota: 8_388_608);

    public bool Exceeds(long chunks, long rows) => chunks > ChunkQuota || rows > RowQuota;
}

// The chunk loop is the platform-forced ADO statement seam: the source drains one
// ArrowChunk quantum at a time, gates each chunk through gate.Admit ahead of the stage
// body, sheds past the CapacityBound budget, and projects the stage-specific StageStep.
public delegate IO<StageStep> ChunkSource(PipelineStage stage, ChunkGate gate, CapacityBound bound, ClockPolicy clocks, long mark);

public static class BulkPipeline {
    public static IO<PipelineReceipt> Run(
        Seq<PipelineStage> ladder,
        ChunkSource source,
        ChunkGate gate,
        CapacityBound bound,
        Func<DeadLetter, IO<Unit>> quarantine,
        ClockPolicy clocks) =>
        IO.lift(clocks.Mark).Bind(mark =>
            ladder.Fold(
                IO.pure(RunState.Genesis),
                (acc, stage) => acc.Bind(state =>
                    source(stage, gate, bound, clocks, mark).Bind(step =>
                        step.Quarantine.Traverse(quarantine).As()
                            .Map(_ => state.Absorb(step)))))
            .Bind(state => state.Conserve().Match(
                Succ: _ => IO.pure(state.Seal(clocks, mark)),
                Fail: IO.fail<PipelineReceipt>)));

    private readonly record struct RunState(
        Seq<PipelineFactKind> Stages, long Extracted, long Staged, long Resumed, long Validated, long Rejected,
        long Transformed, long Loaded, long Shed, ReconcileLedger Net, Seq<DeadLetter> Poison) {
        public static readonly RunState Genesis =
            new(Seq<PipelineFactKind>(), 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, ReconcileLedger.Empty, Seq<DeadLetter>());

        public RunState Absorb(StageStep step) =>
            step.Switch(
                state: this with { Stages = Stages.Add(step.Kind), Rejected = Rejected + step.RejectedCount, Shed = Shed + step.ShedCount },
                drained:     static (s, x) => s with { Extracted = s.Extracted + x.Rows },
                staged:      static (s, x) => s with { Staged = s.Staged + x.Rows },
                resumed:     static (s, x) => s with { Resumed = s.Resumed + x.SkippedChunks },
                validated:   static (s, x) => s with { Validated = s.Validated + x.Passed },
                transformed: static (s, x) => s with { Transformed = s.Transformed + x.Rows },
                loaded:      static (s, x) => s with { Loaded = s.Loaded + x.Receipt.Rows },
                reconciled:  static (s, x) => s with { Net = s.Net.Merge(x.Ledger) },
                poisoned:    static (s, x) => s with { Poison = s.Poison + x.Letters },
                shed:        static (s, _) => s);

        public Fin<ReconcileLedger> Conserve() =>
            Rejected > 0
                ? Fin<ReconcileLedger>.Fail(new PipelineFault.TransformFaulted("validate", $"<{Rejected}-rows-failed-quality>", recoverable: false))
                : Net.Conserve();

        public PipelineReceipt Seal(ClockPolicy clocks, long mark) =>
            new(Stages, Extracted, Staged, Resumed, Validated, Rejected, Transformed, Loaded, Net.Total, Shed, Net, Poison,
                clocks.Elapsed(mark), clocks.Now);
    }
}
```

| [INDEX] | [STAGE]    | [MECHANISM]                                            | [LAW]                                                          |
| :-----: | :--------- | :---------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | extract    | `Runtime/codecs` `InterchangeIo` canonical bytes      | parse is Compute; Persistence reads the `ArtifactIndexRow`    |
|  [02]   | stage      | resumable staging keyed by `StagingCursor.ChunkIndex` | crash resumes from the persisted `Watermark`                  |
|  [03]   | validate   | `Store/quality` rules in-flight                       | violation caught before load, never an after-the-fact scan   |
|  [04]   | transform  | `TransformSpec` leg: DuckDB `Analytical`/`Scalar` UDF/`Relation` carrier, Arrow `Flight`/`Adbc` | zero-copy columnar over `ArrowChunk`, never a managed fold |
|  [05]   | load       | `BulkRoute` set/copy/merge op-log                     | self-emits changefeed, never a trigger                       |
|  [06]   | reconcile  | `MergeWithOutputAsync` RETURNING → `ReconcileLedger`  | per-verdict conservation (`Old==New` is `Unchanged`); a lost row is `PipelineFault.Unconserved` |
|  [07]   | idempotent | `ChunkGate` `ContentChunker.Novel` bloom + `XxHash128` | re-run skip-not-duplicate; edited source re-stages deltas     |
|  [08]   | dead-letter| `DeadLetter` quarantine + typed `PipelineFault` + `Schedule` budget | poison quarantined, run resumable; only `IsTransient` re-drains, terminal `Exhausted` at capture |
|  [09]   | shed       | `CapacityBound` quantum budget → `StoreFact.BulkShed` | pull-shaped throttle off the existing stream, never silent    |
|  [10]   | export     | `AnalyticalTraversal` / `Lakehouse.Snapshot` parquet  | one HLC-ordered drain, never a second egress pipeline         |

## [04]-[RESEARCH]

- [ARROW_FLIGHT_TRANSFORM]: the Arrow Flight SQL / ADBC C-data path for the `Transform` stage — whether a columnar transform rides the Flight ticket zero-copy over the `ArrowChunk` carrier and the ADBC schema crosses to the `typescript:interchange` leg, proven against the admitted `Apache.Arrow.Flight`/`Apache.Arrow.Adbc` (the Arrow C-data interface binding is the noted `api-arrow.md`/`api-duckdb.md` gap the carrier currently routes through the catalogued DuckDB vector reader, settled at the `Query/rail#ARROW_PLANE` `[ARROW_ZERO_COPY]` gate).
- [PIPELINE_IDEMPOTENCY]: the `ChunkGate` idempotency over the FastCDC `ContentChunk` cut against the `ArtifactIndexRow` dedup lane — whether the settled `ContentChunker.Novel(manifest, mayHold, holds)` projection skips an applied chunk cheaply through the 64-bit `ShortTag` bloom (`mayHold`) ahead of the `XxHash128` content compare (`holds`) and the resumable `StagingCursor.Watermark` resumes from the last staged chunk index across a crash, and whether the FastCDC cut boundaries survive an interior insertion so an edited source re-stages only the changed chunks, measured before the idempotency fence pins.
- [RECONCILE_LEDGER]: the `MergeWithOutputAsync` RETURNING OLD/NEW image folding into the per-verdict `ReconcileLedger` and its conservation identity (`Total == Inserted + Updated + Deleted + Unchanged`) against a live PG18 server behind the `Query/rail#BULK_LANE` ReturningOldNew capability column — whether the `BulkDelta<TRow>` `Old`/`New` `IEquatable<TRow>` pair classifies insert/update/delete/unchanged exactly (a both-present-and-equal image is `Unchanged`, the no-op merge row PG18 RETURNING still emits, so the fold compares the images rather than collapsing every both-present case to `Updated`) and a conservation breach surfaces as `PipelineFault.Unconserved`, proven before the reconcile fence pins; the sqlite downgrade captures the delta through the SaveChanges interceptor hook.
- [EXTRACT_CANONICAL_BYTES]: the Compute interchange-lane parse-to-canonical-bytes hand-off for the `Extract` stage — the canonical-bytes shape the IFC/Speckle/Parquet/CSV parse produces (the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `ExportArtifact` content-keyed through `InterchangeIdentity.Admit` onto the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Interchange` row, the same row the field/tile/tessellated-GLB projections land on) and the seam boundary where Compute owns the parse and Persistence owns the staging, confirmed against the `Rasm.Compute/Runtime/codecs` `InterchangeIo` rail before the `Extract` fence pins.

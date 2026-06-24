# [PERSISTENCE_PIPELINE]

Rasm.Persistence orchestrates large import, export, and transform across Arrow, Parquet, CSV, IFC, and Speckle as one back-pressured fold over the existing columnar carrier rather than ad hoc batch loops. `PipelineStage` is the `[Union]` stage family — extract, stage, validate, transform, load, reconcile — and `BulkPipeline` drives it as a back-pressured fold over the `Query/rail#ARROW_PLANE` `ArrowChunk` zero-copy quantum, made idempotent through a chunk index plus the idempotency-dedup `ArtifactClassRow`, with a typed `PipelineReject` dead-letter and the `Query/rail#ARROW_EGRESS` `ArrowEgress` / `BulkRoute` op-log terminators. The admitted `Apache.Arrow.Flight`/`Apache.Arrow.Adbc` carry the streaming-transform and Flight-egress legs, `DuckDB.NET.Data.Full` the lakehouse and analytical transform, and `Sep` the CSV leg; `ArrowChunk`/`ArrowPlane`/`ArrowEgress`, `BulkRoute`/`BulkShed`, `TabularExportSpec`/`LakehouseCatalog`, `ContentChunker`, `ContentChunk.ShortTag`, `ClockPolicy`, and `ReceiptSinkPort` arrive settled. The `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIo` parse-to-canonical-bytes — the `ExportArtifact` carrier `InterchangeIdentity.Admit` lands content-addressed on the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Interchange` row — is the one `Extract` source this owner reads.

Wire posture: this page is host-local — the pipeline stages execute over the in-process columnar carrier and the op-log, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The parse-to-canonical-bytes `Extract` source arrives from the `Query/pipeline ⇄ Rasm.Compute/Runtime/codecs # [PORT]: parse-to-canonical-bytes (Extract)` seam as settled canonical bytes the Compute interchange lane already content-addressed, never minted here.

## [01]-[INDEX]

- [01]-[PIPELINE_STAGE]: the stage union, the back-pressured fold, idempotency, and the dead-letter.
- [02]-[BULK_PIPELINE]: the `ArrowChunk` fold, the Flight/ADBC/DuckDB transform legs, and the op-log terminator.

## [02]-[PIPELINE_STAGE]

- Owner: `PipelineStage` the `[Union]` stage family; `PipelineReject` the typed dead-letter record; `IdempotencyKey` the chunk-index plus content-key dedup token reusing the `ArtifactClassRow` idempotency lane; `PipelineFact` with `PipelineFactKind` the page-wide fact stream; `PipelineReceipt` the typed run evidence.
- Cases: `Extract` (parse-to-canonical-bytes from a source format), `Stage` (land canonical bytes into resumable staging), `Validate` (in-flight quality-rule check), `Transform` (a DuckDB/Flight columnar transform), `Load` (a `BulkRoute` set/copy/merge into the target), `Reconcile` (a source-against-target merge with verdicts) on `PipelineStage`; `PipelineFactKind` extracted | staged | validated | transformed | loaded | reconciled | rejected | shed.
- Entry: `public static PipelineStage Next(PipelineStage stage)` advances the stage ladder; `public static Fin<IdempotencyKey> Admit(long chunkIndex, UInt128 contentKey, Func<UInt128, bool> seen)` is the idempotency gate folding the chunk index plus the `XxHash128` content key against the dedup lane so a re-run skips an already-applied chunk; a `PipelineReject` carries the failing chunk, the stage, and the typed fault so a dead-letter capture is one record.
- Auto: the stages form one ladder over the `ArrowChunk` carrier — `Extract` reads the Compute exchange parse-to-canonical-bytes (so the IFC/Speckle/Parquet/CSV parse is the Compute concern and Persistence reads the canonical bytes), `Stage` lands the canonical bytes into resumable staging keyed by chunk index so a crash resumes from the last staged chunk, `Validate` runs the `Store/quality#QUALITY_RULE` rules in-flight so a violation is caught before load, `Transform` runs a DuckDB or Arrow Flight columnar transform over the carrier zero-copy, `Load` rides the `Query/rail#BULK_LANE` `BulkRoute` set/copy/merge so the load self-emits its changefeed and invalidation in one transaction, and `Reconcile` runs a source-against-target merge with per-row verdicts; idempotency reuses the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactClassRow` dedup lane keyed by chunk index plus content key so a re-run is skip-not-duplicate; back-pressure rides the existing `BulkShed` — a stage that exceeds the per-shape capacity bound emits a `StoreFact.BulkShed` fact and throttles its producer, never a thrown capacity exception; a stage failure routes the failing chunk to the typed `PipelineReject` dead-letter rather than aborting the whole run.
- Receipt: each stage rides a `PipelineFact` carrying the stage, the chunk count, and the elapsed; `PipelineReceipt(Stages, Extracted, Loaded, Rejected, Shed, Instant At)` is the typed run evidence feeding `StoreFact`/`StoreEvidence`; a dead-letter capture rides `pipeline.reject` carrying the failing stage and fault; a back-pressure shed rides the existing `store.bulk.shed`, never a second signal owner.
- Packages: Apache.Arrow, Apache.Arrow.Flight, Apache.Arrow.Adbc, DuckDB.NET.Data.Full, Sep, FastCDC.Net, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new stage is one `PipelineStage` case plus one fold arm; a new dead-letter dimension is one column on `PipelineReject`; a new evidence bucket is one `PipelineFactKind` row; zero new surface — a per-format import loop, a second staging table, or a parallel dead-letter channel is the deleted form because the pipeline is one fold over the `ArrowChunk` carrier and reuses the `ArtifactClassRow` idempotency and the `BulkRoute` op-log.
- Boundary: the pipeline is one back-pressured fold over the existing `ArrowChunk` carrier — an ad hoc batch loop or a managed-array result buffer is the deleted form, the fold threads the engine's zero-copy vector quantum and bounds peak memory to one chunk; the `Extract` stage reads the Compute interchange-lane parse-to-canonical-bytes through the `Query/pipeline ⇄ Rasm.Compute/Runtime/codecs # [PORT]: parse-to-canonical-bytes (Extract)` seam — the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIo` parse plus `InterchangeIdentity.Admit` lands the `ExportArtifact` content-addressed on the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Interchange` row this stage reads — so the IFC/Speckle/Parquet/CSV parse is the Compute concern and Persistence owns the staging, validation, transform, load, and reconcile, never a second parser; idempotency reuses the `ArtifactClassRow` dedup lane keyed by chunk index plus content key so a re-run is skip-not-duplicate and a second idempotency table is the deleted form; back-pressure rides the existing `BulkShed` `StoreFact.BulkShed` fact so a downstream lane throttles off the existing fact stream, never a thrown capacity exception or a parallel back-pressure channel; the dead-letter is the typed `PipelineReject` so a failing chunk is captured with its stage and fault rather than aborting the run, and a silent drop is the deleted form; the `Load` stage rides the `BulkRoute` op-log so the load self-emits its changefeed and invalidation, never a trigger-based second write path; the `Transform` stage runs over the DuckDB analytical lane or the Arrow Flight C-data path so a columnar transform is zero-copy, never a row-oriented carrier; the `Validate` stage runs the `Store/quality` rules in-flight so a quality violation is caught before load, never an after-the-fact scan.

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
    public static readonly PipelineFactKind Rejected = new("rejected");
    public static readonly PipelineFactKind Shed = new("shed");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record PipelineStage {
    private PipelineStage() { }

    public sealed record Extract(string SourceFormat, string SourceUri) : PipelineStage;
    public sealed record Stage(string StagingTable, long ChunkIndex) : PipelineStage;
    public sealed record Validate(Seq<QualityRule> Rules) : PipelineStage;
    public sealed record Transform(string Sql, bool ViaFlight) : PipelineStage;
    public sealed record Load(string Target, BulkRoute Route) : PipelineStage;
    public sealed record Reconcile(string Target, string SourceKey) : PipelineStage;

    public PipelineStage Next() =>
        this switch {
            Extract e => new Stage(e.SourceFormat, 0L),
            Stage s => new Validate(Seq<QualityRule>()),
            Validate => new Transform(string.Empty, ViaFlight: false),
            Transform t => new Load(string.Empty, BulkRoute.Copy),
            Load l => new Reconcile(l.Target, string.Empty),
            var terminal => terminal,
        };
}

public readonly record struct IdempotencyKey(long ChunkIndex, UInt128 ContentKey, ulong ShortTag);

public sealed record PipelineReject(PipelineStage Stage, long ChunkIndex, UInt128 ContentKey, Error Fault, Instant At);

public readonly record struct StageStep(long Rows, long Rejected, long Shed, Option<PipelineReject> DeadLetter) {
    public static StageStep Ran(long rows) => new(rows, 0L, 0L, None);
    public static StageStep Poisoned(PipelineReject reject) => new(0L, 1L, 0L, Some(reject));
    public static StageStep Throttled(long rows, long shed) => new(rows, 0L, shed, None);
}

public readonly record struct PipelineFact(PipelineFactKind Kind, string Stage, long ChunkCount, long Rows, Duration Elapsed, Instant At);

public sealed record PipelineReceipt(
    Seq<string> Stages, long Extracted, long Loaded, long Rejected, long Shed, Duration Elapsed, Instant At);
```

## [03]-[BULK_PIPELINE]

- Owner: `BulkPipeline` the static surface driving the stage ladder as a back-pressured fold over the `ArrowChunk` carrier, the Flight/ADBC/DuckDB transform legs, and the `BulkRoute`/`ArrowEgress` op-log terminator.
- Entry: `public static IO<PipelineReceipt> Run(Seq<PipelineStage> ladder, Func<PipelineStage, ChunkSink, IO<StageStep>> drive, Func<PipelineReject, IO<Unit>> deadLetter, ClockPolicy clocks)` — folds the stage ladder, drives each stage over the `ArrowChunk` carrier through the `ChunkSink` ref-struct-safe callback, folds each `StageStep` outcome's rows/rejected/shed counts, routes a `Some` `DeadLetter` reject to the dead-letter sink, and folds the typed run receipt; the per-chunk idempotency gate (`PipelineStage.Admit` over the `ShortTag` pre-filter) lives inside the `drive` chunk loop so the run-level fold consumes the already-deduped `StageStep` outcome rather than re-gating.
- Auto: the fold drives each stage over the `ArrowChunk` carrier so the same columnar memory threads extract → stage → validate → transform → load with zero managed-array copy and one-chunk-wide peak memory; the `Transform` stage runs over the DuckDB analytical lane (a `GROUP BY`/window/scalar transform) or the Arrow Flight C-data path (`ArrowEgress`) so a columnar transform is zero-copy and a remote-engine transform rides the Flight ticket; the `Load` stage terminates on the `BulkRoute` op-log so the load self-emits its changefeed; the `Reconcile` stage terminates on a `MergeWithOutputAsync` so the source-against-target verdicts ride the RETURNING image; each stage's `drive` chunk loop gates each chunk through the `PipelineStage.Admit` `ShortTag` pre-filter ahead of the content-key compare so a re-run skips an applied chunk cheaply and the surviving rows surface as the `StageStep.Rows` count; back-pressure folds the `StageStep.Shed` count into the receipt so a downstream lane throttles; a stage failure surfaces a `StageStep.Poisoned` carrying the `PipelineReject`, the fold routes it to the dead-letter and continues so a poison chunk never aborts the whole run.
- Receipt: the run rides the typed `PipelineReceipt` carrying the per-stage counts, the rejected count, and the shed count; each stage's `PipelineFact` rides the interceptor stream; the export leg rides the `Query/rail#ARROW_EGRESS` Flight/parquet receipt, never a second egress.
- Packages: Apache.Arrow.Flight, Apache.Arrow.Adbc, DuckDB.NET.Data.Full, linq2db.EntityFrameworkCore, LanguageExt.Core, NodaTime.
- Growth: a new transform leg is one arm on `Run`; a new terminator is one `BulkRoute` case (owned at the rail); zero new surface — a second pipeline driver, a parallel transform engine, or a per-stage service is the deleted form because the pipeline drives the one stage union over the one `ArrowChunk` carrier and terminates on the one `BulkRoute` op-log.
- Boundary: the pipeline driver folds the one stage union over the `ArrowChunk` carrier and terminates on the `BulkRoute` op-log — a second driver, a parallel transform engine, or a row-oriented carrier is the deleted form; the `Transform` stage rides the DuckDB analytical lane or the Arrow Flight C-data path so a columnar transform is zero-copy and never a managed fold; the `Load` and `Reconcile` stages ride the `BulkRoute`/`MergeWithOutputAsync` so the load self-emits its changefeed and the reconcile verdicts ride RETURNING, never a trigger-based second write path; the idempotency gate is the `ShortTag` pre-filter ahead of the content-key compare so a re-run skip is cheap and a false-positive tag only costs one fall-through compare; the dead-letter folds a failing chunk and continues so the run is resumable past a poison chunk; back-pressure folds the `BulkShed` count off the existing fact stream so the throttle is observable, never silent; the export leg is the settled `ArrowEgress` so a Flight/parquet egress rides the one Arrow egress, never a second pipeline.

```csharp signature
public static class BulkPipeline {
    public static IO<PipelineReceipt> Run(
        Seq<PipelineStage> ladder,
        Func<PipelineStage, ChunkSink, IO<StageStep>> drive,
        Func<PipelineReject, IO<Unit>> deadLetter,
        ClockPolicy clocks) =>
        IO.lift(clocks.Mark).Bind(mark =>
            ladder.Fold(
                IO.pure((Extracted: 0L, Loaded: 0L, Rejected: 0L, Shed: 0L, Stages: Seq<string>())),
                (acc, stage) => acc.Bind(counts => drive(stage, default).Bind(step =>
                    step.DeadLetter.Match(
                        Some: reject => deadLetter(reject).Map(_ => counts),
                        None: () => IO.pure(counts)).Map(folded => folded with {
                            Stages = folded.Stages.Add(stage.GetType().Name),
                            Extracted = stage is PipelineStage.Extract ? folded.Extracted + step.Rows : folded.Extracted,
                            Loaded = stage is PipelineStage.Load ? folded.Loaded + step.Rows : folded.Loaded,
                            Rejected = folded.Rejected + step.Rejected,
                            Shed = folded.Shed + step.Shed,
                        }))))
            .Map(counts => new PipelineReceipt(
                counts.Stages, counts.Extracted, counts.Loaded, counts.Rejected, counts.Shed, clocks.Elapsed(mark), clocks.Now)));
}
```

| [INDEX] | [STAGE]    | [MECHANISM]                                       | [LAW]                                                     |
| :-----: | :--------- | :------------------------------------------------ | :-------------------------------------------------------- |
|  [01]   | extract    | `Runtime/codecs` `InterchangeIo` canonical bytes  | parse is Compute; Persistence reads canonical bytes      |
|  [02]   | stage      | resumable staging keyed by chunk index            | crash resumes from last staged chunk                     |
|  [03]   | validate   | `Store/quality` rules in-flight                   | violation caught before load, never after-the-fact scan  |
|  [04]   | transform  | DuckDB analytical pass / Arrow Flight C-data       | zero-copy columnar, never a managed fold                 |
|  [05]   | load       | `BulkRoute` set/copy/merge op-log                 | self-emits changefeed, never a trigger                   |
|  [06]   | reconcile  | `MergeWithOutputAsync` RETURNING verdicts          | source-against-target verdicts in one statement          |

## [04]-[RESEARCH]

- [ARROW_FLIGHT_TRANSFORM]: the Arrow Flight SQL / ADBC C-data path for the `Transform` stage — whether a columnar transform rides the Flight ticket zero-copy over the `ArrowChunk` carrier and the ADBC schema crosses to the `typescript:interchange` leg, proven against the admitted `Apache.Arrow.Flight`/`Apache.Arrow.Adbc` (the Arrow C-data interface binding is the noted `api-arrow.md` gap the carrier currently routes through the catalogued DuckDB vector reader).
- [PIPELINE_IDEMPOTENCY]: the chunk-index-plus-content-key idempotency against the `ArtifactClassRow` dedup lane and the `ShortTag` pre-filter — whether a re-run skips an applied chunk cheaply through the 64-bit sketch ahead of the `XxHash128` compare and the resumable staging resumes from the last staged chunk index, measured before the idempotency fence pins.
- [EXTRACT_CANONICAL_BYTES]: the Compute interchange-lane parse-to-canonical-bytes hand-off for the `Extract` stage — the canonical-bytes shape the IFC/Speckle/Parquet/CSV parse produces (the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `ExportArtifact` content-keyed through `InterchangeIdentity.Admit` onto the `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Interchange` row, the same row the field/tile/tessellated-GLB projections land on) and the seam boundary where Compute owns the parse and Persistence owns the staging, confirmed against the `Rasm.Compute/Runtime/codecs` `InterchangeIo` rail before the `Extract` fence pins.

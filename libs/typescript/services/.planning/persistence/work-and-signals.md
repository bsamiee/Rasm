# [SERVICES_WORK_AND_SIGNALS]

The durable work and signalling surfaces over the one `PgClient` — `WorkQueue`/`EventJournal`/`Notifications`, the jobs/DLQ, event-journal, and notification-channel surfaces; `AssetTransfer`, the `Schema.Literal` export-codec axis; and `FeatureFlags`, the percentage-rollout buckets. Every variation is a row or literal on the owning axis, never a sibling surface. The export-codec axis is one `Schema.Literal` and a parallel exporter per format is the named defect. This is a node-only surface and crosses no .NET wire.

## [1]-[INDEX]

One cluster: `[2]-[WORK_AND_SIGNALS]` owns jobs/DLQ, the event journal, notifications, the asset-export codec axis, and the feature-flag buckets.

## [2]-[WORK_AND_SIGNALS]

- Owner: `WorkQueue`/`EventJournal`/`Notifications`, the durable work and signalling surfaces; `AssetTransfer`, the `Schema.Literal` export-codec axis; and `FeatureFlags`, the percentage-rollout buckets — every variation a row or literal on the owning axis, never a sibling surface.
- Cases: `WorkQueue` is the `Job` priority (`critical`/`high`/`normal`/`low`) and status (`queued`/`processing`/`complete`/`failed`/`cancelled`) surface drained by a `FOR UPDATE SKIP LOCKED` claim with `LISTEN`/`NOTIFY` as the wake signal, with `JobDlq` as the first-class dead-letter sourced from `job`|`event`; `EventJournal` is the retainable tenant-scoped event ledger; `Notifications` is the multi-channel (`email`/`webhook`/`inApp`) preference matrix with `mutedUntil` and the delivery state machine (`queued`/`sending`/`delivered`/`failed`/`dlq`); `AssetTransfer` is the export-codec axis keyed by format; `FeatureFlags` evaluates a 0-100 integer rollout bucket per flag.
- Auto: `AssetTransfer` is one `Schema.Literal` format axis — `csv`/`xlsx`/`pdf`/`archive` — selecting the export codec (`papaparse` for csv, `exceljs` for xlsx, `jspdf` for pdf, `jszip` for archive, `sharp` for image transform inside the asset pipeline), so the four export libs are one codec row each on the axis and a parallel exporter per format is the deleted form.
- Entry: every work and signalling surface rides the one `PgClient` (`persistence/store-boundary#STORE_BOUNDARY`); the queue claim and the notification wake use the Postgres-native `LISTEN`/`NOTIFY` push edge — at-most-once, 8 KB-capped, non-persistent — so the durable claim and the persisted row are the source of truth and `NOTIFY` is only the wake. The `EventJournal` and `Notifications` atomic-publish guarantee arrives from `eventing/transactional-outbox#TRANSACTIONAL_OUTBOX`, which writes the outbox row in the same transaction as the domain mutation.
- Packages: `@effect/sql-pg` for the queue/journal/notification tables over the one `PgClient`, `@effect-aws/client-s3`/`@aws-sdk/client-s3`/`@aws-sdk/s3-request-presigner` for the asset object-store, `@aws-sdk/client-sesv2` and `nodemailer` for the email channel, and `exceljs`/`papaparse`/`jspdf`/`jszip`/`sharp` for the asset-transfer codecs.
- Growth: a new job priority/status lands as one literal; a new notification channel lands as one matrix row; a new export format lands as one `AssetTransfer` codec row; a new flag lands as one `FeatureFlags` bucket.
- Boundary: every work and signalling surface rides the one `PgClient`; the export-codec axis is one `Schema.Literal` and a parallel exporter per format is the named defect; this is a node-only surface, never browser-reachable.

```ts contract
class Job extends Model.Class<Job>("Job")({
  id: Model.Generated(Schema.UUID),
  appId: Schema.UUID,
  priority: Schema.Literal("critical", "high", "normal", "low"),
  status: Schema.Literal("queued", "processing", "complete", "failed", "cancelled"),
  payload: Schema.parseJson(Schema.Unknown),
  attempts: Schema.Number,
}) {}

const AssetTransferFormat = Schema.Literal("csv", "xlsx", "pdf", "archive");
type AssetTransferFormat = Schema.Schema.Type<typeof AssetTransferFormat>;

interface AssetTransfer {
  readonly export: (format: AssetTransferFormat, rows: Stream.Stream<Record<string, unknown>>) => Effect.Effect<Uint8Array, FaultDetail>;
}

interface FeatureFlags {
  readonly enabled: (flag: string, subjectKey: string) => Effect.Effect<boolean>;
}
```

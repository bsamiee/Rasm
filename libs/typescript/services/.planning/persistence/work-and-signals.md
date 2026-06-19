# [SERVICES_WORK_AND_SIGNALS]

The durable work and signalling surfaces over the one `PgClient` — `WorkQueue`/`EventJournal`/`Notifications`, the jobs/DLQ, event-journal, and notification-channel surfaces; and `FeatureFlags`, the percentage-rollout buckets. Every variation is a row or literal on the owning axis, never a sibling surface. The asset-export codec fan-out the page once carried (`papaparse`/`exceljs`/`jspdf`/`jszip` over a `Schema.Literal` format axis, plus `sharp` the in-pipeline image transform) is now owned by `folder:object-store/object-store#OBJECT_STORE` as the streaming `ObjectStore.put` fan-out — this page references that owner for the codec concern and keeps only the job/journal/notification/flag surfaces; a `Job` whose payload is an asset export enqueues the work, and the `object-store` owner streams the encoded bytes. This is a node-only surface and crosses no .NET wire.

## [1]-[INDEX]

One cluster: `[2]-[WORK_AND_SIGNALS]` owns jobs/DLQ, the event journal, notifications, and the feature-flag buckets; the asset-export codec fan-out is owned by `folder:object-store/object-store#OBJECT_STORE`, which this page references and enqueues work for.

## [2]-[WORK_AND_SIGNALS]

- Owner: `WorkQueue`/`EventJournal`/`Notifications`, the durable work and signalling surfaces; and `FeatureFlags`, the percentage-rollout buckets — every variation a row or literal on the owning axis, never a sibling surface. The asset-export codec axis is owned by `folder:object-store/object-store#OBJECT_STORE`; a `Job` enqueues an export and that owner streams the encoded bytes.
- Cases: `WorkQueue` is the `Job` priority (`critical`/`high`/`normal`/`low`) and status (`queued`/`processing`/`complete`/`failed`/`cancelled`) surface drained by a `FOR UPDATE SKIP LOCKED` claim with `LISTEN`/`NOTIFY` as the wake signal, with `JobDlq` as the first-class dead-letter sourced from `job`|`event`; `EventJournal` is the retainable tenant-scoped event ledger; `Notifications` is the multi-channel (`email`/`webhook`/`inApp`) preference matrix with `mutedUntil` and the delivery state machine (`queued`/`sending`/`delivered`/`failed`/`dlq`); `FeatureFlags` evaluates a 0-100 integer rollout bucket per flag. An asset-export `Job` carries the export request in its payload and the `folder:object-store/object-store#OBJECT_STORE` `AssetCodec` fan-out streams the encoded bytes to the store — this page owns the work enqueue, that page owns the codec.
- Entry: every work and signalling surface rides the one `PgClient` (`folder:persistence/store-boundary#STORE_BOUNDARY`); the queue claim and the notification wake use the Postgres-native `LISTEN`/`NOTIFY` push edge — at-most-once, 8 KB-capped, non-persistent — so the durable claim and the persisted row are the source of truth and `NOTIFY` is only the wake. The `EventJournal` and `Notifications` atomic-publish guarantee arrives from `folder:eventing/transactional-outbox#TRANSACTIONAL_OUTBOX`, which writes the outbox row in the same transaction as the domain mutation; the `AssetTransferFault` an export `Job` surfaces is owned at `folder:object-store/object-store#OBJECT_STORE`, keyed by format and `encode`/`object-store` stage.
- Packages: `@effect/sql-pg` for the queue/journal/notification tables over the one `PgClient`, `@aws-sdk/client-sesv2` and `nodemailer` for the email channel; the asset object-store and the export codecs (`@effect-aws/client-s3`, `exceljs`/`papaparse`/`jspdf`/`jszip`/`sharp`) are consumed by `folder:object-store/object-store#OBJECT_STORE`, not this page.
- Growth: a new job priority/status lands as one literal; a new notification channel lands as one matrix row; a new flag lands as one `FeatureFlags` bucket; a new export format lands as one `AssetCodec` row on the `folder:object-store/object-store#OBJECT_STORE` owner, not here.
- Boundary: every work and signalling surface rides the one `PgClient`; the asset-export codec concern is owned by `folder:object-store/object-store#OBJECT_STORE`, not re-implemented here — a parallel exporter on this page is the named defect; this is a node-only surface, never browser-reachable.

```ts contract
class Job extends Model.Class<Job>("Job")({
  id: Model.Generated(Schema.UUID),
  appId: Schema.UUID,
  priority: Schema.Literal("critical", "high", "normal", "low"),
  status: Schema.Literal("queued", "processing", "complete", "failed", "cancelled"),
  payload: Schema.parseJson(Schema.Unknown),
  attempts: Schema.Number,
}) {}

interface FeatureFlags {
  readonly enabled: (flag: string, subjectKey: string) => Effect.Effect<boolean>;
}
```

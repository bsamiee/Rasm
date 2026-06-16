# [SERVICES_PERSISTENCE]

One page owns the durable tier's SQL persistence boundary and the full entity-model registry it carries — `SqlBoundary`, the single `@effect/sql-pg` `PgClient` Layer with the `Model.Class` row schemas and the `Migrator`; the ~15-entity registry bound to ONE `Model.Class` per entity with projections via `Model.fields`/`Schema.pick`; the multi-tenant RLS scoping axis; the jobs/DLQ, events/journal, and notifications surfaces; the asset-transfer export-codec axis; and the feature-flag percentage-rollout buckets. The Postgres client is the single SQL surface; a second SQL owner, a drizzle/kysely parallel query surface, or N parallel schemas per entity is the named defect. The page owns three clusters — the store boundary and entity registry, the tenancy RLS axis, and the work-and-signals surfaces — one transcription unit. The page consumes `TenantContextWire` as the RLS predicate input and crosses no .NET wire.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                       |
| :-----: | :--------------- | :--------------------------------------------------------- |
|   [1]   | STORE_BOUNDARY   | the single PgClient/Migrator boundary and the one-Model.Class-per-entity registry |
|   [2]   | TENANCY          | the multi-tenant RLS axis, lifecycle, and purge handler family |
|   [3]   | WORK_AND_SIGNALS | jobs/DLQ, events, notifications, asset-export codec, feature-flag buckets |

## [2]-[STORE_BOUNDARY]

- Owner: `SqlBoundary`, the single `@effect/sql-pg` `PgClient` Layer constructed from one config carrying the `provisioning.md` `StackOutputs` Postgres DSN, the pool bounds, and the connect/idle timeouts; the `Migrator` running the entity and store DDL at startup; and `EntityRegistry`, the ~15-entity `Model.Class` set with projections derived off the one class.
- Cases: the durable cluster's message and runner stores (owned at `internal-rpc.md`) ride this one `SqlClient` surface; row schemas are the one `Model.Class` pattern the corpus uses — `User`, `Permission`, `Session`, `OauthAccount`, `MfaSecret`, `WebauthnCredential`, `ApiKey`, `App`, `Asset`, `AuditLog`, `Job`, `JobDlq`, `Notification`, `KvStore`, plus the `AgentJournal` `durable-execution.md` owns — never parallel structs, and a read projection is `Model.fields`/`Schema.pick` off the one class, never a sibling schema.
- Entry: `ClusterEngine`'s persistence backend is this `PgClient` layer; the message and runner stores reach the kernel through it; a workflow or activity persists its durable state through the one client and never opens a second connection; the `listen`/`notify` channel pair is the only Postgres-native push edge, reserved for the durable tier.
- Auto: `EntityRegistry` exposes each entity's variant schemas (`insert`/`update`/`json`/`jsonCreate`/`jsonUpdate`) off the one `Model.Class` so the isolatedDeclarations `.d.ts` emit resolves without a hand-written declaration.
- Packages: `@effect/sql` for the `Migrator` and the `Model.Class` row schema, `@effect/sql-pg` for the `PgClient` and the `listen`/`notify` channel, and `@effect/platform-node` for the driver host.
- Growth: a new entity lands as one `Model.Class` on `EntityRegistry`; a new projection lands as a `Model.fields`/`Schema.pick` derivation, never a sibling schema; a new push channel lands as one `listen`/`notify` row, never a second client.
- Boundary: the Postgres client is the single SQL surface — a second SQL owner or a drizzle/kysely parallel query surface is the named defect; N parallel schemas per entity breaking the isolatedDeclarations emit is the named defect; the DSN arrives from the `provisioning.md` `StackOutputs` typed reference, never a hand-set literal; this is a node-only surface, never browser-reachable.

```ts contract
class User extends Model.Class<User>("User")({
  id: Model.Generated(Schema.UUID),
  appId: Schema.UUID,
  email: Schema.String,
  role: Schema.Literal("owner", "admin", "member", "viewer", "guest"),
  createdAt: Model.DateTimeInsert,
  updatedAt: Model.DateTimeUpdate,
}) {}

const UserSummary = User.pipe(Schema.pick("id", "email", "role"));

interface SqlBoundary {
  readonly client: Layer.Layer<PgClient.PgClient, ConfigError.ConfigError | SqlError.SqlError>;
  readonly migrator: Effect.Effect<ReadonlyArray<readonly [number, string]>, SqlError.SqlError, PgClient.PgClient>;
  readonly listen: (channel: string) => Stream.Stream<string, SqlError.SqlError, PgClient.PgClient>;
}
```

## [3]-[TENANCY]

- Owner: `TenantScope`, the multi-tenant RLS axis scoping every entity by `app_id` FK, the tenant lifecycle vocabulary, and the purge-handler family, set per request at the connection boundary.
- Cases: `TenantScope` scopes every entity by `app_id` FK with the `app.current_tenant` GUC the RLS predicate reads (`current_setting('rasm.tenant')`), the tenant lifecycle (`active`/`suspended`/`archived`/`purging`), and the purge handler family (sessions/api-keys/assets/event-journal/job-dlq/kv-store/mfa-secrets/oauth-accounts/archive/purge-tenant); `withTenant` sets the GUC for the wrapped effect's connection so every query inside it reads the RLS-scoped row set.
- Entry: the `app.current_tenant` GUC is set per request at the connection boundary so every query reads the RLS-scoped row set; a purge runs the handler family in dependency order under the one client.
- Packages: `@effect/sql-pg` for the `set_config` GUC binding over the one `PgClient`.
- Growth: a new tenant lifecycle state lands as one `TenantLifecycle` literal; a new purge handler lands as one `PurgeTarget` literal, never a second tenant axis.
- Boundary: the tenant key is consumed from `TenantContextWire`, never re-minted; RLS scoping rides the one `PgClient` GUC and never a branch-side WHERE-clause filter; this is a node-only surface, never browser-reachable.

```ts contract
const TenantLifecycle = Schema.Literal("active", "suspended", "archived", "purging");
const PurgeTarget = Schema.Literal(
  "sessions", "api-keys", "assets", "event-journal", "job-dlq", "kv-store", "mfa-secrets", "oauth-accounts", "archive", "purge-tenant",
);
type PurgeTarget = Schema.Schema.Type<typeof PurgeTarget>;

interface TenantScope {
  readonly withTenant: <A, E, R>(tenant: string, eff: Effect.Effect<A, E, R>) => Effect.Effect<A, E | SqlError.SqlError, R | PgClient.PgClient>;
  readonly purge: (tenant: string, target: PurgeTarget) => Effect.Effect<number, SqlError.SqlError, PgClient.PgClient>;
}
```

## [4]-[WORK_AND_SIGNALS]

- Owner: `WorkQueue`/`EventJournal`/`Notifications`, the durable work and signalling surfaces; `AssetTransfer`, the `Schema.Literal` export-codec axis; and `FeatureFlags`, the percentage-rollout buckets — every variation a row or literal on the owning axis, never a sibling surface.
- Cases: `WorkQueue` is the `Job` priority (`critical`/`high`/`normal`/`low`) and status (`queued`/`processing`/`complete`/`failed`/`cancelled`) surface with `JobDlq` as the first-class dead-letter sourced from `job`|`event`; `EventJournal` is the retainable tenant-scoped event ledger; `Notifications` is the multi-channel (`email`/`webhook`/`inApp`) preference matrix with `mutedUntil` and the delivery state machine (`queued`/`sending`/`delivered`/`failed`/`dlq`); `AssetTransfer` is the export-codec axis keyed by format; `FeatureFlags` evaluates a 0-100 integer rollout bucket per flag.
- Auto: `AssetTransfer` is one `Schema.Literal` format axis — `csv`/`xlsx`/`pdf`/`archive` — selecting the export codec (`papaparse` for csv, `exceljs` for xlsx, `jspdf` for pdf, `jszip` for archive, `sharp` for image transform inside the asset pipeline), so the four export libs are one codec row each on the axis and a parallel exporter per format is the deleted form.
- Packages: `@effect/sql-pg` for the queue/journal/notification tables over the one `PgClient`, `@effect-aws/client-s3` and `@aws-sdk/client-s3` for the asset object-store, and `exceljs`/`papaparse`/`jspdf`/`jszip`/`sharp` for the asset-transfer codecs.
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

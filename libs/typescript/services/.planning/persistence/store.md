# [SERVICES_STORE]

The single SQL persistence boundary and the entity-model registry it carries — `SqlBoundary`, the one `@effect/sql-pg` `PgClient` Layer with the `Model.Class` row schemas and the `Migrator`; and `EntityRegistry`, the ~15-entity set bound to ONE `Model.Class` per entity with projections via `Model.fields`/`Schema.pick`. The Postgres client is the single SQL surface; a second SQL owner, a drizzle/kysely parallel query surface, or N parallel schemas per entity is the named defect. This is a node-only surface and crosses no .NET wire.

## [01]-[INDEX]

- [01]-[STORE_BOUNDARY]: owns the single `PgClient`/`Migrator` boundary and the one-`Model.Class`-per-entity registry.

## [02]-[STORE_BOUNDARY]

- Owner: `SqlBoundary`, the single `@effect/sql-pg` `PgClient` Layer constructed from one config carrying the `provisioning/contract#PROVISIONING` `StackOutputs` Postgres DSN, the pool bounds, and the connect/idle timeouts; the `Migrator` running the entity and store DDL at startup; and `EntityRegistry`, the ~15-entity `Model.Class` set with projections derived off the one class.
- Cases: the durable cluster's message and runner stores (owned at `execution/backplane#RUNNER_AND_SCHEDULING`) ride this one `SqlClient` surface; row schemas are the one `Model.Class` pattern the corpus uses — `User`, `Permission`, `Session`, `OauthAccount`, `MfaSecret`, `WebauthnCredential`, `ApiKey`, `App`, `Asset`, `AuditLog`, `Job`, `JobDlq`, `Notification`, `KvStore`, plus the `AgentJournal` `execution/ai#AI_ACTIVITY` owns — never parallel structs, and a read projection is `Model.fields`/`Schema.pick` off the one class, never a sibling schema; the credential-specific field shapes of `MfaSecret` and `WebauthnCredential` (the Base32 `Model.Sensitive` secret and `afterTimeStep` replay floor; the credential id, public key, `counter`, transports, and device flags) are authored at their consuming owner `security/auth#VERIFIER`, registered here as one `Model.Class` each on the registry exactly as `AgentJournal` is.
- Entry: `ClusterEngine`'s persistence backend is this `PgClient` layer; the message and runner stores reach the kernel through it; a workflow or activity persists its durable state through the one client and never opens a second connection; the `listen`/`notify` channel pair is the only Postgres-native push edge, reserved for the durable tier and consumed by the transactional outbox relay (`execution/outbox#TRANSACTIONAL_OUTBOX`).
- Auto: `EntityRegistry` exposes each entity's variant schemas (`insert`/`update`/`json`/`jsonCreate`/`jsonUpdate`) off the one `Model.Class` so the isolatedDeclarations `.d.ts` emit resolves without a hand-written declaration.
- Packages: `@effect/sql` for the `Migrator` and the `Model.Class` row schema, `@effect/sql-pg` for the `PgClient` and the `listen`/`notify` channel, `@effect/platform-node` for the driver host.
- Growth: a new entity lands as one `Model.Class` on `EntityRegistry`; a new projection lands as a `Model.fields`/`Schema.pick` derivation, never a sibling schema; a new push channel lands as one `listen`/`notify` row, never a second client.
- Boundary: the Postgres client is the single SQL surface — a second SQL owner or a drizzle/kysely parallel query surface is the named defect; N parallel schemas per entity breaking the isolatedDeclarations emit is the named defect; the DSN arrives from the `provisioning/contract#PROVISIONING` `StackOutputs` typed reference, never a hand-set literal; this is a node-only surface, never browser-reachable.

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

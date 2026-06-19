# [SERVICES_SECRET]

The runtime secret-resolution and rotation owner the whole node tier reads — `SecretStore`, the one `Effect.Service` that resolves every key (`auth#VERIFIER` credential secrets, `execution/ai#AI_ACTIVITY` provider keys, the `persistence/store#STORE_BOUNDARY` connection string, the object-store credentials) through the one `ConfigProvider` boundary, never a bespoke `process.env` read or a second secret scheme. A key is one `SecretRef` `Data.TaggedEnum` row over the provider axis — `EscEnv` (the `@pulumi/esc-sdk` session-resolved environment), `Doppler` (the `@dopplerhq/node-sdk` config secret, with the dynamic-secret lease arm), `Static` (the deploy-injected `ConfigProvider` value) — folded by `Match.tagsExhaustive` so a new provider breaks every resolution site at compile time; `AwsSecretsManager` is present in the family shape for anticipatory collapse but blocked-gated on `@aws-sdk/client-secrets-manager` admission and never settled as an active row. Resolution carries a `Config.redacted` `Redacted.Redacted` so a secret never widens to a loggable string, and a `LeasedSecret` `SubscriptionRef`-backed cache holds the resolved value under a TTL: a rotation pushes a new value through `SubscriptionRef.changes`, and every dependent owner subscribed to that `Stream` re-resolves without a process restart — the rotation edge the deploy-time-only `provisioning/contract#PROVISIONING` `SecretResolver` cannot give a running node. Deploy-time and runtime resolution MEET at the one `ConfigProvider` boundary: `provisioning/contract#PROVISIONING` injects the ESC/Doppler-backed values at deploy and `SecretStore` re-resolves and re-leases them at runtime against the same provider rows, never two parallel schemes. Every resolution emits one `AuditReceipt` under the `persistence/tenancy#TENANCY` `app.current_tenant` GUC so a secret read is RLS-scoped and recorded. This surface crosses no .NET wire and is node-only, never browser-reachable — the browser owns no key. This is the net-new `secrets/` sub-domain's one page.

## [01]-[INDEX]

- [01]-[SECRET_STORE]: owns the `SecretRef` provider axis, the `SecretStore` `Effect.Service` over the `ConfigProvider` boundary, the TTL-leased `SubscriptionRef`-backed rotation cache, and the tenancy-scoped `AuditReceipt`.

## [02]-[SECRET_STORE]

- Owner: `SecretStore`, the one `Effect.Service` resolving every runtime secret through the `ConfigProvider` boundary; `SecretRef`, the closed `Data.TaggedEnum` over the provider axis (`EscEnv`/`Doppler`/`Static` settled, `AwsSecretsManager` declared-but-gated) folded by one `Match.tagsExhaustive`; `LeasedSecret`, the TTL-leased `SubscriptionRef`-backed cache cell whose `changes` `Stream` is the rotation-invalidation edge; `AuditReceipt`, the one `Model.Class` every resolution emits under the tenancy GUC; `LeasePolicy`, the `as const satisfies Record` vocabulary keyed by provider carrying the per-provider TTL and rotation discipline.
- Cases: `SecretStore.resolve` is the one entrypoint over the closed `SecretRef` family, dispatched by one `Match.value(ref).pipe(Match.tagsExhaustive(...))`, so a new provider verb breaks every resolution site at compile time rather than growing a sibling resolver service. The `EscEnv` arm drives the `@pulumi/esc-sdk` session flow — `EscApi.openAndReadEnvironment(org, project, env)` opens a session and resolves the `EnvironmentDefinitionValues` in one call (or the explicit `openEnvironment` → capture `OpenEnvironment.id` → `readOpenEnvironment(…, id)` pair when a session must be reused), reading the named property out of the resolved values, and `Value.secret` marks the resolved property redacted; the arm wraps the resolved string in `Redacted.make` so it never widens. The `Doppler` arm drives the `@dopplerhq/node-sdk` facade — `sdk.secrets.get(project, config, name)` reads one secret's `value.computed`, or `sdk.secrets.download(project, config, { format: 'env' })` bulk-loads the config as a dotenv payload parsed into a `ConfigProvider.fromMap`, and the dynamic-lease sub-arm calls `sdk.dynamicSecrets.issueLease({ ttl_sec })` for a short-lived credential, capturing the returned lease `slug` so the TTL-expiry sweep calls `revokeLease(slug)` rather than leaving the lease dangling. The `Static` arm reads the deploy-injected value straight off the ambient `ConfigProvider` through `Config.redacted(key)` with no provider call, the deploy boundary the `provisioning/contract#PROVISIONING` `SecretResolver` populated. Every arm resolves into one `Redacted.Redacted` and threads it into a `LeasedSecret`: the first resolution mints a `SubscriptionRef` seeded with the value plus its `expiresAt`, and subsequent reads return the cached `Redacted` while the lease holds. The rotation edge is `SubscriptionRef.changes`: a `rotate` call (a webhook from the provider, or the TTL-expiry sweep) re-resolves the arm and `SubscriptionRef.set`s the new value, and every dependent owner that subscribed via `subscribe(ref)` re-reads off the `changes` `Stream` without a restart — the load-bearing concept distinguishing a runtime resolver from the deploy-time `SecretResolver`. The `AwsSecretsManager` arm's shape is authored (`secretId`/`versionStage`) for anticipatory collapse but its resolution body fails with `provider_gated` until `@aws-sdk/client-secrets-manager` is admitted, never a half-wired call.
- Auto: `LeasePolicy` is the one `as const satisfies Record` vocabulary keyed by `SecretRef` tag — `EscEnv`/`Doppler`/`Static`/`AwsSecretsManager` — each row carrying the per-provider lease TTL (`Duration`), whether the provider rotates (`rotates: boolean` — ESC/Doppler/AWS rotate, `Static` does not), and the `SecretFault` reason the arm rejects with, read by `keyof typeof` indexed access so a `Match` chain re-deriving the per-provider lease/rotation policy is the deleted form; the `Doppler` dynamic-lease TTL flows straight into `issueLease({ ttl_sec })` so the policy row IS the lease request, never a second constant.
- Entry: `SecretStore` is one `Effect.Service` whose layer carries the `EscApi` client (built once from an explicit `Configuration`, the access token itself resolved through the ambient `ConfigProvider` and passed in) and the `DopplerSDK` facade (constructed with its service token, likewise ambient-resolved), so the provider clients are bootstrapped from the `Static` arm and every other key resolves through them; resolution runs inside the `persistence/tenancy#TENANCY` `withTenant` GUC scope so the `AuditReceipt` insert is RLS-scoped to `app.current_tenant`, and the receipt rides the one `persistence/store#STORE_BOUNDARY` `PgClient` as one `EntityRegistry` `Model.Class` (no sibling schema). The runtime owners read through this one resolver — `auth#VERIFIER` reaches for the API-key/OAuth secrets, `execution/ai#AI_ACTIVITY` for the per-provider AI keys, `persistence/store#STORE_BOUNDARY` for the DSN, the object-store for its credentials — each as one `resolve(SecretRef.Doppler({ … }))` call, never an ad-hoc `Config` read; the deploy-time `provisioning/contract#PROVISIONING` ESC/Doppler resolution and this runtime resolution meet at the one `ConfigProvider` boundary the `Static` arm reads. The TTL-expiry sweep registers as a `execution/backplane#RUNNER_AND_SCHEDULING` `ScheduledWork.singleton` so exactly one runner walks the lease table, re-resolves the rotating rows, and revokes the expired Doppler dynamic leases.
- Wire: the owner crosses no .NET wire and carries no wire type — `SecretRef` is this folder's own provider vocabulary, the `EnvironmentDefinitionValues`/`SecretsGetResponse` payloads are the `@pulumi/esc-sdk`/`@dopplerhq/node-sdk` provider shapes (not a .NET wire), and the C# branch owns no secret-resolution seam this page decodes; the browser owns no key and never reaches this surface.
- Packages: `effect` for the `ConfigProvider`/`Config.redacted`/`Redacted` resolution carrier, the `SubscriptionRef` lease cell and its `changes` rotation `Stream`, the `SecretRef` `Data.TaggedEnum` dispatch, the `LeasePolicy` vocabulary, and the `SecretFault` policy projection; `@pulumi/esc-sdk` for the `EscEnv` arm (`EscApi.openAndReadEnvironment`/`openEnvironment`+`readOpenEnvironment`, the explicit `Configuration` accessToken carrier, the `OpenEnvironment.id` session handle, `EnvironmentDefinitionValues`/`Value.secret`); `@dopplerhq/node-sdk` for the `Doppler` arm (`DopplerSDK`, `sdk.secrets.get`/`sdk.secrets.download` Format `'env'`, `DynamicSecretsService.issueLease`/`revokeLease` for the TTL-leased rows); `@effect/sql`/`@effect/sql-pg` for the `AuditReceipt` `Model.Class` and the lease-table sweep through `persistence/store#STORE_BOUNDARY`; `@effect/cluster` for the `ScheduledWork.singleton` the sweep registers on. The two `.api/` catalogues (`pulumi-esc-sdk.md`, `dopplerhq-node-sdk.md`) deepen the runtime-resolution annotation on already-catalogued surfaces — not a new admission.
- Growth: a new provider lands as one `SecretRef` `Data.TaggedEnum` variant plus one `LeasePolicy` row breaking the `Match.tagsExhaustive` fold at compile time, never a parallel resolver service — `AwsSecretsManager` settles by un-gating its arm once `@aws-sdk/client-secrets-manager` is admitted; a new secret-backed capability is one `resolve(SecretRef…)` call site, never a sibling key reader; a new rotation discipline lands as one `LeasePolicy.rotates`/`ttl` row; a new resolution-failure mode lands as one `SecretFault` reason row on the policy table, never a parallel error class.
- Boundary: the named defects — a bespoke `process.env`/raw-`Config` read instead of one `SecretRef` resolution through the one `ConfigProvider` boundary; a sibling resolver method or service per provider instead of the one `SecretRef` `Match.tagsExhaustive`; two parallel secret schemes (deploy-time and runtime) instead of the one boundary they meet at; a secret widened to a plain `string` instead of held as `Redacted.Redacted`; a rotation forcing a process restart instead of pushing through the `SubscriptionRef.changes` edge; a Doppler dynamic lease issued without capturing the `slug` for `revokeLease`, leaking the lease; an `AwsSecretsManager` arm settled as an active row before `@aws-sdk/client-secrets-manager` admission; a resolution that skips the tenancy-GUC `AuditReceipt`. This is a node-only surface, never browser-reachable.

```ts contract
import type { EnvironmentDefinitionValues } from "@pulumi/esc-sdk"
import { Configuration, EscApi } from "@pulumi/esc-sdk"
import DopplerSDK from "@dopplerhq/node-sdk"
import type { PgClient } from "@effect/sql-pg"
import { Model, SqlClient, SqlError } from "@effect/sql"
import { Config, ConfigError, Data, Duration, Effect, Layer, Match, Redacted, Schema as S, Stream, SubscriptionRef } from "effect"

// --- [TYPES] -----------------------------------------------------------------------------

type SecretRef = Data.TaggedEnum<{
  readonly EscEnv:            { readonly org: string; readonly project: string; readonly env: string; readonly property: string }
  readonly Doppler:           { readonly project: string; readonly config: string; readonly name: string; readonly dynamic: boolean }
  readonly Static:            { readonly key: string }
  readonly AwsSecretsManager: { readonly secretId: string; readonly versionStage: string }
}>
const SecretRef = Data.taggedEnum<SecretRef>()

type ProviderTag = SecretRef["_tag"]

// --- [CONSTANTS] -------------------------------------------------------------------------

const LeasePolicy = {
  EscEnv:            { ttl: Duration.minutes(15), rotates: true,  reason: "esc_resolve"     },
  Doppler:           { ttl: Duration.minutes(5),  rotates: true,  reason: "doppler_resolve" },
  Static:            { ttl: Duration.hours(24),   rotates: false, reason: "static_absent"   },
  AwsSecretsManager: { ttl: Duration.minutes(15), rotates: true,  reason: "provider_gated"  },
} as const satisfies Record<ProviderTag, { ttl: Duration.Duration; rotates: boolean; reason: keyof typeof SecretPolicy }>

const SecretPolicy = {
  esc_resolve:     { status: 502, retryable: true,  ord: 0 },
  doppler_resolve: { status: 502, retryable: true,  ord: 1 },
  static_absent:   { status: 500, retryable: false, ord: 2 },
  lease_expired:   { status: 410, retryable: true,  ord: 3 },
  provider_gated:  { status: 501, retryable: false, ord: 4 },
} as const satisfies Record<string, { status: number; retryable: boolean; ord: number }>

// --- [MODELS] ----------------------------------------------------------------------------

class AuditReceipt extends Model.Class<AuditReceipt>("AuditReceipt")({
  id: Model.Generated(S.UUID),
  provider: S.Literal("EscEnv", "Doppler", "Static", "AwsSecretsManager"),
  keyHash: S.String,
  rotated: S.Boolean,
  leaseSlug: S.optionalWith(S.String, { as: "Option" }),
  resolvedAt: Model.DateTimeInsert,
}) {}

interface LeasedSecret {
  readonly ref: SecretRef
  readonly cell: SubscriptionRef.SubscriptionRef<{ readonly value: Redacted.Redacted; readonly expiresAt: number; readonly slug: ReturnType<typeof Redacted.make> | undefined }>
}

// --- [ERRORS] ----------------------------------------------------------------------------

class SecretFault extends S.TaggedError<SecretFault>()("SecretFault", {
  reason: S.Literal(...(Object.keys(SecretPolicy) as [keyof typeof SecretPolicy, ...Array<keyof typeof SecretPolicy>])),
  provider: S.Literal("EscEnv", "Doppler", "Static", "AwsSecretsManager"),
  detail: S.String,
}) {
  get status()    { return SecretPolicy[this.reason].status }
  get retryable() { return SecretPolicy[this.reason].retryable }
}

// --- [SERVICES] --------------------------------------------------------------------------

interface ProviderClients {
  readonly esc: EscApi
  readonly doppler: DopplerSDK
}

// --- [OPERATIONS] ------------------------------------------------------------------------

const refKey = (ref: SecretRef): string =>
  Match.value(ref).pipe(
    Match.tagsExhaustive({
      EscEnv:            ({ org, project, env, property }) => `esc:${org}/${project}/${env}#${property}`,
      Doppler:           ({ project, config, name })       => `doppler:${project}/${config}#${name}`,
      Static:            ({ key })                          => `static:${key}`,
      AwsSecretsManager: ({ secretId, versionStage })      => `aws:${secretId}@${versionStage}`,
    }),
  )

const resolveEsc = (esc: EscApi, ref: Extract<SecretRef, { readonly _tag: "EscEnv" }>): Effect.Effect<Redacted.Redacted, SecretFault> =>
  Effect.tryPromise({
    try: () => esc.openAndReadEnvironment(ref.org, ref.project, ref.env),
    catch: (cause) => new SecretFault({ reason: "esc_resolve", provider: "EscEnv", detail: String(cause) }),
  }).pipe(
    Effect.flatMap((resolved: { values?: EnvironmentDefinitionValues } | undefined) => {
      const raw = (resolved?.values as Record<string, unknown> | undefined)?.[ref.property]
      return raw === undefined
        ? Effect.fail(new SecretFault({ reason: "esc_resolve", provider: "EscEnv", detail: `absent: ${ref.property}` }))
        : Effect.succeed(Redacted.make(String((raw as { value?: unknown }).value ?? raw)))
    }),
  )

const resolveDoppler = (doppler: DopplerSDK, ref: Extract<SecretRef, { readonly _tag: "Doppler" }>): Effect.Effect<{ readonly value: Redacted.Redacted; readonly slug: string | undefined }, SecretFault> =>
  ref.dynamic
    ? Effect.tryPromise({
        try: () => doppler.dynamicSecrets.issueLease({ project: ref.project, config: ref.config, name: ref.name, ttl_sec: Math.trunc(Duration.toSeconds(LeasePolicy.Doppler.ttl)) }),
        catch: (cause) => new SecretFault({ reason: "doppler_resolve", provider: "Doppler", detail: String(cause) }),
      }).pipe(
        Effect.map((lease: { slug?: string; value?: string }) => ({ value: Redacted.make(lease.value ?? ""), slug: lease.slug })),
      )
    : Effect.tryPromise({
        try: () => doppler.secrets.get(ref.project, ref.config, ref.name),
        catch: (cause) => new SecretFault({ reason: "doppler_resolve", provider: "Doppler", detail: String(cause) }),
      }).pipe(
        Effect.map((res: { value?: { computed?: string; raw?: string } }) => ({ value: Redacted.make(res.value?.computed ?? res.value?.raw ?? ""), slug: undefined })),
      )

const resolveArm = (clients: ProviderClients, ref: SecretRef): Effect.Effect<{ readonly value: Redacted.Redacted; readonly slug: string | undefined }, SecretFault | ConfigError.ConfigError> =>
  Match.value(ref).pipe(
    Match.tagsExhaustive({
      EscEnv:            (r) => resolveEsc(clients.esc, r).pipe(Effect.map((value) => ({ value, slug: undefined }))),
      Doppler:           (r) => resolveDoppler(clients.doppler, r),
      Static:            (r) => Config.redacted(r.key).pipe(Effect.map((value) => ({ value, slug: undefined }))),
      AwsSecretsManager: (r) => Effect.fail(new SecretFault({ reason: "provider_gated", provider: "AwsSecretsManager", detail: r.secretId })),
    }),
  )

const lease = (clients: ProviderClients, ref: SecretRef): Effect.Effect<LeasedSecret, SecretFault | ConfigError.ConfigError> =>
  resolveArm(clients, ref).pipe(
    Effect.flatMap(({ value, slug }) =>
      SubscriptionRef.make({ value, expiresAt: Date.now() + Duration.toMillis(LeasePolicy[ref._tag].ttl), slug: slug === undefined ? undefined : Redacted.make(slug) }).pipe(
        Effect.map((cell): LeasedSecret => ({ ref, cell })),
      ),
    ),
  )

const audit = (sql: SqlClient.SqlClient, ref: SecretRef, rotated: boolean, slug: string | undefined): Effect.Effect<void, SqlError.SqlError> =>
  sql`INSERT INTO audit_receipt (provider, key_hash, rotated, lease_slug)
      VALUES (${ref._tag}, ${refKey(ref)}, ${rotated}, ${slug ?? null})`

const resolve = (sql: SqlClient.SqlClient, clients: ProviderClients, cache: Map<string, LeasedSecret>) =>
  (ref: SecretRef): Effect.Effect<Redacted.Redacted, SecretFault | ConfigError.ConfigError | SqlError.SqlError> =>
    Effect.suspend(() => {
      const key = refKey(ref)
      const held = cache.get(key)
      return held === undefined
        ? lease(clients, ref).pipe(
            Effect.tap((leased) => Effect.sync(() => cache.set(key, leased))),
            Effect.flatMap((leased) => leased.cell.get.pipe(Effect.tap((s) => audit(sql, ref, false, Redacted.value(s.slug ?? Redacted.make("")) || undefined)), Effect.map((s) => s.value))),
          )
        : held.cell.get.pipe(
            Effect.flatMap((snapshot) =>
              snapshot.expiresAt > Date.now()
                ? Effect.succeed(snapshot.value)
                : rotate(sql, clients, cache)(ref),
            ),
          )
    })

const rotate = (sql: SqlClient.SqlClient, clients: ProviderClients, cache: Map<string, LeasedSecret>) =>
  (ref: SecretRef): Effect.Effect<Redacted.Redacted, SecretFault | ConfigError.ConfigError | SqlError.SqlError> =>
    resolveArm(clients, ref).pipe(
      Effect.flatMap(({ value, slug }) => {
        const held = cache.get(refKey(ref))
        const next = { value, expiresAt: Date.now() + Duration.toMillis(LeasePolicy[ref._tag].ttl), slug: slug === undefined ? undefined : Redacted.make(slug) }
        return held === undefined
          ? lease(clients, ref).pipe(Effect.tap((leased) => Effect.sync(() => cache.set(refKey(ref), leased))), Effect.as(value))
          : SubscriptionRef.set(held.cell, next).pipe(Effect.zipRight(audit(sql, ref, true, slug)), Effect.as(value))
      }),
    )

const subscribe = (sql: SqlClient.SqlClient, clients: ProviderClients, cache: Map<string, LeasedSecret>) =>
  (ref: SecretRef): Effect.Effect<Stream.Stream<Redacted.Redacted>, SecretFault | ConfigError.ConfigError> =>
    Effect.suspend(() => {
      const held = cache.get(refKey(ref))
      return held === undefined
        ? lease(clients, ref).pipe(
            Effect.tap((leased) => Effect.sync(() => cache.set(refKey(ref), leased))),
            Effect.map((leased) => leased.cell.changes.pipe(Stream.map((s) => s.value))),
          )
        : Effect.succeed(held.cell.changes.pipe(Stream.map((s) => s.value)))
    })

const sweep = (sql: SqlClient.SqlClient, clients: ProviderClients, cache: Map<string, LeasedSecret>): Effect.Effect<number, SecretFault | ConfigError.ConfigError | SqlError.SqlError> =>
  Effect.forEach(
    [...cache.values()].filter((leased) => LeasePolicy[leased.ref._tag].rotates),
    (leased) =>
      leased.cell.get.pipe(
        Effect.flatMap((snapshot) =>
          snapshot.expiresAt > Date.now()
            ? Effect.succeed(0)
            : Effect.zipRight(
                snapshot.slug === undefined
                  ? Effect.void
                  : revokeDopplerLease(clients.doppler, leased.ref, Redacted.value(snapshot.slug)),
                rotate(sql, clients, cache)(leased.ref).pipe(Effect.as(1)),
              ),
        ),
      ),
    { concurrency: "unbounded" },
  ).pipe(Effect.map((counts) => counts.reduce((a, b) => a + b, 0)))

const revokeDopplerLease = (doppler: DopplerSDK, ref: SecretRef, slug: string): Effect.Effect<void, SecretFault> =>
  ref._tag === "Doppler"
    ? Effect.tryPromise({
        try: () => doppler.dynamicSecrets.revokeLease({ project: ref.project, config: ref.config, name: ref.name, slug }),
        catch: (cause) => new SecretFault({ reason: "doppler_resolve", provider: "Doppler", detail: String(cause) }),
      }).pipe(Effect.asVoid)
    : Effect.void

// --- [SERVICES] --------------------------------------------------------------------------

const makeClients: Effect.Effect<ProviderClients, ConfigError.ConfigError> =
  Effect.gen(function* () {
    const escToken = yield* Config.redacted("PULUMI_ACCESS_TOKEN")
    const dopplerToken = yield* Config.redacted("DOPPLER_TOKEN")
    return {
      esc: new EscApi(new Configuration({ accessToken: Redacted.value(escToken) })),
      doppler: new DopplerSDK({ accessToken: Redacted.value(dopplerToken) }),
    }
  })

class SecretStore extends Effect.Service<SecretStore>()("services/SecretStore", {
  accessors: true,
  effect: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const clients = yield* makeClients
    const cache = new Map<string, LeasedSecret>()
    return {
      resolve: resolve(sql, clients, cache),
      subscribe: subscribe(sql, clients, cache),
      rotate: rotate(sql, clients, cache),
      sweep: sweep(sql, clients, cache),
    } as const
  }),
}) {}

// --- [COMPOSITION] -----------------------------------------------------------------------

const SecretStoreLayer: Layer.Layer<SecretStore, ConfigError.ConfigError, SqlClient.SqlClient | PgClient.PgClient> =
  SecretStore.Default
```

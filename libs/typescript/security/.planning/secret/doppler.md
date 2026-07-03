# [SECURITY_DOPPLER] — the Doppler leased-secret axis: TTL rotation, Redacted end-to-end

`secret/doppler` is the leased-secret custody owner: one `DopplerSDK` client built behind a `Layer.scoped`, of which the design admits exactly three surfaces — `secrets.download` (the leased env-set fetch), `dynamicSecrets.issueLease`/`revokeLease` (the explicit dynamic-lease lifecycle), and `auth.me` (the startup liveness probe) — and treats the projects/configs/integrations administration roster as out of scope, because a runtime folder that reached for it would be re-implementing Doppler administration. The 39 `BaseHTTPError` status subclasses collapse to one `SecretFault` reason-family folded on `statusCode`, the same folder fault shape. TTL leasing is Doppler-side (`dynamicSecretsTtlSec`, default 1800s): the design refetches on a `Schedule` slightly under the window, republishes the set through a `SubscriptionRef` so `sign` observes rotation via `Stream.changes`, and revokes every explicitly-issued lease in a `Scope` finalizer. Every fetched value is `Redacted` from the first decode and the `DOPPLER_TOKEN` is a `Config.redacted`, so a secret never exists as a bare string in the interior; the fetched key material terminates in `secret/material` as `CredentialPemWire`, and `sign` sources JWT keys, webhook HMAC secrets, and the argon2 pepper here — never talking to Doppler directly.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                        | [OWNER]                              | [PACKAGES]            | [REJECTED_FORM]                             |
| :-----: | :------------------------------- | :----------------------------------- | :-------------------- | :------------------------------------------ |
|  [01]   | `BaseHTTPError` → tagged fault   | `SecretFault` `statusCode`-fold      | `@dopplerhq/node-sdk` | an `instanceof` ladder over 39 subclasses   |
|  [02]   | leased env-set fetch + rotation  | `Secret` scoped service, `SubscriptionRef` | `@dopplerhq/node-sdk` | a per-consumer refetch, a plaintext cache   |
|  [03]   | explicit dynamic-lease lifecycle | `Secret.lease` / `Secret.revoke`     | `@dopplerhq/node-sdk` | a lease left to silently expire on teardown |

## [2]-[SECRET_VOCABULARY]

[SET_AND_FAULT]:
- Owner: `SecretSet` is the decoded name→`Redacted<string>` map published through a `SubscriptionRef`; `SecretFault` is the `statusCode`-folded reason-family. `Config.redacted` sources the token, `Redacted.make` wraps every value at decode.
- Packages: `@dopplerhq/node-sdk` — `BaseHTTPError` carries an RFC 9457 `statusCode`, so `401/403` fold to `credential`, `404` to `missing`, `429` to `rateLimit`, `5xx` to `transient`; a network throw is `transient` too.
- Boundary: `secret/material` decodes the fetched PEM/JWK strings into `CredentialPemWire`; the edge maps `policy.status`; the `retry`-gated `transient`/`rateLimit` reasons drive the client's own transport retry and the design's refresh `Schedule`, which are orthogonal.
- Growth: a new failure class is one `_reasonOf` status arm; a new fetched secret is a new key in the same `download` response, no code change.

```typescript
import DopplerSDK from "@dopplerhq/node-sdk"
import { Config, Duration, Effect, HashMap, Option, Redacted, Ref, Schedule, Schema, SubscriptionRef } from "effect"

// --- [TYPES] ----------------------------------------------------------------------------

type SecretSet = HashMap.HashMap<string, Redacted.Redacted<string>>
type LeaseSpec = Parameters<DopplerSDK["dynamicSecrets"]["issueLease"]>[0]
type LeaseHandle = Parameters<DopplerSDK["dynamicSecrets"]["revokeLease"]>[0]

// --- [CONSTANTS] ------------------------------------------------------------------------

const _reasons = ["credential", "missing", "rateLimit", "transient", "lease"] as const

const SecretFaultPolicy = {
  credential: { rank: 5, retry: false, status: 500 },
  missing: { rank: 3, retry: false, status: 500 },
  rateLimit: { rank: 2, retry: true, status: 503 },
  transient: { rank: 2, retry: true, status: 503 },
  lease: { rank: 4, retry: false, status: 500 },
} as const

declare namespace SecretFault {
  type Reason = keyof typeof SecretFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof SecretFaultPolicy> = T
}

// --- [ERRORS] ---------------------------------------------------------------------------

class SecretFault extends Schema.TaggedError<SecretFault>()("SecretFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): SecretFault.Row {
    return SecretFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<secret:${this.reason}> ${this.detail}`
  }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

const _reasonOf = (cause: unknown): SecretFault.Reason => {
  const status = (cause as { readonly statusCode?: number }).statusCode
  return status === undefined ? "transient"
    : status === 401 || status === 403 ? "credential"
    : status === 404 ? "missing"
    : status === 429 ? "rateLimit"
    : status >= 500 ? "transient"
    : "lease"
}

const _decode = Schema.decodeUnknown(Schema.Record({ key: Schema.String, value: Schema.String }))

const _set = (raw: unknown): Effect.Effect<SecretSet, SecretFault> =>
  _decode(raw).pipe(
    Effect.mapError((cause) => new SecretFault({ reason: "missing", detail: String(cause) })),
    Effect.map((record) => HashMap.map(HashMap.fromIterable(Object.entries(record)), Redacted.make)),
  )
```

## [3]-[LEASED_CUSTODY]

[CUSTODY]:
- Owner: `Secret` — a `Layer.scoped` service holding one client, publishing the current set through a `SubscriptionRef`, refreshing on a `Schedule.spaced` under the lease window, and revoking every issued lease in an `addFinalizer`. `get` reads the current cell, `changes` is the rotation feed, `lease`/`revoke` are the explicit dynamic-lease passthrough whose handles the finalizer drains.
- Packages: `@dopplerhq/node-sdk` — `secrets.download` with `includeDynamicSecrets`+`dynamicSecretsTtlSec` issues the inline TTL lease, `auth.me` probes the token at boot, `dynamicSecrets.issueLease`/`revokeLease` own the explicit lifecycle; the SDK's own `retryAttempts` covers transport, the `Schedule` covers lease-window refresh.
- Law: the SDK owns its node HTTP transport, so Doppler is not routed through `host/net`'s `HttpClient` — the boundary is the `Effect.tryPromise` seam; a `download` failure keeps the last good set (the refresh loop retries), and a lease outlives the fetch that issued it only until the finalizer revokes it.
- Receipt: `SecretSet` — a `missing` key is a `SecretFault`, never an `undefined`; the whole set is `Redacted`-valued, so a log or error never carries a plaintext secret.

```typescript
// --- [SERVICES] -------------------------------------------------------------------------

class Secret extends Effect.Service<Secret>()("security/secret/Secret", {
  scoped: Effect.gen(function* () {
    const token = yield* Config.redacted("DOPPLER_TOKEN")
    const project = yield* Config.string("DOPPLER_PROJECT")
    const config = yield* Config.string("DOPPLER_CONFIG")
    const ttl = yield* Config.integer("DOPPLER_LEASE_TTL").pipe(Config.withDefault(1800))
    const sdk = new DopplerSDK({ accessToken: Redacted.value(token) })
    const leases = yield* Ref.make<ReadonlyArray<LeaseHandle>>([])
    const fetch = Effect.tryPromise({
      try: () => sdk.secrets.download(project, config, { format: "json", includeDynamicSecrets: true, dynamicSecretsTtlSec: ttl }),
      catch: (cause) => new SecretFault({ reason: _reasonOf(cause), detail: String(cause) }),
    }).pipe(Effect.flatMap(_set))
    yield* Effect.tryPromise({ try: () => sdk.auth.me(), catch: (cause) => new SecretFault({ reason: _reasonOf(cause), detail: String(cause) }) })
    const cell = yield* SubscriptionRef.make(yield* fetch)
    yield* Effect.forkScoped(Effect.repeat(Effect.flatMap(fetch, (set) => SubscriptionRef.set(cell, set)), Schedule.spaced(Duration.seconds(Math.max(1, Math.floor(ttl * 0.8))))))
    yield* Effect.addFinalizer(() =>
      Ref.get(leases).pipe(Effect.flatMap((held) => Effect.forEach(held, (handle) => Effect.ignore(Effect.promise(() => sdk.dynamicSecrets.revokeLease(handle))), { discard: true }))))
    const get = (name: string): Effect.Effect<Redacted.Redacted<string>, SecretFault> =>
      Effect.flatMap(SubscriptionRef.get(cell), (set) =>
        Option.match(HashMap.get(set, name), { onNone: () => Effect.fail(new SecretFault({ reason: "missing", detail: name })), onSome: Effect.succeed }))
    const lease = (spec: LeaseSpec): Effect.Effect<void, SecretFault> =>
      Effect.tryPromise({ try: () => sdk.dynamicSecrets.issueLease(spec), catch: (cause) => new SecretFault({ reason: "lease", detail: String(cause) }) }).pipe(
        Effect.flatMap((handle) => Ref.update(leases, (held) => [...held, handle as LeaseHandle])))
    const revoke = (handle: LeaseHandle): Effect.Effect<void, SecretFault> =>
      Effect.tryPromise({ try: () => sdk.dynamicSecrets.revokeLease(handle), catch: (cause) => new SecretFault({ reason: "lease", detail: String(cause) }) }).pipe(Effect.asVoid)
    return { get, lease, revoke, changes: cell.changes } as const
  }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Secret, SecretFault }
export type { SecretSet }
```

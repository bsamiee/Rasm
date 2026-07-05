# [SECURITY_SECRET]

Leased-secret custody: one `DopplerSDK` client built behind a `Layer.scoped`, of which exactly four surfaces are admitted — `secrets.download` (the leased env-set fetch), `secrets.get`/`secrets.names` (the targeted single-secret read and the name census a partial refresh rides), `dynamicSecrets.issueLease`/`revokeLease` (the explicit dynamic-lease lifecycle), and `auth.me`/`auth.revoke` (the boot liveness probe and the credential-rotation retirement). The projects/configs/integrations administration roster is out of scope: a runtime folder that reached for it re-implements Doppler administration, which belongs to the deploy plane. TTL leasing is Doppler-side (`dynamicSecretsTtlSec`): the custodian refetches on a spaced cadence under the lease window, with an inner jittered-exponential retry gated on `FaultClass.retryable` re-driving a transient fault inside the tick and a per-call deadline bounding every SDK promise; an `effect` `Cache` collapses concurrent refetches of the one `(project, config)` coordinate to a single in-flight download. Rotation republishes through a `SubscriptionRef` — an observed set change increments `security_secret_rotation` and writes the audit log line — and `changes` is the feed the composition root's `Reloadable.auto` row consumes to rebuild `Jwt.Default(keyset)` without a graph teardown. Every fetched value is `Redacted` from the first decode, the `DOPPLER_TOKEN` is a `Config.redacted`, and fetched key material leaves this page only as the core `Credential` landing — `credential` folds a named PEM/JWK value into the sealed carrier `crypt/sign`'s `Material.admit` terminates, so the folder has one admission path for wire-carried and fetched keys alike. `SecretFault` instantiates the folder fault shape with the guard pair closed in both directions: the 39 `BaseHTTPError` status subclasses collapse to one reason family whose rows carry the core `FaultClass` kind.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                              | [PUBLIC]      |
| :-----: | :---------------- | :-------------------------------------------------------------------- | :------------ |
|  [01]   | `SECRET_FAULT`    | the `statusCode`-folded reason family over the problem-detail carrier | `SecretFault` |
|  [02]   | `LEASED_CUSTODY`  | the scoped client, rotation feed, partial refresh, lease lifecycle    | `Secret`      |
|  [03]   | `KEY_HANDOFF`     | the fetched-material fold into the core `Credential` landing          | `Secret`      |

## [2]-[SECRET_FAULT]

[SECRET_FAULT]:
- Owner: `SecretFault` — the folder fault shape instantiated over the Doppler axis: `credential` (401/403 — the service token is dead), `missing` (404 or an absent name), `rateLimit` (429), `transient` (5xx, network throws, and a per-call deadline), `lease` (lease issue/revoke refused). Rows carry the core `FaultClass` kind; the refresh loop's inner retry and the compiled branch `Budget` schedules gate on `FaultClass.retryable`, so the `rateLimit`/`transient` arms re-drive and the `credential` arm never does.
- Law: the fold reads `statusCode` off the RFC 9457 `BaseHTTPError` carrier — 39 status subclasses are seed data over one problem-detail shape, and an instance-of ladder over them is the rejected form; a throw without a `statusCode` is `transient`.
- Growth: a new failure class is one `_reasonOf` status arm plus one class row.
- Packages: `@dopplerhq/node-sdk` (`BaseHTTPError` carrier); `effect` (`Schema`, `Predicate`); `@rasm/ts/core` (`FaultClass`).

```typescript
import DopplerSDK from "@dopplerhq/node-sdk"
import { Credential, FaultClass } from "@rasm/ts/core"
import { Cache, Config, DateTime, Duration, Effect, Equal, HashMap, Metric, Option, Predicate, Record, Redacted, Ref, Schedule, Schema, SubscriptionRef } from "effect"
import { Crypto } from "./sign.ts"

type SecretSet = HashMap.HashMap<string, Redacted.Redacted<string>>
type LeaseSpec = Parameters<DopplerSDK["dynamicSecrets"]["issueLease"]>[0]
type LeaseGrant = Awaited<ReturnType<DopplerSDK["dynamicSecrets"]["issueLease"]>>
type LeaseHandle = Parameters<DopplerSDK["dynamicSecrets"]["revokeLease"]>[0]

const _reasons = ["credential", "missing", "rateLimit", "transient", "lease"] as const

const _faults = {
  credential: { class: "denied" },
  missing: { class: "absent" },
  rateLimit: { class: "exhausted" },
  transient: { class: "unavailable" },
  lease: { class: "invalid" },
} as const

declare namespace SecretFault {
  type Reason = (typeof _reasons)[number]
  type _Rows<T extends { readonly [K in Reason]: { readonly class: FaultClass.Kind } } = typeof _faults> = T
  type _Closed<K extends Reason = keyof typeof _faults> = K
}

class SecretFault extends Schema.TaggedError<SecretFault>()("SecretFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<secret:${this.reason}> ${this.detail}`
  }
}

const _reasonOf = (cause: unknown): SecretFault.Reason =>
  !Predicate.hasProperty(cause, "statusCode") || !Predicate.isNumber(cause.statusCode) ? "transient"
    : cause.statusCode === 401 || cause.statusCode === 403 ? "credential"
    : cause.statusCode === 404 ? "missing"
    : cause.statusCode === 429 ? "rateLimit"
    : cause.statusCode >= 500 ? "transient"
    : "lease"

const _decode = Schema.decodeUnknown(Schema.Record({ key: Schema.String, value: Schema.String }))

const _set = (raw: unknown): Effect.Effect<SecretSet, SecretFault> =>
  _decode(raw).pipe(
    Effect.mapError((cause) => new SecretFault({ reason: "missing", detail: String(cause) })),
    Effect.map((record) => HashMap.map(HashMap.fromIterable(Record.toEntries(record)), Redacted.make)),
  )
```

## [3]-[LEASED_CUSTODY]

[LEASED_CUSTODY]:
- Owner: `Secret` — a `Layer.scoped` service holding one client, publishing the current set through a `SubscriptionRef`, refreshing on a spaced cadence under the lease window, and revoking every issued lease in an `addFinalizer`. `get` reads the current cell; `probe` is the targeted single-secret read that refreshes one name without a full download; `names` is the census a partial-refresh planner diffs against the held set; `changes` is the rotation feed; `lease` issues and registers a revocable handle; `revoke` is the immediate arm; `retire` calls `auth.revoke` on credential rotation so a superseded service token dies server-side.
- Law: two schedules are orthogonal and both explicit — the outer `Schedule.spaced` cadence paces refresh across ticks, and the inner `Effect.retry` (jittered exponential, bounded by `Schedule.recurs`, gated on `FaultClass.retryable`) re-drives a transient fault inside the tick, so a Doppler blip costs milliseconds, never a full stale interval; a tick whose retries exhaust keeps the last good set.
- Law: the download de-dupes — an `effect` `Cache` keyed by the one `(project, config)` coordinate with a TTL at half the refresh cadence collapses a concurrent `probe`-storm or refresh overlap to one in-flight request; every SDK promise carries the per-call deadline, so a hung transport is a typed `transient`, never a wedged fiber.
- Law: the boot probe (`auth.me`) and the first fetch gate construction under a bounded jittered retry — a transient boot blip re-drives, a dead token fails the layer, not the first read; the composition root additionally wraps `Secret.Default` in `Layer.retry` under the branch boot budget.
- Law: a rotation is observed, never silent — a refreshed set that differs structurally from the held set increments `security_secret_rotation` and logs the audit line before republication, and `probe` folds its single value into the same cell so consumers observe one rotation stream regardless of refresh grain.
- Receipt: `SecretSet` — a missing key is a `SecretFault`, never `undefined`; every value is `Redacted`, so a log or error never carries plaintext.
- Growth: a new fetched secret is a new name in the same response; a new refresh grain is a caller-composed diff over `names` plus `probe`, never a second custody service.
- Packages: `@dopplerhq/node-sdk` (`secrets.download`/`get`/`names`, `dynamicSecrets.issueLease`/`revokeLease`, `auth.me`/`revoke`); `effect` (`Cache`, `Schedule`, `SubscriptionRef`, `Metric`).

```typescript
const _rotation = Metric.counter("security_secret_rotation")

const _retryable = Schedule.exponential(Duration.seconds(1)).pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(4)))

class Secret extends Effect.Service<Secret>()("security/crypt/Secret", {
  scoped: Effect.gen(function* () {
    const token = yield* Config.redacted("DOPPLER_TOKEN")
    const project = yield* Config.string("DOPPLER_PROJECT")
    const config = yield* Config.string("DOPPLER_CONFIG")
    const ttl = yield* Config.integer("DOPPLER_LEASE_TTL").pipe(Config.withDefault(1800))
    const deadline = yield* Config.duration("DOPPLER_CALL_DEADLINE").pipe(Config.withDefault(Duration.seconds(10)))
    const sdk = new DopplerSDK({ accessToken: Redacted.value(token) })
    const leases = yield* Ref.make<ReadonlyArray<LeaseHandle>>([])
    const _lift = <A>(run: () => Promise<A>, reason?: SecretFault.Reason): Effect.Effect<A, SecretFault> =>
      Effect.tryPromise({ try: run, catch: (cause) => new SecretFault({ reason: reason ?? _reasonOf(cause), detail: String(cause) }) }).pipe(
        Effect.timeoutFail({ duration: deadline, onTimeout: () => new SecretFault({ reason: "transient", detail: "doppler deadline" }) }))
    const _download = _lift(() => sdk.secrets.download(project, config, { format: "json", includeDynamicSecrets: true, dynamicSecretsTtlSec: ttl })).pipe(Effect.flatMap(_set))
    const deduped = yield* Cache.make({
      capacity: 1,
      timeToLive: Duration.seconds(Math.max(1, Math.floor(ttl * 0.4))),
      lookup: (_: string) => _download,
    })
    const fetch = deduped.get(`${project}/${config}`)
    yield* _lift(() => sdk.auth.me()).pipe(Effect.retry({ schedule: _retryable, while: (fault) => FaultClass.retryable(fault.class) }))
    const cell = yield* SubscriptionRef.make(yield* fetch.pipe(Effect.retry({ schedule: _retryable, while: (fault) => FaultClass.retryable(fault.class) })))
    const _publish = (set: SecretSet): Effect.Effect<void> =>
      Effect.flatMap(SubscriptionRef.get(cell), (prior) =>
        Effect.zipRight(
          Equal.equals(prior, set)
            ? Effect.void
            : Effect.zipRight(Metric.increment(_rotation), Effect.logInfo("secret rotation observed")),
          SubscriptionRef.set(cell, set)))
    yield* Effect.forkScoped(Effect.repeat(
      fetch.pipe(
        Effect.retry({ schedule: _retryable, while: (fault) => FaultClass.retryable(fault.class) }),
        Effect.flatMap(_publish),
        Effect.ignore,
      ),
      Schedule.spaced(Duration.seconds(Math.max(1, Math.floor(ttl * 0.8)))),
    ))
    yield* Effect.addFinalizer(() =>
      Ref.get(leases).pipe(Effect.flatMap((held) =>
        Effect.forEach(held, (handle) => Effect.ignore(Effect.promise(() => sdk.dynamicSecrets.revokeLease(handle))), { discard: true }))))
    const get = (name: string): Effect.Effect<Redacted.Redacted<string>, SecretFault> =>
      Effect.flatMap(SubscriptionRef.get(cell), (set) =>
        Option.match(HashMap.get(set, name), {
          onNone: () => Effect.fail(new SecretFault({ reason: "missing", detail: name })),
          onSome: Effect.succeed,
        }))
    const probe = (name: string): Effect.Effect<Redacted.Redacted<string>, SecretFault> =>
      _lift(() => sdk.secrets.get(project, config, name)).pipe(
        Effect.flatMap((response) =>
          Schema.decodeUnknown(Schema.Struct({ value: Schema.Struct({ raw: Schema.String }) }))(response).pipe(
            Effect.mapError((cause) => new SecretFault({ reason: "missing", detail: String(cause) })))),
        Effect.map((decoded) => Redacted.make(decoded.value.raw)),
        Effect.tap((value) => SubscriptionRef.update(cell, HashMap.set(name, value))),
      )
    const names = (): Effect.Effect<ReadonlyArray<string>, SecretFault> =>
      _lift(() => sdk.secrets.names(project, config)).pipe(
        Effect.flatMap((response) =>
          Schema.decodeUnknown(Schema.Struct({ names: Schema.Array(Schema.String) }))(response).pipe(
            Effect.mapError((cause) => new SecretFault({ reason: "missing", detail: String(cause) })))),
        Effect.map((decoded) => decoded.names),
      )
    const lease = (spec: LeaseSpec, handle: (grant: LeaseGrant) => LeaseHandle): Effect.Effect<LeaseGrant, SecretFault> =>
      _lift(() => sdk.dynamicSecrets.issueLease(spec), "lease").pipe(
        Effect.tap((grant) => Ref.update(leases, (held) => [...held, handle(grant)])))
    const revoke = (handle: LeaseHandle): Effect.Effect<void, SecretFault> =>
      _lift(() => sdk.dynamicSecrets.revokeLease(handle), "lease").pipe(Effect.asVoid)
    const retire = (spent: Redacted.Redacted<string>): Effect.Effect<void, SecretFault> =>
      _lift(() => sdk.auth.revoke({ token: Redacted.value(spent) }), "credential").pipe(Effect.asVoid)
    return { get, probe, names, lease, revoke, retire, changes: cell.changes } as const
  }),
  accessors: true,
}) {}
```

## [4]-[KEY_HANDOFF]

[KEY_HANDOFF]:
- Owner: `credential` — the fold from a named fetched value into the core `Credential` landing: the PEM/JWK string stays `Redacted`, the `fingerprint` is `Crypto.fingerprint` over the sealed material, and the validity window spans the current instant to the configured rotation horizon. The result is the exact carrier `crypt/sign`'s `Material.admit` terminates, so fetched and wire-carried keys share one admission path and `Credential.rotated` compares sealed across refreshes.
- Law: this page never imports jose and never inspects key structure — format sniffing, importer selection, and thumbprint identity are `crypt/sign`'s admission concerns; the custodian only seals, fingerprints, and windows.
- Law: signing keys, webhook HMAC secrets, and the argon2 pepper are sourced here and injected into the `crypt` layers at construction — no sibling talks to Doppler directly.
- Law: the rotation loop is sealed end to end — `Secret.changes` drives the composition root's `Reloadable.auto` row, which re-runs `credential` → `Material.ring` → `Jwt.Default(keyset)` on each observed rotation, so a Doppler key roll lands as a live ring swap with no graph teardown and no restart.
- Growth: a new credential kind row (`tls`, `api`) is the same fold with a different `kind` literal; a per-name rotation horizon is one config row.
- Boundary: this page produces carriers and never holds a `CryptoKey`; the composition root owns the `Reloadable` wiring.
- Packages: `@rasm/ts/core` (`Credential`); `crypt/sign` (`Crypto.fingerprint`).

```typescript
const credential = (
  name: string,
  kind: Credential["kind"],
  horizon: Duration.DurationInput,
): Effect.Effect<Credential, SecretFault, Crypto | Secret> =>
  Effect.gen(function* () {
    const cipher = yield* Crypto
    const custody = yield* Secret
    const material = yield* custody.get(name)
    const now = yield* DateTime.now
    return new Credential({
      kind,
      material,
      fingerprint: cipher.fingerprint(material),
      notBefore: now,
      notAfter: DateTime.addDuration(now, Duration.decode(horizon)),
    })
  })

// --- [EXPORTS] --------------------------------------------------------------------------

export { credential, Secret, SecretFault }
export type { LeaseGrant, LeaseHandle, LeaseSpec, SecretSet }
```

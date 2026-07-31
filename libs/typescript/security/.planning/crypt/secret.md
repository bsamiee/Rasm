# [SECURITY_SECRET]

Leased-secret custody: one `DopplerSDK` client built behind a `Layer.scoped` admits a closed surface set — `secrets.download` (the leased env-set fetch), `secrets.get`/`secrets.list`/`secrets.names` (the targeted single-secret read, the full-object census a partial refresh diffs against custody, and the name-only enumeration), `dynamicSecrets.issueLease`/`revokeLease` (the explicit dynamic-lease lifecycle), and `auth.me`/`auth.revoke` (the boot liveness probe and the credential-rotation retirement). Projects/configs/integrations administration stays out of scope: a runtime folder that reached for it re-implements Doppler administration, which belongs to the deploy plane. TTL leasing is Doppler-side (`dynamicSecretsTtlSec`): the custodian refetches on a spaced cadence under the lease window, with the branch `Budget.schedule("lease")` compile re-driving a transient fault inside the tick under its own class gate and a per-call deadline bounding every SDK promise; an `effect` `Cache` collapses concurrent refetches of the one `(project, config)` coordinate to a single in-flight download. Rotation republishes through a serialized `SubscriptionRef` transition — custody state lands before its metric, fact, and log taps — and `changes` is the feed the composition root's `Reloadable.auto` row consumes to rebuild `Jwt.Default(keyset)` without a graph teardown. Every fetched value is `Redacted` from the first decode, the `DOPPLER_TOKEN` is a `Config.redacted`, and fetched key material leaves this page only as the core `Credential` landing — `credential` folds a named PEM/JWK value into the sealed carrier `crypt/sign`'s `Material.admit` terminates, so the folder has one admission path for wire-carried and fetched keys alike. `SecretFault` instantiates the folder fault shape over the core `FaultClass.family` seam: every `BaseHTTPError` status subclass folds to one reason family whose rows carry the core `FaultClass` kind.

## [01]-[INDEX]

- [02]-[SECRET_FAULT]: the `statusCode`-folded reason family over the problem-detail carrier; `SecretFault`.
- [03]-[LEASED_CUSTODY]: the encoded spec, scoped client, rotation feed, and lease lifecycle; `LeaseSpec`, `Secret`.
- [04]-[KEY_HANDOFF]: the fetched-material fold into the core `Credential` landing; `Secret`.

## [02]-[SECRET_FAULT]

[SECRET_FAULT]:
- Owner: `SecretFault` — the folder fault shape instantiated over the Doppler axis: `credential` (401/403 — the service token is dead), `missing` (404 or an absent name), `rateLimit` (429), `transient` (5xx, network throws, and a per-call deadline), `lease` (lease issue/revoke refused). Rows carry the core `FaultClass` kind; the refresh loop's inner retry and the compiled branch `Budget` schedules gate on `FaultClass.retryable`, so the `rateLimit`/`transient` arms re-drive and the `credential` arm never does.
- Law: the fold reads `statusCode` off the RFC 9457 `BaseHTTPError` carrier — every status subclass is seed data over one problem-detail shape, and an instance-of ladder over them is the rejected form; a throw without a `statusCode` is `transient`.
- Growth: a new failure class is one `_reasonOf` status arm and one class row.
- Packages: `@dopplerhq/node-sdk` (`BaseHTTPError` carrier); `effect` (`Schema`, `Predicate`); `@rasm/ts/core` (`FaultClass`).

```typescript
import DopplerSDK from "@dopplerhq/node-sdk"
import { Budget, Convention, Credential, FaultClass } from "@rasm/ts/core"
import { Cache, Cause, Config, DateTime, Duration, Effect, Equal, HashMap, Metric, Option, Predicate, Record, Redacted, Ref, Schedule, Schema, Stream, SubscriptionRef } from "effect"
import { SecurityFact, Witness } from "../access/audit.ts"
import { Crypto } from "./sign.ts"

type SecretSet = HashMap.HashMap<string, Redacted.Redacted<string>>
type LeaseGrant = Awaited<ReturnType<DopplerSDK["dynamicSecrets"]["issueLease"]>>
type LeaseHandle = Parameters<DopplerSDK["dynamicSecrets"]["revokeLease"]>[0]

const _renewals = ["rolling", "bounded"] as const
// The TTL crosses the wire as whole seconds (ttl_sec), so second alignment is admission: a
// sub-second duration would silently floor at the seconds boundary and desynchronize the remote
// lease, the cache expiry, the renewal cadence, and the bounded clear from the declared value.
const _LeaseTtl = Schema.DurationFromMillis.pipe(
  Schema.filter((ttl) => Duration.toMillis(ttl) >= 1000 && Duration.toMillis(ttl) % 1000 === 0, { identifier: "WholeSecondLeaseTtl" }),
)
const LeaseSpec = Schema.Struct({
  scope: Schema.NonEmptyString,
  keys: Schema.NonEmptyArray(Schema.NonEmptyString).pipe(
    Schema.filter((keys) => new Set(keys).size === keys.length, { identifier: "UniqueLeaseKeys" }),
  ),
  ttl: _LeaseTtl,
  renewal: Schema.Literal(..._renewals),
  epoch: Schema.NonEmptyString,
})

type LeaseSpec = typeof LeaseSpec.Type

const _family = FaultClass.family(["credential", "missing", "rateLimit", "transient", "lease"] as const, {
  credential: { class: "denied" },
  missing: { class: "absent" },
  rateLimit: { class: "exhausted" },
  transient: { class: "unavailable" },
  lease: { class: "invalid" },
})

declare namespace SecretFault {
  type Reason = (typeof _family.reasons)[number]
}

class SecretFault extends Schema.TaggedError<SecretFault>()("SecretFault", {
  reason: _family.schema,
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _family.classOf(this.reason)
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

## [03]-[LEASED_CUSTODY]

[LEASED_CUSTODY]:
- Owner: `LeaseSpec` — the encoded app-custody boundary: `scope` names the isolated custody cell, `keys` is its unique non-empty allowlist, `ttl` encodes as milliseconds and admits only whole seconds of at least one — the remote `ttl_sec`, cache expiry, renewal cadence, and bounded clear all read one exact second count — `renewal` is `rolling | bounded`, and `epoch` is the replacement identity. `Secret` is the `Layer.scoped` custodian holding one client, publishing the current set through a `SubscriptionRef`, renewing or expiring it by posture, and revoking every still-held lease in an `addFinalizer`. `get` reads the current cell; `probe` refreshes one admitted name; `census` refreshes the whole allowlist in one read; `names` enumerates membership alone; `changes` is the rotation feed; `lease` issues one admitted dynamic key and registers its revocable handle; `revoke` retires that handle after remote success; `retire` revokes a superseded service token.
- Boundary: the app root provides `SECURITY_LEASE_SPEC` as the encoded `LeaseSpec` and `DOPPLER_TOKEN` from one namespace custody cell, then composes `Secret.Default`. The deploy plane realizes each spec as a config containing only `keys`, a read-only service token scoped to that config, and one namespace secret; `scope + epoch` keys replacement, so the new token and cell land before the prior token retires. Security owns the value and renewal semantics; deployment owns the Doppler and Kubernetes resources.
- Law: two schedules are orthogonal and both explicit — the outer `Schedule.spaced` cadence paces refresh across ticks, and the inner `Effect.retry(Budget.schedule("lease"))` re-drives a transient fault inside the tick under the branch compile's jitter, attempt bound, quiet-reset, elapsed ceiling, and its own `FaultClass.retryable` gate, so a Doppler blip costs milliseconds, never a full stale interval; a tick whose retries exhaust keeps the last good set, and the cadence is this page's policy while the re-drive geometry is the floor's.
- Law: the download de-dupes — an `effect` `Cache` keyed by `(project, config, scope, epoch)` with a TTL below the refresh cadence collapses concurrent allowlisted reads to one in-flight request; targeted `probe` rejects names outside `LeaseSpec.keys`. Every SDK promise carries the per-call deadline and `_lift` hands `tryPromise`'s interruption-wired `AbortSignal` to its runner — the SDK transport is signal-blind, so the deadline bounds the caller as a typed `transient` while an orphaned read settles harmlessly — and the lease issue rides a shielded `disconnect` window, so a grant landing after its deadline still registers and teardown revokes it, never an orphaned lease.
- Law: the boot probe (`auth.me`) and the first fetch gate construction under that same `lease` budget — a transient boot blip re-drives, a dead token fails the layer, not the first read; the composition root wraps `Secret.Default` in `Layer.retry` under the branch boot budget.
- Law: a rotation is observed, never silent — the custody semaphore serializes only the full or targeted compare/set transition, and the `SubscriptionRef.changes` stream serially increments `Convention.instrument.securitySecretRotation`, publishes the `Rotation` fact through `Witness`, and logs the audit line after custody releases. `probe` enters the same revision fold, so consumers observe one ordered rotation stream regardless of refresh grain; a blocked tap cannot stop later custody transitions, and an interrupted tap never rolls custody back.
- Receipt: `SecretSet` — a missing key is a `SecretFault`, never `undefined`; every value is `Redacted`, so a log or error never carries plaintext.
- Law: refresh grain is a member, never a planner — `census` reads the whole allowlist in one call and enters the same revision fold `probe` and the rolling tick enter, so custody's own equality check is what decides whether a rotation event fires and a name whose bytes never moved costs no republish; a caller diffing `names` re-fetches values to learn membership changed, which is the round trip this member deletes. Dynamic leasing stays off every read path — an inline lease surfaces no handle, so only the explicit `lease` seam can register one for revocation.
- Law: generated response shapes are claims, never contracts — `SecretsListResponse` types its map as four fixed example key names, so every read on this page decodes the wire through `Schema` and no member is read off the declared shape.
- Growth: a new fetched secret is a new name in the same response; a new refresh grain is one member on this custodian, never a second custody service.
- Law: `rolling` refreshes at four-fifths of `ttl`; `bounded` performs no renewal and clears the cell at `ttl`, so a one-shot scope cannot retain material past its lease. Every epoch change replaces the deploy-side token and custody cell regardless of posture.
- Packages: `@dopplerhq/node-sdk` (`secrets.download`/`get`/`list`/`names`, `dynamicSecrets.issueLease`/`revokeLease`, `auth.me`/`revoke`); `effect` (`Cache`, `Schedule`, `SubscriptionRef`, `Metric`); `@rasm/ts/core` (`Budget`, `Convention`); `access/audit` (`Witness`, `SecurityFact`).

```typescript
const _rotation = Convention.mount(Convention.metric.securitySecretRotation)


class Secret extends Effect.Service<Secret>()("security/crypt/Secret", {
  scoped: Effect.gen(function* () {
    const leaseSpec = yield* Schema.Config("SECURITY_LEASE_SPEC", LeaseSpec)
    const token = yield* Config.redacted("DOPPLER_TOKEN")
    const project = yield* Config.string("DOPPLER_PROJECT")
    const config = yield* Config.string("DOPPLER_CONFIG")
    const ttl = Duration.toSeconds(leaseSpec.ttl) // exact by admission: LeaseSpec.ttl is second-aligned, so no flooring exists to drift
    const deadline = yield* Config.duration("DOPPLER_CALL_DEADLINE").pipe(Config.withDefault(Duration.seconds(10)))
    const sdk = new DopplerSDK({ accessToken: Redacted.value(token) })
    const leases = yield* Ref.make<ReadonlyArray<LeaseHandle>>([])
    // BOUNDARY ADAPTER: tryPromise wires fiber interruption into the AbortSignal handed to run.
    // The Doppler transport is signal-blind — the SDK owns its own node client and takes no per-call
    // signal — so the deadline bounds the CALLER: a hung call types as transient while the orphaned
    // read settles harmlessly in the background; the one state-mutating call (issueLease) rides its
    // own shielded window below so no landed grant escapes custody.
    const _lift = <A>(run: (signal: AbortSignal) => Promise<A>, reason?: SecretFault.Reason): Effect.Effect<A, SecretFault> =>
      Effect.tryPromise({ try: run, catch: (cause) => new SecretFault({ reason: reason ?? _reasonOf(cause), detail: String(cause) }) }).pipe(
        Effect.timeoutFail({ duration: deadline, onTimeout: () => new SecretFault({ reason: "transient", detail: "doppler deadline" }) }))
    const _download = _lift(() => sdk.secrets.download(project, config, {
      format: "json",
      includeDynamicSecrets: true,
      dynamicSecretsTtlSec: ttl,
      secrets: leaseSpec.keys.join(","),
    })).pipe(Effect.flatMap(_set))
    const deduped = yield* Cache.make({
      capacity: 1,
      timeToLive: Duration.seconds(Math.max(1, Math.floor(ttl * 0.4))),
      lookup: (_: string) => _download,
    })
    const fetch = deduped.get(`${project}/${config}/${leaseSpec.scope}/${leaseSpec.epoch}`)
    yield* _lift(() => sdk.auth.me()).pipe(Effect.retry(Budget.schedule("lease")))
    const cell = yield* SubscriptionRef.make(yield* fetch.pipe(Effect.retry(Budget.schedule("lease"))))
    const rotation = yield* Effect.makeSemaphore(1)
    const _publish = (revise: (prior: SecretSet) => SecretSet): Effect.Effect<void> =>
      rotation.withPermits(1)(
        Effect.flatMap(SubscriptionRef.get(cell), (prior) => {
          const next = revise(prior)
          return Equal.equals(prior, next)
            ? Effect.void
            : SubscriptionRef.set(cell, next)
        }),
      )
    const observed = Metric.increment(_rotation).pipe(
      Effect.zipRight(Witness.publish(SecurityFact.Rotation({ coordinate: `${project}/${config}/${leaseSpec.scope}` }))),
      Effect.zipRight(Effect.logInfo("secret rotation observed")),
    )
    yield* cell.changes.pipe(
      Stream.drop(1),
      Stream.runForEach(() => observed),
      Effect.forkScoped,
    )
    yield* Effect.forkScoped(leaseSpec.renewal === "rolling"
      ? Effect.repeat(
          fetch.pipe(
            Effect.retry(Budget.schedule("lease")),
            Effect.flatMap((set) => _publish(() => set)),
            // Discrimination runs INTERRUPT-FIRST: a scope teardown ends this loop cleanly while an exhausted
            // retry is custody that stopped refreshing, and a bare swallow reports the two identically — so a
            // permanently dead refresh reads exactly like a clean shutdown and every later `get` serves the
            // last set nothing renewed. The tick still survives its own failure; only the silence goes.
            Effect.tapErrorCause((cause) =>
              Cause.isInterruptedOnly(cause) ? Effect.void : Effect.logError("secret refresh exhausted", cause)),
            Effect.ignore,
          ),
          Schedule.spaced(Duration.seconds(Math.max(1, Math.floor(ttl * 0.8)))),
        )
      : Effect.sleep(leaseSpec.ttl).pipe(
          Effect.zipRight(_publish(() => HashMap.empty<string, Redacted.Redacted<string>>())),
        ))
    yield* Effect.addFinalizer(() =>
      Ref.get(leases).pipe(Effect.flatMap((held) =>
        Effect.forEach(held, (handle) => Effect.ignore(_lift(() => sdk.dynamicSecrets.revokeLease(handle), "lease")), { discard: true }))))
    const get = (name: string): Effect.Effect<Redacted.Redacted<string>, SecretFault> =>
      Effect.flatMap(SubscriptionRef.get(cell), (set) =>
        Option.match(HashMap.get(set, name), {
          onNone: () => Effect.fail(new SecretFault({ reason: "missing", detail: name })),
          onSome: Effect.succeed,
        }))
    const probe = (name: string): Effect.Effect<Redacted.Redacted<string>, SecretFault> =>
      leaseSpec.keys.includes(name)
        ? _lift(() => sdk.secrets.get(project, config, name)).pipe(
            Effect.flatMap((response) =>
              Schema.decodeUnknown(Schema.Struct({ value: Schema.Struct({ raw: Schema.String }) }))(response).pipe(
                Effect.mapError((cause) => new SecretFault({ reason: "missing", detail: String(cause) })))),
            Effect.map((decoded) => Redacted.make(decoded.value.raw)),
            Effect.tap((value) => _publish(HashMap.set(name, value))),
          )
        : Effect.fail(new SecretFault({ reason: "missing", detail: name }))
    // Census answers membership AND value for the WHOLE allowlist in one read, so its answer IS custody rather
    // than a delta over it: publication REPLACES, and a name Doppler stopped serving stops resolving through
    // `get` instead of surviving as a value nothing refreshes and no rotation event mentions. `_publish`'s
    // equality fold still suppresses the rotation event where nothing changed. `includeDynamicSecrets` stays
    // OFF: an inline lease surfaces no handle, so a census issuing one could never revoke it, and the dynamic
    // lifecycle keeps its explicit `lease` seam.
    const census = (): Effect.Effect<SecretSet, SecretFault> =>
      _lift(() => sdk.secrets.list(project, config, {
        includeDynamicSecrets: false,
        includeManagedSecrets: false,
        secrets: leaseSpec.keys.join(","),
      })).pipe(
        // Codegen fixed four example key names onto this response type, so its real name-keyed map decodes here
        // exactly as every other read on this page decodes rather than off a shape the SDK only claims
        Effect.flatMap((response) =>
          Schema.decodeUnknown(Schema.Struct({
            secrets: Schema.Record({ key: Schema.String, value: Schema.Struct({ raw: Schema.String }) }),
          }))(response).pipe(
            Effect.mapError((cause) => new SecretFault({ reason: "missing", detail: String(cause) })))),
        // `leaseSpec.keys` is the one positive admission boundary on BOTH faces — `probe` and `lease` already
        // refuse an unlisted name, so a response row beyond it never enters the set custody serves.
        Effect.map((decoded) =>
          Record.reduce(decoded.secrets, HashMap.empty<string, Redacted.Redacted<string>>(), (held, row, name) =>
            leaseSpec.keys.includes(name) ? HashMap.set(held, name, Redacted.make(row.raw)) : held)),
        Effect.tap((set) => _publish(() => set)),
      )
    const names = (): Effect.Effect<ReadonlyArray<string>, SecretFault> =>
      _lift(() => sdk.secrets.names(project, config)).pipe(
        Effect.flatMap((response) =>
          Schema.decodeUnknown(Schema.Struct({ names: Schema.Array(Schema.String) }))(response).pipe(
            Effect.mapError((cause) => new SecretFault({ reason: "missing", detail: String(cause) })))),
        Effect.map((decoded) => decoded.names),
      )
    const lease = (name: string, handle: (grant: LeaseGrant) => LeaseHandle): Effect.Effect<LeaseGrant, SecretFault> =>
      leaseSpec.keys.includes(name)
        ? Effect.tryPromise({
            try: () => sdk.dynamicSecrets.issueLease({ project, config, dynamic_secret: name, ttl_sec: ttl }),
            catch: (cause) => new SecretFault({ reason: "lease", detail: String(cause) }),
          }).pipe(
            Effect.tap((grant) => Ref.update(leases, (held) => [...held, handle(grant)])),
            // Grant-and-register is ONE shielded window severed onto its own fiber: the deadline
            // settles the caller on time while a late-landing grant still registers into the leases
            // cell, so the scope finalizer revokes it and no orphaned lease escapes custody.
            Effect.uninterruptible,
            Effect.disconnect,
            Effect.timeoutFail({ duration: deadline, onTimeout: () => new SecretFault({ reason: "transient", detail: "doppler deadline" }) }),
          )
        : Effect.fail(new SecretFault({ reason: "missing", detail: name }))
    const revoke = (handle: LeaseHandle): Effect.Effect<void, SecretFault> =>
      _lift(() => sdk.dynamicSecrets.revokeLease(handle), "lease").pipe(
        Effect.tap(() => Ref.update(leases, (held) => held.filter((leased) => leased !== handle))),
        Effect.asVoid,
      )
    const retire = (spent: Redacted.Redacted<string>): Effect.Effect<void, SecretFault> =>
      _lift(() => sdk.auth.revoke({ token: Redacted.value(spent) }), "credential").pipe(Effect.asVoid)
    return { get, probe, census, names, lease, revoke, retire, changes: cell.changes } as const
  }),
  accessors: true,
}) {}
```

## [04]-[KEY_HANDOFF]

[KEY_HANDOFF]:
- Owner: `credential` — the fold from a named fetched value into the core `Credential` landing: the PEM/JWK string stays `Redacted`, the `fingerprint` is `Crypto.fingerprint` over the sealed material, and the validity window spans the current instant to the configured rotation horizon. Its result is the exact carrier `crypt/sign`'s `Material.admit` terminates, so fetched and wire-carried keys share one admission path and `Credential.rotated` compares sealed across refreshes.
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

export { credential, LeaseSpec, Secret, SecretFault }
export type { LeaseGrant, LeaseHandle, SecretSet }
```

## [05]-[RESEARCH]

(none)

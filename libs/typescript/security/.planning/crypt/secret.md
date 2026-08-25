# [SECURITY_SECRET]

Leased-secret custody: one `DopplerSDK` client built behind a `Layer.scoped` admits a closed surface set — `secrets.download` (the leased env-set fetch), `secrets.get`/`secrets.list`/`secrets.names` (the targeted single-secret read, the full-object census a partial refresh diffs against custody, and the name-only enumeration), `dynamicSecrets.issueLease`/`revokeLease` (the explicit dynamic-lease lifecycle), and `auth.me`/`auth.revoke` (the boot liveness probe and the credential-rotation retirement). Projects/configs/integrations administration stays out of scope: a runtime folder that reached for it re-implements Doppler administration, which belongs to the deploy plane. TTL leasing is Doppler-side (`dynamicSecretsTtlSec`): the custodian refetches on a spaced cadence under the lease window, with the branch `Fault.Budget.schedule("lease")` compile re-driving a transient fault inside the tick under its own class gate and a per-call deadline bounding every SDK promise; an `effect` `Cache` collapses concurrent refetches of the one `(project, config)` coordinate to a single in-flight download. Rotation republishes through a serialized `SubscriptionRef` transition — custody state lands before its metric, fact, and log taps — and `changes` is the `Rotation` feed the composition root hands `Jwt.Default` beside the `credential` → `Material.ring` rebuild, so the authority swaps its live ring on each observed roll without a graph teardown. Every fetched value is `Redacted` from the first decode, the custody coordinates resolve at the boot line through the typed `Coordinate` contract — one described record naming each env key beside its proven injection source — and fetched key material leaves this page only as a `Material.Source.Held` mint — the host-side trust boundary `crypt/sign`'s `Material.admit` terminates beside the peer-attested `Credential` source, so the folder has one admission path for wire-carried and fetched keys alike. `SecretFault` instantiates the folder fault shape over the core `Fault.Class.family` seam: every `BaseHTTPError` status subclass folds to one reason family whose rows carry the core `Fault.Class` kind.

## [01]-[INDEX]

- [02]-[SECRET_FAULT]: the `statusCode`-folded reason family over the problem-detail carrier; `SecretFault`.
- [03]-[LEASED_CUSTODY]: the encoded spec, the coordinate contract, scoped client, rotation feed, and lease lifecycle; `LeaseSpec`, `Coordinate`, `Secret`.
- [04]-[KEY_HANDOFF]: the fetched-material mint of `Material.Source.Held`; `Secret`.

## [02]-[SECRET_FAULT]

[SECRET_FAULT]:
- Law: the fold reads `statusCode` off the RFC 9457 `BaseHTTPError` carrier — every status subclass is seed data over one problem-detail shape, and an instance-of ladder over them is the rejected form; a throw without a `statusCode` is `transient`.
- Growth: a new failure class is one `_reasonOf` status arm and one class row.
- Packages: `@dopplerhq/node-sdk` (`BaseHTTPError` carrier); `effect` (`Schema`, `Predicate`); `@rasm/core` (`Fault.Class`).

```typescript
import DopplerSDK from "@dopplerhq/node-sdk"
import { Fault, Convention } from "@rasm/core"
import { Cache, Cause, Config, DateTime, Duration, Effect, Equal, HashMap, Metric, Option, Predicate, Record, Redacted, Ref, Schedule, Schema, Stream, SubscriptionRef } from "effect"
import { SecurityFact, Witness } from "../access/audit.ts"
import { Crypto, Material } from "./sign.ts"

type SecretSet = HashMap.HashMap<string, Redacted.Redacted<string>>
type LeaseGrant = Awaited<ReturnType<DopplerSDK["dynamicSecrets"]["issueLease"]>>
type LeaseHandle = Parameters<DopplerSDK["dynamicSecrets"]["revokeLease"]>[0]

const _renewals = ["rolling", "bounded"] as const
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

const _family = Fault.Class.family(["credential", "missing", "rateLimit", "transient", "lease"] as const, {
  credential: Fault.Class.row({
    class: "denied",
    leg: "custody",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `custody token refused at ${coordinate}: ${cause}`,
  }),
  missing: Fault.Class.row({
    class: "absent",
    leg: "custody",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `${coordinate} names no secret this custody serves: ${cause}`,
  }),
  rateLimit: Fault.Class.row({
    class: "exhausted",
    leg: "custody",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `doppler throttled ${coordinate}: ${cause}`,
  }),
  transient: Fault.Class.row({
    class: "unavailable",
    leg: "custody",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `doppler unreachable at ${coordinate}: ${cause}`,
  }),
  lease: Fault.Class.row({
    class: "invalid",
    leg: "lease",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `dynamic lease refused at ${coordinate}: ${cause}`,
  }),
})

declare namespace SecretFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class SecretFault extends Schema.TaggedError<SecretFault>()("SecretFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
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

const _set = (coordinate: string) => (raw: unknown): Effect.Effect<SecretSet, SecretFault> =>
  _decode(raw).pipe(
    Effect.mapError((cause) => new SecretFault({ case: { reason: "missing", coordinate, cause: String(cause) } })),
    Effect.map((record) => HashMap.map(HashMap.fromIterable(Record.toEntries(record)), Redacted.make)),
  )
```

## [03]-[LEASED_CUSTODY]

[LEASED_CUSTODY]:
- Owner: `LeaseSpec` — the encoded app-custody boundary: `scope` names the isolated custody cell, `keys` is its unique non-empty allowlist, `ttl` encodes as milliseconds and admits only whole seconds of at least one — the remote `ttl_sec`, cache expiry, renewal cadence, and bounded clear all read one exact second count — `renewal` is `rolling | bounded`, and `epoch` is the replacement identity. `Secret` is the `Layer.scoped` custodian holding one client, publishing the current set through a `SubscriptionRef`, renewing or expiring it by posture, and revoking every still-held lease in an `addFinalizer`. `get` reads the current cell; `probe` refreshes one admitted name; `census` refreshes the whole allowlist in one read; `names` enumerates membership alone; `changes` is the rotation feed; `lease` issues one admitted dynamic key and registers its revocable handle; `revoke` retires that handle after remote success; `retire` revokes a superseded service token.
- Boundary: the app root provides `SECURITY_LEASE_SPEC` as the encoded `LeaseSpec` and `DOPPLER_TOKEN` from one namespace custody cell, then composes `Secret.Default`. The deploy plane realizes each spec as a config containing only `keys`, a read-only service token scoped to that config, and one namespace secret; `scope + epoch` keys replacement, so the new token and cell land before the prior token retires. Security owns the value and renewal semantics; deployment owns the Doppler and Kubernetes resources.
- Law: custody coordinates are the `Coordinate` contract — one row per env key beside its PROVEN injection source: the deploy plane mounts `DOPPLER_TOKEN` as the workload secret, and the `doppler run --` entrypoint injects `DOPPLER_PROJECT` and `DOPPLER_CONFIG` into the wrapped process, so the workload stamps no coordinate the entrypoint already owns and custodian boot cannot fail on a coordinate nobody stamped; the iac workload page names its injection source per coordinate off this table, never a deploy-side convention.
- Law: every custody row resolves at the boot line as one described record — `_custody` is the folder's one `DOPPLER_`/`SECURITY_LEASE_SPEC` decode site, so a malformed environment fails the layer and no second decode site can fork the namespace.
- Law: the download de-dupes — an `effect` `Cache` keyed by `(project, config, scope, epoch)` with a TTL below the refresh cadence collapses concurrent allowlisted reads to one in-flight request; targeted `probe` rejects names outside `LeaseSpec.keys`. Every SDK promise carries the per-call deadline and `_lift` hands `tryPromise`'s interruption-wired `AbortSignal` to its runner — the SDK transport is signal-blind, so the deadline bounds the caller as a typed `transient` while an orphaned read settles harmlessly — and the lease issue rides a shielded `disconnect` window, so a grant landing after its deadline still registers and teardown revokes it, never an orphaned lease.
- Law: the boot probe (`auth.me`) and the first fetch gate construction under that same `lease` budget — a transient boot blip re-drives, a dead token fails the layer, not the first read; the composition root wraps `Secret.Default` in `Layer.retry` under the same `lease` budget.
- Law: a rotation is observed, never silent — the custody semaphore serializes only the full or targeted compare/set transition, and the `SubscriptionRef.changes` stream serially increments `Convention.instrument.securitySecretRotation`, publishes the `Rotation` fact through `Witness`, and logs the audit line after custody releases. `probe` enters the same revision fold, so consumers observe one ordered rotation stream regardless of refresh grain; a blocked tap cannot stop later custody transitions, and an interrupted tap never rolls custody back.
- Receipt: `SecretSet` — a missing key is a `SecretFault`, never `undefined`; every value is `Redacted`, so a log or error never carries plaintext.
- Law: refresh grain is a member, never a planner — `census` reads the whole allowlist in one call and enters the same revision fold `probe` and the rolling tick enter, so custody's own equality check is what decides whether a rotation event fires and a name whose bytes never moved costs no republish; a caller diffing `names` re-fetches values to learn membership changed, which is the round trip this member deletes. Dynamic leasing stays off every read path — an inline lease surfaces no handle, so only the explicit `lease` seam can register one for revocation.
- Law: generated response shapes are claims, never contracts — `SecretsListResponse` types its map as four fixed example key names, so every read on this page decodes the wire through `Schema` and no member is read off the declared shape.
- Growth: a new fetched secret is a new name in the same response; a new refresh grain is one member on this custodian, never a second custody service.
- Law: `rolling` refreshes at four-fifths of `ttl`; `bounded` performs no renewal and clears the cell at `ttl`, so a one-shot scope cannot retain material past its lease. Every epoch change replaces the deploy-side token and custody cell regardless of posture.

```typescript
const _rotation = Convention.mount(Convention.metric.securitySecretRotation)

const Coordinate = {
  token: { key: "DOPPLER_TOKEN", source: "mounted" },
  project: { key: "DOPPLER_PROJECT", source: "injected" },
  config: { key: "DOPPLER_CONFIG", source: "injected" },
} as const

declare namespace Coordinate {
  type Kind = keyof typeof Coordinate
  type _Rows<T extends Record<string, { readonly key: string; readonly source: "mounted" | "injected" }> = typeof Coordinate> = T
}

const _custody = Config.unwrap({
  leaseSpec: Schema.Config("SECURITY_LEASE_SPEC", Schema.parseJson(LeaseSpec)).pipe(
    Config.withDescription("encoded LeaseSpec the app root supplies; scope, allowlist, ttl, renewal posture, epoch"),
  ),
  token: Config.redacted(Coordinate.token.key).pipe(
    Config.withDescription("read-only service token the deploy plane mounts as the workload secret; sealed Redacted"),
  ),
  project: Config.string(Coordinate.project.key).pipe(
    Config.withDescription("doppler project the run entrypoint injects into the wrapped process"),
  ),
  config: Config.string(Coordinate.config.key).pipe(
    Config.withDescription("doppler config the run entrypoint injects into the wrapped process"),
  ),
  deadline: Config.duration("DOPPLER_CALL_DEADLINE").pipe(
    Config.withDefault(Duration.seconds(10)),
    Config.withDescription("per-call SDK deadline bounding every custody read"),
  ),
})

class Secret extends Effect.Service<Secret>()("security/crypt/Secret", {
  scoped: Effect.gen(function* () {
    const { config, deadline, leaseSpec, project, token } = yield* _custody
    const ttl = Duration.toSeconds(leaseSpec.ttl)
    const sdk = new DopplerSDK({ accessToken: Redacted.value(token) })
    const leases = yield* Ref.make<ReadonlyArray<LeaseHandle>>([])
    const coordinate = `${project}/${config}/${leaseSpec.scope}`
    const _lift = <A>(run: (signal: AbortSignal) => Promise<A>, reason?: SecretFault.Reason): Effect.Effect<A, SecretFault> =>
      Effect.tryPromise({
        try: run,
        catch: (cause) => new SecretFault({ case: { reason: reason ?? _reasonOf(cause), coordinate, cause: String(cause) } }),
      }).pipe(
        Effect.timeoutFail({
          duration: deadline,
          onTimeout: () => new SecretFault({ case: { reason: "transient", coordinate, cause: "per-call deadline spent" } }),
        }))
    const _download = _lift(() => sdk.secrets.download(project, config, {
      format: "json",
      includeDynamicSecrets: true,
      dynamicSecretsTtlSec: ttl,
      secrets: leaseSpec.keys.join(","),
    })).pipe(Effect.flatMap(_set(coordinate)))
    const deduped = yield* Cache.make({
      capacity: 1,
      timeToLive: Duration.seconds(Math.max(1, Math.floor(ttl * 0.4))),
      lookup: (_: string) => _download,
    })
    const fetch = deduped.get(`${project}/${config}/${leaseSpec.scope}/${leaseSpec.epoch}`)
    yield* _lift(() => sdk.auth.me()).pipe(Effect.retry(Fault.Budget.schedule("lease")))
    const cell = yield* SubscriptionRef.make(yield* fetch.pipe(Effect.retry(Fault.Budget.schedule("lease"))))
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
      Effect.zipRight(Witness.publish(SecurityFact.Rotation({ coordinate }))),
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
            Effect.retry(Fault.Budget.schedule("lease")),
            Effect.flatMap((set) => _publish(() => set)),
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
          onNone: () => Effect.fail(new SecretFault({ case: { reason: "missing", coordinate: `${coordinate}/${name}`, cause: "absent from the held custody cell" } })),
          onSome: Effect.succeed,
        }))
    const probe = (name: string): Effect.Effect<Redacted.Redacted<string>, SecretFault> =>
      leaseSpec.keys.includes(name)
        ? _lift(() => sdk.secrets.get(project, config, name)).pipe(
            Effect.flatMap((response) =>
              Schema.decodeUnknown(Schema.Struct({ value: Schema.Struct({ raw: Schema.String }) }))(response).pipe(
                Effect.mapError((cause) => new SecretFault({ case: { reason: "missing", coordinate: `${coordinate}/${name}`, cause: String(cause) } })))),
            Effect.map((decoded) => Redacted.make(decoded.value.raw)),
            Effect.tap((value) => _publish(HashMap.set(name, value))),
          )
        : Effect.fail(new SecretFault({ case: { reason: "missing", coordinate: `${coordinate}/${name}`, cause: "outside the lease allowlist" } }))
    const census = (): Effect.Effect<SecretSet, SecretFault> =>
      _lift(() => sdk.secrets.list(project, config, {
        includeDynamicSecrets: false,
        includeManagedSecrets: false,
        secrets: leaseSpec.keys.join(","),
      })).pipe(
        Effect.flatMap((response) =>
          Schema.decodeUnknown(Schema.Struct({
            secrets: Schema.Record({ key: Schema.String, value: Schema.Struct({ raw: Schema.String }) }),
          }))(response).pipe(
            Effect.mapError((cause) => new SecretFault({ case: { reason: "missing", coordinate, cause: String(cause) } })))),
        Effect.map((decoded) =>
          Record.reduce(decoded.secrets, HashMap.empty<string, Redacted.Redacted<string>>(), (held, row, name) =>
            leaseSpec.keys.includes(name) ? HashMap.set(held, name, Redacted.make(row.raw)) : held)),
        Effect.tap((set) => _publish(() => set)),
      )
    const names = (): Effect.Effect<ReadonlyArray<string>, SecretFault> =>
      _lift(() => sdk.secrets.names(project, config)).pipe(
        Effect.flatMap((response) =>
          Schema.decodeUnknown(Schema.Struct({ names: Schema.Array(Schema.String) }))(response).pipe(
            Effect.mapError((cause) => new SecretFault({ case: { reason: "missing", coordinate, cause: String(cause) } })))),
        Effect.map((decoded) => decoded.names),
      )
    const lease = (name: string, handle: (grant: LeaseGrant) => LeaseHandle): Effect.Effect<LeaseGrant, SecretFault> =>
      leaseSpec.keys.includes(name)
        ? Effect.tryPromise({
            try: () => sdk.dynamicSecrets.issueLease({ project, config, dynamic_secret: name, ttl_sec: ttl }),
            catch: (cause) => new SecretFault({ case: { reason: "lease", coordinate: `${coordinate}/${name}`, cause: String(cause) } }),
          }).pipe(
            Effect.tap((grant) => Ref.update(leases, (held) => [...held, handle(grant)])),
            Effect.uninterruptible,
            Effect.disconnect,
            Effect.timeoutFail({
              duration: deadline,
              onTimeout: () => new SecretFault({ case: { reason: "transient", coordinate: `${coordinate}/${name}`, cause: "per-call deadline spent" } }),
            }),
          )
        : Effect.fail(new SecretFault({ case: { reason: "missing", coordinate: `${coordinate}/${name}`, cause: "outside the lease allowlist" } }))
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
- Owner: `credential` — the mint of this folder's own host-side source: the fetched PEM/JWK string stays `Redacted` inside a `Material.Source.Held`, the `fingerprint` is `Crypto.fingerprint` over the sealed material, and the validity window spans the current instant to the configured rotation horizon. The core `Credential` landing is the peer-attested public carrier and holds no material column, so fetched keys enter admission through the `Held` trust boundary while wire-carried public identity enters through `Attested` — one `Material.admit`, two sources.
- Law: this page never imports jose and never inspects key structure — importer selection off the label vocabulary, handle derivation, and thumbprint identity are `crypt/sign`'s admission concerns; the custodian only seals, fingerprints, and windows.
- Law: signing keys, webhook HMAC secrets, and the argon2 pepper are sourced here and injected into the `crypt` layers at construction — no sibling talks to Doppler directly.
- Law: the rotation loop is sealed end to end — `Secret.changes` is the `Rotation` feed the root hands `Jwt.Default` beside the `credential` → `Material.ring` rebuild, and the `Jwt` authority swaps its live ring on each observed roll, so a Doppler key roll lands as a live ring swap with no graph teardown and no restart.
- Growth: a per-name rotation horizon is one config row; a new trust boundary is a `Material.Source` case at its `crypt/sign` owner, never a column here.
- Boundary: this page produces sources and never holds a `CryptoKey`; the composition root owns the `Rotation` wiring that hands this feed to the `Jwt` authority.
- Packages: `crypt/sign` (`Material`, `Crypto.fingerprint`).

```typescript
const credential = (
  name: string,
  horizon: Duration.DurationInput,
): Effect.Effect<Material.Source, SecretFault, Crypto | Secret> =>
  Effect.gen(function* () {
    const cipher = yield* Crypto
    const custody = yield* Secret
    const bundle = yield* custody.get(name)
    const now = yield* DateTime.now
    return Material.Source.Held({
      bundle,
      fingerprint: cipher.fingerprint(bundle),
      notBefore: now,
      notAfter: DateTime.addDuration(now, Duration.decode(horizon)),
    })
  })

// --- [EXPORTS] -------------------------------------------------------------------------

export { Coordinate, credential, LeaseSpec, Secret, SecretFault }
export type { LeaseGrant, LeaseHandle, SecretSet }
```

## [05]-[RESEARCH]

(none)

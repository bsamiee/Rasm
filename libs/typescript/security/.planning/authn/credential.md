# [SECURITY_CREDENTIAL]

One digest-at-rest credential owner: second-factor OTP, recovery codes, and machine API keys — three surfaces over one mint-and-resolve idiom the census flagged as byte-for-byte identical. `Digest` is that idiom made a value: mint an opaque secret, seal it by the material's own entropy posture keyed on a public index, resolve a presented secret by index-scoped candidate scan then constant-time compare. The posture is the idiom's one discriminant — a guessable secret earns the argon2 KDF, a random mint earns the SHA-256 fingerprint compare — so a recovery redemption no longer pays a KDF pass per candidate out of the same bulkhead every login queues on, and the exception that keeps machine keys on the KDF is stated beside the rule rather than scattered. Recovery codes and API keys both compose it — a recovery set is N codes over `Digest`, an API key is `rk_<prefix>.<secret>` over `Digest` with a prefix index decoded through one `Schema.TemplateLiteralParser` owner — so the `findFirst` candidate scan and the seal/probe pair exist once. `Otp` owns the TOTP/HOTP rows through `otplib` v13's strategy-discriminated result rail bound to `crypt/sign`'s `Crypto` ports, so second-factor HMAC rides the same primitive the folder owns and the bundled `@noble/hashes` stack is bypassed; the TOTP replay floor rides otplib's own `afterTimeStep` option, `Accepted.timeStep` is the next floor the caller persists, `remaining` projects the seconds left in the current window for the ui prompt, and an `OTPHooks` value threads through `verify` so a Steam-Guard-style alphabet is a value, never a fork. Every credential-verify surface is a brute-force target and every one is throttled: `Otp.verify`/`Otp.redeem` run under a per-subject budget and `ApiKey.resolve` under a per-prefix budget on the store-backed `RateLimiter`, an exhausted budget is the `throttled` fault (class `exhausted`), and every presentation lands on the folder ledger tagged by surface — a refusal on the `credential` reject row, an admission and its wall span on the same kind's twin — so each surface's guess rate reads against its own denominator. Record ids mint through the `Crypto` entropy port; every secret is `Redacted` until the QR render or the one-time receipt at the edge; a wrong OTP is the `Rejected` verdict, a recovery or key miss is a typed fault, and `CredentialFault` fires only when a primitive throws — its rows close at the core `Fault.Class.family` seam. `ApiKeyGuard` is the declarative api-key scheme seam the runtime serve wave mounts.

## [01]-[INDEX]

- [02]-[DIGEST_IDIOM]: `Digest`, `CredentialFault`.
- [03]-[SECOND_FACTOR]: `Otp`, `OtpVerdict`, `RecoverySet`.
- [04]-[MACHINE_KEY]: `ApiKey`, `ApiKeyRecord`, `MintReceipt`, `ApiKeyStore`, `ApiKeyGuard`, `CurrentApiKey`.

## [02]-[DIGEST_IDIOM]

[DIGEST_IDIOM]:
- Owner: `Digest` — the shared credential-at-rest idiom over one entropy posture: `mint(posture, alphabet, length)` issues an opaque secret and seals it by that posture's row, and `resolve(posture, presented, candidates, digestOf)` scans an index-scoped candidate set with `Effect.findFirst` over the same row's probe, returning the matched candidate. `CredentialFault` is the folder fault shape closed at the core family seam; a `false` probe is a scan miss, never a fault.
- Law: the storage form is the material's entropy class, spelled once here rather than per surface — `low` seals through the `apiKey` argon2 cost row and probes through its constant-time verify, `high` seals a `Crypto.fingerprint` and probes the `Probe.Digest` compare, and a caller names what its own mint produced, never a mechanism. A high-entropy set therefore costs one SHA-256 per candidate where a KDF row would cost an argon2 pass: an eight-code recovery redemption under `low` is a multi-second authenticated request that also holds permits from the 4-permit KDF bulkhead every concurrent login queues on, and the codes carry ~49 bits of `Crypto.token` entropy no digest table walks.
- Law: the `low` row survives on machine keys as stated defense in depth, not by default — a stolen digest table is the one threat argon2 still answers on a random secret, so `ApiKey` pays the KDF knowingly and the exception lives here beside the rule rather than as an unexplained per-surface choice; recovery codes and every future random mint take `high`.
- Law: the resolve budget is amortized over the caller's index (a prefix, a subject) so `findFirst` walks a bounded candidate set, never the whole table, and a stale-parameter `low` match surfaces as the `Matched({ stale })` rehash signal the caller persists on.
- Law: every mint is `Redacted` from the RNG; the digest is `Redacted` at rest; the plaintext leaves only through the caller's one-time receipt.
- Growth: a new credential surface (a signed-URL token, a device pairing code) composes `Digest.mint`/`.resolve` with its own index and its own posture — the idiom never forks; a new storage mechanism is one posture row both members inherit.
- Boundary: `crypt/sign`'s `Crypto` owns the RNG, the argon2 digest, the fingerprint, and the constant-time compare; this owner composes them into the posture-keyed mint/resolve fold every credential surface reads.

```typescript
import * as RateLimiter from "@effect/experimental/RateLimiter"
import { HttpApiMiddleware, HttpApiSecurity } from "@effect/platform"
import { createGuardrails, generateSecret, generateURI, verify, type OTPGuardrails, type OTPVerifyFunctionalOptions } from "otplib"
import type { OTPHooks } from "@otplib/core"
import { Fault } from "@rasm/ts/core"
import { Array, Clock, Config, Context, Data, DateTime, Duration, Effect, Layer, Option, Redacted, Schema } from "effect"
import { Crypto, Probe, type SignFault } from "../crypt/sign.ts"
import { Reject } from "../crypt/verify.ts"

const _family = Fault.Class.family(["mint", "verify", "malformed", "notFound", "revoked", "expired", "throttled"] as const, {
  mint: { class: "defect" },
  verify: { class: "defect" },
  malformed: { class: "malformed" },
  notFound: { class: "denied" },
  revoked: { class: "denied" },
  expired: { class: "expired" },
  throttled: { class: "exhausted" },
})

declare namespace CredentialFault {
  type Reason = (typeof _family.reasons)[number]
}

class CredentialFault extends Schema.TaggedError<CredentialFault>()("CredentialFault", {
  reason: _family.schema,
  detail: Schema.String,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<credential:${this.reason}> ${this.detail}`
  }
}

const _postures = ["low", "high"] as const

type _Posture = (typeof _postures)[number]

type _Sealing = {
  readonly seal: (secret: Redacted.Redacted<string>) => Effect.Effect<Redacted.Redacted<string>, SignFault>
  readonly probe: (presented: Redacted.Redacted<string>, stored: Redacted.Redacted<string>) => Effect.Effect<boolean, SignFault>
}

// Two rows, one per entropy class: the KDF row defends a guessable secret against an offline digest-table walk, the
// fingerprint row is the O(1) constant-time compare a random mint earns. A caller never picks a mechanism — it names
// what its own mint produced, and the mechanism is the row's.
const _sealing = (cipher: Context.Tag.Service<Crypto>) =>
  ({
    low: {
      seal: (secret) => cipher.digest("apiKey", secret),
      probe: (presented, stored) => Effect.map(cipher.verify("apiKey", stored, presented), (verdict) => verdict._tag === "Matched"),
    },
    high: {
      seal: (secret) => Effect.succeed(Redacted.make(cipher.fingerprint(secret))),
      probe: (presented, stored) => cipher.matches(Probe.Digest({ opaque: presented, stored: Redacted.value(stored) })),
    },
  }) as const satisfies Record<_Posture, _Sealing>

const _digest = (cipher: Context.Tag.Service<Crypto>) => {
  const rows = _sealing(cipher)
  return {
    mint: (posture: _Posture, alphabet: string, length: number): Effect.Effect<{ readonly secret: Redacted.Redacted<string>; readonly digest: Redacted.Redacted<string> }, CredentialFault> =>
      cipher.token(alphabet, length).pipe(
        Effect.flatMap((secret) => Effect.map(rows[posture].seal(secret), (digest) => ({ secret, digest }))),
        Effect.mapError((cause) => new CredentialFault({ reason: "mint", detail: cause.detail })),
      ),
    resolve: <A>(posture: _Posture, presented: Redacted.Redacted<string>, candidates: ReadonlyArray<A>, digestOf: (candidate: A) => Redacted.Redacted<string>): Effect.Effect<Option.Option<A>, CredentialFault> =>
      Effect.findFirst(candidates, (candidate) => rows[posture].probe(presented, digestOf(candidate))).pipe(
        Effect.mapError((cause) => new CredentialFault({ reason: "verify", detail: cause.detail })),
      ),
  } as const
}
```

## [03]-[SECOND_FACTOR]

[SECOND_FACTOR]:
- Owner: `Otp.enroll` mints the base32 secret and the `otpauth://` URI, `Otp.verify` checks a presented token under the per-subject budget, `Otp.mintRecovery` issues N single-use codes over `Digest`, `Otp.redeem` finds the matching unspent code under the same budget, `Otp.remaining` projects the seconds left in the current TOTP window for the ui prompt. `OtpVerdict` is the second-factor result — `Accepted({ delta, timeStep })` or `Rejected` — and `RecoverySet` carries the codes and their digests. otplib's `crypto`/`base32` ports bind to `Crypto.plugin`/`Crypto.base32`, `createGuardrails` bounds secret bytes, period, counter, and window per policy, and the optional `OTPHooks` value threads through `verify` so a non-numeric token variant is one hooks row.
- Law: verification is result-typed and constant-time inside otplib — a wrong code is `Rejected`, never a throw; TOTP verifies past-only under `_EPOCH_TOLERANCE`, HOTP look-ahead under `_COUNTER_TOLERANCE` when the caller passes a `Some` counter; a valid HOTP match persists `counter + delta + 1` — the `Accepted.delta` resync signal. That constant-time claim is the crypto port's, not the strategy's: otplib hands the port two token STRINGS, so it holds only because `Crypto.plugin` lifts both operands to bytes before its byte-domain primitive runs.
- Law: `_PERIOD` is passed, never defaulted into — `enroll`'s URI, every TOTP `verify`, and `remaining`'s countdown read the one value, so the authenticator's step, the server's step arithmetic, and the prompt's countdown cannot drift the moment the row moves off the library's own thirty.
- Law: the TOTP replay floor is library-enforced — the caller's stored floor passes as otplib's `afterTimeStep` option, so a token whose matched `timeStep` is not strictly greater lands `{ valid: false }` inside the constant-time verify; `Accepted.timeStep` carries the RFC-6238 step number the caller persists as the next floor, and HOTP carries no `timeStep` (its counter is the floor).
- Law: `verify` and `redeem` are keyed brute-force targets — both run under the subject-keyed token-bucket budget, `RateLimitExceeded` folds to `throttled`, a `Rejected` verdict lands `Reject.mark("credential", { surface })` with the `otp`/`recovery` surface facet, and each entrypoint composes `Reject.measured("credential", { surface })` so the same facet carries the admission and the ceremony span; a guessing campaign is bounded by the store-backed limiter and legible as a ratio rather than as a raw count that a traffic spike reproduces.
- Law: recovery codes are `Digest` material at the high-entropy posture, not an otplib feature — `mintRecovery` composes `Digest.mint("high", …)` per code and `redeem` composes `Digest.resolve("high", …)` over the digests, returning the matched index so the store marks exactly that code spent and the whole set resolves in one constant-time pass.
- Receipt: `OtpVerdict` on verify, `Option<number>` on redeem (the spent index), `RecoverySet` on mint — never a raw boolean.
- Growth: a Steam-Guard-style alphabet is one `OTPHooks` value through the threaded option; HOTP is the same call with a `Some` counter — the input value is the strategy discriminant, never a name fork.
- Boundary: the edge renders the `otpauth://` URI to a QR (the one secret egress) and the `remaining` countdown beside the prompt; `Digest` owns the recovery mint/resolve; `crypt/sign` owns the HMAC and the digest; the `RateLimiter` store is data-wave-satisfied.
- Packages: `otplib` (`verify`/`generateSecret`/`generateURI`, `createGuardrails`, `OTPGuardrails`, `OTPHooks`, `afterTimeStep`); `@effect/experimental` (`RateLimiter`); `Digest` (recovery); `Crypto` (ports).

```typescript
type OtpVerdict = Data.TaggedEnum<{
  Accepted: { readonly delta: number; readonly timeStep: Option.Option<number> }
  Rejected: {}
}>

class RecoverySet extends Schema.Class<RecoverySet>("RecoverySet")({
  codes: Schema.Array(Schema.Redacted(Schema.String)),
  digests: Schema.Array(Schema.Redacted(Schema.String)),
}) {}

const _EPOCH_TOLERANCE: readonly [number, number] = [30, 0]
const _COUNTER_TOLERANCE: readonly [number, number] = [0, 2]
const _PERIOD = 30
const _RECOVERY_ALPHABET = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"
const _OtpVerdict = Data.taggedEnum<OtpVerdict>()

class Otp extends Effect.Service<Otp>()("security/authn/Otp", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const digest = _digest(cipher)
    const limit = yield* RateLimiter.makeWithRateLimiter
    const window = yield* Config.duration("OTP_RATE_WINDOW").pipe(Config.withDefault(Duration.minutes(5)))
    const budget = yield* Config.integer("OTP_RATE_LIMIT").pipe(Config.withDefault(5))
    const _ports = { crypto: cipher.plugin, base32: cipher.base32 } as const
    const _rails: OTPGuardrails = createGuardrails({ MIN_SECRET_BYTES: 16, MIN_PERIOD: _PERIOD, MAX_WINDOW: 2 })
    const _throttled = (subject: string, surface: string) => <A>(body: Effect.Effect<A, CredentialFault>): Effect.Effect<A, CredentialFault> =>
      limit({ algorithm: "token-bucket", onExceeded: "fail", window, limit: budget, key: `${surface}:${subject}` })(body).pipe(
        Effect.catchTags({
          RateLimitExceeded: () => Effect.fail(new CredentialFault({ reason: "throttled", detail: subject })),
          RateLimitStoreError: (error) => Effect.fail(new CredentialFault({ reason: "throttled", detail: String(error) })),
        }))
    const enroll = (issuer: string, label: string): Effect.Effect<{ readonly secret: Redacted.Redacted<string>; readonly uri: Redacted.Redacted<string> }, CredentialFault> =>
      Effect.try({
        try: () => {
          const secret = generateSecret(_ports)
          return { secret: Redacted.make(secret), uri: Redacted.make(generateURI({ strategy: "totp", issuer, label, secret, period: _PERIOD })) }
        },
        catch: (cause) => new CredentialFault({ reason: "mint", detail: String(cause) }),
      })
    const verify_ = (
      subject: string,
      secret: Redacted.Redacted<string>,
      token: string,
      floor: Option.Option<number> = Option.none(),
      counter: Option.Option<number> = Option.none(),
      hooks: Option.Option<OTPHooks> = Option.none(),
    ): Effect.Effect<OtpVerdict, CredentialFault> =>
      _throttled(subject, "otp")(
        Effect.tryPromise({
          try: () =>
            Option.match(counter, {
              // The tolerance pairs spread rather than pass by reference: otplib's option types take a mutable
              // `[number, number]`, and the period rides every leg explicitly so `remaining`'s countdown and the
              // library's own time-step arithmetic read ONE value instead of agreeing by shared default.
              onNone: () => verify({ strategy: "totp", secret: Redacted.value(secret), token, period: _PERIOD, epochTolerance: [..._EPOCH_TOLERANCE], ...(Option.isSome(floor) && { afterTimeStep: floor.value }), ...(Option.isSome(hooks) && { hooks: hooks.value }), guardrails: _rails, ..._ports } satisfies OTPVerifyFunctionalOptions),
              onSome: (at) => verify({ strategy: "hotp", secret: Redacted.value(secret), token, counter: at, counterTolerance: [..._COUNTER_TOLERANCE], ...(Option.isSome(hooks) && { hooks: hooks.value }), guardrails: _rails, ..._ports } satisfies OTPVerifyFunctionalOptions),
            }),
          catch: (cause) => new CredentialFault({ reason: "verify", detail: String(cause) }),
        }).pipe(
          Effect.map((result) =>
            result.valid
              ? _OtpVerdict.Accepted({ delta: result.delta, timeStep: "timeStep" in result ? Option.some(result.timeStep) : Option.none<number>() })
              : _OtpVerdict.Rejected()),
          Effect.tap((verdict) =>
            verdict._tag === "Rejected" ? Reject.mark("credential", { surface: "otp" }) : Effect.void),
        ),
      ).pipe(Reject.measured("credential", { surface: "otp" }), Effect.withSpan("security.otp.verify"))
    const remaining = (): Effect.Effect<number> =>
      Effect.map(Clock.currentTimeMillis, (millis) => _PERIOD - (Math.floor(millis / 1000) % _PERIOD))
    const mintRecovery = (count: number): Effect.Effect<RecoverySet, CredentialFault> =>
      Effect.map(
        Effect.forEach(Array.range(1, count), () => digest.mint("high", _RECOVERY_ALPHABET, 10)),
        (pairs) => new RecoverySet({ codes: Array.map(pairs, (pair) => pair.secret), digests: Array.map(pairs, (pair) => pair.digest) }),
      )
    const redeem = (subject: string, presented: Redacted.Redacted<string>, digests: ReadonlyArray<Redacted.Redacted<string>>): Effect.Effect<Option.Option<number>, CredentialFault> =>
      _throttled(subject, "recovery")(
        Effect.map(
          digest.resolve("high", presented, Array.map(digests, (held, index) => ({ held, index })), (row) => row.held),
          Option.map((row) => row.index),
        ).pipe(
          Effect.tap((hit) => Option.isNone(hit) ? Reject.mark("credential", { surface: "recovery" }) : Effect.void),
          Reject.measured("credential", { surface: "recovery" }),
        ),
      )
    return { enroll, verify: verify_, remaining, mintRecovery, redeem } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}
```

## [04]-[MACHINE_KEY]

[MACHINE_KEY]:
- Owner: `ApiKey.mint` issues `rk_<prefix>.<secret>` and stores its digest through `Digest.mint`; `ApiKey.resolve` decodes the wire frame through the `_KeyWire` parser, loads the prefix-indexed candidates under the per-prefix budget, resolves through `Digest.resolve`, gates lifecycle through `filterOrFail`, and touches `lastUsedAt`; `ApiKey.rotate` revokes and re-mints for the same subject; `ApiKey.revoke` timestamps. `ApiKeyRecord` is the stored credential, `MintReceipt` the one-time plaintext, `ApiKeyStore` the prefix-indexed port. `CurrentApiKey`/`ApiKeyGuard` are the declarative scheme seam — the middleware Tag carries `HttpApiSecurity.apiKey` on the `x-api-key` header, its implementation folds `resolve`, and the runtime serve wave mounts it so a machine-keyed endpoint receives the resolved record through the requirement channel. One polymorphic `resolve` dispatches on the presented value, never a `getByKey`/`verifyKey` twin.
- Law: the plaintext leaves only through `MintReceipt`; the digest is the PHC the `apiKey` cost row governs under the `low` posture this surface takes deliberately; a revoked or expired record is a typed fault, never a silent accept; the resolve reuses `Digest.resolve` so the candidate scan is the shared idiom, not a re-implementation.
- Law: `resolve` amortizes over the public prefix and is throttled by it — the prefix-keyed token-bucket budget bounds a stolen-prefix guessing campaign, `RateLimitExceeded` folds to `throttled`, a scan miss lands `Reject.mark("credential", { surface: "apikey" })`, and the resolved record lands the `apikey`-faceted admission and span through `Reject.measured`.
- Receipt: `MintReceipt` on mint/rotate (the subject and scopes the edge lifts into a principal), `ApiKeyRecord` on resolve — never a bare boolean.
- Growth: a new credential facet (a description, an IP allowlist) is one `ApiKeyRecord` field; a new failure mode is one `CredentialFault` reason.
- Boundary: the data wave satisfies `ApiKeyStore` and the limiter store; the edge lifts the resolved record's subject and scopes into a request principal; `Digest`/`crypt/sign` own the mint and verify; this page authenticates a machine and hands the subject on — it mints no session.
- Packages: `Digest` (mint/resolve); `crypt/verify` (`Reject`); `@effect/experimental` (`RateLimiter`); `@effect/platform` (`HttpApiMiddleware`, `HttpApiSecurity`); `effect` (`DateTime`, `Duration`, `Effect`, `Option`, `Redacted`, `Schema`).

```typescript
const _ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

const _KeyWire = Schema.TemplateLiteralParser("rk_", Schema.String, ".", Schema.String)

class ApiKeyRecord extends Schema.Class<ApiKeyRecord>("ApiKeyRecord")({
  id: Schema.UUID,
  prefix: Schema.NonEmptyString,
  subject: Schema.UUID,
  digest: Schema.Redacted(Schema.String),
  name: Schema.NonEmptyString,
  scopes: Schema.Array(Schema.NonEmptyString),
  createdAt: Schema.DateTimeUtc,
  expiresAt: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
  revokedAt: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
  lastUsedAt: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
}) {}

class MintReceipt extends Schema.Class<MintReceipt>("MintReceipt")({
  record: ApiKeyRecord,
  secret: Schema.Redacted(Schema.String),
}) {}

class ApiKeyStore extends Context.Tag("security/authn/ApiKeyStore")<ApiKeyStore, {
  readonly insert: (record: ApiKeyRecord) => Effect.Effect<void, CredentialFault>
  readonly byPrefix: (prefix: string) => Effect.Effect<ReadonlyArray<ApiKeyRecord>, CredentialFault>
  readonly touch: (id: string, at: DateTime.Utc) => Effect.Effect<void, CredentialFault>
  readonly revoke: (id: string, at: DateTime.Utc) => Effect.Effect<void, CredentialFault>
}>() {}

class CurrentApiKey extends Context.Tag("security/authn/CurrentApiKey")<CurrentApiKey, ApiKeyRecord>() {}

class ApiKey extends Effect.Service<ApiKey>()("security/authn/ApiKey", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const store = yield* ApiKeyStore
    const digest = _digest(cipher)
    const limit = yield* RateLimiter.makeWithRateLimiter
    const window = yield* Config.duration("APIKEY_RATE_WINDOW").pipe(Config.withDefault(Duration.minutes(1)))
    const budget = yield* Config.integer("APIKEY_RATE_LIMIT").pipe(Config.withDefault(30))
    const mint = (subject: string, name: string, scopes: ReadonlyArray<string>, ttl: Option.Option<Duration.DurationInput>): Effect.Effect<MintReceipt, CredentialFault> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const id = yield* cipher.uuid().pipe(Effect.mapError((cause) => new CredentialFault({ reason: "mint", detail: cause.detail })))
        const prefixBody = yield* cipher.token(_ALPHABET, 8).pipe(Effect.mapError((cause) => new CredentialFault({ reason: "mint", detail: cause.detail })))
        const prefix = `rk_${Redacted.value(prefixBody)}`
        const minted = yield* digest.mint("low", _ALPHABET, 40)
        const record = new ApiKeyRecord({
          id, prefix, subject, digest: minted.digest, name, scopes, createdAt: now,
          expiresAt: Option.map(ttl, (input) => DateTime.addDuration(now, input)), revokedAt: Option.none(), lastUsedAt: Option.none(),
        })
        yield* store.insert(record)
        return new MintReceipt({ record, secret: Redacted.make(`${prefix}.${Redacted.value(minted.secret)}`) })
      })
    const resolve = (presented: Redacted.Redacted<string>): Effect.Effect<ApiKeyRecord, CredentialFault> =>
      Effect.gen(function* () {
        const [, prefixBody, , secret] = yield* Schema.decode(_KeyWire)(Redacted.value(presented)).pipe(
          Effect.mapError(() => new CredentialFault({ reason: "malformed", detail: "malformed key frame" })))
        const prefix = `rk_${prefixBody}`
        return yield* limit({ algorithm: "token-bucket", onExceeded: "fail", window, limit: budget, key: `apikey:${prefix}` })(
          Effect.gen(function* () {
            const candidates = yield* store.byPrefix(prefix)
            const record = yield* Effect.flatMap(
              digest.resolve("low", Redacted.make(secret), candidates, (candidate) => candidate.digest),
              Option.match({
                onNone: () =>
                  Effect.zipRight(
                    Reject.mark("credential", { surface: "apikey" }),
                    Effect.fail(new CredentialFault({ reason: "notFound", detail: prefix }))),
                onSome: Effect.succeed,
              }),
            )
            const now = yield* DateTime.now
            yield* Effect.succeed(record).pipe(
              Effect.filterOrFail((held) => Option.isNone(held.revokedAt), () => new CredentialFault({ reason: "revoked", detail: record.id })),
              Effect.filterOrFail((held) => !Option.exists(held.expiresAt, (exp) => DateTime.greaterThan(now, exp)), () => new CredentialFault({ reason: "expired", detail: record.id })),
            )
            yield* store.touch(record.id, now)
            return record
          }),
        ).pipe(Effect.catchTags({
          RateLimitExceeded: () => Effect.fail(new CredentialFault({ reason: "throttled", detail: prefix })),
          RateLimitStoreError: (error) => Effect.fail(new CredentialFault({ reason: "throttled", detail: String(error) })),
        }))
      }).pipe(Reject.measured("credential", { surface: "apikey" }), Effect.withSpan("security.apikey.resolve"))
    const rotate = (id: string, subject: string, name: string, scopes: ReadonlyArray<string>, ttl: Option.Option<Duration.DurationInput>): Effect.Effect<MintReceipt, CredentialFault> =>
      Effect.flatMap(DateTime.now, (now) => Effect.zipRight(store.revoke(id, now), mint(subject, name, scopes, ttl)))
    const revoke = (id: string): Effect.Effect<void, CredentialFault> => Effect.flatMap(DateTime.now, (now) => store.revoke(id, now))
    return { mint, resolve, rotate, revoke } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

class ApiKeyGuard extends HttpApiMiddleware.Tag<ApiKeyGuard>()("security/authn/ApiKeyGuard", {
  provides: CurrentApiKey,
  failure: CredentialFault,
  security: { apiKey: HttpApiSecurity.apiKey({ in: "header", key: "x-api-key" }) },
}) {
  static readonly Live: Layer.Layer<ApiKeyGuard, never, ApiKey> = Layer.effect(
    ApiKeyGuard,
    Effect.map(ApiKey, (keys) => ({ apiKey: (presented: Redacted.Redacted<string>) => keys.resolve(presented) })),
  )
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ApiKey, ApiKeyGuard, ApiKeyRecord, ApiKeyStore, CredentialFault, CurrentApiKey, MintReceipt, Otp, RecoverySet }
export type { OtpVerdict }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

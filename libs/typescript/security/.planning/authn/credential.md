# [SECURITY_CREDENTIAL]

The one digest-at-rest credential owner: second-factor OTP, recovery codes, and machine API keys — three surfaces over one mint-and-resolve idiom the census flagged as byte-for-byte identical. `Digest` is that idiom made a value: mint an opaque secret, store its argon2 digest keyed by a public index, resolve a presented secret by index-scoped candidate scan then constant-time verify. Recovery codes and API keys both compose it — a recovery set is N codes over `Digest`, an API key is `rk_<prefix>.<secret>` over `Digest` with a prefix index — so the `findFirst` candidate scan and the `Crypto.digest`/`Crypto.verify` pair exist once. `Otp` owns the TOTP/HOTP rows through `otplib` v13's strategy-discriminated result rail bound to `crypt/sign`'s `Crypto` ports, so second-factor HMAC rides the same primitive the folder owns and the bundled `@noble/hashes` stack is bypassed; the TOTP replay floor rides otplib's own `afterTimeStep` option — the caller's stored floor rejects a valid-within-window replay inside the library — and `Accepted.timeStep` is what the caller persists as the next floor; `createGuardrails` bounds secret bytes, period, counter, and window per policy. Every secret — enrollment secret, provisioning URI, recovery code, API key — is `Redacted` until the QR render or the one-time receipt at the edge; a wrong OTP is the `Rejected` verdict, a recovery or key miss is a typed fault, and `CredentialFault` fires only when a primitive throws.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                              | [PUBLIC]                                       |
| :-----: | :------------------ | :------------------------------------------------------------------ | :--------------------------------------------- |
|  [01]   | `DIGEST_IDIOM`      | the shared mint + candidate-resolve fold, the folder fault          | `Digest`, `CredentialFault`                    |
|  [02]   | `SECOND_FACTOR`     | TOTP/HOTP enroll/verify, the replay floor, recovery codes           | `Otp`, `OtpVerdict`, `RecoverySet`             |
|  [03]   | `MACHINE_KEY`       | mint / prefix-resolve / rotate / revoke over `Digest`               | `ApiKey`, `ApiKeyRecord`, `MintReceipt`, `ApiKeyStore` |

## [2]-[DIGEST_IDIOM]

[DIGEST_IDIOM]:
- Owner: `Digest` — the shared credential-at-rest idiom: `mint(alphabet, length)` issues an opaque secret and its argon2 digest through `Crypto.token`/`Crypto.digest("apiKey", ...)`, and `resolve(presented, candidates, digestOf)` scans an index-scoped candidate set with `Effect.findFirst` over `Crypto.verify`, returning the matched candidate. `CredentialFault` is the folder fault shape; a `false` verify is a scan miss, never a fault.
- Law: the digest is the PHC string the `apiKey` cost row governs; the resolve budget is amortized over the caller's index (a prefix, a subject) so `findFirst` walks a bounded candidate set, never the whole table, and a stale-parameter match surfaces as the `Matched({ stale })` rehash signal the caller persists on.
- Law: every mint is `Redacted` from the RNG; the digest is `Redacted` at rest; the plaintext leaves only through the caller's one-time receipt.
- Growth: a new credential surface (a signed-URL token, a device pairing code) composes `Digest.mint`/`.resolve` with its own index — the idiom never forks.
- Boundary: `crypt/sign`'s `Crypto` owns the RNG, the argon2 digest, and the constant-time verify; this owner composes them into the mint/resolve fold every credential surface reads.
- Packages: `crypt/sign` (`Crypto.token`/`.digest`/`.verify`); `effect` (`Array`, `Effect`, `Option`, `Redacted`, `Schema`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { createGuardrails, generateSecret, generateURI, verify, type OTPGuardrails, type OTPVerifyFunctionalOptions } from "otplib"
import { FaultClass } from "@rasm/ts/core"
import { Array, Context, Data, DateTime, type Duration, Effect, Option, Redacted, Schema } from "effect"
import { Crypto } from "../crypt/sign.ts"

const _reasons = ["mint", "verify", "malformed", "notFound", "revoked", "expired"] as const

const _faults = {
  mint: { class: "defect" },
  verify: { class: "defect" },
  malformed: { class: "malformed" },
  notFound: { class: "denied" },
  revoked: { class: "denied" },
  expired: { class: "expired" },
} as const

declare namespace CredentialFault {
  type Reason = keyof typeof _faults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
}

class CredentialFault extends Schema.TaggedError<CredentialFault>()("CredentialFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<credential:${this.reason}> ${this.detail}`
  }
}

const _digest = (cipher: Context.Tag.Service<Crypto>) => ({
  mint: (alphabet: string, length: number): Effect.Effect<{ readonly secret: Redacted.Redacted<string>; readonly digest: Redacted.Redacted<string> }, CredentialFault> =>
    cipher.token(alphabet, length).pipe(
      Effect.mapError((cause) => new CredentialFault({ reason: "mint", detail: cause.detail })),
      Effect.flatMap((secret) =>
        cipher.digest("apiKey", secret).pipe(
          Effect.mapError((cause) => new CredentialFault({ reason: "mint", detail: cause.detail })),
          Effect.map((digest) => ({ secret, digest })))),
    ),
  resolve: <A>(presented: Redacted.Redacted<string>, candidates: ReadonlyArray<A>, digestOf: (candidate: A) => Redacted.Redacted<string>): Effect.Effect<Option.Option<A>, CredentialFault> =>
    Effect.findFirst(candidates, (candidate) =>
      cipher.verify("apiKey", digestOf(candidate), presented).pipe(
        Effect.mapError((cause) => new CredentialFault({ reason: "verify", detail: cause.detail })),
        Effect.map((verdict) => verdict._tag === "Matched"))),
} as const)
```

## [3]-[SECOND_FACTOR]

[SECOND_FACTOR]:
- Owner: `Otp.enroll` mints the base32 secret and the `otpauth://` URI, `Otp.verify` checks a presented token, `Otp.mintRecovery` issues N single-use codes over `Digest`, `Otp.redeem` finds the matching unspent code. `OtpVerdict` is the second-factor result — `Accepted({ delta, timeStep })` or `Rejected` — and `RecoverySet` carries the codes and their digests. The otplib `crypto`/`base32` ports bind to `Crypto.plugin`/`Crypto.base32`, and `createGuardrails` bounds secret bytes, period, counter, and window per policy.
- Law: verification is result-typed and constant-time inside otplib — a wrong code is `Rejected`, never a throw; TOTP verifies past-only under `_EPOCH_TOLERANCE`, HOTP look-ahead under `_COUNTER_TOLERANCE` when the caller passes a `Some` counter; a valid HOTP match persists `counter + delta + 1` — the `Accepted.delta` resync signal.
- Law: the TOTP replay floor is library-enforced — the caller's stored floor passes as otplib's `afterTimeStep` option, so a token whose matched `timeStep` is not strictly greater lands `{ valid: false }` inside the constant-time verify; `Accepted.timeStep` carries the RFC-6238 step number the caller persists as the next floor, and HOTP carries no `timeStep` (its counter is the floor).
- Law: recovery codes are `Digest` material, not an otplib feature — `mintRecovery` composes `Digest.mint` per code and `redeem` composes `Digest.resolve` over the digests, returning the matched index so the store marks exactly that code spent.
- Receipt: `OtpVerdict` on verify, `Option<number>` on redeem (the spent index), `RecoverySet` on mint — never a raw boolean.
- Growth: a Steam-Guard-style alphabet is one otplib `hooks` value; HOTP is the same call with a `Some` counter — the input value is the strategy discriminant, never a name fork.
- Boundary: the edge renders the `otpauth://` URI to a QR (the one secret egress); `Digest` owns the recovery mint/resolve; `crypt/sign` owns the HMAC and the digest.
- Packages: `otplib` (`verify`/`generateSecret`/`generateURI`, `createGuardrails`, `OTPGuardrails`, `afterTimeStep`); `Digest` (recovery); `Crypto` (ports).

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
const _RECOVERY_ALPHABET = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"
const _OtpVerdict = Data.taggedEnum<OtpVerdict>()

class Otp extends Effect.Service<Otp>()("security/authn/Otp", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const digest = _digest(cipher)
    const _ports = { crypto: cipher.plugin, base32: cipher.base32 } as const
    const _rails: OTPGuardrails = createGuardrails({ MIN_SECRET_BYTES: 16, MIN_PERIOD: 30, MAX_WINDOW: 2 })
    const enroll = (issuer: string, label: string): Effect.Effect<{ readonly secret: Redacted.Redacted<string>; readonly uri: Redacted.Redacted<string> }, CredentialFault> =>
      Effect.try({
        try: () => {
          const secret = generateSecret({ ..._ports })
          return { secret: Redacted.make(secret), uri: Redacted.make(generateURI({ strategy: "totp", issuer, label, secret })) }
        },
        catch: (cause) => new CredentialFault({ reason: "mint", detail: String(cause) }),
      })
    const verify_ = (secret: Redacted.Redacted<string>, token: string, floor: Option.Option<number> = Option.none(), counter: Option.Option<number> = Option.none()): Effect.Effect<OtpVerdict, CredentialFault> =>
      Effect.tryPromise({
        try: () =>
          Option.match(counter, {
            onNone: () => verify({ strategy: "totp", secret: Redacted.value(secret), token, epochTolerance: _EPOCH_TOLERANCE, ...(Option.isSome(floor) && { afterTimeStep: floor.value }), guardrails: _rails, ..._ports } satisfies OTPVerifyFunctionalOptions),
            onSome: (at) => verify({ strategy: "hotp", secret: Redacted.value(secret), token, counter: at, counterTolerance: _COUNTER_TOLERANCE, guardrails: _rails, ..._ports } satisfies OTPVerifyFunctionalOptions),
          }),
        catch: (cause) => new CredentialFault({ reason: "verify", detail: String(cause) }),
      }).pipe(Effect.map((result) =>
        result.valid
          ? _OtpVerdict.Accepted({ delta: result.delta, timeStep: "timeStep" in result ? Option.some(result.timeStep) : Option.none<number>() })
          : _OtpVerdict.Rejected()))
    const mintRecovery = (count: number): Effect.Effect<RecoverySet, CredentialFault> =>
      Effect.map(
        Effect.forEach(Array.range(1, count), () => digest.mint(_RECOVERY_ALPHABET, 10)),
        (pairs) => new RecoverySet({ codes: Array.map(pairs, (pair) => pair.secret), digests: Array.map(pairs, (pair) => pair.digest) }),
      )
    const redeem = (presented: Redacted.Redacted<string>, digests: ReadonlyArray<Redacted.Redacted<string>>): Effect.Effect<Option.Option<number>, CredentialFault> =>
      Effect.map(
        digest.resolve(presented, Array.map(digests, (held, index) => ({ held, index })), (row) => row.held),
        Option.map((row) => row.index),
      )
    return { enroll, verify: verify_, mintRecovery, redeem } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}
```

## [4]-[MACHINE_KEY]

[MACHINE_KEY]:
- Owner: `ApiKey.mint` issues `rk_<prefix>.<secret>` and stores its digest through `Digest.mint`; `ApiKey.resolve` splits the prefix, loads the prefix-indexed candidates, resolves through `Digest.resolve`, checks lifecycle, and touches `lastUsedAt`; `ApiKey.rotate` revokes and re-mints for the same subject; `ApiKey.revoke` timestamps. `ApiKeyRecord` is the stored credential, `MintReceipt` the one-time plaintext, `ApiKeyStore` the prefix-indexed port. One polymorphic `resolve` dispatches on the presented value, never a `getByKey`/`verifyKey` twin.
- Law: the plaintext leaves only through `MintReceipt`; the digest is the PHC the `apiKey` cost row governs; a revoked or expired record is a typed fault, never a silent accept; the resolve reuses `Digest.resolve` so the candidate scan is the shared idiom, not a re-implementation.
- Law: `resolve` amortizes over the public prefix — a full-table scan is the rejected naive form, and the prefix index bounds the candidate set the constant-time verify walks.
- Receipt: `MintReceipt` on mint/rotate (the subject and scopes the edge lifts into a principal), `ApiKeyRecord` on resolve — never a bare boolean.
- Growth: a new credential facet (a description, an IP allowlist) is one `ApiKeyRecord` field; a new failure mode is one `CredentialFault` reason.
- Boundary: the data wave satisfies `ApiKeyStore`; the edge lifts the resolved record's subject and scopes into a request principal; `Digest`/`crypt/sign` own the mint and verify; this page authenticates a machine and hands the subject on — it mints no session.
- Packages: `Digest` (mint/resolve); `effect` (`DateTime`, `Duration`, `Effect`, `Option`, `Redacted`, `Schema`).

```typescript
const _ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

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

const _prefixOf = (presented: string): Option.Option<string> => {
  const dot = presented.indexOf(".")
  return dot > 0 ? Option.some(presented.slice(0, dot)) : Option.none()
}

class ApiKey extends Effect.Service<ApiKey>()("security/authn/ApiKey", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const store = yield* ApiKeyStore
    const digest = _digest(cipher)
    const mint = (subject: string, name: string, scopes: ReadonlyArray<string>, ttl: Option.Option<Duration.DurationInput>): Effect.Effect<MintReceipt, CredentialFault> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const prefixBody = yield* cipher.token(_ALPHABET, 8).pipe(Effect.mapError((cause) => new CredentialFault({ reason: "mint", detail: cause.detail })))
        const prefix = `rk_${Redacted.value(prefixBody)}`
        const minted = yield* digest.mint(_ALPHABET, 40)
        const record = new ApiKeyRecord({
          id: crypto.randomUUID(), prefix, subject, digest: minted.digest, name, scopes, createdAt: now,
          expiresAt: Option.map(ttl, (input) => DateTime.addDuration(now, input)), revokedAt: Option.none(), lastUsedAt: Option.none(),
        })
        yield* store.insert(record)
        return new MintReceipt({ record, secret: Redacted.make(`${prefix}.${Redacted.value(minted.secret)}`) })
      })
    const resolve = (presented: Redacted.Redacted<string>): Effect.Effect<ApiKeyRecord, CredentialFault> =>
      Effect.gen(function* () {
        const raw = Redacted.value(presented)
        const prefix = yield* Option.match(_prefixOf(raw), { onNone: () => Effect.fail(new CredentialFault({ reason: "malformed", detail: "no prefix" })), onSome: Effect.succeed })
        const secret = Redacted.make(raw.slice(prefix.length + 1))
        const candidates = yield* store.byPrefix(prefix)
        const record = yield* Effect.flatMap(
          digest.resolve(secret, candidates, (candidate) => candidate.digest),
          Option.match({ onNone: () => Effect.fail(new CredentialFault({ reason: "notFound", detail: prefix })), onSome: Effect.succeed }),
        )
        yield* Option.isSome(record.revokedAt) ? Effect.fail(new CredentialFault({ reason: "revoked", detail: record.id })) : Effect.void
        const now = yield* DateTime.now
        yield* Option.match(record.expiresAt, { onNone: () => false, onSome: (exp) => DateTime.greaterThan(now, exp) })
          ? Effect.fail(new CredentialFault({ reason: "expired", detail: record.id }))
          : Effect.void
        yield* store.touch(record.id, now)
        return record
      })
    const rotate = (id: string, subject: string, name: string, scopes: ReadonlyArray<string>, ttl: Option.Option<Duration.DurationInput>): Effect.Effect<MintReceipt, CredentialFault> =>
      Effect.flatMap(DateTime.now, (now) => Effect.zipRight(store.revoke(id, now), mint(subject, name, scopes, ttl)))
    const revoke = (id: string): Effect.Effect<void, CredentialFault> => Effect.flatMap(DateTime.now, (now) => store.revoke(id, now))
    return { mint, resolve, rotate, revoke } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ApiKey, ApiKeyRecord, ApiKeyStore, CredentialFault, MintReceipt, Otp, RecoverySet }
export type { OtpVerdict }
```

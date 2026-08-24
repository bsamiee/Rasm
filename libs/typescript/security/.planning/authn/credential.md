# [SECURITY_CREDENTIAL]

One digest-at-rest credential owner: second-factor OTP, recovery codes, and machine API keys — three surfaces over one mint-and-resolve idiom the census flagged as byte-for-byte identical. `Digest` is that idiom made a value: mint an opaque secret, seal it by the material's own entropy posture keyed on a public index, resolve a presented secret by index-scoped candidate scan then constant-time compare. The posture is the idiom's one discriminant and it is absolute — guessable material earns the argon2 KDF, a random mint earns the SHA-256 fingerprint compare — so neither a recovery redemption nor a machine-key resolve pays a KDF pass per candidate out of the same bulkhead every login queues on; a KDF on a random secret buys defense its entropy already provides and prices it on every authenticated call. Recovery codes and API keys both compose it — a recovery set is N codes over `Digest`, an API key is `rk_<prefix>.<secret>` over `Digest` with a prefix index decoded through one `Schema.TemplateLiteralParser` owner — so the `findFirst` candidate scan and the seal/probe pair exist once. `Otp` owns the TOTP/HOTP rows through `otplib` v13's strategy-discriminated result rail bound to `crypt/sign`'s `Crypto` ports, so second-factor HMAC rides the same primitive the folder owns and the bundled `@noble/hashes` stack is bypassed; the TOTP replay floor rides otplib's own `afterTimeStep` option, `Accepted.timeStep` is the next floor the caller persists, `remaining` projects the seconds left in the current window for the ui prompt off the same folder clock the verify's own `epoch` reads, and an `OTPHooks` value threads through `verify` so a Steam-Guard-style alphabet is a value, never a fork. Every credential-verify surface is a brute-force target and every one is throttled: `Otp.verify`/`Otp.redeem` run under their per-subject `Curb` rows and `ApiKey.resolve` under the per-prefix `apikey` row, an exhausted budget is the `throttled` fault (class `exhausted`), and every presentation lands on the folder ledger tagged by surface — a refusal on the `credential` reject row, an admission and its wall span on the same kind's twin — so each surface's guess rate reads against its own denominator. Record ids mint through the `Crypto` entropy port; every secret is `Redacted` until the QR render or the one-time receipt at the edge; a wrong OTP is the `Rejected` verdict, a malformed one refuses at a shape gate ahead of the budget so garbage never spends a victim's allowance, a recovery or key miss is a typed fault, and a thrown `OTPError` grades by whose input raised it — the caller's token shape or this deployment's own material — never as one opaque string; the `CredentialFault` rows close at the core `Fault.Class.family` seam. `ApiKeyGuard` is the declarative api-key scheme face — the runtime serve admission lift composes the same `resolve`, and a direct HttpApi consumer mounts it.

## [01]-[INDEX]

- [02]-[DIGEST_IDIOM]: `Digest`, `CredentialFault`.
- [03]-[SECOND_FACTOR]: `Otp`, `OtpVerdict`, `RecoverySet`.
- [04]-[MACHINE_KEY]: `ApiKey`, `ApiKeyRecord`, `MintReceipt`, `ApiKeyStore`, `ApiKeyGuard`, `CurrentApiKey`.

## [02]-[DIGEST_IDIOM]

[DIGEST_IDIOM]:
- Owner: `Digest` — the shared credential-at-rest idiom over one entropy posture: `mint(posture, alphabet, length)` issues an opaque secret and seals it by that posture's row, and `resolve(posture, presented, candidates, digestOf)` scans an index-scoped candidate set with `Effect.findFirst` over the same row's probe, returning the matched candidate. `CredentialFault` is the folder fault shape closed at the core family seam; a `false` probe is a scan miss, never a fault.
- Law: the storage form is the material's entropy class, spelled once here rather than per surface — `low` seals through the `login` argon2 cost row and probes through its constant-time verify, `high` seals a `Crypto.fingerprint` and probes the `Probe.Digest` compare, and a caller names what its own mint produced, never a mechanism. A high-entropy set therefore costs one SHA-256 per candidate where a KDF row would cost an argon2 pass: an eight-code recovery redemption under `low` is a multi-second authenticated request that also holds permits from the 4-permit KDF bulkhead every concurrent login queues on, and the codes carry ~49 bits of `Crypto.token` entropy no digest table walks.
- Law: the discriminant admits no per-surface exception — machine keys, recovery codes, and every future random mint take `high`, because argon2 on a ~230-bit random secret defends against no walkable digest table while pricing a KDF pass and a bulkhead permit onto every machine call; `low` exists for material a human can type or guess, and it is the general password-hashing posture standing at full strength for a planned consumer — its one guessable credential kind is the `password` `CredentialRef.kind` no ceremony resolves yet, never a live session path, because session refresh presents a random fingerprint and takes `high` like every other machine-minted secret.
- Law: the resolve budget is amortized over the caller's index (a prefix, a subject) so `findFirst` walks a bounded candidate set, never the whole table, and a stale-parameter `low` match surfaces as the `Matched({ stale })` rehash signal the caller persists on.
- Law: every mint is `Redacted` from the RNG; the digest is `Redacted` at rest; the plaintext leaves only through the caller's one-time receipt.
- Growth: a new credential surface (a signed-URL token, a device pairing code) composes `Digest.mint`/`.resolve` with its own index and its own posture — the idiom never forks; a new storage mechanism is one posture row both members inherit.
- Boundary: `crypt/sign`'s `Crypto` owns the RNG, the argon2 digest, the fingerprint, and the constant-time compare; this owner composes them into the posture-keyed mint/resolve fold every credential surface reads.

```typescript
import { HttpApiMiddleware, HttpApiSecurity } from "@effect/platform"
import { createGuardrails, generateSecret, generateURI, verify, type OTPGuardrails, type OTPVerifyFunctionalOptions } from "otplib"
import type { OTPHooks } from "@otplib/core"
import { OTPError, TokenError } from "@otplib/core/errors"
import { validateToken } from "@otplib/core/utils"
import { Fault } from "@rasm/core"
import { Array, Clock, Context, Data, DateTime, Duration, Effect, Layer, Option, Redacted, Schema } from "effect"
import { Alphabet, Crypto, Probe, type SignFault } from "../crypt/sign.ts"
import { Curb, Reject } from "../crypt/verify.ts"

// Four legs partition the surface and each reason renders the subject its own reader acts on: the digest leg carries
// only the primitive's cause because the material it handled must never reach a message, the shape leg the same, the
// record leg names the credential id and its subject, and the throttle leg names the surface and the index whose
// budget went. The blame split lives in the CLASS column and the leg makes it legible — `malformed` is the caller's
// token, `verify` this deployment's own material, and one free `detail` string reported both as the same prose.
const _family = Fault.Class.family(["mint", "verify", "malformed", "notFound", "revoked", "expired", "throttled"] as const, {
  mint: Fault.Class.row({
    class: "defect",
    leg: "digest",
    detail: Schema.Struct({ cause: Schema.String }),
    render: ({ cause }) => `credential mint refused: ${cause}`,
  }),
  verify: Fault.Class.row({
    class: "defect",
    leg: "digest",
    detail: Schema.Struct({ cause: Schema.String }),
    render: ({ cause }) => `credential compare refused: ${cause}`,
  }),
  malformed: Fault.Class.row({
    class: "malformed",
    leg: "shape",
    detail: Schema.Struct({ cause: Schema.String }),
    render: ({ cause }) => `presented credential is unreadable: ${cause}`,
  }),
  notFound: Fault.Class.row({
    class: "denied",
    leg: "record",
    detail: Schema.Struct({ index: Schema.String }),
    render: ({ index }) => `no live credential under ${index}`,
  }),
  revoked: Fault.Class.row({
    class: "denied",
    leg: "record",
    detail: Schema.Struct({ id: Schema.String, subject: Schema.String }),
    render: ({ id, subject }) => `credential ${id} of ${subject} is revoked`,
  }),
  expired: Fault.Class.row({
    class: "expired",
    leg: "record",
    detail: Schema.Struct({ id: Schema.String, subject: Schema.String }),
    render: ({ id, subject }) => `credential ${id} of ${subject} is past its expiry`,
  }),
  throttled: Fault.Class.row({
    class: "exhausted",
    leg: "throttle",
    detail: Schema.Struct({ surface: Schema.String, index: Schema.String, cause: Schema.String }),
    render: ({ cause, index, surface }) => `${surface} budget spent on ${index}: ${cause}`,
  }),
})

declare namespace CredentialFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class CredentialFault extends Schema.TaggedError<CredentialFault>()("CredentialFault", {
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
      seal: (secret) => cipher.digest("login", secret),
      probe: (presented, stored) => Effect.map(cipher.verify("login", stored, presented), (verdict) => verdict._tag === "Matched"),
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
        Effect.mapError((cause) => new CredentialFault({ case: { reason: "mint", cause: cause.message } })),
      ),
    resolve: <A>(posture: _Posture, presented: Redacted.Redacted<string>, candidates: ReadonlyArray<A>, digestOf: (candidate: A) => Redacted.Redacted<string>): Effect.Effect<Option.Option<A>, CredentialFault> =>
      Effect.findFirst(candidates, (candidate) => rows[posture].probe(presented, digestOf(candidate))).pipe(
        Effect.mapError((cause) => new CredentialFault({ case: { reason: "verify", cause: cause.message } })),
      ),
  } as const
}
```

## [03]-[SECOND_FACTOR]

[SECOND_FACTOR]:
- Owner: `Otp.enroll` mints the base32 secret and the `otpauth://` URI, `Otp.verify` gates the presented token's shape and then checks it under the per-subject budget, `Otp.mintRecovery` issues N single-use codes over `Digest`, `Otp.redeem` finds the matching unspent code under the same budget, `Otp.remaining` projects the seconds left in the current TOTP window for the ui prompt. `OtpVerdict` is the second-factor result — `Accepted({ delta, timeStep })` or `Rejected` — and `RecoverySet` carries the codes and their digests. otplib's `crypto`/`base32` ports bind to `Crypto.plugin`/`Crypto.base32`, `createGuardrails` bounds secret bytes, period, counter, and window per policy, and the optional `OTPHooks` value threads through `verify` so a non-numeric token variant is one hooks row.
- Law: verification is result-typed and constant-time inside otplib — a wrong code is `Rejected`, never a throw; TOTP verifies past-only under `_EPOCH_TOLERANCE`, HOTP look-ahead under `_COUNTER_TOLERANCE` when the caller passes a `Some` counter; a valid HOTP match persists `counter + delta + 1` — the `Accepted.delta` resync signal. That constant-time claim is the crypto port's, not the strategy's: otplib hands the port two token STRINGS, so it holds only because `Crypto.plugin` lifts both operands to bytes before its byte-domain primitive runs.
- Law: the TOTP parameters are passed, never defaulted into, on all three axes — `_PERIOD` and `_DIGITS` ride `enroll`'s URI and every `verify` leg, and the INSTANT rides beside them: `_seconds` projects the folder `Clock` into otplib's `epoch` and into `remaining`'s countdown, so the authenticator's step, the server's step arithmetic, the prompt's countdown, and the window the compare walks are one clock and one row. Passing period alone left the verify reading ambient `Date.now()` while the countdown read the injected clock — the no-drift claim held on the step and leaked on the time axis, and a TestClock moved the two apart.
- Law: a THROWN otplib fault is graded by whose input caused it — `_thrown` reads the `OTPError` family and folds a `TokenError` (length, charset) onto the caller-caused `malformed` reason while every other family — secret, plugin, crypto, period, tolerance, replay floor — stays the system-caused `verify` defect. `_shaped` refuses a malformed presentation AHEAD of the `Curb` guard, so a garbage string spends no budget: shape needs no secret, no HMAC, and no bucket to decide, and letting it inside the guard handed any caller a free denial-of-service against a named subject's five-per-window allowance. That refusal still marks the ledger, because a ledger write is evidence and never gates.
- Law: the TOTP replay floor is library-enforced — the caller's stored floor passes as otplib's `afterTimeStep` option, so a token whose matched `timeStep` is not strictly greater lands `{ valid: false }` inside the constant-time verify; `Accepted.timeStep` carries the RFC-6238 step number the caller persists as the next floor, and HOTP carries no `timeStep` (its counter is the floor).
- Law: `verify` and `redeem` are keyed brute-force targets — each runs its VERIFY under its own subject-keyed `Curb` row (`otp`, `recovery`), an exhausted budget folds to `throttled` at the guard, a `Rejected` verdict lands `Reject.mark("credential", { surface })` with the `otp`/`recovery` surface facet, and each entrypoint composes `Reject.measured("credential", { surface })` so the same facet carries the admission and the ceremony span; a guessing campaign is bounded by the store-backed limiter and legible as a ratio rather than as a raw count that a traffic spike reproduces.
- Law: recovery codes are `Digest` material at the high-entropy posture, not an otplib feature — `mintRecovery` composes `Digest.mint("high", …)` per code and `redeem` composes `Digest.resolve("high", …)` over the digests, returning the matched index so the store marks exactly that code spent and the whole set resolves in one constant-time pass.
- Receipt: `OtpVerdict` on verify, `Option<number>` on redeem (the spent index), `RecoverySet` on mint — never a raw boolean.
- Growth: a Steam-Guard-style alphabet is one `OTPHooks` value through the threaded option; HOTP is the same call with a `Some` counter — the input value is the strategy discriminant, never a name fork.
- Boundary: the edge renders the `otpauth://` URI to a QR (the one secret egress) and the `remaining` countdown beside the prompt; `Digest` owns the recovery mint/resolve; `crypt/sign` owns the HMAC and the digest; `crypt/verify`'s `Curb` owns the budget rows.
- Packages: `otplib` (`verify`/`generateSecret`/`generateURI`, `createGuardrails`, `OTPGuardrails`, `OTPHooks`, `afterTimeStep`, `epoch`, `digits`); `@otplib/core/errors` (`OTPError`, `TokenError`); `@otplib/core/utils` (`validateToken`); `crypt/verify` (`Reject`, `Curb`); `Digest` (recovery); `Crypto` (ports); `effect` (`Clock`).

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
const _DIGITS = 6
const _RECOVERY_ALPHABET = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"
const _OtpVerdict = Data.taggedEnum<OtpVerdict>()

// ONE clock for the whole TOTP surface. otplib defaults `epoch` to ambient `Date.now()`, so a verify that never
// passes it reads a different instant than the countdown beside the prompt, and a TestClock moves the countdown
// while the verification window stays pinned to wall time — the no-drift law held on the step and leaked on the axis.
const _seconds = Effect.map(Clock.currentTimeMillis, (millis) => Math.floor(millis / 1000))

// otplib raises a 15-class `OTPError` tree and two distinct blames live inside it, so one opaque string was never
// this fold: `TokenError` names the presented value's own length or charset — the CALLER's, class `malformed` —
// while a secret, plugin, crypto, period, tolerance, or replay-floor fault names this deployment's stored material
// or its own policy, class `defect`. Grading five typed digits as a defect pages an operator on user typing.
const _thrown = (cause: unknown): CredentialFault =>
  new CredentialFault({
    case: {
      reason: cause instanceof TokenError ? "malformed" : "verify",
      cause: cause instanceof OTPError ? cause.message : String(cause),
    },
  })

// Shape gating sits AHEAD of the budget: `validateToken` decides length and charset with no secret, no HMAC, and
// no bucket spend, so a flood of garbage keyed on a victim's subject cannot burn that subject's window before one
// real code is ever checked. Refusals still land their ledger row — evidence is free and never gates — and the
// guard then wraps the verify alone, which is the only leg an attacker can price.
const _shaped = (token: string): Effect.Effect<void, CredentialFault> =>
  Effect.try({ try: () => validateToken(token, _DIGITS), catch: _thrown }).pipe(
    Effect.tapError(() => Reject.mark("credential", { surface: "otp" })),
  )

class Otp extends Effect.Service<Otp>()("security/authn/Otp", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const digest = _digest(cipher)
    const curb = yield* Curb
    const _ports = { crypto: cipher.plugin, base32: cipher.base32 } as const
    const _rails: OTPGuardrails = createGuardrails({ MIN_SECRET_BYTES: 16, MIN_PERIOD: _PERIOD, MAX_WINDOW: 2 })
    const _throttled = (subject: string, surface: "otp" | "recovery") =>
      curb.guard(surface, subject, (cause: string) => new CredentialFault({ case: { reason: "throttled", surface, index: subject, cause } }))
    const enroll = (issuer: string, label: string): Effect.Effect<{ readonly secret: Redacted.Redacted<string>; readonly uri: Redacted.Redacted<string> }, CredentialFault> =>
      Effect.try({
        try: () => {
          const secret = generateSecret(_ports)
          return { secret: Redacted.make(secret), uri: Redacted.make(generateURI({ strategy: "totp", issuer, label, secret, period: _PERIOD, digits: _DIGITS })) }
        },
        catch: (cause) => new CredentialFault({ case: { reason: "mint", cause: String(cause) } }),
      })
    const verify_ = (
      subject: string,
      secret: Redacted.Redacted<string>,
      token: string,
      floor: Option.Option<number> = Option.none(),
      counter: Option.Option<number> = Option.none(),
      hooks: Option.Option<OTPHooks> = Option.none(),
    ): Effect.Effect<OtpVerdict, CredentialFault> =>
      Effect.gen(function* () {
        yield* _shaped(token)
        const epoch = yield* _seconds
        return yield* _throttled(subject, "otp")(
          Effect.tryPromise({
            try: () =>
              Option.match(counter, {
                // Tolerance pairs spread rather than pass by reference: otplib's option types take a mutable
                // `[number, number]`. Period, digits, and epoch each ride the leg explicitly, because the library
                // defaults every one of them off its own constants and its own `Date.now()` — three silent forks
                // between what the authenticator was provisioned with and what the server checks against.
                onNone: () => verify({ strategy: "totp", secret: Redacted.value(secret), token, period: _PERIOD, digits: _DIGITS, epoch, epochTolerance: [..._EPOCH_TOLERANCE], ...(Option.isSome(floor) && { afterTimeStep: floor.value }), ...(Option.isSome(hooks) && { hooks: hooks.value }), guardrails: _rails, ..._ports } satisfies OTPVerifyFunctionalOptions),
                onSome: (at) => verify({ strategy: "hotp", secret: Redacted.value(secret), token, digits: _DIGITS, counter: at, counterTolerance: [..._COUNTER_TOLERANCE], ...(Option.isSome(hooks) && { hooks: hooks.value }), guardrails: _rails, ..._ports } satisfies OTPVerifyFunctionalOptions),
              }),
            catch: _thrown,
          }).pipe(
            Effect.map((result) =>
              result.valid
                ? _OtpVerdict.Accepted({ delta: result.delta, timeStep: "timeStep" in result ? Option.some(result.timeStep) : Option.none<number>() })
                : _OtpVerdict.Rejected()),
            Effect.tap((verdict) =>
              verdict._tag === "Rejected" ? Reject.mark("credential", { surface: "otp" }) : Effect.void),
          ),
        )
      }).pipe(Reject.measured("credential", { surface: "otp" }), Effect.withSpan("security.otp.verify"))
    const remaining = (): Effect.Effect<number> =>
      Effect.map(_seconds, (seconds) => _PERIOD - (seconds % _PERIOD))
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
  dependencies: [Crypto.Default, Curb.Default],
  accessors: true,
}) {}
```

## [04]-[MACHINE_KEY]

[MACHINE_KEY]:
- Owner: `ApiKey.mint` issues `rk_<prefix>.<secret>` and stores its digest through `Digest.mint`; `ApiKey.resolve` decodes the wire frame through the `_KeyWire` parser, loads the prefix-indexed candidates under the `Curb` `apikey` budget, resolves through `Digest.resolve`, gates lifecycle through `filterOrFail`, and touches `lastUsedAt`; `ApiKey.rotate` revokes and re-mints for the same subject; `ApiKey.revoke` timestamps; `ApiKey.held` enumerates every key a subject holds and `ApiKey.sweep` revokes them all — the machine twin of revoke-every-session, so rotation, offboarding, and breach response reach a principal's whole key set. `ApiKeyRecord` is the stored credential, `MintReceipt` the one-time plaintext, `ApiKeyStore` the prefix- and subject-indexed port. `CurrentApiKey`/`ApiKeyGuard` are the declarative scheme seam — the middleware Tag carries `HttpApiSecurity.apiKey` on the `x-api-key` header, its implementation folds `resolve`, the runtime serve admission lift composes the same `resolve`, and a consumer composing security without it mounts the Tag so a machine-keyed endpoint receives the resolved record through the requirement channel. One polymorphic `resolve` dispatches on the presented value, never a `getByKey`/`verifyKey` twin.
- Law: the plaintext leaves only through `MintReceipt`; the digest is the SHA-256 fingerprint the `high` posture mints — the secret is a ~230-bit random value, so resolve is one indexed constant-time compare per candidate and never a KDF pass; a revoked or expired record is a typed fault, never a silent accept; the resolve reuses `Digest.resolve` so the candidate scan is the shared idiom, not a re-implementation.
- Law: `resolve` amortizes over the public prefix and is throttled by it — the `Curb` `apikey` row bounds a stolen-prefix guessing campaign, an exhausted budget folds to `throttled` at the guard, a scan miss lands `Reject.mark("credential", { surface: "apikey" })`, and the resolved record lands the `apikey`-faceted admission and span through `Reject.measured`.
- Receipt: `MintReceipt` on mint/rotate (the subject and scopes the edge lifts into a principal), `ApiKeyRecord` on resolve, `ReadonlyArray<ApiKeyRecord>` on held — never a bare boolean.
- Law: the `scopes` array `mint` stamps is the machine key's delegation bound the `access/claim` ceiling reads — a key minted with `rasm:` scopes caps its authority to that bundle's union where a token caller's does, one carrying none holds its subject's whole grant, and the vocabulary is `access/claim`'s `Scope`, so a machine key is the delegation surface a service narrows without a second policy path.
- Growth: a new credential facet (a description, an IP allowlist) is one `ApiKeyRecord` field; a new failure mode is one `CredentialFault` reason.
- Boundary: the data wave satisfies `ApiKeyStore` and the limiter store; the edge lifts the resolved record's subject and scopes into a request principal; `Digest`/`crypt/sign` own the mint and verify; `Curb` owns the budget row; this page authenticates a machine and hands the subject on — it mints no session.
- Packages: `Digest` (mint/resolve); `crypt/sign` (`Alphabet`); `crypt/verify` (`Reject`, `Curb`); `@effect/platform` (`HttpApiMiddleware`, `HttpApiSecurity`); `effect` (`DateTime`, `Duration`, `Effect`, `Option`, `Redacted`, `Schema`).

```typescript
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
  readonly bySubject: (subject: string) => Effect.Effect<ReadonlyArray<ApiKeyRecord>, CredentialFault>
  readonly touch: (id: string, at: DateTime.Utc) => Effect.Effect<void, CredentialFault>
  readonly revoke: (id: string, at: DateTime.Utc) => Effect.Effect<void, CredentialFault>
  readonly revokeSubject: (subject: string, at: DateTime.Utc) => Effect.Effect<void, CredentialFault>
}>() {}

class CurrentApiKey extends Context.Tag("security/authn/CurrentApiKey")<CurrentApiKey, ApiKeyRecord>() {}

class ApiKey extends Effect.Service<ApiKey>()("security/authn/ApiKey", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const store = yield* ApiKeyStore
    const digest = _digest(cipher)
    const curb = yield* Curb
    const mint = (subject: string, name: string, scopes: ReadonlyArray<string>, ttl: Option.Option<Duration.DurationInput>): Effect.Effect<MintReceipt, CredentialFault> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const id = yield* cipher.uuid().pipe(Effect.mapError((cause) => new CredentialFault({ case: { reason: "mint", cause: cause.message } })))
        const prefixBody = yield* cipher.token(Alphabet.base62, 8).pipe(Effect.mapError((cause) => new CredentialFault({ case: { reason: "mint", cause: cause.message } })))
        const prefix = `rk_${Redacted.value(prefixBody)}`
        const minted = yield* digest.mint("high", Alphabet.base62, 40)
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
          Effect.mapError(() => new CredentialFault({ case: { reason: "malformed", cause: "key frame is not rk_<prefix>.<secret>" } })))
        const prefix = `rk_${prefixBody}`
        return yield* curb.guard("apikey", prefix, (cause: string) => new CredentialFault({ case: { reason: "throttled", surface: "apikey", index: prefix, cause } }))(
          Effect.gen(function* () {
            const candidates = yield* store.byPrefix(prefix)
            const record = yield* Effect.flatMap(
              digest.resolve("high", Redacted.make(secret), candidates, (candidate) => candidate.digest),
              Option.match({
                onNone: () =>
                  Effect.zipRight(
                    Reject.mark("credential", { surface: "apikey" }),
                    Effect.fail(new CredentialFault({ case: { reason: "notFound", index: prefix } }))),
                onSome: Effect.succeed,
              }),
            )
            const now = yield* DateTime.now
            yield* Effect.succeed(record).pipe(
              Effect.filterOrFail((held) => Option.isNone(held.revokedAt), () => new CredentialFault({ case: { reason: "revoked", id: record.id, subject: record.subject } })),
              Effect.filterOrFail(
                (held) => !Option.exists(held.expiresAt, (exp) => DateTime.greaterThan(now, exp)),
                () => new CredentialFault({ case: { reason: "expired", id: record.id, subject: record.subject } }),
              ),
            )
            yield* store.touch(record.id, now)
            return record
          }),
        )
      }).pipe(Reject.measured("credential", { surface: "apikey" }), Effect.withSpan("security.apikey.resolve"))
    const rotate = (id: string, subject: string, name: string, scopes: ReadonlyArray<string>, ttl: Option.Option<Duration.DurationInput>): Effect.Effect<MintReceipt, CredentialFault> =>
      Effect.flatMap(DateTime.now, (now) => Effect.zipRight(store.revoke(id, now), mint(subject, name, scopes, ttl)))
    const revoke = (id: string): Effect.Effect<void, CredentialFault> => Effect.flatMap(DateTime.now, (now) => store.revoke(id, now))
    const held = (subject: string): Effect.Effect<ReadonlyArray<ApiKeyRecord>, CredentialFault> => store.bySubject(subject)
    const sweep = (subject: string): Effect.Effect<void, CredentialFault> =>
      Effect.flatMap(DateTime.now, (now) => store.revokeSubject(subject, now))
    return { mint, resolve, rotate, revoke, held, sweep } as const
  }),
  dependencies: [Crypto.Default, Curb.Default],
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

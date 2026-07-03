# [SECURITY_OTP] — otplib TOTP/HOTP over the sign/crypto plugin, plus recovery/backup codes

`authn/otp` owns the second-factor rows: TOTP/HOTP enrollment and verification through `otplib` v13's strategy-discriminated result-typed rail, and recovery/backup codes — which are *not* an otplib feature, composed from `sign/crypto`'s RNG and argon2 digest-at-rest. The v13 API is options-object, async-first, and plugin-swappable: `otplib`'s HMAC and base32 are ports, so the folder binds them to the *same* `sign/crypto` primitive the rest of `sign` owns, bypassing the bundled `@noble/hashes` stack and honoring the algorithm value (SHA-1 authenticator compat by default, raised per policy). Verification returns a `VerifyResult` whose `valid` discriminant narrows to an `OtpVerdict` tagged verdict carrying `delta` for drift — never a boolean-plus-throw — and the enrollment secret and provisioning URI stay `Redacted` until the QR render at the edge. A wrong code is the `Rejected` verdict the caller maps to 401; a recovery miss is `Option.none`; `OtpFault` fires only when a primitive throws.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                       | [OWNER]                             | [PACKAGES]                    | [REJECTED_FORM]                            |
| :-----: | :------------------------------ | :--------------------------------- | :---------------------------- | :----------------------------------------- |
|  [01]   | second-factor verdict + fault   | `OtpVerdict` / `OtpFault`          | `otplib`                      | a boolean verify, a throw on a wrong code  |
|  [02]   | enroll + verify TOTP/HOTP       | `Otp.enroll` / `Otp.verify`        | `otplib`, `sign/crypto` port  | otplib's default `@noble/hashes`, a `generateTotp` fork |
|  [03]   | recovery/backup codes           | `Otp.mintRecovery` / `.redeem`     | `sign/crypto`                 | otplib codes, a plaintext recovery column  |

## [2]-[OTP_VOCABULARY]

[VERDICT_AND_FAULT]:
- Owner: `OtpVerdict` is the second-factor result — `Accepted({ delta })` (the drift signal HOTP resync consumes) or `Rejected`; `RecoverySet` carries the one-time codes and their digests; `OtpFault` is the folder 500 fault shape. `OtpConfig` fixes the strategy and tolerance policy.
- Packages: `otplib` — `generate`/`verify` are async because the `CryptoPlugin.hmac` may be async, `generateSecret`/`generateURI` are sync; `VerifyResult` narrows on `.valid`. `sign/crypto` supplies the `CryptoPlugin`/`Base32Plugin` and the recovery digest.
- Boundary: the edge renders the `otpauth://` URI to a QR (the one place the secret leaves `Redacted`); `sign/crypto` owns the HMAC and the recovery hash, so no folder pulls a second crypto stack.
- Growth: a Steam-Guard-style alphabet is one otplib `hooks` value, never a new package; HOTP is the same call with a `Some` counter — the input value is the strategy discriminant, never a name fork.

```typescript
import { generate, generateSecret, generateURI, verify, type OTPVerifyOptions } from "otplib"
import { Array, Data, Effect, Option, Redacted, Schema } from "effect"
import { Crypto } from "../sign/crypto.ts"

// --- [TYPES] ----------------------------------------------------------------------------

type OtpVerdict = Data.TaggedEnum<{
  Accepted: { readonly delta: number }
  Rejected: {}
}>

// --- [CONSTANTS] ------------------------------------------------------------------------

const _reasons = ["mint", "verify"] as const

const OtpFaultPolicy = {
  mint: { rank: 3, retry: false, status: 500 },
  verify: { rank: 3, retry: false, status: 500 },
} as const

const _EPOCH_TOLERANCE: readonly [number, number] = [1, 0]

const _COUNTER_TOLERANCE: readonly [number, number] = [0, 2]

declare namespace OtpFault {
  type Reason = keyof typeof OtpFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof OtpFaultPolicy> = T
}

// --- [MODELS] ---------------------------------------------------------------------------

class RecoverySet extends Schema.Class<RecoverySet>("RecoverySet")({
  codes: Schema.Array(Schema.Redacted(Schema.String)),
  digests: Schema.Array(Schema.Redacted(Schema.String)),
}) {}

// --- [ERRORS] ---------------------------------------------------------------------------

class OtpFault extends Schema.TaggedError<OtpFault>()("OtpFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): OtpFault.Row {
    return OtpFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<otp:${this.reason}> ${this.detail}`
  }
}
```

## [3]-[SECOND_FACTOR]

[OTP]:
- Owner: `Otp.enroll` mints the base32 secret and the `otpauth://` URI; `Otp.verify` checks a presented token — TOTP past-only under `_EPOCH_TOLERANCE`, or HOTP look-ahead under `_COUNTER_TOLERANCE` when the caller passes a `Some` counter; `Otp.mintRecovery` issues N single-use codes and their digests; `Otp.redeem` finds the matching unspent digest by constant-time argon2 verify.
- Packages: `otplib` bound to `sign/crypto`'s `plugin`/`base32` ports; `sign/crypto` `Crypto.token`/`.digest`/`.verify` for the recovery codes; `Effect.findFirst` over the recovery digests.
- Law: verification is result-typed and constant-time inside otplib — a wrong code is `Rejected`, never a throw; the secret and URI stay `Redacted`; a valid HOTP match persists `counter + delta + 1` — the `Accepted.delta` the caller's store consumes as the resync signal; recovery codes are `sign/crypto` material digested at rest, and `redeem` returns the matched index so the store marks exactly that code spent.
- Receipt: `OtpVerdict` on verify (`delta` for drift), `Option<number>` on redeem (the spent index), `RecoverySet` on mint — never a raw boolean.

```typescript
// --- [OPERATIONS] -----------------------------------------------------------------------

const _RECOVERY_ALPHABET = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"

const _OtpVerdict = Data.taggedEnum<OtpVerdict>()

// --- [SERVICES] -------------------------------------------------------------------------

class Otp extends Effect.Service<Otp>()("security/authn/Otp", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const _ports = { crypto: cipher.plugin, base32: cipher.base32 } as const
    const enroll = (issuer: string, label: string): Effect.Effect<{ readonly secret: Redacted.Redacted<string>; readonly uri: Redacted.Redacted<string> }, OtpFault> =>
      Effect.try({
        try: () => {
          const secret = generateSecret({ ..._ports })
          return { secret: Redacted.make(secret), uri: Redacted.make(generateURI({ strategy: "totp", issuer, label, secret })) }
        },
        catch: (cause) => new OtpFault({ reason: "mint", detail: String(cause) }),
      })
    const verify_ = (secret: Redacted.Redacted<string>, token: string, counter: Option.Option<number> = Option.none()): Effect.Effect<OtpVerdict, OtpFault> =>
      Effect.tryPromise({
        try: () =>
          Option.match(counter, {
            onNone: () => verify({ strategy: "totp", secret: Redacted.value(secret), token, epochTolerance: _EPOCH_TOLERANCE, ..._ports } satisfies OTPVerifyOptions),
            onSome: (at) => verify({ strategy: "hotp", secret: Redacted.value(secret), token, counter: at, counterTolerance: _COUNTER_TOLERANCE, ..._ports } satisfies OTPVerifyOptions),
          }),
        catch: (cause) => new OtpFault({ reason: "verify", detail: String(cause) }),
      }).pipe(Effect.map((result) => (result.valid ? _OtpVerdict.Accepted({ delta: result.delta }) : _OtpVerdict.Rejected())))
    const mintRecovery = (count: number): Effect.Effect<RecoverySet, OtpFault> =>
      Effect.map(
        Effect.forEach(Array.range(1, count), () =>
          cipher.token(_RECOVERY_ALPHABET, 10).pipe(
            Effect.orDie,
            Effect.flatMap((code) => Effect.map(cipher.digest("apiKey", code).pipe(Effect.orDie), (digest) => ({ code, digest }))),
          )),
        (pairs) => new RecoverySet({ codes: pairs.map((pair) => pair.code), digests: pairs.map((pair) => pair.digest) }),
      )
    const redeem = (presented: Redacted.Redacted<string>, digests: ReadonlyArray<Redacted.Redacted<string>>): Effect.Effect<Option.Option<number>, OtpFault> =>
      Effect.map(
        Effect.findFirst(Array.map(digests, (digest, index) => ({ digest, index })), (row) =>
          cipher.verify("apiKey", row.digest, presented).pipe(Effect.map((verdict) => verdict._tag === "Matched"), Effect.orDie)),
        Option.map((row) => row.index),
      )
    const generate_ = (secret: Redacted.Redacted<string>, counter: Option.Option<number> = Option.none()): Effect.Effect<Redacted.Redacted<string>, OtpFault> =>
      Effect.tryPromise({
        try: () =>
          Option.match(counter, {
            onNone: () => generate({ strategy: "totp", secret: Redacted.value(secret), ..._ports }),
            onSome: (at) => generate({ strategy: "hotp", secret: Redacted.value(secret), counter: at, ..._ports }),
          }),
        catch: (cause) => new OtpFault({ reason: "mint", detail: String(cause) }),
      }).pipe(Effect.map(Redacted.make))
    return { enroll, verify: verify_, mintRecovery, redeem, generate: generate_ } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Otp, OtpFault, RecoverySet }
export type { OtpVerdict }
```

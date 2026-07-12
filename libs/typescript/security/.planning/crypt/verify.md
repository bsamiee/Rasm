# [SECURITY_VERIFY]

The external-signature ingress row: one closed dialect table carries every inbound authenticity convention — symmetric HMAC webhooks and asymmetric ECDSA/RSA partner and attestation signatures in both PKIX-DER and IEEE-P1363 wire forms — and one verify fold runs any dialect over the HELD request octets, so a provider integration is a table row, never a bespoke verifier. The byte-identity law governs the whole page: verification computes over the exact bytes admitted at the edge before any parse, because a re-encoded body respells floats, key order, and escapes and signs a document the provider never sent, and the octets travel onward untouched. The HMAC dialects route `crypt/sign`'s `Crypto.matches` `Mac` probe; the asymmetric dialects route `@oslojs/crypto`'s verify-only public-key surface, with the `PublicKey` tagged family carrying the SEC1/PKIX key-encoding axis and the dialect row carrying the `sigForm` signature-encoding axis, so a partner signing raw `r‖s` P1363 (the JWS ES256 wire form) and a partner shipping SPKI-DER keys both land as rows. Every verify runs under a store-backed `RateLimiter` keyed by dialect and presented key, every reject increments the dialect-tagged `security_verify_reject` counter, and the fold rides its span — inbound-attack telemetry is structural, not optional. `VerifyFault` instantiates the folder fault shape with the guard pair closed in both directions, folding a `crypt/sign` primitive fault to a caller-caused `malformed` at this seam so a bad presented signature is never a 500. Timestamp participation, candidate rotation, and the signed prefix are row grammar; tolerance, keys, and freshness are fold parameters a row cannot weaken, so admitting a dialect is review-free on the security axis. `Intake` is the typed `HttpApiMiddleware` spelling of the held-octets seam the runtime serve wave mounts.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                                           | [PUBLIC]           |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------- | :----------------- |
|  [01]   | `VERIFY_FAULT`  | the folder fault shape and the `crypt/sign` re-spell                                             | `VerifyFault`      |
|  [02]   | `DIALECT_TABLE` | the signing-convention rows and their header parse folds                                         | `Verify`           |
|  [03]   | `VERIFY_FOLD`   | the throttled constant-time verify pipeline, `Verified` receipt, key registry, intake middleware | `Verify`, `Intake` |

## [02]-[VERIFY_FAULT]

[VERIFY_FAULT]:
- Owner: `VerifyFault` — the folder fault shape for inbound authenticity: `missing` (required signature header absent), `malformed` (header grammar or signature encoding refused, or a `crypt/sign` primitive fault re-spelled here), `mismatch` (every candidate failed the constant-time compare), `stale` (timestamp outside tolerance), `unknownKey` (no registered key for the presented `kid`/issuer), `throttled` (the per-key verify budget exhausted, class `exhausted` so the edge renders `Retry-After`). Rows carry the core `FaultClass` kind and the guard pair closes the table in both directions.
- Law: a crypto-primitive fault is re-spelled at this seam — a `SignFault` from a malformed presented signature folds to `malformed` (caller-caused), never escapes as a `defect`; a genuine key or algorithm defect on Rasm's side stays a fold-internal `defect`.
- Law: verification is result-typed — a valid signature lands the `Verified` receipt, a failed one a typed fault; there is no boolean-plus-throw and a `false` compare is `mismatch`, never a thrown value.
- Growth: a new failure mode is one reason literal plus one class row.
- Packages: `effect` (`Schema`); `@rasm/ts/core` (`FaultClass`); `crypt/sign` (`SignFault`).

```typescript
import * as RateLimiter from "@effect/experimental/RateLimiter"
import { HttpApiMiddleware } from "@effect/platform"
import {
  decodeIEEEP1363ECDSASignature, decodePKIXECDSAPublicKey, decodePKIXECDSASignature, decodeSEC1ECDSAPublicKey,
  p256, p384, p521, verifyECDSASignature,
} from "@oslojs/crypto/ecdsa"
import { decodePKCS1RSAPublicKey, decodePKIXRSAPublicKey, SHA256ObjectIdentifier, verifyRSASSAPKCS1v15Signature, verifyRSASSASignature } from "@oslojs/crypto/rsa"
import { SHA256, sha256 } from "@oslojs/crypto/sha2"
import { decodeBase64, decodeHex, encodeHexLowerCase } from "@oslojs/encoding"
import { FaultClass } from "@rasm/ts/core"
import { Array, Config, Context, Data, DateTime, Duration, Effect, Either, Metric, Number, Option, Predicate, Redacted, Schema, pipe } from "effect"
import { Crypto, Probe, SignFault } from "./sign.ts"

const _reasons = ["missing", "malformed", "mismatch", "stale", "unknownKey", "throttled"] as const

const _faults = {
  missing: { class: "malformed" },
  malformed: { class: "malformed" },
  mismatch: { class: "denied" },
  stale: { class: "expired" },
  unknownKey: { class: "denied" },
  throttled: { class: "exhausted" },
} as const

declare namespace VerifyFault {
  type Reason = (typeof _reasons)[number]
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
  type _Closed<K extends Reason = keyof typeof _faults> = K
}

class VerifyFault extends Schema.TaggedError<VerifyFault>()("VerifyFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<verify:${this.reason}> ${this.detail}`
  }
}

const _respell = (fault: SignFault): VerifyFault => new VerifyFault({ reason: "malformed", detail: fault.detail })
```

## [03]-[DIALECT_TABLE]

[DIALECT_TABLE]:
- Owner: `_dialects` — one row per inbound signing convention, each carrying `header` (the signature header, lowercase), `scheme` (`"hmac"` symmetric or `"ecdsa"`/`"rsa-pkcs1"`/`"rsa-pss"` asymmetric), `sigForm` on the ECDSA rows (`"pkix"` DER or `"p1363"` raw `r‖s`), `parse` (header value to the candidate signature set plus the optional epoch-second stamp — `Option`-total, so any grammar refusal is one `malformed`), and `prefix` (the bytes prepended to the payload before signing — the `${t}.` stripe frame, empty elsewhere). The rows: `github` (`sha256=<hex>`, HMAC), `stripe` (`t=<epoch>,v1=<hex>` rotation candidates, HMAC), `hmacHex`/`hmacBase64` (bare digests, HMAC), `ecdsaPkix`/`ecdsaP1363` (`kid=<id>,sig=<base64>` ECDSA in either signature encoding), `rsaPss`/`rsaPkcs1` (`kid=<id>,sig=<base64>` RSA), `attestation` (raw base64 signature over the attestation object, ECDSA PKIX).
- Law: the candidate set is non-empty by parse — a row returning zero marks is a parse refusal, so the verify fold never runs an empty compare loop and "no signature" is `missing`/`malformed`, never a vacuous pass; a base64 decode refusal is `Option.none`, never an empty-array sentinel, so decode failure and an empty candidate set never conflate; stripe's every `v1` candidate is tried, so key-rotation windows verify.
- Law: rows are grammar, never trust policy — tolerance, secrets, and keys are verify-fold parameters a row cannot weaken; an asymmetric row resolves its registry key by the presented `kid`, or by the dialect name when the row carries none (`attestation`), and the scheme plus `sigForm` select the oslo decode and verify primitives.
- Law: the `_kinds` tuple anchors the key set — the `Verified.dialect` wire literal spreads it, and the guard pair closes tuple and table against each other in both directions, so a row without its tuple entry (or the converse) fails at the declaration.
- Growth: a new provider is one row plus its tuple entry; a provider changing grammar is a row edit every intake inherits; a new asymmetric suite (Ed25519 when a partner signs with it) is one row over the existing key-registry resolution.
- Packages: `@oslojs/crypto` (`decodeSEC1ECDSAPublicKey`/`decodePKIXECDSAPublicKey`, `decodePKIXECDSASignature`/`decodeIEEEP1363ECDSASignature`, `decodePKCS1RSAPublicKey`/`decodePKIXRSAPublicKey`, curve/OID rows); `@oslojs/encoding` (base64/hex decode).

```typescript
const _kinds = ["github", "stripe", "hmacHex", "hmacBase64", "ecdsaPkix", "ecdsaP1363", "rsaPss", "rsaPkcs1", "attestation"] as const

const _utf8 = new TextEncoder()
const _EMPTY = new Uint8Array(0)

declare namespace Verify {
  type Dialect = keyof typeof _dialects
  type Scheme = "hmac" | "ecdsa" | "rsa-pkcs1" | "rsa-pss"
  type SigForm = "pkix" | "p1363"
  type Parsed = { readonly marks: Array.NonEmptyReadonlyArray<string>; readonly kid: Option.Option<string>; readonly stamp: Option.Option<number> }
  type _Keys<K extends Dialect = (typeof _kinds)[number]> = K
  type _Kinds<K extends (typeof _kinds)[number] = Dialect> = K
}

const _pairs = (value: string): ReadonlyArray<readonly [string, string]> =>
  Array.filterMap(value.split(","), (part) => {
    const at = part.indexOf("=")
    return at <= 0 ? Option.none() : Option.some([part.slice(0, at).trim(), part.slice(at + 1)] as const)
  })

const _marked = (marks: ReadonlyArray<string>, kid: Option.Option<string>, stamp: Option.Option<number>): Option.Option<Verify.Parsed> =>
  Array.isNonEmptyReadonlyArray(marks) ? Option.some({ marks, kid, stamp }) : Option.none()

const _base64Hex = (value: string): Option.Option<string> =>
  Either.match(Either.try(() => decodeBase64(value)), {
    onLeft: () => Option.none(),
    onRight: (bytes) => Option.some(encodeHexLowerCase(bytes)),
  })

const _keyed = (value: string): Option.Option<Verify.Parsed> => {
  const pairs = _pairs(value)
  const kid = pipe(Array.findFirst(pairs, ([key]) => key === "kid"), Option.map(([, held]) => held))
  const marks = pipe(
    Array.findFirst(pairs, ([key]) => key === "sig"),
    Option.flatMap(([, held]) => _base64Hex(held)),
    Option.toArray,
  )
  return _marked(marks, kid, Option.none())
}

const _dialects = {
  github: {
    header: "x-hub-signature-256", scheme: "hmac",
    parse: (value: string) => _marked(value.startsWith("sha256=") ? [value.slice(7)] : [], Option.none(), Option.none()),
    prefix: () => _EMPTY,
  },
  stripe: {
    header: "stripe-signature", scheme: "hmac",
    parse: (value: string) => {
      const pairs = _pairs(value)
      const stamp = pipe(Array.findFirst(pairs, ([key]) => key === "t"), Option.flatMap(([, held]) => Number.parse(held)))
      return Option.isNone(stamp) ? Option.none()
        : _marked(Array.filterMap(pairs, ([key, held]) => (key === "v1" ? Option.some(held) : Option.none())), Option.none(), stamp)
    },
    prefix: (stamp: Option.Option<number>) => _utf8.encode(`${Option.getOrElse(stamp, () => 0)}.`),
  },
  hmacHex: { header: "x-signature", scheme: "hmac", parse: (value: string) => _marked([value], Option.none(), Option.none()), prefix: () => _EMPTY },
  hmacBase64: {
    header: "x-signature", scheme: "hmac",
    parse: (value: string) => Option.flatMap(_base64Hex(value), (hex) => _marked([hex], Option.none(), Option.none())),
    prefix: () => _EMPTY,
  },
  ecdsaPkix: { header: "x-signature-ecdsa", scheme: "ecdsa", sigForm: "pkix", parse: _keyed, prefix: () => _EMPTY },
  ecdsaP1363: { header: "x-signature-ecdsa-p1363", scheme: "ecdsa", sigForm: "p1363", parse: _keyed, prefix: () => _EMPTY },
  rsaPss: { header: "x-signature-rsa", scheme: "rsa-pss", parse: _keyed, prefix: () => _EMPTY },
  rsaPkcs1: { header: "x-signature-rsa", scheme: "rsa-pkcs1", parse: _keyed, prefix: () => _EMPTY },
  attestation: {
    header: "x-attestation-signature", scheme: "ecdsa", sigForm: "pkix",
    parse: (value: string) => Option.flatMap(_base64Hex(value), (hex) => _marked([hex], Option.none(), Option.none())),
    prefix: () => _EMPTY,
  },
} as const satisfies Record<string, {
  readonly header: string
  readonly scheme: Verify.Scheme
  readonly sigForm?: Verify.SigForm
  readonly parse: (value: string) => Option.Option<Verify.Parsed>
  readonly prefix: (stamp: Option.Option<number>) => Uint8Array
}>
```

## [04]-[VERIFY_FOLD]

[VERIFY_FOLD]:
- Owner: `Verify` — the assembled owner: `verify` runs a dialect over held octets against a resolved key into a `Verified` receipt under the per-key rate budget, and `PublicKeyStore` is the `Context.Tag` registry the asymmetric dialects resolve a partner or attestation public key from by `kid`. `PublicKey` is the tagged key family — `Ecdsa` carries `bytes`, the pinned `curve`, and the `encoding` axis (`sec1` raw point or `pkix` SPKI-DER), `Rsa` carries `bytes` and its `pkcs1`/`pkix` encoding — and `$match` drives the asymmetric dispatch, so a scheme/key family mismatch is the residue arm, never an if-ladder. `Intake` is the `HttpApiMiddleware` Tag the runtime serve wave implements over the raw request octets before any body parse.
- Law: the compare runs over the exact admitted bytes — the payload is the held request octets, the prefix rides the row, and freshness is checked before the signature (a stale stamp short-circuits to `stale` under the caller's tolerance `Duration`), so a replay outside the window never reaches the compare.
- Law: every verify is throttled — the fold body runs under `RateLimiter.makeWithRateLimiter` keyed `<dialect>:<kid|dialect>` with `onExceeded: "fail"`, `RateLimitExceeded` folds to `throttled`, and the store-backed limiter holds the budget across every app sharing the library; every fault increments `security_verify_reject` tagged by dialect and reason.
- Law: every asymmetric candidate resolves its key first — the registry key is the presented `kid` or the dialect name for a kid-less row, and a miss is `unknownKey`, never a silent skip; the ECDSA arm decodes SEC1 or PKIX keys over the `p256`/`p384`/`p521` roster the registry pins per key and PKIX-DER or IEEE-P1363 signatures per the row's `sigForm`; the RSA arm decodes PKCS1/PKIX keys and checks PKCS1-v1.5 or PSS with the SHA-256 OID; the oslo decoders throw on malformed DER, so the per-candidate verify runs inside `Either.try` and a candidate whose signature refuses to decode verifies `false` — a structurally garbage presented signature is `mismatch`, never a defect.
- Law: the HMAC fold tries every candidate under one `Crypto.matches` `Mac` probe and folds a primitive throw to `malformed`; a non-empty candidate set that matches none is `mismatch`.
- Receipt: `Verified` — the `dialect`, the resolved `kid` when asymmetric, and the verified octet length, so the admitting edge enqueues exactly what was verified; `verify` returns `Verified` or a `VerifyFault`, never a boolean.
- Growth: a new scheme is one `$match` arm; a new key roster is a registry row; the HMAC path never changes when an asymmetric row lands.
- Boundary: the edge holds the octets and lifts the header/signature into the fold; `crypt/sign` owns the HMAC compare and the SHA-256 primitive; the registry is satisfied by a config-sourced or fetched key set the composition root wires; the `RateLimiter` store is a data-wave-satisfied Layer; the runtime serve wave implements `Intake` and mounts it on ingress routes.
- Packages: `@oslojs/crypto` (verify + decode primitives, curve roster); `crypt/sign` (`Crypto.matches`); `@effect/experimental` (`RateLimiter`); `@effect/platform` (`HttpApiMiddleware`); `effect` (`Context`, `Schema`, `Effect`, `Metric`).

```typescript
type MacKey = Redacted.Redacted<Uint8Array>

type PublicKey = Data.TaggedEnum<{
  Ecdsa: { readonly bytes: Uint8Array; readonly curve: "p256" | "p384" | "p521"; readonly encoding: "sec1" | "pkix" }
  Rsa: { readonly bytes: Uint8Array; readonly encoding: "pkcs1" | "pkix" }
}>

const PublicKey = Data.taggedEnum<PublicKey>()

class Verified extends Schema.Class<Verified>("Verified")({
  dialect: Schema.Literal(..._kinds),
  kid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  length: Schema.NonNegativeInt,
}) {}

class PublicKeyStore extends Context.Tag("security/crypt/PublicKeyStore")<PublicKeyStore, {
  readonly byKid: (kid: string) => Effect.Effect<Option.Option<PublicKey>, VerifyFault>
}>() {}

class Intake extends HttpApiMiddleware.Tag<Intake>()("security/crypt/Intake", {
  failure: VerifyFault,
}) {}

const _CURVES = { p256, p384, p521 } as const

const _reject = Metric.counter("security_verify_reject")

const _sigForm = (row: (typeof _dialects)[Verify.Dialect]): Verify.SigForm =>
  Predicate.hasProperty(row, "sigForm") ? row.sigForm : "pkix"

const _fresh = (stamp: Option.Option<number>, tolerance: Duration.Duration): Effect.Effect<void, VerifyFault> =>
  Option.match(stamp, {
    onNone: () => Effect.void,
    onSome: (epoch) =>
      Effect.flatMap(DateTime.now, (now) =>
        Duration.lessThanOrEqualTo(Duration.millis(Math.abs(DateTime.toEpochMillis(now) - epoch * 1000)), tolerance)
          ? Effect.void
          : Effect.fail(new VerifyFault({ reason: "stale", detail: String(epoch) }))),
  })

const _verifyAsym = (scheme: Verify.Scheme, sigForm: Verify.SigForm, key: PublicKey, digest: Uint8Array, mark: string): boolean =>
  Either.getOrElse(
    Either.try(() => {
      const sig = decodeHex(mark)
      return PublicKey.$match(key, {
        Ecdsa: ({ bytes, curve, encoding }) =>
          scheme === "ecdsa"
          && verifyECDSASignature(
            encoding === "sec1" ? decodeSEC1ECDSAPublicKey(_CURVES[curve], bytes) : decodePKIXECDSAPublicKey(bytes, [_CURVES[curve]]),
            digest,
            sigForm === "p1363" ? decodeIEEEP1363ECDSASignature(_CURVES[curve], sig) : decodePKIXECDSASignature(sig),
          ),
        Rsa: ({ bytes, encoding }) =>
          (scheme === "rsa-pss" || scheme === "rsa-pkcs1")
          && (scheme === "rsa-pss"
            ? verifyRSASSASignature(encoding === "pkcs1" ? decodePKCS1RSAPublicKey(bytes) : decodePKIXRSAPublicKey(bytes), SHA256, SHA256, 32, digest, sig)
            : verifyRSASSAPKCS1v15Signature(encoding === "pkcs1" ? decodePKCS1RSAPublicKey(bytes) : decodePKIXRSAPublicKey(bytes), SHA256ObjectIdentifier, digest, sig)),
      })
    }),
    () => false,
  )

class Verify extends Effect.Service<Verify>()("security/crypt/Verify", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const keys = yield* PublicKeyStore
    const limit = yield* RateLimiter.makeWithRateLimiter
    const window = yield* Config.duration("VERIFY_RATE_WINDOW").pipe(Config.withDefault(Duration.minutes(1)))
    const budget = yield* Config.integer("VERIFY_RATE_LIMIT").pipe(Config.withDefault(60))
    const verify = (
      dialect: Verify.Dialect,
      octets: Uint8Array,
      header: Option.Option<string>,
      mac: Option.Option<MacKey>,
      tolerance: Duration.Duration,
    ): Effect.Effect<Verified, VerifyFault> =>
      Effect.gen(function* () {
        const row = _dialects[dialect]
        const raw = yield* Option.match(header, {
          onNone: () => Effect.fail(new VerifyFault({ reason: "missing", detail: row.header })),
          onSome: Effect.succeed,
        })
        const parsed = yield* Option.match(row.parse(raw), {
          onNone: () => Effect.fail(new VerifyFault({ reason: "malformed", detail: dialect })),
          onSome: Effect.succeed,
        })
        yield* _fresh(parsed.stamp, tolerance)
        const keyId = Option.getOrElse(parsed.kid, () => dialect)
        const payload = new Uint8Array([...row.prefix(parsed.stamp), ...octets])
        const matched = yield* limit({ algorithm: "token-bucket", onExceeded: "fail", window, limit: budget, key: `${dialect}:${keyId}` })(
          row.scheme === "hmac"
            ? Effect.flatMap(
                Option.match(mac, { onNone: () => Effect.fail(new VerifyFault({ reason: "malformed", detail: "hmac key absent" })), onSome: Effect.succeed }),
                (key) => Effect.map(
                  Effect.forEach(parsed.marks, (mark) => cipher.matches(Probe.Mac({ key, body: payload, signature: mark })).pipe(Effect.mapError(_respell))),
                  (results) => Array.contains(results, true),
                ),
              )
            : Effect.gen(function* () {
                const key = yield* Effect.flatMap(keys.byKid(keyId), Option.match({
                  onNone: () => Effect.fail(new VerifyFault({ reason: "unknownKey", detail: keyId })),
                  onSome: Effect.succeed,
                }))
                const digest = sha256(payload)
                return Array.some(parsed.marks, (mark) => _verifyAsym(row.scheme, _sigForm(row), key, digest, mark))
              }),
        ).pipe(Effect.catchTags({
          RateLimitExceeded: () => Effect.fail(new VerifyFault({ reason: "throttled", detail: `${dialect}:${keyId}` })),
          RateLimitStoreError: (error) => Effect.fail(new VerifyFault({ reason: "throttled", detail: String(error) })),
        }))
        return matched
          ? new Verified({ dialect, kid: parsed.kid, length: octets.byteLength })
          : yield* Effect.fail(new VerifyFault({ reason: "mismatch", detail: dialect }))
      }).pipe(
        Effect.tapError((fault) => Metric.increment(_reject.pipe(Metric.tagged("dialect", dialect), Metric.tagged("reason", fault.reason)))),
        Effect.withSpan("security.verify", { attributes: { dialect } }),
      )
    return { verify } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Intake, PublicKey, PublicKeyStore, Verified, Verify, VerifyFault }
export type { MacKey }
```

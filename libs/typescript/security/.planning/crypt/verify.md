# [SECURITY_VERIFY]

The external-signature ingress row: one closed dialect table carries every inbound authenticity convention — symmetric HMAC webhooks and asymmetric ECDSA/RSA partner and attestation signatures — and one verify fold runs any dialect over the HELD request octets, so a provider integration is a table row, never a bespoke verifier. The byte-identity law governs the whole page: verification computes over the exact bytes admitted at the edge before any parse, because a re-encoded body respells floats, key order, and escapes and signs a document the provider never sent, and the octets travel onward untouched. The HMAC dialects route `crypt/sign`'s `Crypto.compare`; the asymmetric dialects route `@oslojs/crypto`'s verify-only public-key surface — `verifyECDSASignature`/`verifyRSASSAPKCS1v15Signature`/`verifyRSASSAPSSSignature` decode a partner or attestation key and check a signature Rasm never minted, the row that closes the entire inbound trust surface HMAC alone cannot reach. `VerifyFault` instantiates the folder fault shape, folding a `crypt/sign` primitive fault to a caller-caused `malformed` at this seam so a bad presented signature is never a 500. Timestamp participation, candidate rotation, and the signed prefix are row grammar; tolerance, keys, and freshness are fold parameters a row cannot weaken, so admitting a dialect is review-free on the security axis.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                              | [PUBLIC]      |
| :-----: | :---------------- | :-------------------------------------------------------------------- | :------------ |
|  [01]   | `VERIFY_FAULT`    | the folder fault shape and the `crypt/sign` re-spell                  | `VerifyFault` |
|  [02]   | `DIALECT_TABLE`   | the signing-convention rows and their header parse folds             | `Verify`      |
|  [03]   | `VERIFY_FOLD`     | the constant-time verify pipeline, `Verified` receipt, key registry  | `Verify`      |

## [2]-[VERIFY_FAULT]

[VERIFY_FAULT]:
- Owner: `VerifyFault` — the folder fault shape for inbound authenticity: `missing` (required signature header absent), `malformed` (header grammar or signature encoding refused, or a `crypt/sign` primitive fault re-spelled here), `mismatch` (every candidate failed the constant-time compare), `stale` (timestamp outside tolerance), `unknownKey` (no registered key for the presented `kid`/issuer). Rows carry the core `FaultClass` kind, so status, blame, and retryability derive from the branch table.
- Law: a crypto-primitive fault is re-spelled at this seam — a `SignFault` from a malformed presented signature folds to `malformed` (caller-caused, class `malformed`), never escapes as a `defect` — the point-of-knowledge re-spell the carrier law demands; a genuine key or algorithm defect on Rasm's side stays a fold-internal `defect`.
- Law: verification is result-typed — a valid signature lands the `Verified` receipt, a failed one a typed fault; there is no boolean-plus-throw and a `false` compare is `mismatch`, never a thrown value.
- Growth: a new failure mode is one reason literal plus one class row.
- Packages: `effect` (`Schema`); `@rasm/ts/core` (`FaultClass`); `crypt/sign` (`SignFault`).

```typescript
import { decodePKIXECDSASignature, decodeSEC1ECDSAPublicKey, p256, p384, p521, verifyECDSASignature } from "@oslojs/crypto/ecdsa"
import { decodePKCS1RSAPublicKey, decodePKIXRSAPublicKey, SHA256ObjectIdentifier, verifyRSASSAPKCS1v15Signature, verifyRSASSASignature } from "@oslojs/crypto/rsa"
import { SHA256, sha256 } from "@oslojs/crypto/sha2"
import { decodeBase64, decodeHex, encodeHexLowerCase } from "@oslojs/encoding"
import { FaultClass } from "@rasm/ts/core"
import { Array, DateTime, Duration, Effect, Either, HashMap, Number, Option, Predicate, Redacted, Schema, pipe } from "effect"
import { Crypto, SignFault } from "./sign.ts"

const _reasons = ["missing", "malformed", "mismatch", "stale", "unknownKey"] as const

const _faults = {
  missing: { class: "malformed" },
  malformed: { class: "malformed" },
  mismatch: { class: "denied" },
  stale: { class: "expired" },
  unknownKey: { class: "denied" },
} as const

declare namespace VerifyFault {
  type Reason = keyof typeof _faults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
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

## [3]-[DIALECT_TABLE]

[DIALECT_TABLE]:
- Owner: `_dialects` — one row per inbound signing convention, each carrying `header` (the signature header, lowercase), `scheme` (`"hmac"` symmetric or `"ecdsa"`/`"rsa-pkcs1"`/`"rsa-pss"` asymmetric), `parse` (header value to the candidate signature set plus the optional epoch-second stamp — `Option`-total, so any grammar refusal is one `malformed`), and `prefix` (the bytes prepended to the payload before signing — the `${t}.` stripe frame, empty elsewhere). The rows: `github` (`sha256=<hex>`, HMAC), `stripe` (`t=<epoch>,v1=<hex>` rotation candidates, HMAC), `hmacHex`/`hmacBase64` (bare digests, HMAC), `ecdsaPkix` (`kid=<id>,sig=<base64>` PKIX-encoded ECDSA), `rsaPss`/`rsaPkcs1` (`kid=<id>,sig=<base64>` RSA), `attestation` (raw base64 signature over the attestation object, ECDSA).
- Law: the candidate set is non-empty by parse — a row returning zero marks is a parse refusal, so the verify fold never runs an empty compare loop and "no signature" is `missing`/`malformed`, never a vacuous pass; stripe's every `v1` candidate is tried, so key-rotation windows verify.
- Law: rows are grammar, never trust policy — tolerance, secrets, and keys are verify-fold parameters a row cannot weaken; asymmetric rows carry a `kid` field the fold resolves against the key registry, and the scheme selects the oslo verify primitive and its curve/hash roster.
- Law: the `_kinds` tuple anchors the key set — the `Verified.dialect` wire literal spreads it, and the guard pair closes tuple and table against each other in both directions, so a row without its tuple entry (or the converse) fails at the declaration.
- Growth: a new provider is one row plus its tuple entry; a provider changing grammar is a row edit every intake inherits; a new asymmetric suite (Ed25519 when a partner signs with it) is one row over the existing key-registry resolution.
- Packages: `@oslojs/crypto` (`decodeSEC1ECDSAPublicKey`, `decodePKIXECDSASignature`, `decodePKCS1RSAPublicKey`/`decodePKIXRSAPublicKey`, curve/OID rows); `@oslojs/encoding` (base64/hex decode).

```typescript
const _kinds = ["github", "stripe", "hmacHex", "hmacBase64", "ecdsaPkix", "rsaPss", "rsaPkcs1", "attestation"] as const

const _utf8 = new TextEncoder()
const _EMPTY = new Uint8Array(0)

declare namespace Verify {
  type Dialect = keyof typeof _dialects
  type Scheme = "hmac" | "ecdsa" | "rsa-pkcs1" | "rsa-pss"
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

const _base64Hex = (value: string): ReadonlyArray<string> =>
  Either.match(decodeBase64(value), { onLeft: () => [], onRight: (bytes) => [encodeHexLowerCase(bytes)] })

const _keyed = (value: string, decodeSig: (raw: string) => ReadonlyArray<string>): Option.Option<Verify.Parsed> => {
  const pairs = _pairs(value)
  const kid = pipe(Array.findFirst(pairs, ([key]) => key === "kid"), Option.map(([, held]) => held))
  const marks = pipe(Array.findFirst(pairs, ([key]) => key === "sig"), Option.match({ onNone: () => [], onSome: ([, held]) => decodeSig(held) }))
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
  hmacBase64: { header: "x-signature", scheme: "hmac", parse: (value: string) => _marked(_base64Hex(value), Option.none(), Option.none()), prefix: () => _EMPTY },
  ecdsaPkix: { header: "x-signature-ecdsa", scheme: "ecdsa", parse: (value: string) => _keyed(value, (raw) => _base64Hex(raw)), prefix: () => _EMPTY },
  rsaPss: { header: "x-signature-rsa", scheme: "rsa-pss", parse: (value: string) => _keyed(value, (raw) => _base64Hex(raw)), prefix: () => _EMPTY },
  rsaPkcs1: { header: "x-signature-rsa", scheme: "rsa-pkcs1", parse: (value: string) => _keyed(value, (raw) => _base64Hex(raw)), prefix: () => _EMPTY },
  attestation: {
    header: "x-attestation-signature", scheme: "ecdsa",
    parse: (value: string) => _marked(_base64Hex(value), Option.none(), Option.none()),
    prefix: () => _EMPTY,
  },
} as const satisfies Record<string, { readonly header: string; readonly scheme: Verify.Scheme; readonly parse: (value: string) => Option.Option<Verify.Parsed>; readonly prefix: (stamp: Option.Option<number>) => Uint8Array }>
```

## [4]-[VERIFY_FOLD]

[VERIFY_FOLD]:
- Owner: `Verify` — the assembled owner: `verify` runs a dialect over held octets against a resolved key into a `Verified` receipt, and `PublicKeyStore` is the `Context.Tag` registry the asymmetric dialects resolve a partner or attestation public key from by `kid`. The HMAC schemes route `Crypto.compare` over the `MacKey` the caller passes; the asymmetric schemes hash the prefixed payload with SHA-256 and check the decoded signature against the registered public key through the oslo verify primitive the scheme selects.
- Law: the compare runs over the exact admitted bytes — the payload is the held request octets, the prefix rides the row, and freshness is checked before the signature (a stale stamp short-circuits to `stale` under the caller's tolerance `Duration`), so a replay outside the window never reaches the compare.
- Law: every asymmetric candidate resolves its key first — a presented `kid` with no registered key is `unknownKey` (class `denied`), never a silent skip; the ECDSA scheme decodes SEC1 keys and PKIX signatures over the `p256`/`p384`/`p521` roster the registry pins per key, the RSA schemes decode PKCS1/PKIX keys and check PKCS1-v1.5 or PSS with the SHA-256 OID.
- Law: the HMAC fold tries every candidate under one `Crypto.compare` and folds a primitive throw to `malformed`; a non-empty candidate set that matches none is `mismatch`.
- Receipt: `Verified` — the `dialect`, the resolved `kid` when asymmetric, and the verified octet length, so the admitting edge enqueues exactly what was verified; `verify` returns `Verified` or a `VerifyFault`, never a boolean.
- Growth: a new scheme is one `_verifyAsym` arm; a new key roster is a registry row; the HMAC path never changes when an asymmetric row lands.
- Boundary: the edge holds the octets and lifts the header/signature into the fold; `crypt/sign` owns the HMAC compare and the SHA-256 primitive; the registry is satisfied by a config-sourced or fetched key set the composition root wires; the OTP/OAuth pages never touch this — it is the inbound-only trust surface.
- Packages: `@oslojs/crypto` (verify + decode primitives, curve roster); `crypt/sign` (`Crypto.compare`); `effect` (`Context`, `Schema`, `Effect`).

```typescript
type MacKey = Redacted.Redacted<Uint8Array>
type PublicKey =
  | { readonly scheme: "ecdsa"; readonly sec1: Uint8Array; readonly curve: "p256" | "p384" | "p521" }
  | { readonly scheme: "rsa"; readonly key: Uint8Array; readonly encoding: "pkcs1" | "pkix" }

class Verified extends Schema.Class<Verified>("Verified")({
  dialect: Schema.Literal(..._kinds),
  kid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  length: Schema.NonNegativeInt,
}) {}

class PublicKeyStore extends Context.Tag("security/crypt/PublicKeyStore")<PublicKeyStore, {
  readonly byKid: (kid: string) => Effect.Effect<Option.Option<PublicKey>, VerifyFault>
}>() {}

const _CURVES = { p256, p384, p521 } as const

const _fresh = (stamp: Option.Option<number>, tolerance: Duration.Duration): Effect.Effect<void, VerifyFault> =>
  Option.match(stamp, {
    onNone: () => Effect.void,
    onSome: (epoch) =>
      Effect.flatMap(DateTime.now, (now) =>
        Duration.lessThanOrEqualTo(Duration.millis(Math.abs(DateTime.toEpochMillis(now) - epoch * 1000)), tolerance)
          ? Effect.void
          : Effect.fail(new VerifyFault({ reason: "stale", detail: String(epoch) }))),
  })

const _verifyAsym = (scheme: Verify.Scheme, key: PublicKey, digest: Uint8Array, mark: string): boolean => {
  const sig = decodeHex(mark)
  if (scheme === "ecdsa" && key.scheme === "ecdsa") {
    return verifyECDSASignature(decodeSEC1ECDSAPublicKey(_CURVES[key.curve], key.sec1), digest, decodePKIXECDSASignature(sig))
  }
  if (key.scheme === "rsa") {
    const pub = key.encoding === "pkcs1" ? decodePKCS1RSAPublicKey(key.key) : decodePKIXRSAPublicKey(key.key)
    return scheme === "rsa-pss"
      ? verifyRSASSASignature(pub, SHA256, SHA256, 32, digest, sig)
      : verifyRSASSAPKCS1v15Signature(pub, SHA256ObjectIdentifier, digest, sig)
  }
  return false
}

class Verify extends Effect.Service<Verify>()("security/crypt/Verify", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const keys = yield* PublicKeyStore
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
        const payload = new Uint8Array([...row.prefix(parsed.stamp), ...octets])
        const matched = yield* row.scheme === "hmac"
          ? Effect.flatMap(
              Option.match(mac, { onNone: () => Effect.fail(new VerifyFault({ reason: "malformed", detail: "hmac key absent" })), onSome: Effect.succeed }),
              (key) => Effect.map(
                Effect.forEach(parsed.marks, (mark) => cipher.compare(key, payload, mark).pipe(Effect.mapError(_respell))),
                (results) => Array.contains(results, true),
              ),
            )
          : Effect.gen(function* () {
              const kid = yield* Option.match(parsed.kid, { onNone: () => Effect.succeed(Option.none<string>()), onSome: (k) => Effect.succeed(Option.some(k)) })
              const key = yield* Effect.flatMap(
                Option.match(kid, { onNone: () => Effect.succeed(Option.none<PublicKey>()), onSome: keys.byKid }),
                Option.match({ onNone: () => Effect.fail(new VerifyFault({ reason: "unknownKey", detail: Option.getOrElse(kid, () => dialect) })), onSome: Effect.succeed }),
              )
              const digest = sha256(payload)
              return Array.some(parsed.marks, (mark) => _verifyAsym(row.scheme, key, digest, mark))
            })
        return matched
          ? new Verified({ dialect, kid: parsed.kid, length: octets.byteLength })
          : yield* Effect.fail(new VerifyFault({ reason: "mismatch", detail: dialect }))
      })
    return { verify } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { PublicKeyStore, Verified, Verify, VerifyFault }
export type { MacKey, PublicKey }
```

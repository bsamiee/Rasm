# [SECURITY_SIGN]

One crypto authority: argon2id credential digest-at-rest under a semaphore bulkhead, HMAC egress signing, opaque-token minting, the AES-GCM crypto-shredding `Shredder`, jose key-material admission with RFC 7638 thumbprint identity, and the JWT/JWS/JWKS/JWE token authority — one module because every concern shares one key plane and one fault family. Key material enters exactly once: the core-decoded `Credential` landing folds through `Material.admit` into a `Redacted<CryptoKey>` `KeyHandle`, the JWK body decodes once through `Schema.parseJson` at the same seam, the `kid` is the wire fingerprint or the computed thumbprint, and the validity window is enforced at admission — no second import path exists for a Doppler-fetched, wire-carried, or self-minted key. `Jwt` mints with the active ring key, verifies against the local JWKS or a remote per-issuer resolver through one overloaded `verify` discriminating on the issuer descriptor, keeps the remote cache warm with a `Schedule`-driven proactive `reload`, bounds every remote resolve with a deadline and a jittered retry gated on `FaultClass.retryable`, and seals confidential claims as JWE. Every secret — pepper, password, data key, minted token, private handle — is `Redacted` from admission and unwraps only into the primitive call; algorithm, cost, permit budget, cache age, and reason are vocabulary rows or `Config` policy values, never call-site knobs. Cost rows are bench-graded, never copied folklore: `Calibration` measures every `CryptoCost` row on the executing host into core `Claim` receipts — the `BenchmarkClaimWire` family with foreign-host admission — graded against per-class latency ceilings that land on the KDF timer's own bucket bounds. Every crypto surface rides its span and metric at the declaration seam — KDF latency, JWKS resolve latency, cold-miss and quarantine counters, each instrument minted from its core `Convention.instrument` row — so the runtime wave's OTLP lane exports the folder's audit stream with zero call-site change. `SignFault` is the folder's canonical fault shape: one reason family whose rows carry the core `FaultClass` classification, closed in both directions by the guard pair, so retryability, dominance, and blame derive from the branch table and the serving edge folds `class` to status through its own governed record.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [PUBLIC]                               |
| :-----: | :----------------- | :------------------------------------- |
|  [01]   | `FAULT_AND_ALG`    | `SignFault`, `KeyAlg`                  |
|  [02]   | `KEY_MATERIAL`     | `Material`                             |
|  [03]   | `CRYPTO_PRIMITIVE` | `Crypto`, `CredentialVerdict`, `Probe` |
|  [04]   | `SHREDDER`         | `Shredder`                             |
|  [05]   | `TOKEN_AUTHORITY`  | `Jwt`, `AccessClaims`                  |
|  [06]   | `CALIBRATION`      | `Calibration`                          |

## [02]-[FAULT_AND_ALG]

[FAULT_AND_ALG]:
- Owner: `SignFault` — the one reason-discriminated `Schema.TaggedError` every page in this folder instantiates with its own reason set; each row carries the core `FaultClass` kind, `get class()` projects it so `FaultClass.of` classifies structurally, and `override get message()` derives from fields. `KeyAlg` is the bounded signature-scheme vocabulary — each row carries `{ kty, crv?, use }`, the discriminant derives through `keyof typeof`, and a new scheme is one row.
- Law: rows carry `class` only — rank, retryability, and blame derive from the branch `FaultClass` table, and the class-to-status projection is the serving edge's governed record; a local `{ rank, retry, status }` triple beside the class column is the split-brain this shape kills.
- Law: the guard pair closes every vocabulary in both directions — `_Rows` proves every reason has a row, `_Closed` proves every row has a reason, and `_Keys`/`_Kinds` do the same for `KeyAlg`; a dead row or a rowless reason fails at the declaration, and every sibling fault family carries the identical pair.
- Law: a `false` argon2 verify, a rejected OTP, and a rotated-out token are verdict arms, never faults — `SignFault` fires only when a primitive throws, a key refuses import, a load-shed sheds, or a token fails a trust gate.
- Growth: a new failure mode is one reason literal and one class row; a new signature scheme is one `KeyAlg` row that `Material`, `Jwt`, and the external-verify page inherit unchanged.
- Packages: `effect` (`Schema`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { Algorithm, hash, hashRaw, Version, verify, type Options } from "@node-rs/argon2"
import { hmac } from "@oslojs/crypto/hmac"
import { type RandomReader, generateRandomString } from "@oslojs/crypto/random"
import { SHA1 } from "@oslojs/crypto/sha1"
import { SHA256, SHA512, sha256 } from "@oslojs/crypto/sha2"
import { constantTimeEqual } from "@oslojs/crypto/subtle"
import { decodeBase32, decodeHex, encodeBase32UpperCaseNoPadding, encodeHexLowerCase } from "@oslojs/encoding"
import { Claim, Convention, Credential, FaultClass, type AppIdentity, type WireFault } from "@rasm/ts/core"
import {
  calculateJwkThumbprint, calculateJwkThumbprintUri, createLocalJWKSet, createRemoteJWKSet, EncryptJWT, exportJWK,
  generateKeyPair, importJWK, importPKCS8, importSPKI, importX509, jwtDecrypt, jwtVerify, SignJWT, customFetch, jwksCache,
  type ExportedJWKSCache, type JSONWebKeySet, type JWK, type JWTPayload,
} from "jose"
import {
  Array, Clock, Config, Context, Data, DateTime, Duration, Effect, HashMap, Layer, Metric, Number, Option, Order, Predicate, Redacted,
  Ref, Schedule, Schema, Struct, pipe,
} from "effect"
import { SecurityFact, Witness } from "../access/audit.ts"

const _reasons = [
  "digest", "mac", "rng", "seal", "open", "wrap", "throttled",
  "material", "unsupported", "window",
  "expired", "claim", "signature", "algorithm", "jwks", "malformed",
] as const

const _faults = {
  digest: { class: "defect" },
  mac: { class: "defect" },
  rng: { class: "defect" },
  seal: { class: "defect" },
  open: { class: "breached" },
  wrap: { class: "breached" },
  throttled: { class: "exhausted" },
  material: { class: "malformed" },
  unsupported: { class: "invalid" },
  window: { class: "expired" },
  expired: { class: "expired" },
  claim: { class: "denied" },
  signature: { class: "denied" },
  algorithm: { class: "denied" },
  jwks: { class: "unavailable" },
  malformed: { class: "malformed" },
} as const

const _algs = ["ES256", "ES384", "RS256", "EdDSA"] as const

const KeyAlg = {
  ES256: { kty: "EC", crv: "P-256", use: "sig" },
  ES384: { kty: "EC", crv: "P-384", use: "sig" },
  RS256: { kty: "RSA", use: "sig" },
  EdDSA: { kty: "OKP", crv: "Ed25519", use: "sig" },
} as const

declare namespace SignFault {
  type Reason = (typeof _reasons)[number]
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
  type _Closed<K extends Reason = keyof typeof _faults> = K
}

declare namespace KeyAlg {
  type Kind = keyof typeof KeyAlg
  type Row = (typeof KeyAlg)[Kind]
  type _Keys<K extends Kind = (typeof _algs)[number]> = K
  type _Kinds<K extends (typeof _algs)[number] = Kind> = K
}

class SignFault extends Schema.TaggedError<SignFault>()("SignFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<sign:${this.reason}> ${this.detail}`
  }
}
```

## [03]-[KEY_MATERIAL]

[KEY_MATERIAL]:
- Owner: `Material` — the assembled key-material owner: `admit` folds one core `Credential` landing into a `KeyHandle`, `mint` self-issues an ephemeral non-extractable ring for a KMS-less bootstrap or test composition, `ring` narrows a signing credential and a published JWKS into the `{ active, verify }` set `Jwt` consumes, `jwks` projects the verify handles back to a `JSONWebKeySet` for publication, and `thumbprint`/`thumbprintUri` are the RFC 7638 identity mints — the URI form is the stable `cnf.jkt` subject for a key-bound principal. Its `Credential` landing arrives sealed from the core interchange decode — `material` is `Redacted` from the wire, `fingerprint` is the only audit identity — and this owner is its terminus: the handle never crosses back to a wire and never reaches a log.
- Law: admission is one polymorphic fold — the PEM header sniffs the jose importer (`importPKCS8` private, `importSPKI`/`importX509` public), a JSON body decodes exactly once through the `_Jwk` `Schema.parseJson` owner and routes `importJWK`, the private discriminant is the decoded `d` field — never a re-parse — the private side lands `Signing` and the public side `Verify`, and there is no `admitSigning`/`admitVerify` twin; a symmetric `importJWK` result is `unsupported`.
- Law: the validity window is enforced at admission — an instant outside `[notBefore, notAfter]` is `SignFault.window`; rotation is re-admission of a fresh `Credential`, and `Credential.rotated` is the sealed compare the custodian already carries.
- Law: `ring` accumulates — `Effect.partition` admits every satisfying published key and quarantines each malformed entry onto the `Convention.instrument.securityJwksQuarantined` counter, a warning log, and an `Admission` fact through `Witness`, so one rotated-in bad key never collapses the verify set and the quarantine is receipt-truth; an empty surviving set is the only `material` failure, and the synthetic verify carrier's horizon is the caller's policy `Duration`, never a buried literal.
- Growth: a new signature scheme is one `KeyAlg` row; a new material source (KMS, HSM) produces the same `Credential` value and terminates through the same `admit`; a detached-signature or co-signed-document surface is a `GeneralSign` row over the same handles.
- Boundary: `crypt/secret` constructs `Credential` values from fetched material; the core interchange codec decodes the C#-minted `CredentialPemWire` into the same class; `Jwt` is the only consumer that unwraps `Signing`, and the external-verify page consumes `Verify` handles only through `jwks`.
- Packages: `jose` (`importPKCS8`/`importSPKI`/`importX509`/`importJWK`, `exportJWK`, `generateKeyPair`, `calculateJwkThumbprint`/`calculateJwkThumbprintUri`); `@rasm/ts/core` (`Convention`, `Credential`); `access/audit` (`Witness`, `SecurityFact`).

```typescript
type KeyHandle = Data.TaggedEnum<{
  Signing: { readonly kid: string; readonly alg: KeyAlg.Kind; readonly key: Redacted.Redacted<CryptoKey> }
  Verify: { readonly kid: string; readonly alg: KeyAlg.Kind; readonly key: Redacted.Redacted<CryptoKey> }
}>

type Ring = {
  readonly active: Extract<KeyHandle, { readonly _tag: "Signing" }>
  readonly verify: ReadonlyArray<Extract<KeyHandle, { readonly _tag: "Verify" }>>
}

const _KeyHandle = Data.taggedEnum<KeyHandle>()

const _quarantined = Metric.counter(Convention.instrument.securityJwksQuarantined.name, {
  description: Convention.instrument.securityJwksQuarantined.description,
  incremental: true,
})

const _material = (cause: unknown): SignFault => new SignFault({ reason: "material", detail: String(cause) })

const _Jwk = Schema.parseJson(Schema.Struct(
  { kty: Schema.String, d: Schema.optional(Schema.String) },
  { key: Schema.String, value: Schema.Unknown },
))

const _sniff = (pem: string): "pkcs8" | "spki" | "x509" | "jwk" =>
  pem.startsWith("-----BEGIN PRIVATE KEY-----") ? "pkcs8"
    : pem.startsWith("-----BEGIN PUBLIC KEY-----") ? "spki"
    : pem.startsWith("-----BEGIN CERTIFICATE-----") ? "x509"
    : "jwk"

const _windowed = (credential: Credential): Effect.Effect<void, SignFault> =>
  Effect.flatMap(DateTime.now, (now) =>
    DateTime.between(now, { minimum: credential.notBefore, maximum: credential.notAfter })
      ? Effect.void
      : Effect.fail(new SignFault({ reason: "window", detail: credential.fingerprint })))

const _admit = (credential: Credential, alg: KeyAlg.Kind): Effect.Effect<KeyHandle, SignFault> =>
  Effect.gen(function* () {
    yield* _windowed(credential)
    const pem = Redacted.value(credential.material)
    const format = _sniff(pem)
    const body = format === "jwk"
      ? Option.some(yield* Schema.decodeUnknown(_Jwk)(pem).pipe(Effect.mapError(_material)))
      : Option.none<Schema.Schema.Type<typeof _Jwk>>()
    const key = yield* Option.match(body, {
      onSome: (jwk) => Effect.tryPromise({ try: () => importJWK(jwk, alg), catch: _material }),
      onNone: () =>
        Effect.tryPromise({
          try: () =>
            format === "pkcs8" ? importPKCS8(pem, alg)
              : format === "spki" ? importSPKI(pem, alg)
              : importX509(pem, alg),
          catch: _material,
        }),
    }).pipe(Effect.filterOrFail(
      (held): held is CryptoKey => !(held instanceof Uint8Array),
      () => new SignFault({ reason: "unsupported", detail: "symmetric jwk material" }),
    ))
    const secret = format === "pkcs8" || Option.exists(body, (jwk) => jwk.d !== undefined)
    const held = Redacted.make(key)
    return secret && credential.kind === "signing"
      ? _KeyHandle.Signing({ kid: credential.fingerprint, alg, key: held })
      : _KeyHandle.Verify({ kid: credential.fingerprint, alg, key: held })
  })

const Material = {
  admit: _admit,
  mint: (alg: KeyAlg.Kind): Effect.Effect<Ring, SignFault> =>
    Effect.gen(function* () {
      const pair = yield* Effect.tryPromise({ try: () => generateKeyPair(alg, { extractable: false }), catch: _material })
      const jwk = yield* Effect.tryPromise({ try: () => exportJWK(pair.publicKey), catch: _material })
      const kid = yield* Material.thumbprint(jwk)
      return {
        active: _KeyHandle.Signing({ kid, alg, key: Redacted.make(pair.privateKey) }),
        verify: [_KeyHandle.Verify({ kid, alg, key: Redacted.make(pair.publicKey) })],
      }
    }),
  thumbprint: (jwk: JWK): Effect.Effect<string, SignFault> =>
    Effect.tryPromise({ try: () => calculateJwkThumbprint(jwk, "sha256"), catch: _material }),
  thumbprintUri: (jwk: JWK): Effect.Effect<string, SignFault> =>
    Effect.tryPromise({ try: () => calculateJwkThumbprintUri(jwk), catch: _material }),
  jwks: (keys: ReadonlyArray<Extract<KeyHandle, { readonly _tag: "Verify" }>>): Effect.Effect<JSONWebKeySet, SignFault> =>
    Effect.map(
      Effect.forEach(keys, (handle) =>
        Effect.tryPromise({
          try: async () => ({ ...(await exportJWK(Redacted.value(handle.key))), kid: handle.kid, alg: handle.alg, use: "sig" }),
          catch: _material,
        })),
      (list) => ({ keys: Array.fromIterable(list) }),
    ),
  ring: (signing: Credential, alg: KeyAlg.Kind, published: JSONWebKeySet, horizon: Duration.DurationInput): Effect.Effect<Ring, SignFault> =>
    Effect.gen(function* () {
      const active = yield* _admit(signing, alg).pipe(Effect.filterOrFail(
        (handle): handle is Extract<KeyHandle, { readonly _tag: "Signing" }> => handle._tag === "Signing",
        () => new SignFault({ reason: "material", detail: "signing credential resolved public" }),
      ))
      const [excluded, verify] = yield* Effect.partition(published.keys, (jwk) =>
        Effect.gen(function* () {
          const kid = yield* Option.match(Option.fromNullable(jwk.kid), {
            onSome: Effect.succeed,
            onNone: () => Material.thumbprint(jwk),
          })
          const scheme = yield* Schema.decodeUnknown(Schema.Literal(..._algs))(jwk.alg ?? alg).pipe(
            Effect.mapError(() => new SignFault({ reason: "unsupported", detail: String(jwk.alg) })))
          const now = yield* DateTime.now
          const carrier = new Credential({
            kind: "verify", material: Redacted.make(JSON.stringify(jwk)), fingerprint: kid,
            notBefore: now, notAfter: DateTime.addDuration(now, Duration.decode(horizon)),
          })
          return yield* _admit(carrier, scheme).pipe(Effect.filterOrFail(
            (handle): handle is Extract<KeyHandle, { readonly _tag: "Verify" }> => handle._tag === "Verify",
            () => new SignFault({ reason: "material", detail: "jwks entry resolved private" }),
          ))
        }))
      yield* Effect.forEach(excluded, (fault) =>
        Effect.zipRight(
          Effect.zipRight(Metric.increment(_quarantined), Effect.logWarning("jwks entry quarantined", fault)),
          Witness.publish(SecurityFact.Admission({ kid: Option.none(), detail: fault.detail })),
        ), { discard: true })
      return yield* Array.isNonEmptyReadonlyArray(verify)
        ? Effect.succeed<Ring>({ active, verify })
        : Effect.fail(new SignFault({ reason: "material", detail: "empty verify set" }))
    }),
} as const
```

## [04]-[CRYPTO_PRIMITIVE]

[CRYPTO_PRIMITIVE]:
- Owner: `Crypto` — `digest`/`verify` own argon2id credential-at-rest with the `CredentialVerdict` receipt, `derive` is the raw-KDF row minting deterministic key bytes from a passphrase, `sign` owns HMAC-SHA256 egress signing rendered hex, `matches` is the one constant-time comparison entrypoint discriminating on the `Probe` case — `Mac` (HMAC-over-body), `Digest` (SHA-256 fingerprint), `Text` (raw string) — `token` mints opaque high-entropy material over the WebCrypto-filled `RandomReader`, `uuid` mints a v4 identifier from the same reader so id minting is test-injectable, `fingerprint` is the SHA-256 hex projection for high-entropy token lookup, and `plugin`/`base32` are the otplib ports over these same primitives.
- Law: every KDF call runs inside the semaphore bulkhead — `login`/`apiKey` rows take one permit, the `kek` derive takes the whole budget, so a login storm queues at the `CRYPTO_KDF_PERMITS` bound instead of spawning unbounded 19–64MB hashes; each call rides the `Convention.instrument.securityKdf` timer and its span, and the fiber's interrupt threads the `AbortSignal` so a request-scoped hash cancels with its caller.
- Law: cost is a named `CryptoCost` row selected by credential class — `login` interactive, `apiKey` machine, `kek` the derive row backing the `Shredder` master key — with `Argon2id`+`V0x13` pinned; the pepper is one `Config.redacted` injected at construction and threaded as `secret`.
- Law: `verify` reads the PHC-embedded parameters, and a match under stale parameters returns `Matched({ stale: true })` — the rehash signal the caller persists on; `Rejected` is the ordinary auth-fail arm and only a malformed stored digest throws into `SignFault.digest`.
- Law: every compare routes constant-time through one `matches` — length is the only short-circuit, a length mismatch is `false`, a malformed stored hex is `SignFault.mac`, never an uncaught throw; a stored argon2 digest is checked by argon2's own constant-time `verify` and never re-compared through `constantTimeEqual`; the otplib `hmac` port dispatches the `HashAlgorithm` value off the `_HASHES` row table so a new hash is a row, never a name fork.
- Growth: a new credential class is one `CryptoCost` row; a cost bump is a row edit the rehash fold detects on the next successful verify; a new comparison shape is one `Probe` case.
- Boundary: `authn/credential` delegates every digest-at-rest here; `authn/session` consumes `token`/`uuid`/`fingerprint`/`matches`; `crypt/verify` composes `matches` under its dialect grammar; no sibling imports `@node-rs/argon2` or `@oslojs/*` directly.
- Packages: `@node-rs/argon2` (`hash`/`hashRaw`/`verify`, `Algorithm`, `Version`); `@oslojs/crypto` (`hmac`, `SHA1`/`SHA256`/`SHA512`, `sha256`, `constantTimeEqual`, `generateRandomString`, `RandomReader`); `@oslojs/encoding` (hex + base32 rows); `effect` (`Effect.makeSemaphore`, `Metric`); `@rasm/ts/core` (`Convention`).

```typescript
type CredentialVerdict = Data.TaggedEnum<{
  Matched: { readonly stale: boolean }
  Rejected: {}
}>

type Probe = Data.TaggedEnum<{
  Mac: { readonly key: Redacted.Redacted<Uint8Array>; readonly body: Uint8Array; readonly signature: string }
  Digest: { readonly opaque: Redacted.Redacted<string>; readonly stored: string }
  Text: { readonly held: Redacted.Redacted<string>; readonly presented: string }
}>

const CryptoCost = {
  login: {
    targetMs: 250,
    options: { memoryCost: 19456, timeCost: 2, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
  },
  apiKey: {
    targetMs: 500,
    options: { memoryCost: 12288, timeCost: 3, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
  },
  kek: {
    targetMs: 2500,
    options: { memoryCost: 65536, timeCost: 3, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
  },
} as const

declare namespace CryptoCost {
  type Row = { readonly targetMs: number; readonly options: Omit<Options, "secret" | "salt"> }
  type _Rows<T extends Record<string, Row> = typeof CryptoCost> = T
}

const _HASHES = { sha1: SHA1, sha256: SHA256, sha512: SHA512 } as const

const _CredentialVerdict = Data.taggedEnum<CredentialVerdict>()

const Probe = Data.taggedEnum<Probe>()

const _argonMs = Metric.timerWithBoundaries(Convention.instrument.securityKdf.name, [
  10, 25, 50, 100,
  CryptoCost.login.targetMs,
  CryptoCost.apiKey.targetMs,
  1000,
  CryptoCost.kek.targetMs,
])

const _enc = new TextEncoder()

const _bytes = (text: string): Uint8Array => _enc.encode(text)

const _sameBytes = (left: Uint8Array, right: Uint8Array): boolean =>
  left.byteLength === right.byteLength && constantTimeEqual(left, right)

const _ARGON_ALGORITHMS = {
  [Algorithm.Argon2d]: "argon2d",
  [Algorithm.Argon2i]: "argon2i",
  [Algorithm.Argon2id]: "argon2id",
} as const

const _ARGON_VERSIONS = {
  [Version.V0x10]: 16,
  [Version.V0x13]: 19,
} as const

const _stale = (phc: string, cost: CryptoCost.Row["options"]): boolean => {
  const parts = /^\$(argon2d|argon2i|argon2id)\$v=(\d+)\$m=(\d+),t=(\d+),p=(\d+)\$/.exec(phc)
  return parts === null
    || cost.algorithm === undefined
    || parts[1] !== _ARGON_ALGORITHMS[cost.algorithm]
    || cost.version === undefined
    || globalThis.Number(parts[2]) !== _ARGON_VERSIONS[cost.version]
    || globalThis.Number(parts[3]) !== cost.memoryCost
    || globalThis.Number(parts[4]) !== cost.timeCost
    || globalThis.Number(parts[5]) !== cost.parallelism
}

class Crypto extends Effect.Service<Crypto>()("security/crypt/Crypto", {
  effect: Effect.gen(function* () {
    const pepper = yield* Config.redacted("CREDENTIAL_PEPPER")
    const permits = yield* Config.integer("CRYPTO_KDF_PERMITS").pipe(Config.withDefault(4))
    const gate = yield* Effect.makeSemaphore(permits)
    const secret = _bytes(Redacted.value(pepper))
    const reader: RandomReader = { read: (bytes) => crypto.getRandomValues(bytes) }
    const _kdf = <A>(row: keyof typeof CryptoCost, body: Effect.Effect<A, SignFault>): Effect.Effect<A, SignFault> =>
      gate.withPermits(row === "kek" ? permits : 1)(body).pipe(
        Metric.trackDuration(_argonMs),
        Effect.withSpan("security.crypto.kdf", { attributes: { row } }),
      )
    const digest = (row: keyof typeof CryptoCost, plaintext: Redacted.Redacted<string>): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
      _kdf(row, Effect.tryPromise({
        try: (signal) => hash(Redacted.value(plaintext), { ...CryptoCost[row].options, secret }, signal),
        catch: (cause) => new SignFault({ reason: "digest", detail: String(cause) }),
      }).pipe(Effect.map(Redacted.make)))
    const verify_ = (row: keyof typeof CryptoCost, stored: Redacted.Redacted<string>, plaintext: Redacted.Redacted<string>): Effect.Effect<CredentialVerdict, SignFault> =>
      _kdf(row, Effect.tryPromise({
        try: (signal) => verify(Redacted.value(stored), Redacted.value(plaintext), { secret }, signal),
        catch: (cause) => new SignFault({ reason: "digest", detail: String(cause) }),
      }).pipe(Effect.map((matched) =>
        matched ? _CredentialVerdict.Matched({ stale: _stale(Redacted.value(stored), CryptoCost[row].options) }) : _CredentialVerdict.Rejected())))
    const derive = (row: keyof typeof CryptoCost, seed: Redacted.Redacted<string>, salt: Uint8Array): Effect.Effect<Redacted.Redacted<Uint8Array>, SignFault> =>
      _kdf(row, Effect.tryPromise({
        try: (signal) => hashRaw(Redacted.value(seed), { ...CryptoCost[row].options, secret, salt }, signal),
        catch: (cause) => new SignFault({ reason: "digest", detail: String(cause) }),
      }).pipe(Effect.map((buf) => Redacted.make(new Uint8Array(buf)))))
    const sign_ = (key: Redacted.Redacted<Uint8Array>, body: Uint8Array): Effect.Effect<string, SignFault> =>
      Effect.try({ try: () => encodeHexLowerCase(hmac(SHA256, Redacted.value(key), body)), catch: (cause) => new SignFault({ reason: "mac", detail: String(cause) }) })
    const matches = (probe: Probe): Effect.Effect<boolean, SignFault> =>
      Effect.try({
        try: () =>
          Probe.$match(probe, {
            Mac: ({ key, body, signature }) => _sameBytes(hmac(SHA256, Redacted.value(key), body), decodeHex(signature)),
            Digest: ({ opaque, stored }) => _sameBytes(sha256(_bytes(Redacted.value(opaque))), decodeHex(stored)),
            Text: ({ held, presented }) => _sameBytes(_bytes(Redacted.value(held)), _bytes(presented)),
          }),
        catch: (cause) => new SignFault({ reason: "mac", detail: String(cause) }),
      })
    const token = (alphabet: string, length: number): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
      Effect.try({ try: () => Redacted.make(generateRandomString(reader, alphabet, length)), catch: (cause) => new SignFault({ reason: "rng", detail: String(cause) }) })
    const uuid = (): Effect.Effect<string, SignFault> =>
      Effect.try({
        try: () => {
          const bytes = new Uint8Array(16)
          reader.read(bytes)
          const view = new DataView(bytes.buffer)
          view.setUint8(6, (view.getUint8(6) & 0x0f) | 0x40)
          view.setUint8(8, (view.getUint8(8) & 0x3f) | 0x80)
          const hex = encodeHexLowerCase(bytes)
          return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`
        },
        catch: (cause) => new SignFault({ reason: "rng", detail: String(cause) }),
      })
    const fingerprint = (opaque: Redacted.Redacted<string>): string =>
      encodeHexLowerCase(sha256(_bytes(Redacted.value(opaque))))
    const plugin = {
      name: "rasm-sign",
      hmac: (alg: keyof typeof _HASHES, key: Uint8Array, data: Uint8Array) => hmac(_HASHES[alg], key, data),
      randomBytes: (len: number) => { const bytes = new Uint8Array(len); reader.read(bytes); return bytes },
      constantTimeEqual,
    } as const
    const base32 = { name: "rasm-b32", encode: encodeBase32UpperCaseNoPadding, decode: decodeBase32 } as const
    return { digest, verify: verify_, derive, sign: sign_, matches, token, uuid, fingerprint, plugin, base32 } as const
  }),
  accessors: true,
}) {}
```

## [05]-[SHREDDER]

[SHREDDER]:
- Owner: `Shredder` — the AES-GCM envelope and AES-KW key-wrap primitive the data wave's journal imports for per-subject crypto-shredding: `mint` issues a per-subject data key, `seal`/`open` run the envelope under a 96-bit IV drawn from the `Crypto` entropy port, `wrap`/`unwrap` carry the data key under the master KEK, and erasure is the store dropping the `WrappedKey` — `unwrap` then fails, `open` becomes impossible, and the ciphertext journal is never rewritten.
- Law: the master KEK derives — `Crypto.derive("kek", passphrase, salt)` folds the `Config.redacted` passphrase and a pinned salt into 32 raw bytes imported once as a non-extractable AES-KW key — so KEK custody is one argon2id derivation under the whole-budget bulkhead permit, the passphrase never touches WebCrypto raw, and a KMS provider is a construction-row swap with the seal/open/wrap surface unchanged.
- Law: the data key never leaves the layer except as a `WrappedKey`; `SealedEnvelope` carries IV and ciphertext as opaque base64 bytes; an `open` failure is `SignFault.open` — tamper or shredded-key evidence, class `breached` — and increments the `Convention.instrument.securityShredReject` counter and publishes the `ShredOpen` fact through `Witness` before it surfaces; the crypto floor mints its own Convention row because the folder reject stream composes one stratum above, while the fact floor sits below it.
- Growth: a second envelope suite (XChaCha via a future WebCrypto row) is one algorithm row on the same four-verb surface.
- Boundary: which wrapped key belongs to which subject, and its destruction, is the data wave's journal; this owner holds only the envelope algebra. `@effect/experimental`'s `EventLogEncryption.layerSubtle` zero-knowledge lane consumes this page's key material at the app root — security is the key provider, never the sync engine.
- Packages: WebCrypto `SubtleCrypto` (`generateKey`/`encrypt`/`decrypt`/`wrapKey`/`unwrapKey`/`importKey`); `Crypto` (`derive`, `plugin.randomBytes`); `@rasm/ts/core` (`Convention`); `access/audit` (`Witness`, `SecurityFact`).

```typescript
class SealedEnvelope extends Schema.Class<SealedEnvelope>("SealedEnvelope")({
  iv: Schema.Uint8ArrayFromBase64,
  ciphertext: Schema.Uint8ArrayFromBase64,
}) {}

class WrappedKey extends Schema.Class<WrappedKey>("WrappedKey")({
  wrapped: Schema.Uint8ArrayFromBase64,
}) {}

const _openReject = Metric.counter(Convention.instrument.securityShredReject.name, {
  description: Convention.instrument.securityShredReject.description,
  incremental: true,
})

class Shredder extends Effect.Service<Shredder>()("security/crypt/Shredder", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const passphrase = yield* Config.redacted("SHRED_MASTER_KEY")
    const salt = yield* Config.string("SHRED_MASTER_SALT")
    const raw = yield* cipher.derive("kek", passphrase, _bytes(salt))
    const kek = yield* Effect.tryPromise({
      try: () => crypto.subtle.importKey("raw", Redacted.value(raw), { name: "AES-KW" }, false, ["wrapKey", "unwrapKey"]),
      catch: (cause) => new SignFault({ reason: "wrap", detail: String(cause) }),
    })
    const mint = (): Effect.Effect<Redacted.Redacted<CryptoKey>, SignFault> =>
      Effect.tryPromise({
        try: () => crypto.subtle.generateKey({ name: "AES-GCM", length: 256 }, true, ["encrypt", "decrypt"]),
        catch: (cause) => new SignFault({ reason: "seal", detail: String(cause) }),
      }).pipe(Effect.map(Redacted.make))
    const wrap = (dataKey: Redacted.Redacted<CryptoKey>): Effect.Effect<WrappedKey, SignFault> =>
      Effect.tryPromise({
        try: () => crypto.subtle.wrapKey("raw", Redacted.value(dataKey), kek, "AES-KW"),
        catch: (cause) => new SignFault({ reason: "wrap", detail: String(cause) }),
      }).pipe(Effect.map((buf) => new WrappedKey({ wrapped: new Uint8Array(buf) })))
    const unwrap = (key: WrappedKey): Effect.Effect<Redacted.Redacted<CryptoKey>, SignFault> =>
      Effect.tryPromise({
        try: () => crypto.subtle.unwrapKey("raw", key.wrapped, kek, "AES-KW", { name: "AES-GCM" }, false, ["encrypt", "decrypt"]),
        catch: (cause) => new SignFault({ reason: "wrap", detail: String(cause) }),
      }).pipe(Effect.map(Redacted.make))
    const seal = (dataKey: Redacted.Redacted<CryptoKey>, plaintext: Uint8Array): Effect.Effect<SealedEnvelope, SignFault> =>
      Effect.gen(function* () {
        const iv = cipher.plugin.randomBytes(12)
        const ciphertext = yield* Effect.tryPromise({
          try: () => crypto.subtle.encrypt({ name: "AES-GCM", iv }, Redacted.value(dataKey), plaintext),
          catch: (cause) => new SignFault({ reason: "seal", detail: String(cause) }),
        })
        return new SealedEnvelope({ iv, ciphertext: new Uint8Array(ciphertext) })
      })
    const open = (dataKey: Redacted.Redacted<CryptoKey>, envelope: SealedEnvelope): Effect.Effect<Uint8Array, SignFault> =>
      Effect.tryPromise({
        try: () => crypto.subtle.decrypt({ name: "AES-GCM", iv: envelope.iv }, Redacted.value(dataKey), envelope.ciphertext),
        catch: (cause) => new SignFault({ reason: "open", detail: String(cause) }),
      }).pipe(
        Effect.tapError((fault) => Effect.zipRight(Metric.increment(_openReject), Witness.publish(SecurityFact.ShredOpen({ detail: fault.detail })))),
        Effect.map((buf) => new Uint8Array(buf)),
      )
    return { mint, wrap, unwrap, seal, open } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}
```

## [06]-[TOKEN_AUTHORITY]

[TOKEN_AUTHORITY]:
- Owner: `Jwt` — a scoped Layer factory over a `Keyset`: `mint` stamps `{ alg, kid }` from the active ring key so verifiers route by `kid`; one overloaded `verify` owns both trust roots — `verify(token)` runs `createLocalJWKSet` over every published verify handle with `algorithms` pinned and the declarative claim gates applied, decoding the payload through `AccessClaims`, and `verify(token, issuer)` resolves the per-issuer remote JWKS and returns the verified raw payload for the OAuth page to project from; `seal`/`unseal` are the JWE confidential profile over the keyset's optional symmetric handle. `SingleUse` is the stash contract every two-leg ceremony port in the folder instantiates — stash with a TTL, consume exactly once — so the satisfying layer is an `effect` `Cache` or `@effect/experimental` `PersistedCache`/`Persistence.layerResultKeyValueStore` row, never a hand-rolled map.
- Law: `algorithms` is always pinned — an unpinned `alg` is accepted-algorithm confusion; the claim gates (`issuer`, `audience`, `clockTolerance`, and required `iat`/`exp`/`iss`/`aud`/`sub`) are one jose verification policy, never hand timestamp or presence checks; `decodeJwt` is never verification; `cnf.jkt` carries the `Material.thumbprintUri` binding for a sender-constrained token, and a verifier that receives `cnf` matches it against the presented key's thumbprint URI.
- Law: the factory form is the rotation seam — the composition root builds the `Keyset` from `crypt/secret` credentials through `Material.ring`, wraps `Jwt.Default(keyset)` in `Reloadable.auto` driven by `Secret.changes`, so a Doppler rotation republishes the ring without a graph teardown, a `kid` retires with zero edits here, and a retired signing key keeps verifying while its handle stays published.
- Law: the remote resolver is built once per issuer under `Effect.cachedFunction` — the ledger snapshot pre-seeds the jose cache record, a scoped fiber drives `resolver.reload()` on a jittered `Schedule.spaced(cacheAge)` so a provider key roll lands before the first `kid` miss, and every reload and successful verify persists the record back through `JwksLedger`; a cold build increments the `Convention.instrument.securityJwksMiss` counter.
- Law: every remote verify is internally resilient — a `deadline` timeout, a jittered exponential retry bounded by `Schedule.recurs` and gated on `FaultClass.retryable`, the `Convention.instrument.securityJwksResolve` timer, and its span; the fetch routes through `JwksTransport`, defaulted to the platform fetch and bound by the runtime wave to its instrumented `HttpClient.retryTransient({ schedule }).pipe(HttpClient.withTracerPropagation)` fetch adapter so rotation inherits the shared net policy and W3C trace propagation.
- Law: the JWE profile is confidentiality, not a second token system — `seal` encrypts the same `AccessClaims` under `{ alg: "dir", enc: "A256GCM" }` and `unseal` reverses it with the same claim gates; a keyset without a seal handle refuses the profile as `unsupported`.
- Receipt: `mint`/`seal` return the token `Redacted`; `verify`/`unseal` return `AccessClaims`, never a bare `JWTPayload`; the issuer overload returns the verified payload.
- Growth: a new claim is one `AccessClaims` field; a new JOSE failure code is one `_codeReason` arm; a new external issuer costs nothing — the resolver memoizes per `jwksUri`.
- Packages: `jose` (`SignJWT`/`jwtVerify`/`EncryptJWT`/`jwtDecrypt`, `createLocalJWKSet`/`createRemoteJWKSet`, `jwksCache`/`customFetch` symbols, `ExportedJWKSCache`); `effect` (`Schedule`, `Metric`, `Effect.cachedFunction`, `Effect.forkScoped`); `@rasm/ts/core` (`Convention`).

```typescript
class AccessClaims extends Schema.Class<AccessClaims>("AccessClaims")({
  sub: Schema.NonEmptyString,
  sid: Schema.NonEmptyString,
  scope: Schema.Array(Schema.NonEmptyString),
  tid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  cnf: Schema.optionalWith(Schema.Struct({ jkt: Schema.NonEmptyString }), { as: "Option" }),
}) {}

type Keyset = {
  readonly ring: Ring
  readonly seal: Option.Option<Redacted.Redacted<CryptoKey>>
  readonly issuer: string
  readonly audience: string
}

type IssuerRef = {
  readonly issuer: string
  readonly audience: string
  readonly jwksUri: string
  readonly algorithms: ReadonlyArray<KeyAlg.Kind>
}

type SingleUse<A, E> = {
  readonly stash: (key: string, value: A, ttl: Duration.DurationInput) => Effect.Effect<void, E>
  readonly consume: (key: string) => Effect.Effect<Option.Option<A>, E>
}

const _requiredClaims = ["iat", "exp", "iss", "aud", "sub"] as const

const _codeReason = {
  ERR_JWT_EXPIRED: "expired",
  ERR_JWT_CLAIM_VALIDATION_FAILED: "claim",
  ERR_JWS_SIGNATURE_VERIFICATION_FAILED: "signature",
  ERR_JWS_INVALID: "signature",
  ERR_JWT_INVALID: "signature",
  ERR_JWE_DECRYPTION_FAILED: "signature",
  ERR_JWE_INVALID: "malformed",
  ERR_JOSE_ALG_NOT_ALLOWED: "algorithm",
  ERR_JOSE_NOT_SUPPORTED: "algorithm",
  ERR_JWKS_NO_MATCHING_KEY: "jwks",
  ERR_JWKS_MULTIPLE_MATCHING_KEYS: "jwks",
  ERR_JWKS_TIMEOUT: "jwks",
  ERR_JWKS_INVALID: "jwks",
} as const satisfies Record<string, SignFault.Reason>

const _codes: Record<string, SignFault.Reason | undefined> = _codeReason

const _reasonOf = (cause: unknown): SignFault.Reason =>
  Predicate.hasProperty(cause, "code") && Predicate.isString(cause.code) ? (_codes[cause.code] ?? "malformed") : "malformed"

const _jwksMs = Metric.timerWithBoundaries(Convention.instrument.securityJwksResolve.name, [5, 25, 100, 250, 1000, 5000])
const _jwksMiss = Metric.counter(Convention.instrument.securityJwksMiss.name, {
  description: Convention.instrument.securityJwksMiss.description,
  incremental: true,
})

class JwksLedger extends Context.Tag("security/crypt/JwksLedger")<JwksLedger, {
  readonly load: (issuer: string) => Effect.Effect<Option.Option<ExportedJWKSCache>>
  readonly save: (issuer: string, snapshot: ExportedJWKSCache) => Effect.Effect<void>
}>() {
  static readonly memory: Layer.Layer<JwksLedger> = Layer.effect(
    JwksLedger,
    Effect.map(Ref.make(HashMap.empty<string, ExportedJWKSCache>()), (cell) => ({
      load: (issuer) => Effect.map(Ref.get(cell), HashMap.get(issuer)),
      save: (issuer, snapshot) => Ref.update(cell, HashMap.set(issuer, snapshot)),
    })),
  )
}

class JwksTransport extends Context.Reference<JwksTransport>()("security/crypt/JwksTransport", {
  defaultValue: (): typeof globalThis.fetch => globalThis.fetch,
}) {}

class Jwt extends Effect.Service<Jwt>()("security/crypt/Jwt", {
  scoped: (keyset: Keyset) =>
    Effect.gen(function* () {
      const ledger = yield* JwksLedger
      const transport = yield* JwksTransport
      const tolerance = yield* Config.integer("JWT_CLOCK_TOLERANCE").pipe(Config.withDefault(5))
      const cacheAge = yield* Config.duration("JWKS_CACHE_AGE").pipe(Config.withDefault(Duration.minutes(10)))
      const cooldown = yield* Config.duration("JWKS_COOLDOWN").pipe(Config.withDefault(Duration.seconds(30)))
      const deadline = yield* Config.duration("JWKS_DEADLINE").pipe(Config.withDefault(Duration.seconds(5)))
      const local = createLocalJWKSet(yield* Material.jwks(keyset.ring.verify))
      const _algorithms = Array.map(keyset.ring.verify, (handle) => handle.alg)
      const _remote = yield* Effect.cachedFunction((jwksUri: string) =>
        Effect.gen(function* () {
          const held = yield* ledger.load(jwksUri)
          yield* Metric.increment(_jwksMiss).pipe(Effect.when(() => Option.isNone(held)))
          const record: { jwks?: JSONWebKeySet; uat?: number } = Option.getOrElse(held, () => ({}))
          const resolver = createRemoteJWKSet(new URL(jwksUri), {
            cacheMaxAge: Duration.toMillis(cacheAge), cooldownDuration: Duration.toMillis(cooldown),
            timeoutDuration: Duration.toMillis(deadline), [jwksCache]: record, [customFetch]: transport,
          })
          const persist = Effect.suspend(() =>
            record.jwks !== undefined && record.uat !== undefined
              ? ledger.save(jwksUri, { jwks: record.jwks, uat: record.uat })
              : Effect.void)
          yield* Effect.forkScoped(Effect.repeat(
            Effect.ignore(Effect.zipRight(
              Effect.tryPromise({ try: () => resolver.reload(), catch: (cause) => new SignFault({ reason: "jwks", detail: String(cause) }) }),
              persist,
            )),
            Schedule.spaced(cacheAge).pipe(Schedule.jittered),
          ))
          return { resolver, persist } as const
        }))
      const _decoded = (payload: JWTPayload): Effect.Effect<AccessClaims, SignFault> =>
        Schema.decodeUnknown(AccessClaims)(payload).pipe(
          Effect.mapError((cause) => new SignFault({ reason: "claim", detail: String(cause) })))
      const _claims = (claims: AccessClaims) => ({
        sid: claims.sid, scope: claims.scope,
        ...(Option.isSome(claims.tid) && { tid: claims.tid.value }),
        ...(Option.isSome(claims.cnf) && { cnf: { jkt: claims.cnf.value.jkt } }),
      })
      const _seconds = (ttl: Duration.DurationInput): string =>
        `${Math.max(1, Math.round(Duration.toSeconds(Duration.decode(ttl))))}s`
      const _local = (token: Redacted.Redacted<string>): Effect.Effect<AccessClaims, SignFault> =>
        Effect.tryPromise({
          try: () => jwtVerify(Redacted.value(token), local, {
            algorithms: _algorithms, issuer: keyset.issuer, audience: keyset.audience,
            clockTolerance: tolerance, requiredClaims: [..._requiredClaims],
          }),
          catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
        }).pipe(
          Effect.flatMap((result) => _decoded(result.payload)),
          Effect.withSpan("security.jwt.verify"),
        )
      const _external = (token: Redacted.Redacted<string>, issuer: IssuerRef): Effect.Effect<JWTPayload, SignFault> =>
        Effect.flatMap(_remote(issuer.jwksUri), ({ persist, resolver }) =>
          Effect.tryPromise({
            try: () => jwtVerify(Redacted.value(token), resolver, {
              algorithms: [...issuer.algorithms], issuer: issuer.issuer, audience: issuer.audience,
              clockTolerance: tolerance, requiredClaims: [..._requiredClaims],
            }),
            catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
          }).pipe(
            Effect.timeoutFail({ duration: deadline, onTimeout: () => new SignFault({ reason: "jwks", detail: issuer.jwksUri }) }),
            Effect.retry({
              schedule: Schedule.exponential(Duration.millis(100)).pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(2))),
              while: (fault) => FaultClass.retryable(fault.class),
            }),
            Metric.trackDuration(_jwksMs),
            Effect.tap(() => persist),
            Effect.map((result) => result.payload),
            Effect.withSpan("security.jwt.verifyExternal", { attributes: { issuer: issuer.issuer } }),
          ))
      function verify(token: Redacted.Redacted<string>): Effect.Effect<AccessClaims, SignFault>
      function verify(token: Redacted.Redacted<string>, issuer: IssuerRef): Effect.Effect<JWTPayload, SignFault>
      function verify(token: Redacted.Redacted<string>, issuer?: IssuerRef): Effect.Effect<AccessClaims, SignFault> | Effect.Effect<JWTPayload, SignFault> {
        return issuer === undefined ? _local(token) : _external(token, issuer)
      }
      const mint = (claims: AccessClaims, ttl: Duration.DurationInput): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
        Effect.tryPromise({
          try: () =>
            new SignJWT(_claims(claims))
              .setProtectedHeader({ alg: keyset.ring.active.alg, kid: keyset.ring.active.kid })
              .setIssuedAt().setIssuer(keyset.issuer).setAudience(keyset.audience).setSubject(claims.sub)
              .setExpirationTime(_seconds(ttl))
              .sign(Redacted.value(keyset.ring.active.key)),
          catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
        }).pipe(Effect.map(Redacted.make), Effect.withSpan("security.jwt.mint"))
      const _sealKey = Effect.mapError(keyset.seal, () => new SignFault({ reason: "unsupported", detail: "no seal handle" }))
      const seal = (claims: AccessClaims, ttl: Duration.DurationInput): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
        Effect.flatMap(_sealKey, (key) =>
          Effect.tryPromise({
            try: () =>
              new EncryptJWT(_claims(claims))
                .setProtectedHeader({ alg: "dir", enc: "A256GCM" })
                .setIssuedAt().setIssuer(keyset.issuer).setAudience(keyset.audience).setSubject(claims.sub)
                .setExpirationTime(_seconds(ttl))
                .encrypt(Redacted.value(key)),
            catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
          })).pipe(Effect.map(Redacted.make))
      const unseal = (token: Redacted.Redacted<string>): Effect.Effect<AccessClaims, SignFault> =>
        Effect.flatMap(_sealKey, (key) =>
          Effect.tryPromise({
            try: () => jwtDecrypt(Redacted.value(token), Redacted.value(key), {
              issuer: keyset.issuer, audience: keyset.audience,
              clockTolerance: tolerance, requiredClaims: [..._requiredClaims],
            }),
            catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
          })).pipe(Effect.flatMap((result) => _decoded(result.payload)))
      return { mint, verify, seal, unseal } as const
    }),
  accessors: true,
}) {}
```

## [07]-[CALIBRATION]

[CALIBRATION]:
- Owner: `Calibration` — the bench leg that turns `CryptoCost` folklore into per-host receipts: `bench` measures any rail probe into one core `Claim`, the decoded owner of `BenchmarkClaimWire`, and admits it against the executing `AppIdentity`; `calibrate` folds the same measurement over every cost row through `Crypto.digest` itself and grades p99 against the row's latency ceiling. Selection is evidence, never mutation: an unadmitted row demands a `CryptoCost` row edit backed by its receipt, and the `_stale` rehash fold propagates the options edit on the next successful verify.
- Law: each target is a `CryptoCost` field and `_argonMs` takes its target boundaries from those rows, so the KDF histogram and calibration verdict read the same value; a target cannot drift beside its cost owner.
- Law: `trials` admits only positive integers before a probe starts, and each sample derives milliseconds from `Clock.currentTimeNanos`; invalid counts reject on the typed rail and wall-clock adjustment cannot skew a receipt.
- Law: trials run the production shape — `calibrate` probes `Crypto.digest` per row, so every trial passes the semaphore bulkhead, the `Convention.instrument.securityKdf` timer, and the KDF span exactly as a login does; a bench that bypasses the bulkhead measures a machine that does not exist.
- Law: receipts are the core claim family — each metric carries the complete `Claim.Band` ladder (`ticks`, raw `samples`, `min`, `max`, `avg`, `p25`, `p50`, `p75`, `p99`, `p999`, optional enrichment bands), the host rides `Claim["host"]`, and every returned claim passes `Claim.admit` before grading. Core's codec maps the same class to `BenchmarkClaimWire`; no security-local wire exists.
- Law: throughput claims ride the same family — a `Jwt` mint or verify-fold claim is one `bench` call over that probe with its own suite key; a second receipt shape per crypto surface is the forked-family defect.
- Growth: a new statistic is one metrics row inside `_receipt`; a new bench subject is one `bench` call; a new credential class inherits its target row through the guard pair.
- Boundary: `HostFingerprint` construction (print, machine, arch, cores, runtime) is the composing runtime's boot fact, passed in — this page never probes the host; claim persistence and cross-host trend boards are the core bench-pack and corpus-gate consumers over the encoded family.
- Packages: `effect` (`Clock`, `Order`, `Struct`, `Array`, `Number`); `@rasm/ts/core` (`Claim`); `Crypto` (`digest` as the measured probe).

```typescript
declare namespace Calibration {
  type Row = keyof typeof CryptoCost
  type Stats = typeof Claim.Band.Type
  type Verdict = { readonly admitted: boolean; readonly claim: Claim; readonly row: Row; readonly target: number }
}

const _quantile = (sorted: ReadonlyArray<number>, q: number): number =>
  Option.getOrElse(Array.get(sorted, Math.min(sorted.length - 1, Math.floor(q * sorted.length))), () => 0)

const _stats = (timings: Array.NonEmptyReadonlyArray<number>): Calibration.Stats =>
  pipe(Array.sort(timings, Order.number), (sorted) => ({
    avg: Number.sumAll(sorted) / sorted.length,
    counters: Option.none(),
    gc: Option.none(),
    heap: Option.none(),
    max: Array.lastNonEmpty(sorted),
    min: Array.headNonEmpty(sorted),
    p25: _quantile(sorted, 0.25),
    p50: _quantile(sorted, 0.5),
    p75: _quantile(sorted, 0.75),
    p99: _quantile(sorted, 0.99),
    p999: _quantile(sorted, 0.999),
    samples: sorted,
    ticks: sorted.length,
  }))

const _timed = <A, E, R>(probe: Effect.Effect<A, E, R>): Effect.Effect<number, E, R> =>
  Effect.gen(function* () {
    const opened = yield* Clock.currentTimeNanos
    yield* probe
    return globalThis.Number((yield* Clock.currentTimeNanos) - opened) / 1_000_000
  })

const _measured = <E, R>(probe: Effect.Effect<unknown, E, R>, trials: number): Effect.Effect<Calibration.Stats, E | SignFault, R> =>
  globalThis.Number.isInteger(trials) && trials > 0
    ? Effect.flatMap(
        Effect.forEach(Array.range(1, trials), () => _timed(probe), { concurrency: 1 }),
        (timings) =>
          Array.isNonEmptyReadonlyArray(timings) ? Effect.succeed(_stats(timings)) : Effect.dieMessage("empty calibration trial set"),
      )
    : Effect.fail(new SignFault({ reason: "unsupported", detail: `invalid calibration trials: ${trials}` }))

const _receipt = (suite: string, host: Claim["host"], stats: Calibration.Stats): Effect.Effect<Claim> =>
  Effect.map(DateTime.now, (minted) =>
    new Claim({
      suite,
      metrics: [
        { label: "wall", unit: "ms", kind: "fn", band: stats },
      ],
      host,
      minted,
    }))

const Calibration = {
  bench: <E, R>(suite: string, identity: AppIdentity, host: Claim["host"], probe: Effect.Effect<unknown, E, R>, trials: number): Effect.Effect<Claim, E | SignFault | WireFault, R> =>
    Effect.flatMap(_measured(probe, trials), (stats) =>
      Effect.flatMap(_receipt(suite, host, stats), (claim) => Claim.admit(claim, identity))),
  calibrate: (
    identity: AppIdentity,
    host: Claim["host"],
    probe: Redacted.Redacted<string>,
    trials: number,
  ): Effect.Effect<ReadonlyArray<Calibration.Verdict>, SignFault | WireFault, Crypto> =>
    Effect.flatMap(Crypto, (cipher) =>
      Effect.forEach(Struct.keys(CryptoCost), (row) =>
        Effect.gen(function* () {
          const stats = yield* _measured(cipher.digest(row, probe), trials)
          const claim = yield* Effect.flatMap(_receipt(`security-kdf-${row}`, host, stats), (receipt) => Claim.admit(receipt, identity))
          return { admitted: stats.p99 <= CryptoCost[row].targetMs, claim, row, target: CryptoCost[row].targetMs }
        }))),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { AccessClaims, Calibration, Crypto, CryptoCost, Jwt, JwksLedger, JwksTransport, KeyAlg, Material, Probe, SealedEnvelope, Shredder, SignFault, WrappedKey }
export type { CredentialVerdict, IssuerRef, KeyHandle, Keyset, Ring, SingleUse }
```

## [08]-[RESEARCH]

(none)

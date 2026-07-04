# [SECURITY_SIGN]

The one crypto authority of the folder: argon2id credential digest-at-rest, HMAC egress signing, opaque-token minting, the AES-GCM crypto-shredding `Shredder`, jose key-material admission with RFC 7638 thumbprint identity, and the JWT/JWS/JWKS/JWE token authority — one module because every concern shares one key plane and one fault family. Key material enters exactly once: the core-decoded `Credential` landing folds through `Material.admit` into a `Redacted<CryptoKey>` `KeyHandle`, the `kid` is the wire fingerprint or the computed thumbprint, and the validity window is enforced at admission — no second import path exists for a Doppler-fetched or wire-carried key. `Jwt` mints with the active ring key, verifies against the local JWKS with a pinned `algorithms` allow-list and declarative claim gates, verifies external OIDC tokens through a cooldown-throttled remote resolver whose cache persists through the `JwksLedger` port and whose transport is the `JwksTransport` reference, and seals confidential claims as JWE. Every secret — pepper, password, data key, minted token, private handle — is `Redacted` from admission and unwraps only into the primitive call; algorithm, cost, and reason are vocabulary rows, never call-site knobs. `SignFault` is the folder's canonical fault shape: one reason family whose rows carry the core `FaultClass` classification, so retryability, dominance, and blame derive from the branch table and the serving edge folds `class` to status through its own governed record.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                | [PUBLIC]                     |
| :-----: | :----------------- | :--------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `FAULT_AND_ALG`    | the folder fault shape, the `KeyAlg` scheme vocabulary                 | `SignFault`, `KeyAlg`        |
|  [02]   | `KEY_MATERIAL`     | `Credential` landing admission, `KeyHandle`/`Ring`, JWKS projection    | `Material`                   |
|  [03]   | `CRYPTO_PRIMITIVE` | argon2 digest/verify/derive, HMAC, token RNG, otplib ports             | `Crypto`, `CredentialVerdict`|
|  [04]   | `SHREDDER`         | the AES-GCM envelope + AES-KW wrap for per-subject crypto-shredding    | `Shredder`                   |
|  [05]   | `TOKEN_AUTHORITY`  | JWT mint/verify, remote JWKS rotation, the JWE confidential profile    | `Jwt`, `AccessClaims`        |

## [2]-[FAULT_AND_ALG]

[FAULT_AND_ALG]:
- Owner: `SignFault` — the one reason-discriminated `Schema.TaggedError` every page in this folder instantiates with its own reason set; each row carries the core `FaultClass` kind, `get class()` projects it so `FaultClass.of` classifies structurally, and `override get message()` derives from fields. `KeyAlg` is the bounded signature-scheme vocabulary — each row carries `{ kty, crv?, use }`, the discriminant derives through `keyof typeof`, and a new scheme is one row.
- Law: rows carry `class` only — rank, retryability, and blame derive from the branch `FaultClass` table, and the class-to-status projection is the serving edge's governed record; a local `{ rank, retry, status }` triple beside the class column is the split-brain this shape kills.
- Law: a `false` argon2 verify, a rejected OTP, and a rotated-out token are verdict arms, never faults — `SignFault` fires only when a primitive throws, a key refuses import, or a token fails a trust gate.
- Growth: a new failure mode is one reason literal plus one class row; a new signature scheme is one `KeyAlg` row that `Material`, `Jwt`, and the external-verify page inherit unchanged.
- Packages: `effect` (`Schema`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { Algorithm, hash, hashRaw, Version, verify, type Options } from "@node-rs/argon2"
import { hmac } from "@oslojs/crypto/hmac"
import { type RandomReader, generateRandomString } from "@oslojs/crypto/random"
import { SHA1 } from "@oslojs/crypto/sha1"
import { SHA256, SHA512, sha256 } from "@oslojs/crypto/sha2"
import { constantTimeEqual } from "@oslojs/crypto/subtle"
import { decodeBase32, decodeHex, encodeBase32UpperCaseNoPadding, encodeHexLowerCase } from "@oslojs/encoding"
import { Credential, FaultClass } from "@rasm/ts/core"
import {
  calculateJwkThumbprint, createLocalJWKSet, createRemoteJWKSet, EncryptJWT, exportJWK, importJWK, importPKCS8,
  importSPKI, importX509, jwtDecrypt, jwtVerify, SignJWT, customFetch, jwksCache,
  type ExportedJWKSCache, type JSONWebKeySet, type JWK, type JWTPayload,
} from "jose"
import { Array, Config, Context, Data, DateTime, Duration, Effect, HashMap, Layer, Option, Predicate, Redacted, Ref, Schema } from "effect"

const _reasons = [
  "digest", "mac", "rng", "seal", "open", "wrap",
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
  type Reason = keyof typeof _faults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
}

declare namespace KeyAlg {
  type Kind = keyof typeof KeyAlg
  type Row = (typeof KeyAlg)[Kind]
  type _Keys<K extends Kind = (typeof _algs)[number]> = K
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

## [3]-[KEY_MATERIAL]

[KEY_MATERIAL]:
- Owner: `Material` — the assembled key-material owner: `admit` folds one core `Credential` landing into a `KeyHandle`, `ring` narrows a signing credential plus a published JWKS into the `{ active, verify }` set `Jwt` consumes, `jwks` projects the verify handles back to a `JSONWebKeySet` for publication, and `thumbprint` is the RFC 7638 identity mint. The `Credential` landing arrives sealed from the core interchange decode — `material` is `Redacted` from the wire, `fingerprint` is the only audit identity — and this owner is its terminus: the handle never crosses back to a wire and never reaches a log.
- Law: admission is one polymorphic fold — the PEM header sniffs the jose importer (`importPKCS8` private, `importSPKI`/`importX509` public, a JSON body routes `importJWK`), the private side lands `Signing` and the public side `Verify`, and there is no `admitSigning`/`admitVerify` twin; a symmetric `importJWK` result is `unsupported`.
- Law: the validity window is enforced at admission — an instant outside `[notBefore, notAfter]` is `SignFault.window`, so an expired or not-yet-live credential never becomes a handle; rotation is re-admission of a fresh `Credential`, and `Credential.rotated` is the sealed compare the custodian already carries.
- Law: `kid` is the wire `fingerprint` when the landing carries one, else the computed RFC 7638 thumbprint over the exported JWK — rotation keys carry stable identity and a stored kid can never drift from its key.
- Growth: a new signature scheme is one `KeyAlg` row; a new material source (KMS, HSM) produces the same `Credential` value and terminates through the same `admit`.
- Boundary: `crypt/secret` constructs `Credential` values from fetched material; the core interchange codec decodes the C#-minted `CredentialPemWire` into the same class; `Jwt` is the only consumer that unwraps `Signing`, and the external-verify page consumes `Verify` handles only through `jwks`.
- Packages: `jose` (`importPKCS8`/`importSPKI`/`importX509`/`importJWK`, `exportJWK`, `calculateJwkThumbprint`); `@rasm/ts/core` (`Credential`).

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

const _format = (pem: string): "pkcs8" | "spki" | "x509" | "jwk" =>
  pem.startsWith("-----BEGIN PRIVATE KEY-----") ? "pkcs8"
    : pem.startsWith("-----BEGIN PUBLIC KEY-----") ? "spki"
    : pem.startsWith("-----BEGIN CERTIFICATE-----") ? "x509"
    : "jwk"

const _imported = (pem: string, format: "pkcs8" | "spki" | "x509" | "jwk", alg: KeyAlg.Kind): Effect.Effect<CryptoKey, SignFault> =>
  Effect.tryPromise({
    try: () =>
      format === "pkcs8" ? importPKCS8(pem, alg)
        : format === "spki" ? importSPKI(pem, alg)
        : format === "x509" ? importX509(pem, alg)
        : importJWK(JSON.parse(pem) as JWK, alg),
    catch: (cause) => new SignFault({ reason: "material", detail: String(cause) }),
  }).pipe(Effect.filterOrFail(
    (key): key is CryptoKey => !(key instanceof Uint8Array),
    () => new SignFault({ reason: "unsupported", detail: "symmetric jwk material" }),
  ))

const _windowed = (credential: Credential): Effect.Effect<void, SignFault> =>
  Effect.flatMap(DateTime.now, (now) =>
    DateTime.between(now, { minimum: credential.notBefore, maximum: credential.notAfter })
      ? Effect.void
      : Effect.fail(new SignFault({ reason: "window", detail: credential.fingerprint })))

const _admit = (credential: Credential, alg: KeyAlg.Kind): Effect.Effect<KeyHandle, SignFault> =>
  Effect.gen(function* () {
    yield* _windowed(credential)
    const pem = Redacted.value(credential.material)
    const format = _format(pem)
    const key = yield* _imported(pem, format, alg)
    const secret = format === "pkcs8" || (format === "jwk" && Predicate.hasProperty(JSON.parse(pem), "d"))
    const kid = credential.fingerprint
    const held = Redacted.make(key)
    return secret && credential.kind === "signing"
      ? _KeyHandle.Signing({ kid, alg, key: held })
      : _KeyHandle.Verify({ kid, alg, key: held })
  })

const Material = {
  admit: _admit,
  thumbprint: (jwk: JWK): Effect.Effect<string, SignFault> =>
    Effect.tryPromise({ try: () => calculateJwkThumbprint(jwk, "sha256"), catch: (cause) => new SignFault({ reason: "material", detail: String(cause) }) }),
  jwks: (keys: ReadonlyArray<Extract<KeyHandle, { readonly _tag: "Verify" }>>): Effect.Effect<JSONWebKeySet, SignFault> =>
    Effect.map(
      Effect.forEach(keys, (handle) =>
        Effect.tryPromise({
          try: async () => ({ ...(await exportJWK(Redacted.value(handle.key))), kid: handle.kid, alg: handle.alg, use: "sig" }),
          catch: (cause) => new SignFault({ reason: "material", detail: String(cause) }),
        })),
      (list) => ({ keys: Array.fromIterable(list) }),
    ),
  ring: (signing: Credential, alg: KeyAlg.Kind, published: JSONWebKeySet): Effect.Effect<Ring, SignFault> =>
    Effect.gen(function* () {
      const active = yield* _admit(signing, alg).pipe(Effect.filterOrFail(
        (handle): handle is Extract<KeyHandle, { readonly _tag: "Signing" }> => handle._tag === "Signing",
        () => new SignFault({ reason: "material", detail: "signing credential resolved public" }),
      ))
      const verify = yield* Effect.forEach(published.keys, (jwk) =>
        Effect.gen(function* () {
          const kid = yield* Option.match(Option.fromNullable(jwk.kid), {
            onSome: Effect.succeed,
            onNone: () => Material.thumbprint(jwk),
          })
          const scheme = yield* Schema.decodeUnknown(Schema.Literal(..._algs))(jwk.alg ?? alg).pipe(
            Effect.mapError(() => new SignFault({ reason: "unsupported", detail: String(jwk.alg) })))
          const now = yield* DateTime.now
          const carrier = new Credential({
            kind: "signing", material: Redacted.make(JSON.stringify(jwk)), fingerprint: kid,
            notBefore: now, notAfter: DateTime.addDuration(now, Duration.days(3650)),
          })
          return yield* _admit(carrier, scheme).pipe(Effect.filterOrFail(
            (handle): handle is Extract<KeyHandle, { readonly _tag: "Verify" }> => handle._tag === "Verify",
            () => new SignFault({ reason: "material", detail: "jwks entry resolved private" }),
          ))
        }))
      return { active, verify }
    }),
} as const
```

## [4]-[CRYPTO_PRIMITIVE]

[CRYPTO_PRIMITIVE]:
- Owner: `Crypto` — `digest`/`verify` own argon2id credential-at-rest with the `CredentialVerdict` receipt, `derive` is the raw-KDF row minting deterministic key bytes from a passphrase, `sign`/`compare` own HMAC-SHA256 egress signing rendered hex, `token` mints opaque high-entropy material over the WebCrypto-filled `RandomReader`, `fingerprint`/`matches` are the SHA-256 pair for high-entropy token lookup, `same` is the constant-time string compare the session CSRF fold rides, and `plugin`/`base32` are the otplib ports over these same primitives.
- Law: cost is a named `CryptoCost` row selected by credential class — `login` interactive, `apiKey` machine, `kek` the derive row backing the `Shredder` master key — with `Argon2id`+`V0x13` pinned; the pepper is one `Config.redacted` injected at construction and threaded as `secret`; the fiber's interrupt threads the `AbortSignal` so a request-scoped hash cancels with its caller.
- Law: `verify` reads the PHC-embedded parameters, and a match under stale parameters returns `Matched({ stale: true })` — the rehash signal the caller persists on; `Rejected` is the ordinary auth-fail arm and only a malformed stored digest throws into `SignFault.digest`.
- Law: every compare routes constant-time — length is the only short-circuit, a length mismatch is `false`, never a throw; the otplib `hmac` port dispatches the `HashAlgorithm` value off the `_HASHES` row table so a new hash is a row, never a name fork; a stored argon2 digest is checked by argon2's own constant-time `verify` and never re-compared through `constantTimeEqual`.
- Growth: a new credential class is one `CryptoCost` row; a cost bump is a row edit the rehash fold detects on the next successful verify.
- Boundary: `authn/credential` delegates every digest-at-rest here; `authn/session` consumes `token`/`fingerprint`/`matches`/`same`; `crypt/verify` composes `compare` under its dialect grammar; no sibling imports `@node-rs/argon2` or `@oslojs/*` directly.
- Packages: `@node-rs/argon2` (`hash`/`hashRaw`/`verify`, `Algorithm`, `Version`); `@oslojs/crypto` (`hmac`, `SHA1`/`SHA256`/`SHA512`, `sha256`, `constantTimeEqual`, `generateRandomString`, `RandomReader`); `@oslojs/encoding` (hex + base32 rows).

```typescript
type CredentialVerdict = Data.TaggedEnum<{
  Matched: { readonly stale: boolean }
  Rejected: {}
}>

const CryptoCost = {
  login: { memoryCost: 19456, timeCost: 2, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
  apiKey: { memoryCost: 12288, timeCost: 3, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
  kek: { memoryCost: 65536, timeCost: 3, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
} as const satisfies Record<string, Omit<Options, "secret" | "salt">>

const _HASHES = { sha1: SHA1, sha256: SHA256, sha512: SHA512 } as const

const _CredentialVerdict = Data.taggedEnum<CredentialVerdict>()

const _bytes = (text: string): Uint8Array => new TextEncoder().encode(text)

const _sameBytes = (left: Uint8Array, right: Uint8Array): boolean =>
  left.byteLength === right.byteLength && constantTimeEqual(left, right)

const _stale = (phc: string, cost: (typeof CryptoCost)[keyof typeof CryptoCost]): boolean => {
  const parts = /\$m=(\d+),t=(\d+),p=(\d+)\$/.exec(phc)
  return parts === null || Number(parts[1]) !== cost.memoryCost || Number(parts[2]) !== cost.timeCost || Number(parts[3]) !== cost.parallelism
}

class Crypto extends Effect.Service<Crypto>()("security/crypt/Crypto", {
  effect: Effect.gen(function* () {
    const pepper = yield* Config.redacted("CREDENTIAL_PEPPER")
    const secret = _bytes(Redacted.value(pepper))
    const reader: RandomReader = { read: (bytes) => crypto.getRandomValues(bytes) }
    const digest = (row: keyof typeof CryptoCost, plaintext: Redacted.Redacted<string>): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
      Effect.tryPromise({
        try: (signal) => hash(Redacted.value(plaintext), { ...CryptoCost[row], secret }, signal),
        catch: (cause) => new SignFault({ reason: "digest", detail: String(cause) }),
      }).pipe(Effect.map(Redacted.make))
    const verify_ = (row: keyof typeof CryptoCost, stored: Redacted.Redacted<string>, plaintext: Redacted.Redacted<string>): Effect.Effect<CredentialVerdict, SignFault> =>
      Effect.tryPromise({
        try: (signal) => verify(Redacted.value(stored), Redacted.value(plaintext), { secret }, signal),
        catch: (cause) => new SignFault({ reason: "digest", detail: String(cause) }),
      }).pipe(Effect.map((matched) =>
        matched ? _CredentialVerdict.Matched({ stale: _stale(Redacted.value(stored), CryptoCost[row]) }) : _CredentialVerdict.Rejected()))
    const derive = (row: keyof typeof CryptoCost, seed: Redacted.Redacted<string>, salt: Uint8Array): Effect.Effect<Redacted.Redacted<Uint8Array>, SignFault> =>
      Effect.tryPromise({
        try: (signal) => hashRaw(Redacted.value(seed), { ...CryptoCost[row], secret, salt }, signal),
        catch: (cause) => new SignFault({ reason: "digest", detail: String(cause) }),
      }).pipe(Effect.map((buf) => Redacted.make(new Uint8Array(buf))))
    const sign_ = (key: Redacted.Redacted<Uint8Array>, body: Uint8Array): Effect.Effect<string, SignFault> =>
      Effect.try({ try: () => encodeHexLowerCase(hmac(SHA256, Redacted.value(key), body)), catch: (cause) => new SignFault({ reason: "mac", detail: String(cause) }) })
    const compare = (key: Redacted.Redacted<Uint8Array>, body: Uint8Array, signature: string): Effect.Effect<boolean, SignFault> =>
      Effect.try({ try: () => _sameBytes(hmac(SHA256, Redacted.value(key), body), decodeHex(signature)), catch: (cause) => new SignFault({ reason: "mac", detail: String(cause) }) })
    const token = (alphabet: string, length: number): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
      Effect.try({ try: () => Redacted.make(generateRandomString(reader, alphabet, length)), catch: (cause) => new SignFault({ reason: "rng", detail: String(cause) }) })
    const fingerprint = (opaque: Redacted.Redacted<string>): string =>
      encodeHexLowerCase(sha256(_bytes(Redacted.value(opaque))))
    const matches = (opaque: Redacted.Redacted<string>, storedHex: string): boolean =>
      _sameBytes(sha256(_bytes(Redacted.value(opaque))), decodeHex(storedHex))
    const same = (left: Redacted.Redacted<string>, right: string): boolean =>
      _sameBytes(_bytes(Redacted.value(left)), _bytes(right))
    const plugin = {
      name: "rasm-sign",
      hmac: (alg: keyof typeof _HASHES, key: Uint8Array, data: Uint8Array) => hmac(_HASHES[alg], key, data),
      randomBytes: (len: number) => { const bytes = new Uint8Array(len); reader.read(bytes); return bytes },
      constantTimeEqual,
    } as const
    const base32 = { name: "rasm-b32", encode: encodeBase32UpperCaseNoPadding, decode: decodeBase32 } as const
    return { digest, verify: verify_, derive, sign: sign_, compare, token, fingerprint, matches, same, plugin, base32 } as const
  }),
  accessors: true,
}) {}
```

## [5]-[SHREDDER]

[SHREDDER]:
- Owner: `Shredder` — the AES-GCM envelope plus AES-KW key-wrap primitive the data wave's journal imports for per-subject crypto-shredding: `mint` issues a per-subject data key, `seal`/`open` run the envelope under a 96-bit random IV, `wrap`/`unwrap` carry the data key under the master KEK, and erasure is the store dropping the `WrappedKey` — `unwrap` then fails, `open` becomes impossible, and the ciphertext journal is never rewritten.
- Law: the master KEK derives — `Crypto.derive("kek", passphrase, salt)` folds the `Config.redacted` passphrase and a pinned salt into 32 raw bytes imported once as a non-extractable AES-KW key — so KEK custody is one argon2id derivation, the passphrase never touches WebCrypto raw, and a KMS provider is a construction-row swap with the seal/open/wrap surface unchanged.
- Law: the data key never leaves the layer except as a `WrappedKey`; `SealedEnvelope` carries IV and ciphertext as opaque base64 bytes; an `open` failure is `SignFault.open` — tamper or shredded-key evidence, class `breached`.
- Growth: a second envelope suite (XChaCha via a future WebCrypto row) is one algorithm row on the same four-verb surface.
- Boundary: which wrapped key belongs to which subject, and its destruction, is the data wave's journal; this owner holds only the envelope algebra.
- Packages: WebCrypto `SubtleCrypto` (`generateKey`/`encrypt`/`decrypt`/`wrapKey`/`unwrapKey`/`importKey`); `Crypto` (`derive`).

```typescript
class SealedEnvelope extends Schema.Class<SealedEnvelope>("SealedEnvelope")({
  iv: Schema.Uint8ArrayFromBase64,
  ciphertext: Schema.Uint8ArrayFromBase64,
}) {}

class WrappedKey extends Schema.Class<WrappedKey>("WrappedKey")({
  wrapped: Schema.Uint8ArrayFromBase64,
}) {}

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
        const iv = crypto.getRandomValues(new Uint8Array(12))
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
      }).pipe(Effect.map((buf) => new Uint8Array(buf)))
    return { mint, wrap, unwrap, seal, open } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}
```

## [6]-[TOKEN_AUTHORITY]

[TOKEN_AUTHORITY]:
- Owner: `Jwt` — a Layer factory over a `Keyset`: `mint` stamps `{ alg, kid }` from the active ring key so verifiers route by `kid`; `verify` runs `createLocalJWKSet` over every published verify handle with `algorithms` pinned and the declarative claim gates applied, then decodes the payload through `AccessClaims`; `verifyExternal` resolves a per-issuer remote JWKS with cooldown throttling, `jwksCache` persistence through the `JwksLedger` port, and transport through the `JwksTransport` reference; `seal`/`unseal` are the JWE confidential profile over the keyset's optional symmetric handle.
- Law: `algorithms` is always pinned — an unpinned `alg` is accepted-algorithm confusion; the claim gates (`issuer`/`audience`/`clockTolerance`) are library-enforced options, never hand timestamp checks; `decodeJwt` is never verification.
- Law: the factory form is the rotation seam — the composition root builds the `Keyset` from `crypt/secret` credentials through `Material.ring` and wraps `Jwt.Default(keyset)` in its `Reloadable.auto` row, so a `kid` retires with zero edits here and a retired signing key keeps verifying while its handle stays published.
- Law: the remote resolver is built once per issuer under `Effect.cachedFunction` — the ledger snapshot pre-seeds the jose cache record, jose mutates it across fetches, and every successful external verify writes the record back, so a stateless invocation resumes warm; the fetch routes through `JwksTransport`, defaulted to the platform fetch and overridden by the runtime wave's instrumented client.
- Law: the JWE profile is confidentiality, not a second token system — `seal` encrypts the same `AccessClaims` under `{ alg: "dir", enc: "A256GCM" }` and `unseal` reverses it with the same claim gates; a keyset without a seal handle refuses the profile as `unsupported`.
- Receipt: `mint`/`seal` return the token `Redacted`; `verify`/`unseal` return `AccessClaims`, never a bare `JWTPayload`; `verifyExternal` returns the verified payload for the OAuth page to project its subject from.
- Growth: a new claim is one `AccessClaims` field; a new JOSE failure code is one `_codeReason` arm; a new external issuer costs nothing — the resolver memoizes per `jwksUri`.
- Packages: `jose` (`SignJWT`/`jwtVerify`/`EncryptJWT`/`jwtDecrypt`, `createLocalJWKSet`/`createRemoteJWKSet`, `jwksCache`/`customFetch` symbols, `ExportedJWKSCache`).

```typescript
class AccessClaims extends Schema.Class<AccessClaims>("AccessClaims")({
  sub: Schema.NonEmptyString,
  sid: Schema.NonEmptyString,
  scope: Schema.Array(Schema.NonEmptyString),
  tid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

type Keyset = {
  readonly ring: Ring
  readonly seal: Option.Option<Redacted.Redacted<CryptoKey>>
  readonly issuer: string
  readonly audience: string
}

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
  effect: (keyset: Keyset) =>
    Effect.gen(function* () {
      const ledger = yield* JwksLedger
      const transport = yield* JwksTransport
      const tolerance = yield* Config.integer("JWT_CLOCK_TOLERANCE").pipe(Config.withDefault(5))
      const local = createLocalJWKSet(yield* Material.jwks(keyset.ring.verify))
      const _algorithms = Array.map(keyset.ring.verify, (handle) => handle.alg)
      const _remote = yield* Effect.cachedFunction((jwksUri: string) =>
        Effect.map(ledger.load(jwksUri), (held) => {
          const record: { jwks?: JSONWebKeySet; uat?: number } = Option.getOrElse(held, () => ({}))
          const resolver = createRemoteJWKSet(new URL(jwksUri), {
            cacheMaxAge: 600_000, cooldownDuration: 30_000,
            [jwksCache]: record, [customFetch]: transport,
          })
          return { resolver, record } as const
        }))
      const _decoded = (payload: JWTPayload): Effect.Effect<AccessClaims, SignFault> =>
        Schema.decodeUnknown(AccessClaims)(payload).pipe(
          Effect.mapError((cause) => new SignFault({ reason: "claim", detail: String(cause) })))
      const mint = (claims: AccessClaims, ttl: Duration.DurationInput): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
        Effect.tryPromise({
          try: () =>
            new SignJWT({ sid: claims.sid, scope: claims.scope, ...(Option.isSome(claims.tid) && { tid: claims.tid.value }) })
              .setProtectedHeader({ alg: keyset.ring.active.alg, kid: keyset.ring.active.kid })
              .setIssuedAt().setIssuer(keyset.issuer).setAudience(keyset.audience).setSubject(claims.sub)
              .setExpirationTime(`${Math.max(1, Math.round(Duration.toSeconds(Duration.decode(ttl))))}s`)
              .sign(Redacted.value(keyset.ring.active.key)),
          catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
        }).pipe(Effect.map(Redacted.make))
      const verify = (token: Redacted.Redacted<string>): Effect.Effect<AccessClaims, SignFault> =>
        Effect.tryPromise({
          try: () => jwtVerify(Redacted.value(token), local, { algorithms: _algorithms, issuer: keyset.issuer, audience: keyset.audience, clockTolerance: tolerance }),
          catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
        }).pipe(Effect.flatMap((result) => _decoded(result.payload)))
      const verifyExternal = (token: Redacted.Redacted<string>, issuer: { readonly issuer: string; readonly audience: string; readonly jwksUri: string; readonly algorithms: ReadonlyArray<KeyAlg.Kind> }): Effect.Effect<JWTPayload, SignFault> =>
        Effect.flatMap(_remote(issuer.jwksUri), ({ resolver, record }) =>
          Effect.tryPromise({
            try: () => jwtVerify(Redacted.value(token), resolver, { algorithms: issuer.algorithms, issuer: issuer.issuer, audience: issuer.audience, clockTolerance: tolerance }),
            catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
          }).pipe(
            Effect.tap(() =>
              record.jwks !== undefined && record.uat !== undefined
                ? ledger.save(issuer.jwksUri, { jwks: record.jwks, uat: record.uat })
                : Effect.void),
            Effect.map((result) => result.payload),
          ))
      const _sealKey = Effect.mapError(keyset.seal, () => new SignFault({ reason: "unsupported", detail: "no seal handle" }))
      const seal = (claims: AccessClaims, ttl: Duration.DurationInput): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
        Effect.flatMap(_sealKey, (key) =>
          Effect.tryPromise({
            try: () =>
              new EncryptJWT({ sid: claims.sid, scope: claims.scope, ...(Option.isSome(claims.tid) && { tid: claims.tid.value }) })
                .setProtectedHeader({ alg: "dir", enc: "A256GCM" })
                .setIssuedAt().setIssuer(keyset.issuer).setAudience(keyset.audience).setSubject(claims.sub)
                .setExpirationTime(`${Math.max(1, Math.round(Duration.toSeconds(Duration.decode(ttl))))}s`)
                .encrypt(Redacted.value(key)),
            catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
          })).pipe(Effect.map(Redacted.make))
      const unseal = (token: Redacted.Redacted<string>): Effect.Effect<AccessClaims, SignFault> =>
        Effect.flatMap(_sealKey, (key) =>
          Effect.tryPromise({
            try: () => jwtDecrypt(Redacted.value(token), Redacted.value(key), { issuer: keyset.issuer, audience: keyset.audience, clockTolerance: tolerance }),
            catch: (cause) => new SignFault({ reason: _reasonOf(cause), detail: String(cause) }),
          })).pipe(Effect.flatMap((result) => _decoded(result.payload)))
      return { mint, verify, verifyExternal, seal, unseal } as const
    }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AccessClaims, Crypto, CryptoCost, Jwt, JwksLedger, JwksTransport, KeyAlg, Material, SealedEnvelope, Shredder, SignFault, WrappedKey }
export type { CredentialVerdict, KeyHandle, Keyset, Ring }
```

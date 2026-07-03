# [SECURITY_CRYPTO] — argon2 credential digest, HMAC egress signing, the AES-GCM crypto-shredding Shredder

`sign/crypto` is the one crypto primitive the whole folder delegates to: argon2id credential digest-at-rest over `@node-rs/argon2`, HMAC webhook signing and constant-time token comparison over `@oslojs/crypto`, opaque-token minting over the `RandomReader` port filled from WebCrypto, and the AES-GCM envelope `Shredder` primitive `store/journal` imports for per-subject crypto-shredding — per-subject erasure is data-key destruction, and the log is never rewritten. It is node-only (`@node-rs/argon2` is a NAPI addon), admitted in `sign/` alone, and sets the folder's fault-shape vocabulary every later page instantiates: a `Schema.TaggedError` reason-family whose `get policy()` projects one `as const` table row of `{ rank, retry, status }`, the `rank` folding an accumulated set to its dominant member, the `retry` gating a `Schedule`, the `status` the edge's problem-detail projection. Every secret — pepper, password, data key, minted token — is `Redacted` from admission and unwraps only into the primitive call. Algorithm choice is a value, never a call family: argon2 is fixed to `Argon2id`+`V0x13`, HMAC takes the `HashAlgorithm` value, and cost is a named policy row selected by credential class, never a call-site knob.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                          | [OWNER]                                   | [PACKAGES]                              | [REJECTED_FORM]                          |
| :-----: | :--------------------------------- | :---------------------------------------- | :-------------------------------------- | :--------------------------------------- |
|  [01]   | credential digest at rest          | `Crypto.digest` / `.verify` argon2 rows   | `@node-rs/argon2`                       | bcrypt, call-site cost knobs, `needsRehash` recall |
|  [02]   | egress HMAC + opaque-token digest  | `Crypto.sign`/`.compare`/`.token`/`.fingerprint`/`.matches` | `@oslojs/crypto`, `@oslojs/encoding` | `crypto.createHmac`, `===` on token bytes |
|  [03]   | otplib HMAC/base32 port bridge     | `Crypto.plugin` `CryptoPlugin`            | `@oslojs/crypto`, `@oslojs/encoding`    | otplib's default `@noble/hashes` stack   |
|  [04]   | per-subject crypto-shredding       | `Shredder` AES-GCM envelope + AES-KW wrap | WebCrypto `SubtleCrypto`                | log rewrite on erasure, DEK in plaintext |

## [2]-[FAULT_VOCABULARY]

[CRYPTO_FAULT]:
- Owner: the folder's canonical fault shape — one reason-discriminated `Schema.TaggedError` per surface, `get policy()` projecting one `as const` `{ rank, retry, status }` row, `override get message()` derived from fields. Every later `security` page instantiates this exact shape with its own reason set.
- Growth: a new crypto failure mode is one policy row plus one `reason` literal; the union derives through `keyof typeof`, and the edge's `status` and the recovery `retry`/`rank` follow with zero new branch.
- Boundary: the edge maps `policy.status` to an RFC 9457 problem detail (`edge/problem`); the `rank` lattice and `retry`-gated `Schedule` composition are `rails-and-effects.md`'s, consumed here as the settled fold. A `false` argon2 verify is never this fault — it is the `Rejected` verdict arm; `CryptoFault` fires only when a primitive throws.
- Receipt: a wrong password returns `CredentialVerdict.Rejected`, a stale-parameter match returns `Matched({ stale: true })` — the rehash signal — so the caller never re-derives the verdict from a boolean.

```typescript
import { hash, hashRaw, verify, Algorithm, Version, type Options } from "@node-rs/argon2"
import { hmac } from "@oslojs/crypto/hmac"
import { SHA1 } from "@oslojs/crypto/sha1"
import { sha256, SHA256, SHA512 } from "@oslojs/crypto/sha2"
import { constantTimeEqual } from "@oslojs/crypto/subtle"
import { generateRandomString, type RandomReader } from "@oslojs/crypto/random"
import { decodeBase32, decodeHex, encodeBase32UpperCaseNoPadding, encodeHexLowerCase } from "@oslojs/encoding"
import { Config, Data, Effect, Redacted, Schema } from "effect"

// --- [CONSTANTS] ------------------------------------------------------------------------

const _reasons = ["hash", "verify", "mac", "rng", "seal", "open", "wrap"] as const

const CryptoFaultPolicy = {
  hash: { rank: 4, retry: false, status: 500 },
  verify: { rank: 4, retry: false, status: 500 },
  mac: { rank: 3, retry: false, status: 500 },
  rng: { rank: 4, retry: false, status: 500 },
  seal: { rank: 4, retry: false, status: 500 },
  open: { rank: 5, retry: false, status: 500 },
  wrap: { rank: 5, retry: false, status: 500 },
} as const

declare namespace CryptoFault {
  type Reason = keyof typeof CryptoFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof CryptoFaultPolicy> = T
}

// --- [ERRORS] ---------------------------------------------------------------------------

class CryptoFault extends Schema.TaggedError<CryptoFault>()("CryptoFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): CryptoFault.Row {
    return CryptoFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<crypto:${this.reason}> ${this.detail}`
  }
}
```

## [3]-[CREDENTIAL_DIGEST]

[DIGEST]:
- Owner: `Crypto.digest`/`Crypto.verify` — argon2id at rest, PHC string stored verbatim, cost a named policy row keyed by credential class (`login` interactive, `apiKey` higher). `authn/apikey` and password rows delegate here; no folder re-imports `@node-rs/argon2`.
- Packages: `@node-rs/argon2` — `hash` mints the PHC, `verify` reads its embedded params (constant-time, `false` on mismatch, throws only on a malformed digest), `hashRaw` derives raw KDF bytes when the design owns its own salt.
- Law: `Argon2id`+`V0x13` pinned; the pepper is a `Config.redacted` injected once at the Layer and threaded as `secret`; the fiber's interrupt threads the `AbortSignal` so a request-scoped hash cancels; rehash-on-verify is a design fold over the PHC-embedded `m,t,p` versus the current row, since the package has no `needsRehash`.
- Growth: a new credential class is one `CryptoCost` row; a cost bump is a row edit that the rehash fold detects on the next successful verify.
- Receipt: `CredentialVerdict` — `Matched({ stale })` carries the rehash signal, `Rejected` is the ordinary auth-fail arm; a malformed stored digest is the only `CryptoFault.verify`.

```typescript
// --- [TYPES] ----------------------------------------------------------------------------

type CredentialVerdict = Data.TaggedEnum<{
  Matched: { readonly stale: boolean }
  Rejected: {}
}>

// --- [CONSTANTS] ------------------------------------------------------------------------

const CryptoCost = {
  login: { memoryCost: 19456, timeCost: 2, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
  apiKey: { memoryCost: 12288, timeCost: 3, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
} as const satisfies Record<string, Omit<Options, "secret" | "salt">>

// --- [OPERATIONS] -----------------------------------------------------------------------

const _CredentialVerdict = Data.taggedEnum<CredentialVerdict>()

const _stale = (phc: string, cost: (typeof CryptoCost)[keyof typeof CryptoCost]): boolean => {
  const parts = /\$m=(\d+),t=(\d+),p=(\d+)\$/.exec(phc)
  return parts === null || Number(parts[1]) !== cost.memoryCost || Number(parts[2]) !== cost.timeCost || Number(parts[3]) !== cost.parallelism
}
```

```typescript
// --- [SERVICES] -------------------------------------------------------------------------

class Crypto extends Effect.Service<Crypto>()("security/sign/Crypto", {
  effect: Effect.gen(function* () {
    const pepper = yield* Config.redacted("CREDENTIAL_PEPPER")
    const secret = Buffer.from(Redacted.value(pepper), "utf8")
    const reader: RandomReader = { read: (bytes) => crypto.getRandomValues(bytes) }
    const digest = (row: keyof typeof CryptoCost, plaintext: Redacted.Redacted<string>): Effect.Effect<Redacted.Redacted<string>, CryptoFault> =>
      Effect.tryPromise({
        try: (signal) => hash(Redacted.value(plaintext), { ...CryptoCost[row], secret }, signal),
        catch: (cause) => new CryptoFault({ reason: "hash", detail: String(cause) }),
      }).pipe(Effect.map(Redacted.make))
    const verify_ = (row: keyof typeof CryptoCost, stored: Redacted.Redacted<string>, plaintext: Redacted.Redacted<string>): Effect.Effect<CredentialVerdict, CryptoFault> =>
      Effect.tryPromise({
        try: (signal) => verify(Redacted.value(stored), Redacted.value(plaintext), { secret }, signal),
        catch: (cause) => new CryptoFault({ reason: "verify", detail: String(cause) }),
      }).pipe(Effect.map((matched) => (matched ? _CredentialVerdict.Matched({ stale: _stale(Redacted.value(stored), CryptoCost[row]) }) : _CredentialVerdict.Rejected())))
    const derive = (row: keyof typeof CryptoCost, seed: Redacted.Redacted<string>, salt: Uint8Array): Effect.Effect<Redacted.Redacted<Uint8Array>, CryptoFault> =>
      Effect.tryPromise({
        try: (signal) => hashRaw(Redacted.value(seed), { ...CryptoCost[row], secret, salt }, signal),
        catch: (cause) => new CryptoFault({ reason: "hash", detail: String(cause) }),
      }).pipe(Effect.map((buf) => Redacted.make(new Uint8Array(buf))))
    const sign_ = (key: Redacted.Redacted<Uint8Array>, body: Uint8Array): Effect.Effect<string, CryptoFault> =>
      Effect.try({ try: () => encodeHexLowerCase(hmac(SHA256, Redacted.value(key), body)), catch: (cause) => new CryptoFault({ reason: "mac", detail: String(cause) }) })
    const compare = (key: Redacted.Redacted<Uint8Array>, body: Uint8Array, signature: string): Effect.Effect<boolean, CryptoFault> =>
      Effect.try({ try: () => constantTimeEqual(hmac(SHA256, Redacted.value(key), body), decodeHex(signature)), catch: (cause) => new CryptoFault({ reason: "mac", detail: String(cause) }) })
    const token = (alphabet: string, length: number): Effect.Effect<Redacted.Redacted<string>, CryptoFault> =>
      Effect.try({ try: () => Redacted.make(generateRandomString(reader, alphabet, length)), catch: (cause) => new CryptoFault({ reason: "rng", detail: String(cause) }) })
    const fingerprint = (opaque: Redacted.Redacted<string>): string =>
      encodeHexLowerCase(sha256(new TextEncoder().encode(Redacted.value(opaque))))
    const matches = (opaque: Redacted.Redacted<string>, storedHex: string): boolean =>
      constantTimeEqual(sha256(new TextEncoder().encode(Redacted.value(opaque))), decodeHex(storedHex))
    const same = (left: Redacted.Redacted<string>, right: string): boolean =>
      constantTimeEqual(new TextEncoder().encode(Redacted.value(left)), new TextEncoder().encode(right))
    const plugin = {
      name: "rasm-sign",
      hmac: (alg: string, key: Uint8Array, data: Uint8Array) => hmac(alg === "sha512" ? SHA512 : alg === "sha256" ? SHA256 : SHA1, key, data),
      randomBytes: (len: number) => { const bytes = new Uint8Array(len); reader.read(bytes); return bytes },
      constantTimeEqual,
    } as const
    const base32 = { name: "rasm-b32", encode: encodeBase32UpperCaseNoPadding, decode: decodeBase32 } as const
    return { digest, verify: verify_, derive, sign: sign_, compare, token, fingerprint, matches, same, plugin, base32 } as const
  }),
  accessors: true,
}) {}
```

## [4]-[CRYPTO_SHREDDER]

[SHREDDER]:
- Owner: `Shredder` — the AES-GCM envelope + AES-KW key-wrap primitive `store/journal` imports for per-subject crypto-shredding (the `sign/crypto → store/journal [SHAPE]` seam). It mints a per-subject data key, seals/opens payloads under it, and wraps/unwraps it under a Layer-injected master KEK; `store` persists the wrapped key and destroys that row to erase a subject.
- Packages: WebCrypto `SubtleCrypto` — `generateKey`/`encrypt`/`decrypt` (AES-GCM, 96-bit random IV), `wrapKey`/`unwrapKey` (AES-KW) — the isomorphic platform primitive, threaded through `Effect.tryPromise` at the seam.
- Boundary: `store/journal` owns which wrapped key belongs to which subject and its destruction; `Shredder` owns only the envelope algebra. Erasure is the store dropping the `WrappedKey` — `unwrap` then fails `CryptoFault.wrap`, `open` becomes impossible, and the ciphertext journal is never rewritten.
- Receipt: `SealedEnvelope` carries the IV and ciphertext as opaque bytes; the data key never leaves the layer except as a `WrappedKey` (KEK-encrypted, opaque).
- Growth: a KMS/HSM key-custody provider is a prepared `Shredder` construction row swapping the master-KEK source; the seal/open/wrap surface is unchanged.

```typescript
// --- [MODELS] ---------------------------------------------------------------------------

class SealedEnvelope extends Schema.Class<SealedEnvelope>("SealedEnvelope")({
  iv: Schema.Uint8ArrayFromBase64,
  ciphertext: Schema.Uint8ArrayFromBase64,
}) {}

class WrappedKey extends Schema.Class<WrappedKey>("WrappedKey")({
  wrapped: Schema.Uint8ArrayFromBase64,
}) {}

// --- [SERVICES] -------------------------------------------------------------------------

class Shredder extends Effect.Service<Shredder>()("security/sign/Shredder", {
  effect: Effect.gen(function* () {
    const master = yield* Config.redacted("SHRED_MASTER_KEY")
    const kek = yield* Effect.tryPromise({
      try: () => crypto.subtle.importKey("raw", decodeHex(Redacted.value(master)), { name: "AES-KW" }, false, ["wrapKey", "unwrapKey"]),
      catch: (cause) => new CryptoFault({ reason: "wrap", detail: String(cause) }),
    })
    const mint = (): Effect.Effect<Redacted.Redacted<CryptoKey>, CryptoFault> =>
      Effect.tryPromise({ try: () => crypto.subtle.generateKey({ name: "AES-GCM", length: 256 }, true, ["encrypt", "decrypt"]), catch: (cause) => new CryptoFault({ reason: "seal", detail: String(cause) }) }).pipe(Effect.map(Redacted.make))
    const wrap = (dataKey: Redacted.Redacted<CryptoKey>): Effect.Effect<WrappedKey, CryptoFault> =>
      Effect.tryPromise({ try: () => crypto.subtle.wrapKey("raw", Redacted.value(dataKey), kek, "AES-KW"), catch: (cause) => new CryptoFault({ reason: "wrap", detail: String(cause) }) }).pipe(Effect.map((buf) => new WrappedKey({ wrapped: new Uint8Array(buf) })))
    const unwrap = (key: WrappedKey): Effect.Effect<Redacted.Redacted<CryptoKey>, CryptoFault> =>
      Effect.tryPromise({ try: () => crypto.subtle.unwrapKey("raw", key.wrapped, kek, "AES-KW", { name: "AES-GCM" }, false, ["encrypt", "decrypt"]), catch: (cause) => new CryptoFault({ reason: "wrap", detail: String(cause) }) }).pipe(Effect.map(Redacted.make))
    const seal = (dataKey: Redacted.Redacted<CryptoKey>, plaintext: Uint8Array): Effect.Effect<SealedEnvelope, CryptoFault> =>
      Effect.gen(function* () {
        const iv = crypto.getRandomValues(new Uint8Array(12))
        const ciphertext = yield* Effect.tryPromise({ try: () => crypto.subtle.encrypt({ name: "AES-GCM", iv }, Redacted.value(dataKey), plaintext), catch: (cause) => new CryptoFault({ reason: "seal", detail: String(cause) }) })
        return new SealedEnvelope({ iv, ciphertext: new Uint8Array(ciphertext) })
      })
    const open = (dataKey: Redacted.Redacted<CryptoKey>, envelope: SealedEnvelope): Effect.Effect<Uint8Array, CryptoFault> =>
      Effect.tryPromise({ try: () => crypto.subtle.decrypt({ name: "AES-GCM", iv: envelope.iv }, Redacted.value(dataKey), envelope.ciphertext), catch: (cause) => new CryptoFault({ reason: "open", detail: String(cause) }) }).pipe(Effect.map((buf) => new Uint8Array(buf)))
    return { mint, wrap, unwrap, seal, open } as const
  }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Crypto, CryptoFault, SealedEnvelope, Shredder, WrappedKey }
export type { CredentialVerdict }
```

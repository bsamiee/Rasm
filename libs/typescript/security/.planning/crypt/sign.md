# [SECURITY_SIGN]

One crypto authority: argon2id credential digest-at-rest under a semaphore bulkhead, HMAC egress signing, opaque-token minting, the AES-GCM crypto-shredding `Shredder`, jose key-material admission with RFC 7638 thumbprint identity, and the JWT/JWS/JWKS/JWE token authority — one module because every concern shares one key plane and one fault family. Key material enters exactly once, through `Material.admit`, and the `Material.Source` case a caller holds IS its trust boundary: the peer-attested `Credential` landing publishes a public chain and admits through `importSPKI`/`importX509` alone, this folder's own host-held bundle is the only source `importPKCS8` ever reads, and a published JWKS entry decodes once through `Schema.parseJson`. Private key material never crosses the AppHost wire and no second import path exists for a Doppler-fetched, peer-attested, or self-minted key; the `kid` is the producer's redacted key-id or the computed thumbprint, `CryptoKey.type` decides signing versus verify, and the lease window gates the one source that carries one. `Jwt` mints with the active ring key, verifies against the local JWKS or a remote per-issuer resolver through one overloaded `verify` discriminating on the issuer descriptor, keeps the remote cache warm with a `Schedule`-driven proactive `reload`, bounds every remote resolve with a deadline and a jittered retry gated on `FaultClass.retryable`, and seals confidential claims as JWE. Every secret — pepper, password, data key, minted token, private handle — is `Redacted` from admission and unwraps only into the primitive call; algorithm, cost, permit budget, cache age, and reason are vocabulary rows or `Config` policy values, never call-site knobs. Cost rows are bench-graded, never copied folklore: `Calibration` measures every `CryptoCost` row on the executing host into core `Claim` receipts — the `BenchmarkClaimWire` family with foreign-host admission — graded against each row's own `targetMs` ceiling, never against the KDF distribution's buckets — those freeze on the Convention row, so a cost bump moves a grade and re-buckets nothing. Every crypto surface rides its span and metric at the declaration seam — KDF latency, JWKS resolve latency, cold-miss and quarantine counters, each instrument mounted from its core `Convention` row — so the runtime wave's OTLP lane exports the folder's audit stream with zero call-site change. `SignFault` is the folder's canonical fault shape: one reason family whose rows carry the core `FaultClass` classification and close at the core `FaultClass.family` seam, so retryability, dominance, and blame derive from the branch table and the serving edge folds `class` to status through its own governed record.

## [01]-[INDEX]

- [02]-[FAULT_AND_ALG]: `SignFault`, `KeyAlg`.
- [03]-[KEY_MATERIAL]: `Material`, `Material.Source`, `KeyHandle`, `Ring`.
- [04]-[CRYPTO_PRIMITIVE]: `Crypto`, `CredentialVerdict`, `Probe`.
- [05]-[SHREDDER]: `Shredder`.
- [06]-[TOKEN_AUTHORITY]: `Jwt`, `AccessClaims`, `JwksSnapshot`, `JwksLedger`.
- [07]-[CALIBRATION]: `Calibration`.

## [02]-[FAULT_AND_ALG]

[FAULT_AND_ALG]:
- Owner: `SignFault` — the one reason-discriminated `Schema.TaggedError` every page in this folder instantiates with its own reason set; each row carries the core `FaultClass` kind, `get class()` projects it so `FaultClass.of` classifies structurally, and `override get message()` derives from fields. `KeyAlg` is the bounded signature-scheme vocabulary — each row carries `{ kty, crv?, use }`, the discriminant derives through `keyof typeof`, and a new scheme is one row.
- Law: rows carry `class` only — rank, retryability, and blame derive from the branch `FaultClass` table, and the class-to-status projection is the serving edge's governed record; a local `{ rank, retry, status }` triple beside the class column is the split-brain this shape kills.
- Law: the family seam closes the reason vocabulary by construction — `FaultClass.family` freezes the reason tuple and its exact-key row set, deriving the literal schema and the `classOf` projection, so a dead row or a rowless reason fails at the mint and no local guard pair exists here or on any sibling family; `KeyAlg` keeps its own `_Keys`/`_Kinds` pair because a scheme table is vocabulary, not a fault family.
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
import { Budget, Claim, Convention, Credential, FaultClass, type AppIdentity, type WireFault } from "@rasm/ts/core"
import {
  calculateJwkThumbprint, calculateJwkThumbprintUri, createLocalJWKSet, createRemoteJWKSet, EncryptJWT, exportJWK,
  generateKeyPair, importJWK, importPKCS8, importSPKI, importX509, jwtDecrypt, jwtVerify, SignJWT, customFetch, jwksCache,
  type ExportedJWKSCache, type JSONWebKeySet, type JWK, type JWTPayload,
} from "jose"
import type { JWK as CachedJwk } from "openid-client"
import {
  Array, Cause, Clock, Config, Context, Data, DateTime, Duration, Effect, HashMap, Layer, Metric, Number, Option, Order, Predicate, Redacted,
  Ref, Schedule, Schema, Struct, pipe,
} from "effect"
import { SecurityFact, Witness } from "../access/audit.ts"

const _family = FaultClass.family(
  [
    "digest", "mac", "rng", "seal", "open", "wrap", "throttled",
    "material", "unsupported", "window",
    "expired", "claim", "signature", "algorithm", "jwks", "malformed",
  ] as const,
  {
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
  },
)

const _algs = ["ES256", "ES384", "RS256", "EdDSA"] as const

const KeyAlg = {
  ES256: { kty: "EC", crv: "P-256", use: "sig" },
  ES384: { kty: "EC", crv: "P-384", use: "sig" },
  RS256: { kty: "RSA", use: "sig" },
  EdDSA: { kty: "OKP", crv: "Ed25519", use: "sig" },
} as const

declare namespace SignFault {
  type Reason = (typeof _family.reasons)[number]
}

declare namespace KeyAlg {
  type Kind = keyof typeof KeyAlg
  type Row = (typeof KeyAlg)[Kind]
  type _Keys<K extends Kind = (typeof _algs)[number]> = K
  type _Kinds<K extends (typeof _algs)[number] = Kind> = K
}

class SignFault extends Schema.TaggedError<SignFault>()("SignFault", {
  reason: _family.schema,
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<sign:${this.reason}> ${this.detail}`
  }
}
```

## [03]-[KEY_MATERIAL]

[KEY_MATERIAL]:
- Owner: `Material` — the assembled key-material owner: `Source` the admission-source family, `admit` the one fold from any source into a `KeyHandle`, `mint` self-issues an ephemeral non-extractable ring for a KMS-less bootstrap or test composition, `ring` narrows a signing source and a published JWKS into the `{ active, verify }` set `Jwt` consumes, `jwks` projects the verify handles back to a `JSONWebKeySet` for publication, and `thumbprint`/`thumbprintUri` are the RFC 7638 identity mints — the bare form is the `cnf.jkt` confirmation value a sender-constrained token carries, the URI form the stable subject a key-named principal reads. This owner is every source's terminus: the handle never crosses back to a wire and never reaches a log.
- Law: PRIVATE KEY MATERIAL NEVER ARRIVES OFF THE WIRE — `Source` carries one case per trust boundary and the boundary decides what a case can hold. `Attested` is the core `Credential` landing whose producer publishes the public chain, the RFC-7468 label set, and the block digests alone, so its admission reaches `importSPKI`/`importX509` and nothing else; a landing whose `sealed` read answers true is evidence the peer mint leaked and refuses as `material` rather than importing. `Held` is this folder's own host-side material — `crypt/secret` seals a Doppler-leased bundle into it and `Shredder` keys never leave the layer — and it alone reaches `importPKCS8`. `Published` is a remote JWKS entry. One entrypoint over three cases beats an `admitWire`/`admitHost` pair, because the case IS the trust boundary and a caller cannot pick the wrong one.
- Law: the importer is a row read over the core RFC-7468 vocabulary, never a header sniff — `_PEM` keys `Credential.Label` to its jose importer, `PKCS7` carries none and refuses `unsupported`, and the prefix ladder that inferred a format from four hardcoded armor strings silently fell through to the JWK arm for every label outside its three.
- Law: the handle side is the KEY's own witness — `CryptoKey.type` answers `"private"` or `"public"` after the import, so `Signing` and `Verify` derive from what the platform produced rather than from a re-parsed `d` field or a caller-declared role; a symmetric `importJWK` result is `unsupported`.
- Law: the validity window rides the source that carries one — `Held` states its lease bounds and an instant outside them is `SignFault.window`; an `Attested` landing carries the producer's mint instant alone, so its retirement is the producer's own lease observed as a fresh landing and `Credential.rotated` compares the bundle digest across the two.
- Law: `ring` accumulates — `Effect.partition` admits every satisfying published key and quarantines each malformed entry onto the `Convention.metric.securityJwksQuarantined` counter, a warning log, and an `Admission` fact through `Witness`, so one rotated-in bad key never collapses the verify set and the quarantine is receipt-truth; an empty surviving set is the only `material` failure. The synthetic verify carrier and its horizon parameter are gone with it — `Published` admits a JWKS entry directly, so no fabricated window and no `"verify"` role outside the credential vocabulary is minted to reach the same import.
- Growth: a new signature scheme is one `KeyAlg` row; a new armor label is one `_PEM` row beside the core vocabulary row that mints it; a new material source (KMS, HSM) is one `Source` case terminating through the same `admit`; a detached-signature or co-signed-document surface is a `GeneralSign` row over the same handles.
- Boundary: `crypt/secret` mints `Material.Source.Held` from fetched material and is the only host-side key source this folder owns; the core interchange codec decodes the `csharp:Rasm.AppHost/Runtime/secrets#CREDENTIAL_PEM`-produced `CredentialPemWire` into the `Credential` landing `Attested` carries under that mint's `json` arm; proving a landing's chain against its `blockDigests` is the producer's non-cryptographic content-identity fold and stays the codec's parity concern; `Jwt` is the only consumer that unwraps `Signing`, and the external-verify page consumes `Verify` handles only through `jwks`.
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

// One case per trust boundary: the peer-attested landing whose producer publishes public blocks only, this
// folder's own host-held material under its lease window, and a remote JWKS entry.
type _Source = Data.TaggedEnum<{
  Attested: { readonly credential: Credential }
  Held: {
    readonly bundle: Redacted.Redacted<string>
    readonly fingerprint: string
    readonly notBefore: DateTime.Utc
    readonly notAfter: DateTime.Utc
  }
  Published: { readonly jwk: JWK }
}>

const _KeyHandle = Data.taggedEnum<KeyHandle>()
const _Source: Data.TaggedEnum.Constructor<_Source> = Data.taggedEnum<_Source>()

const _quarantined = Convention.mount(Convention.metric.securityJwksQuarantined)

const _material = (cause: unknown): SignFault => new SignFault({ reason: "material", detail: String(cause) })

const _Jwk = Schema.parseJson(Schema.Struct({ kty: Schema.String }, { key: Schema.String, value: Schema.Unknown }))
const _jwkBody = Schema.decodeUnknown(_Jwk)
const _scheme = Schema.decodeUnknown(Schema.Literal(..._algs))

// The importer keys off the core RFC-7468 label vocabulary, so `importPKCS8` is reachable from a private label
// alone — a label the AppHost mint never writes — and PKCS7 refuses rather than falling through to a JWK parse.
const _PEM = {
  "CERTIFICATE": Option.some(importX509),
  "PUBLIC KEY": Option.some(importSPKI),
  "PKCS7": Option.none<(pem: string, alg: string) => Promise<CryptoKey>>(),
  "PRIVATE KEY": Option.some(importPKCS8),
  "EC PRIVATE KEY": Option.some(importPKCS8),
  "RSA PRIVATE KEY": Option.some(importPKCS8),
} as const satisfies Record<Credential.Label, Option.Option<(pem: string, alg: string) => Promise<CryptoKey>>>

const _ARMOR = /-----BEGIN ([A-Z0-9 ]+)-----/
const _BLOCK = /-----BEGIN ([A-Z0-9 ]+)-----[\s\S]*?-----END \1-----/g
const _label = Schema.decodeUnknownOption(Credential.Label)

const _labelOf = (armored: string): Option.Option<Credential.Label> =>
  Option.flatMap(Option.fromNullable(_ARMOR.exec(armored)), (found) => _label(found[1]))

const _handleOf = (key: CryptoKey, kid: string, alg: KeyAlg.Kind): KeyHandle =>
  key.type === "private"
    ? _KeyHandle.Signing({ kid, alg, key: Redacted.make(key) })
    : _KeyHandle.Verify({ kid, alg, key: Redacted.make(key) })

const _armored = (block: string, alg: KeyAlg.Kind, kid: string): Effect.Effect<KeyHandle, SignFault> =>
  Option.match(Option.flatMap(_labelOf(block), (label) => _PEM[label]), {
    onNone: () => Effect.fail(new SignFault({ reason: "unsupported", detail: "<unimportable-armor>" })),
    onSome: (admit) =>
      Effect.map(Effect.tryPromise({ try: () => admit(block, alg), catch: _material }), (key) => _handleOf(key, kid, alg)),
  })

const _fromJwk = (jwk: JWK, alg: KeyAlg.Kind, kid: string): Effect.Effect<KeyHandle, SignFault> =>
  Effect.tryPromise({ try: () => importJWK(jwk, alg), catch: _material }).pipe(
    Effect.filterOrFail(
      (held): held is CryptoKey => !(held instanceof Uint8Array),
      () => new SignFault({ reason: "unsupported", detail: "symmetric jwk material" }),
    ),
    Effect.map((key) => _handleOf(key, kid, alg)),
  )

const _admit = (source: Material.Source, alg: KeyAlg.Kind): Effect.Effect<KeyHandle, SignFault> =>
  _Source.$match(source, {
    // the wire arm reads the LEAF block: the producer's bundle order is the chain order, and `sealed` is the
    // broken-mint refusal — a private label here means material crossed that the carrier law forbids
    Attested: ({ credential }) =>
      credential.sealed
        ? Effect.fail(new SignFault({ reason: "material", detail: `<attested-private-label:${credential.fingerprint}>` }))
        : Option.match(Option.flatMap(Option.fromNullable(credential.chain.match(_BLOCK)), Array.head), {
            onNone: () => Effect.fail(new SignFault({ reason: "material", detail: "<unarmored-chain>" })),
            onSome: (leaf) => _armored(leaf, alg, credential.fingerprint),
          }),
    // the lease window gates the host arm alone, because it is the only source that carries one
    Held: ({ bundle, fingerprint, notBefore, notAfter }) =>
      Effect.flatMap(
        // one unwrap, deferred past the window gate: the sealed bundle never exists raw ahead of its own admission
        Effect.map(
          Effect.filterOrFail(
            DateTime.now,
            (now) => DateTime.between(now, { minimum: notBefore, maximum: notAfter }),
            () => new SignFault({ reason: "window", detail: fingerprint }),
          ),
          () => Redacted.value(bundle),
        ),
        (text) =>
          Option.match(_labelOf(text), {
            onNone: () => Effect.flatMap(_jwkBody(text).pipe(Effect.mapError(_material)), (jwk) => _fromJwk(jwk, alg, fingerprint)),
            onSome: () => _armored(text, alg, fingerprint),
          }),
      ),
    Published: ({ jwk }) =>
      Effect.gen(function* () {
        const kid = yield* Option.match(Option.fromNullable(jwk.kid), { onSome: Effect.succeed, onNone: () => Material.thumbprint(jwk) })
        const scheme = yield* _scheme(jwk.alg ?? alg).pipe(
          Effect.mapError(() => new SignFault({ reason: "unsupported", detail: String(jwk.alg) })))
        return yield* _fromJwk(jwk, scheme, kid)
      }),
  })

declare namespace Material {
  type Source = _Source
}

const Material = {
  Source: _Source,
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
  ring: (signing: Material.Source, alg: KeyAlg.Kind, published: JSONWebKeySet): Effect.Effect<Ring, SignFault> =>
    Effect.gen(function* () {
      const active = yield* _admit(signing, alg).pipe(Effect.filterOrFail(
        (handle): handle is Extract<KeyHandle, { readonly _tag: "Signing" }> => handle._tag === "Signing",
        () => new SignFault({ reason: "material", detail: "signing source resolved public" }),
      ))
      // each published entry admits as its own source: no synthetic carrier, no fabricated window, no role
      // literal outside the credential vocabulary standing in to reach one import
      const [excluded, verify] = yield* Effect.partition(published.keys, (jwk) =>
        _admit(_Source.Published({ jwk }), alg).pipe(Effect.filterOrFail(
          (handle): handle is Extract<KeyHandle, { readonly _tag: "Verify" }> => handle._tag === "Verify",
          () => new SignFault({ reason: "material", detail: "jwks entry resolved private" }),
        )))
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
- Law: every KDF call runs inside the semaphore bulkhead — `login`/`apiKey` rows take one permit, the `kek` derive takes the whole budget, so a login storm queues at the `CRYPTO_KDF_PERMITS` bound instead of spawning unbounded 19–64MB hashes; each call rides the `Convention.metric.securityKdf` distribution and its span, and the fiber's interrupt threads the `AbortSignal` so a request-scoped hash cancels with its caller.
- Law: cost is a named `CryptoCost` row selected by credential class — `login` interactive, `apiKey` machine, `kek` the derive row backing the `Shredder` master key — with `Argon2id`+`V0x13` pinned; the pepper is one `Config.redacted` injected at construction and threaded as `secret`.
- Law: `verify` reads the PHC-embedded parameters, and a match under stale parameters returns `Matched({ stale: true })` — the rehash signal the caller persists on; `Rejected` is the ordinary auth-fail arm and only a malformed stored digest throws into `SignFault.digest`.
- Law: every compare routes constant-time through one `matches` — length is the only short-circuit, a length mismatch is `false`, a malformed stored hex is `SignFault.mac`, never an uncaught throw; a stored argon2 digest is checked by argon2's own constant-time `verify` and never re-compared through `constantTimeEqual`; the otplib `hmac` port dispatches the `HashAlgorithm` value off the `_HASHES` row table so a new hash is a row, never a name fork.
- Law: a port surrenders its operand contract, never its primitive — the OTP `crypto` port's compare admits `string | Uint8Array` because the strategy hands it two token strings, so `plugin.constantTimeEqual` lifts both operands to bytes before the byte-domain primitive runs; handing that primitive over bare type-checks under method-shorthand bivariance and then compares characters, which is an accept-everything gate for any alphabet outside the digits.
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

const _argonMs = Convention.mount(Convention.metric.securityKdf)

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
      // The OTP port's compare is called with the two TOKEN STRINGS, never bytes, so the primitive is lifted here
      // rather than handed over bare: the oslo compare indexes its operands, and on a string that read yields
      // characters whose XOR is NaN outside the digit alphabet — every character of a non-numeric variant then
      // compares equal and the whole token passes. Method-shorthand bivariance hides the mismatch at the type
      // level, so the normalization is the only thing standing between a hooks-encoded dialect and a blanket accept.
      constantTimeEqual: (left: string | Uint8Array, right: string | Uint8Array) =>
        _sameBytes(typeof left === "string" ? _bytes(left) : left, typeof right === "string" ? _bytes(right) : right),
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
- Law: the data key never leaves the layer except as a `WrappedKey`; `SealedEnvelope` carries IV and ciphertext as opaque base64 bytes; an `open` failure is `SignFault.open` — tamper or shredded-key evidence, class `breached` — and increments the `Convention.metric.securityShredReject` counter and publishes the `ShredOpen` fact through `Witness` before it surfaces; the crypto floor mints its own Convention row because the folder reject stream composes one stratum above, while the fact floor sits below it.
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

const _openReject = Convention.mount(Convention.metric.securityShredReject)

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
- Law: the factory form is the rotation seam — the composition root builds the `Keyset` from `crypt/secret`'s `Material.Source.Held` values through `Material.ring`, wraps `Jwt.Default(keyset)` in `Reloadable.auto` driven by `Secret.changes`, so a Doppler rotation republishes the ring without a graph teardown, a `kid` retires with zero edits here, and a retired signing key keeps verifying while its handle stays published.
- Law: `JwksSnapshot` is the ledger's own shape and the folder's single JWKS custody — it carries the key set beside the instant this owner observed it, never a package's `uat` scalar, because jose stamps that field in epoch MILLISECONDS off `Date.now()` while the certified relying-party client stamps it in epoch SECONDS: one stored number read under the wrong unit either reads as 1970 and refetches on every call or reads as the far future and never refetches a rotated key. The unit is therefore a per-seam projection off one owned instant — `JwksSnapshot.jose` renders the millisecond form this page seeds — and every other consumer of the same ledger projects its own.
- Law: the remote resolver is built once per issuer under `Effect.cachedFunction` — the ledger snapshot seeds jose's cache through that projection, and a scoped fiber drives `resolver.reload()` on a jittered `Schedule.spaced(cacheAge)` so a provider key roll lands before the first `kid` miss; the tick asks only where the ask can land, gating on the resolver's own published `fresh`/`coolingDown`/`reloading` state so it stops issuing reloads jose refuses inside `cooldownDuration` and stops refetching an already-fresh set every `cacheAge` span. Each landed reload and each successful verify persists through `JwksLedger` from `resolver.jwks()` — the resolver's own accessor, so no mutable record survives the closure — and a tick whose reload genuinely failed logs at warning while an interrupted teardown stays silent; a cold build increments the `Convention.metric.securityJwksMiss` counter.
- Law: every remote verify is internally resilient — a `deadline` timeout, the branch `Budget.schedule("pulse")` compile whose jitter, attempt bound, quiet-reset, and elapsed ceiling arrive as one value under the owner's own `FaultClass.retryable` gate, the `Convention.metric.securityJwksResolve` distribution, and its span; the fetch routes through `JwksTransport`, defaulted to the platform fetch and bound by the runtime wave to its instrumented `HttpClient.retryTransient({ schedule }).pipe(HttpClient.withTracerPropagation)` fetch adapter so rotation inherits the shared net policy and W3C trace propagation.
- Law: the JWE profile is confidentiality, not a second token system — `seal` encrypts the same `AccessClaims` under `{ alg: "dir", enc: "A256GCM" }` and `unseal` reverses it with the same claim gates; a keyset without a seal handle refuses the profile as `unsupported`.
- Receipt: `mint`/`seal` return the token `Redacted`; `verify`/`unseal` return `AccessClaims`, never a bare `JWTPayload`; the issuer overload returns the verified payload.
- Growth: a new claim is one `AccessClaims` field; a new JOSE failure code is one `_codeReason` arm; a new external issuer costs nothing — the resolver memoizes per `jwksUri`.
- Packages: `jose` (`SignJWT`/`jwtVerify`/`EncryptJWT`/`jwtDecrypt`, `createLocalJWKSet`/`createRemoteJWKSet`, the resolver's `reload`/`fresh`/`coolingDown`/`reloading`/`jwks` state, `jwksCache`/`customFetch` symbols, `ExportedJWKSCache`); `effect` (`Schedule`, `Metric`, `Effect.cachedFunction`, `Effect.forkScoped`, `Effect.unless`, `Schema.declare`/`Schema.mutable`); `@rasm/ts/core` (`Budget`, `Convention`).

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

const _jwksMiss = Convention.mount(Convention.metric.securityJwksMiss)
const _jwksMs = Convention.mount(Convention.metric.securityJwksResolve)

// A JWK is a provider-shaped open record, not a class instance, so the cell admits by guard and its codec is
// identity — the stored bytes are already the JSON the provider served. The cell takes the certified client's
// index-signature-bearing spelling because that one flows into BOTH consumers, where the closed jose interface flows
// into neither; the reverse crossing is the decode below, which is where a stored value belongs anyway.
const _CachedJwk = Schema.declare((input: unknown): input is CachedJwk => Predicate.isRecord(input), { identifier: "CachedJwk" })

class JwksSnapshot extends Schema.Class<JwksSnapshot>("JwksSnapshot")({
  keys: Schema.mutable(Schema.Array(_CachedJwk)),
  observedAt: Schema.DateTimeUtc,
}) {
  static readonly jose = (snapshot: JwksSnapshot): ExportedJWKSCache => ({
    jwks: { keys: snapshot.keys },
    uat: DateTime.toEpochMillis(snapshot.observedAt),
  })
}

class JwksLedger extends Context.Tag("security/crypt/JwksLedger")<JwksLedger, {
  readonly load: (issuer: string) => Effect.Effect<Option.Option<JwksSnapshot>>
  readonly save: (issuer: string, snapshot: JwksSnapshot) => Effect.Effect<void>
}>() {
  static readonly memory: Layer.Layer<JwksLedger> = Layer.effect(
    JwksLedger,
    Effect.map(Ref.make(HashMap.empty<string, JwksSnapshot>()), (cell) => ({
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
          const resolver = createRemoteJWKSet(new URL(jwksUri), {
            cacheMaxAge: Duration.toMillis(cacheAge), cooldownDuration: Duration.toMillis(cooldown),
            timeoutDuration: Duration.toMillis(deadline),
            [jwksCache]: Option.match(held, { onNone: () => ({}), onSome: JwksSnapshot.jose }),
            [customFetch]: transport,
          })
          // The snapshot reads the resolver's own accessor and stamps OUR observation instant, so no mutable record
          // survives the closure and the ledger's unit stays the owner's rather than whichever library wrote last.
          const persist = Effect.flatMap(DateTime.now, (observedAt) =>
            Option.match(Option.fromNullable(resolver.jwks()), {
              onNone: () => Effect.void,
              onSome: (set) =>
                Schema.decodeUnknown(JwksSnapshot)({ keys: set.keys, observedAt: DateTime.formatIso(observedAt) }).pipe(
                  Effect.mapError((cause) => new SignFault({ reason: "jwks", detail: String(cause) })),
                  Effect.flatMap((snapshot) => ledger.save(jwksUri, snapshot)),
                ),
            }))
          yield* Effect.forkScoped(Effect.repeat(
            // The tick asks only where the ask can land — jose refuses a reload inside its own cooldown, answers a
            // fresh set from cache, and holds one reload at a time — so the guard reads the resolver's published
            // state and an interrupted teardown stays distinguishable from a permanently failing rotation.
            Effect.unless(
              Effect.zipRight(
                Effect.tryPromise({ try: () => resolver.reload(), catch: (cause) => new SignFault({ reason: "jwks", detail: String(cause) }) }),
                persist,
              ),
              () => resolver.fresh || resolver.coolingDown || resolver.reloading,
            ).pipe(
              Effect.tapErrorCause((cause) =>
                Cause.isInterruptedOnly(cause) ? Effect.void : Effect.logWarning("jwks proactive reload failed", cause)),
              Effect.ignore,
            ),
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
            Effect.retry(Budget.schedule("pulse")), // the branch compile, gate included: jitter, attempt bound, reset, and elapsed ceiling arrive as one value
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
- Law: trials run the production shape — `calibrate` probes `Crypto.digest` per row, so every trial passes the semaphore bulkhead, the `Convention.metric.securityKdf` distribution, and the KDF span exactly as a login does; a bench that bypasses the bulkhead measures a machine that does not exist.
- Law: receipts are the core claim family — each metric carries `Claim.Band`'s sample count beside the rungs this probe measured, its optional `ticks`, raw `samples`, and enrichment bands, the host rides `Claim["host"]`, and every returned claim passes `Claim.admit` before grading; the grade names `p99`, so a probe that never measured that rung refuses at the board's rung axis rather than reading an absent value. Core's codec maps the same class to `BenchmarkClaimWire`; no security-local wire exists.
- Law: throughput claims ride the same family — a `Jwt` mint or verify-fold claim is one `bench` call over that probe with its own suite key; a second receipt shape per crypto surface is the forked-family defect.
- Growth: a new statistic is one metrics row inside `_receipt`; a new bench subject is one `bench` call; a new credential class inherits its target row through the `CryptoCost` guard.
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

export { AccessClaims, Calibration, Crypto, CryptoCost, Jwt, JwksLedger, JwksSnapshot, JwksTransport, KeyAlg, Material, Probe, SealedEnvelope, Shredder, SignFault, WrappedKey }
export type { CredentialVerdict, IssuerRef, KeyHandle, Keyset, Ring, SingleUse }
```

## [08]-[RESEARCH]

(none)

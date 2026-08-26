# [SECURITY_SIGN]

Sole crypto mint: every digest, signature, token, and envelope the folder emits originates here — the `SignFault` family and `KeyAlg` scheme table, `Material` as the one key-admission terminus over three trust-boundary sources, the `Crypto` primitive surface (argon2id at-rest digests under a bulkheaded cost-row table, HMAC egress signing, the one constant-time `matches` entrypoint, entropy-port token and uuid mints over the shared `Alphabet` rows), the `Shredder` AES-GCM/AES-KW envelope behind per-subject crypto-shredding, the `Jwt` token authority over a ring it swaps live on rotation with the folder's one JWKS custody and its one `SingleUse` satisfier, and `Calibration` grading each cost row against its own target on its own production member. No sibling imports `@node-rs/argon2`, `@noble/hashes`, or `jose` directly; `@oslojs/encoding` stays the shared codec both crypt pages read for hex, base32, and base64url.

Composition is settled: fault rows close at the core `Fault.Class.family` boundary; instruments mint from `Convention.instrument` rows and loud arms publish typed `SecurityFact` evidence through the silent `Witness` port; `crypt/secret` supplies `Material.Source.Held` bundles and hands `Secret.changes` in as the `Rotation` feed the authority swaps its live ring on; `JwksLedger` owns the observed-instant JWKS snapshot both the jose resolver and the certified relying-party client seed from, each rendering its own `uat` unit; KDF cost claims leave as core `Board.Claim` values off the `mitata` sampler.

## [01]-[INDEX]

- [02]-[FAULT_AND_ALG]: `SignFault`, `KeyAlg`.
- [03]-[KEY_MATERIAL]: `Material`, `Material.Source`, `KeyHandle`, `Ring`.
- [04]-[CRYPTO_PRIMITIVE]: `Crypto`, `CredentialVerdict`, `Probe`.
- [05]-[SHREDDER]: `Shredder`.
- [06]-[TOKEN_AUTHORITY]: `Jwt`, `Rotation`, `AccessClaims`, `JwksSnapshot`, `JwksLedger`, `JwksTransport`, `SingleUse`.
- [07]-[CALIBRATION]: `Calibration`.

## [02]-[FAULT_AND_ALG]

[FAULT_AND_ALG]:
- Law: a `false` argon2 verify, a rejected OTP, and a rotated-out token are verdict arms, never faults — `SignFault` fires only when a primitive throws, a key refuses import, a load-shed sheds, or a token fails a trust gate.
- Growth: a new failure mode is one reason literal and one class row; a new signature scheme is one `KeyAlg` row that `Material`, `Jwt`, and the external-verify page inherit unchanged.
- Packages: `effect` (`Schema`); `@rasm/core` (`Fault.Class`).

```typescript
import { Persistence } from "@effect/experimental"
import { HttpClient, HttpClientRequest } from "@effect/platform"
import { Algorithm, hash, hashRaw, Version, verify, type Options } from "@node-rs/argon2"
import { hmac } from "@noble/hashes/hmac.js"
import { sha1 } from "@noble/hashes/legacy.js"
import { sha256, sha512 } from "@noble/hashes/sha2.js"
import { decodeBase32, decodeHex, encodeBase32UpperCaseNoPadding, encodeBase64urlNoPadding, encodeHexLowerCase } from "@oslojs/encoding"
import { Board, Convention, Fault, Identity, Wire } from "@rasm/core"
import {
  calculateJwkThumbprint, createLocalJWKSet, createRemoteJWKSet, EncryptJWT, exportJWK,
  generateKeyPair, importJWK, importPKCS8, importSPKI, importX509, jwtDecrypt, jwtVerify, SignJWT, customFetch, jwksCache,
  type ExportedJWKSCache, type JSONWebKeySet, type JWK, type JWTPayload,
} from "jose"
import { do_not_optimize, measure } from "mitata/src/lib.mjs"
import type { JWK as CachedJwk } from "openid-client"
import {
  Array, Cause, Config, Context, Data, DateTime, Duration, Effect, Encoding, Exit, HashMap, Layer, Match, Metric, Option,
  Predicate, PrimaryKey, Record, Redacted, Ref, Runtime, Schedule, Schema, Stream, Struct, pipe,
} from "effect"
import { SecurityFact, Witness } from "../access/audit.ts"

const _costs = ["login", "kek"] as const

const _family = Fault.Class.family(
  [
    "digest", "mac", "rng", "seal", "open", "wrap",
    "material", "unsupported", "window",
    "expired", "claim", "signature", "algorithm", "jwks", "malformed",
  ] as const,
  {
    digest: Fault.Class.row({
      class: "defect",
      leg: "kdf",
      detail: Schema.Struct({ row: Schema.Literal(..._costs), cause: Schema.String }),
      render: ({ cause, row }) => `argon2 ${row} row refused: ${cause}`,
    }),
    mac: Fault.Class.row({
      class: "defect",
      leg: "mac",
      detail: Schema.Struct({ op: Schema.Literal("sign", "compare"), cause: Schema.String }),
      render: ({ cause, op }) => `hmac ${op} refused: ${cause}`,
    }),
    rng: Fault.Class.row({
      class: "defect",
      leg: "entropy",
      detail: Schema.Struct({ mint: Schema.Literal("token", "uuid"), cause: Schema.String }),
      render: ({ cause, mint }) => `${mint} mint drew no entropy: ${cause}`,
    }),
    seal: Fault.Class.row({
      class: "defect",
      leg: "envelope",
      detail: Schema.Struct({ op: Schema.Literal("mint", "encrypt"), cause: Schema.String }),
      render: ({ cause, op }) => `data-key ${op} refused: ${cause}`,
    }),
    open: Fault.Class.row({
      class: "breached",
      leg: "envelope",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `sealed envelope did not open: ${cause}`,
    }),
    wrap: Fault.Class.row({
      class: "breached",
      leg: "envelope",
      detail: Schema.Struct({ op: Schema.Literal("import", "wrap", "unwrap"), cause: Schema.String }),
      render: ({ cause, op }) => `kek ${op} refused: ${cause}`,
    }),
    material: Fault.Class.row({
      class: "malformed",
      leg: "material",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `key material refused: ${cause}`,
    }),
    unsupported: Fault.Class.row({
      class: "invalid",
      leg: "capability",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `this service signs nothing that shape: ${cause}`,
    }),
    window: Fault.Class.row({
      class: "expired",
      leg: "material",
      detail: Schema.Struct({ fingerprint: Schema.String }),
      render: ({ fingerprint }) => `credential ${fingerprint} is outside its stated lease window`,
    }),
    expired: Fault.Class.row({
      class: "expired",
      leg: "token",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `token is past its expiry: ${cause}`,
    }),
    claim: Fault.Class.row({
      class: "denied",
      leg: "token",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `token claims refused: ${cause}`,
    }),
    signature: Fault.Class.row({
      class: "denied",
      leg: "token",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `token signature did not verify: ${cause}`,
    }),
    algorithm: Fault.Class.row({
      class: "denied",
      leg: "token",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `token algorithm is outside the pinned set: ${cause}`,
    }),
    jwks: Fault.Class.row({
      class: "unavailable",
      leg: "jwks",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `jwks endpoint answered nothing usable: ${cause}`,
    }),
    malformed: Fault.Class.row({
      class: "malformed",
      leg: "token",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `token is unreadable: ${cause}`,
    }),
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
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

declare namespace KeyAlg {
  type Kind = keyof typeof KeyAlg
  type Row = (typeof KeyAlg)[Kind]
  type _Keys<K extends Kind = (typeof _algs)[number]> = K
  type _Kinds<K extends (typeof _algs)[number] = Kind> = K
}

class SignFault extends Schema.TaggedError<SignFault>()("SignFault", {
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
```

## [03]-[KEY_MATERIAL]

[KEY_MATERIAL]:
- Owner: `Material` — the assembled key-material owner: `Source` the admission-source family, `admit` the one fold from any source into a `KeyHandle`, `mint` self-issues an ephemeral non-extractable ring for a KMS-less bootstrap or test composition, `ring` narrows a signing source and a published JWKS into the `{ active, verify }` set `Jwt` consumes, `jwks` projects the verify handles back to a `JSONWebKeySet` for publication, and `thumbprint` is the one RFC 7638 identity mint — the bare form, which is both the fallback `kid` a kid-less published key takes and the `cnf.jkt` confirmation value a sender-constrained token carries. This owner is every source's terminus: the handle never crosses back to a wire and never reaches a log.
- Law: PRIVATE KEY MATERIAL NEVER ARRIVES OFF THE WIRE — `Source` carries one case per trust boundary and the boundary decides what a case can hold. `Attested` carries the opaque `CredentialPublicWire` frame to its first typed consumer, where `Material.admit` calls `Wire.decode` and immediately admits the validated public material; the decoded credential never escapes as a second API. That wire declares a certificate-chain arm and a bare-SPKI arm and NO private arm, so private material is unrepresentable rather than filtered — what the collapse gives up is the self-describing armor label, and what stands in its place is the X.509 and SPKI format parse each importer runs, which refuses a PKCS#8 body by encoding instead of trusting a string the payload wrote about itself. `Held` is this folder's own host-side material — `crypt/secret` seals a Doppler-leased bundle into it and `Shredder` keys never leave the layer — and it alone reaches `importPKCS8`. `Published` is a remote JWKS entry. One entrypoint over three cases beats an `admitWire`/`admitHost` pair, because the case IS the trust boundary and a caller cannot pick the wrong one.
- Law: DER crosses and text never does — `_admissible` dispatches the generated `material` discriminant and `_armor` wraps the chosen octets at the jose call alone, because jose polices an armor prefix it then strips back to the same DER and exposes no reachable DER entrypoint. Re-armoring keeps ONE algorithm authority: jose's own JWS table already answers algorithm and usages for every `KeyAlg` row. Three alternatives reject — a direct `crypto.subtle.importKey` on the SPKI arm, which seats a second algorithm table the certificate arm never reaches; an admitted X.509 package for an extraction jose already ships; and a hand-walked certificate ASN.1.
- Law: the handle side is the KEY's own witness — `CryptoKey.type` answers `"private"` or `"public"` after the import, so `Signing` and `Verify` derive from what the platform produced rather than from a re-parsed `d` field or a caller-declared role; a symmetric `importJWK` result is `unsupported`.
- Law: the validity window rides the source that carries one — `Held` states its lease bounds and an instant outside them is `SignFault.window`; an `Attested` landing carries no local lease claim, so a replacement arrives as a newly admitted frame.
- Law: `ring` accumulates — `Effect.partition` admits every satisfying published key and quarantines each malformed entry onto the `Convention.metric.securityJwksQuarantined` counter, a warning log, and an `Admission` fact through `Witness`, so one rotated-in bad key never collapses the verify set and every quarantine lands counted, logged, and published at the fold; an empty surviving set is the only `material` failure. The synthetic verify carrier and its horizon parameter are gone with it — `Published` admits a JWKS entry directly, so no fabricated window and no `"verify"` role outside the credential vocabulary is minted to reach the same import.
- Growth: a new signature scheme is one `KeyAlg` row; a new wire material encoding is one generated oneof member landing beside one `_admissible` arm; a new armor form is one `_IMPORT` row; a new material source (KMS, HSM) is one `Source` case terminating through the same `admit`; a detached-signature or co-signed-document surface is a `GeneralSign` row over the same handles.
- Boundary: `crypt/secret` mints `Material.Source.Held` from fetched material and is the only host-side key source this folder owns; `Material.admit` is the first-sight byte boundary for the `dotnet:Rasm.AppHost/Runtime/secrets#CREDENTIAL_PEM`-produced `CredentialPublicWire` and decodes its ProtoJSON frame through the direct family before the credential reaches any interior logic; `Jwt` is the only consumer that unwraps `Signing`, and the external-verify page consumes `Verify` handles only through `jwks`.
- Packages: `jose` (`importPKCS8`/`importSPKI`/`importX509`/`importJWK`, `exportJWK`, `generateKeyPair`, `calculateJwkThumbprint`); `effect` (`Encoding`, `Match`); `@rasm/core` (`Convention`, `Wire.decode`); `access/audit` (`Witness`, `SecurityFact`).

```typescript
type KeyHandle = Data.TaggedEnum<{
  Signing: { readonly kid: string; readonly alg: KeyAlg.Kind; readonly key: Redacted.Redacted<CryptoKey> }
  Verify: { readonly kid: string; readonly alg: KeyAlg.Kind; readonly key: Redacted.Redacted<CryptoKey> }
}>

type Ring = {
  readonly active: Extract<KeyHandle, { readonly _tag: "Signing" }>
  readonly verify: ReadonlyArray<Extract<KeyHandle, { readonly _tag: "Verify" }>>
}

type _Source = Data.TaggedEnum<{
  Attested: { readonly bytes: Uint8Array }
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

const _material = (cause: unknown): SignFault => new SignFault({ case: { reason: "material", cause: String(cause) } })

const _Jwk = Schema.parseJson(Schema.Struct({ kty: Schema.String }, { key: Schema.String, value: Schema.Unknown }))
const _jwkBody = Schema.decodeUnknown(_Jwk)
const _scheme = Schema.decodeUnknown(Schema.Literal(..._algs))

const _IMPORT = {
  "CERTIFICATE": importX509,
  "PUBLIC KEY": importSPKI,
  "PRIVATE KEY": importPKCS8,
} as const satisfies Record<string, (pem: string, alg: string) => Promise<CryptoKey>>
type _Label = keyof typeof _IMPORT

const _ARMOR = /-----BEGIN ([A-Z0-9 ]+)-----/
const _label = Schema.decodeUnknownOption(Schema.Literal(...Record.keys(_IMPORT)))

const _labelOf = (armored: string): Option.Option<_Label> =>
  Option.flatMap(Option.fromNullable(_ARMOR.exec(armored)), (found) => _label(found[1]))

const _armor = (label: _Label, der: Uint8Array): string =>
  `-----BEGIN ${label}-----\n${Encoding.encodeBase64(der)}\n-----END ${label}-----`

const _handleOf = (key: CryptoKey, kid: string, alg: KeyAlg.Kind): KeyHandle =>
  key.type === "private"
    ? _KeyHandle.Signing({ kid, alg, key: Redacted.make(key) })
    : _KeyHandle.Verify({ kid, alg, key: Redacted.make(key) })

const _imported = (label: _Label, pem: string, alg: KeyAlg.Kind, kid: string): Effect.Effect<KeyHandle, SignFault> =>
  Effect.map(Effect.tryPromise({ try: () => _IMPORT[label](pem, alg), catch: _material }), (key) => _handleOf(key, kid, alg))

const _armored = (block: string, alg: KeyAlg.Kind, kid: string): Effect.Effect<KeyHandle, SignFault> =>
  Option.match(_labelOf(block), {
    onNone: () => Effect.fail(new SignFault({ case: { reason: "unsupported", cause: "armor label admits no importer" } })),
    onSome: (label) => _imported(label, block, alg, kid),
  })

const _admissible = (material: Wire.Credential["material"]): Option.Option<readonly [_Label, Uint8Array]> =>
  Match.value(material).pipe(
    Match.discriminators("case")({
      certificateChain: ({ value }) => Option.map(Array.head(value.certificates), (leaf) => ["CERTIFICATE", leaf] as const),
      spkiDer: ({ value }) => Option.some(["PUBLIC KEY", value] as const),
    }),
    Match.option,
    Option.flatten,
  )

const _fromJwk = (jwk: JWK, alg: KeyAlg.Kind, kid: string): Effect.Effect<KeyHandle, SignFault> =>
  Effect.tryPromise({ try: () => importJWK(jwk, alg), catch: _material }).pipe(
    Effect.filterOrFail(
      (held): held is CryptoKey => !(held instanceof Uint8Array),
      () => new SignFault({ case: { reason: "unsupported", cause: "jwk resolved symmetric material" } }),
    ),
    Effect.map((key) => _handleOf(key, kid, alg)),
  )

const _admit = (source: Material.Source, alg: KeyAlg.Kind): Effect.Effect<KeyHandle, SignFault> =>
  _Source.$match(source, {
    Attested: ({ bytes }) =>
      Effect.flatMap(Wire.decode("CredentialPublicWire", bytes).pipe(Effect.mapError(_material)), (credential) =>
        Option.match(_admissible(credential.material), {
          onNone: () => Effect.fail(new SignFault({ case: { reason: "material", cause: "attested credential carries no admissible der" } })),
          onSome: ([label, der]) => _imported(label, _armor(label, der), alg, credential.keyId),
        }),
      ),
    Held: ({ bundle, fingerprint, notBefore, notAfter }) =>
      Effect.flatMap(
        Effect.map(
          Effect.filterOrFail(
            DateTime.now,
            (now) => DateTime.between(now, { minimum: notBefore, maximum: notAfter }),
            () => new SignFault({ case: { reason: "window", fingerprint } }),
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
          Effect.mapError(() => new SignFault({ case: { reason: "unsupported", cause: `jwk alg ${String(jwk.alg)}` } })))
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
        () => new SignFault({ case: { reason: "material", cause: "signing source resolved public" } }),
      ))
      const [excluded, verify] = yield* Effect.partition(published.keys, (jwk) =>
        _admit(_Source.Published({ jwk }), alg).pipe(Effect.filterOrFail(
          (handle): handle is Extract<KeyHandle, { readonly _tag: "Verify" }> => handle._tag === "Verify",
          () => new SignFault({ case: { reason: "material", cause: "jwks entry resolved private" } }),
        )))
      yield* Effect.forEach(excluded, (fault) =>
        Effect.zipRight(
          Effect.zipRight(Metric.increment(_quarantined), Effect.logWarning("jwks entry quarantined", fault)),
          Witness.publish(SecurityFact.Admission({ kid: Option.none(), detail: fault.message })),
        ), { discard: true })
      return yield* Array.isNonEmptyReadonlyArray(verify)
        ? Effect.succeed<Ring>({ active, verify })
        : Effect.fail(new SignFault({ case: { reason: "material", cause: "every jwks entry quarantined" } }))
    }),
} as const
```

## [04]-[CRYPTO_PRIMITIVE]

[CRYPTO_PRIMITIVE]:
- Owner: `Crypto` — `digest`/`verify` own argon2id credential-at-rest answering the `CredentialVerdict` union, `derive` is the raw-KDF row minting deterministic key bytes from a passphrase, `sign` owns HMAC-SHA256 egress signing rendered hex, `matches` is the one constant-time comparison entrypoint discriminating on the `Probe` case — `Mac` (streaming HMAC over a signed prefix then the body), `Digest` (SHA-256 fingerprint), `Text` (raw string) — `token` mints opaque high-entropy material over the WebCrypto-filled `Entropy` port, `uuid` mints a v4 identifier from the same reader so id minting is test-injectable, `fingerprint` is the SHA-256 hex projection for high-entropy token lookup, and `plugin`/`base32` are the otplib ports over these same primitives.
- Law: every KDF call runs inside the semaphore bulkhead — credential rows take one permit, the `kek` derive takes the whole budget, so a login storm queues at the `CRYPTO_KDF_PERMITS` bound instead of spawning unbounded 19–64MB hashes; each call rides the `Convention.metric.securityKdf` distribution and its span, and the fiber's interrupt threads the `AbortSignal` so a request-scoped hash cancels with its caller.
- Law: cost is a named `CryptoCost` row selected by credential class — `login` the interactive guessable-material row, `kek` the derive row backing the `Shredder` master key — with `Argon2id`+`V0x13` pinned; the pepper is one `Config.redacted` injected at construction and threaded as `secret`. Each row also names its own production member in a `probe` field, so `[07]` grades `login` on `digest` and `kek` on `derive` rather than probing one member for both.
- Law: the KDF exists for guessable material alone: a random mint takes the `fingerprint` compare, so no machine-class cost row exists to buy defense a 200-bit secret never needs. `kek` runs live under `Shredder`; `login` reaches production through `authn/credential`'s `Digest` `low` posture, whose one guessable credential kind is the `password` `CredentialRef.kind` no ceremony page resolves yet — the row is the general password-hashing capability standing at full strength for a planned consumer, and a zero-consumer count lowers neither its cost floor nor its calibration target.
- Law: `Alphabet` is the shared entropy-alphabet owner — `base62` is the one spelling the api-key and CSRF mints compose, so an alphabet change is one edit and two byte-identical literals cannot drift; recovery's ambiguity-pruned set is the one dialect-shaped alphabet a page still holds as a caller value, earning its own spelling by pruning `I`/`L`/`O`/`0`/`1`; the session refresh and webauthn challenge mint through `base62` and the byte overload respectively and hold none. `token` discriminates on its input shape — an alphabet string mints a caller-shaped dialect, a bare byte count mints raw entropy rendered base64url-noPadding, the URL-safe wire for tokens riding paths and fragments; hex stays the fingerprint projection.
- Law: `verify` reads the PHC-embedded parameters, and a match under stale parameters returns `Matched({ stale: true })` — the rehash signal the caller persists on; `Rejected` is the ordinary auth-fail arm and only a malformed stored digest throws into `SignFault.digest`.
- Law: every compare routes constant-time through one `matches` — length is the only short-circuit, a length mismatch is `false`, a malformed stored hex is `SignFault.mac`, never an uncaught throw; a stored argon2 digest is checked by argon2's own constant-time `verify` and never re-compared through `_sameBytes`; the otplib `hmac` port dispatches the `HashAlgorithm` value off the `_HASHES` row table so a new hash is a row, never a name fork.
- Law: a port surrenders its operand contract, never its primitive — the OTP `crypto` port's compare admits `string | Uint8Array` because the strategy hands it two token strings, so `plugin.constantTimeEqual` lifts both operands to bytes before the byte-domain primitive runs; handing that primitive over bare type-checks under method-shorthand bivariance and then compares characters, which is an accept-everything gate for any alphabet outside the digits.
- Growth: a new credential class is one `CryptoCost` row; a cost bump is a row edit the rehash fold detects on the next successful verify; a new comparison shape is one `Probe` case.
- Boundary: `authn/credential` delegates every digest-at-rest here; `authn/session` consumes `token`/`uuid`/`fingerprint`/`matches`; `crypt/verify` composes `matches` under its dialect grammar; no sibling imports `@node-rs/argon2` or `@oslojs/*` directly.
- Packages: `@node-rs/argon2` (`hash`/`hashRaw`/`verify`, `Algorithm`, `Version`); `@noble/hashes` (`hmac` streaming through `hmac.create` and one-shot, `sha1`/`sha256`/`sha512`); `@oslojs/encoding` (hex + base32 rows); the constant-time byte compare, the unbiased alphabet sampler, and the `Entropy` port are folder-owned over WebCrypto's `getRandomValues`; `effect` (`Effect.makeSemaphore`, `Metric`); `@rasm/core` (`Convention`).

```typescript
type CredentialVerdict = Data.TaggedEnum<{
  Matched: { readonly stale: boolean }
  Rejected: {}
}>

type Probe = Data.TaggedEnum<{
  Mac: { readonly key: Redacted.Redacted<Uint8Array>; readonly prefix: Uint8Array; readonly body: Uint8Array; readonly signature: string }
  Digest: { readonly opaque: Redacted.Redacted<string>; readonly stored: string }
  Text: { readonly held: Redacted.Redacted<string>; readonly presented: string }
}>

const Alphabet = {
  base62: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
} as const

const CryptoCost = {
  login: {
    targetMs: 250,
    probe: "digest",
    options: { memoryCost: 19456, timeCost: 2, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
  },
  kek: {
    targetMs: 2500,
    probe: "derive",
    options: { memoryCost: 65536, timeCost: 3, parallelism: 1, outputLen: 32, algorithm: Algorithm.Argon2id, version: Version.V0x13 },
  },
} as const

declare namespace CryptoCost {
  type Kind = (typeof _costs)[number]
  type Probe = "digest" | "derive"
  type Row = { readonly targetMs: number; readonly probe: Probe; readonly options: Omit<Options, "secret" | "salt"> }
  type _Rows<T extends Record<Kind, Row> = typeof CryptoCost> = T
  type _Kinds<K extends Kind = keyof typeof CryptoCost> = K
}

const _HASHES = { sha1, sha256, sha512 } as const

type Entropy = { readonly read: (bytes: Uint8Array) => void }

const _CredentialVerdict = Data.taggedEnum<CredentialVerdict>()

const Probe = Data.taggedEnum<Probe>()

const _argonMs = Convention.mount(Convention.metric.securityKdf)

const _enc = new TextEncoder()

const _bytes = (text: string): Uint8Array => _enc.encode(text)

const _sameBytes = (left: Uint8Array, right: Uint8Array): boolean => {
  if (left.byteLength !== right.byteLength) return false
  let acc = 0
  for (let index = 0; index < left.byteLength; index += 1) acc |= left[index] ^ right[index]
  return acc === 0
}

const _chunked = (key: Uint8Array, prefix: Uint8Array, body: Uint8Array): Uint8Array =>
  hmac.create(sha256, key).update(prefix).update(body).digest()

const _sample = (reader: Entropy, alphabet: string, length: number): string => {
  const ceiling = 256 - (256 % alphabet.length)
  const out = new Array<string>(length)
  const one = new Uint8Array(1)
  let filled = 0
  while (filled < length) {
    reader.read(one)
    if (one[0] < ceiling) {
      out[filled] = alphabet[one[0] % alphabet.length]
      filled += 1
    }
  }
  return out.join("")
}

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
    const pepper = yield* Config.redacted("CREDENTIAL_PEPPER").pipe(Config.withDescription("argon2 secret pepper folded into every digest and derivation; sealed Redacted"))
    const permits = yield* Config.integer("CRYPTO_KDF_PERMITS").pipe(Config.withDefault(4), Config.withDescription("KDF bulkhead permits bounding concurrent argon2 work"))
    const gate = yield* Effect.makeSemaphore(permits)
    const secret = _bytes(Redacted.value(pepper))
    const reader: Entropy = { read: (bytes) => crypto.getRandomValues(bytes) }
    const _kdf = <A>(row: keyof typeof CryptoCost, body: Effect.Effect<A, SignFault>): Effect.Effect<A, SignFault> =>
      gate.withPermits(row === "kek" ? permits : 1)(body).pipe(
        Metric.trackDuration(_argonMs),
        Effect.withSpan("security.crypto.kdf", { attributes: { row } }),
      )
    const digest = (row: keyof typeof CryptoCost, plaintext: Redacted.Redacted<string>): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
      _kdf(row, Effect.tryPromise({
        try: (signal) => hash(Redacted.value(plaintext), { ...CryptoCost[row].options, secret }, signal),
        catch: (cause) => new SignFault({ case: { reason: "digest", row, cause: String(cause) } }),
      }).pipe(Effect.map(Redacted.make)))
    const verify_ = (row: keyof typeof CryptoCost, stored: Redacted.Redacted<string>, plaintext: Redacted.Redacted<string>): Effect.Effect<CredentialVerdict, SignFault> =>
      _kdf(row, Effect.tryPromise({
        try: (signal) => verify(Redacted.value(stored), Redacted.value(plaintext), { secret }, signal),
        catch: (cause) => new SignFault({ case: { reason: "digest", row, cause: String(cause) } }),
      }).pipe(Effect.map((matched) =>
        matched ? _CredentialVerdict.Matched({ stale: _stale(Redacted.value(stored), CryptoCost[row].options) }) : _CredentialVerdict.Rejected())))
    const derive = (row: keyof typeof CryptoCost, seed: Redacted.Redacted<string>, salt: Uint8Array): Effect.Effect<Redacted.Redacted<Uint8Array>, SignFault> =>
      _kdf(row, Effect.tryPromise({
        try: (signal) => hashRaw(Redacted.value(seed), { ...CryptoCost[row].options, secret, salt }, signal),
        catch: (cause) => new SignFault({ case: { reason: "digest", row, cause: String(cause) } }),
      }).pipe(Effect.map((buf) => Redacted.make(new Uint8Array(buf)))))
    const sign_ = (key: Redacted.Redacted<Uint8Array>, body: Uint8Array): Effect.Effect<string, SignFault> =>
      Effect.try({ try: () => encodeHexLowerCase(hmac(sha256, Redacted.value(key), body)), catch: (cause) => new SignFault({ case: { reason: "mac", op: "sign", cause: String(cause) } }) })
    const matches = (probe: Probe): Effect.Effect<boolean, SignFault> =>
      Effect.try({
        try: () =>
          Probe.$match(probe, {
            Mac: ({ key, prefix, body, signature }) => _sameBytes(_chunked(Redacted.value(key), prefix, body), decodeHex(signature)),
            Digest: ({ opaque, stored }) => _sameBytes(sha256(_bytes(Redacted.value(opaque))), decodeHex(stored)),
            Text: ({ held, presented }) => _sameBytes(_bytes(Redacted.value(held)), _bytes(presented)),
          }),
        catch: (cause) => new SignFault({ case: { reason: "mac", op: "compare", cause: String(cause) } }),
      })
    function token(alphabet: string, length: number): Effect.Effect<Redacted.Redacted<string>, SignFault>
    function token(bytes: number): Effect.Effect<Redacted.Redacted<string>, SignFault>
    function token(form: string | number, length = 0): Effect.Effect<Redacted.Redacted<string>, SignFault> {
      return Effect.try({
        try: () => {
          if (typeof form === "number") {
            const bytes = new Uint8Array(form)
            reader.read(bytes)
            return Redacted.make(encodeBase64urlNoPadding(bytes))
          }
          return Redacted.make(_sample(reader, form, length))
        },
        catch: (cause) => new SignFault({ case: { reason: "rng", mint: "token", cause: String(cause) } }),
      })
    }
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
        catch: (cause) => new SignFault({ case: { reason: "rng", mint: "uuid", cause: String(cause) } }),
      })
    const fingerprint = (opaque: Redacted.Redacted<string>): string =>
      encodeHexLowerCase(sha256(_bytes(Redacted.value(opaque))))
    const plugin = {
      name: "rasm-sign",
      hmac: (alg: keyof typeof _HASHES, key: Uint8Array, data: Uint8Array) => hmac(_HASHES[alg], key, data),
      randomBytes: (len: number) => { const bytes = new Uint8Array(len); reader.read(bytes); return bytes },
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
- Packages: WebCrypto `SubtleCrypto` (`generateKey`/`encrypt`/`decrypt`/`wrapKey`/`unwrapKey`/`importKey`); `Crypto` (`derive`, `plugin.randomBytes`); `@rasm/core` (`Convention`); `access/audit` (`Witness`, `SecurityFact`).

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
    const passphrase = yield* Config.redacted("SHRED_MASTER_KEY").pipe(Config.withDescription("master KEK passphrase the argon2id kek row derives from; sealed Redacted"))
    const salt = yield* Config.string("SHRED_MASTER_SALT").pipe(Config.withDescription("pinned KEK derivation salt; a change re-keys every wrapped data key"))
    const raw = yield* cipher.derive("kek", passphrase, _bytes(salt))
    const kek = yield* Effect.tryPromise({
      try: () => crypto.subtle.importKey("raw", Redacted.value(raw), { name: "AES-KW" }, false, ["wrapKey", "unwrapKey"]),
      catch: (cause) => new SignFault({ case: { reason: "wrap", op: "import", cause: String(cause) } }),
    })
    const mint = (): Effect.Effect<Redacted.Redacted<CryptoKey>, SignFault> =>
      Effect.tryPromise({
        try: () => crypto.subtle.generateKey({ name: "AES-GCM", length: 256 }, true, ["encrypt", "decrypt"]),
        catch: (cause) => new SignFault({ case: { reason: "seal", op: "mint", cause: String(cause) } }),
      }).pipe(Effect.map(Redacted.make))
    const wrap = (dataKey: Redacted.Redacted<CryptoKey>): Effect.Effect<WrappedKey, SignFault> =>
      Effect.tryPromise({
        try: () => crypto.subtle.wrapKey("raw", Redacted.value(dataKey), kek, "AES-KW"),
        catch: (cause) => new SignFault({ case: { reason: "wrap", op: "wrap", cause: String(cause) } }),
      }).pipe(Effect.map((buf) => new WrappedKey({ wrapped: new Uint8Array(buf) })))
    const unwrap = (key: WrappedKey): Effect.Effect<Redacted.Redacted<CryptoKey>, SignFault> =>
      Effect.tryPromise({
        try: () => crypto.subtle.unwrapKey("raw", key.wrapped, kek, "AES-KW", { name: "AES-GCM" }, false, ["encrypt", "decrypt"]),
        catch: (cause) => new SignFault({ case: { reason: "wrap", op: "unwrap", cause: String(cause) } }),
      }).pipe(Effect.map(Redacted.make))
    const seal = (dataKey: Redacted.Redacted<CryptoKey>, plaintext: Uint8Array): Effect.Effect<SealedEnvelope, SignFault> =>
      Effect.gen(function* () {
        const iv = cipher.plugin.randomBytes(12)
        const ciphertext = yield* Effect.tryPromise({
          try: () => crypto.subtle.encrypt({ name: "AES-GCM", iv }, Redacted.value(dataKey), plaintext),
          catch: (cause) => new SignFault({ case: { reason: "seal", op: "encrypt", cause: String(cause) } }),
        })
        return new SealedEnvelope({ iv, ciphertext: new Uint8Array(ciphertext) })
      })
    const open = (dataKey: Redacted.Redacted<CryptoKey>, envelope: SealedEnvelope): Effect.Effect<Uint8Array, SignFault> =>
      Effect.tryPromise({
        try: () => crypto.subtle.decrypt({ name: "AES-GCM", iv: envelope.iv }, Redacted.value(dataKey), envelope.ciphertext),
        catch: (cause) => new SignFault({ case: { reason: "open", cause: String(cause) } }),
      }).pipe(
        Effect.tapError((fault) => Effect.zipRight(Metric.increment(_openReject), Witness.publish(SecurityFact.ShredOpen({ detail: fault.message })))),
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
- Owner: `Jwt` — a scoped Layer factory over a pinned `Keyset` or a `Rotation` pair: `mint` stamps `{ alg, kid }` from the active ring key so verifiers route by `kid`; one overloaded `verify` owns both trust roots — `verify(token)` runs `createLocalJWKSet` over every published verify handle with `algorithms` pinned and the declarative claim gates applied, decoding the payload through `AccessClaims`, and `verify(token, issuer)` resolves the per-issuer remote JWKS and returns the verified raw payload for the OAuth page to project from; `seal`/`unseal` are the JWE confidential profile over the keyset's optional symmetric handle. `SingleUse` is the stash contract every two-leg ceremony port in the folder instantiates — stash with a TTL, consume exactly once — and `SingleUse.persisted` is the shipped satisfier every one of them composes.
- Law: `algorithms` is always pinned — an unpinned `alg` is accepted-algorithm confusion; the claim gates (`issuer`, `audience`, `clockTolerance`, and required `iat`/`exp`/`iss`/`aud`/`sub`) are one jose verification policy, never hand timestamp or presence checks; `decodeJwt` is never verification; `cnf.jkt` carries the BARE `Material.thumbprint` binding for a sender-constrained token, and a verifier holding `cnf` recomputes the bare RFC 7638 thumbprint of the presented proof key and matches that — never a `urn:ietf:params:oauth:jwk-thumbprint` URI, which is a subject spelling no confirmation value matches.
- Law: rotation swaps the ring INSIDE the live authority — the layer takes either a pinned `Keyset` or a `Rotation` pair carrying the rebuild and the roll feed, and the rotating arm forks a drain that rebuilds the ring on each observed roll, recompiles the local verify set, and swaps one `Ref`. `Reloadable.auto` cannot hold this boundary and the folder ships the drain instead: that combinator re-runs a layer VALUE on a `Schedule`, so `Jwt.Default(keyset)` closed over one captured ring rebuilds the RETIRED ring on every reload, a rotation feed reaches it nowhere, and it republishes under the `Reloadable<Jwt>` tag no consumer of this page requires. Swapping in place keeps the `Jwt` tag intact, so a Doppler roll costs no dependent-layer teardown, a `kid` retires with zero edits here, and a retired signing key keeps verifying while its handle stays published.
- Law: the folder ships one `SingleUse` satisfier and it is `Persistence`-backed — `SingleUse.persisted` binds a consumer's own port Tag to a `ResultPersistence` store, `stash` sets one settled `Exit` under a per-call TTL carried on the key, and `consume` reads then removes, so a second consume answers `None` inside the window. `PersistedCache` is refused here: it computes its value from a `lookup` a stash has none of, so satisfying a two-leg ceremony through it means inventing a lookup that fabricates the challenge, state, or principal the first leg was supposed to seal. Backing store binds at the app root — `Persistence.layerResultKeyValueStore` over the data wave's `KeyValueStore`, `Persistence.layerResultMemory` for a single-process composition.
- Law: `JwksSnapshot` is the ledger's own shape and the folder's single JWKS custody — it carries the key set beside the instant this owner observed it, never a package's `uat` scalar, because jose stamps that field in epoch MILLISECONDS off `Date.now()` while the certified relying-party client stamps it in epoch SECONDS: one stored number read under the wrong unit either reads as 1970 and refetches on every call or reads as the far future and never refetches a rotated key. The unit is therefore a per-consumer projection off one owned instant — `JwksSnapshot.jose` renders the millisecond form this page seeds — and every other consumer of the same ledger projects its own.
- Law: the remote resolver is built once per issuer under `Effect.cachedFunction` — the ledger snapshot seeds jose's cache through that projection, and a scoped fiber drives `resolver.reload()` on a jittered `Schedule.spaced(cacheAge)` so a provider key roll lands before the first `kid` miss; the tick asks only where the ask can land, gating on the resolver's own published `fresh`/`coolingDown`/`reloading` state so it stops issuing reloads jose refuses inside `cooldownDuration` and stops refetching an already-fresh set every `cacheAge` span. Each landed reload and each successful verify persists through `JwksLedger` from `resolver.jwks()` — the resolver's own accessor, so no mutable record survives the closure — and a tick whose reload genuinely failed logs at warning while an interrupted teardown stays silent; a cold build increments the `Convention.metric.securityJwksMiss` counter.
- Law: the JWKS hop rides the platform client, never a bare `globalThis.fetch` — `JwksTransport` is a Tag whose shipped `Live` Layer renders an `HttpClient.retryTransient({ schedule })` client back into the `customFetch` shape jose asks for, so a provider's transient 502 or reset socket redrives under the branch retry budget and the hop inherits the client's span rather than leaving a hole in the trace across the one call a key roll depends on. jose's own `AbortSignal` threads into `Runtime.runPromise`, so `timeoutDuration` still interrupts the fiber; the app root overrides the Layer to pin a proxy, a mutual-TLS agent, or a stub.
- Law: the JWE profile is confidentiality, not a second token system — `seal` encrypts the same `AccessClaims` under `{ alg: "dir", enc: "A256GCM" }` and `unseal` reverses it with the same claim gates; a keyset without a seal handle refuses the profile as `unsupported`.
- Output: `mint`/`seal` return the token `Redacted`; `verify`/`unseal` return `AccessClaims`, never a bare `JWTPayload`; the issuer overload returns the verified payload.
- Growth: a new claim is one `AccessClaims` field; a new JOSE failure code is one `_codeReason` arm; a new external issuer costs nothing — the resolver memoizes per `jwksUri`; a new two-leg ceremony is one port Tag and one `SingleUse.persisted` row.
- Boundary: the composition root builds the `Keyset` from `crypt/secret`'s `Material.Source.Held` values through `Material.ring` and hands that rebuild in beside `Secret.changes` as the `Rotation` pair — this page never imports `crypt/secret`, which imports it. Both the `KeyValueStore` behind `SingleUse.persisted` and the `HttpClient` behind `JwksTransport.Live` are root-bound.
- Packages: `jose` (`SignJWT`/`jwtVerify`, `EncryptJWT`/`jwtDecrypt`, `createLocalJWKSet`/`createRemoteJWKSet`, `jwksCache`/`customFetch`); `@effect/experimental` (`Persistence.ResultPersistence`); `@effect/platform` (`HttpClient.retryTransient`, `HttpClientRequest`); `effect` (`Exit`, `PrimaryKey`, `Ref`, `Runtime`, `Stream`); `@rasm/core` (`Convention`, `Fault.Budget`).

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

type Rotation = {
  readonly rebuild: Effect.Effect<Keyset, SignFault>
  readonly rotations: Stream.Stream<unknown>
}

type _Compiled = {
  readonly keyset: Keyset
  readonly local: ReturnType<typeof createLocalJWKSet>
  readonly algorithms: ReadonlyArray<KeyAlg.Kind>
}

const _compiled = (keyset: Keyset): Effect.Effect<_Compiled, SignFault> =>
  Effect.map(Material.jwks(keyset.ring.verify), (published) => ({
    keyset,
    local: createLocalJWKSet(published),
    algorithms: Array.map(keyset.ring.verify, (handle) => handle.alg),
  }))

type SingleUse<A, E> = {
  readonly stash: (key: string, value: A, ttl: Duration.DurationInput) => Effect.Effect<void, E>
  readonly consume: (key: string) => Effect.Effect<Option.Option<A>, E>
}

const _stashRow = <A, IA>(storeId: string, value: Schema.Schema<A, IA>) =>
  class Stash extends Schema.TaggedRequest<Stash>()(storeId, {
    payload: { key: Schema.String, ttl: Schema.DurationFromMillis },
    success: value,
    failure: Schema.Never,
  }) {
    [PrimaryKey.symbol](): string {
      return this.key
    }
  }

const SingleUse = {
  persisted: <I, A, IA, E>(options: {
    readonly tag: Context.Tag<I, SingleUse<A, E>>
    readonly storeId: string
    readonly value: Schema.Schema<A, IA>
    readonly failed: (cause: Persistence.PersistenceError) => E
  }): Layer.Layer<I, never, Persistence.ResultPersistence> =>
    Layer.scoped(
      options.tag,
      Effect.gen(function* () {
        const persistence = yield* Persistence.ResultPersistence
        const Row = _stashRow(options.storeId, options.value)
        const store = yield* persistence.make({ storeId: options.storeId, timeToLive: (request) => (request as InstanceType<typeof Row>).ttl })
        const row = (key: string, ttl: Duration.DurationInput) => new Row({ key, ttl: Duration.decode(ttl) })
        return {
          stash: (key, held, ttl) => store.set(row(key, ttl), Exit.succeed(held)).pipe(Effect.mapError(options.failed)),
          consume: (key) =>
            Effect.flatMap(store.get(row(key, Duration.zero)), Option.match({
              onNone: () => Effect.succeedNone,
              onSome: (settled) =>
                Effect.as(store.remove(row(key, Duration.zero)), Exit.match(settled, { onFailure: () => Option.none<A>(), onSuccess: Option.some })),
            })).pipe(Effect.mapError(options.failed)),
        }
      }),
    ),
} as const

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

class JwksTransport extends Context.Tag("security/crypt/JwksTransport")<JwksTransport, typeof globalThis.fetch>() {
  static readonly Live: Layer.Layer<JwksTransport, never, HttpClient.HttpClient> = Layer.effect(
    JwksTransport,
    Effect.gen(function* () {
      const client = HttpClient.retryTransient({ schedule: Fault.Budget.schedule("pulse", () => true) })(yield* HttpClient.HttpClient)
      const runtime = yield* Effect.runtime<never>()
      return (input, init) =>
        Runtime.runPromise(runtime)(
          HttpClientRequest.make(init?.method ?? "GET")(input instanceof Request ? input.url : String(input)).pipe(
            HttpClientRequest.setHeaders(new Headers(init?.headers)),
            client.execute,
            Effect.flatMap((response) =>
              Effect.map(response.text, (body) => new Response(body, { status: response.status, headers: { ...response.headers } }))),
            Effect.scoped,
          ),
          { signal: init?.signal ?? undefined },
        )
    }),
  )
}

const _policy = Config.unwrap({
  tolerance: Config.integer("JWT_CLOCK_TOLERANCE").pipe(Config.withDefault(5), Config.withDescription("jose clockTolerance seconds applied on every claim gate")),
  cacheAge: Config.duration("JWKS_CACHE_AGE").pipe(Config.withDefault(Duration.minutes(10)), Config.withDescription("remote JWKS cacheMaxAge and the proactive reload cadence")),
  cooldown: Config.duration("JWKS_COOLDOWN").pipe(Config.withDefault(Duration.seconds(30)), Config.withDescription("jose cooldownDuration between forced JWKS reloads")),
  deadline: Config.duration("JWKS_DEADLINE").pipe(Config.withDefault(Duration.seconds(5)), Config.withDescription("per-fetch JWKS timeoutDuration and the verify leg's outer deadline")),
})

class Jwt extends Effect.Service<Jwt>()("security/crypt/Jwt", {
  scoped: (source: Keyset | Rotation) =>
    Effect.gen(function* () {
      const ledger = yield* JwksLedger
      const transport = yield* JwksTransport
      const { cacheAge, cooldown, deadline, tolerance } = yield* _policy
      const rebuild = Predicate.hasProperty(source, "rebuild") ? source.rebuild : Effect.succeed(source)
      const cell = yield* Ref.make(yield* Effect.flatMap(rebuild, _compiled))
      yield* Effect.forEach(
        Predicate.hasProperty(source, "rotations") ? [source.rotations] : [],
        (rotations) =>
          Effect.forkScoped(Stream.runDrain(Stream.mapEffect(rotations, () =>
            Effect.flatMap(rebuild, _compiled).pipe(
              Effect.flatMap((next) =>
                Effect.zipRight(Ref.set(cell, next), Witness.publish(SecurityFact.Admission({ kid: Option.some(next.keyset.ring.active.kid), detail: "ring rotated" })))),
              Effect.tapError((fault) => Effect.logWarning("jwt ring rebuild failed; live ring retained", fault)),
              Effect.ignore,
            )))),
        { discard: true },
      )
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
          const persist = Effect.flatMap(DateTime.now, (observedAt) =>
            Option.match(Option.fromNullable(resolver.jwks()), {
              onNone: () => Effect.void,
              onSome: (set) =>
                Schema.decodeUnknown(JwksSnapshot)({ keys: set.keys, observedAt: DateTime.formatIso(observedAt) }).pipe(
                  Effect.mapError((cause) => new SignFault({ case: { reason: "jwks", cause: String(cause) } })),
                  Effect.flatMap((snapshot) => ledger.save(jwksUri, snapshot)),
                ),
            }))
          yield* Effect.forkScoped(Effect.repeat(
            Effect.unless(
              Effect.zipRight(
                Effect.tryPromise({ try: () => resolver.reload(), catch: (cause) => new SignFault({ case: { reason: "jwks", cause: String(cause) } }) }),
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
          Effect.mapError((cause) => new SignFault({ case: { reason: "claim", cause: String(cause) } })))
      const _claims = (claims: AccessClaims) => ({
        sid: claims.sid, scope: claims.scope,
        ...(Option.isSome(claims.tid) && { tid: claims.tid.value }),
        ...(Option.isSome(claims.cnf) && { cnf: { jkt: claims.cnf.value.jkt } }),
      })
      const _seconds = (ttl: Duration.DurationInput): string =>
        `${Math.max(1, Math.round(Duration.toSeconds(Duration.decode(ttl))))}s`
      const _local = (token: Redacted.Redacted<string>): Effect.Effect<AccessClaims, SignFault> =>
        Effect.flatMap(Ref.get(cell), ({ algorithms, keyset, local }) =>
          Effect.tryPromise({
            try: () => jwtVerify(Redacted.value(token), local, {
              algorithms, issuer: keyset.issuer, audience: keyset.audience,
              clockTolerance: tolerance, requiredClaims: [..._requiredClaims],
            }),
            catch: (cause) => new SignFault({ case: { reason: _reasonOf(cause), cause: String(cause) } }),
          })).pipe(
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
            catch: (cause) => new SignFault({ case: { reason: _reasonOf(cause), cause: String(cause) } }),
          }).pipe(
            Effect.timeoutFail({ duration: deadline, onTimeout: () => new SignFault({ case: { reason: "jwks", cause: `${issuer.jwksUri} did not answer inside the deadline` } }) }),
            Effect.retry(Fault.Budget.schedule("pulse")),
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
        Effect.flatMap(Ref.get(cell), ({ keyset }) =>
          Effect.tryPromise({
            try: () =>
              new SignJWT(_claims(claims))
                .setProtectedHeader({ alg: keyset.ring.active.alg, kid: keyset.ring.active.kid })
                .setIssuedAt().setIssuer(keyset.issuer).setAudience(keyset.audience).setSubject(claims.sub)
                .setExpirationTime(_seconds(ttl))
                .sign(Redacted.value(keyset.ring.active.key)),
            catch: (cause) => new SignFault({ case: { reason: _reasonOf(cause), cause: String(cause) } }),
          })).pipe(Effect.map(Redacted.make), Effect.withSpan("security.jwt.mint"))
      const _sealed = Effect.flatMap(Ref.get(cell), ({ keyset }) =>
        Effect.map(
          Effect.mapError(keyset.seal, () => new SignFault({ case: { reason: "unsupported", cause: "keyset carries no seal handle" } })),
          (key) => ({ key, keyset }) as const,
        ))
      const seal = (claims: AccessClaims, ttl: Duration.DurationInput): Effect.Effect<Redacted.Redacted<string>, SignFault> =>
        Effect.flatMap(_sealed, ({ key, keyset }) =>
          Effect.tryPromise({
            try: () =>
              new EncryptJWT(_claims(claims))
                .setProtectedHeader({ alg: "dir", enc: "A256GCM" })
                .setIssuedAt().setIssuer(keyset.issuer).setAudience(keyset.audience).setSubject(claims.sub)
                .setExpirationTime(_seconds(ttl))
                .encrypt(Redacted.value(key)),
            catch: (cause) => new SignFault({ case: { reason: _reasonOf(cause), cause: String(cause) } }),
          })).pipe(Effect.map(Redacted.make))
      const unseal = (token: Redacted.Redacted<string>): Effect.Effect<AccessClaims, SignFault> =>
        Effect.flatMap(_sealed, ({ key, keyset }) =>
          Effect.tryPromise({
            try: () => jwtDecrypt(Redacted.value(token), Redacted.value(key), {
              issuer: keyset.issuer, audience: keyset.audience,
              clockTolerance: tolerance, requiredClaims: [..._requiredClaims],
            }),
            catch: (cause) => new SignFault({ case: { reason: _reasonOf(cause), cause: String(cause) } }),
          })).pipe(Effect.flatMap((result) => _decoded(result.payload)))
      return { mint, verify, seal, unseal } as const
    }),
  accessors: true,
}) {}
```

## [07]-[CALIBRATION]

[CALIBRATION]:
- Law: sampling belongs to `mitata` and only the verdict is folder-owned — `measure` drives warmup, batching, and the sample ladder, and `Board.Bench.fromMitata` folds the returned `stats` into the claim band, so this page mints no quantile of its own and a KDF claim compares rung-for-rung against every other benchmark claim in the repo. Hand-rolling a percentile fold beside the engine's mints a second, non-comparable claim-band producer, and two producers over one claim family make a cross-surface comparison silently meaningless.
- Law: each target is a `CryptoCost` field and `_argonMs` takes its target boundaries from those rows, so the KDF histogram and calibration verdict read the same value; a target cannot drift beside its cost owner.
- Law: `trials` admits only positive integers and rides in as the sampler's own `min_samples`/`max_samples` bound; every probe value pins through `do_not_optimize`, so a dead-code eliminator cannot delete the hash the row exists to grade and report a target met on work never done.
- Law: the engine reports NANOSECONDS while `CryptoCost.targetMs` is the operator-facing target, so the band keeps the engine's unit and the conversion happens once at the comparison — a claim that silently respelled its own unit compares against a stored baseline in the other.
- Law: trials run the production shape — each row probes the member its own `probe` field names, so `login` grades `digest` and `kek` grades `derive` under the whole-budget permit exactly as `Shredder` boot takes it; every trial passes the semaphore bulkhead, the `Convention.metric.securityKdf` distribution, and the KDF span, because a bench that bypasses the bulkhead measures a machine that does not exist and a bench that probes the wrong member grades a call no consumer makes.
- Law: throughput claims ride the same family — a `Jwt` mint or verify-fold claim is one `bench` call over that probe with its own suite key; a second claim shape per crypto surface is the forked-family defect.
- Growth: a new bench subject is one `bench` call; a new credential class inherits its target AND its production member through the `CryptoCost` guard; a new statistic is a core-owned claim-band field this page reads rather than computes.
- Boundary: `HostFingerprint` construction (print, machine, arch, cores, runtime) is the composing runtime's boot fact, passed in — this page never probes the host; claim persistence and cross-host trend boards are the core bench-pack and corpus-gate consumers over the encoded family. mitata's registration and render surface stays in the bench lane under `tests/` — domain code reaches the state-free kernel alone.
- Packages: `mitata` (`measure`, `do_not_optimize`, the `stats` ladder derived off `measure`); `@rasm/core` (`Board.Bench.fromMitata`/`.measured`, `Board.Claim`, `Identity.App`); `effect` (`Runtime.runPromise`, `Exit`).

```typescript
declare namespace Calibration {
  type Row = keyof typeof CryptoCost
  type Stats = Awaited<ReturnType<typeof measure>>
  type Verdict = { readonly admitted: boolean; readonly claim: Board.Claim; readonly row: Row; readonly target: number }
}

const _NS_PER_MS = 1_000_000

const _sampled = <A, R>(probe: Effect.Effect<A, SignFault, R>, trials: number): Effect.Effect<Calibration.Stats, SignFault, R> =>
  globalThis.Number.isInteger(trials) && trials > 0
    ? Effect.gen(function* () {
        const runtime = yield* Effect.runtime<R>()
        yield* probe
        return yield* Effect.tryPromise({
          try: () =>
            measure(async () => { do_not_optimize(await Runtime.runPromise(runtime)(Effect.exit(probe))) }, { min_samples: trials, max_samples: trials }),
          catch: (cause) => new SignFault({ case: { reason: "unsupported", cause: `calibration sampler: ${String(cause)}` } }),
        })
      })
    : Effect.fail(new SignFault({ case: { reason: "unsupported", cause: `calibration trials ${trials} is not a positive count` } }))

const _claimed = (suite: string, host: Board.Claim["host"], stats: Calibration.Stats): Effect.Effect<Board.Claim> =>
  Effect.map(DateTime.now, (minted) =>
    Board.Bench.fromMitata(stats, {
      suite, label: "wall", unit: "ns", polarity: "minimize", host, subject: { subject: "probe" }, minted,
      warmups: Option.none(), allocatedBytes: Option.none(), operations: Option.none(),
    }))

const _admitted = (identity: Identity.App, claim: Board.Claim): Effect.Effect<Board.Claim, SignFault> =>
  Board.Claim.matches(claim, identity)
    ? Effect.succeed(claim)
    : Effect.fail(new SignFault({ case: { reason: "unsupported", cause: `claim host ${claim.host.print} is not ${identity.host}` } }))

const _PROBES = {
  digest: (cipher: Crypto, row: Calibration.Row, secret: Redacted.Redacted<string>) => cipher.digest(row, secret),
  derive: (cipher: Crypto, row: Calibration.Row, secret: Redacted.Redacted<string>) => cipher.derive(row, secret, cipher.plugin.randomBytes(16)),
} as const satisfies Record<CryptoCost.Probe, (cipher: Crypto, row: Calibration.Row, secret: Redacted.Redacted<string>) => Effect.Effect<unknown, SignFault>>

const Calibration = {
  bench: <R>(suite: string, identity: Identity.App, host: Board.Claim["host"], probe: Effect.Effect<unknown, SignFault, R>, trials: number): Effect.Effect<Board.Claim, SignFault, R> =>
    Effect.flatMap(_sampled(probe, trials), (stats) =>
      Effect.flatMap(_claimed(suite, host, stats), (claim) => _admitted(identity, claim))),
  calibrate: (
    identity: Identity.App,
    host: Board.Claim["host"],
    probe: Redacted.Redacted<string>,
    trials: number,
  ): Effect.Effect<ReadonlyArray<Calibration.Verdict>, SignFault, Crypto> =>
    Effect.flatMap(Crypto, (cipher) =>
      Effect.forEach(Struct.keys(CryptoCost), (row) =>
        Effect.gen(function* () {
          const stats = yield* _sampled(_PROBES[CryptoCost[row].probe](cipher, row, probe), trials)
          const claim = yield* Effect.flatMap(_claimed(`security-kdf-${row}`, host, stats), (minted) => _admitted(identity, minted))
          const p99 = yield* Option.match(Board.Bench.measured(Array.headNonEmpty(claim.metrics), "p99"), {
            onNone: () => Effect.fail(new SignFault({ case: { reason: "unsupported", cause: `${row} row measured no p99` } })),
            onSome: Effect.succeed,
          })
          return { admitted: p99 / _NS_PER_MS <= CryptoCost[row].targetMs, claim, row, target: CryptoCost[row].targetMs }
        }))),
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { AccessClaims, Alphabet, Calibration, Crypto, CryptoCost, Jwt, JwksLedger, JwksSnapshot, JwksTransport, KeyAlg, Material, Probe, SealedEnvelope, Shredder, SignFault, SingleUse, WrappedKey }
export type { CredentialVerdict, IssuerRef, KeyHandle, Keyset, Ring, Rotation }
```

## [08]-[RESEARCH]

(none)

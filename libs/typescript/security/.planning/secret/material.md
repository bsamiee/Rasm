# [SECURITY_MATERIAL] — key-material vocabulary; the CredentialPemWire redacted carrier terminates here

`secret/material` owns the key-material vocabulary and the terminus of the `CredentialPemWire` seam: the C#-minted PEM/JWK carrier decodes in `wire/codec/credential` and terminates here as a `Redacted` `CryptoKey` handle that never crosses back to a wire and never reaches a log. Algorithm is a value from one `KeyAlg` vocabulary table — a new signature scheme is one row — and admission is one polymorphic `Material.admit` dispatching the JOSE import over the wire's `format`+`secret` discriminants into a `KeyHandle` tagged family (`Signing` holds the private key `sign/jwt` mints with, `Verify` holds the public key rotation publishes). The `kid` is the RFC 7638 JWK thumbprint computed once at admission so rotation keys carry stable identity, and every imported key is non-extractable and `Redacted`. This page consumes `sign/crypto`'s fault shape — `MaterialFault` is the same `Schema.TaggedError` reason-family with `get policy()` over an `as const` `{ rank, retry, status }` table — and hands `SigningKey`/`VerifyKey` records to `sign/jwt`, the only downstream that unwraps the handle.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                          | [OWNER]                                  | [PACKAGES]                    | [REJECTED_FORM]                       |
| :-----: | :--------------------------------- | :--------------------------------------- | :---------------------------- | :------------------------------------ |
|  [01]   | signature-scheme vocabulary        | `KeyAlg` `as const` row table            | derived — `keyof typeof`      | a hand union, an `alg` string per site |
|  [02]   | PEM/JWK wire → redacted key handle | `Material.admit` `KeyHandle` dispatch    | `jose` key import             | interface mirror, `as`-cast key shape |
|  [03]   | rotation identity + JWKS publish   | `Material.thumbprint` / `.jwks`          | `jose` thumbprint/export      | a stored kid drifting from the key    |
|  [04]   | signing-key + verify-set assembly  | `Material.ring` narrowed handle set      | `jose` import                 | jwt narrowing the union by hand       |

## [2]-[MATERIAL_VOCABULARY]

[KEY_ALG]:
- Owner: `KeyAlg` — the bounded signature-scheme vocabulary; each row carries `{ kty, crv?, use }`, the discriminant derives through `keyof typeof`, and `CredentialPemWire.alg` and `SigningKey.alg` both speak `KeyAlg.Kind`. A new scheme (an ECDSA curve, a post-quantum suite when JOSE admits it) is one row.
- Boundary: the wire twin `CredentialPemWire` is a `Schema.Class` — its encoded side is the C# `Rasm.AppHost` shape decoded in `wire/codec/credential`; this page owns the decoded interior only, and `pem`/`jwk` fields are `Schema.Redacted` from the decode.
- Growth: a fetched key from `secret/doppler` arrives as the same `CredentialPemWire` and terminates through the same `admit`; there is no second import path.

```typescript
import { calculateJwkThumbprint, exportJWK, importJWK, importPKCS8, importSPKI, importX509, type JSONWebKeySet, type JWK } from "jose"
import { Array, Data, Effect, Option, Redacted, Schema } from "effect"

// --- [CONSTANTS] ------------------------------------------------------------------------

const _algs = ["ES256", "ES384", "RS256", "EdDSA"] as const

const KeyAlg = {
  ES256: { kty: "EC", crv: "P-256", use: "sig" },
  ES384: { kty: "EC", crv: "P-384", use: "sig" },
  RS256: { kty: "RSA", use: "sig" },
  EdDSA: { kty: "OKP", crv: "Ed25519", use: "sig" },
} as const

const _reasons = ["decode", "unsupported", "import"] as const

const MaterialFaultPolicy = {
  decode: { rank: 3, retry: false, status: 400 },
  unsupported: { rank: 4, retry: false, status: 500 },
  import: { rank: 4, retry: false, status: 500 },
} as const

declare namespace KeyAlg {
  type Kind = keyof typeof KeyAlg
  type Row = (typeof KeyAlg)[Kind]
}

declare namespace MaterialFault {
  type Reason = keyof typeof MaterialFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof MaterialFaultPolicy> = T
}

// --- [TYPES] ----------------------------------------------------------------------------

type KeyHandle = Data.TaggedEnum<{
  Signing: { readonly kid: string; readonly alg: KeyAlg.Kind; readonly key: Redacted.Redacted<CryptoKey> }
  Verify: { readonly kid: string; readonly alg: KeyAlg.Kind; readonly key: Redacted.Redacted<CryptoKey> }
}>

// --- [MODELS] ---------------------------------------------------------------------------

class CredentialPemWire extends Schema.Class<CredentialPemWire>("CredentialPemWire")({
  material: Schema.Redacted(Schema.String),
  format: Schema.Literal("pkcs8", "spki", "x509", "jwk"),
  alg: Schema.Literal(..._algs),
  secret: Schema.Boolean,
  kid: Schema.optionalWith(Schema.String, { as: "Option" }),
}) {}

// --- [ERRORS] ---------------------------------------------------------------------------

class MaterialFault extends Schema.TaggedError<MaterialFault>()("MaterialFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): MaterialFault.Row {
    return MaterialFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<material:${this.reason}> ${this.detail}`
  }
}
```

## [3]-[KEY_ADMISSION]

[ADMIT]:
- Owner: `Material.admit` — one polymorphic import folding a `CredentialPemWire` into a `KeyHandle`. The `format` selects the JOSE importer (`importPKCS8` private, `importSPKI`/`importX509` public, `importJWK` either), and `secret` selects the `Signing`/`Verify` arm for the ambiguous `jwk` case; there is no `admitSigning`/`admitVerify` twin.
- Packages: `jose` — `importPKCS8`/`importSPKI`/`importX509`/`importJWK` yield a non-extractable `CryptoKey`; `exportJWK`+`calculateJwkThumbprint` compute the RFC 7638 `kid` when the wire omits one.
- Boundary: `sign/jwt` is the only consumer that unwraps the `Redacted<CryptoKey>` — into `SignJWT.sign`/`jwtVerify`; `secret/doppler` produces the `CredentialPemWire` this admits. The import throw folds to `MaterialFault.import`; an alg outside `KeyAlg` is `MaterialFault.unsupported`.
- Receipt: `KeyHandle` — `Signing` carries the private handle, `Verify` the public; both carry the stable `kid` rotation keys are addressed by. `Material.ring` narrows a signing wire plus a public JWKS into the `{ active, verify }` set `sign/jwt` consumes, so no downstream re-narrows the union by hand.

```typescript
// --- [TYPES] ----------------------------------------------------------------------------

type Ring = { readonly active: Extract<KeyHandle, { readonly _tag: "Signing" }>; readonly verify: ReadonlyArray<Extract<KeyHandle, { readonly _tag: "Verify" }>> }

// --- [OPERATIONS] -----------------------------------------------------------------------

const _KeyHandle = Data.taggedEnum<KeyHandle>()

const _import = (wire: CredentialPemWire): Effect.Effect<CryptoKey, MaterialFault> =>
  Effect.tryPromise({
    try: () => {
      const pem = Redacted.value(wire.material)
      return wire.format === "pkcs8"
        ? importPKCS8(pem, wire.alg)
        : wire.format === "spki"
          ? importSPKI(pem, wire.alg)
          : wire.format === "x509"
            ? importX509(pem, wire.alg)
            : importJWK(JSON.parse(pem) as JWK, wire.alg) as Promise<CryptoKey>
    },
    catch: (cause) => new MaterialFault({ reason: "import", detail: String(cause) }),
  })

const _kid = (wire: CredentialPemWire, key: CryptoKey): Effect.Effect<string, MaterialFault> =>
  Option.match(wire.kid, {
    onSome: (kid) => Effect.succeed(kid),
    onNone: () =>
      Effect.tryPromise({
        try: async () => calculateJwkThumbprint(await exportJWK(key), "sha256"),
        catch: (cause) => new MaterialFault({ reason: "import", detail: String(cause) }),
      }),
  })

const _admit = (wire: CredentialPemWire): Effect.Effect<KeyHandle, MaterialFault> =>
  Effect.gen(function* () {
    const key = yield* _import(wire)
    const kid = yield* _kid(wire, key)
    const held = Redacted.make(key)
    return wire.format === "pkcs8" || (wire.format === "jwk" && wire.secret)
      ? _KeyHandle.Signing({ kid, alg: wire.alg, key: held })
      : _KeyHandle.Verify({ kid, alg: wire.alg, key: held })
  })
```

```typescript
// --- [SERVICES] -------------------------------------------------------------------------

class Material extends Effect.Service<Material>()("security/secret/Material", {
  succeed: {
    admit: _admit,
    thumbprint: (jwk: JWK): Effect.Effect<string, MaterialFault> =>
      Effect.tryPromise({ try: () => calculateJwkThumbprint(jwk, "sha256"), catch: (cause) => new MaterialFault({ reason: "import", detail: String(cause) }) }),
    jwks: (keys: ReadonlyArray<Extract<KeyHandle, { readonly _tag: "Verify" }>>): Effect.Effect<JSONWebKeySet, MaterialFault> =>
      Effect.map(
        Effect.forEach(keys, (handle) =>
          Effect.tryPromise({
            try: async () => ({ ...(await exportJWK(Redacted.value(handle.key))), kid: handle.kid, alg: handle.alg, use: "sig" }) as JWK,
            catch: (cause) => new MaterialFault({ reason: "import", detail: String(cause) }),
          })),
        (list) => ({ keys: Array.fromIterable(list) }),
      ),
    ring: (input: { readonly signingPem: Redacted.Redacted<string>; readonly signingAlg: KeyAlg.Kind; readonly jwks: JSONWebKeySet }): Effect.Effect<Ring, MaterialFault> =>
      Effect.gen(function* () {
        const active = yield* _admit(new CredentialPemWire({ material: input.signingPem, format: "pkcs8", alg: input.signingAlg, secret: true, kid: Option.none() }))
        if (active._tag !== "Signing") return yield* Effect.fail(new MaterialFault({ reason: "import", detail: "signing wire resolved to a public key" }))
        const verify = yield* Effect.forEach(input.jwks.keys, (jwk) =>
          _admit(new CredentialPemWire({ material: Redacted.make(JSON.stringify(jwk)), format: "jwk", alg: (jwk.alg ?? input.signingAlg) as KeyAlg.Kind, secret: false, kid: Option.fromNullable(jwk.kid) })).pipe(
            Effect.flatMap((handle) => handle._tag === "Verify" ? Effect.succeed(handle) : Effect.fail(new MaterialFault({ reason: "import", detail: "jwks entry resolved to a private key" })))))
        return { active, verify }
      }),
  },
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { CredentialPemWire, KeyAlg, Material, MaterialFault }
export type { KeyHandle, Ring }
```

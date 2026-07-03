# [SECURITY_JWT] — jose JWT/JWS/JWKS mint, verify, and rotation — the one token-crypto owner

`sign/jwt` is the single JWT/JWS/JWKS authority: it mints own-issued access tokens with the active rotation key, verifies them against the local JWKS with a pinned algorithm allow-list and declarative claim gates, and verifies external OIDC `id_token`s against a cooldown-throttled `createRemoteJWKSet` resolver held per issuer. The claim gates are `jose`'s own `JWTClaimVerificationOptions` — `exp`/`nbf`/`aud`/`iss`/`maxTokenAge` under a `clockTolerance` — so no timestamp is re-checked by hand, and `algorithms` is always pinned because an unpinned `alg` is an accepted-algorithm-confusion hole. Keys arrive only as `KeyHandle`s from `secret/material` — the private signing key is unwrapped exactly into `SignJWT.sign`, never re-imported per call — and rotation is a `KeyRing` of one active `Signing` handle plus every published `Verify` handle, re-admitted on a `Schedule` through `Reloadable.auto`. The whole `JOSEError` family folds by its stable `code` into `JwtFault`, the folder's `Schema.TaggedError` reason-shape; `session/token` consumes `mint`/`verify` and `authn/oauth` consumes `verifyExternal`.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                       | [OWNER]                                 | [PACKAGES]                 | [REJECTED_FORM]                              |
| :-----: | :------------------------------ | :-------------------------------------- | :------------------------- | :------------------------------------------- |
|  [01]   | `JOSEError` → tagged fault      | `JwtFault` `code`-fold                  | `jose` errors              | an `instanceof` ladder over the error family |
|  [02]   | own-issued mint + local verify  | `Jwt.mint` / `Jwt.verify` over `KeyRing`| `jose`, `secret/material`  | a hand `exp` check, an unpinned `alg`        |
|  [03]   | external OIDC id_token verify   | `Jwt.verifyExternal` remote JWKS        | `jose`                     | a per-request JWKS fetch, `decodeJwt` as trust |

## [2]-[JWT_VOCABULARY]

[CLAIMS_AND_FAULT]:
- Owner: `AccessClaims` is the branded verified claim set — `jose`'s open `JWTPayload` decoded through one `Schema.Class` so the interior never trusts a raw claim; `JwtFault` is the `code`-folded reason-family. `session/token` re-brands `AccessClaims.sub` into its `SubjectId` at its own boundary, so this page owns the raw claim shape and stays upstream of `session`.
- Packages: `jose` — the closed `JOSEError` family carries a stable `code` (`ERR_JWT_EXPIRED`, `ERR_JWS_SIGNATURE_VERIFICATION_FAILED`, `ERR_JWKS_TIMEOUT`, …), so the fold is a `code` lookup, never 39 `instanceof` arms.
- Boundary: the edge maps `policy.status` (401 auth, 400 malformed, 503 transient-JWKS) to a problem detail; the `retry`-gated `jwks` reason drives a `Schedule` reload of the resolver.
- Growth: a new claim is one `AccessClaims` field; a new JOSE failure code is one `_reasonOf` arm plus, if novel, one `JwtFaultPolicy` row.

```typescript
import { createLocalJWKSet, createRemoteJWKSet, jwtVerify, SignJWT, type JSONWebKeySet, type JWTPayload, type JWTVerifyResult } from "jose"
import { Config, DateTime, Duration, Effect, Option, Redacted, Schema } from "effect"
import { KeyAlg, Material, type Ring } from "../secret/material.ts"

// --- [CONSTANTS] ------------------------------------------------------------------------

const _reasons = ["expired", "claim", "signature", "algorithm", "jwks", "malformed"] as const

const JwtFaultPolicy = {
  expired: { rank: 2, retry: false, status: 401 },
  claim: { rank: 3, retry: false, status: 401 },
  signature: { rank: 4, retry: false, status: 401 },
  algorithm: { rank: 5, retry: false, status: 401 },
  jwks: { rank: 2, retry: true, status: 503 },
  malformed: { rank: 3, retry: false, status: 400 },
} as const

const _codeReason = {
  ERR_JWT_EXPIRED: "expired",
  ERR_JWT_CLAIM_VALIDATION_FAILED: "claim",
  ERR_JWS_SIGNATURE_VERIFICATION_FAILED: "signature",
  ERR_JWS_INVALID: "signature",
  ERR_JWT_INVALID: "signature",
  ERR_JOSE_ALG_NOT_ALLOWED: "algorithm",
  ERR_JWKS_NO_MATCHING_KEY: "jwks",
  ERR_JWKS_MULTIPLE_MATCHING_KEYS: "jwks",
  ERR_JWKS_TIMEOUT: "jwks",
  ERR_JWKS_INVALID: "jwks",
} as const satisfies Record<string, keyof typeof JwtFaultPolicy>

declare namespace JwtFault {
  type Reason = keyof typeof JwtFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof JwtFaultPolicy> = T
}

// --- [MODELS] ---------------------------------------------------------------------------

class AccessClaims extends Schema.Class<AccessClaims>("AccessClaims")({
  sub: Schema.NonEmptyString,
  sid: Schema.NonEmptyString,
  scope: Schema.Array(Schema.NonEmptyString),
  tid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

// --- [ERRORS] ---------------------------------------------------------------------------

class JwtFault extends Schema.TaggedError<JwtFault>()("JwtFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): JwtFault.Row {
    return JwtFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<jwt:${this.reason}> ${this.detail}`
  }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

const _reasonOf = (cause: unknown): JwtFault.Reason => {
  const code = (cause as { readonly code?: string }).code
  return code !== undefined && code in _codeReason ? _codeReason[code as keyof typeof _codeReason] : "malformed"
}
```

## [3]-[MINT_AND_VERIFY]

[LOCAL]:
- Owner: `Jwt.mint`/`Jwt.verify` over the `KeyRing` — `mint` stamps `{ alg, kid }` from the active `Signing` handle so verifiers route by `kid`; `verify` runs `createLocalJWKSet` over every published `Verify` handle with `algorithms` pinned and the claim gates applied, then decodes the payload through `AccessClaims`.
- Packages: `jose` — `SignJWT` fluent builder + `sign`, `jwtVerify` with `JWTVerifyOptions`; `secret/material` supplies the `KeyRing`, `Material.jwks` renders the local set.
- Law: the private key unwraps only into `SignJWT.sign`; the claim gate (`issuer`/`audience`/`clockTolerance`) is one `Config`-sourced policy value; rotation re-admits the ring on a `Schedule` through `Reloadable.auto`, so a `kid` retires without a consumer edit.
- Receipt: `mint` returns the token `Redacted`; `verify` returns `AccessClaims`, never a bare `JWTPayload`.

```typescript
// --- [SERVICES] -------------------------------------------------------------------------

class Jwt extends Effect.Service<Jwt>()("security/sign/Jwt", {
  effect: Effect.gen(function* () {
    const material = yield* Material
    const issuer = yield* Config.string("JWT_ISSUER")
    const audience = yield* Config.string("JWT_AUDIENCE")
    const tolerance = yield* Config.integer("JWT_CLOCK_TOLERANCE").pipe(Config.withDefault(5))
    const signingPem = yield* Config.redacted("JWT_SIGNING_KEY")
    const signingAlg = yield* Config.literal("ES256", "ES384", "RS256", "EdDSA")("JWT_SIGNING_ALG").pipe(Config.withDefault<KeyAlg.Kind>("ES256"))
    const jwks = JSON.parse(Redacted.value(yield* Config.redacted("JWT_JWKS"))) as JSONWebKeySet
    const ring: Ring = yield* material.ring({ signingPem, signingAlg, jwks })
    const local = createLocalJWKSet(yield* material.jwks(ring.verify))
    const _remote = yield* Effect.cachedFunction((jwksUri: string) =>
      Effect.sync(() => createRemoteJWKSet(new URL(jwksUri), { cacheMaxAge: 600_000, cooldownDuration: 30_000 })))
    const _algorithms = ring.verify.map((handle) => handle.alg)
    const mint = (claims: AccessClaims, ttl: Duration.DurationInput): Effect.Effect<Redacted.Redacted<string>, JwtFault> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const exp = Math.floor(DateTime.toEpochMillis(now) / 1000) + Math.max(1, Math.round(Duration.toSeconds(Duration.decode(ttl))))
        const token = yield* Effect.tryPromise({
          try: () =>
            new SignJWT({ sid: claims.sid, scope: claims.scope, tid: Option.getOrUndefined(claims.tid) })
              .setProtectedHeader({ alg: ring.active.alg, kid: ring.active.kid })
              .setIssuedAt().setIssuer(issuer).setAudience(audience).setSubject(claims.sub).setExpirationTime(exp)
              .sign(Redacted.value(ring.active.key)),
          catch: (cause) => new JwtFault({ reason: _reasonOf(cause), detail: String(cause) }),
        })
        return Redacted.make(token)
      })
    const verify = (token: Redacted.Redacted<string>): Effect.Effect<AccessClaims, JwtFault> =>
      Effect.tryPromise({
        try: () => jwtVerify(Redacted.value(token), local, { algorithms: _algorithms, issuer, audience, clockTolerance: tolerance }),
        catch: (cause) => new JwtFault({ reason: _reasonOf(cause), detail: String(cause) }),
      }).pipe(Effect.flatMap((result: JWTVerifyResult<JWTPayload>) =>
        Schema.decodeUnknown(AccessClaims)(result.payload).pipe(Effect.mapError((cause) => new JwtFault({ reason: "claim", detail: String(cause) })))))
    const verifyExternal = (token: Redacted.Redacted<string>, provider: { readonly issuer: string; readonly audience: string; readonly jwksUri: string; readonly algorithms: ReadonlyArray<KeyAlg.Kind> }): Effect.Effect<JWTPayload, JwtFault> =>
      Effect.flatMap(_remote(provider.jwksUri), (resolver) =>
        Effect.tryPromise({
          try: () => jwtVerify(Redacted.value(token), resolver, { algorithms: provider.algorithms, issuer: provider.issuer, audience: provider.audience, clockTolerance: tolerance }),
          catch: (cause) => new JwtFault({ reason: _reasonOf(cause), detail: String(cause) }),
        }).pipe(Effect.map((result) => result.payload)))
    return { mint, verify, verifyExternal } as const
  }),
  dependencies: [Material.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AccessClaims, Jwt, JwtFault }
```

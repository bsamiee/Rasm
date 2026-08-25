# [SECURITY_OAUTH]

OAuth 2.0 authorization-code ceremony over `openid-client`, modeling every issuer as one vocabulary row rather than a method family: each row bundles its own `Config` requirement, its `Configuration` source, and its client-authentication strategy, and the ceremony legs — `url` builds the redirect (pushed through PAR or sealed in a signed request object where the bind's carriage elects it), `exchange` swaps the landed authorization response for a decoded `Grant`, `refresh` rotates the provider grant, `revoke` retires it, `logout` builds the RP-initiated end-session redirect — resolve into ONE uniform `Ceremony` closure over that `Configuration`, so per-provider PKCE, constructor arity, and credential shape all vanish behind one dispatch and a new provider is one row carrying exactly the config it needs. Each leg is a package function taking the `Configuration` first, so the per-provider client class dies with the variance it existed to hide and the roster becomes data: `google`/`microsoft`/`apple` and the generic self-hosted row discover their metadata, `github` states its endpoints as a `ServerMetadata` literal because plain OAuth 2.0 publishes no well-known document, and `apple` alone demands `teamId`/`keyId`/`pkcs8` because its client secret is a signed assertion. Every capability the ceremony gates on — PKCE support, the request carriage, the OIDC descriptor, the refresh grant, the revocation and end-session legs — is READ off that metadata rather than asserted per row, so no hand-copied endpoint drifts from what the issuer publishes. This two-leg ceremony is durable data, not an ad-hoc stash: `authorize` seals a `Departed` snapshot — kind, PKCE verifier, OIDC nonce, expiry — into the `OAuthStateStore` single-use port under a TTL, and `callback` consumes it exactly once across the redirect, type-witnessing the leg order; a replayed, foreign, expired, or provider-mismatched state is `OAuthFault.state` and lands the `state` row on the folder ledger, beside the admission and ceremony span an accepted callback lands under the same kind. Every fetch leg is internally resilient — the `Configuration`'s own deadline aborts the in-flight request and the `transport` arm (the only `unavailable`-classed row) re-drives under the branch retry budget's own class gate — while a body the issuer already sent is terminal by class and never spends that budget. OIDC `id_token`s verify through `crypt/sign`'s issuer-overloaded `Jwt.verify` against the folder's one JWKS custody, and the verified subject becomes the `CredentialRef` `authn/session` establishes from. `Match.instanceOf` triages the package fault family into `OAuthFault`, and every secret stays `Redacted`.

## [01]-[INDEX]

- [02]-[PROVIDER_ROSTER]: `Provider`, `Grant`, `Carriage`, `OAuthFault`, `Departed`, `OAuthStateStore`.
- [03]-[CEREMONY]: `OAuth`.
- [04]-[GRANT_LIFECYCLE]: `OAuth`, `Renewal`, `Farewell`.

## [02]-[PROVIDER_ROSTER]

[PROVIDER_ROSTER]:
- Owner: `Provider` is the closed provider table — each row carries `scopes`, the vendor-mandated authorization `parameters`, and one `admission` effect resolving exactly the credential fields the provider demands into an `Admission`: the client id, the redirect, the `Issuer` the `Configuration` builds from, and the `ClientAuth` strategy; `_bind` folds one `Admission` into a `Bound`; `Grant` is the decoded token response every leg answers; `OAuthFault` is the folded fault; `Departed` is the serializable ceremony snapshot; `OAuthStateStore` is the `SingleUse<Departed>` port.
- Law: a provider is data over `buildAuthorizationUrl`/`authorizationCodeGrant`/`refreshTokenGrant`/`tokenRevocation`, and those four take the `Configuration` first — so ONE `_ceremony` construction serves the whole roster where a per-issuer client class needed one closure literal per row to normalize five different constructor arities. Each row's `Config` is that row's whole credential contract — Apple's `teamId`/`keyId`/`pkcs8`, Microsoft's `tenant`, the generic row's `issuer` URL are per-row fields, so no provider reads another's knobs; the `OAUTH_PROVIDERS` roster decides which rows resolve, every enabled bag resolves at the service boot line, and a kind outside the roster refuses `provider` typed.
- Law: `Bound` holds every RESOLVED fact and the row holds only what the deployment declares, because an issuer's endpoints, JWKS URI, signing roster, PKCE support, refresh grant, and revocation capability are all readable and a hand-asserted copy of a readable value drifts the first time the issuer moves. `Issuer.Discovered` reads them once at bind through `discovery`'s `serverMetadata()`; `Issuer.Pinned` states them as a `ServerMetadata` literal under the vendor's own contract, which `github` alone takes because plain OAuth 2.0 publishes no well-known document — and because the literal feeds the SAME metadata read, the capability github lacks falls out of what it declines to publish rather than out of a per-row flag someone must keep honest. Bind refuses `provider` on a missing authorization or token endpoint, lowers `hasRevoke` to the typed lifecycle refusal where no `revocation_endpoint` is published rather than dialing a URL the issuer never named, and intersects the advertised signing roster with the folder's own `KeyAlg` table so an unverifiable issuer refuses before its first id_token. Request CARRIAGE is the same kind of fact — a row-supplied JAR key elects the signed request object and refuses where the advertised request-object roster misses its scheme, a published `pushed_authorization_request_endpoint` elects PAR, an issuer requiring PAR without publishing the endpoint dies at bind, and `end_session_endpoint` decides the logout leg exactly as `revocation_endpoint` decides revoke.
- Law: a TEMPLATED issuer refuses at bind. Microsoft's multi-tenant `common` endpoint publishes `https://login.microsoftonline.com/{tenantid}/v2.0`, a string no id_token ever carries, so pinning it as the verify issuer rejects every token that arrives — the `tenant` field therefore carries no default and the template dies at boot rather than at the first sign-in.
- Law: `Departed` is a `Schema.Class` so the ceremony snapshot is wire-serializable — the state store persists it across the redirect and any process restart, the single-use consume is the transition witness, and the TTL is stamped as `expiresAt` data checked on land; the satisfying layer is a `Cache`/`PersistedCache` row over the `SingleUse` contract, never a hand-rolled map. `Departed` carries the `nonce` beside the `verifier`, because an OIDC replay defense the callback cannot present is no defense.
- Law: `Grant` is the decode, never a cast — `TokenEndpointResponse` carries an untyped index band beside its named fields, so every token leg answers a decoded `Grant` with canonical names over the provider's snake-case wire; a body missing `access_token` or spelling `expires_in` as prose is `OAuthFault.shape`, terminal by class, and the decode sits outside the retry so a deterministic parse failure never spends a transport budget.
- Growth: a new provider is one row and its `_kinds` entry; a self-hosted issuer reaches the generic row with zero endpoint knowledge; a multi-leg enrollment ceremony (device onboarding across restarts) is an `@effect/experimental` `Machine.makeSerializable` actor whose snapshot rides the same store.
- Boundary: the edge owns the redirect and hands the landed authorization response URL back whole; `authn/session` establishes the session; `crypt/sign` owns `Material` (Apple's assertion key), `Crypto` (its fingerprint), and the `Jwt` authority that verifies the id_token; `OAuthStateStore` is satisfied by a short-lived data or session band; `authn/workload` runs the machine-grant plane over the same package and reaches none of these legs — one custodian, two planes, each with its own `Configuration` per issuer.

```typescript
import {
  type AuthorizationCodeGrantChecks, AuthorizationResponseError, authorizationCodeGrant, buildAuthorizationUrl,
  buildAuthorizationUrlWithJAR, buildAuthorizationUrlWithPAR, buildEndSessionUrl,
  calculatePKCECodeChallenge, type ClientAuth, ClientError, ClientSecretBasic, ClientSecretPost, Configuration,
  discovery, modifyAssertion, PrivateKeyJwt, randomNonce, randomPKCECodeVerifier, randomState, refreshTokenGrant,
  ResponseBodyError, type ServerMetadata, type TokenEndpointResponse, tokenRevocation, WWWAuthenticateChallengeError,
} from "openid-client"
import { Fault } from "@rasm/core"
import {
  Array, Config, Context, Data, DateTime, Duration, Effect, Match, Option, Predicate, Redacted, Schema, Struct,
} from "effect"
import { Crypto, Jwt, KeyAlg, Material, type SingleUse } from "../crypt/sign.ts"
import { Reject } from "../crypt/verify.ts"
import { CredentialRef, type SessionFault, Token, type TokenPair } from "./session.ts"

const _kinds = ["google", "github", "microsoft", "apple", "generic"] as const

const _GITHUB: ServerMetadata = {
  issuer: "https://github.com",
  authorization_endpoint: "https://github.com/login/oauth/authorize",
  token_endpoint: "https://github.com/login/oauth/access_token",
  grant_types_supported: ["authorization_code"],
}

type Issuer = Data.TaggedEnum<{
  Discovered: { readonly url: URL }
  Pinned: { readonly metadata: ServerMetadata }
}>

const Issuer = Data.taggedEnum<Issuer>()

type Carriage = Data.TaggedEnum<{
  Plain: {}
  Pushed: {}
  Signed: { readonly key: Redacted.Redacted<CryptoKey> }
}>

const Carriage = Data.taggedEnum<Carriage>()

type Ceremony = {
  readonly url: (parameters: Record<string, string>) => URL
  readonly push: (parameters: Record<string, string>) => Promise<URL>
  readonly signed: (parameters: Record<string, string>, key: CryptoKey) => Promise<URL>
  readonly exchange: (landed: URL, checks: AuthorizationCodeGrantChecks) => Promise<TokenEndpointResponse>
  readonly refresh: (token: string, scopes: ReadonlyArray<string>) => Promise<TokenEndpointResponse>
  readonly revoke: (token: string) => Promise<void>
  readonly logout: (parameters: Record<string, string>) => URL
}

const Grant = Schema.Struct({
  accessToken: Schema.propertySignature(Schema.NonEmptyString).pipe(Schema.fromKey("access_token")),
  tokenType: Schema.propertySignature(Schema.Literal("bearer", "dpop")).pipe(Schema.fromKey("token_type")),
  expiresIn: Schema.optionalWith(Schema.Positive, { as: "Option" }).pipe(Schema.fromKey("expires_in")),
  refreshToken: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }).pipe(Schema.fromKey("refresh_token")),
  scope: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  idToken: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }).pipe(Schema.fromKey("id_token")),
})
type Grant = typeof Grant.Type

type Oidc = { readonly issuer: string; readonly jwksUri: string; readonly algorithms: ReadonlyArray<KeyAlg.Kind> }
type Admission = {
  readonly clientId: string
  readonly redirect: string
  readonly issuer: Issuer
  readonly auth: ClientAuth
  readonly jar: Option.Option<{ readonly alg: KeyAlg.Kind; readonly key: Redacted.Redacted<CryptoKey> }>
}
type Bound = {
  readonly clientId: string
  readonly redirect: string
  readonly ceremony: Ceremony
  readonly pkce: boolean
  readonly carriage: Carriage
  readonly oidc: Option.Option<Oidc>
  readonly hasRefresh: boolean
  readonly hasRevoke: boolean
  readonly hasEndSession: boolean
}
type ProviderRow = {
  readonly scopes: ReadonlyArray<string>
  readonly parameters: Record<string, string>
  readonly admission: Config.Config<Effect.Effect<Admission, OAuthFault, Crypto>>
}

const _states = ["absent", "unknown", "provider", "expired"] as const
const _lifecycles = ["refresh-grant", "revocation-endpoint", "end-session-endpoint"] as const

const _family = Fault.Class.family(
  ["provider", "transport", "shape", "state", "consent", "idToken", "lifecycle"] as const,
  {
    provider: Fault.Class.row({
      class: "invalid",
      leg: "discovery",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `provider configuration refused: ${cause}`,
    }),
    transport: Fault.Class.row({
      class: "unavailable",
      leg: "transport",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `provider leg did not complete: ${cause}`,
    }),
    shape: Fault.Class.row({
      class: "invalid",
      leg: "admission",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `provider response did not admit: ${cause}`,
    }),
    state: Fault.Class.row({
      class: "denied",
      leg: "ceremony",
      detail: Schema.Struct({ refusal: Schema.Literal(..._states) }),
      render: ({ refusal }) => `callback state refused: ${refusal}`,
    }),
    consent: Fault.Class.row({
      class: "denied",
      leg: "consent",
      detail: Schema.Struct({ code: Schema.String }),
      render: ({ code }) => `end-user consent refused: ${code}`,
    }),
    idToken: Fault.Class.row({
      class: "denied",
      leg: "identity",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `id_token refused: ${cause}`,
    }),
    lifecycle: Fault.Class.row({
      class: "invalid",
      leg: "lifecycle",
      detail: Schema.Struct({ missing: Schema.Literal(..._lifecycles) }),
      render: ({ missing }) => `provider publishes no ${missing}`,
    }),
  },
)

declare namespace OAuthFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class OAuthFault extends Schema.TaggedError<OAuthFault>()("OAuthFault", {
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

const _cfg = (name: string) => ({
  clientId: Config.string(`OAUTH_${name}_CLIENT_ID`),
  redirect: Config.string(`OAUTH_${name}_REDIRECT`),
})

const _admitted = <A>(
  config: Config.Config<A>,
  make: (values: A) => Admission,
): Config.Config<Effect.Effect<Admission, OAuthFault, Crypto>> =>
  Config.map(config, (values) => Effect.sync(() => make(values)))

const _published = (value: string | undefined, name: string): Effect.Effect<string, OAuthFault> =>
  Option.match(Option.fromNullable(value), {
    onNone: () => Effect.fail(new OAuthFault({ case: { reason: "provider", cause: `issuer publishes no ${name}` } })),
    onSome: Effect.succeed,
  })

const _literal = (issuer: string): Effect.Effect<string, OAuthFault> =>
  issuer.includes("{")
    ? Effect.fail(new OAuthFault({ case: { reason: "provider", cause: `templated issuer ${issuer}` } }))
    : Effect.succeed(issuer)

const _oidcOf = (
  metadata: Readonly<ServerMetadata>,
  issuer: string,
): Effect.Effect<Option.Option<Oidc>, OAuthFault> =>
  Option.match(Option.fromNullable(metadata.jwks_uri), {
    onNone: () => Effect.succeedNone,
    onSome: (jwksUri) => {
      const algorithms = Array.filter(Struct.keys(KeyAlg), (alg) =>
        Option.match(Option.fromNullable(metadata.id_token_signing_alg_values_supported), {
          onNone: () => alg === "RS256",
          onSome: (advertised) => Array.contains(advertised, alg),
        }))
      return Array.isNonEmptyReadonlyArray(algorithms)
        ? Effect.succeedSome({ issuer, jwksUri, algorithms })
        : Effect.fail(new OAuthFault({ case: { reason: "provider", cause: "no shared id_token signing algorithm" } }))
    },
  })

const _ceremony = (config: Configuration): Ceremony => ({
  url: (parameters) => buildAuthorizationUrl(config, parameters),
  push: (parameters) => buildAuthorizationUrlWithPAR(config, parameters),
  signed: (parameters, key) => buildAuthorizationUrlWithJAR(config, parameters, key),
  exchange: (landed, checks) => authorizationCodeGrant(config, landed, checks),
  refresh: (token, scopes) => refreshTokenGrant(config, token, { scope: scopes.join(" ") }),
  revoke: (token) => tokenRevocation(config, token, { token_type_hint: "access_token" }),
  logout: (parameters) => buildEndSessionUrl(config, parameters),
})

const _carriage = (
  metadata: Readonly<ServerMetadata>,
  jar: Admission["jar"],
): Effect.Effect<Carriage, OAuthFault> =>
  Option.match(jar, {
    onSome: ({ alg, key }) =>
      Option.match(Option.fromNullable(metadata.request_object_signing_alg_values_supported), {
        onNone: () => Effect.succeed(Carriage.Signed({ key })),
        onSome: (advertised) =>
          Array.contains(advertised, alg)
            ? Effect.succeed(Carriage.Signed({ key }))
            : Effect.fail(new OAuthFault({ case: { reason: "provider", cause: `issuer advertises no ${alg} request-object signing` } })),
      }),
    onNone: () =>
      Predicate.isNotUndefined(metadata.pushed_authorization_request_endpoint)
        ? Effect.succeed(Carriage.Pushed())
        : metadata.require_pushed_authorization_requests === true
          ? Effect.fail(new OAuthFault({ case: { reason: "provider", cause: "issuer requires PAR yet publishes no endpoint" } }))
          : Effect.succeed(Carriage.Plain()),
  })

const _bind = (admission: Admission, deadline: Duration.Duration): Effect.Effect<Bound, OAuthFault> =>
  Effect.gen(function* () {
    const config = yield* Effect.tryPromise({
      try: () =>
        Issuer.$match(admission.issuer, {
          Discovered: ({ url }) => discovery(url, admission.clientId, undefined, admission.auth),
          Pinned: ({ metadata }) =>
            Promise.resolve(new Configuration(metadata, admission.clientId, undefined, admission.auth)),
        }),
      catch: _faultOf,
    })
    config.timeout = Duration.toSeconds(deadline)
    const metadata = config.serverMetadata()
    yield* _published(metadata.authorization_endpoint, "authorization_endpoint")
    yield* _published(metadata.token_endpoint, "token_endpoint")
    const issuer = yield* _literal(metadata.issuer)
    const oidc = yield* _oidcOf(metadata, issuer)
    const carriage = yield* _carriage(metadata, admission.jar)
    return {
      clientId: admission.clientId,
      redirect: admission.redirect,
      ceremony: _ceremony(config),
      oidc,
      pkce: metadata.supportsPKCE(),
      carriage,
      hasRefresh: Option.match(Option.fromNullable(metadata.grant_types_supported), {
        onNone: () => false,
        onSome: (grants) => Array.contains(grants, "refresh_token"),
      }),
      hasRevoke: Predicate.isNotUndefined(metadata.revocation_endpoint),
      hasEndSession: Predicate.isNotUndefined(metadata.end_session_endpoint),
    }
  })

const _heldKey = (
  pkcs8: Redacted.Redacted<string>,
  alg: KeyAlg.Kind,
  horizon: Duration.Duration,
): Effect.Effect<Redacted.Redacted<CryptoKey>, OAuthFault, Crypto> =>
  Effect.gen(function* () {
    const cipher = yield* Crypto
    const now = yield* DateTime.now
    const handle = yield* Effect.mapError(
      Material.admit(
        Material.Source.Held({
          bundle: pkcs8,
          fingerprint: cipher.fingerprint(pkcs8),
          notBefore: now,
          notAfter: DateTime.addDuration(now, horizon),
        }),
        alg,
      ),
      (fault) => new OAuthFault({ case: { reason: "provider", cause: fault.message } }),
    )
    return yield* handle._tag === "Signing"
      ? Effect.succeed(handle.key)
      : Effect.fail(new OAuthFault({ case: { reason: "provider", cause: "held key admitted verify-only" } }))
  })

const _rows = {
  google: {
    scopes: ["openid", "email", "profile"], parameters: { access_type: "offline", prompt: "consent" },
    admission: _admitted(
      Config.all({ ..._cfg("GOOGLE"), secret: Config.redacted("OAUTH_GOOGLE_SECRET") }),
      ({ clientId, redirect, secret }) => ({
        clientId, redirect,
        issuer: Issuer.Discovered({ url: new URL("https://accounts.google.com") }),
        auth: ClientSecretPost(Redacted.value(secret)),
        jar: Option.none(),
      })),
  },
  github: {
    scopes: ["read:user", "user:email"], parameters: {},
    admission: _admitted(
      Config.all({ ..._cfg("GITHUB"), secret: Config.redacted("OAUTH_GITHUB_SECRET") }),
      ({ clientId, redirect, secret }) => ({
        clientId, redirect,
        issuer: Issuer.Pinned({ metadata: _GITHUB }),
        auth: ClientSecretPost(Redacted.value(secret)),
        jar: Option.none(),
      })),
  },
  microsoft: {
    scopes: ["openid", "email", "profile", "offline_access"], parameters: {},
    admission: _admitted(
      Config.all({
        ..._cfg("MICROSOFT"),
        secret: Config.redacted("OAUTH_MICROSOFT_SECRET"),
        tenant: Config.string("OAUTH_MICROSOFT_TENANT"),
      }),
      ({ clientId, redirect, secret, tenant }) => ({
        clientId, redirect,
        issuer: Issuer.Discovered({ url: new URL(`https://login.microsoftonline.com/${tenant}/v2.0`) }),
        auth: ClientSecretPost(Redacted.value(secret)),
        jar: Option.none(),
      })),
  },
  apple: {
    scopes: ["name", "email"], parameters: { response_mode: "form_post" },
    admission: Config.map(
      Config.all({
        ..._cfg("APPLE"),
        teamId: Config.string("OAUTH_APPLE_TEAM_ID"),
        keyId: Config.string("OAUTH_APPLE_KEY_ID"),
        pkcs8: Config.redacted("OAUTH_APPLE_PKCS8"),
        horizon: Config.duration("OAUTH_APPLE_KEY_HORIZON").pipe(Config.withDefault(Duration.days(180))),
      }),
      ({ clientId, horizon, keyId, pkcs8, redirect, teamId }) =>
        Effect.gen(function* () {
          const key = yield* _heldKey(pkcs8, "ES256", horizon)
          return {
            clientId, redirect,
            issuer: Issuer.Discovered({ url: new URL("https://appleid.apple.com") }),
            auth: PrivateKeyJwt({ key: Redacted.value(key), kid: keyId }, {
              [modifyAssertion]: (_header, payload) => {
                payload.iss = teamId
              },
            }),
            jar: Option.none(),
          }
        }),
    ),
  },
  generic: {
    scopes: ["openid", "email", "profile"], parameters: {},
    admission: Config.map(
      Config.all({
        ..._cfg("GENERIC"),
        secret: Config.redacted("OAUTH_GENERIC_SECRET"),
        issuer: Config.string("OAUTH_GENERIC_ISSUER"),
        scheme: Config.literal("post", "basic")("OAUTH_GENERIC_AUTH").pipe(Config.withDefault("post" as const)),
        jarPkcs8: Config.option(Config.redacted("OAUTH_GENERIC_JAR_PKCS8")),
        jarHorizon: Config.duration("OAUTH_GENERIC_JAR_HORIZON").pipe(Config.withDefault(Duration.days(180))),
      }),
      ({ clientId, issuer, jarHorizon, jarPkcs8, redirect, scheme, secret }) =>
        Effect.gen(function* () {
          const jar = yield* Option.match(jarPkcs8, {
            onNone: () => Effect.succeedNone,
            onSome: (pkcs8) => Effect.map(_heldKey(pkcs8, "ES256", jarHorizon), (key) => Option.some({ alg: "ES256" as const, key })),
          })
          return {
            clientId, redirect,
            issuer: Issuer.Discovered({ url: new URL(issuer) }),
            auth: scheme === "post" ? ClientSecretPost(Redacted.value(secret)) : ClientSecretBasic(Redacted.value(secret)),
            jar,
          }
        }),
    ),
  },
} as const satisfies Record<(typeof _kinds)[number], ProviderRow>

declare namespace Provider {
  type Kind = keyof typeof _rows
  type _Keys<K extends Kind = (typeof _kinds)[number]> = K
  type _Kinds<K extends (typeof _kinds)[number] = Kind> = K
}

class Departed extends Schema.Class<Departed>("Departed")({
  kind: Schema.Literal(..._kinds),
  verifier: Schema.optionalWith(Schema.String, { as: "Option" }),
  nonce: Schema.optionalWith(Schema.String, { as: "Option" }),
  expiresAt: Schema.DateTimeUtc,
}) {}

class OAuthStateStore extends Context.Tag("security/authn/OAuthStateStore")<OAuthStateStore, SingleUse<Departed, OAuthFault>>() {}
```

## [03]-[CEREMONY]

[CEREMONY]:
- Owner: `OAuth.authorize` mints `state`, the PKCE verifier, and the OIDC nonce, seals the `Departed` snapshot under the ceremony TTL, and returns the redirect `URL`; `OAuth.callback` takes the LANDED authorization response, consumes the snapshot exactly once, gates kind and expiry, exchanges under the resilient leg, verifies the OIDC `id_token`, reads the grant's expiry and scopes, and establishes the session. Dispatch is by `Provider.Kind`; every enabled row's `Config` resolves at the service head and the bound handle constructs once per kind under `Effect.cachedFunction`.
- Law: `callback` takes the whole authorization response URL, not a pair of decoded params, because the response carries facts the exchange itself validates — the RFC 9207 `iss` an issuer advertising it REQUIRES present, the `error`/`error_description` pair a declined consent arrives as, and the `state` this page looks its snapshot up by. Rebuilding that URL from two named params either drops `iss` and refuses every callback from a Keycloak or Google issuer, or forges it and defeats the mix-up check outright. Stripping the URL is how the token request derives its `redirect_uri`, so the ROW's own registered redirect supplies origin and path while only the search rides in from the edge — a URL rebuilt off a request `Host` header sends an attacker-chosen or proxy-internal value.
- Law: the state is consumed single-use so a replayed or foreign state is `OAuthFault.state`, a stale snapshot is `OAuthFault.state` on the expiry gate, and both land `Reject.mark("state")` while a completed callback lands its `state`-kinded admission and ceremony span through `Reject.measured` — the redirect surface's replay rate reads against its own completion rate; the verifier is never client-readable. `expectedState` receives that same value at the exchange, so the reconstructed URL asserts against what the snapshot already proved rather than trusting it twice.
- Law: the package's in-grant id-token handling is a CLAIM check, never verification — it validates `iss`, `aud`, `exp`, `azp`, and the advertised algorithm against a token delivered on the TLS-authenticated token-endpoint channel, and reaches no JWKS unless non-repudiation is switched on. Signature authority stays `Jwt.verify(token, issuer)`, which pins issuer/audience/algorithms against the row's `oidc` and reads the folder's ONE JWKS custody, so the estate fetches an issuer's key set once rather than once per library. `Grant` carries `idToken` as an `Option` once decoded, so an OIDC row whose exchange returns no `id_token` is `OAuthFault.idToken`, never a defect; a non-OIDC row resolves its subject through the caller's `resolveSubject` over that same decoded grant; the granted `scope`, not the requested scope, seeds the session.
- Law: the nonce is minted for every OIDC row and presented back as `expectedNonce`, so a replayed id_token refuses inside the exchange — the snapshot that already carries the PKCE verifier across the redirect is exactly where the nonce belongs, and a defense the callback cannot present is no defense.
- Law: decoding lands between the retry and the first field read — `_granted` folds `_leg` into `_decoded`, so a severed socket or an aborted deadline re-drives under the budget while a malformed body refuses once as `shape`; running the two the other way around re-asks a deterministic parse failure until the schedule exhausts and grades a caller-visible defect as an outage. Triage grades its DEFAULT arm terminal for the same reason: only a bare network `TypeError` or an abort `DOMException` is genuinely retryable, and the protocol error the root barrel does not export otherwise rides an `orElse` straight into the outage budget.
- Law: the carriage reshapes the SEQUENCE, never the contract — the `Departed` snapshot seals before any carriage leg runs, so the state a provider echoes always names a consumable ceremony; the pushed arm redirects with only `client_id` and `request_uri` while verifier, nonce, and scopes travel the PAR back channel, the signed arm seals the same parameter set under the row's admitted key, and both ride `_leg`'s class-gated retry because a push is an authenticated network call the plain arm never makes. `callback` is carriage-blind — PKCE, nonce, and state verify identically whichever way the request travelled.
- Receipt: `URL` on authorize (the edge redirects), `TokenPair` on callback (the edge frames it) — never a raw `TokenEndpointResponse`.
- Growth: a new provider is one row; a new claim projection is one `resolveSubject` composition; a form-post provider is a POST callback route handing the same seam its `Request`, which the exchange leg already admits.
- Boundary: `authn/session` `Token.establish` mints the session; `crypt/sign` verifies external tokens and admits Apple's assertion key; the state store is data/session-satisfied; the composition root binds `Crypto` behind this service's layer, so the deferred binds carry no requirement into the ceremony members.

```typescript
const _decoded = (response: TokenEndpointResponse): Effect.Effect<Grant, OAuthFault> =>
  Schema.decodeUnknown(Grant)(response).pipe(
    Effect.mapError((issue) => new OAuthFault({ case: { reason: "shape", cause: issue.message } })),
  )

const _scopes = (grant: Grant, requested: ReadonlyArray<string>): ReadonlyArray<string> =>
  Option.match(grant.scope, { onNone: () => requested, onSome: (granted) => granted.split(" ") })

const _landed = (redirect: string, response: URL): URL => {
  const landed = new URL(redirect)
  landed.search = response.search
  return landed
}

const _challenged = (error: WWWAuthenticateChallengeError): string =>
  Option.match(Array.head(error.cause), {
    onNone: () => String(error.status),
    onSome: (challenge) =>
      Option.getOrElse(Option.fromNullable(challenge.parameters.error), () => String(error.status)),
  })

const _severed = (cause: unknown): boolean =>
  cause instanceof DOMException || (cause instanceof TypeError && !Predicate.hasProperty(cause, "code"))

const _faultOf: (cause: unknown) => OAuthFault = Match.type<unknown>().pipe(
  Match.when(Match.instanceOf(AuthorizationResponseError), (error) => new OAuthFault({ case: { reason: "consent", code: error.error } })),
  Match.when(Match.instanceOf(ResponseBodyError), (error) => new OAuthFault({ case: { reason: "provider", cause: error.error } })),
  Match.when(Match.instanceOf(WWWAuthenticateChallengeError), (error) => new OAuthFault({ case: { reason: "provider", cause: _challenged(error) } })),
  Match.when(Match.instanceOf(ClientError), (error) => new OAuthFault({ case: { reason: "shape", cause: Option.getOrElse(Option.fromNullable(error.code), () => error.message) } })),
  Match.when(_severed, (error) => new OAuthFault({ case: { reason: "transport", cause: String(error) } })),
  Match.orElse((error) => new OAuthFault({ case: { reason: "shape", cause: String(error) } })),
)

class OAuth extends Effect.Service<OAuth>()("security/authn/OAuth", {
  effect: Effect.gen(function* () {
    const jwt = yield* Jwt
    const token = yield* Token
    const states = yield* OAuthStateStore
    const ceremonyTtl = yield* Config.duration("OAUTH_CEREMONY_TTL").pipe(
      Config.withDefault(Duration.minutes(10)),
      Config.withDescription("Departed snapshot lease bounding the redirect round trip"),
    )
    const legDeadline = yield* Config.duration("OAUTH_LEG_DEADLINE").pipe(
      Config.withDefault(Duration.seconds(10)),
      Config.withDescription("per-leg provider deadline the Configuration aborts on"),
    )
    const enabled = yield* Config.array(Config.literal(..._kinds)(), "OAUTH_PROVIDERS").pipe(
      Config.withDefault([]),
      Config.withDescription("enabled provider kinds; each named row's credential bag resolves at this boot line"),
    )
    const custody = yield* Effect.context<Crypto>()
    const binds = new Map(Array.zip(
      enabled,
      Array.map(
        yield* Config.all(Array.map(enabled, (kind) => _rows[kind].admission)),
        (admission) => Effect.provide(Effect.flatMap(admission, (admitted) => _bind(admitted, legDeadline)), custody),
      ),
    ))
    const _leg = <A>(run: () => Promise<A>): Effect.Effect<A, OAuthFault> =>
      Effect.tryPromise({ try: run, catch: _faultOf }).pipe(
        Effect.retry(Fault.Budget.schedule("pulse")),
      )
    const _granted = (run: () => Promise<TokenEndpointResponse>): Effect.Effect<Grant, OAuthFault> =>
      Effect.flatMap(_leg(run), _decoded)
    const _binding = yield* Effect.cachedFunction((kind: Provider.Kind) =>
      Option.match(Option.fromNullable(binds.get(kind)), {
        onNone: () => Effect.fail(new OAuthFault({ case: { reason: "provider", cause: `${kind} not enabled` } })),
        onSome: (bind) => bind,
      }))
    const _row = (kind: Provider.Kind): ProviderRow => _rows[kind]
    const authorize = (kind: Provider.Kind): Effect.Effect<URL, OAuthFault> =>
      Effect.gen(function* () {
        const bound = yield* _binding(kind)
        const row = _row(kind)
        const state = randomState()
        const verifier = bound.pkce ? Option.some(randomPKCECodeVerifier()) : Option.none<string>()
        const nonce = Option.isSome(bound.oidc) ? Option.some(randomNonce()) : Option.none<string>()
        const challenge = yield* Option.match(verifier, {
          onNone: () => Effect.succeedNone,
          onSome: (held) => Effect.map(Effect.promise(() => calculatePKCECodeChallenge(held)), Option.some),
        })
        const now = yield* DateTime.now
        const departed = new Departed({ kind, verifier, nonce, expiresAt: DateTime.addDuration(now, ceremonyTtl) })
        yield* states.stash(state, departed, ceremonyTtl)
        const parameters = {
          ...row.parameters,
          ...Array.isNonEmptyReadonlyArray(row.scopes) ? { scope: row.scopes.join(" ") } : {},
          ...Option.match(nonce, { onNone: () => ({}), onSome: (held) => ({ nonce: held }) }),
          ...Option.match(challenge, {
            onNone: () => ({}),
            onSome: (held) => ({ code_challenge: held, code_challenge_method: "S256" }),
          }),
          redirect_uri: bound.redirect,
          state,
        }
        return yield* Carriage.$match(bound.carriage, {
          Plain: () => Effect.try({ try: () => bound.ceremony.url(parameters), catch: _faultOf }),
          Pushed: () => _leg(() => bound.ceremony.push(parameters)),
          Signed: ({ key }) => _leg(() => bound.ceremony.signed(parameters, Redacted.value(key))),
        })
      }).pipe(Effect.withSpan("security.oauth.authorize", { attributes: { kind } }))
    const callback = (kind: Provider.Kind, response: URL, resolveSubject: (grant: Grant) => Effect.Effect<string, OAuthFault>): Effect.Effect<TokenPair, OAuthFault | SessionFault> =>
      Effect.gen(function* () {
        const presented = yield* Option.match(Option.fromNullable(response.searchParams.get("state")), {
          onNone: () => Effect.fail(new OAuthFault({ case: { reason: "state", refusal: "absent" } })),
          onSome: Effect.succeed,
        })
        const departed = yield* Effect.flatMap(states.consume(presented), Option.match({
          onNone: () => Effect.fail(new OAuthFault({ case: { reason: "state", refusal: "unknown" } })),
          onSome: Effect.succeed,
        }))
        const now = yield* DateTime.now
        yield* Effect.succeed(departed).pipe(
          Effect.filterOrFail((held) => held.kind === kind, () => new OAuthFault({ case: { reason: "state", refusal: "provider" } })),
          Effect.filterOrFail((held) => DateTime.lessThanOrEqualTo(now, held.expiresAt), () => new OAuthFault({ case: { reason: "state", refusal: "expired" } })),
        )
        const bound = yield* _binding(kind)
        const grant = yield* _granted(() =>
          bound.ceremony.exchange(_landed(bound.redirect, response), {
            ...Option.match(departed.verifier, { onNone: () => ({}), onSome: (held) => ({ pkceCodeVerifier: held }) }),
            ...Option.match(departed.nonce, { onNone: () => ({}), onSome: (held) => ({ expectedNonce: held }) }),
            expectedState: presented,
          }))
        const sub = yield* Option.match(bound.oidc, {
          onSome: (oidc) =>
            Option.match(grant.idToken, {
              onNone: () => Effect.fail(new OAuthFault({ case: { reason: "idToken", cause: "response carried none" } })),
              onSome: (raw) =>
                jwt.verify(Redacted.make(raw), { issuer: oidc.issuer, audience: bound.clientId, jwksUri: oidc.jwksUri, algorithms: oidc.algorithms }).pipe(
                  Effect.mapError((fault) => new OAuthFault({ case: { reason: "idToken", cause: fault.message } })),
                  Effect.map((payload) => String(payload.sub)),
                ),
            }),
          onNone: () => resolveSubject(grant),
        })
        const ref = new CredentialRef({ kind: "oauth", key: `${kind}:${sub}` })
        return yield* token.establish(ref, _scopes(grant, _row(kind).scopes), { tenant: Option.none(), verified: true })
      }).pipe(
        Effect.tapErrorTag("OAuthFault", (fault) => (fault.case.reason === "state" ? Reject.mark("state") : Effect.void)),
        Reject.measured("state"),
        Effect.withSpan("security.oauth.callback", { attributes: { kind } }),
      )
    const refresh = (kind: Provider.Kind, grant: Grant): Effect.Effect<Renewal, OAuthFault> =>
      Effect.flatMap(_binding(kind), (bound) => _lifecycle(bound, _row(kind).scopes, _leg, _granted).refresh(grant))
    const revoke = (kind: Provider.Kind, grant: Grant): Effect.Effect<void, OAuthFault> =>
      Effect.flatMap(_binding(kind), (bound) => _lifecycle(bound, _row(kind).scopes, _leg, _granted).revoke(grant))
    const logout = (kind: Provider.Kind, hint: Farewell): Effect.Effect<URL, OAuthFault> =>
      Effect.flatMap(_binding(kind), (bound) => _lifecycle(bound, _row(kind).scopes, _leg, _granted).logout(hint))
    return { authorize, callback, refresh, revoke, logout } as const
  }),
  dependencies: [Token.Default],
  accessors: true,
}) {}
```

## [04]-[GRANT_LIFECYCLE]

[GRANT_LIFECYCLE]:
- Owner: `OAuth.refresh` rotates a stored provider refresh grant when the bound issuer advertises one, projecting the fresh expiry and scopes as a `Renewal`; `OAuth.revoke` retires the grant on sign-out when the issuer publishes a revocation leg; `OAuth.logout` builds the RP-initiated end-session redirect from a `Farewell` hint when the issuer publishes one — the front-channel half of sign-out that `revoke`'s token-endpoint half never covers, since a revoked grant leaves the IdP session alive to silently re-authenticate the next redirect. All three dispatch by `Provider.Kind` through `_binding` and share the `_faultOf` triage; `_lifecycle` is the bound closure the service delegates to, declared before the service so every leg reads one shape.
- Law: both legs take a decoded `Grant`, never a live response object — a caller holding "a stored refresh grant" holds decoded fields that round-trip a store; the same decode that gates the exchange gates the rotation, and the rotated response is a `Grant` before any field is read.
- Law: the capability flags gate the legs and they are `Bound` facts read off the issuer's own publication, not row assertions — a provider whose advertised grant roster omits `refresh_token` or that published no `revocation_endpoint` short-circuits to `OAuthFault.lifecycle` rather than calling an endpoint that does not exist, so the unavailable capability is a typed refusal, never a swallowed no-op; the issuer capability and the grant's own `refreshToken` fold into ONE `Option`, so both absences answer through one arm; `hasEndSession` gates logout the same way, and each `Farewell` field rides only when the caller holds it — `id_token_hint` the issuer's own evidence of whose session ends, `post_logout_redirect_uri` the registered return, `state` the echo the return leg verifies.
- Law: the rotated expiry and scopes read off the fresh `Grant`, so a provider-side scope reduction propagates and the session re-establishes with the narrowed grant; `expires_in` is a lifetime, so the instant derives from the effect clock at rotation rather than a wall-clock read inside a library accessor, and a grant without one projects `none`.
- Receipt: `Renewal` — the fresh expiry and scopes the caller re-establishes a session from; `void` on revoke; the end-session `URL` the edge redirects on logout.
- Growth: a lifecycle capability change needs no edit at all — every flag re-reads from the issuer's metadata at the next bind; a sender-constrained rotation is the `DPoPOptions` argument both grant legs already admit; a second JAR signing scheme is one config row on the generic admission.
- Boundary: the caller composes `refresh` output back into `authn/session`; the provider grant storage is the caller's coordinate; the serve edge's logout ceremony arm composes `OAuth.logout` as its upstream redirect after the session revocation it already owns; this page owns only the browser ceremony's legs.
- Packages: `openid-client` (`refreshTokenGrant`, `tokenRevocation`, `buildEndSessionUrl` — the lifecycle legs, `buildAuthorizationUrlWithPAR`/`buildAuthorizationUrlWithJAR` the carriage builders, and `TokenEndpointResponse` the body `Grant` decodes).

`Renewal`, `Farewell`, and `_lifecycle` are declared above the `OAuth` service (the service delegates its `refresh`/`revoke`/`logout` members to it over a resolved `Bound` and the shared leg seams):

```typescript
type Renewal = { readonly expiresAt: Option.Option<DateTime.Utc>; readonly scopes: ReadonlyArray<string> }

type Farewell = {
  readonly idToken: Option.Option<string>
  readonly redirect: Option.Option<string>
  readonly state: Option.Option<string>
}

const _renewed = (fresh: Grant, requested: ReadonlyArray<string>): Effect.Effect<Renewal> =>
  Effect.map(
    Option.match(fresh.expiresIn, {
      onNone: () => Effect.succeedNone,
      onSome: (seconds) => Effect.map(DateTime.now, (now) => Option.some(DateTime.addDuration(now, Duration.seconds(seconds)))),
    }),
    (expiresAt) => ({ expiresAt, scopes: _scopes(fresh, requested) }),
  )

const _lifecycle = (
  bound: Bound,
  scopes: ReadonlyArray<string>,
  leg: <A>(run: () => Promise<A>) => Effect.Effect<A, OAuthFault>,
  granted: (run: () => Promise<TokenEndpointResponse>) => Effect.Effect<Grant, OAuthFault>,
) => ({
  refresh: (grant: Grant): Effect.Effect<Renewal, OAuthFault> =>
    Option.match(bound.hasRefresh ? grant.refreshToken : Option.none<string>(), {
      onNone: () => Effect.fail(new OAuthFault({ case: { reason: "lifecycle", missing: "refresh-grant" } })),
      onSome: (held) => Effect.flatMap(granted(() => bound.ceremony.refresh(held, scopes)), (fresh) => _renewed(fresh, scopes)),
    }),
  revoke: (grant: Grant): Effect.Effect<void, OAuthFault> =>
    bound.hasRevoke
      ? leg(() => bound.ceremony.revoke(grant.accessToken))
      : Effect.fail(new OAuthFault({ case: { reason: "lifecycle", missing: "revocation-endpoint" } })),
  logout: (hint: Farewell): Effect.Effect<URL, OAuthFault> =>
    bound.hasEndSession
      ? Effect.try({
          try: () =>
            bound.ceremony.logout({
              ...Option.match(hint.idToken, { onNone: () => ({}), onSome: (held) => ({ id_token_hint: held }) }),
              ...Option.match(hint.redirect, { onNone: () => ({}), onSome: (held) => ({ post_logout_redirect_uri: held }) }),
              ...Option.match(hint.state, { onNone: () => ({}), onSome: (held) => ({ state: held }) }),
            }),
          catch: _faultOf,
        })
      : Effect.fail(new OAuthFault({ case: { reason: "lifecycle", missing: "end-session-endpoint" } })),
})

// --- [EXPORTS] -------------------------------------------------------------------------

export { Departed, Grant, OAuth, OAuthFault, OAuthStateStore }
export type { Farewell, Provider, Renewal }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

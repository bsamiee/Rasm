# [SECURITY_OAUTH]

OAuth 2.0 authorization-code ceremony over `arctic`, modeling every issuer as one vocabulary row rather than a method family: each row bundles its own `Config` requirement and the four ceremony legs — `url` builds the redirect, `exchange` swaps the code for `OAuth2Tokens`, `refresh` rotates the provider grant, `revoke` retires it — into a uniform `Ceremony` closure, so per-provider PKCE, constructor arity, and credential shape all vanish behind one dispatch and a new provider is one row carrying exactly the config it needs — the wide all-providers credential bag is dead. Its roster is the full arctic surface as data: `google`/`github`/`microsoft`/`apple` (whose row alone demands `teamId`/`keyId`/`pkcs8`) and the generic `OAuth2Client` fallback for self-hosted Keycloak/Authentik/Okta issuers — all sharing the ceremony shape, anchored by the `_kinds` tuple with the guard pair closed in both directions. This two-leg ceremony is durable data, not an ad-hoc stash: `authorize` seals a `Departed` snapshot — kind, PKCE verifier, expiry — into the `OAuthStateStore` single-use port under a TTL, the browser redirect crosses any process boundary, and `callback` consumes the snapshot exactly once, type-witnessing the leg order; a replayed, foreign, expired, or provider-mismatched state is `OAuthFault.state` and lands the `state` row on the folder reject stream. Every fetch leg is internally resilient — a deadline bounds arctic's own `fetch`, and the `transport` arm (`ArcticFetchError`, the only retryable classification) re-drives under a bounded jittered exponential while `OAuth2RequestError` stays terminal. OIDC `id_token`s verify through `crypt/sign`'s issuer-overloaded `Jwt.verify` — arctic's `decodeIdToken` is never trusted — and the verified subject becomes the `CredentialRef` `authn/session` establishes from. Arctic's fault family triages by `Match.instanceOf` into `OAuthFault`, and every secret stays `Redacted`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [PUBLIC]                                                |
| :-----: | :---------------- | :------------------------------------------------------ |
|  [01]   | `PROVIDER_ROSTER` | `Provider`, `OAuthFault`, `Departed`, `OAuthStateStore` |
|  [02]   | `CEREMONY`        | `OAuth`                                                 |
|  [03]   | `GRANT_LIFECYCLE` | `OAuth`                                                 |

## [02]-[PROVIDER_ROSTER]

[PROVIDER_ROSTER]:
- Owner: `Provider` is the closed provider table — each row carries `scopes`, a PKCE flag, an optional OIDC `{ issuer, jwksUri, algorithms }`, `hasRefresh`/`hasRevoke` availability flags, and a `ceremony` `Config` that resolves exactly the credential fields the provider demands and yields the bound arctic client behind the uniform four-leg closure; `OAuthFault` is the folded fault; `Departed` is the serializable ceremony snapshot; `OAuthStateStore` is the `SingleUse<Departed>` port.
- Law: the 60-plus arctic provider classes share the ceremony shape, so a row is data over `createAuthorizationURL`/`validateAuthorizationCode`/`refreshAccessToken`/`revokeToken`; the row's `Config` is its whole credential contract — Apple's `teamId`/`keyId`/`pkcs8`, Microsoft's `tenant`, the generic row's `issuer` URL are per-row fields, so an unconfigured provider fails at first use with a precise `ConfigError` and no provider reads another's knobs.
- Law: `Departed` is a `Schema.Class` so the ceremony snapshot is wire-serializable — the state store persists it across the redirect and any process restart, the single-use consume is the transition witness, and the TTL is stamped as `expiresAt` data checked on land; the satisfying layer is a `Cache`/`PersistedCache` row over the `SingleUse` contract, never a hand-rolled map.
- Law: the fault rows carry the core `FaultClass` kind with the guard pair closed — `transport` is `unavailable` (the one retryable arm), `provider` is `invalid` (the issuer rejected the request; terminal), `shape` is `invalid`, `state` and `idToken` are `denied`, `lifecycle` is `invalid`.
- Growth: a new provider is one row and its `_kinds` entry; a self-hosted issuer reaches the generic `OAuth2Client` in the same row shape; a multi-leg enrollment ceremony (device onboarding across restarts) is an `@effect/experimental` `Machine.makeSerializable` actor whose snapshot rides the same store.
- Boundary: the edge owns the redirect and callback URL params; `authn/session` establishes the session; `crypt/sign` verifies the id_token; `OAuthStateStore` is satisfied by a short-lived data or session band.
- Packages: `arctic` (`OAuth2Client`, `Google`/`GitHub`/`MicrosoftEntraId`/`Apple`, `generateState`/`generateCodeVerifier`, `OAuth2Tokens`, the fault family); `effect` (`Config`, `Context`, `Match`, `Redacted`, `Schema`); `@rasm/ts/core` (`FaultClass`); `crypt/sign` (`SingleUse`).

```typescript
import {
  Apple, ArcticFetchError, CodeChallengeMethod, generateCodeVerifier, generateState, GitHub, Google, MicrosoftEntraId,
  OAuth2Client, type OAuth2Tokens, OAuth2RequestError, UnexpectedErrorResponseBodyError, UnexpectedResponseError,
} from "arctic"
import { FaultClass } from "@rasm/ts/core"
import { Config, Context, DateTime, Duration, Effect, Match, Option, Redacted, Schedule, Schema } from "effect"
import { Jwt, type KeyAlg, type SingleUse } from "../crypt/sign.ts"
import { Reject } from "../crypt/verify.ts"
import { CredentialRef, type SessionFault, Token, type TokenPair } from "./session.ts"

const _kinds = ["google", "github", "microsoft", "apple", "generic"] as const

type Ceremony = {
  readonly url: (state: string, verifier: string, scopes: ReadonlyArray<string>) => URL
  readonly exchange: (code: string, verifier: string) => Promise<OAuth2Tokens>
  readonly refresh: (token: string, scopes: ReadonlyArray<string>) => Promise<OAuth2Tokens>
  readonly revoke: (token: string) => Promise<void>
}
type Bound = { readonly clientId: string; readonly ceremony: Ceremony }
type Oidc = { readonly issuer: string; readonly jwksUri: string; readonly algorithms: ReadonlyArray<KeyAlg.Kind> }
type ProviderRow = {
  readonly scopes: ReadonlyArray<string>
  readonly pkce: boolean
  readonly hasRefresh: boolean
  readonly hasRevoke: boolean
  readonly oidc?: Oidc
  readonly ceremony: Config.Config<Bound>
}

const _cfg = (name: string) => ({
  clientId: Config.string(`OAUTH_${name}_CLIENT_ID`),
  redirect: Config.string(`OAUTH_${name}_REDIRECT`),
})

const _rows = {
  google: {
    scopes: ["openid", "email", "profile"], pkce: true, hasRefresh: true, hasRevoke: true,
    oidc: { issuer: "https://accounts.google.com", jwksUri: "https://www.googleapis.com/oauth2/v3/certs", algorithms: ["RS256"] },
    ceremony: Config.map(
      Config.all({ ..._cfg("GOOGLE"), secret: Config.redacted("OAUTH_GOOGLE_SECRET") }),
      ({ clientId, redirect, secret }) => {
        const client = new Google(clientId, Redacted.value(secret), redirect)
        return {
          clientId,
          ceremony: {
            url: (state, verifier, scopes) => client.createAuthorizationURL(state, verifier, scopes),
            exchange: (code, verifier) => client.validateAuthorizationCode(code, verifier),
            refresh: (token, scopes) => client.refreshAccessToken(token, scopes),
            revoke: (token) => client.revokeToken(token),
          },
        }
      }),
  },
  github: {
    scopes: ["read:user", "user:email"], pkce: false, hasRefresh: false, hasRevoke: false,
    ceremony: Config.map(
      Config.all({ ..._cfg("GITHUB"), secret: Config.redacted("OAUTH_GITHUB_SECRET") }),
      ({ clientId, redirect, secret }) => {
        const client = new GitHub(clientId, Redacted.value(secret), redirect)
        return {
          clientId,
          ceremony: {
            url: (state, _verifier, scopes) => client.createAuthorizationURL(state, scopes),
            exchange: (code, _verifier) => client.validateAuthorizationCode(code),
            refresh: () => Promise.reject(new Error("github has no refresh grant")),
            revoke: () => Promise.resolve(),
          },
        }
      }),
  },
  microsoft: {
    scopes: ["openid", "email", "profile", "offline_access"], pkce: true, hasRefresh: true, hasRevoke: false,
    oidc: { issuer: "https://login.microsoftonline.com/common/v2.0", jwksUri: "https://login.microsoftonline.com/common/discovery/v2.0/keys", algorithms: ["RS256"] },
    ceremony: Config.map(
      Config.all({ ..._cfg("MICROSOFT"), secret: Config.redacted("OAUTH_MICROSOFT_SECRET"), tenant: Config.string("OAUTH_MICROSOFT_TENANT").pipe(Config.withDefault("common")) }),
      ({ clientId, redirect, secret, tenant }) => {
        const client = new MicrosoftEntraId(tenant, clientId, Redacted.value(secret), redirect)
        return {
          clientId,
          ceremony: {
            url: (state, verifier, scopes) => client.createAuthorizationURL(state, verifier, scopes),
            exchange: (code, verifier) => client.validateAuthorizationCode(code, verifier),
            refresh: (token, scopes) => client.refreshAccessToken(token, scopes),
            revoke: () => Promise.resolve(),
          },
        }
      }),
  },
  apple: {
    scopes: ["openid", "email", "name"], pkce: false, hasRefresh: true, hasRevoke: true,
    oidc: { issuer: "https://appleid.apple.com", jwksUri: "https://appleid.apple.com/auth/keys", algorithms: ["ES256"] },
    ceremony: Config.map(
      Config.all({
        ..._cfg("APPLE"),
        teamId: Config.string("OAUTH_APPLE_TEAM_ID"),
        keyId: Config.string("OAUTH_APPLE_KEY_ID"),
        pkcs8: Config.redacted("OAUTH_APPLE_PKCS8"),
      }),
      ({ clientId, keyId, pkcs8, redirect, teamId }) => {
        const client = new Apple(clientId, teamId, keyId, new TextEncoder().encode(Redacted.value(pkcs8)), redirect)
        return {
          clientId,
          ceremony: {
            url: (state, _verifier, scopes) => client.createAuthorizationURL(state, scopes),
            exchange: (code, _verifier) => client.validateAuthorizationCode(code),
            refresh: (token, scopes) => client.refreshAccessToken(token, scopes),
            revoke: (token) => client.revokeToken(token),
          },
        }
      }),
  },
  generic: {
    scopes: ["openid", "email", "profile"], pkce: true, hasRefresh: true, hasRevoke: true,
    ceremony: Config.map(
      Config.all({ ..._cfg("GENERIC"), secret: Config.redacted("OAUTH_GENERIC_SECRET"), issuer: Config.string("OAUTH_GENERIC_ISSUER") }),
      ({ clientId, issuer, redirect, secret }) => {
        const client = new OAuth2Client(clientId, Redacted.value(secret), redirect)
        const base = new URL(issuer)
        const authorizeEndpoint = new URL("/authorize", base).toString()
        const tokenEndpoint = new URL("/token", base).toString()
        const revokeEndpoint = new URL("/revoke", base).toString()
        return {
          clientId,
          ceremony: {
            url: (state, verifier, scopes) => client.createAuthorizationURLWithPKCE(authorizeEndpoint, state, CodeChallengeMethod.S256, verifier, scopes),
            exchange: (code, verifier) => client.validateAuthorizationCode(tokenEndpoint, code, verifier),
            refresh: (token, scopes) => client.refreshAccessToken(tokenEndpoint, token, scopes),
            revoke: (token) => client.revokeToken(revokeEndpoint, token),
          },
        }
      }),
  },
} as const satisfies Record<(typeof _kinds)[number], ProviderRow>

const _reasons = ["provider", "transport", "shape", "state", "idToken", "lifecycle"] as const

const _faults = {
  provider: { class: "invalid" },
  transport: { class: "unavailable" },
  shape: { class: "invalid" },
  state: { class: "denied" },
  idToken: { class: "denied" },
  lifecycle: { class: "invalid" },
} as const

declare namespace Provider {
  type Kind = keyof typeof _rows
  type _Keys<K extends Kind = (typeof _kinds)[number]> = K
  type _Kinds<K extends (typeof _kinds)[number] = Kind> = K
}

declare namespace OAuthFault {
  type Reason = (typeof _reasons)[number]
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
  type _Closed<K extends Reason = keyof typeof _faults> = K
}

class OAuthFault extends Schema.TaggedError<OAuthFault>()("OAuthFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<oauth:${this.reason}> ${this.detail}`
  }
}

class Departed extends Schema.Class<Departed>("Departed")({
  kind: Schema.Literal(..._kinds),
  verifier: Schema.optionalWith(Schema.String, { as: "Option" }),
  expiresAt: Schema.DateTimeUtc,
}) {}

class OAuthStateStore extends Context.Tag("security/authn/OAuthStateStore")<OAuthStateStore, SingleUse<Departed, OAuthFault>>() {}
```

## [03]-[CEREMONY]

[CEREMONY]:
- Owner: `OAuth.authorize` mints `state`+`verifier`, seals the `Departed` snapshot under the ceremony TTL, and returns the redirect `URL`; `OAuth.callback` consumes the snapshot exactly once, gates kind and expiry, exchanges the code under the resilient leg, verifies the OIDC `id_token`, reads the grant's expiry and scopes, and establishes the session. Dispatch is by `Provider.Kind`; the row's `Config` resolves and the arctic client constructs once per kind under `Effect.cachedFunction`.
- Law: the state is consumed single-use so a replayed or foreign state is `OAuthFault.state`, a stale snapshot is `OAuthFault.state` on the expiry gate, and both land `Reject.mark("state")` on the folder reject stream — the CSRF/replay telemetry of the redirect surface; the verifier is never client-readable.
- Law: `decodeIdToken` is never verification — `Jwt.verify(token, issuer)` pins issuer/audience/algorithms against the row's `oidc`; the throwing `idToken()` read is `Option`-lifted at the seam, so an OIDC row whose exchange returns no `id_token` is `OAuthFault.idToken`, never a defect; a non-OIDC row resolves its subject through the caller's `resolveSubject`, so every path lands a verified `CredentialRef`; `accessTokenExpiresAt`/`scopes` seed the session so the granted scope, not the requested scope, is authoritative.
- Law: every provider leg is internally resilient — the `_leg` seam bounds arctic's fetch with the leg deadline and re-drives only the `transport` arm under a bounded jittered exponential; the classified-terminal `provider` arm never retries.
- Receipt: `URL` on authorize (the edge redirects), `TokenPair` on callback (the edge frames it) — never a raw `OAuth2Tokens`.
- Growth: a new provider is one row; a new claim projection is one `resolveSubject` composition.
- Boundary: `authn/session` `Token.establish` mints the session; `crypt/sign` verifies external tokens; the state store is data/session-satisfied.
- Packages: `arctic` (PKCE mints, the fault family); `crypt/sign` (`Jwt.verify` issuer overload); `crypt/verify` (`Reject`); `authn/session` (`Token.establish`, `CredentialRef`); `effect` (`Schedule`).

```typescript
const _idToken = Option.liftThrowable((tokens: OAuth2Tokens) => tokens.idToken())

const _faultOf: (cause: unknown) => OAuthFault = Match.type<unknown>().pipe(
  Match.when(Match.instanceOf(OAuth2RequestError), (error) => new OAuthFault({ reason: "provider", detail: error.code })),
  Match.when(Match.instanceOf(ArcticFetchError), (error) => new OAuthFault({ reason: "transport", detail: String(error) })),
  Match.when(Match.instanceOf(UnexpectedResponseError), (error) => new OAuthFault({ reason: "shape", detail: String(error.status) })),
  Match.when(Match.instanceOf(UnexpectedErrorResponseBodyError), (error) => new OAuthFault({ reason: "shape", detail: String(error.status) })),
  Match.orElse((error) => new OAuthFault({ reason: "transport", detail: String(error) })),
)

class OAuth extends Effect.Service<OAuth>()("security/authn/OAuth", {
  effect: Effect.gen(function* () {
    const jwt = yield* Jwt
    const token = yield* Token
    const states = yield* OAuthStateStore
    const ceremonyTtl = yield* Config.duration("OAUTH_CEREMONY_TTL").pipe(Config.withDefault(Duration.minutes(10)))
    const legDeadline = yield* Config.duration("OAUTH_LEG_DEADLINE").pipe(Config.withDefault(Duration.seconds(10)))
    const _leg = <A>(run: () => Promise<A>): Effect.Effect<A, OAuthFault> =>
      Effect.tryPromise({ try: run, catch: _faultOf }).pipe(
        Effect.timeoutFail({ duration: legDeadline, onTimeout: () => new OAuthFault({ reason: "transport", detail: "provider deadline" }) }),
        Effect.retry({
          schedule: Schedule.exponential(Duration.millis(200)).pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(2))),
          while: (fault) => fault.reason === "transport",
        }),
      )
    const _bound = yield* Effect.cachedFunction((kind: Provider.Kind) => Effect.orDie(_rows[kind].ceremony))
    const _row = (kind: Provider.Kind): ProviderRow => _rows[kind]
    const authorize = (kind: Provider.Kind): Effect.Effect<URL, OAuthFault> =>
      Effect.gen(function* () {
        const bound = yield* _bound(kind)
        const state = generateState()
        const verifier = _row(kind).pkce ? Option.some(generateCodeVerifier()) : Option.none<string>()
        const now = yield* DateTime.now
        yield* states.stash(state, new Departed({ kind, verifier, expiresAt: DateTime.addDuration(now, ceremonyTtl) }), ceremonyTtl)
        return bound.ceremony.url(state, Option.getOrElse(verifier, () => ""), _row(kind).scopes)
      }).pipe(Effect.withSpan("security.oauth.authorize", { attributes: { kind } }))
    const callback = (kind: Provider.Kind, code: string, presentedState: string, resolveSubject: (tokens: OAuth2Tokens) => Effect.Effect<string, OAuthFault>): Effect.Effect<TokenPair, OAuthFault | SessionFault> =>
      Effect.gen(function* () {
        const departed = yield* Effect.flatMap(states.consume(presentedState), Option.match({
          onNone: () => Effect.fail(new OAuthFault({ reason: "state", detail: "unknown state" })),
          onSome: Effect.succeed,
        }))
        const now = yield* DateTime.now
        yield* Effect.succeed(departed).pipe(
          Effect.filterOrFail((held) => held.kind === kind, () => new OAuthFault({ reason: "state", detail: "provider mismatch" })),
          Effect.filterOrFail((held) => DateTime.lessThanOrEqualTo(now, held.expiresAt), () => new OAuthFault({ reason: "state", detail: "ceremony expired" })),
        )
        const bound = yield* _bound(kind)
        const tokens = yield* _leg(() => bound.ceremony.exchange(code, Option.getOrElse(departed.verifier, () => "")))
        const sub = yield* Option.match(Option.fromNullable(_row(kind).oidc), {
          onSome: (oidc) =>
            Option.match(_idToken(tokens), {
              onNone: () => Effect.fail(new OAuthFault({ reason: "idToken", detail: "id_token absent" })),
              onSome: (raw) =>
                jwt.verify(Redacted.make(raw), { issuer: oidc.issuer, audience: bound.clientId, jwksUri: oidc.jwksUri, algorithms: oidc.algorithms }).pipe(
                  Effect.mapError((fault) => new OAuthFault({ reason: "idToken", detail: fault.detail })),
                  Effect.map((payload) => String(payload.sub)),
                ),
            }),
          onNone: () => resolveSubject(tokens),
        })
        const scopes = tokens.hasScopes() ? tokens.scopes() : _row(kind).scopes
        const ref = new CredentialRef({ kind: "oauth", key: `${kind}:${sub}` })
        return yield* token.establish(ref, scopes, { tenant: Option.none(), verified: true })
      }).pipe(
        Effect.tapError((fault) =>
          fault._tag === "OAuthFault" && fault.reason === "state" ? Reject.mark("state") : Effect.void),
        Effect.withSpan("security.oauth.callback", { attributes: { kind } }),
      )
    const refresh = (kind: Provider.Kind, tokens: OAuth2Tokens): Effect.Effect<{ readonly expiresAt: Option.Option<DateTime.Utc>; readonly scopes: ReadonlyArray<string> }, OAuthFault> =>
      Effect.flatMap(_bound(kind), (bound) => _lifecycle(_row(kind), bound.ceremony, _leg).refresh(tokens))
    const revoke = (kind: Provider.Kind, tokens: OAuth2Tokens): Effect.Effect<void, OAuthFault> =>
      Effect.flatMap(_bound(kind), (bound) => _lifecycle(_row(kind), bound.ceremony, _leg).revoke(tokens))
    return { authorize, callback, refresh, revoke } as const
  }),
  dependencies: [Token.Default],
  accessors: true,
}) {}
```

## [04]-[GRANT_LIFECYCLE]

[GRANT_LIFECYCLE]:
- Owner: `OAuth.refresh` rotates a stored provider refresh grant when the row's `hasRefresh` flag is set, reading the new `accessTokenExpiresAt`/`scopes`; `OAuth.revoke` retires the grant on sign-out when `hasRevoke` is set. Both dispatch by `Provider.Kind` through `_bound`, ride the same resilient `_leg` seam, and route the arctic fault family through the same `_faultOf` triage; `_lifecycle` is the bound-row closure the service delegates to, declared before the service so both legs read one shape.
- Law: the availability flags gate the legs — a provider without a refresh grant (`github`) short-circuits to `OAuthFault.lifecycle` rather than calling an unsupported endpoint, so the unavailable capability is a typed refusal, never a swallowed no-op; `hasRefreshToken` on the tokens gates whether a refresh grant exists to rotate.
- Law: the rotated grant's expiry and scopes are read from the fresh `OAuth2Tokens`, so a provider-side scope reduction propagates and the session re-establishes with the narrowed grant; a grant without an expiry projects `none` — the throwing `accessTokenExpiresAt` read is `Option`-lifted at the seam.
- Receipt: the fresh grant projection (expiry + scopes) the caller re-establishes a session from, or `void` on revoke.
- Growth: a lifecycle capability change is a row flag flip; the legs never change.
- Boundary: the caller composes `refresh` output back into `authn/session`; the provider grant storage is the caller's coordinate; this page owns only the arctic legs.
- Packages: `arctic` (`OAuth2Tokens.refreshToken`/`hasRefreshToken`/`accessTokenExpiresAt`/`scopes`).

`_lifecycle` is declared above the `OAuth` service (the service delegates its `refresh`/`revoke` members to it over a `_bound` row and the shared `_leg` seam):

```typescript
const _expiry = Option.liftThrowable((tokens: OAuth2Tokens) => tokens.accessTokenExpiresAt())

const _lifecycle = (row: ProviderRow, ceremony: Ceremony, leg: <A>(run: () => Promise<A>) => Effect.Effect<A, OAuthFault>) => ({
  refresh: (tokens: OAuth2Tokens): Effect.Effect<{ readonly expiresAt: Option.Option<DateTime.Utc>; readonly scopes: ReadonlyArray<string> }, OAuthFault> =>
    !row.hasRefresh || !tokens.hasRefreshToken()
      ? Effect.fail(new OAuthFault({ reason: "lifecycle", detail: "no refresh grant" }))
      : Effect.map(
          leg(() => ceremony.refresh(tokens.refreshToken(), row.scopes)),
          (fresh) => ({ expiresAt: Option.flatMap(_expiry(fresh), DateTime.make), scopes: fresh.hasScopes() ? fresh.scopes() : row.scopes }),
        ),
  revoke: (tokens: OAuth2Tokens): Effect.Effect<void, OAuthFault> =>
    !row.hasRevoke
      ? Effect.fail(new OAuthFault({ reason: "lifecycle", detail: "no revoke endpoint" }))
      : leg(() => ceremony.revoke(tokens.accessToken())),
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { Departed, OAuth, OAuthFault, OAuthStateStore }
export type { Provider }
```

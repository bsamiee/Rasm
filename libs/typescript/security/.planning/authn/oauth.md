# [SECURITY_OAUTH]

The OAuth 2.0 authorization-code ceremony over `arctic`, modeling every issuer as one vocabulary row rather than a method family: each row bundles the arctic constructor and the four ceremony legs — `url` builds the redirect, `exchange` swaps the code for `OAuth2Tokens`, `refresh` rotates the provider grant, `revoke` retires it — into a uniform `Ceremony` closure, so per-provider PKCE and constructor differences vanish behind one dispatch and a new provider is one row. The roster is the full arctic surface as data: `google`/`github`/`microsoft`/`apple` (whose row carries the `teamId`/`keyId`/`pkcs8` construction the others lack) plus the generic `OAuth2Client` fallback for self-hosted Keycloak/Authentik/Okta issuers — all sharing the ceremony shape, differing only in endpoint identity, PKCE presence, and refresh/revoke availability. Dispatch is a single `Provider.Kind` row lookup, never an `ExecutionPlan` ladder: OAuth has one issuer per request, so failover is not a provision cascade. The full token lifecycle is exploited — `accessTokenExpiresAt`/`hasScopes`/`scopes` seed the session, `hasRefreshToken` gates the refresh-rotation row, and `revokeToken` fires on sign-out. The `state` (CSRF) and PKCE `codeVerifier` are per-request secrets stashed in the `OAuthStateStore` port between the redirect legs, keyed by state and consumed single-use. OIDC `id_token`s verify through `crypt/sign`'s `Jwt.verifyExternal` — arctic's `decodeIdToken` is never trusted — and the verified subject becomes the `CredentialRef` `authn/session` establishes from. The arctic fault family triages by `Match.instanceOf` into `OAuthFault`, and every secret stays `Redacted`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                              | [PUBLIC]                       |
| :-----: | :------------------ | :------------------------------------------------------------------ | :----------------------------- |
|  [01]   | `PROVIDER_ROSTER`   | the closed provider row table, the fault, the state port            | `Provider`, `OAuthFault`, `OAuthStateStore` |
|  [02]   | `CEREMONY`          | authorize + callback + establish, single-dispatch id_token verify   | `OAuth`                        |
|  [03]   | `GRANT_LIFECYCLE`   | refresh rotation and revocation over the row's provider grant        | `OAuth`                        |

## [2]-[PROVIDER_ROSTER]

[PROVIDER_ROSTER]:
- Owner: `Provider` is the closed provider table — each row carries `scopes`, a PKCE flag, an optional OIDC `{ issuer, jwksUri, algorithms }`, `hasRefresh`/`hasRevoke` availability flags, and a `ceremony(creds)` closure that captures the concrete arctic client so all four legs read uniformly; `OAuthFault` is the folded fault; `OAuthStateStore` holds the per-request `state → { verifier, kind }` stash.
- Law: the 60-plus arctic provider classes share the ceremony shape, so a row is data (endpoint identity, PKCE, OIDC, lifecycle availability) over `createAuthorizationURL`/`validateAuthorizationCode`/`refreshAccessToken`/`revokeToken`; the closure erases the per-provider constructor and method arity, and Apple's `teamId`/`keyId`/`pkcs8` construction and the generic `OAuth2Client` self-hosted fallback are rows in the same shape.
- Law: dispatch is a single `Provider.Kind` row lookup — OAuth resolves one issuer per request, so no `ExecutionPlan` failover ladder exists; the credential resolve and client construct memoize once per kind through `Effect.cachedFunction`, so an unconfigured provider costs nothing.
- Law: the fault rows carry the core `FaultClass` kind; the arctic transport fault is `unavailable` (retryable), the provider and shape faults `unavailable`/`invalid`, the state fault `denied`, the id_token fault `denied`.
- Growth: a new provider is one `_rows` entry; a self-hosted issuer reaches the generic `OAuth2Client` in the same row shape; a lifecycle flag flip toggles the refresh/revoke rows without touching the ceremony.
- Boundary: the edge owns the redirect and callback URL params; `authn/session` establishes the session; `crypt/sign` verifies the id_token; `OAuthStateStore` is satisfied by a short-lived data or session band.
- Packages: `arctic` (`OAuth2Client`, `Google`/`GitHub`/`MicrosoftEntraId`/`Apple`, `generateState`/`generateCodeVerifier`, `OAuth2Tokens`, the fault family); `effect` (`Config`, `Context`, `Match`, `Redacted`, `Schema`); `@rasm/ts/core` (`FaultClass`).

```typescript
import {
  Apple, ArcticFetchError, CodeChallengeMethod, generateCodeVerifier, generateState, GitHub, Google, MicrosoftEntraId,
  OAuth2Client, type OAuth2Tokens, OAuth2RequestError, UnexpectedErrorResponseBodyError, UnexpectedResponseError,
} from "arctic"
import { FaultClass } from "@rasm/ts/core"
import { Config, Context, DateTime, Effect, Match, Option, Redacted, Schema } from "effect"
import { Jwt, type KeyAlg } from "../crypt/sign.ts"
import { CredentialRef, type SessionFault, Token, type TokenPair } from "./session.ts"

type Creds = {
  readonly clientId: string
  readonly clientSecret: Redacted.Redacted<string>
  readonly redirectURI: string
  readonly tenant: string
  readonly teamId: string
  readonly keyId: string
  readonly pkcs8: Redacted.Redacted<string>
  readonly issuerUrl: string
}
type Ceremony = {
  readonly url: (state: string, verifier: string, scopes: ReadonlyArray<string>) => URL
  readonly exchange: (code: string, verifier: string) => Promise<OAuth2Tokens>
  readonly refresh: (token: string, scopes: ReadonlyArray<string>) => Promise<OAuth2Tokens>
  readonly revoke: (token: string) => Promise<void>
}
type Oidc = { readonly issuer: string; readonly jwksUri: string; readonly algorithms: ReadonlyArray<KeyAlg.Kind> }
type ProviderRow = {
  readonly scopes: ReadonlyArray<string>
  readonly pkce: boolean
  readonly hasRefresh: boolean
  readonly hasRevoke: boolean
  readonly oidc?: Oidc
  readonly ceremony: (creds: Creds) => Ceremony
}

const _rows = {
  google: {
    scopes: ["openid", "email", "profile"], pkce: true, hasRefresh: true, hasRevoke: true,
    oidc: { issuer: "https://accounts.google.com", jwksUri: "https://www.googleapis.com/oauth2/v3/certs", algorithms: ["RS256"] },
    ceremony: (creds: Creds) => {
      const client = new Google(creds.clientId, Redacted.value(creds.clientSecret), creds.redirectURI)
      return {
        url: (state, verifier, scopes) => client.createAuthorizationURL(state, verifier, scopes),
        exchange: (code, verifier) => client.validateAuthorizationCode(code, verifier),
        refresh: (token, scopes) => client.refreshAccessToken(token, scopes),
        revoke: (token) => client.revokeToken(token),
      }
    },
  },
  github: {
    scopes: ["read:user", "user:email"], pkce: false, hasRefresh: false, hasRevoke: false,
    ceremony: (creds: Creds) => {
      const client = new GitHub(creds.clientId, Redacted.value(creds.clientSecret), creds.redirectURI)
      return {
        url: (state, _verifier, scopes) => client.createAuthorizationURL(state, scopes),
        exchange: (code, _verifier) => client.validateAuthorizationCode(code),
        refresh: () => Promise.reject(new Error("github has no refresh grant")),
        revoke: () => Promise.resolve(),
      }
    },
  },
  microsoft: {
    scopes: ["openid", "email", "profile", "offline_access"], pkce: true, hasRefresh: true, hasRevoke: false,
    oidc: { issuer: "https://login.microsoftonline.com/common/v2.0", jwksUri: "https://login.microsoftonline.com/common/discovery/v2.0/keys", algorithms: ["RS256"] },
    ceremony: (creds: Creds) => {
      const client = new MicrosoftEntraId(creds.tenant, creds.clientId, Redacted.value(creds.clientSecret), creds.redirectURI)
      return {
        url: (state, verifier, scopes) => client.createAuthorizationURL(state, verifier, scopes),
        exchange: (code, verifier) => client.validateAuthorizationCode(code, verifier),
        refresh: (token, scopes) => client.refreshAccessToken(token, scopes),
        revoke: () => Promise.resolve(),
      }
    },
  },
  apple: {
    scopes: ["openid", "email", "name"], pkce: false, hasRefresh: true, hasRevoke: true,
    oidc: { issuer: "https://appleid.apple.com", jwksUri: "https://appleid.apple.com/auth/keys", algorithms: ["ES256"] },
    ceremony: (creds: Creds) => {
      const client = new Apple(creds.clientId, creds.teamId, creds.keyId, new TextEncoder().encode(Redacted.value(creds.pkcs8)), creds.redirectURI)
      return {
        url: (state, _verifier, scopes) => client.createAuthorizationURL(state, scopes),
        exchange: (code, _verifier) => client.validateAuthorizationCode(code),
        refresh: (token, scopes) => client.refreshAccessToken(token, scopes),
        revoke: (token) => client.revokeToken(token),
      }
    },
  },
  generic: {
    scopes: ["openid", "email", "profile"], pkce: true, hasRefresh: true, hasRevoke: true,
    ceremony: (creds: Creds) => {
      const client = new OAuth2Client(creds.clientId, Redacted.value(creds.clientSecret), creds.redirectURI)
      const base = new URL(creds.issuerUrl)
      const authorizeEndpoint = new URL("/authorize", base).toString()
      const tokenEndpoint = new URL("/token", base).toString()
      const revokeEndpoint = new URL("/revoke", base).toString()
      return {
        url: (state, verifier, scopes) => client.createAuthorizationURLWithPKCE(authorizeEndpoint, state, CodeChallengeMethod.S256, verifier, scopes),
        exchange: (code, verifier) => client.validateAuthorizationCode(tokenEndpoint, code, verifier),
        refresh: (token, scopes) => client.refreshAccessToken(tokenEndpoint, token, scopes),
        revoke: (token) => client.revokeToken(revokeEndpoint, token),
      }
    },
  },
} as const satisfies Record<string, ProviderRow>

const _reasons = ["provider", "transport", "shape", "state", "idToken", "lifecycle"] as const

const _faults = {
  provider: { class: "unavailable" },
  transport: { class: "unavailable" },
  shape: { class: "invalid" },
  state: { class: "denied" },
  idToken: { class: "denied" },
  lifecycle: { class: "invalid" },
} as const

declare namespace Provider {
  type Kind = keyof typeof _rows
}

declare namespace OAuthFault {
  type Reason = keyof typeof _faults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
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

class OAuthStateStore extends Context.Tag("security/authn/OAuthStateStore")<OAuthStateStore, {
  readonly stash: (state: string, entry: { readonly verifier: Option.Option<string>; readonly kind: Provider.Kind }) => Effect.Effect<void, OAuthFault>
  readonly consume: (state: string) => Effect.Effect<Option.Option<{ readonly verifier: Option.Option<string>; readonly kind: Provider.Kind }>, OAuthFault>
}>() {}
```

## [3]-[CEREMONY]

[CEREMONY]:
- Owner: `OAuth.authorize` mints `state`+`verifier`, stashes them, and returns the redirect `URL`; `OAuth.callback` consumes the stash, exchanges the code, verifies the OIDC `id_token`, reads the grant's expiry and scopes, and establishes the session. Dispatch is by `Provider.Kind`, the ceremony closure erasing per-provider arity.
- Law: credentials resolve and the arctic client constructs once per kind — `Effect.cachedFunction` memoizes the bound ceremony on first use, so an unconfigured provider costs nothing and no request re-reads `Config` or re-mints a client; the state is consumed single-use so a replayed or foreign state is `OAuthFault.state`; the verifier is never client-readable.
- Law: `decodeIdToken` is never verification — `Jwt.verifyExternal` pins issuer/audience/algorithms against the row's `oidc`; the throwing `idToken()` read is `Option`-lifted at the seam, so an OIDC row whose exchange returns no `id_token` is `OAuthFault.idToken`, never a defect; a non-OIDC row resolves its subject through the caller's `resolveSubject`, so every path lands a verified `CredentialRef`; `accessTokenExpiresAt`/`scopes` seed the session so the granted scope, not the requested scope, is authoritative.
- Receipt: `URL` on authorize (the edge redirects), `TokenPair` on callback (the edge frames it) — never a raw `OAuth2Tokens`.
- Growth: a new provider is one row; a new claim projection is one `resolveSubject` composition.
- Boundary: `authn/session` `Token.establish` mints the session; `crypt/sign` verifies external tokens; the state store is data/session-satisfied.
- Packages: `arctic` (PKCE mints, the fault family); `crypt/sign` (`Jwt.verifyExternal`); `authn/session` (`Token.establish`, `CredentialRef`).

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
    const _credsOf = (kind: Provider.Kind): Effect.Effect<Creds> =>
      Effect.gen(function* () {
        const upper = kind.toUpperCase()
        const clientId = yield* Config.string(`OAUTH_${upper}_CLIENT_ID`)
        const clientSecret = yield* Config.redacted(`OAUTH_${upper}_SECRET`).pipe(Config.withDefault(Redacted.make("")))
        const redirectURI = yield* Config.string(`OAUTH_${upper}_REDIRECT`)
        const tenant = yield* Config.string(`OAUTH_${upper}_TENANT`).pipe(Config.withDefault("common"))
        const teamId = yield* Config.string(`OAUTH_${upper}_TEAM_ID`).pipe(Config.withDefault(""))
        const keyId = yield* Config.string(`OAUTH_${upper}_KEY_ID`).pipe(Config.withDefault(""))
        const pkcs8 = yield* Config.redacted(`OAUTH_${upper}_PKCS8`).pipe(Config.withDefault(Redacted.make("")))
        const issuerUrl = yield* Config.string(`OAUTH_${upper}_ISSUER`).pipe(Config.withDefault(""))
        return { clientId, clientSecret, redirectURI, tenant, teamId, keyId, pkcs8, issuerUrl }
      }).pipe(Effect.orDie)
    const _bound = yield* Effect.cachedFunction((kind: Provider.Kind) =>
      Effect.map(_credsOf(kind), (creds) => {
        const row: ProviderRow = _rows[kind]
        return { row, clientId: creds.clientId, ceremony: row.ceremony(creds) }
      }))
    const authorize = (kind: Provider.Kind): Effect.Effect<URL, OAuthFault> =>
      Effect.gen(function* () {
        const bound = yield* _bound(kind)
        const state = generateState()
        const verifier = bound.row.pkce ? Option.some(generateCodeVerifier()) : Option.none<string>()
        yield* states.stash(state, { verifier, kind })
        return bound.ceremony.url(state, Option.getOrElse(verifier, () => ""), bound.row.scopes)
      })
    const callback = (kind: Provider.Kind, code: string, presentedState: string, resolveSubject: (tokens: OAuth2Tokens) => Effect.Effect<string, OAuthFault>): Effect.Effect<TokenPair, OAuthFault | SessionFault> =>
      Effect.gen(function* () {
        const stashed = yield* Effect.flatMap(states.consume(presentedState), Option.match({ onNone: () => Effect.fail(new OAuthFault({ reason: "state", detail: "unknown state" })), onSome: Effect.succeed }))
        yield* stashed.kind === kind ? Effect.void : Effect.fail(new OAuthFault({ reason: "state", detail: "provider mismatch" }))
        const bound = yield* _bound(kind)
        const tokens = yield* Effect.tryPromise({ try: () => bound.ceremony.exchange(code, Option.getOrElse(stashed.verifier, () => "")), catch: _faultOf })
        const sub = yield* Option.match(Option.fromNullable(bound.row.oidc), {
          onSome: (oidc) =>
            Option.match(_idToken(tokens), {
              onNone: () => Effect.fail(new OAuthFault({ reason: "idToken", detail: "id_token absent" })),
              onSome: (raw) =>
                jwt.verifyExternal(Redacted.make(raw), { issuer: oidc.issuer, audience: bound.clientId, jwksUri: oidc.jwksUri, algorithms: oidc.algorithms }).pipe(
                  Effect.mapError((fault) => new OAuthFault({ reason: "idToken", detail: fault.detail })),
                  Effect.map((payload) => String(payload.sub)),
                ),
            }),
          onNone: () => resolveSubject(tokens),
        })
        const scopes = tokens.hasScopes() ? tokens.scopes() : bound.row.scopes
        const ref = new CredentialRef({ kind: "oauth", key: `${kind}:${sub}` })
        return yield* token.establish(ref, scopes, { tenant: Option.none(), verified: true })
      })
    const refresh = (kind: Provider.Kind, tokens: OAuth2Tokens): Effect.Effect<{ readonly expiresAt: Option.Option<DateTime.Utc>; readonly scopes: ReadonlyArray<string> }, OAuthFault> =>
      Effect.flatMap(_bound(kind), (bound) => _lifecycle(bound.row, bound.ceremony).refresh(tokens))
    const revoke = (kind: Provider.Kind, tokens: OAuth2Tokens): Effect.Effect<void, OAuthFault> =>
      Effect.flatMap(_bound(kind), (bound) => _lifecycle(bound.row, bound.ceremony).revoke(tokens))
    return { authorize, callback, refresh, revoke } as const
  }),
  dependencies: [Token.Default],
  accessors: true,
}) {}
```

## [4]-[GRANT_LIFECYCLE]

[GRANT_LIFECYCLE]:
- Owner: `OAuth.refresh` rotates a stored provider refresh grant when the row's `hasRefresh` flag is set, reading the new `accessTokenExpiresAt`/`scopes`; `OAuth.revoke` retires the grant on sign-out when `hasRevoke` is set. Both dispatch by `Provider.Kind` through `_bound` and route the arctic fault family through the same `_faultOf` triage; `_lifecycle` is the bound-row closure the service delegates to, declared before the service so both legs read one shape.
- Law: the availability flags gate the legs — a provider without a refresh grant (`github`) short-circuits to `OAuthFault.lifecycle` rather than calling an unsupported endpoint, so the unavailable capability is a typed refusal, never a swallowed no-op; `hasRefreshToken` on the tokens gates whether a refresh grant exists to rotate.
- Law: the rotated grant's expiry and scopes are read from the fresh `OAuth2Tokens`, so a provider-side scope reduction propagates and the session re-establishes with the narrowed grant; a grant without an expiry projects `none` — the throwing `accessTokenExpiresAt` read is `Option`-lifted at the seam.
- Receipt: the fresh grant projection (expiry + scopes) the caller re-establishes a session from, or `void` on revoke.
- Growth: a lifecycle capability change is a row flag flip; the legs never change.
- Boundary: the caller composes `refresh` output back into `authn/session`; the provider grant storage is the caller's coordinate; this page owns only the arctic legs.
- Packages: `arctic` (`OAuth2Tokens.refreshToken`/`hasRefreshToken`/`accessTokenExpiresAt`/`scopes`).

The `_lifecycle` closure is declared above the `OAuth` service (the service delegates its `refresh`/`revoke` members to it over a `_bound` row):

```typescript
const _expiry = Option.liftThrowable((tokens: OAuth2Tokens) => tokens.accessTokenExpiresAt())

const _lifecycle = (row: ProviderRow, ceremony: Ceremony) => ({
  refresh: (tokens: OAuth2Tokens): Effect.Effect<{ readonly expiresAt: Option.Option<DateTime.Utc>; readonly scopes: ReadonlyArray<string> }, OAuthFault> =>
    !row.hasRefresh || !tokens.hasRefreshToken()
      ? Effect.fail(new OAuthFault({ reason: "lifecycle", detail: "no refresh grant" }))
      : Effect.map(
          Effect.tryPromise({ try: () => ceremony.refresh(tokens.refreshToken(), row.scopes), catch: _faultOf }),
          (fresh) => ({ expiresAt: Option.flatMap(_expiry(fresh), DateTime.make), scopes: fresh.hasScopes() ? fresh.scopes() : row.scopes }),
        ),
  revoke: (tokens: OAuth2Tokens): Effect.Effect<void, OAuthFault> =>
    !row.hasRevoke
      ? Effect.fail(new OAuthFault({ reason: "lifecycle", detail: "no revoke endpoint" }))
      : Effect.tryPromise({ try: () => ceremony.revoke(tokens.accessToken()), catch: _faultOf }),
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { OAuth, OAuthFault, OAuthStateStore }
export type { Provider }
```

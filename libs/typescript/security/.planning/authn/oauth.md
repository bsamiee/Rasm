# [SECURITY_OAUTH]

OAuth 2.0 authorization-code ceremony over `arctic`, modeling every issuer as one vocabulary row rather than a method family: each row bundles its own `Config` requirement and the four ceremony legs — `url` builds the redirect, `exchange` swaps the code for `OAuth2Tokens`, `refresh` rotates the provider grant, `revoke` retires it — into a uniform `Ceremony` closure, so per-provider PKCE, constructor arity, and credential shape all vanish behind one dispatch and a new provider is one row carrying exactly the config it needs — the wide all-providers credential bag is dead. Its roster is the full arctic surface as data: `google`/`github`/`microsoft`/`apple` (whose row alone demands `teamId`/`keyId`/`pkcs8`) and the generic `OAuth2Client` row for self-hosted Keycloak/Authentik/Okta issuers, which resolves its endpoints, JWKS URI, signing roster, and revocation capability from the issuer's own published metadata rather than concatenating paths onto the issuer root — all sharing the ceremony shape, anchored by the `_kinds` tuple with the guard pair closed in both directions. This two-leg ceremony is durable data, not an ad-hoc stash: `authorize` seals a `Departed` snapshot — kind, PKCE verifier, expiry — into the `OAuthStateStore` single-use port under a TTL, the browser redirect crosses any process boundary, and `callback` consumes the snapshot exactly once, type-witnessing the leg order; a replayed, foreign, expired, or provider-mismatched state is `OAuthFault.state` and lands the `state` row on the folder ledger, beside the admission and ceremony span an accepted callback lands under the same kind. Every fetch leg is internally resilient — a deadline bounds arctic's own `fetch`, and the `transport` arm (`ArcticFetchError`, the only `unavailable`-classed row) re-drives under the branch retry budget's own class gate while `OAuth2RequestError` stays terminal. OIDC `id_token`s verify through `crypt/sign`'s issuer-overloaded `Jwt.verify` — arctic's `decodeIdToken` is never trusted — and the verified subject becomes the `CredentialRef` `authn/session` establishes from. Arctic's fault family triages by `Match.instanceOf` into `OAuthFault`, and every secret stays `Redacted`.

## [01]-[INDEX]

- [02]-[PROVIDER_ROSTER]: `Provider`, `OAuthFault`, `Departed`, `OAuthStateStore`.
- [03]-[CEREMONY]: `OAuth`.
- [04]-[GRANT_LIFECYCLE]: `OAuth`.

## [02]-[PROVIDER_ROSTER]

[PROVIDER_ROSTER]:
- Owner: `Provider` is the closed provider table — each row carries `scopes`, a PKCE flag, and one `bind` effect resolving exactly the credential fields the provider demands into a `Bound`: the client id, the uniform four-leg arctic closure, the OIDC descriptor as an `Option`, and the refresh/revoke capability flags; `OAuthFault` is the folded fault; `Departed` is the serializable ceremony snapshot; `OAuthStateStore` is the `SingleUse<Departed>` port.
- Law: the 60-plus arctic provider classes share the ceremony shape, so a row is data over `createAuthorizationURL`/`validateAuthorizationCode`/`refreshAccessToken`/`revokeToken`; the row's `Config` is its whole credential contract — Apple's `teamId`/`keyId`/`pkcs8`, Microsoft's `tenant`, the generic row's `issuer` URL are per-row fields, so no provider reads another's knobs; the `OAUTH_PROVIDERS` roster decides which rows resolve, every enabled bag resolves at the service boot line, a kind outside the roster refuses `provider` typed, and only the generic row's discovery leg stays a runtime read.
- Law: `Bound` holds every RESOLVED fact and the row holds only what the deployment declares, because an issuer's endpoints, JWKS URI, signing roster, and revocation capability are readable and a hand-asserted copy of a readable value drifts the first time the issuer moves. A pinned row states them as literals under its own vendor contract; the generic row reads them once at bind through `discovery`'s `serverMetadata()` — self-hosted Keycloak, Authentik, and Okta each serve the authorization, token, and revocation legs off unrelated paths, so `new URL("/authorize", issuer)` is wrong for all three and fails as a 404 at ceremony time rather than at bind. A missing authorization or token endpoint is `provider` at bind, an absent `revocation_endpoint` lowers `hasRevoke` to the typed lifecycle refusal instead of dialing a URL the issuer never published, and the signing roster is the advertised set intersected with the folder's own `KeyAlg` table so an unverifiable issuer refuses before its first id_token.
- Law: `Departed` is a `Schema.Class` so the ceremony snapshot is wire-serializable — the state store persists it across the redirect and any process restart, the single-use consume is the transition witness, and the TTL is stamped as `expiresAt` data checked on land; the satisfying layer is a `Cache`/`PersistedCache` row over the `SingleUse` contract, never a hand-rolled map.
- Growth: a new provider is one row and its `_kinds` entry; a self-hosted issuer reaches the generic row with zero endpoint knowledge; a multi-leg enrollment ceremony (device onboarding across restarts) is an `@effect/experimental` `Machine.makeSerializable` actor whose snapshot rides the same store.
- Boundary: the edge owns the redirect and callback URL params; `authn/session` establishes the session; `crypt/sign` verifies the id_token; `OAuthStateStore` is satisfied by a short-lived data or session band; `openid-client` appears here as the metadata reader alone — arctic still runs all four ceremony legs, so the browser-ceremony custody split holds untouched.

```typescript
import {
  Apple, ArcticFetchError, CodeChallengeMethod, generateCodeVerifier, generateState, GitHub, Google, MicrosoftEntraId,
  OAuth2Client, type OAuth2Tokens, OAuth2RequestError, UnexpectedErrorResponseBodyError, UnexpectedResponseError,
} from "arctic"
import { discovery, ResponseBodyError } from "openid-client"
import { Fault } from "@rasm/ts/core"
import { Array, Config, Context, DateTime, Duration, Effect, Match, Option, Redacted, Schema, Struct } from "effect"
import { Jwt, KeyAlg, type SingleUse } from "../crypt/sign.ts"
import { Reject } from "../crypt/verify.ts"
import { CredentialRef, type SessionFault, Token, type TokenPair } from "./session.ts"

const _kinds = ["google", "github", "microsoft", "apple", "generic"] as const

type Ceremony = {
  readonly url: (state: string, verifier: string, scopes: ReadonlyArray<string>) => URL
  readonly exchange: (code: string, verifier: string) => Promise<OAuth2Tokens>
  readonly refresh: (token: string, scopes: ReadonlyArray<string>) => Promise<OAuth2Tokens>
  readonly revoke: (token: string) => Promise<void>
}
type Oidc = { readonly issuer: string; readonly jwksUri: string; readonly algorithms: ReadonlyArray<KeyAlg.Kind> }
// Everything a bind RESOLVES rides here, so a pinned row and a discovered one answer one shape: the OIDC descriptor
// and the lifecycle capability are facts about the issuer, and an issuer that publishes them is read, never asserted.
type Bound = {
  readonly clientId: string
  readonly ceremony: Ceremony
  readonly oidc: Option.Option<Oidc>
  readonly hasRefresh: boolean
  readonly hasRevoke: boolean
}
// `bind` is a Config OF an effect: the credential bag resolves at the service boot line for every enabled kind,
// while the effect inside runs at first ceremony — so only a genuinely runtime leg (discovery) stays runtime.
type ProviderRow = {
  readonly scopes: ReadonlyArray<string>
  readonly pkce: boolean
  readonly bind: Config.Config<Effect.Effect<Bound, OAuthFault>>
}

const _family = Fault.Class.family(["provider", "transport", "shape", "state", "idToken", "lifecycle"] as const, {
  provider: { class: "invalid" },
  transport: { class: "unavailable" },
  shape: { class: "invalid" },
  state: { class: "denied" },
  idToken: { class: "denied" },
  lifecycle: { class: "invalid" },
})

declare namespace OAuthFault {
  type Reason = (typeof _family.reasons)[number]
}

class OAuthFault extends Schema.TaggedError<OAuthFault>()("OAuthFault", {
  reason: _family.schema,
  detail: Schema.String,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<oauth:${this.reason}> ${this.detail}`
  }
}

const _cfg = (name: string) => ({
  clientId: Config.string(`OAUTH_${name}_CLIENT_ID`),
  redirect: Config.string(`OAUTH_${name}_REDIRECT`),
})

const _bound = <A>(config: Config.Config<A>, make: (values: A) => Bound): Config.Config<Effect.Effect<Bound, OAuthFault>> =>
  Config.map(config, (values) => Effect.sync(() => make(values)))

const _metadataFault = (cause: unknown): OAuthFault =>
  cause instanceof ResponseBodyError
    ? new OAuthFault({ reason: "provider", detail: cause.error })
    : new OAuthFault({ reason: "transport", detail: String(cause) })

const _rows = {
  google: {
    scopes: ["openid", "email", "profile"], pkce: true,
    bind: _bound(
      Config.all({ ..._cfg("GOOGLE"), secret: Config.redacted("OAUTH_GOOGLE_SECRET") }),
      ({ clientId, redirect, secret }) => {
        const client = new Google(clientId, Redacted.value(secret), redirect)
        return {
          clientId, hasRefresh: true, hasRevoke: true,
          oidc: Option.some({ issuer: "https://accounts.google.com", jwksUri: "https://www.googleapis.com/oauth2/v3/certs", algorithms: ["RS256"] }),
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
    scopes: ["read:user", "user:email"], pkce: false,
    bind: _bound(
      Config.all({ ..._cfg("GITHUB"), secret: Config.redacted("OAUTH_GITHUB_SECRET") }),
      ({ clientId, redirect, secret }) => {
        const client = new GitHub(clientId, Redacted.value(secret), redirect)
        return {
          clientId, hasRefresh: false, hasRevoke: false, oidc: Option.none(),
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
    scopes: ["openid", "email", "profile", "offline_access"], pkce: true,
    bind: _bound(
      Config.all({ ..._cfg("MICROSOFT"), secret: Config.redacted("OAUTH_MICROSOFT_SECRET"), tenant: Config.string("OAUTH_MICROSOFT_TENANT").pipe(Config.withDefault("common")) }),
      ({ clientId, redirect, secret, tenant }) => {
        const client = new MicrosoftEntraId(tenant, clientId, Redacted.value(secret), redirect)
        return {
          clientId, hasRefresh: true, hasRevoke: false,
          oidc: Option.some({
            issuer: "https://login.microsoftonline.com/common/v2.0",
            jwksUri: "https://login.microsoftonline.com/common/discovery/v2.0/keys",
            algorithms: ["RS256"],
          }),
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
    scopes: ["openid", "email", "name"], pkce: false,
    bind: _bound(
      Config.all({
        ..._cfg("APPLE"),
        teamId: Config.string("OAUTH_APPLE_TEAM_ID"),
        keyId: Config.string("OAUTH_APPLE_KEY_ID"),
        pkcs8: Config.redacted("OAUTH_APPLE_PKCS8"),
      }),
      ({ clientId, keyId, pkcs8, redirect, teamId }) => {
        const client = new Apple(clientId, teamId, keyId, new TextEncoder().encode(Redacted.value(pkcs8)), redirect)
        return {
          clientId, hasRefresh: true, hasRevoke: true,
          oidc: Option.some({ issuer: "https://appleid.apple.com", jwksUri: "https://appleid.apple.com/auth/keys", algorithms: ["ES256"] }),
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
    scopes: ["openid", "email", "profile"], pkce: true,
    bind: Config.map(
      Config.all({ ..._cfg("GENERIC"), secret: Config.redacted("OAUTH_GENERIC_SECRET"), issuer: Config.string("OAUTH_GENERIC_ISSUER") }),
      ({ clientId, issuer, redirect, secret }) =>
        Effect.gen(function* () {
          // A self-hosted issuer publishes its own endpoints: Keycloak, Authentik, and Okta each serve the code, token,
          // and revocation legs off unrelated paths, so concatenating `/authorize` onto the issuer root works for none
          // of them and fails as a 404 at ceremony time rather than at boot. Every endpoint, the JWKS URI, the signing
          // roster, and the revocation capability are metadata reads.
          const config = yield* Effect.tryPromise({ try: () => discovery(new URL(issuer), clientId, Redacted.value(secret)), catch: _metadataFault })
          const metadata = config.serverMetadata()
          const client = new OAuth2Client(clientId, Redacted.value(secret), redirect)
          const _endpoint = (value: string | undefined, name: string): Effect.Effect<string, OAuthFault> =>
            Option.match(Option.fromNullable(value), {
              onNone: () => Effect.fail(new OAuthFault({ reason: "provider", detail: `issuer publishes no ${name}` })),
              onSome: Effect.succeed,
            })
          const authorizeEndpoint = yield* _endpoint(metadata.authorization_endpoint, "authorization_endpoint")
          const tokenEndpoint = yield* _endpoint(metadata.token_endpoint, "token_endpoint")
          const revokeEndpoint = Option.fromNullable(metadata.revocation_endpoint)
          // The signing roster is the issuer's advertisement intersected with what this folder can verify, so an issuer
          // offering only algorithms the `KeyAlg` table lacks refuses at bind instead of at first id_token.
          const algorithms = Array.filter(Struct.keys(KeyAlg), (alg) =>
            Option.match(Option.fromNullable(metadata.id_token_signing_alg_values_supported), {
              onNone: () => alg === "RS256",
              onSome: (advertised) => Array.contains(advertised, alg),
            }))
          const oidc = yield* Option.match(Option.fromNullable(metadata.jwks_uri), {
            onNone: () => Effect.succeedNone,
            onSome: (jwksUri) =>
              Array.isNonEmptyReadonlyArray(algorithms)
                ? Effect.succeedSome({ issuer: metadata.issuer, jwksUri, algorithms })
                : Effect.fail(new OAuthFault({ reason: "provider", detail: "no shared id_token signing algorithm" })),
          })
          return {
            clientId, oidc, hasRefresh: true, hasRevoke: Option.isSome(revokeEndpoint),
            ceremony: {
              url: (state, verifier, scopes) => client.createAuthorizationURLWithPKCE(authorizeEndpoint, state, CodeChallengeMethod.S256, verifier, scopes),
              exchange: (code, verifier) => client.validateAuthorizationCode(tokenEndpoint, code, verifier),
              refresh: (token, scopes) => client.refreshAccessToken(tokenEndpoint, token, scopes),
              revoke: (token) =>
                Option.match(revokeEndpoint, {
                  onNone: () => Promise.reject(new Error("issuer publishes no revocation_endpoint")),
                  onSome: (endpoint) => client.revokeToken(endpoint, token),
                }),
            },
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
  expiresAt: Schema.DateTimeUtc,
}) {}

class OAuthStateStore extends Context.Tag("security/authn/OAuthStateStore")<OAuthStateStore, SingleUse<Departed, OAuthFault>>() {}
```

## [03]-[CEREMONY]

[CEREMONY]:
- Owner: `OAuth.authorize` mints `state`+`verifier`, seals the `Departed` snapshot under the ceremony TTL, and returns the redirect `URL`; `OAuth.callback` consumes the snapshot exactly once, gates kind and expiry, exchanges the code under the resilient leg, verifies the OIDC `id_token`, reads the grant's expiry and scopes, and establishes the session. Dispatch is by `Provider.Kind`; every enabled row's `Config` resolves at the service head and the bound client constructs once per kind under `Effect.cachedFunction`.
- Law: the state is consumed single-use so a replayed or foreign state is `OAuthFault.state`, a stale snapshot is `OAuthFault.state` on the expiry gate, and both land `Reject.mark("state")` while a completed callback lands its `state`-kinded admission and ceremony span through `Reject.measured` — the redirect surface's replay rate reads against its own completion rate; the verifier is never client-readable.
- Law: `decodeIdToken` is never verification — `Jwt.verify(token, issuer)` pins issuer/audience/algorithms against the row's `oidc`; the throwing `idToken()` read is `Option`-lifted at the seam, so an OIDC row whose exchange returns no `id_token` is `OAuthFault.idToken`, never a defect; a non-OIDC row resolves its subject through the caller's `resolveSubject`, so every path lands a verified `CredentialRef`; `accessTokenExpiresAt`/`scopes` seed the session so the granted scope, not the requested scope, is authoritative.
- Receipt: `URL` on authorize (the edge redirects), `TokenPair` on callback (the edge frames it) — never a raw `OAuth2Tokens`.
- Growth: a new provider is one row; a new claim projection is one `resolveSubject` composition.
- Boundary: `authn/session` `Token.establish` mints the session; `crypt/sign` verifies external tokens; the state store is data/session-satisfied.

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
    const ceremonyTtl = yield* Config.duration("OAUTH_CEREMONY_TTL").pipe(
      Config.withDefault(Duration.minutes(10)),
      Config.withDescription("Departed snapshot lease bounding the redirect round trip"),
    )
    const legDeadline = yield* Config.duration("OAUTH_LEG_DEADLINE").pipe(
      Config.withDefault(Duration.seconds(10)),
      Config.withDescription("per-leg provider deadline before the transport retry re-drives"),
    )
    const enabled = yield* Config.array(Config.literal(..._kinds)(), "OAUTH_PROVIDERS").pipe(
      Config.withDefault([]),
      Config.withDescription("enabled provider kinds; each named row's credential bag resolves at this boot line"),
    )
    // every enabled bag resolves HERE, so a missing credential fails the root proof, not the first user's redirect
    const binds = new Map(Array.zip(enabled, yield* Config.all(Array.map(enabled, (kind) => _rows[kind].bind))))
    const _leg = <A>(run: () => Promise<A>): Effect.Effect<A, OAuthFault> =>
      Effect.tryPromise({ try: run, catch: _faultOf }).pipe(
        Effect.timeoutFail({ duration: legDeadline, onTimeout: () => new OAuthFault({ reason: "transport", detail: "provider deadline" }) }),
        // `transport` is the family's one `unavailable` row, so the branch compile's own class gate re-drives exactly
        // the arm the hand predicate named — and brings the quiet-reset and elapsed ceiling that predicate dropped.
        Effect.retry(Fault.Budget.schedule("pulse")),
      )
    const _binding = yield* Effect.cachedFunction((kind: Provider.Kind) =>
      Option.match(Option.fromNullable(binds.get(kind)), {
        onNone: () => Effect.fail(new OAuthFault({ reason: "provider", detail: `${kind} not enabled` })),
        onSome: (bind) => bind,
      }))
    const _row = (kind: Provider.Kind): ProviderRow => _rows[kind]
    const authorize = (kind: Provider.Kind): Effect.Effect<URL, OAuthFault> =>
      Effect.gen(function* () {
        const bound = yield* _binding(kind)
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
        const bound = yield* _binding(kind)
        const tokens = yield* _leg(() => bound.ceremony.exchange(code, Option.getOrElse(departed.verifier, () => "")))
        const sub = yield* Option.match(bound.oidc, {
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
        Effect.tapErrorTag("OAuthFault", (fault) => (fault.reason === "state" ? Reject.mark("state") : Effect.void)),
        Reject.measured("state"),
        Effect.withSpan("security.oauth.callback", { attributes: { kind } }),
      )
    const refresh = (kind: Provider.Kind, tokens: OAuth2Tokens): Effect.Effect<{ readonly expiresAt: Option.Option<DateTime.Utc>; readonly scopes: ReadonlyArray<string> }, OAuthFault> =>
      Effect.flatMap(_binding(kind), (bound) => _lifecycle(bound, _row(kind).scopes, _leg).refresh(tokens))
    const revoke = (kind: Provider.Kind, tokens: OAuth2Tokens): Effect.Effect<void, OAuthFault> =>
      Effect.flatMap(_binding(kind), (bound) => _lifecycle(bound, _row(kind).scopes, _leg).revoke(tokens))
    return { authorize, callback, refresh, revoke } as const
  }),
  dependencies: [Token.Default],
  accessors: true,
}) {}
```

## [04]-[GRANT_LIFECYCLE]

[GRANT_LIFECYCLE]:
- Owner: `OAuth.refresh` rotates a stored provider refresh grant when the bound issuer carries one, reading the new `accessTokenExpiresAt`/`scopes`; `OAuth.revoke` retires the grant on sign-out when the issuer publishes a revocation leg. Both dispatch by `Provider.Kind` through `_binding`, ride the same resilient `_leg` seam, and route the arctic fault family through the same `_faultOf` triage; `_lifecycle` is the bound closure the service delegates to, declared before the service so both legs read one shape.
- Law: the capability flags gate the legs and they are `Bound` facts, not row assertions — a provider without a refresh grant (`github`) or an issuer that published no `revocation_endpoint` short-circuits to `OAuthFault.lifecycle` rather than calling an endpoint that does not exist, so the unavailable capability is a typed refusal, never a swallowed no-op; `hasRefreshToken` on the tokens gates whether a refresh grant exists to rotate.
- Law: the rotated grant's expiry and scopes are read from the fresh `OAuth2Tokens`, so a provider-side scope reduction propagates and the session re-establishes with the narrowed grant; a grant without an expiry projects `none` — the throwing `accessTokenExpiresAt` read is `Option`-lifted at the seam.
- Receipt: the fresh grant projection (expiry + scopes) the caller re-establishes a session from, or `void` on revoke.
- Growth: a lifecycle capability change is a `Bound` column — a flag flip on a pinned row, a metadata read on a discovered one; the legs never change.
- Boundary: the caller composes `refresh` output back into `authn/session`; the provider grant storage is the caller's coordinate; this page owns only the arctic legs.
- Packages: `arctic` (`OAuth2Tokens.refreshToken`/`hasRefreshToken`/`accessTokenExpiresAt`/`scopes`).

`_lifecycle` is declared above the `OAuth` service (the service delegates its `refresh`/`revoke` members to it over a resolved `Bound` and the shared `_leg` seam):

```typescript
const _expiry = Option.liftThrowable((tokens: OAuth2Tokens) => tokens.accessTokenExpiresAt())

const _lifecycle = (bound: Bound, scopes: ReadonlyArray<string>, leg: <A>(run: () => Promise<A>) => Effect.Effect<A, OAuthFault>) => ({
  refresh: (tokens: OAuth2Tokens): Effect.Effect<{ readonly expiresAt: Option.Option<DateTime.Utc>; readonly scopes: ReadonlyArray<string> }, OAuthFault> =>
    !bound.hasRefresh || !tokens.hasRefreshToken()
      ? Effect.fail(new OAuthFault({ reason: "lifecycle", detail: "no refresh grant" }))
      : Effect.map(
          leg(() => bound.ceremony.refresh(tokens.refreshToken(), scopes)),
          (fresh) => ({ expiresAt: Option.flatMap(_expiry(fresh), DateTime.make), scopes: fresh.hasScopes() ? fresh.scopes() : scopes }),
        ),
  revoke: (tokens: OAuth2Tokens): Effect.Effect<void, OAuthFault> =>
    !bound.hasRevoke
      ? Effect.fail(new OAuthFault({ reason: "lifecycle", detail: "no revoke endpoint" }))
      : leg(() => bound.ceremony.revoke(tokens.accessToken())),
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { Departed, OAuth, OAuthFault, OAuthStateStore }
export type { Provider }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

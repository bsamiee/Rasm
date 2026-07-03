# [SECURITY_OAUTH] — the arctic authorization-code + PKCE ceremony as a uniform provider vocabulary

`authn/oauth` runs the OAuth 2.0 authorization-code ceremony over `arctic`, modeling a provider as one vocabulary row rather than a method family per issuer: each row bundles the arctic constructor and the two ceremony legs into a uniform `Ceremony` closure — `url` builds the redirect, `exchange` swaps the code for `OAuth2Tokens` — so the provider-specific PKCE and constructor differences vanish behind one dispatch and a new provider is one row. The `state` (CSRF) and PKCE `codeVerifier` are per-request secrets stashed in the `OAuthStateStore` port between the two redirect legs, keyed by state and consumed single-use with a constant-time check. OIDC `id_token`s are verified by `sign/jwt` — arctic's `decodeIdToken` is never trusted — and the verified `sub` (or a provider profile the caller resolves for non-OIDC rows) becomes the `CredentialRef` `session/token` establishes a session from. The whole arctic fault family folds by `Match.instanceOf` triage into `OAuthFault`, and every secret stays `Redacted`.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                       | [OWNER]                             | [PACKAGES]                          | [REJECTED_FORM]                              |
| :-----: | :------------------------------ | :--------------------------------- | :---------------------------------- | :------------------------------------------- |
|  [01]   | provider vocabulary + fault     | `Provider` ceremony rows / `OAuthFault` | `arctic`                        | a `get*`/`refresh*` method family per issuer |
|  [02]   | state boundary                  | `OAuthStateStore` port             | `effect` `Context.Tag`              | a client-readable state cookie, a reused verifier |
|  [03]   | authorize + callback + establish| `OAuth.authorize` / `OAuth.callback`| `arctic`, `sign/jwt`, `session/token` | `decodeIdToken` as verification, a hand URL  |

## [2]-[PROVIDER_VOCABULARY]

[ROWS_AND_FAULT]:
- Owner: `Provider` is the closed provider table — each row carries `scopes`, a PKCE flag, an optional OIDC `{ issuer, jwksUri, algorithms }`, and a `ceremony(creds)` closure that captures the concrete arctic client so the two legs read uniformly; `OAuthFault` is the folded fault. `OAuthStateStore` holds the per-request `state`→`{ verifier, kind }` stash.
- Packages: `arctic` — the 60-plus provider classes share the ceremony shape, so a row is data (endpoint identity, PKCE, OIDC) over `createAuthorizationURL`/`validateAuthorizationCode`; the closure erases the per-provider constructor and method arity.
- Boundary: the edge owns the redirect and the callback URL params; `session/token` establishes the session; `sign/jwt` verifies the id_token; `OAuthStateStore` is satisfied by a short-lived `store`/`session` band.
- Growth: a new provider is one `_rows` entry; a self-hosted issuer reaches the generic `OAuth2Client` in the same row shape.

```typescript
import { generateCodeVerifier, generateState, GitHub, Google, MicrosoftEntraId, type OAuth2Tokens, ArcticFetchError, OAuth2RequestError, UnexpectedErrorResponseBodyError, UnexpectedResponseError } from "arctic"
import { Config, Context, Effect, Match, Option, Redacted, Schema } from "effect"
import { Jwt } from "../sign/jwt.ts"
import { CredentialRef, type SessionFault, Token, type TokenPair } from "../session/token.ts"

// --- [TYPES] ----------------------------------------------------------------------------

type Creds = { readonly clientId: string; readonly clientSecret: string; readonly redirectURI: string; readonly tenant: string }
type Ceremony = { readonly url: (state: string, verifier: string, scopes: ReadonlyArray<string>) => URL; readonly exchange: (code: string, verifier: string) => Promise<OAuth2Tokens> }
type Oidc = { readonly issuer: string; readonly jwksUri: string; readonly algorithms: ReadonlyArray<"ES256" | "ES384" | "RS256" | "EdDSA"> }
type ProviderRow = { readonly scopes: ReadonlyArray<string>; readonly pkce: boolean; readonly oidc?: Oidc; readonly ceremony: (creds: Creds) => Ceremony }

// --- [CONSTANTS] ------------------------------------------------------------------------

const _rows = {
  google: {
    scopes: ["openid", "email", "profile"], pkce: true,
    oidc: { issuer: "https://accounts.google.com", jwksUri: "https://www.googleapis.com/oauth2/v3/certs", algorithms: ["RS256"] },
    ceremony: (creds) => {
      const client = new Google(creds.clientId, creds.clientSecret, creds.redirectURI)
      return { url: (state, verifier, scopes) => client.createAuthorizationURL(state, verifier, scopes), exchange: (code, verifier) => client.validateAuthorizationCode(code, verifier) }
    },
  },
  github: {
    scopes: ["read:user", "user:email"], pkce: false,
    ceremony: (creds) => {
      const client = new GitHub(creds.clientId, creds.clientSecret, creds.redirectURI)
      return { url: (state, _verifier, scopes) => client.createAuthorizationURL(state, scopes), exchange: (code, _verifier) => client.validateAuthorizationCode(code) }
    },
  },
  microsoft: {
    scopes: ["openid", "email", "profile"], pkce: true,
    oidc: { issuer: "https://login.microsoftonline.com/common/v2.0", jwksUri: "https://login.microsoftonline.com/common/discovery/v2.0/keys", algorithms: ["RS256"] },
    ceremony: (creds) => {
      const client = new MicrosoftEntraId(creds.tenant, creds.clientId, creds.clientSecret, creds.redirectURI)
      return { url: (state, verifier, scopes) => client.createAuthorizationURL(state, verifier, scopes), exchange: (code, verifier) => client.validateAuthorizationCode(code, verifier) }
    },
  },
} as const satisfies Record<string, ProviderRow>

const _reasons = ["provider", "transport", "shape", "state", "idToken"] as const

const OAuthFaultPolicy = {
  provider: { rank: 4, retry: false, status: 502 },
  transport: { rank: 2, retry: true, status: 503 },
  shape: { rank: 3, retry: false, status: 502 },
  state: { rank: 5, retry: false, status: 403 },
  idToken: { rank: 4, retry: false, status: 401 },
} as const

declare namespace Provider {
  type Kind = keyof typeof _rows
}

declare namespace OAuthFault {
  type Reason = keyof typeof OAuthFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof OAuthFaultPolicy> = T
}

// --- [ERRORS] ---------------------------------------------------------------------------

class OAuthFault extends Schema.TaggedError<OAuthFault>()("OAuthFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): OAuthFault.Row {
    return OAuthFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<oauth:${this.reason}> ${this.detail}`
  }
}

// --- [SERVICES] -------------------------------------------------------------------------

class OAuthStateStore extends Context.Tag("security/authn/OAuthStateStore")<OAuthStateStore, {
  readonly stash: (state: string, entry: { readonly verifier: Option.Option<string>; readonly kind: Provider.Kind }) => Effect.Effect<void, OAuthFault>
  readonly consume: (state: string) => Effect.Effect<Option.Option<{ readonly verifier: Option.Option<string>; readonly kind: Provider.Kind }>, OAuthFault>
}>() {}
```

## [3]-[CEREMONY]

[OAUTH]:
- Owner: `OAuth.authorize` mints `state`+`verifier`, stashes them, and returns the redirect `URL`; `OAuth.callback` consumes the stash, exchanges the code, verifies the OIDC `id_token`, and establishes the session. Dispatch is by `Provider.Kind`, the ceremony closure erasing per-provider arity.
- Packages: `arctic` `generateState`/`generateCodeVerifier` + the row `ceremony`; `sign/jwt` `Jwt.verifyExternal` for the id_token; `session/token` `Token.establish` for the session; the arctic fault family triages by `Match.instanceOf`.
- Law: the state is compared single-use so a replayed or foreign state is `OAuthFault.state`; the verifier is never client-readable; `decodeIdToken` is never verification — `Jwt.verifyExternal` pins issuer/audience/algorithms; a non-OIDC row resolves its subject through the caller's `resolveSubject`, so every path lands a verified `CredentialRef`.
- Receipt: `URL` on authorize (the edge redirects), `TokenPair` on callback (the edge frames it) — never a raw `OAuth2Tokens`.

```typescript
// --- [OPERATIONS] -----------------------------------------------------------------------

const _faultOf: (cause: unknown) => OAuthFault = Match.type<unknown>().pipe(
  Match.when(Match.instanceOf(OAuth2RequestError), (error) => new OAuthFault({ reason: "provider", detail: error.code })),
  Match.when(Match.instanceOf(ArcticFetchError), (error) => new OAuthFault({ reason: "transport", detail: String(error) })),
  Match.when(Match.instanceOf(UnexpectedResponseError), (error) => new OAuthFault({ reason: "shape", detail: String(error.status) })),
  Match.when(Match.instanceOf(UnexpectedErrorResponseBodyError), (error) => new OAuthFault({ reason: "shape", detail: String(error.status) })),
  Match.orElse((error) => new OAuthFault({ reason: "transport", detail: String(error) })),
)

// --- [SERVICES] -------------------------------------------------------------------------

class OAuth extends Effect.Service<OAuth>()("security/authn/OAuth", {
  effect: Effect.gen(function* () {
    const jwt = yield* Jwt
    const token = yield* Token
    const states = yield* OAuthStateStore
    const _creds = (kind: Provider.Kind): Effect.Effect<Creds> =>
      Effect.gen(function* () {
        const upper = kind.toUpperCase()
        const clientId = yield* Config.string(`OAUTH_${upper}_CLIENT_ID`)
        const clientSecret = yield* Config.redacted(`OAUTH_${upper}_SECRET`)
        const redirectURI = yield* Config.string(`OAUTH_${upper}_REDIRECT`)
        const tenant = yield* Config.string(`OAUTH_${upper}_TENANT`).pipe(Config.withDefault("common"))
        return { clientId, clientSecret: Redacted.value(clientSecret), redirectURI, tenant }
      }).pipe(Effect.orDie)
    const authorize = (kind: Provider.Kind): Effect.Effect<URL, OAuthFault> =>
      Effect.gen(function* () {
        const row = _rows[kind]
        const ceremony = row.ceremony(yield* _creds(kind))
        const state = generateState()
        const verifier = row.pkce ? Option.some(generateCodeVerifier()) : Option.none<string>()
        yield* states.stash(state, { verifier, kind })
        return ceremony.url(state, Option.getOrElse(verifier, () => ""), row.scopes)
      })
    const callback = (kind: Provider.Kind, code: string, presentedState: string, resolveSubject: (tokens: OAuth2Tokens) => Effect.Effect<string, OAuthFault>): Effect.Effect<TokenPair, OAuthFault | SessionFault> =>
      Effect.gen(function* () {
        const stashed = yield* Effect.flatMap(states.consume(presentedState), Option.match({ onNone: () => Effect.fail(new OAuthFault({ reason: "state", detail: "unknown state" })), onSome: Effect.succeed }))
        if (stashed.kind !== kind) return yield* Effect.fail(new OAuthFault({ reason: "state", detail: "provider mismatch" }))
        const row = _rows[kind]
        const creds = yield* _creds(kind)
        const ceremony = row.ceremony(creds)
        const tokens = yield* Effect.tryPromise({ try: () => ceremony.exchange(code, Option.getOrElse(stashed.verifier, () => "")), catch: _faultOf })
        const sub = yield* Option.match(Option.fromNullable(row.oidc), {
          onSome: (oidc) =>
            jwt.verifyExternal(Redacted.make(tokens.idToken()), { issuer: oidc.issuer, audience: creds.clientId, jwksUri: oidc.jwksUri, algorithms: oidc.algorithms }).pipe(
              Effect.mapError((fault) => new OAuthFault({ reason: "idToken", detail: fault.message })),
              Effect.map((payload) => String(payload.sub)),
            ),
          onNone: () => resolveSubject(tokens),
        })
        const ref = new CredentialRef({ kind: "oauth", key: `${kind}:${sub}` })
        return yield* token.establish(ref, row.scopes, { tenant: Option.none(), verified: true })
      })
    return { authorize, callback } as const
  }),
  dependencies: [Jwt.Default, Token.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { OAuth, OAuthFault, OAuthStateStore }
export type { Provider }
```

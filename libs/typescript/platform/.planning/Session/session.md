# [PLATFORM_SESSION]

One page owns the browser credential lifecycle — `AuthSession`, the OIDC authorization-code-with-PKCE acquisition through `arctic` as the one browser-safe flow, the redirect-continuity round-trip that persists the `{ state, codeVerifier, returnTo, nonce }` pending-flow record across the full-page IdP navigation `arctic` cannot itself span, the session-lifecycle fold over a `SubscriptionRef` carrying the current token with its expiry and refresh schedule, the silent-refresh `Schedule` firing before expiry, the `revokeToken` logout, the per-call `tokenHeader` producer the `interchange` transport interceptor reads at call time, and TOTP/WebAuthn second-factor enrolment riding the one session owner. The pending-flow record persists into the `Session/store.md` `auth-flow` `StoreDomain` named store (the one upstream chain); session faults fold into one closed `AuthFault` `Data.TaggedEnum` so `OAuth2RequestError.code` lands as one typed `ProtocolError` arm under total dispatch. The page holds session state as single-fiber host state inside its own `SubscriptionRef` and authors no decode.

## [1]-[INDEX]

- [1]-[AUTH_SESSION]: the PKCE acquisition, the redirect round-trip, the session fold, the refresh, the revocation, and the token header.

## [2]-[AUTH_SESSION]

- Owner: `AuthSession`, the browser credential owner bound under the platform layer as the auth boot edge — OIDC authorization-code-with-PKCE acquisition, the `mint`-persist-redirect / extract-match-validate-decode redirect-continuity round-trip, the session-lifecycle `SubscriptionRef` fold, the silent-refresh `Schedule`, the `revokeToken` logout, the per-call `tokenHeader` producer, and the second-factor enrolment surfaces.
- Cases: `AuthSession` owns the browser bearer credential through `arctic` `OAuth2Client` as the one browser-safe public-client flow with no implicit grant and no client secret; `login` mints a CSRF `state` through `generateState` and a PKCE `codeVerifier` through `generateCodeVerifier`, persists the `{ state, codeVerifier: Redacted, returnTo, nonce }` `PendingFlow` record through the `local-persistence` `auth-flow` `StoreDomain` named store (single-entry, cleared on completion), then redirects to the `createAuthorizationURLWithPKCE` authorization URL — the in-memory `SubscriptionRef` cannot span the document reload, so the persisted record is the continuity the callback reads back; `complete(callbackUrl)` takes the persisted `PendingFlow`, matches the callback `state` against the persisted value as the CSRF check (a mismatch folding to `AuthFault.ProtocolError`), exchanges the code through `validateAuthorizationCode(code, codeVerifier)`, `decodeIdToken`s the OIDC claims into the `SessionStatus.Authenticated({ subject, expiresAt })` subject, clears the pending record, and pushes the `returnTo` route; the session-lifecycle fold runs over a `SubscriptionRef` carrying the `SessionStatus` `Data.TaggedEnum` (`Anonymous`/`Authenticating`/`Authenticated`/`Expired`) constructed only through the `SessionStatus.Authenticated({...})` constructor so a hand-listed `_tag` literal union the `Session/guard.md` fold cannot prove total is the deleted form; silent refresh runs one `refreshOnce` arm on a `Schedule.spaced` tick that exchanges the carried `Redacted` refresh token through `refreshAccessToken` once the cell crosses its `refreshAt` watermark (one minute before `expiresAt`) and re-stamps the cell from the fresh `OAuth2Tokens`, so the browser never stamps a token cached past its expiry and a missing refresh token folds the session to `Expired`; `logout` calls `revokeToken` on the carried refresh token (falling back to the access token) to invalidate the grant server-side rather than only clearing the cell; `tokenHeader` is the per-call producer the `interchange` `WireTransport` interceptor reads at call time — it unwraps the `Redacted` bearer inside `platform` and ships only the assembled `Bearer ...` header string across the seam (the `Redacted` value never crosses), so `interchange` declares no OIDC dependency; optional TOTP and WebAuthn second-factor enrolment ride the same session owner through one credential axis, never a parallel auth owner — the browser owns only the ceremony (the `@simplewebauthn/browser` `startRegistration`/`startAuthentication` navigator-credential call and the collected TOTP code), and the attestation/secret verification is the `services` `Authn` concern reached over the wire, never a browser-side verifier.
- Cases (fault): `AuthFault` is one closed `Data.TaggedEnum` case family (`Denied`/`Expired`/`RefreshFailed`/`ProtocolError`) — NOT a flat `reason` string union — so the `arctic` `OAuth2RequestError.code` RFC-6749 reason folds in as the `ProtocolError({ code })` case under total dispatch and a new IdP error class breaks every `Match` site at compile time; `Denied` is the user-refused grant, `Expired` the lapsed token, `RefreshFailed` the silent-refresh failure, and `ProtocolError` carries the `OAuth2RequestError.code` string. The collapse is internal-only: `navigation-guard.md` reads `AuthSession.status` (the `SessionStatus` enum), never `AuthFault`, and the only `AuthFault` references are the `login`/`complete`/`currentToken` error channels inside this page, so no external `reason`-matcher pre-dates the collapse.
- Entry: the session status feeds the `ui` login-logout leaf through the atom binding, the `routing` `NavigationGuard` reads `AuthSession.status` to gate a protected route, the `returnTo` drives the post-login `routing` `AppRouter` push and the callback ingress is a `routing` `RouteKey` `login-callback` arm the router resolves (a read-path routing edge, no mutation bypassing the `interchange` `CommandGateway`), and an expired-or-rejected token folds to the `interchange` `FaultDetail` typed failure as a re-auth fault, never a silent redirect from inside a decode; the second-factor verification rides the `interchange` `CommandGateway` to the `services` verifier and never runs in the browser.
- Packages: `effect` for the `SubscriptionRef` session cell, the `Schedule` refresh, the `Redacted` token wrapping, and the `Data.TaggedEnum` `SessionStatus`/`AuthFault` families; `arctic` `OAuth2Client` as the single OAuth/OIDC authorization-code-with-PKCE owner mined to full capability — `generateState`/`generateCodeVerifier` (CSRF+PKCE mint), `createAuthorizationURLWithPKCE` (authorization URL), `validateAuthorizationCode` (code exchange), `refreshAccessToken` (silent refresh), `revokeToken` (logout revocation), `decodeIdToken` (OIDC claims), and `OAuth2RequestError.code` (RFC-6749 reason -> `ProtocolError`); `@simplewebauthn/browser` for the passkey registration/authentication ceremony (the server-side `@simplewebauthn/server` verifier is the `services` concern, never imported here); `local-persistence` for the `auth-flow` `StoreDomain` `pendingFlow` record; `RuntimeConfig` for the OIDC authority and client-id read.
- Growth: a new credential modality lands as one row on the credential axis, never a parallel session owner; a new IdP error class lands as one `AuthFault` `Data.TaggedEnum` case, never a fifth flat `reason` literal; DPoP refresh-token binding (RFC 9449) and cross-origin Bearer stamping are designed-only hardening rows that activate with the cross-origin growth row, aligned with the C#-side Bearer gating, never a current admission.
- Boundary: `AuthSession` holds session state as single-fiber host state inside its own `SubscriptionRef`, never a sixth store fold, so a parallel `AuthStore` arm is the named defect; the pending-flow record persists ONLY into the `local-persistence` `auth-flow` `StoreDomain` named store and the `codeVerifier` stays `Redacted` in the persisted record and is never logged; the bearer stamp and DPoP binding are designed-only growth that activate with the cross-origin growth row exactly as the C# side gates Bearer behind the cross-origin deployment; second-factor verification is the `services` `Authn` concern — the browser owns the ceremony and forwards the result, and a browser-side `@simplewebauthn/server`/`otplib` verifier is the named defect; `AuthSession` reads its endpoints through `RuntimeConfig`, never a direct environment read; it emits the enrolment command through the `interchange` `CommandGateway` and authors no decode; `ui` reads the session status through the binding and never imports `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Data, Effect, Option, Redacted, Schedule, SubscriptionRef } from "effect";
import { OAuth2Client, OAuth2RequestError, OAuth2Tokens, generateCodeVerifier, generateState, decodeIdToken, CodeChallengeMethod } from "arctic";
import { LocalPersistenceLive, type PendingFlow } from "../local-persistence/local-persistence.ts";
import { RuntimeConfig } from "../runtime-config/runtime-config.ts";

// --- [TYPES] ---------------------------------------------------------------------------
type SessionStatus = Data.TaggedEnum<{
  readonly Anonymous: object;
  readonly Authenticating: object;
  readonly Authenticated: { readonly subject: string; readonly expiresAt: number };
  readonly Expired: object;
}>;
const SessionStatus = Data.taggedEnum<SessionStatus>();

type BearerToken = {
  readonly value: Redacted.Redacted;
  readonly refresh: Option.Option<Redacted.Redacted>;
  readonly expiresAt: number;
  readonly refreshAt: number;
};

// --- [ERRORS] --------------------------------------------------------------------------
type AuthFault = Data.TaggedEnum<{
  readonly Denied: object;
  readonly Expired: object;
  readonly RefreshFailed: object;
  readonly ProtocolError: { readonly code: string };
}>;
const AuthFault = Data.taggedEnum<AuthFault>();

const faultOf = (error: unknown): AuthFault =>
  error instanceof OAuth2RequestError ? AuthFault.ProtocolError({ code: error.code }) : AuthFault.RefreshFailed();

// --- [OPERATIONS] ----------------------------------------------------------------------
const tokensToBearer = (tokens: OAuth2Tokens): BearerToken => {
  const expiresAt = tokens.accessTokenExpiresAt().getTime();
  return {
    value: Redacted.make(tokens.accessToken()),
    refresh: tokens.hasRefreshToken() ? Option.some(Redacted.make(tokens.refreshToken())) : Option.none(),
    expiresAt,
    refreshAt: expiresAt - 60_000,
  };
};

// --- [SERVICES] ------------------------------------------------------------------------
interface AuthSession {
  readonly status: SubscriptionRef.SubscriptionRef<SessionStatus>;
  readonly login: (returnTo: string) => Effect.Effect<void, AuthFault>;
  readonly complete: (callbackUrl: URL) => Effect.Effect<string, AuthFault>;
  readonly logout: Effect.Effect<void>;
  readonly currentToken: Effect.Effect<Option.Option<BearerToken>, AuthFault>;
  readonly tokenHeader: Effect.Effect<Option.Option<string>>;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
class AuthSessionLive extends Effect.Service<AuthSessionLive>()("@rasm/ts/platform/AuthSession", {
  scoped: Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const store = yield* LocalPersistenceLive;
    const authority = yield* config.oidcAuthority;
    const clientId = yield* config.oidcClientId;
    const redirectUri = yield* config.oidcRedirectUri;
    const client = new OAuth2Client(clientId, null, redirectUri);
    const status = yield* SubscriptionRef.make<SessionStatus>(SessionStatus.Anonymous());
    const token = yield* SubscriptionRef.make<Option.Option<BearerToken>>(Option.none());

    const login = (returnTo: string): Effect.Effect<void, AuthFault> =>
      Effect.gen(function* () {
        const state = generateState();
        const codeVerifier = generateCodeVerifier();
        const nonce = generateState();
        const pending: PendingFlow = { state, codeVerifier: Redacted.make(codeVerifier), returnTo, nonce };
        yield* store.pendingFlow.put(pending);
        yield* SubscriptionRef.set(status, SessionStatus.Authenticating());
        const url = client.createAuthorizationURLWithPKCE(`${authority}/authorize`, state, CodeChallengeMethod.S256, codeVerifier, ["openid", "profile"]);
        yield* Effect.sync(() => { window.location.assign(url.toString()); }); // BOUNDARY ADAPTER: full-page IdP redirect
      });

    const complete = (callbackUrl: URL): Effect.Effect<string, AuthFault> =>
      Effect.gen(function* () {
        const pending = yield* store.pendingFlow.take;
        const flow = yield* Option.match(pending, {
          onNone: () => Effect.fail(AuthFault.ProtocolError({ code: "no_pending_flow" })),
          onSome: (value) => Effect.succeed(value),
        });
        const returnedState = callbackUrl.searchParams.get("state");
        const code = callbackUrl.searchParams.get("code");
        yield* returnedState === flow.state && code !== null
          ? Effect.void
          : Effect.fail(AuthFault.ProtocolError({ code: "state_mismatch" }));
        const tokens = yield* Effect.tryPromise({
          try: () => client.validateAuthorizationCode(`${authority}/token`, code as string, Redacted.value(flow.codeVerifier)),
          catch: faultOf,
        });
        const claims = decodeIdToken(tokens.idToken()) as { readonly sub: string };
        const bearer = tokensToBearer(tokens);
        yield* SubscriptionRef.set(token, Option.some(bearer));
        yield* SubscriptionRef.set(status, SessionStatus.Authenticated({ subject: claims.sub, expiresAt: bearer.expiresAt }));
        yield* store.pendingFlow.clear;
        return flow.returnTo;
      });

    const refreshOnce: Effect.Effect<void, AuthFault> = SubscriptionRef.get(token).pipe(
      Effect.flatMap(Option.match({
        onNone: () => Effect.void,
        onSome: (bearer) =>
          Date.now() < bearer.refreshAt
            ? Effect.void
            : Option.match(bearer.refresh, {
                onNone: () => Effect.fail(AuthFault.Expired()),
                onSome: (refreshToken) =>
                  Effect.tryPromise({
                    try: () => client.refreshAccessToken(`${authority}/token`, Redacted.value(refreshToken), ["openid", "profile"]),
                    catch: faultOf,
                  }).pipe(
                    Effect.flatMap((next) => SubscriptionRef.set(token, Option.some(tokensToBearer(next)))),
                  ),
              }),
      })),
    );

    yield* refreshOnce.pipe(
      Effect.catchAll(() => SubscriptionRef.set(status, SessionStatus.Expired())),
      Effect.repeat(Schedule.spaced("30 seconds")),
      Effect.forkScoped,
    );

    const logout: Effect.Effect<void> = SubscriptionRef.get(token).pipe(
      Effect.flatMap(Option.match({
        onNone: () => Effect.void,
        onSome: (bearer) =>
          Effect.promise(() =>
            client.revokeToken(`${authority}/revoke`, Redacted.value(Option.getOrElse(bearer.refresh, () => bearer.value))),
          ),
      })),
      Effect.zipRight(SubscriptionRef.set(token, Option.none())),
      Effect.zipRight(SubscriptionRef.set(status, SessionStatus.Anonymous())),
    );

    const currentToken: Effect.Effect<Option.Option<BearerToken>, AuthFault> = SubscriptionRef.get(token);

    const tokenHeader: Effect.Effect<Option.Option<string>> = SubscriptionRef.get(token).pipe(
      Effect.map(Option.map((bearer) => `Bearer ${Redacted.value(bearer.value)}`)),
    );

    return { status, login, complete, logout, currentToken, tokenHeader } satisfies AuthSession;
  }),
  dependencies: [LocalPersistenceLive.Default],
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type AuthSession, type SessionStatus, type AuthFault, type BearerToken, SessionStatus, AuthFault, AuthSessionLive };
```

# [PLATFORM_AUTH_SESSION]

One page owns the browser credential lifecycle — `AuthSession`, the OIDC authorization-code-with-PKCE acquisition through `arctic` as the one browser-safe flow, the session-lifecycle fold over a `SubscriptionRef` carrying the current token with its expiry and refresh schedule, the silent-refresh `Schedule` firing before expiry, the per-call `tokenHeader` producer the `interchange` transport interceptor reads at call time, and TOTP/WebAuthn second-factor enrolment riding the one session owner. The page holds session state as single-fiber host state inside its own `SubscriptionRef` and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                  |
| :-----: | :----------- | :-------------------------------------------------------------------- |
|   [1]   | AUTH_SESSION | the PKCE acquisition, the session fold, the refresh, and the token header |

## [2]-[AUTH_SESSION]

- Owner: `AuthSession`, the browser credential owner bound under the platform layer as the auth boot edge — OIDC authorization-code-with-PKCE acquisition, the session-lifecycle `SubscriptionRef` fold, the silent-refresh `Schedule`, the per-call `tokenHeader` producer, and the second-factor enrolment surfaces.
- Cases: `AuthSession` owns the browser bearer credential through `arctic` as the one browser-safe public-client flow with no implicit grant and no client secret; the session-lifecycle fold runs over a `SubscriptionRef` carrying the current `BearerToken` with its expiry and refresh point; silent refresh fires through a `Schedule` before expiry so the browser never stamps a token cached past its expiry; `tokenHeader` is the per-call producer the `interchange` `WireTransport` interceptor reads at call time — it unwraps the `Redacted` bearer inside `platform` and ships only the assembled `Bearer ...` header string across the seam (the `Redacted` value never crosses), so `interchange` declares no OIDC dependency; optional TOTP and WebAuthn second-factor enrolment ride the same session owner through one credential axis, never a parallel auth owner — the browser owns only the ceremony (the `@simplewebauthn/browser` `startRegistration`/`startAuthentication` navigator-credential call and the collected TOTP code), and the attestation/secret verification is the `services` `Authn` concern reached over the wire, never a browser-side verifier.
- Entry: the session status feeds the `ui` login-logout leaf through the atom binding, the `routing` `NavigationGuard` reads `AuthSession.status` to gate a protected route, and an expired-or-rejected token folds to the `interchange` `FaultDetail` typed failure as a re-auth fault, never a silent redirect from inside a decode; the second-factor verification rides the `interchange` `CommandGateway` to the `services` verifier and never runs in the browser.
- Packages: `effect` for the `SubscriptionRef` session cell, the `Schedule` refresh, and the `Redacted` token wrapping; `arctic` as the single OAuth/OIDC authorization-code-with-PKCE owner for the token endpoint and refresh; `@simplewebauthn/browser` for the passkey registration/authentication ceremony (the server-side `@simplewebauthn/server` verifier is the `services` concern, never imported here); `RuntimeConfig` for the OIDC authority and client-id read.
- Growth: a new credential modality lands as one row on the credential axis, never a parallel session owner; DPoP refresh-token binding (RFC 9449) is a designed-only hardening row that activates with the cross-origin growth row, aligned with the C#-side Bearer gating, never a current admission.
- Boundary: `AuthSession` holds session state as single-fiber host state inside its own `SubscriptionRef`, never a sixth store fold, so a parallel `AuthStore` arm is the named defect; the bearer stamp is designed-only growth that activates with the cross-origin growth row exactly as the C# side gates Bearer behind the cross-origin deployment; second-factor verification is the `services` `Authn` concern — the browser owns the ceremony and forwards the result, and a browser-side `@simplewebauthn/server`/`otplib` verifier is the named defect; `AuthSession` reads its endpoints through `RuntimeConfig`, never a direct environment read; it emits the enrolment command through the `interchange` `CommandGateway` and authors no decode; `ui` reads the session status through the binding and never imports `platform`.

```ts contract
type SessionStatus =
  | { readonly _tag: "Anonymous" }
  | { readonly _tag: "Authenticating" }
  | { readonly _tag: "Authenticated"; readonly subject: string; readonly expiresAt: number }
  | { readonly _tag: "Expired" };

type BearerToken = { readonly value: Redacted.Redacted; readonly expiresAt: number; readonly refreshAt: number };

class AuthFault extends Data.TaggedError("AuthFault")<{ readonly reason: "denied" | "expired" | "refresh-failed" }> {}

interface AuthSession {
  readonly status: SubscriptionRef.SubscriptionRef<SessionStatus>;
  readonly login: Effect.Effect<void, AuthFault>;
  readonly logout: Effect.Effect<void>;
  readonly currentToken: Effect.Effect<Option.Option<BearerToken>, AuthFault>;
  readonly tokenHeader: Effect.Effect<Option.Option<string>>;
}
```

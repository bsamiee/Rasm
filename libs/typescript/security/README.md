# [SECURITY]

`security` composes authn, authz, sessions, secrets, and signing from stateless primitives into Effect-owned `Layer`/`Service` families — no framework owns the schema, and the better-auth rejection is the architecture. Identity is not stored in-folder: `session` declares `SessionStore`/`IdentityJournal` port `Tag`s against its own models, and the app root satisfies them with `store` journal `Layer`s, so identity state is event-sourced through the same journal the rest of the branch uses. Every crypto package has exactly one admission sub-folder, ban-enforced by `proof/gauge`: `authn`, `sign`, and `secret` are the three admission boundaries, and a package resolved outside its owner is the named defect. This file routes the folder and registers its packages; the sub-domain map and seam record are `ARCHITECTURE.md`; versions live only in the `pnpm-workspace.yaml` catalog.

## [1]-[ROUTER]

Twelve design pages across four sub-domains under `.planning/`; each becomes one `security/src/<sub>/<page>.ts` file.

- `authn/` — the credential-ceremony sub-domain (`arctic`/`@simplewebauthn`/`otplib` admitted here only).
  - [OAUTH](.planning/authn/oauth.md) — `arctic` OAuth/OIDC provider rows (authorization-code + PKCE).
  - [WEBAUTHN](.planning/authn/webauthn.md) — passkey ceremonies over the browser-safe subpath.
  - [OTP](.planning/authn/otp.md) — `otplib` TOTP/HOTP rows plus recovery/backup-code rows.
  - [APIKEY](.planning/authn/apikey.md) — machine credentials: mint, digest-at-rest, rotate/revoke, prefix-indexed byHash resolve; hashing delegates `sign/crypto`.
- `session/` — the token/cookie lifecycle and the identity port declarations.
  - [TOKEN](.planning/session/token.md) — token/refresh vocabulary, refresh rotation/revocation law, and the `SessionStore`/`IdentityJournal` port `Tag`s.
  - [COOKIE](.planning/session/cookie.md) — cookie rows (httpOnly, sameSite, path-scoped) and the CSRF law.
- `authz/` — the entitlement and tenancy claim algebra.
  - [POLICY](.planning/authz/policy.md) — RBAC/ReBAC relation tuples and policy-evaluation folds (verdict evaluation delegates `host/flag`).
  - [CLAIM](.planning/authz/claim.md) — tenant/entitlement claims and the `app.current_tenant` contract `store` enforces as RLS.
- `secret/` — leased-secret custody and key-material vocabulary.
  - [DOPPLER](.planning/secret/doppler.md) — the Doppler axis: TTL-leased rotation, `Redacted` end-to-end.
  - [MATERIAL](.planning/secret/material.md) — key-material vocabulary; `CredentialPemWire` terminates here as a redacted carrier.
- `sign/` — the crypto owner (`jose`/`@node-rs/argon2`/`@oslojs/*` admitted here only, node-only subpath).
  - [JWT](.planning/sign/jwt.md) — `jose` JWT/JWS/JWKS and rotation — the one token-crypto owner.
  - [CRYPTO](.planning/sign/crypto.md) — argon2 hashing, HMAC webhook signing, and the AES-GCM envelope `Shredder` primitive `store/journal` consumes.

## [2]-[DOMAIN_PACKAGES]

The folder-local security libraries, grouped by `[CONCERN]`, each naming its owning admission sub-folder — the ban list resolves a package outside that sub-folder as a build failure. No version pins (centralized in `pnpm-workspace.yaml`); catalogues live at this folder's `.api/`.

[AUTHN] — admitted in `authn/` only:
- `arctic` — OAuth/OIDC provider rows: authorization-code and PKCE flows.
- `@simplewebauthn/server` — passkey registration/authentication ceremony verification (node subpath).
- `@simplewebauthn/browser` — passkey ceremony invocation over the browser-safe subpath.
- `otplib` — TOTP/HOTP generation and verification for the second-factor and recovery-code rows.

[SIGN] — admitted in `sign/` only (node-only subpath):
- `jose` — the one JWT/JWS/JWKS/JWE owner and key rotation; the better-auth/oslo JWT splits are the recorded rejection.
- `@node-rs/argon2` — argon2id password/credential hashing at rest.
- `@oslojs/crypto` — constant-time primitives, HMAC, and RNG behind the signing folds.
- `@oslojs/encoding` — base32/base64url/hex codecs for token and digest material.

[SECRET] — admitted in `secret/` only:
- `@dopplerhq/node-sdk` — the Doppler secret axis: TTL-leased fetch and rotation, `Redacted` end-to-end.

## [3]-[SUBSTRATE_PACKAGES]

The language substrate this folder consumes; charters live in `libs/typescript/.planning/README.md` and API evidence at `libs/typescript/.api/`.

- `effect` — the rails, `Schema`, `Layer`, `Match`, `Stream`, and vocabulary substrate every family builds on.
- `@effect/platform` — the platform service contracts the session/cookie ingress and secret-fetch rows type against.

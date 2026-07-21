# [TS_SECURITY]

`security` is the branch's identity-and-custody authority. Its bar is unspellable bypass: every crypto operation originates from one canonical owner through a single key-admission path, every secret is `Redacted` from first decode and unwraps only into the primitive call, and single-use ceremony snapshots type-witness leg order — replay, cross-ceremony completion, and out-of-order finish cannot be written. Every credential-verify surface is throttled and telemetered structurally; a rejected credential is a verdict arm, never a fault; a new provider, dialect, surface, or role is a table row.

This folder is stateless over ports by construction — every durable obligation is a port Tag satisfied downstream at app composition, identity and claim stores by the data wave and flag evaluation by the runtime wave — so a zero-durable-state browser app composes it whole. It holds keys and verdicts, never rows; content-identity digesting stays core's; tenancy is declared here and enforced as row-level security in the data wave.

## [01]-[ROUTER]

[CRYPT]:
- [01]-[SIGN](.planning/crypt/sign.md): one crypto authority — argon2id digest, HMAC egress, opaque tokens, `Shredder`, key admission, JWT/JWKS/JWE, bench-graded cost calibration.
- [02]-[VERIFY](.planning/crypt/verify.md): inbound-signature dialect table, the throttled held-octet verify fold, and the folder `Reject` stream.
- [03]-[SECRET](.planning/crypt/secret.md): Doppler leased-secret custody — rotation feed, lease lifecycle, `Credential` handoff.

[AUTHN]:
- [04]-[SESSION](.planning/authn/session.md): session spine — rotation statechart with reuse detection, `BearerGuard`, cookie framing, CSRF.
- [05]-[CREDENTIAL](.planning/authn/credential.md): one mint-and-resolve digest idiom over OTP, recovery codes, and machine API keys.
- [06]-[OAUTH](.planning/authn/oauth.md): issuers as rows over one authorization-code ceremony; single-use state, OIDC verify through `Jwt`.
- [07]-[WEBAUTHN](.planning/authn/webauthn.md): both passkey halves as per-runtime subpaths — RP verifier and browser invocation.

[ACCESS]:
- [08]-[CLAIM](.planning/access/claim.md): entitlement vocabulary and the RBAC-union-ReBAC fold resolved once per request into a tagged verdict.
- [09]-[TENANT](.planning/access/tenant.md): ambient `TenantScope` reference, the session-GUC RLS contract, and the tenant metric-tag aspect.
- [10]-[AUDIT](.planning/access/audit.md): security fact rail — `SecurityFact` vocabulary, app-scoped `Witness` publish seam, `AuditJournal` port, pseudonymized egress, snapshot and board/alert projections.

## [02]-[DOMAIN_PACKAGES]

Folder-specific libraries admitted here; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[CRYPTO_TOKEN]:
- `jose`
- `@node-rs/argon2`
- `@oslojs/crypto`
- `@oslojs/encoding`

[CEREMONY]:
- `arctic` — browser authorization-code redirect and provider rows.
- `openid-client` — certified OIDC RP: machine grants (client-credentials, token exchange), DPoP, introspection.
- `@simplewebauthn/server`
- `@simplewebauthn/browser`
- `@otplib/core` — OTP substrate `otplib` composes.
- `otplib`

[CUSTODY]:
- `@dopplerhq/node-sdk`

## [03]-[SUBSTRATE_PACKAGES]

Shared TypeScript substrate consumed from the branch registry; `libs/typescript/.planning/README.md` and `libs/typescript/.api/` own the contracts and evidence.

[EFFECT_RUNTIME]:
- `effect`
- `@effect/platform`
- `@effect/experimental`

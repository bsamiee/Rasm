# [TS_SECURITY]

`security` is the branch's identity-and-custody authority. Its bar is unspellable bypass: every crypto operation originates from one canonical owner through a single key-admission path, every secret is `Redacted` from first decode and unwraps only into the primitive call, and single-use ceremony snapshots type-witness leg order — replay, cross-ceremony completion, and out-of-order finish cannot be written. Every credential-verify surface is throttled and telemetered structurally; a rejected credential is a verdict arm, never a fault; a new provider, dialect, surface, or role is a table row.

This folder is stateless over ports by construction — every durable obligation is a port Tag satisfied downstream at app composition, identity and claim stores by the data stratum and flag evaluation by the runtime stratum — so a zero-durable-state browser app composes it whole. It holds keys and verdicts, never rows; content-identity digesting stays core's; tenancy is declared here and enforced as row-level security in the data stratum.

## [01]-[ROUTER]

[CRYPT]:
- [01]-[SIGN](.planning/crypt/sign.md): Sole crypto mint — every digest, signature, token, and envelope originates here; cost rows bench-calibrated.
- [02]-[VERIFY](.planning/crypt/verify.md): Inbound-signature dialect table, the throttled held-octet verify fold, and the folder `Reject` stream.
- [03]-[SECRET](.planning/crypt/secret.md): Doppler leased-secret custody — rotation feed, lease lifecycle, `Credential` handoff.

[AUTHN]:
- [04]-[SESSION](.planning/authn/session.md): Session spine — rotation statechart with reuse detection, `BearerGuard`, cookie framing, CSRF.
- [05]-[CREDENTIAL](.planning/authn/credential.md): Second factors — OTP, recovery codes, and machine keys ride one mint-and-resolve digest idiom.
- [06]-[OAUTH](.planning/authn/oauth.md): Issuers as rows over one authorization-code ceremony; single-use state, OIDC verify through `Jwt`.
- [07]-[WEBAUTHN](.planning/authn/webauthn.md): Both passkey halves as per-runtime subpaths — RP verifier and browser invocation.

[ACCESS]:
- [08]-[CLAIM](.planning/access/claim.md): Entitlement vocabulary and the RBAC-union-ReBAC fold resolved once per request into a tagged verdict.
- [09]-[TENANT](.planning/access/tenant.md): Ambient `TenantScope` reference, the session-GUC RLS contract, and the tenant metric-tag aspect.
- [10]-[AUDIT](.planning/access/audit.md): Fact rail — loud arms publish through `Witness` into the `AuditJournal` port; egress pseudonymized.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[CRYPTO_TOKEN]:
- `jose`
- `@node-rs/argon2`
- `@oslojs/crypto`
- `@oslojs/encoding`

[CEREMONY]:
- `arctic` — browser authorization-code redirect and provider rows.
- `openid-client` — machine-grant OIDC lane: client-credentials, token exchange, DPoP, introspection.
- `@simplewebauthn/server`
- `@simplewebauthn/browser`
- `@otplib/core` — OTP substrate `otplib` composes.
- `otplib`

[CUSTODY]:
- `@dopplerhq/node-sdk`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Ts registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/experimental`

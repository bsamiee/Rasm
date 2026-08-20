# [TS_SECURITY]

`security` is the branch's identity-and-custody authority: the crypto mint and its inbound mirror, the authentication ceremonies over one session spine, and the access plane turning verified identity into decisions and evidence. Keys and verdicts are its whole state surface; every durable obligation leaves as a port the data stratum satisfies.

## [01]-[ROUTER]

[CRYPT]:
- [01]-[SIGN](.planning/crypt/sign.md): Sole crypto mint — digests, signatures, tokens, and sealed envelopes originate here; costs bench-calibrated.
- [02]-[VERIFY](.planning/crypt/verify.md): Inbound-signature dialect table, the throttled held-octet verify fold, and the folder `Reject` stream.
- [03]-[SECRET](.planning/crypt/secret.md): Doppler leased-secret custody — rotation feed, lease lifecycle, `Material.Source.Held` handoff.

[AUTHN]:
- [04]-[SESSION](.planning/authn/session.md): Session spine — rotation statechart with reuse detection, `BearerGuard`, cookie framing, CSRF.
- [05]-[CREDENTIAL](.planning/authn/credential.md): Second factors — OTP, recovery codes, and machine keys ride one mint-and-resolve digest idiom.
- [06]-[OAUTH](.planning/authn/oauth.md): Issuers as rows over one authorization-code ceremony; single-use state, OIDC verify through `Jwt`.
- [07]-[WEBAUTHN](.planning/authn/webauthn.md): Both passkey halves as per-runtime subpaths — RP verifier and browser invocation.
- [08]-[WORKLOAD](.planning/authn/workload.md): Machine identity — grant cases per discovered issuer client, DPoP constraint, principal projection.

[ACCESS]:
- [09]-[CLAIM](.planning/access/claim.md): Entitlement vocabulary and the RBAC-union-ReBAC fold resolved once per request into a tagged verdict.
- [10]-[TENANT](.planning/access/tenant.md): Ambient `TenantScope` reference, the session-GUC RLS contract, and the tenant metric-tag aspect.
- [11]-[AUDIT](.planning/access/audit.md): Fact rail — loud arms publish through `Witness` into the `AuditJournal` port; egress pseudonymized.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[CRYPTO_TOKEN]:
- `jose`
- `@node-rs/argon2`
- `@noble/hashes` — HMAC and SHA symmetric primitives the crypto authority composes; asymmetric verify rides WebCrypto.
- `@oslojs/encoding`

[CEREMONY]:
- `openid-client` — Sole OAuth custodian: the browser authorization-code ceremony with its provider rows, and the whole machine-grant lane.
- `@simplewebauthn/server`
- `@simplewebauthn/browser`
- `@otplib/core` — OTP substrate `otplib` composes.
- `otplib`

[CUSTODY]:
- `@dopplerhq/node-sdk`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the TypeScript registry, whose charters own the full contracts; `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/experimental`

[BENCH]:
- `mitata` — `crypt/sign#CALIBRATION` samples each KDF cost row through the state-free `measure` kernel.

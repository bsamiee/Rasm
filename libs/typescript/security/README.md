# [TS_SECURITY]

`libs/typescript/security` owns the branch identity-and-custody authority — authentication ceremonies, authorization decisions, and the crypto authority — as Effect Layers over stateless primitives. State lives behind ports the data wave satisfies at app composition; this folder holds keys and verdicts, never rows.

## [01]-[ROUTER]

[CRYPT]:
- [01]-[SIGN](.planning/crypt/sign.md): Crypto authority minting every digest, signature, token, and envelope.
- [02]-[VERIFY](.planning/crypt/verify.md): Inbound-signature dialect table folding one constant-time verify over held request octets.
- [03]-[SECRET](.planning/crypt/secret.md): Leased-secret custody scoping `DopplerSDK` to the surfaces the folder admits.

[AUTHN]:
- [04]-[SESSION](.planning/authn/session.md): Identity spine owning `Session` rotation, ports, and CSRF egress the ceremonies feed.
- [05]-[CREDENTIAL](.planning/authn/credential.md): One mint-and-resolve idiom over OTP, recovery codes, and machine API keys.
- [06]-[OAUTH](.planning/authn/oauth.md): Issuer-row authorization-code ceremony over `arctic` — url, exchange, refresh, revoke per row.
- [07]-[WEBAUTHN](.planning/authn/webauthn.md): Passkey ceremony split by runtime subpath so the browser bundle drops the RP verifier.

[ACCESS]:
- [08]-[CLAIM](.planning/access/claim.md): Entitlement fold evaluating the RBAC-union-ReBAC decision once per request.
- [09]-[TENANT](.planning/access/tenant.md): Tenancy contract projecting the `app.current_tenant` RLS shape the data wave enforces.

## [02]-[DOMAIN_PACKAGES]

Folder-specific libraries admitted here; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[CRYPTO_TOKEN]:
- `jose`
- `@node-rs/argon2`
- `@oslojs/crypto`
- `@oslojs/encoding`

[CEREMONY]:
- `arctic`
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

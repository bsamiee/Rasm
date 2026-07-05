# [TS_SECURITY]

`libs/typescript/security` is the identity-and-custody wave of the branch: authentication ceremonies (session rotation, OAuth, passkeys, digest-at-rest credentials), authorization (claims, tenancy contract), and the crypto authority (signing, token minting, crypto-shredding, leased-secret custody, inbound-signature verification) — all as Effect-owned Layers over stateless primitives. State lives behind ports the data wave satisfies at app composition; this folder holds keys and verdicts, never rows. `ARCHITECTURE.md` carries the domain map and seams, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[SIGN](.planning/crypt/sign.md)
- [02]-[VERIFY](.planning/crypt/verify.md)
- [03]-[SECRET](.planning/crypt/secret.md)
- [04]-[SESSION](.planning/authn/session.md)
- [05]-[CREDENTIAL](.planning/authn/credential.md)
- [06]-[OAUTH](.planning/authn/oauth.md)
- [07]-[WEBAUTHN](.planning/authn/webauthn.md)
- [08]-[CLAIM](.planning/access/claim.md)
- [09]-[TENANT](.planning/access/tenant.md)

## [02]-[DOMAIN_PACKAGES]

Every folder-specific external library, planned or implemented. Versions are centralized in `pnpm-workspace.yaml`; corroborating API evidence lives in the adjacent `.api/` folder.

[TOKEN_AUTHORITY]:
- `jose`

[DIGEST_PRIMITIVES]:
- `@node-rs/argon2`
- `@oslojs/crypto`
- `@oslojs/encoding`
- `otplib`

[OAUTH_CEREMONY]:
- `arctic`

[PASSKEYS]:
- `@simplewebauthn/server`
- `@simplewebauthn/browser`

[SECRET_CUSTODY]:
- `@dopplerhq/node-sdk`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting TypeScript substrate this folder consumes; canonical registry and charters live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` folder.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`

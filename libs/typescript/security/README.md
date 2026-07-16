# [TS_SECURITY]

`security` is the branch's identity-and-custody authority — one body: the crypto authority and key plane, authentication as data-carried ceremony, and the authorization and tenancy fold. Its bar is unspellable bypass, not policed convention: every crypto operation originates from one canonical owner with a single key-admission path, every secret is `Redacted` from first decode and unwraps only into the primitive call, and protocol order is enforced by data — single-use ceremony snapshots type-witness leg order, so replay, cross-ceremony completion, and out-of-order finish cannot be written. Every credential-verify surface is throttled and telemetered structurally with zero call-site change; a rejected OTP or a rotated-out token is a verdict arm, never a fault; a new provider, dialect, credential surface, or role is a table row, never a branch.

This folder is stateless over ports by construction — every durable obligation is a port Tag satisfied downstream at app composition, identity and claim stores by the data wave and flag evaluation by the runtime wave — so a zero-durable-state browser app composes it whole. It holds keys and verdicts, never rows; content-identity digesting stays core's; tenancy is declared here and enforced as row-level security in the data wave.

## [01]-[ROUTER]

- [01]-[CRYPT](.planning/crypt/): One crypto authority — signing, minting, shredding, held-octet verify, and secret custody; one key-admission path.
- [02]-[AUTHN](.planning/authn/): Authentication as data-carried ceremony over the session spine; every second factor and provider a row.
- [03]-[ACCESS](.planning/access/): Entitlement fold resolved once per request into a tagged verdict, and the tenancy contract enforced as RLS.

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

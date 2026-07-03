# [SECURITY_ARCHITECTURE]

The domain map of `security` — the W1 runtime folder owning authn, authz, sessions, secrets, and signing as Effect-owned `Layer` families over stateless primitives. Four sub-domains — `authn`, `session`, `authz`, `secret`, `sign` — compose into one admission-gated identity plane: `authn` runs the credential ceremonies, `session` owns the token/cookie lifecycle and declares the identity ports, `authz` owns the entitlement and tenancy claims, and `secret`/`sign` own custody and crypto. The folder imports `kernel` and `host` only; it never imports `store`, composing port `Tag`s the app root satisfies instead. Dependency direction is fixed once in the branch `ARCHITECTURE.md`.

Each codemap node is the eventual `.ts` source file its `.planning/` design page becomes, in TypeScript lowercase casing. Treat every node as realized code; the `.planning/` scaffold is authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
security/src/ # imports kernel + host only; never store (SessionStore/IdentityJournal are ports)
├── authn/ # Credential ceremonies — arctic/@simplewebauthn/otplib admitted here only
│ ├── oauth.ts     # arctic OAuth/OIDC provider rows (authorization-code + PKCE)
│ ├── webauthn.ts  # passkey registration/authentication ceremonies — browser-safe subpath
│ ├── otp.ts       # otplib TOTP/HOTP rows + recovery/backup-code rows
│ └── apikey.ts    # machine credentials: mint, digest-at-rest, rotate/revoke, prefix-indexed byHash resolve — hashing delegates sign/crypto
├── session/ # Token/cookie lifecycle + identity port declarations
│ ├── token.ts     # token/refresh vocabulary + refresh rotation/revocation law + SessionStore/IdentityJournal port Tags
│ └── cookie.ts    # cookie rows (httpOnly, sameSite, path-scoped) + the CSRF law
├── authz/ # Entitlement + tenancy claim algebra
│ ├── policy.ts    # RBAC/ReBAC relation tuples + policy-evaluation folds (verdict evaluation delegates host/flag)
│ └── claim.ts     # tenant/entitlement claims + the app.current_tenant contract store enforces as RLS
├── secret/ # Leased-secret custody + key-material vocabulary
│ ├── doppler.ts   # Doppler axis: TTL-leased rotation, Redacted end-to-end
│ └── material.ts  # key-material vocabulary; CredentialPemWire terminates here — redacted carrier, never logged
└── sign/ # The crypto owner — jose/@node-rs/argon2/@oslojs/* admitted here only, node-only subpath
  ├── jwt.ts       # jose JWT/JWS/JWKS + rotation — the one token-crypto owner
  └── crypto.ts    # argon2 hashing + HMAC webhook signing + AES-GCM envelope (the store Shredder primitive)
```

The three crypto sub-folders are the admission boundaries the `tests/typescript/_architecture` suite audits: `authn` owns the ceremony packages, `sign` owns the token-crypto and hashing primitives (`sign/crypto` is the one HMAC/AES-GCM owner), and `secret` owns the leased-secret provider. `authn/apikey` delegates its digest to `sign/crypto` — an in-folder relation carried in the map, never a seam. `session` declares the identity ports so a zero-durable-state browser app composes `security` without pulling the SQL folder.

## [02]-[SEAMS]

```text seams
secret/material.ts ← csharp:Rasm.AppHost # [WIRE]: CredentialPemWire redacted carrier — decodes in wire, terminates here, never logged
session/token.ts   ← store/journal       # [PORT]: SessionStore/IdentityJournal declared here, satisfied by store journal Layers at the app root
sign/crypto.ts     → store/journal       # [SHAPE]: the AES-GCM envelope Shredder primitive store/journal imports for per-subject crypto-shredding
authz/claim.ts     → store/scope         # [SHAPE]: the app.current_tenant tenancy contract store enforces as RLS
```

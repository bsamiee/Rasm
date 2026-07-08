# [TS_SECURITY_ARCHITECTURE]

The domain map of `security` — the wave-1 identity-and-custody package. Three sub-domains (`crypt`, `authn`, `access`) meet through one crypto authority, one session vocabulary, and one tenancy contract; every stateful obligation is a port Tag the data wave satisfies at app composition, so the folder imports only core.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, camelCase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
security/
└── src/
    ├── crypt/                 # The crypto authority: signing, minting, shredding, custody, inbound verification
    │   ├── sign.ts            # Argon2id, HMAC egress, opaque tokens, AES-GCM Shredder, jose keys, JWT/JWS/JWKS/JWE
    │   ├── verify.ts          # The inbound-signature dialect table + one constant-time verify fold over HELD request octets
    │   └── secret.ts          # DopplerSDK leased-secret custody behind Layer.scoped — download, targeted read, name census
    ├── authn/                 # Authentication: session spine, digest credentials, OAuth, passkeys
    │   ├── session.ts         # Subject/Session/CredentialRef/TokenPair spine, rotation, ports, CSRF egress
    │   ├── credential.ts      # Digest — the one mint-and-resolve idiom over OTP, recovery codes, and machine API keys
    │   ├── oauth.ts           # Issuer-row OAuth authorization-code ceremony over arctic — url/exchange/refresh/revoke legs per row
    │   └── webauthn.ts        # Both passkey halves as per-runtime subpaths: RP verifier (./server) + browser invocation (./browser)
    └── access/                # Authorization: entitlement fold and the tenancy contract
        ├── claim.ts           # The entitlement vocabulary + the RBAC-union-ReBAC evaluation fold, resolved once per request
        └── tenant.ts          # The ambient TenantContext reference + the app.current_tenant RLS shape the data wave enforces
```

## [02]-[SEAMS]

```text seams
access/tenant   →  typescript:data/lane       # [BOUNDARY]: app.current_tenant RLS contract + ambient TenantScope read
authn/session   ←  typescript:data/lane       # [PORT]: SessionStore/IdentityJournal satisfied by scope-built Layers
access/claim    ←  typescript:data/lane       # [PORT]: ClaimStore/RelationStore satisfied by scope-built Layers
crypt/sign      →  typescript:data/journal    # [SHAPE]: Shredder five-verb envelope, WrappedKey per-subject ledger
crypt/verify    →  typescript:runtime/serve   # [BOUNDARY]: Intake held-octets verify seam on the ingress route
authn/session   →  typescript:runtime/serve   # [PORT]: BearerGuard/ApiKeyGuard HttpApiMiddleware Tags mounted on api routes
authn/session   ←  typescript:data/lane       # [PORT]: RateLimiterStore backing the credential-verify throttle budgets
authn/session   ⇄  typescript:runtime/browser # [SHAPE]: CookieSpec.csrf double-submit read at the session plane
authn/oauth     ⇄  typescript:runtime/browser # [BOUNDARY]: redirect-ceremony continuity (depart/land) at the route plane
crypt/secret    →  typescript:iac/kube        # [BOUNDARY]: doppler-run leased env injection at the workload entrypoint
```

## [03]-[ORGANIZATION]

`crypt/sign` is the one crypto authority — every digest, signature, token, and envelope in the folder mints there; `crypt/verify` is its inbound mirror, one dialect table over held octets so no route hand-rolls signature checks; `crypt/secret` scopes the Doppler client to exactly the leased surfaces the folder admits. `authn/session` is the identity spine the other ceremonies feed: `credential` funnels three second-factor surfaces through one mint-and-resolve idiom, `oauth` models issuers as vocabulary rows, and `webauthn` splits the passkey ceremony by runtime subpath so the browser bundle never carries the RP verifier. `access` turns verified identity into decisions: `claim` evaluates entitlements once per request, `tenant` states the tenancy contract the data wave enforces as row-level security.

## [04]-[BOUNDARIES]

- The folder persists nothing: `SessionStore`, `IdentityJournal`, `ClaimStore`, and `RelationStore` are port Tags; the data wave builds the satisfying Layers and the app root binds them.
- Content-identity digesting is core's; this folder owns secret derivation and authenticated crypto only.
- Cookie framing and CSRF are egress projections declared here and consumed by the runtime browser session plane; no browser API is touched here.
- Tenancy is declared here and enforced in the data wave; the folder never opens a database transaction.

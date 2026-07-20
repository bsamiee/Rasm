# [TS_SECURITY_ARCHITECTURE]

`security` owns the identity-and-custody concern — the `crypt`, `authn`, and `access` sub-domains meeting through one crypto authority, one session vocabulary, and one tenancy contract. Every stateful obligation is a port Tag the data wave satisfies at app composition, so the folder imports only core.

## [01]-[DOMAIN_MAP]

```text codemap
security/
└── src/
    ├── crypt/                 # Crypto authority: signing, minting, shredding, custody, inbound verification
    │   ├── sign.ts            # The sole mint — every digest, signature, token, and envelope originates here
    │   ├── verify.ts          # Inbound-signature dialect table + one constant-time verify fold over HELD request octets
    │   └── secret.ts          # DopplerSDK leased-secret custody behind Layer.scoped — download, targeted read, name census
    ├── authn/                 # Authentication: session spine, digest credentials, OAuth, passkeys
    │   ├── session.ts         # The identity spine the ceremonies feed — rotation, ports, CSRF egress
    │   ├── credential.ts      # Digest — the one mint-and-resolve idiom over OTP, recovery codes, and machine API keys
    │   ├── oauth.ts           # Issuers modeled as rows over one authorization-code ceremony
    │   └── webauthn.ts        # Both passkey halves as per-runtime subpaths: RP verifier (./server) + browser invocation (./browser)
    └── access/                # Authorization: entitlement fold and the tenancy contract
        ├── claim.ts           # Entitlement vocabulary + the RBAC-union-ReBAC evaluation fold, resolved once per request
        └── tenant.ts          # Ambient TenantContext reference + the app.current_tenant RLS shape the data wave enforces
```

## [02]-[STRATA]

- S0 `crypt/sign` + `access/tenant` — two floor mints importing only core: `sign` originates every digest, signature, token, and envelope (`Crypto`, `Jwt`, `AccessClaims`, `Shredder`, `SealedEnvelope`); `tenant` mints the `TenantScope` reference and the RLS shape.
- S1 `crypt/verify` + `crypt/secret` + `authn/session` + `authn/credential` — each composes `sign` alone: `verify` folds `Crypto` over held octets, `secret` scopes the Doppler lease behind `Crypto`, `session` mints `Jwt` tokens as the identity spine, `credential` rides its private digest idiom over `Crypto`.
- S2 `authn/oauth` + `authn/webauthn` + `access/claim` — ceremonies and decisions over the spine: `oauth` and `webauthn` compose `Token` from `session` beside `sign`; `claim` folds `AccessClaims` with `TenantScope`; `authn` and `access` stay mutually independent peers.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Security interior import strata
    accDescr: Three interior waves — ceremonies and the entitlement fold over the verify, secret, session, and credential spine onto the sign and tenant floor — every import downward, labeled edges naming one sourced type each, and one forbidden upward edge styled red.
    subgraph S2["S2 CEREMONY + DECISION"]
        Ceremony["oauth · webauthn"]
        Claim[claim]
    end
    subgraph S1["S1 SPINE"]
        Verify[verify]
        Secret[secret]
        Session[session]
        Credential[credential]
    end
    subgraph S0["S0 FLOOR"]
        Sign[sign]
        TenantRef[tenant]
    end
    Verify e1@-->|"[IMPORT]: Crypto"| Sign
    Secret e7@-->|"[IMPORT]: Crypto"| Sign
    Session e2@-->|"[IMPORT]: Jwt"| Sign
    Credential e8@-->|"[IMPORT]: Crypto"| Sign
    Ceremony e3@-->|"[IMPORT]: Token"| Session
    Ceremony e4@-->|"[IMPORT]: SingleUse"| Sign
    Claim e5@-->|"[IMPORT]: AccessClaims"| Sign
    Claim e6@-->|"[IMPORT]: TenantScope"| TenantRef
    Session e9@-->|"[IMPORT]: Reject"| Verify
    Credential e10@-->|"[IMPORT]: Reject"| Verify
    Ceremony e11@-->|"[IMPORT]: Reject"| Verify
    S0 f1@-->|"forbidden: upward import"| S2
```

## [03]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Security package seam registry
    accDescr: Security sub-domain owners exchanging identity, custody, tenancy, and telemetry contracts with the core, data, runtime, and IaC packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph security[SECURITY]
        Crypt[Crypt authority]
        Authn[Authn spine]
        Access[Access fold]
    end
    Core([core])
    Data[(data)]
    Runtime{{runtime}}
    Iac{{iac}}
    Core e1@-->|"[SHAPE]: TenantContext"| Access
    Data e2@-->|"[PORT]: ClaimStore"| Access
    Access e3@-->|"[BOUNDARY]: TenantScope"| Data
    Data e4@-->|"[PORT]: SessionStore"| Authn
    Authn e5@-->|"[PORT]: BearerGuard"| Runtime
    Authn e6@<-->|"[BOUNDARY]: OAuth"| Runtime
    Authn e7@-->|"[SHAPE]: CookieSpec"| Runtime
    Crypt e8@-->|"[SHAPE]: SealedEnvelope"| Data
    Crypt e9@-->|"[BOUNDARY]: Intake"| Runtime
    Crypt e10@-->|"[BOUNDARY]: LeaseSpec"| Iac
    Access e11@-->|"[PORT]: FlagGate"| Runtime
    Core e12@-->|"[SHAPE]: Convention"| Crypt
    Access e13@-->|"[PROJECTION]: rasm.tenant"| Runtime
```

## [04]-[ORGANIZATION]

`crypt/sign` is the sole mint and `crypt/verify` its inbound mirror over held octets, so no route hand-rolls a signature check; `crypt/secret` scopes the Doppler client to the folder's leased surfaces. `authn/session` is the identity spine the ceremonies feed: `credential` funnels every second factor through one mint-and-resolve idiom, `oauth` models issuers as rows, `webauthn` splits the passkey ceremony by runtime subpath. `access` turns verified identity into decisions: `claim` evaluates entitlements once per request, `tenant` states the tenancy contract the data wave enforces as RLS.

## [05]-[BOUNDARIES]

- Persistence lives outside by construction: every store is a port Tag the data wave satisfies and the app root binds.
- Content-identity digesting stays core's; this folder owns secret derivation and authenticated crypto only.
- Cookie framing and CSRF are egress projections declared here and consumed by the runtime browser plane; no browser API is touched here.
- Tenancy is declared here and enforced in the data wave; the folder opens no database transaction.
- Flag evaluation is the `FlagGate` consumer port the runtime wave satisfies; the entitlement fold composes flag verdicts and owns no flag engine.

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
    └── access/                # Authorization: entitlement fold, tenancy contract, and the security fact rail
        ├── audit.ts           # SecurityFact vocabulary, Witness publish seam, AuditJournal port, pseudonymized egress, board projections
        ├── claim.ts           # Entitlement vocabulary + the RBAC-union-ReBAC evaluation fold, resolved once per request
        └── tenant.ts          # Ambient TenantContext reference + the app.current_tenant RLS shape the data wave enforces
```

## [02]-[STRATA]

- S0 `access/audit` + `access/tenant` — two floor mints importing only core: `audit` mints the security fact plane (`SecurityFact`, the silent `Witness` seam, the `AuditJournal` port, the pseudonymized egress and board projections); `tenant` mints the `TenantScope` reference and the RLS shape.
- S1 `crypt/sign` — the crypto authority originating every digest, signature, token, and envelope (`Crypto`, `Jwt`, `AccessClaims`, `Shredder`, `SealedEnvelope`), composing `Witness` from the fact floor so its shred-open and JWKS-quarantine arms publish facts.
- S2 `crypt/verify` + `crypt/secret` + `authn/session` + `authn/credential` — each composes `sign`: `verify` folds `Crypto` over held octets, `secret` scopes the Doppler lease behind `Crypto` and publishes rotation facts, `session` mints `Jwt` tokens as the identity spine and publishes reuse facts, `credential` rides its private digest idiom over `Crypto`.
- S3 `authn/oauth` + `authn/webauthn` + `access/claim` — ceremonies and decisions over the spine: `oauth` and `webauthn` compose `Token` from `session` beside `sign`, `webauthn` publishing clone and ceremony facts; `claim` folds `AccessClaims` with `TenantScope` and publishes deny facts; `authn` and `access` stay mutually independent peers.

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
    accDescr: Four interior waves — ceremonies and the entitlement fold over the verify, secret, session, and credential spine onto the sign authority and the audit and tenant floor — every import downward, labeled edges naming one sourced type each, and one forbidden upward edge styled red.
    subgraph S3["S3 CEREMONY + DECISION"]
        Ceremony["oauth · webauthn"]
        Claim[claim]
    end
    subgraph S2["S2 SPINE"]
        Verify[verify]
        Secret[secret]
        Session[session]
        Credential[credential]
    end
    subgraph S1["S1 CRYPTO AUTHORITY"]
        Sign[sign]
    end
    subgraph S0["S0 FLOOR"]
        Audit[audit]
        TenantRef[tenant]
    end
    Sign e12@-->|"[IMPORT]: Witness"| Audit
    Verify e1@-->|"[IMPORT]: Crypto"| Sign
    Secret e7@-->|"[IMPORT]: Crypto"| Sign
    Secret e13@-->|"[IMPORT]: Witness"| Audit
    Session e2@-->|"[IMPORT]: Jwt"| Sign
    Session e14@-->|"[IMPORT]: Witness"| Audit
    Credential e8@-->|"[IMPORT]: Crypto"| Sign
    Ceremony e3@-->|"[IMPORT]: Token"| Session
    Ceremony e4@-->|"[IMPORT]: SingleUse"| Sign
    Ceremony e15@-->|"[IMPORT]: Witness"| Audit
    Claim e5@-->|"[IMPORT]: AccessClaims"| Sign
    Claim e6@-->|"[IMPORT]: TenantScope"| TenantRef
    Claim e16@-->|"[IMPORT]: Witness"| Audit
    Session e9@-->|"[IMPORT]: Reject"| Verify
    Credential e10@-->|"[IMPORT]: Reject"| Verify
    Ceremony e11@-->|"[IMPORT]: Reject"| Verify
    S0 f1@-->|"forbidden: upward import"| S3
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
    Data e14@-->|"[PORT]: AuditJournal"| Access
```

## [04]-[ORGANIZATION]

`crypt/sign` is the sole mint and `crypt/verify` its inbound mirror over held octets, so no route hand-rolls a signature check; `crypt/secret` scopes the Doppler client to the folder's leased surfaces. `authn/session` is the identity spine the ceremonies feed: `credential` funnels every second factor through one mint-and-resolve idiom, `oauth` models issuers as rows, `webauthn` splits the passkey ceremony by runtime subpath. `access` turns verified identity into decisions and evidence: `claim` evaluates entitlements once per request, `tenant` states the tenancy contract the data wave enforces as RLS, and `audit` is the fact rail — every loud arm publishes a typed `SecurityFact` through the silent `Witness` seam, the class-routed lanes drain into the `AuditJournal` port, and the board, alert, snapshot, and analytics views are projections of one receipt plane.

## [05]-[BOUNDARIES]

- Persistence lives outside by construction: every store is a port Tag the data wave satisfies and the app root binds.
- Content-identity digesting stays core's; this folder owns secret derivation and authenticated crypto only.
- Cookie framing and CSRF are egress projections declared here and consumed by the runtime browser plane; no browser API is touched here.
- Tenancy is declared here and enforced in the data wave; the folder opens no database transaction.
- Flag evaluation is the `FlagGate` consumer port the runtime wave satisfies; the entitlement fold composes flag verdicts and owns no flag engine.
- Audit facts persist through the `AuditJournal` port the data wave satisfies on its journal spine; analytics egress leaves only as the `AuditTrace` projection under the keyed `Pseudonym` mask, and board/alert compilation rides the core-to-iac `DashboardModel`/`Alert.Spec` seams over the folder's declared objectives.

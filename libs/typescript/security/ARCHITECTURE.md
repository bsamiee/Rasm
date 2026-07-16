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

## [02]-[SEAMS]

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Security package seam registry
    accDescr: Security sub-domain owners exchanging identity, custody, and tenancy contracts with the core, data, runtime, and IaC packages, edge rails colored by kind and nodes classed by seam direction.
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
    Authn e6@<-->|"[BOUNDARY]: OAuth redirect"| Runtime
    Authn e7@<-->|"[SHAPE]: CookieSpec"| Runtime
    Crypt e8@-->|"[SHAPE]: Shredder envelope"| Data
    Crypt e9@-->|"[BOUNDARY]: Intake verify"| Runtime
    Crypt e10@-->|"[BOUNDARY]: leased env"| Iac
    Access e11@-->|"[PORT]: FlagGate"| Runtime
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Crypt,Authn,Access primary
    class Runtime external
    class Data data
    class Core,Iac annotation
    class e1,e2,e3,e4,e5,e6,e7,e8,e9,e10,e11 edgeControl
```

## [03]-[ORGANIZATION]

`crypt/sign` is the sole mint — every digest, signature, token, and envelope originates there; `crypt/verify` mirrors it inbound over held octets so no route hand-rolls a signature check; `crypt/secret` scopes the Doppler client to the leased surfaces the folder admits. `authn/session` is the identity spine the ceremonies feed: `credential` funnels every second factor through one mint-and-resolve idiom, `oauth` models issuers as rows, `webauthn` splits the passkey ceremony by runtime subpath. `access` turns verified identity into decisions: `claim` evaluates entitlements once per request, `tenant` states the tenancy contract the data wave enforces as row-level security.

## [04]-[BOUNDARIES]

- Persistence lives outside by construction: every store is a port Tag the data wave satisfies and the app root binds.
- Content-identity digesting stays core's; this folder owns secret derivation and authenticated crypto only.
- Cookie framing and CSRF are egress projections declared here and consumed by the runtime browser plane; no browser API is touched here.
- Tenancy is declared here and enforced in the data wave; the folder opens no database transaction.
- Flag evaluation is the `FlagGate` consumer port the runtime wave satisfies; the entitlement fold composes flag verdicts and owns no flag engine.

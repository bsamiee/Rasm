# [TS_IAC_ARCHITECTURE]

`iac` owns the plane-distinct deploy package outside the runtime graph: sub-domains `program`, `operate`, and `kube` meet through one `StackSpec` value, one arm-keyed dispatch, and one Automation-API ledger. Every runtime alignment is a mirrored deploy fact, never an import the runtime carries.

## [01]-[DOMAIN_MAP]

```text codemap
iac/
└── src/
    ├── program/          # Program shapes, arm dispatch, the Automation-API drive, and the bootstrap legs
    │   ├── spec.ts       # StackSpec — the one decoded deploy value an app supplies
    │   ├── provider.ts   # Capability-by-arm map and realizer over the shared k8s and docker estates
    │   ├── automation.ts # Sole executor — the Automation-API driver with resilience and the fleet verbs
    │   └── source.ts     # Source-control shells the Doppler mirror fills, plus the distribution leg
    ├── operate/          # Secrets, observability realization, policy, and the hosted control plane
    │   ├── secret.ts     # Doppler hierarchy, mirror fan-out, access RBAC, and the three-lane cert axis
    │   ├── observe.ts    # LGTM distribution and OTel collector compiled onto Grafana boards
    │   ├── policy.ts     # Guard policies, drift projection, and the in-cluster PKO reconcile loop
    │   └── cloud.ts      # Hosted control-plane twin set, gated on the cloud backend
    └── kube/             # K8s estate tiers realized on either plane
        ├── workload.ts   # One spec row realized as the full typed workload set with its _LIFE anchor
        ├── traffic.ts    # Gateway API edge with external-dns automation and the tunnel/WAF/vanity rows
        ├── data.ts       # Typed CNPG data plane — object store, NATS, backups, pooler, replication
        └── tenant.ts     # Isolation modes and the cross-stack platform seam
```

## [02]-[STRATA]

- S0 `program/spec` + `program/automation` — the co-base pair: `StackSpec`/`StackOutputs` decode the one deploy value, `DeployFault`/`RunReceipt` rail every run; the two compose mutually (spec reads `DeployFault`, automation reads `StackSpec`) and nothing sits below them.
- S1 `operate` + `program/source` — each composes the base alone: `secret`/`observe`/`cloud` read `Tier`, `policy` drives `Automation` receipts, `source` shells read `Tier`; no operate module imports an operate sibling.
- S2 `kube` — the estate tiers realized over `Tier` rows; `traffic` alone adds a type-only `Certs` read on `operate/secret`, its issue verb injected partially applied.
- S3 `program/provider` — the `_estate` composition sink: the capability-by-arm map pulls every kube and operate tier plus `StackOutputs` and `DeployFault`; nothing imports it.

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
flowchart TB
    accTitle: Iac interior import strata
    accDescr: Four interior waves — the provider composition sink over the kube estate over the operate plane onto the spec and automation co-base — every import downward, labeled edges naming one sourced type each, and one forbidden upward edge styled red.
    subgraph S3["S3 COMPOSITION SINK"]
        Provider[provider]
    end
    subgraph S2["S2 KUBE ESTATE"]
        Kube["workload · data · tenant"]
        Traffic[traffic]
    end
    subgraph S1["S1 OPERATE"]
        Operate["secret · observe · policy · cloud"]
        Source[source]
    end
    subgraph S0["S0 PROGRAM BASE"]
        Spec[spec]
        Automation[automation]
    end
    Operate e1@-->|"[IMPORT]: Tier"| Spec
    Operate e2@-->|"[IMPORT]: RunReceipt"| Automation
    Source e3@-->|"[IMPORT]: Tier"| Spec
    Kube e4@-->|"[IMPORT]: Tier"| Spec
    Traffic e5@-->|"[IMPORT]: Tier"| Spec
    Traffic e6@-->|"[IMPORT]: Certs"| Operate
    Provider e7@-->|"[IMPORT]: StackOutputs"| Spec
    Provider e8@-->|"[IMPORT]: DeployFault"| Automation
    Provider e9@-->|"[IMPORT]: Workload"| Kube
    Provider e10@-->|"[IMPORT]: Traffic"| Traffic
    Provider e11@-->|"[IMPORT]: Lgtm"| Operate
    S0 f1@-->|"forbidden: upward import"| S3
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    class Provider,Kube,Traffic,Operate,Source primary
    class Spec,Automation recessed
    class e1,e2,e3,e4,e5,e6,e7,e8,e9,e10,e11 edgeControl
    class f1 edgeError
```

## [03]-[SEAMS]

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
    accTitle: IaC package seam registry
    accDescr: IaC plane owners exchanging stack outputs, data-plane shapes, lifecycle posture, leased secrets, and observability projections with the runtime, data, security, and core packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph iac[IAC]
        Program[Program plane]
        Operate[Operate plane]
        Kube[Kube estate]
    end
    Runtime{{runtime}}
    Data[(data)]
    Security([security])
    Core([core])
    Program e1@-->|"[PORT]: StackOutputs"| Runtime
    Data e2@-->|"[SHAPE]: Pg.rows"| Kube
    Data e3@-->|"[BOUNDARY]: Tenancy.rls"| Kube
    Runtime e4@-->|"[BOUNDARY]: Fanout.jetstream"| Kube
    Runtime e5@-->|"[SHAPE]: Setting.life"| Kube
    Security e6@-->|"[BOUNDARY]: LeaseSpec"| Kube
    Core e7@-->|"[PROJECTION]: DashboardModel"| Operate
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Program,Operate,Kube primary
    class Runtime external
    class Data data
    class Security,Core recessed
    class e1,e2,e3,e4,e5,e6 edgeControl
    class e7 edgeExternal
```

## [04]-[ORGANIZATION]

One `StackSpec` decodes into an arm, and the arm realizer proves every spec coordinate on the `DeployFault` rail before minting a `PulumiFn` — a rejected coordinate never reaches a provider. `provider` holds the single `_estate` composition the metal bootstrap and the EKS escalation both feed, so k8s realization keeps one owner across planes beside the docker machine estate at container depth. `automation` is the sole executor; resilience, retry, and per-run budgets internalize there. Per-file wiring — the tier rows each realizer emits, the mirror fan-out, the reconcile loop — lives on the owning implementation pages.

## [05]-[BOUNDARIES]

- Nothing imports this package at runtime; values cross back only as typed stack outputs read from env at boot.
- iac applies DDL and extensions; data verifies at startup, runtime never mutates schema, so divergence fails closed, never a pulumi read-back.
- Object-engine vocabulary is `minio | ceph`; Garage carries no spelling, unable to honor an `If-None-Match: *` conditional put.
- Viewer transcoder assets ship with the app shell, byte-identical through the runtime asset rows; a foreign-CDN side-load is a CSP breach.
- No queue extension is provisioned; the data matrix carries none, and SKIP-LOCKED outbox statements with the runtime relay own the class.

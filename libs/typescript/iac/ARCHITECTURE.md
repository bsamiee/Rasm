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
    │   └── source.ts     # Source-control shells the Doppler mirror fills, with the distribution leg
    ├── operate/          # Secrets, observability realization, policy, and the hosted control plane
    │   ├── secret.ts     # Doppler hierarchy, mirror fan-out, access RBAC, and the three-lane cert axis
    │   ├── observe.ts    # Store-row metrics family, signal backends, collector ingest, dev estate, board compile
    │   ├── policy.ts     # Guard policies, drift projection, the evidence sink spine, and the in-cluster PKO reconcile loop
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
- S3 `program/provider` — the `_estate` composition sink: the capability-by-arm map pulls every kube and operate tier, `StackOutputs`, and `DeployFault`; nothing imports it.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Iac interior import strata
    accDescr: Four interior waves — the provider composition sink over the kube estate over the operate plane onto the spec and automation co-base — every import downward, labeled edges naming one sourced type each, and one edge marking the forbidden upward import.
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
    accTitle: IaC package seam registry
    accDescr: IaC plane owners exchanging stack outputs, data-plane shapes, lifecycle posture, leased secrets, served-asset identities, and observability projections with the runtime, data, security, core, and ui packages, one labeled edge per contract family.
    subgraph iac[IAC]
        Program[Program plane]
        Operate[Operate plane]
        Kube[Kube estate]
    end
    Runtime{{runtime}}
    Data[(data)]
    Security([security])
    Core([core])
    Ui([ui])
    Program e1@-->|"[PORT]: StackOutputs"| Runtime
    Data e2@-->|"[SHAPE]: Pg.rows"| Kube
    Data e3@-->|"[BOUNDARY]: Tenancy.rls"| Kube
    Runtime e4@-->|"[BOUNDARY]: Fanout.jetstream"| Kube
    Runtime e5@-->|"[SHAPE]: Setting.life"| Kube
    Security e6@-->|"[BOUNDARY]: LeaseSpec"| Kube
    Core e7@-->|"[PROJECTION]: DashboardModel"| Operate
    Core e8@-->|"[PROJECTION]: Alert.Spec"| Operate
    Core e9@-->|"[PROJECTION]: Slo.Objective"| Operate
    Runtime e10@-->|"[TRANSPORT]: Export.live"| Program
    Ui e11@-->|"[SHAPE]: ServedAsset.roster"| Program
```

## [04]-[ORGANIZATION]

One `StackSpec` decodes into an arm, and the arm realizer proves every spec coordinate on the `DeployFault` rail before minting a `PulumiFn` — a rejected coordinate never reaches a provider. `provider` holds the single `_estate` composition the metal bootstrap and the EKS escalation both feed, beside the docker machine estate at container depth. `automation` is the sole executor and internalizes resilience, retry, and per-run budgets. Per-file wiring — tier rows, mirror fan-out, the reconcile loop — lives on the owning pages.

## [05]-[BOUNDARIES]

- Nothing imports this package at runtime; values cross back only as typed stack outputs read from env at boot.
- iac applies DDL and extensions; data verifies at startup, runtime never mutates schema, so divergence fails closed, never a pulumi read-back.
- Object-engine vocabulary is `minio | ceph`; Garage carries no spelling, unable to honor an `If-None-Match: *` conditional put.
- Viewer transcoder and engine wasm assets ship same-origin with the app shell: the ui served-asset roster is the sole identity source, each row realized on the static-distribution plane at the content-addressed immutable path `assets/<digest>/<file>` and sealed through the `served` output plane the ui `codec-absent` gate arms on; a foreign-CDN side-load stays a CSP breach.
- No queue extension is provisioned; the data matrix carries none, and SKIP-LOCKED outbox statements with the runtime relay own the class.

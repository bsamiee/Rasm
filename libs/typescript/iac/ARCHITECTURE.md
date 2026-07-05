# [TS_IAC_ARCHITECTURE]

The domain map of `iac` — the plane-distinct deploy package outside the runtime graph. Three sub-domains (`program`, `operate`, `kube`) meet through one `StackSpec` value, one arm-keyed dispatch, and one Automation-API ledger; every runtime alignment is a mirrored deploy fact, never an import the runtime carries.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, camelCase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
iac/
└── src/
    ├── program/          # The program shapes, the arm dispatch, the Automation-API driver, and the bootstrap-axis legs
    │   ├── spec.ts       # StackSpec: the one decoded value — arm union, promotion tiers, capability profile, backend selector, typed StackOutputs plane
    │   ├── provider.ts   # Capability-by-arm _map + _ARMS realizer; _estate shared k8s composition (metal Bootstrap | EKS) beside the docker machine estate
    │   ├── automation.ts # Sole executor: LocalWorkspace programs, Stream-bridged ledger + RunReceipt, internalized retry/budget, fleet verbs, drift reconcile
    │   └── source.ts     # Source — github repo/branch-law/environment gates/deploy keys/webhook + the synced-folder distribution dialect record
    ├── operate/          # Secrets, observability realization, policy, the hosted control plane
    │   ├── secret.ts     # Doppler hierarchy + _MIRRORS fan-out + _ACCESS RBAC rows; Certs three-lane pipeline (self-signed mesh, ACME trusted, foreign pin)
    │   ├── observe.ts    # Lgtm (LGTM distribution + OTel collector) → Boards: grafana dashboards, alert/SLO compile, tenant orgs, machine identity
    │   ├── policy.ts     # Guard policies-as-data pre-apply, the DriftReport projection + fault-isolated sweep + conform read-back, the PKO Reconcile loop
    │   └── cloud.ts      # CloudPlane (pulumiservice schedules/settings/webhooks/RBAC/environments) + the EscApi Environments rail, gated on backend: cloud
    └── kube/             # The k8s estate tiers (either plane source)
        ├── workload.ts   # spec row → ServiceAccount/Role/RoleBinding + Deployment (spread, zone affinity) + PDB/HPA + Service, the env seam, the _LIFE anchor
        ├── traffic.ts    # Traffic — TLS sink, Gateway API edge (crd2pulumi) with Ingress fallback, external-dns, Edge tagged family, tunnel/WAF/vanity rows
        ├── data.ts       # ObjectStore + Nats rows; typed CNPG cluster (crd2pulumi), barman-cloud archive, Pooler, _TENANCY pg escalation, replication statics
        └── tenant.ts     # Tenants — Capsule Tenant CRs | vcluster planes via _MODES, the StackReference platform seam
```

## [02]-[SEAMS]

```text seams
program/spec    →  typescript:runtime/work     # [PORT]: StackOutputs.sharding → ShardingConfig.layerFromEnv
kube/data       ←  typescript:data/lane        # [SHAPE]: Pg.image/Pg.rows extension roster priced into the CNPG image
kube/data       ←  typescript:data/lane        # [BOUNDARY]: Tenancy.rls ensure roster applied by the in-cluster provision job
kube/data       ←  typescript:runtime/net      # [BOUNDARY]: JetStream server posture (websocket, fsync-per-write, quorum) Setting.fanout.origin dial reaches
kube/workload   ←  typescript:runtime/proc     # [SHAPE]: Setting.life.drain + probe routes mirrored as the _LIFE anchor
kube/workload   ←  typescript:security/crypt   # [BOUNDARY]: doppler-run leased env injection at the entrypoint wrap
operate/observe ←  typescript:core/observe     # [PROJECTION]: DashboardModel.Encoded + Alert.Spec + Slo.Objective realized as grafana rows
kube/tenant     ←  typescript:data/lane        # [SHAPE]: Tenancy locus vocabulary the pgTier escalation and RLS ensure rows read
```

## [03]-[ORGANIZATION]

`program` owns the shapes, the drive, and the bootstrap-axis legs: `spec` is the one decoded value an app supplies (arm, tiers, profile with tenancy and compute axes, backend, coordinates, Doppler ref, rotation epoch) and the typed outputs plane with the one `pairsOf` channel-flatten; `provider` keeps the capability-by-arm data and the arm realizer on one page, proves every spec coordinate on the `DeployFault` rail before any `PulumiFn`, and holds `_estate` — the single k8s-estate composition both the metal bootstrap and the EKS escalation feed — beside the docker machine estate that realizes the realized arm's full column at container depth; `automation` is the only executor with resilience internalized (stream-bridged events, jittered retry on state locks, per-run budgets) plus the fleet verbs; `source` provisions the source-control shells the Doppler mirror fills and the static-distribution leg. `operate` mints, realizes, and governs: `secret` is the material owner with the mirror fan-out, access RBAC rows, and the three-lane cert axis; `observe` compiles the core suite's boards, alerts, and SLOs onto Grafana; `policy` judges before apply, projects drift after, and runs the in-cluster PKO reconcile loop; `cloud` is the hosted twin set gated on `backend: cloud`. `kube` realizes the estate on either plane: `workload` turns one spec row into the full typed set including its RBAC, budget, and autoscale rows; `traffic` fronts through the Gateway API with external-dns automation and the tunnel/WAF/vanity rows; `data` lands the typed CNPG plane — plugin-archived backups, pooler bind, tenancy escalation, replication seam; `tenant` realizes the isolation modes and the cross-stack platform seam.

## [04]-[BOUNDARIES]

- Nothing imports this package at runtime; values cross back only as typed stack outputs read from env at boot.
- iac applies DDL and extension rosters at provision; the data wave verifies at startup; the runtime never mutates schema. Out-of-band DB divergence is the data plane's fail-closed startup verify, never a pulumi read-back.
- The object-engine vocabulary is `minio | ceph` (pgsty/minio continuation image, Ceph RGW); Garage has no spelling to select — it cannot honor `If-None-Match: *` conditional put.
- The self-hosted transcoder assets the viewer pins (draco/basis/meshopt) ship with the app shell and serve byte-identical through the runtime asset rows; a foreign-CDN side-load is a CSP breach.
- No queue extension is provisioned: the data matrix carries none; SKIP-LOCKED outbox statements and the runtime relay own the class.

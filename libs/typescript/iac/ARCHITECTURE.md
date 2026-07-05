# [TS_IAC_ARCHITECTURE]

The domain map of `iac` — the plane-distinct deploy package outside the runtime graph. Three sub-domains (`program`, `operate`, `kube`) meet through one `StackSpec` value, one arm-keyed dispatch, and one Automation-API ledger; every runtime alignment is a mirrored deploy fact, never an import the runtime carries.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, camelCase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
iac/
└── src/
    ├── program/          # The program shapes, the arm dispatch, and the Automation-API driver
    │   ├── spec.ts       # StackSpec: Closed arm union with promotion tiers, capability profile, project/account coordinates, and StackOutputs
    │   ├── provider.ts   # The _map equivalence table + the _ARMS handler record, both keyed on StackSpec.Arm; the Bootstrap tier
    │   └── automation.ts # LocalWorkspace.createOrSelectStack inline programs, the run ledger + RunReceipt fold, Automation.reconcile drift leg
    ├── operate/          # Secrets, observability realization, policy
    │   ├── secret.ts     # Doppler hierarchy provisioning + ACME TLS issuance — the two mints other rows only reference
    │   ├── observe.ts    # Lgtm (LGTM distribution + OTel collector charts) → Boards (grafana provider, dashboards, alert rows)
    │   └── policy.ts     # Guard policies-as-data pre-apply + the DriftReport projection and sweep
    └── kube/             # The selfhosted-k8s arm tiers
        ├── workload.ts   # spec row → ServiceAccount/Deployment/Service, the env seam (_KEYS, doppler-run entrypoint), the _LIFE anchor
        ├── traffic.ts    # Traffic — TLS secret sink, Ingress, default-deny NetworkPolicy, the shared _connector projection and tunnel row
        └── data.ts       # ObjectStore (conforming engines: pgsty/minio continuation, Ceph RGW), Nats JetStream server row, the CNPG Postgres row + app finalization
```

## [02]-[SEAMS]

```text seams
program/spec    →  typescript:runtime/work     # [PORT]: StackOutputs.sharding → ShardingConfig.layerFromEnv
kube/data       ←  typescript:data/lane        # [SHAPE]: Pg.image/Pg.rows extension roster priced into the CNPG image
kube/data       ←  typescript:data/lane        # [BOUNDARY]: Tenancy.rls ensure roster applied by the in-cluster provision job
kube/data       ←  typescript:runtime/net      # [BOUNDARY]: JetStream server posture (websocket, fsync-per-write, quorum) the Setting.fanout.origin dial reaches
kube/workload   ←  typescript:runtime/proc     # [SHAPE]: Setting.life.drain + probe routes mirrored as the _LIFE anchor
kube/workload   ←  typescript:security/crypt   # [BOUNDARY]: doppler-run leased env injection at the entrypoint wrap
operate/observe ←  typescript:core/observe     # [PROJECTION]: DashboardModel.Encoded + Alert.Spec realized as grafana rows
```

## [03]-[ORGANIZATION]

`program` owns the shapes and the drive: `spec` is the one decoded value an app supplies (arm, tiers, profile, coordinates, Doppler ref, rotation epoch) and the typed outputs plane; `provider` keeps the capability-by-arm data and the arm realizer on one page so they cannot drift — prepared arms (Cloud SQL, GCS, R2, Workers) stay `_map` rows an app's `StackSpec` instantiates, with no declared code standing for them; `automation` is the only executor, every workspace fact an Effect `Config` read, drift reconciliation the ledger's read-only fifth op. `operate` mints and realizes: `secret` is the material owner both other tiers reference, `observe` produces exactly what its own second tier consumes, `policy` judges before apply and projects drift after. `kube` realizes the self-hosted arm: `workload` turns one spec row into the full typed resource set, `traffic` fences the namespace and carries the tunnel row through the shared `_connector` projection, `data` lands the whole data seam — extension matrix into the CNPG image, ensure-DDL at provision, the JetStream and object-store rows.

## [04]-[BOUNDARIES]

- Nothing imports this package at runtime; values cross back only as typed stack outputs read from env at boot.
- iac applies DDL and extension rosters at provision; the data wave verifies at startup; the runtime never mutates schema. Out-of-band DB divergence is the data plane's fail-closed startup verify, never a pulumi read-back.
- The object-engine vocabulary is `minio | ceph` (pgsty/minio continuation image, Ceph RGW); Garage has no spelling to select — it cannot honor `If-None-Match: *` conditional put.
- The self-hosted transcoder assets the viewer pins (draco/basis/meshopt) ship with the app shell and serve byte-identical through the runtime asset rows; a foreign-CDN side-load is a CSP breach.
- No queue extension is provisioned: the data matrix carries none; SKIP-LOCKED outbox statements and the runtime relay own the class.

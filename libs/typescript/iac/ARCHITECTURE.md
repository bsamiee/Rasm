# [IAC_ARCHITECTURE]

The domain map of `iac` — the deploy plane of the TypeScript branch. Seven sub-domains realize deployment topology as lib code with zero authored YAML: `program` the Automation-API entry and the `StackSpec` vocabulary, `stack` the ComponentResource tiers and typed outputs, `provider` the one closed dispatch and its per-arm surfaces, `kube` the workload/data/traffic rows of the first-class `selfhosted-k8s` arm, `secret` the Doppler-canonical provisioning and injection law, `observe` the LGTM stack and the grafana apply, `policy` the CrossGuard packs and the drift fold.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
iac/src/ # plane:deploy — imports kernel, store (capability vocabulary), telemetry (board functions); depended on by nothing at runtime
├── program/           # The Automation-API entry and the app-supplied spec vocabulary
│   ├── automation.ts  # Automation-API inline programs (LocalWorkspace.createOrSelectStack); the one CLI-on-PATH wrap; typed run receipts (up | preview | refresh | destroy ledger)
│   └── spec.ts        # StackSpec vocabulary: target arm, capability profile, region/domain, Doppler project ref
├── stack/             # ComponentResource tiers and the typed stack outputs
│   ├── component.ts   # ComponentResource tiers
│   └── output.ts      # typed StackOutputs; StackOutputs -> ShardingConfig is the SOLE iac/work meeting seam
├── provider/          # The one closed provider dispatch and its per-arm surfaces
│   ├── dispatch.ts    # closed Match.exhaustive dispatch: selfhosted-k8s | selfhosted-docker | aws | gcp | cloudflare
│   └── surface.ts     # per-arm service surface rows + the service-equivalence map; selfhosted-k8s includes the cluster-bootstrap row (@pulumi/command over owned metal/VPS)
├── kube/              # The selfhosted-k8s workload, data, and traffic rows
│   ├── workload.ts    # typed @pulumi/kubernetes workloads
│   ├── data.ts        # CNPG operator row provisioning the PG18.4-extension image that realizes store/capability; scheduled-backup + PITR rows to the object-store row
│   └── traffic.ts     # cert/dns/ingress rows
├── secret/            # Doppler-canonical secret provisioning and runtime injection
│   ├── doppler.ts     # @pulumiverse/doppler project/config provisioning
│   └── inject.ts      # doppler-run injection law; ESC as a prepared row
├── observe/           # The LGTM observability stack and the grafana apply
│   ├── stack.ts       # LGTM + the OTel collector row via helm.v4 typed values — upstream charts as typed objects, zero authored YAML
│   └── apply.ts       # telemetry/board dashboard/alert functions applied through @pulumiverse/grafana
└── policy/            # CrossGuard packs and the drift fold
    ├── guard.ts       # CrossGuard packs
    └── drift.ts       # previewRefresh drift fold over OpType
```

`program` is the spine: an app supplies a `StackSpec` VALUE, `provider/dispatch` selects the arm over `Match.exhaustive`, the arm composes the `stack` ComponentResource tiers plus the `kube`/`secret`/`observe` rows its `surface` column names, and `program/automation` runs the inline program through `LocalWorkspace.createOrSelectStack`, returning the typed run receipt; `policy` gates every run with the CrossGuard packs and folds `previewRefresh` drift over `OpType`. Growth is row-shaped everywhere: a new cloud is one `dispatch` arm + one `surface` column, a new operator is one `kube` row, a new policy pack is one `guard` row.

## [02]-[SEAMS]

```text seams
stack/output  → typescript:work/engine       # [SHAPE]: typed StackOutputs → ShardingConfig — the sole iac/work meeting seam, the one value crossing back into the runtime graph
kube/data     ← typescript:store/capability  # [SHAPE]: the PG 18.4 extension matrix realized as CNPG image facts; iac applies the declarative DDL at provision time, store verifies at startup
observe/apply ← typescript:telemetry/board   # [PROJECTION]: AppIdentity-derived dashboard/alert outputs applied through @pulumiverse/grafana
```

None inbound from C# — deployment topology is TS-native, and no Python seam exists. `plane:deploy` is depended on by nothing at runtime; every ledger edge points downward (`kernel`, the `store` capability vocabulary, the `telemetry` board functions), and typed StackOutputs → `ShardingConfig` is the sole value crossing back to `work`. `secret/inject` meets the `security/secret` read path at the process boundary only — `doppler run` environment injection, never an import in either direction.

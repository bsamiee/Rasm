# [IAC]

`iac` is the W4 deploy plane of the TypeScript branch — deployment topology as lib code, with zero authored YAML anywhere. Programs are Pulumi Automation-API inline typed programs (`LocalWorkspace.createOrSelectStack`, no `Pulumi.yaml`; the pulumi CLI-binary-on-PATH deploy-host fact wrapped once in `program`, every run returning a typed receipt over the up | preview | refresh | destroy ledger). Provider selection is ONE closed `Match.exhaustive` dispatch — `selfhosted-k8s` first-class (typed `@pulumi/kubernetes` workloads, the CNPG operator row provisioning the PG18.4-extension image that realizes the `store/capability` matrix with scheduled-backup + PITR rows to the object-store row, the MinIO-vs-Garage object row, cert/dns/ingress rows, and the cluster-bootstrap row over `@pulumi/command` on owned metal/VPS), `selfhosted-docker`, and `aws`/`gcp`/`cloudflare` as prepared rows carrying the service-equivalence map. An app finalizes a cloud row by supplying a `StackSpec` VALUE (target arm, capability profile, region/domain, Doppler project ref): adding a provider is one new dispatch arm in the lib; finalizing one is app data, never a lib edit, never a fork. Secrets are Doppler-canonical — `@pulumiverse/doppler` project/config provisioning, `doppler run` runtime injection meeting the `security/secret` read path, ESC demoted to a prepared row. Observability is the LGTM stack + the OTel collector row through `helm.v4` typed values (upstream charts as typed objects), with `telemetry/board` dashboard/alert outputs applied through `@pulumiverse/grafana`. `policy` owns the CrossGuard packs and the `previewRefresh` drift fold over `OpType`. `plane:deploy` — depended on by nothing at runtime; the ledger edges point downward (`kernel`, the `store` capability vocabulary, the `telemetry` board functions) and typed StackOutputs → `ShardingConfig` is the sole value crossing to `work`. The domain map and seam record live in `ARCHITECTURE.md`, the forward pool in `IDEAS.md`, the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[AUTOMATION](.planning/program/automation.md): the Automation-API inline programs over `LocalWorkspace.createOrSelectStack` — no `Pulumi.yaml`, the one CLI-on-PATH wrap, typed run receipts over the up | preview | refresh | destroy ledger.
- [02]-[SPEC](.planning/program/spec.md): the `StackSpec` VALUE vocabulary an app supplies — target arm, capability profile, region/domain, Doppler project ref; finalizing a prepared cloud row is app data.
- [03]-[COMPONENT](.planning/stack/component.md): the ComponentResource tiers.
- [04]-[OUTPUT](.planning/stack/output.md): typed StackOutputs; StackOutputs → `ShardingConfig` is the sole meeting seam of the iac/work altitudes.
- [05]-[DISPATCH](.planning/provider/dispatch.md): the one closed `Match.exhaustive` provider dispatch — `selfhosted-k8s` | `selfhosted-docker` | `aws` | `gcp` | `cloudflare`; a new cloud is one new arm.
- [06]-[SURFACE](.planning/provider/surface.md): per-arm service surface rows and the service-equivalence map; the `selfhosted-k8s` arm carries the cluster-bootstrap row (`@pulumi/command` over owned metal/VPS).
- [07]-[WORKLOAD](.planning/kube/workload.md): typed `@pulumi/kubernetes` workloads.
- [08]-[DATA](.planning/kube/data.md): the CNPG operator row provisioning the PG18.4-extension image that realizes `store/capability`, with scheduled-backup + PITR rows to the object-store row and the MinIO-vs-Garage object row.
- [09]-[TRAFFIC](.planning/kube/traffic.md): the cert/dns/ingress rows.
- [10]-[DOPPLER](.planning/secret/doppler.md): `@pulumiverse/doppler` project/config provisioning — Doppler is the canonical secret owner.
- [11]-[INJECT](.planning/secret/inject.md): the `doppler run` runtime-injection law meeting the `security/secret` read path; ESC as a prepared row.
- [12]-[STACK](.planning/observe/stack.md): the LGTM stack + the OTel collector row via `helm.v4` typed values — upstream charts as typed objects, zero authored YAML.
- [13]-[APPLY](.planning/observe/apply.md): `telemetry/board` dashboard/alert functions applied through `@pulumiverse/grafana`.
- [14]-[GUARD](.planning/policy/guard.md): the CrossGuard policy packs.
- [15]-[DRIFT](.planning/policy/drift.md): the `previewRefresh` drift fold over `OpType`.

## [2]-[DOMAIN_PACKAGES]

The `# iac` owner group in the `pnpm-workspace.yaml` catalog, folder-local and catalogued at `.api/`; versions live only in the catalog. The generated ban list scopes `@pulumi/*` and `@pulumiverse/*` to this folder alone.

[ENGINE]:
- `@pulumi/pulumi` — the Automation API: `LocalWorkspace` inline programs, the `Output`/`ComponentResource` model, typed run events feeding the receipt ledger.

[KUBERNETES]:
- `@pulumi/kubernetes` — typed workloads, `helm.v4` typed chart values, and the operator rows of the first-class `selfhosted-k8s` arm.

[CLOUD_ROWS]:
- `@pulumi/aws` — the prepared `aws` row.
- `@pulumi/awsx` — higher-level AWS compositions backing the prepared `aws` row.
- `@pulumi/gcp` — the prepared `gcp` row.
- `@pulumi/cloudflare` — the prepared `cloudflare` row.

[DATA_PROVISIONING]:
- `@pulumi/postgresql` — per-app database/schema provisioning against the CNPG-provisioned cluster.

[FABRIC]:
- `@pulumi/command` — the cluster-bootstrap row over owned metal/VPS.
- `@pulumi/docker` — the `selfhosted-docker` arm runtime (container/network/volume + swarm) and registry/tag/data-source resources.
- `@pulumi/docker-build` — the canonical buildx-native image build owner (multi-platform, pluggable cache/export, by-value secrets); supersedes the legacy `@pulumi/docker.Image` and is the builder `awsx.ecr.Image` bundles on the `aws` arm.
- `@pulumi/random` — provider-tracked random suffix/password material.
- `@pulumi/tls` — certificate/key material for the `kube/traffic` rows.

[SECRETS]:
- `@pulumiverse/doppler` — Doppler project/config provisioning; the canonical secret owner.

[OBSERVABILITY]:
- `@pulumiverse/grafana` — applies the `telemetry/board` dashboard/alert outputs.

[POLICY]:
- `@pulumi/policy` — the CrossGuard policy packs.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate this folder composes; the registry lives in `libs/typescript/.planning/README.md`, the catalogue at `libs/typescript/.api/`.

- `effect` — the `Match.exhaustive` provider dispatch, `Layer` program composition, `Schema` StackSpec/StackOutputs vocabulary, and the typed run-receipt rails.

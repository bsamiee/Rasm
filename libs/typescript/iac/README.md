# [TS_IAC]

`iac` is the estate's deploy plane: one decoded `StackSpec` becomes a capability-admitted deployment through typed Pulumi programs driven by the Automation API, with no `Pulumi.yaml` anywhere.

## [01]-[ROUTER]

- [01]-[PROGRAM](.planning/program/): `StackSpec` decode and the sole Automation-API executor; every coordinate proves on the `DeployFault` rail.
- [02]-[OPERATE](.planning/operate/): Secret custody, observability, policy, and backend convergence over generated projections.
- [03]-[KUBE](.planning/kube/): K8s workload roles, data targets, traffic, and tenancy; every workload remains digest-pinned.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[ENGINE]:
- `@pulumi/pulumi`
- `@pulumi/policy`
- `@pulumi/pulumiservice`
- `@pulumi/esc-sdk`

[PROVIDERS]:
- `@pulumi/kubernetes`
- `@pulumi/eks`
- `@pulumi/aws`
- `@pulumi/awsx`
- `@pulumi/gcp`
- `@pulumi/cloudflare`
- `@pulumi/docker`
- `@pulumi/docker-build`
- `@pulumi/command`
- `@pulumi/cloudinit`
- `@pulumi/synced-folder`

[MATERIAL]:
- `@pulumi/tls`
- `@pulumi/random`
- `@pulumiverse/acme`
- `@pulumiverse/doppler`

[REALIZATION]:
- `@pulumiverse/grafana`
- `@grafana/grafana-foundation-sdk` — typed dashboard builders compiling the core board model into the JSON the provider applies.
- `@pulumi/postgresql`
- `@pulumi/github`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Ts registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`

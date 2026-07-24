# [TS_IAC]

`iac` is the estate's deploy plane: one decoded `StackSpec` becomes a fully realized multi-cloud deployment through typed Pulumi programs driven by the Automation API, with no `Pulumi.yaml` anywhere. Every coordinate proves on a typed fault rail before a program is entered, so no tier constructor meets an unproven value.

Every cloud, capability, credential, tenancy tier, and injected env fact is a row on its owning surface — promoting a metal cluster to a managed estate is one provider seam swap, and finalizing a cloud is a spec value, never a lib edit. Coordinates are publishable and material never is: the output gate refuses any secret-flagged value, and the one secret source of truth reaches external stores only as mirrors. Deploy and drift evidence share one receipt vocabulary, so drift is pure projection and cannot fork.

Nothing depends on this plane at runtime; its outputs cross back only as typed env facts read at boot. It applies schema and extension rosters at provision while the data stratum verifies fail-closed at startup, and board content, probe grading, and retention semantics belong to their owning planes, arriving here only as encoded data.

## [01]-[ROUTER]

- [01]-[PROGRAM](.planning/program/): `StackSpec` decode and the sole Automation-API executor; every coordinate proves on the `DeployFault` rail.
- [02]-[OPERATE](.planning/operate/): Doppler-sole secret custody mirroring outward; observability realization and the guard-policy plane.
- [03]-[KUBE](.planning/kube/): K8s estate tiers realized on either plane — every workload digest-pinned, tenancy an isolation ladder.

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
- `@grafana/grafana-foundation-sdk` — typed dashboard builders compiling the core board model into the JSON the provider applies
- `@pulumi/postgresql`
- `@pulumi/github`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Ts registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`
- `@effect/platform`
- `@effect/platform-node`

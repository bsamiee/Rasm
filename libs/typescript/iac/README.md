# [TS_IAC]

`iac` is the estate's deploy plane: one decoded `StackSpec` becomes a capability-admitted deployment through typed Pulumi programs driven by the Automation API, with no `Pulumi.yaml` anywhere.

## [01]-[ROUTER]

[PROGRAM]:
- [01]-[SPEC](.planning/program/spec.md): Decoded deploy value — arm union, consumption rosters, capability profile; coordinates, never material.
- [02]-[PROVIDER](.planning/program/provider.md): One dispatch keyed on the arm union — audit surface and realizer never drift apart.
- [03]-[AUTOMATION](.planning/program/automation.md): Inline typed programs with no `Pulumi.yaml` anywhere; every workspace fact arrives as data.
- [04]-[SOURCE](.planning/program/source.md): Bootstrap source-control shell — repository law, environment gates, deploy keys, distribution.

[OPERATE]:
- [05]-[SECRET](.planning/operate/secret.md): Deploy-plane material — provisioned secret hierarchy, epoch-bound credentials, certificates.
- [06]-[OBSERVE](.planning/operate/observe.md): Observability realization — selected store rows, one collector seam, the dev estate, compiled boards.
- [07]-[POLICY](.planning/operate/policy.md): Policy-plane verdicts — guard before apply, drift as projection, in-cluster reconcile.
- [08]-[CONVERGE](.planning/operate/converge.md): Cutover as data — immutable generation construction, hydration, proof, and retention.
- [09]-[CLOUD](.planning/operate/cloud.md): Hosted control-plane twins gated on the cloud backend — settings, schedules, RBAC, drift webhook.

[KUBE]:
- [10]-[WORKLOAD](.planning/kube/workload.md): Service and worker roles lowered from one spec row into typed pod, sizing, and hardening cells.
- [11]-[TRAFFIC](.planning/kube/traffic.md): Network edge — certificate sink, Gateway API fronting, tunnel, WAF, and vanity rows.
- [12]-[DATA](.planning/kube/data.md): K8s durability targets — conforming object engines, the JetStream door, CNPG postgres.
- [13]-[TENANT](.planning/kube/tenant.md): Tenant isolation realized per separation mode through one handler record.

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
- `@grafana/grafana-foundation-sdk` — Typed dashboard builders compiling the core board model into the JSON the provider applies.
- `@pulumi/postgresql`
- `@pulumi/github`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the TypeScript registry, whose charters own the full contracts; `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`

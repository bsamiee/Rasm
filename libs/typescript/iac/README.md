# [TS_IAC]

`libs/typescript/iac` owns the deploy plane: Pulumi typed programs driven through the Automation API with no `Pulumi.yaml`, one arm-dispatched provider realizing the `StackSpec` an app supplies across the shared k8s estate, the docker machine estate, and the workload/traffic/data/tenant tiers, with secret provisioning, observability, three-direction policy, and the Pulumi Cloud control plane gated on the spec backend. Nothing depends on this plane at runtime; its outputs cross back only as typed env facts.

## [01]-[ROUTER]

[PROGRAM]:
- [01]-[SPEC](.planning/program/spec.md): One decoded `StackSpec` deploy value an app supplies — arm, tiers, profile, backend, coordinates.
- [02]-[PROVIDER](.planning/program/provider.md): Capability-by-arm map and realizer over the shared k8s and docker estates.
- [03]-[AUTOMATION](.planning/program/automation.md): Sole Automation-API executor — resilience, retry, per-run budgets, and fleet verbs.
- [04]-[SOURCE](.planning/program/source.md): Source-control shells the Doppler mirror fills, plus the static-distribution leg.

[OPERATE]:
- [05]-[SECRET](.planning/operate/secret.md): Doppler material owner — mirror fan-out, access RBAC rows, three-lane cert axis.
- [06]-[OBSERVE](.planning/operate/observe.md): LGTM distribution and OTel collector compiled onto Grafana boards, alerts, and SLOs.
- [07]-[POLICY](.planning/operate/policy.md): Guard verdicts before apply, drift projection after, in-cluster PKO reconcile loop.
- [08]-[CLOUD](.planning/operate/cloud.md): Hosted control-plane twin set, gated on `backend: cloud`.

[KUBE]:
- [09]-[WORKLOAD](.planning/kube/workload.md): One spec row realized as the full typed workload set with its `_LIFE` anchor.
- [10]-[TRAFFIC](.planning/kube/traffic.md): Gateway API edge with external-dns automation and the tunnel/WAF/vanity rows.
- [11]-[DATA](.planning/kube/data.md): Typed CNPG data plane — object store, NATS, backups, pooler, and replication seam.
- [12]-[TENANT](.planning/kube/tenant.md): Isolation modes and the cross-stack platform seam.

## [02]-[DOMAIN_PACKAGES]

Deploy-plane libraries admitted by this folder; `pnpm-workspace.yaml` centralizes versions and the adjacent `.api/` folder holds the API evidence.

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
- `@pulumi/postgresql`
- `@pulumi/github`

## [03]-[SUBSTRATE_PACKAGES]

Shared TypeScript substrate consumed from the registry; `libs/typescript/.planning/README.md` and the sibling `libs/typescript/.api/` folder own the full contracts and evidence.

[TYPING_RAILS]:
- `effect`

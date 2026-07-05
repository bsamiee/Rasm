# [TS_IAC]

`libs/typescript/iac` is the deploy plane of the branch: Pulumi typed programs driven through the Automation API with no `Pulumi.yaml`, one arm-dispatched provider surface over the `StackSpec` value an app supplies, the `selfhosted-k8s` arm's workload/traffic/data tiers, secret provisioning and TLS issuance, observability realization, and policy in both verdict directions. The plane is depended on by nothing at runtime; its outputs cross back only as typed env facts. `ARCHITECTURE.md` carries the domain map and seams, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[SPEC](.planning/program/spec.md)
- [02]-[PROVIDER](.planning/program/provider.md)
- [03]-[AUTOMATION](.planning/program/automation.md)
- [04]-[SECRET](.planning/operate/secret.md)
- [05]-[WORKLOAD](.planning/kube/workload.md)
- [06]-[TRAFFIC](.planning/kube/traffic.md)
- [07]-[DATA](.planning/kube/data.md)
- [08]-[OBSERVE](.planning/operate/observe.md)
- [09]-[POLICY](.planning/operate/policy.md)

## [02]-[DOMAIN_PACKAGES]

Every folder-specific external library, planned or implemented. Versions are centralized in `pnpm-workspace.yaml`; corroborating API evidence lives in the adjacent `.api/` folder.

[ENGINE]:
- `@pulumi/pulumi`
- `@pulumi/policy`
- `@pulumi/pulumiservice`
- `@pulumi/esc-sdk`

[KUBERNETES]:
- `@pulumi/kubernetes`
- `@pulumi/eks`

[CLOUD_PROVIDERS]:
- `@pulumi/aws`
- `@pulumi/awsx`
- `@pulumi/gcp`
- `@pulumi/cloudflare`

[MACHINE]:
- `@pulumi/docker`
- `@pulumi/docker-build`
- `@pulumi/command`
- `@pulumi/cloudinit`
- `@pulumi/synced-folder`

[IDENTITY_MATERIAL]:
- `@pulumi/tls`
- `@pulumi/random`
- `@pulumiverse/acme`
- `@pulumiverse/doppler`

[REALIZATION]:
- `@pulumiverse/grafana`
- `@pulumi/postgresql`
- `@pulumi/github`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting TypeScript substrate this folder consumes; canonical registry and charters live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` folder.

[TYPING_RAILS]:
- `effect`

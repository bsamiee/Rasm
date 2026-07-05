# [TS_IAC]

`libs/typescript/iac` is the deploy plane of the branch: Pulumi typed programs driven through the Automation API with no `Pulumi.yaml`, one arm-dispatched provider surface over the `StackSpec` value an app supplies, one shared k8s-estate composition riding either the metal bootstrap or the managed EKS plane beside the docker machine estate realizing the same column at container depth, the workload/traffic/data/tenant tiers, secret provisioning with the three-lane cert axis, source-control and static-distribution legs, observability realization with the alert/SLO compile, policy in three verdict directions (pre-apply guard, drift projection, in-cluster reconcile), and the Pulumi Cloud control plane gated on the spec's backend. The plane is depended on by nothing at runtime; its outputs cross back only as typed env facts. `ARCHITECTURE.md` carries the domain map and seams, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[SPEC](.planning/program/spec.md)
- [02]-[PROVIDER](.planning/program/provider.md)
- [03]-[AUTOMATION](.planning/program/automation.md)
- [04]-[SOURCE](.planning/program/source.md)
- [05]-[SECRET](.planning/operate/secret.md)
- [06]-[WORKLOAD](.planning/kube/workload.md)
- [07]-[TRAFFIC](.planning/kube/traffic.md)
- [08]-[DATA](.planning/kube/data.md)
- [09]-[TENANT](.planning/kube/tenant.md)
- [10]-[OBSERVE](.planning/operate/observe.md)
- [11]-[POLICY](.planning/operate/policy.md)
- [12]-[CLOUD](.planning/operate/cloud.md)

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

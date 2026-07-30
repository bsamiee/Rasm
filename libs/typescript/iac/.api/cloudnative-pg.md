# [TS_IAC_API_CLOUDNATIVE_PG]

`cloudnative-pg` installs the PostgreSQL operator and its CRD estate; every cluster, database, pooler, backup, and replication object is a custom resource authored beside the chart. Chart values decide the controller's reach, webhook posture, and configuration ConfigMap — nothing here creates a database. Its webhook Service name is HARDCODED and survives every override, which is the one address a fence must not derive.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudnative-pg`
- chart: `cloudnative-pg` from `https://cloudnative-pg.github.io/charts` (Apache-2.0), chart and `appVersion` versioned independently
- asset: the controller Deployment, the webhook Service, a ServiceAccount, both RBAC pairs, the operator configuration ConfigMap or Secret, the mutating and validating webhook configurations, the monitoring queries ConfigMap, and the CRDs
- plane: `plane:deploy` — rendered by `@pulumi/kubernetes` `helm.v4.Chart`, depended on by nothing at runtime
- rail: deployment / relational plane

## [02]-[CHART_VALUES]

| [INDEX] | [KEY]                                     | [CAPABILITY]                                                                         |
| :-----: | :---------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `crds.create`                             | `boolean` DEFAULT TRUE — whether the CRDs ride the chart                             |
|  [02]   | `config.clusterWide`                      | `boolean` DEFAULT TRUE — cluster-wide observation versus the install namespace alone |
|  [03]   | `config.*` carrier                        | the operator configuration carrier and its content                                   |
|  [04]   | `webhook.port`                            | `int` 9443 — the admission listener                                                  |
|  [05]   | `webhook.{mutating,validating}`           | `{ create, failurePolicy }` — both created with `failurePolicy: Fail` by default     |
|  [06]   | `webhook.*Probe`                          | admission health gating                                                              |
|  [07]   | `monitoring` `monitoringQueriesConfigMap` | the PodMonitor posture and the default query set clusters inherit                    |
|  [08]   | `serviceAccount` `rbac`                   | the workload identity and its grants                                                 |
|  [09]   | `additionalArgs` `additionalEnv`          | controller passthrough                                                               |
|  [10]   | `service`                                 | the webhook door's annotations and shape — NOT its name                              |
|  [11]   | `{name,fullname,namespace}Override`       | FLAT top-level                                                                       |

[SCHEDULING]: `containerSecurityContext` `podSecurityContext` `priorityClassName` `resources` `nodeSelector` `tolerations` `affinity` `topologySpreadConstraints`
[config.data]: the operator configuration keys the ConfigMap or Secret carries — `INHERITED_ANNOTATIONS`, `INHERITED_LABELS`, and `WATCH_NAMESPACE` among them. `WATCH_NAMESPACE` is the real scoping control: `config.clusterWide` decides the RBAC shape while this key decides what the controller reconciles, so a namespaced install still watches everything until the key states otherwise.

[FULLNAME]: the standard collapse scaffold with flat overrides — the pin renames the Deployment, the ServiceAccount, and the RBAC objects.
[SERVICE_NAME]: REFUTED for this chart. The webhook Service renders as `cnpg-webhook-service` VERBATIM and ignores `fullnameOverride` entirely, because the webhook configurations reference it by a fixed name. The pin therefore governs the workload and not the address, so a fence deriving the admission endpoint from the pinned name resolves to nothing — and no consumer needs that address anyway, since admission is reached by the operator's own webhook configuration.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The chart installs the CONTROLLER; the estate authors the CRs. Every CNPG object the branch declares is a committed `crd2pulumi` class, so the operator vocabulary is compile-checked where the estate is PG-heaviest and a raw untyped `CustomResource` has no spelling.
- The generated CRD module and the chart pin move together: a bump regenerates the module rather than shifting an npm dependency, so the cluster schema and the typed classes never disagree.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` with `skipCrds: false` renders the operator AND its CRDs as managed resources, so a version bump diffs the schema rather than leaving Helm's install-once copy in place.
- `../crds/cnpg` (crd2pulumi): `postgresql.v1.Cluster`, `Database`, `ScheduledBackup`, `Pooler`, `Publication`, and `Subscription` beside `barmancloud.v1.ObjectStore` — the typed estate every CR row in the branch is built from.
- `plugin-barman-cloud`(`.api/plugin-barman-cloud.md`): the second chart in the same tier, installed after this one, adding WAL archiving, scheduled backup, recovery, and PITR against one typed `ObjectStore`.
- `kube/data#PG_CLUSTER`: the owner installing both charts, minting per-scope credentials, folding the extension matrix into every cluster, and publishing the pooler host, port, database, role, and realized pooling mode.
- `minio`(`.api/minio.md`) / `rook-ceph-cluster`(`.api/rook-ceph-cluster.md`): the archive destination the barman `ObjectStore` names, reached through whichever object-plane engine row the profile selected.

[LOCAL_ADMISSION]:
- Never derive the webhook address from the pinned name — the Service name is fixed and the pin does not reach it.
- Scope through `config.data.WATCH_NAMESPACE`, not through the install namespace; `clusterWide` shapes RBAC and nothing else.
- Install this chart BEFORE the barman plugin and before any CR, and parent each dependent on it so admission is live when the first cluster lands.
- Let the chart carry the CRDs under a render carrier; a separately applied CRD set drifts from the pin the generated module was built against.
- Leave both webhook `failurePolicy` values at `Fail` — an admission bypass on a database operator turns a rejected spec into a half-built cluster.

[RAIL_LAW]:
- Contract: `cloudnative-pg` chart values + the `postgresql.cnpg.io` CRD estate
- Owns: the PostgreSQL operator — its reach, admission webhooks, operator configuration carrier, monitoring query defaults, and the CRD estate every cluster CR is written against
- Accept: `skipCrds: false` under a render carrier; `config.data.WATCH_NAMESPACE` as the scoping control; `failurePolicy: Fail` on both webhooks; typed CRs from the generated module; the operator installed ahead of the plugin and every CR
- Reject: an address derived from the pinned name for the fixed webhook Service; the install namespace read as the watch scope; a CRD set applied outside the chart; an untyped `CustomResource` where a generated class exists; a database created by any means other than a CR

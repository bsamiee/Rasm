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

## [03]-[CR_CONTRACT]

[CRD_ESTATE]: group `postgresql.cnpg.io`, all served and stored at `v1` — `Cluster`, `Pooler`, `Database`, `ScheduledBackup`, `Backup`, `Publication`, `Subscription`, `DatabaseRole`, `FailoverQuorum`, and the image catalogues `ImageCatalog` (namespaced) and `ClusterImageCatalog` (cluster-scoped).

[CLUSTER_RESOURCES]: `Cluster.spec.resources` is `core/v1` `ResourceRequirements` VERBATIM — `requests` and `limits` as `map[string]Quantity` under the int-or-string quantity pattern, beside the `claims[]` list of `{ name, request }` keyed as a list-map on `name`. There is no CNPG-local shape and no pod-level seat on the CR.

[RESOURCE_REACH]: one envelope reaches PER CONTAINER every container the operator generates for a cluster — the `postgres` container, the `bootstrap-controller` init container, the role container and init container of every primary Job (initdb, import, base-backup, full-recovery, join, snapshot-recovery), and the major-upgrade `prepare` container. Sequential init execution holds the instance pod's effective request at ONE envelope rather than a multiple of it. Docs corroborate the `postgres` stamp alone, so the init-container and Job halves rest on operator source.

[POOLER_SPEC]: `Pooler.spec` requires `cluster` and `pgbouncer`; the remainder is `deploymentStrategy`, `instances`, `monitoring`, `serviceAccountName`, `serviceTemplate`, `template`, and `type`.

| [INDEX] | [FIELD]                                  | [CAPABILITY]                                                                                 |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `spec.instances`                         | `int32` DEFAULT 1 — the replica count; a silent row installs a single-pod pool               |
|  [02]   | `spec.pgbouncer.poolMode`                | enum `session \| transaction`, DEFAULT `session` — `statement` has NO spelling here          |
|  [03]   | `spec.pgbouncer.parameters`              | `map[string]string` — every VALUE is a string; keys prove against an admission allowlist     |
|  [04]   | `spec.template`                          | the pod template — `spec.containers` is MANDATORY under the PodSpec schema, `[]` when unused |
|  [05]   | `…template.spec.containers[]`            | merged BY NAME: the entry named `pgbouncer` is the only seat that sizes the bouncer          |
|  [06]   | `…template.spec.resources`               | alpha `PodLevelResources` — reaches the BOOTSTRAP init container alone                       |
|  [07]   | `spec.pgbouncer.{image,imageCatalogRef}` | MUTUALLY EXCLUSIVE; a `pgbouncer` container `image` overrides both                           |
|  [08]   | `spec.type`                              | which cluster door the pool fronts                                                           |

[POOLER_PARAMETERS]: `pgbouncer.parameters` is validated against `AllowedPgbouncerGenericConfigurationParameters`, an ALLOWLIST — an unlisted key is rejected at admission as `Invalid or reserved parameter`, never silently dropped and never overwritten by a forced set. `max_client_conn` and `default_pool_size` are both on it. CNPG writes `resources` on the `pgbouncer` container nowhere, so a user envelope on that entry passes through verbatim while a container under any other name survives as an unrelated sidecar.

[CR_NAMES]: the `cluster.name` reference on `Database`, `ScheduledBackup`, `Pooler`, `Publication`, and `Subscription` carries the CEL rule `self == oldSelf` — "cluster reference is immutable after creation", so re-pointing one is a new resource by construction. Poolers carry a second name law: a pooler's own `metadata.name` may never equal a cluster name in the same namespace.

## [04]-[IMPLEMENTATION_LAW]

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
- State `Cluster.spec.resources` on every cluster: omitted, the operator stamps an empty envelope onto each generated container and the estate's only stateful pod schedules BestEffort.
- Size a pooler through a container entry named `pgbouncer`, never through the pod-level `template.spec.resources` — that field is alpha, gated, and reaches the bootstrap init container alone.
- Name a `Pooler` so it can never collide with a cluster in its namespace, and treat every `cluster.name` reference as a create-time constant.

[RAIL_LAW]:
- Contract: `cloudnative-pg` chart values + the `postgresql.cnpg.io` CRD estate
- Owns: the PostgreSQL operator — its reach, admission webhooks, operator configuration carrier, monitoring query defaults, and the CRD estate every cluster CR is written against
- Accept: `skipCrds: false` under a render carrier; `config.data.WATCH_NAMESPACE` as the scoping control; `failurePolicy: Fail` on both webhooks; typed CRs from the generated module; a stated `Cluster.spec.resources` envelope and a `pgbouncer` container entry sizing each pooler; the operator installed ahead of the plugin and every CR
- Reject: an address derived from the pinned name for the fixed webhook Service; the install namespace read as the watch scope; a CRD set applied outside the chart; an untyped `CustomResource` where a generated class exists; a cluster or pooler shipped with no scheduling envelope; a pgbouncer knob spelled outside the parameter allowlist or given a non-string value; a mutated `cluster.name` reference; a database created by any means other than a CR

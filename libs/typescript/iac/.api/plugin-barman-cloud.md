# [TS_IAC_API_PLUGIN_BARMAN_CLOUD]

`plugin-barman-cloud` is the CNPG operator's backup plugin: one sidecar-injecting controller and one CRD that turns object storage into a WAL archive, a scheduled backup destination, and a point-in-time recovery source. The chart installs the plugin; the DESTINATION is a typed `ObjectStore` custom resource the cluster tier authors. Its Service name is FIXED because the plugin's own certificate is issued against it.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                | [CAPABILITY]                                                           |
| :-----: | :----------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `crds.create`                        | `boolean` DEFAULT TRUE — whether the `ObjectStore` CRD rides the chart |
|  [02]   | `service.name`                       | `string` `barman-cloud` — FIXED — the certificate is issued against it |
|  [03]   | `service.port`                       | `int` 9090 — the plugin's gRPC door the operator dials                 |
|  [04]   | `certificate.create*` + `issuerName` | the mTLS material between operator and plugin                          |
|  [05]   | `certificate.{duration,renewBefore}` | duration — 2160h with a 360h renewal window                            |
|  [06]   | `image` `sidecarImage`               | the controller image and the image injected into every cluster pod     |
|  [07]   | `replicaCount` `updateStrategy`      | placement — the operator supports `Recreate` ALONE                     |
|  [08]   | `serviceAccount` `rbac`              | the workload identity and its grants                                   |
|  [09]   | `additionalArgs` `additionalEnv`     | controller passthrough                                                 |
|  [10]   | `{name,fullname,namespace}Override`  | `string` — FLAT top-level — renames the workload, never the Service    |

[SCHEDULING]: `containerSecurityContext` `podSecurityContext` `priorityClassName` `resources` `nodeSelector` `tolerations` `affinity` `topologySpreadConstraints`
[FULLNAME]: the standard collapse scaffold; the pin reaches the Deployment, the ServiceAccount, and the RBAC objects.
[SERVICE_NAME]: `barman-cloud` VERBATIM, override-proof, because the server certificate's subject is generated from it. The operator resolves the plugin by that name, so the fixed spelling is a contract rather than a default.
[UPDATE_STRATEGY]: `Recreate` is the only supported value — the operator does not yet handle a rolling plugin update, so a `RollingUpdate` strategy is a values row the chart accepts and the runtime does not survive.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Backup is a PLUGIN, not an operator feature: the CNPG operator carries no object-store archiving of its own on this pin, so WAL archiving, scheduled backup, full recovery, and PITR all resolve through this chart's `ObjectStore` CR. One CR per realized cluster scope, so a dedicated cluster's archive prefix and credential are its own and a shared destination never re-couples the blast radius the dedicated tier bought.
- The plugin injects a SIDECAR into every cluster pod through `sidecarImage`, so a plugin bump changes the cluster workload shape and the two pins move together.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` with `skipCrds: false` renders the plugin and its CRD as managed resources under one pin.
- `cloudnative-pg`(`.api/cloudnative-pg.md`): the operator this plugin extends — installed FIRST, with this chart parented on it so the plugin registers against a live controller.
- `../crds/cnpg` (crd2pulumi): `barmancloud.v1.ObjectStore` is the typed destination class the cluster tier authors, sitting in the same generated module as the `postgresql.v1` CRDs.
- `kube/data#PG_CLUSTER`: the owner installing this chart after the operator, minting one `ObjectStore` per realized scope out of the caller's credential mint, and binding the cluster's backup and recovery rows to it.
- `minio`(`.api/minio.md`) / `rook-ceph-cluster`(`.api/rook-ceph-cluster.md`): the endpoint and bucket the `ObjectStore` CR addresses, with the credential Secret whichever engine row minted.

[LOCAL_ADMISSION]:
- Address the plugin at `barman-cloud`; the name is fixed and a pinned-name derivation resolves to nothing.
- Install after the operator and parent on it; a plugin registering against an absent controller is inert rather than failed.
- Leave `updateStrategy` at `Recreate`; the operator supports no other value on this pin.
- Author the destination as a typed `ObjectStore` CR with a scope-carrying archive prefix, never as chart values — the chart declares no destination at all.
- Move the plugin pin and the sidecar image together, because the sidecar lands inside every cluster pod the operator reconciles.

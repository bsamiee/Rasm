# [TS_IAC_API_ROOK_CEPH_CLUSTER]

`rook-ceph-cluster` is the object plane's storage-cluster arm. The chart renders NO workload of its own — it emits a `CephCluster` and its companion pool, filesystem, and object-store custom resources, and the Rook operator (installed separately, in its own namespace) reconciles every daemon, Service, and StorageClass behind them. Chart values therefore decide the CRs and the OPERATOR decides every rendered object name.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rook-ceph-cluster`
- chart: `rook-ceph-cluster` from `https://charts.rook.io/release` (Apache-2.0), versioned in lockstep with the operator's `appVersion`
- asset: one `CephCluster`, the declared `CephBlockPool`, `CephFilesystem`, and `CephObjectStore` sets with their StorageClasses and VolumeSnapshotClasses, an optional toolbox Deployment, and the monitoring PrometheusRule set
- plane: `plane:deploy` — rendered by `@pulumi/kubernetes` `helm.v4.Chart`, depended on by nothing at runtime
- rail: deployment / object plane
- crds: NONE in this chart — the CRD estate ships with the `rook-ceph` OPERATOR chart, which is a prerequisite

## [02]-[CHART_VALUES]

| [INDEX] | [KEY]                                           | [CAPABILITY]                                                                         |
| :-----: | :---------------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `operatorNamespace`                             | `string` — where the reconciling operator already runs; NOT this release's namespace |
|  [02]   | `cephClusterSpec`                               | mon count, dashboard, network, placement, resources, health checks                   |
|  [03]   | `cephClusterSpec.storage`                       | capacity is CLAIMED DEVICES, never a size string                                     |
|  [04]   | `cephObjectStores[]`                            | the S3 object stores and their bucket StorageClass                                   |
|  [05]   | `cephBlockPools[]` `cephFileSystems[]`          | the RBD and CephFS estates with their own StorageClasses                             |
|  [06]   | `cephImage` `cephClusterMetadata` `clusterName` | the Ceph distribution, its metadata, and the cluster identity                        |
|  [07]   | `toolbox`                                       | `{ enabled, image, resources, … }` — the debug shell; off by default                 |
|  [08]   | `monitoring`                                    | ServiceMonitor and PrometheusRule registration                                       |
|  [09]   | `configOverride`                                | `string` — raw `ceph.conf` passthrough                                               |
|  [10]   | `csiDriverNamePrefix`                           | `string` — the CSI driver prefix the operator's own install fixed                    |
|  [11]   | `ingress` `route`                               | dashboard reach                                                                      |

[SNAPSHOT_CLASSES]: `cephFileSystemVolumeSnapshotClass` and `cephBlockPoolsVolumeSnapshotClass`, one per estate
[cephObjectStores[].spec]: `metadataPool` and `dataPool` are `PoolSpec`s — replication is `replicated.size` and erasure coding is `erasureCoded.{dataChunks,codingChunks}`, and the default data pool is ERASURE CODED 2+1. A bare `size` beside `dataPool` is not a `PoolSpec` field: it is accepted by the values merge, read by nothing, and leaves the erasure-coded default standing. `gateway.{port,instances,resources,securePort,sslCertificateRef}` shapes the RGW, and `preservePoolsOnDelete` decides whether pool data survives the CR.
[cephObjectStores[].storageClass]: `enabled` `name` `reclaimPolicy` `volumeBindingMode` `parameters.region` — `objectStoreName` and `objectStoreNamespace` are set BY the chart and never by a caller.

[FULLNAME]: no override key exists and none is needed — every object this chart renders is a custom resource named by its own `cephObjectStores[].name`, `cephBlockPools[].name`, or `cephFileSystems[].name` row.
[SERVICE_NAME]: the OPERATOR renders the RGW Service as `rook-ceph-rgw-<objectStoreName>` on the gateway's `port` (80 by default), so the address is decorated over the CUSTOM RESOURCE name and never over the Helm release name. An endpoint spelled off the release resolves to nothing, and the CR name is what the values row declared.
[PREREQUISITE]: the `rook-ceph` operator chart must already be installed in `operatorNamespace` with its CRDs present; this chart's objects are inert without it and their reconciliation is silently absent rather than failed.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The object plane is an engine ROW: this arm answers the same profile axis `minio` answers, and each row supplies its own values, endpoint projection, and credential mint. Here the credential is the operator's — a `CephObjectStoreUser` CR yields a Secret named `rook-ceph-object-user-<store>-<user>` keyed `AccessKey`/`SecretKey`, so this tier owns the user CR and never the material.
- Capacity has no size coordinate on this arm. Ceph sizes by the devices its OSDs claim, so the profile's storage string has no seat here and stating one is a declaration rather than a provision.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the CR set; the operator's own objects arrive as reconciled children outside the Pulumi graph.
- `../crds/rook` (crd2pulumi): `ceph.v1.CephObjectStoreUser` is the branch's compile-checked spelling of the user CR, regenerated on chart bumps like every committed CRD module.
- `kube/data#OBJECT_PLANE`: the `ceph` engine row declares one `cephObjectStores` entry named for the bucket, mints the user CR, and projects the endpoint off the RGW decoration over that CR name.
- `minio`(`.api/minio.md`): the alternative arm on the same axis — one declares buckets as values rows and holds its own credential, the other declares object stores as custom resources and takes the operator's.

[LOCAL_ADMISSION]:
- Install the operator first, in its own namespace, and pass that namespace as `operatorNamespace`; this chart plants CRs and reconciles nothing.
- Derive the RGW address from the object-store CR name, never from the release name.
- Spell replication as `dataPool.replicated.size`; a bare `size` is inert and leaves the erasure-coded default in place.
- Declare capacity as devices under `cephClusterSpec.storage`, and route the profile's storage coordinate to the arm that has a seat for it.
- Take the credential from the operator-minted `rook-ceph-object-user-<store>-<user>` Secret with its `AccessKey`/`SecretKey` spellings; the estate's own key names belong to the other arm.

[RAIL_LAW]:
- Contract: `rook-ceph-cluster` chart values + the `ceph.rook.io` custom resources the Rook operator reconciles
- Owns: the Ceph storage cluster and its object, block, and filesystem estates — pool topology, RGW shape, StorageClasses, snapshot classes, and the debug toolbox
- Accept: an `operatorNamespace` naming a live operator install; one `cephObjectStores` row per owned store; `dataPool.replicated.size` for replication posture; device-claimed capacity; the operator's own credential Secret and its key spellings
- Reject: an endpoint derived from the release name; a bare `size` on a `PoolSpec`; a top-level `storage` key, which this chart does not read; an install without the operator chart; a hand-minted RGW credential beside the one the user CR produces

# [TS_IAC_API_MINIO]

`minio` is the self-hosted object plane the deploy plane installs when a stack rules its artifact store in-cluster. The chart is workload-plus-jobs: a StatefulSet or Deployment serving the S3 API beside a family of `mc`-driven post-install Jobs that create buckets, policies, users, and service accounts. Bucket creation is therefore a values row, never a separate resource, and the root credential is a values key the chart renders into a Secret.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `minio`
- chart: `minio` from `https://charts.min.io/` (Apache-2.0)
- asset: the server workload, an API Service and a console Service, a ServiceAccount, a PVC, a Secret, and the `mc` bootstrap Jobs (`makeBucketJob`, `makePolicyJob`, `makeUserJob`, `makeServiceAccountJob`, `customCommandJob`, `postJob`)
- plane: `plane:deploy` — rendered by `@pulumi/kubernetes` `helm.v4.Chart`, depended on by nothing at runtime
- rail: deployment / object plane
- crds: NONE

## [02]-[CHART_VALUES]

| [INDEX] | [KEY]                                  | [CAPABILITY]                                                                      |
| :-----: | :------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `mode`                                 | `distributed` \| `standalone` — distributed defaults to 16 replicas across 1 pool |
|  [02]   | `rootUser` `rootPassword`              | `string` — the root credential; `existingSecret` is the reference alternative     |
|  [03]   | `buckets[]`                            | declarative bucket creation through the `mc` Job                                  |
|  [04]   | `policies[]` `users[]` `svcaccts[]`    | the remaining declarative estate the bootstrap Jobs realize                       |
|  [05]   | `persistence`                          | `{ enabled, size, storageClass, … }` — the data claim, DEFAULT 500Gi              |
|  [06]   | `replicas` `pools` `drivesPerNode`     | `int` — the distributed topology — 16, 1, and 1 by default                        |
|  [07]   | `service`                              | the S3 API door, port 9000                                                        |
|  [08]   | `consoleService` `consoleIngress`      | the browser console door, port 9001                                               |
|  [09]   | `minioAPIPort` `minioConsolePort`      | `string` — the container ports the two Services target                            |
|  [10]   | `image` `mcImage`                      | `{ repository, tag, pullPolicy }` — the server and the bootstrap client images    |
|  [11]   | `tls` `trustedCertsSecret` `certsPath` | in-pod TLS material                                                               |
|  [12]   | `environment` `extraSecret` `oidc`     | server environment, extra material, and OIDC identity                             |
|  [13]   | `nameOverride` `fullnameOverride`      | `string` — FLAT top-level, standard collapse scaffold                             |

[buckets]: `name` `policy` (`none` \| `download` \| `upload` \| `public`) `purge` `versioning` `objectlocking` — object locking enables versioning implicitly, and `purge` DELETES an existing bucket before recreating it
[persistence]: `enabled` `size` `storageClass` (`"-"` disables dynamic provisioning) `accessMode` `existingClaim` `volumeName` `subPath` `annotations`
[jobs]: each `mc` Job carries its own `securityContext`, `resources`, `annotations`, `nodeSelector`, `tolerations`, and `affinity` — `exitCommand` on `postJob` is the escape hatch for a step the declarative rows cannot spell

[FULLNAME]: the standard collapse scaffold with flat `nameOverride`/`fullnameOverride` — absent a pin, a release named `objects` renders `objects-minio`, so every address derived from the release name alone resolves to nothing. The estate pins the override to the release name so the two agree by proof.
[SERVICE_NAME]: with the pin, the API Service is `<fullname>` on port 9000 and the console Service is `<fullname>-console` on 9001, both ClusterIP by default; a distributed install additionally renders the headless `<fullname>-svc` the StatefulSet's peers resolve through. The API door is the only one this estate addresses.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The object plane is an engine ROW, not a fixed choice — the same tier answers `minio` or `ceph` off one profile axis, and each row supplies its own chart, values, endpoint projection, and credential mint. What crosses the seam is the endpoint and the bucket, never the engine.
- Credentials here are the estate's, not the chart's: the root pair arrives from the in-graph Doppler read and lands again in a namespace `Secret` keyed `ACCESS_KEY_ID`/`SECRET_ACCESS_KEY`, which is what the barman archive CR references — one mint, two consumers.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the workload and the bootstrap Jobs; `core.v1.Secret` carries the credential pair the archive CR reads.
- `kube/data#OBJECT_PLANE`: the `minio` engine row supplies `image.repository`, the root pair, `persistence.size` off the profile's storage coordinate, one `buckets` entry named `<app>-artifacts`, and the `fullnameOverride` pin whose value the endpoint projection reads back.
- `cloudnative-pg`(`.api/cloudnative-pg.md`) / `plugin-barman-cloud`(`.api/plugin-barman-cloud.md`): the WAL archive destination is this endpoint and this bucket, reached through the barman `ObjectStore` CR that names the credential Secret.
- `mimir-distributed`(`.api/mimir-distributed.md`): the metrics store's escalation row binds this same endpoint and bucket as its common S3 backend rather than standing up the chart's own bundled object store.

[LOCAL_ADMISSION]:
- Pin `fullnameOverride` to the release name; the collapse helper drops the chart name only for a release that already contains it, and no release named for its role does.
- Declare buckets as `buckets[]` rows, never as a follow-on resource — the chart already owns an `mc` Job for exactly this, and a second creator races it.
- Size through `persistence.size`; the chart's 500Gi default is a capacity commitment no profile made.
- Address the API door alone; the console Service is a second plane this estate does not publish.
- Never spell `purge: true` against a bucket holding artifacts — it deletes before it creates.

[RAIL_LAW]:
- Contract: `minio` chart values
- Owns: the self-hosted object plane — the S3 server, its topology and data claim, the declarative bucket, policy, user, and service-account estate, and the two rendered doors
- Accept: `fullnameOverride` pinned to the release; `rootUser`/`rootPassword` from the in-graph credential read; one `buckets` row per owned bucket; `persistence.size` from the profile's storage coordinate; the API door as the one published endpoint
- Reject: an address derived from an unpinned release name; a bucket created outside `buckets[]`; the 500Gi persistence default; `purge` on a live bucket; a published console door; a credential spelled anywhere the chart renders into a ConfigMap

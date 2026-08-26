# [TS_IAC_API_VCLUSTER]

`vcluster` is the hard-isolation tenancy arm: one chart install stands up a whole virtual control plane inside a host namespace, and every tenant workload lands against that plane rather than against the host API. The chart names nothing after itself — it has no fullname helper at all — so the RELEASE NAME IS THE VCLUSTER NAME and every rendered object reads it verbatim. Its `values.schema.json` refuses excess members on the verified definitions, which turns a stale values file into a render failure rather than an ignored key.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                          | [CAPABILITY]                                                                      |
| :-----: | :--------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `sync.toHost.<resource>`                       | which virtual objects materialize on the host; the schema refuses a third member  |
|  [02]   | `sync.fromHost.<resource>`                     | which host objects the virtual plane observes                                     |
|  [03]   | `controlPlane.distro.k8s`                      | the ONLY distro child; `k3s`, `k0s`, and `eks` are GONE                           |
|  [04]   | `controlPlane.backingStore`                    | at most ONE arm enabled; two is a hard render failure                             |
|  [05]   | `controlPlane.statefulSet.persistence`         | the data claim — defaults to a 5Gi `Retain` PVC that survives uninstall           |
|  [06]   | `controlPlane.service`                         | a `spec.ports` value REPLACES the whole default port list                         |
|  [07]   | `controlPlane.{ingress,tlsRoute}`              | external reach; ingress annotations default to nginx                              |
|  [08]   | `controlPlane.advanced.serviceAccount.name`    | `string` — overrides the `vc-<release>` ServiceAccount name                       |
|  [09]   | `networking.advanced.proxyKubelets.byHostname` | `boolean` — adds the 10250 kubelet port when the Service type is not LoadBalancer |
|  [10]   | `integrations.istio.enabled`                   | `boolean` — adds the 9090 wake-http port                                          |
|  [11]   | `exportKubeConfig`                             | credential egress; `secret` and `additionalSecrets` together fail                 |

[sync.toHost]: `services` `endpoints` `endpointSlices` `persistentVolumeClaims` `configMaps` `secrets` `pods` `ingresses` `gatewayApi` `priorityClasses` `networkPolicies` `podDisruptionBudgets` `serviceAccounts` `storageClasses` `persistentVolumes` `namespaces` `resourceClaims` `resourceClaimTemplates`
[sync.fromHost]: `events` `configMaps` `csiDrivers` `csiNodes` `csiStorageCapacities` `storageClasses` `ingressClasses` `gatewayClasses` `gateways` `runtimeClasses` `priorityClasses` `nodes` `secrets` `deviceClasses`
[controlPlane]: `distro` `backingStore` `proxy` `coredns` `service` `ingress` `tlsRoute` `standalone` `statefulSet` `serviceMonitor` `advanced`

[TOP_LEVEL_REMAINDER]: `privateNodes` `deploy` `rbac` `policies` `plugins` `experimental` `telemetry` `logging`
[SERVICE_ROWS]: `controlPlane.service` carries `enabled` `spec` `httpsNodePort` `kubeletNodePort`; a `spec.ports` or `spec.selector` value REPLACES the chart's whole default list rather than extending it.
[FULLNAME]: REFUTED — no fullname template exists. `_helper.tpl` defines image and version helpers alone, and `nameOverride`/`fullnameOverride` are absent from both `values.yaml` and every consumer. Object names are `.Release.Name` VERBATIM: `<release>`, `<release>-headless`, `<release>-etcd`, `<release>-etcd-headless`, and the ServiceAccount `vc-<release>`. Renaming a vcluster is a different release, and a values row spelling either override names a key the strict schema can reject.
[SERVICE_NAME]: `<release>` serves 443 onto container 8443 named `https` under `controlPlane.service.enabled`, selector `app: vcluster, release: <release>`; `<release>-headless` serves the same pair with `clusterIP: None` and `publishNotReadyAddresses: true`, gated on the workload rendering as a StatefulSet. Optional ports arm per row: 9090 `wake-http` with the Istio integration, and 10250 onto 8443 with hostname kubelet proxying off a LoadBalancer.
[WORKLOAD_KIND]: the kind is COMPUTED, never declared — `StatefulSet` iff `controlPlane.statefulSet.persistence.volumeClaim` is enabled OR `controlPlane.backingStore.etcd.embedded.enabled`, else `Deployment`. Stock values render a StatefulSet with a 5Gi claim, and the kind flips silently on a values change, so a read-back that hardcodes it reads the wrong object after an unrelated edit.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One release per tenant, in that tenant's own namespace: the tier mints the namespace and installs the chart under `<tenant>-plane`, so the virtual control plane's name and the tenant's name are one derivation and the host namespace is the isolation boundary.
- The virtual plane installs its OWN CRDs at runtime, inside itself. The chart ships no `crds/` directory at all, so `skipCrds` is a no-op here and no host-side CRD set belongs to this row.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the plane into the tenant namespace the tier just created; the namespace resource is the parent every rendered object depends on.
- `kube/tenant#TENANT_TIER`: the `vcluster` mode row is the hard-isolation alternative to the `capsule` row — one `k8s.core.v1.Namespace` per tenant, one chart release named `<tenant>-plane`, and `sync.toHost.ingresses.enabled` as the single values row so tenant Ingress objects materialize on the host edge.
- `capsule`(`.api/capsule.md`): the two arms are alternatives on one `tenancy.mode` axis and never compose — soft isolation governs shared namespaces through admission, hard isolation gives each tenant its own API server, and a deployment running both would hold two tenancy authorities over one cluster.

[LOCAL_ADMISSION]:
- Derive every address from the RELEASE name; a `fullnameOverride` row renames nothing here and risks schema rejection.
- Spell `sync.toHost.ingresses` as `{ enabled, patches }` and nothing more — the legacy top-level `sync.ingresses` no longer exists and the schema's `additionalProperties: false` turns the stale spelling into a failed render.
- Never carry a values file forward across a minor without re-reading the removals: five keys removed at 0.36.0 (`sync.toHost.volumeSnapshots`, `sync.toHost.volumeSnapshotContents`, `sync.fromHost.volumeSnapshotClasses`, `deploy.volumeSnapshotController`, `rbac.enableVolumeSnapshotRules`) fail the render on MERE PRESENCE.
- Enable at most one backing store; two arms is a render failure, as is `exportKubeConfig.secret` beside `additionalSecrets`.
- Read the workload kind back from the cluster, never from a values assumption, and treat the default 5Gi `Retain` claim as state that outlives the release.

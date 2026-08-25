# [TS_IAC_API_PULUMI_KUBERNETES_OPERATOR]

`pulumi-kubernetes-operator` is the in-cluster reconciler: the chart installs one controller and its four CRDs, and each reconciled estate is a `Stack` custom resource the controller drives on its own clock. Chart values decide the controller's identity, RBAC reach, and metrics posture; the `Stack` CRD decides which program runs, from where, and how often. The chart publishes NO watch-namespace value, so reach is an RBAC shape rather than a scope setting.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                        | [CAPABILITY]                                                                        |
| :-----: | :------------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `rbac.createClusterRole`                     | `boolean` DEFAULT TRUE — the cluster-wide grant; the only real reach control        |
|  [02]   | `rbac.*` remainder                           | the remaining RBAC shape — no namespace-watch value exists                          |
|  [03]   | `image.{registry,repository,tag,pullPolicy}` | `string` — the controller image, injected again as `AGENT_IMAGE` for workspace pods |
|  [04]   | `controller.*`                               | log posture and the address the file-server arg carries                             |
|  [05]   | `leaderElection.*`                           | durations — single-writer election across replicas                                  |
|  [06]   | `kubeAPI.timeout`                            | duration — the controller's own API client deadline                                 |
|  [07]   | `flux.maxUntarSizeBytes`                     | `int` — the Flux source-archive ceiling                                             |
|  [08]   | `serviceAccount.*`                           | the workload identity the estate's own cell binds                                   |
|  [09]   | `serviceMonitor.*`                           | Prometheus Operator scrape registration, off by default                             |
|  [10]   | `replicaCount` + placement rows              | placement and the 200m/128Mi request pair under UID 65532                           |
|  [11]   | `nameOverride` `fullnameOverride`            | `string` — FLAT top-level, standard collapse scaffold                               |

[PLACEMENT]: `replicaCount` `deploymentStrategy` `terminationGracePeriodSeconds` `resources` (200m/128Mi) `podSecurityContext` (UID 65532)
[RBAC_ROWS]: `rbac.create` (true) `createClusterRole` (TRUE) `createRole` (false) `createClusterAggregationRoles` (true) `extraRules` — the whole reach vocabulary, since no watch-namespace value exists
[ARGS]: the container args are FIXED and no value edits them — `--leader-elect`, `--health-probe-bind-address=:8081`, `--metrics-bind-address=:8383`, `--metrics-secure`, `--program-fs-adv-addr=<controller.advertisedAddress>:80`, and the zap flags `controller.logLevel`/`logFormat` project into.
[FULLNAME]: the standard collapse scaffold — `fullnameOverride` wins, else the name collapses to `.Release.Name` when it CONTAINS the chart name, else `<release>-pulumi-kubernetes-operator`.
[SERVICE_NAME]: the Service is `<fullname>` UNSUFFIXED and hardcoded ClusterIP, serving `http-fileserver` 80 onto container 9090 and `http-metrics` 8383 onto 8383; the Deployment is `<fullname>-controller-manager` and its probes answer on 8081 at `/healthz` and `/readyz`. Reading the workload suffix onto the Service is the address that resolves to nothing.
[CRD_LIFECYCLE]: `crds/` ships `pulumi.com_stacks`, `pulumi.com_programs`, `auto.pulumi.com_workspaces`, and `auto.pulumi.com_updates`. `helm upgrade` installs that directory once and never revisits it; a render carrier that hands each object to a provider escapes the rule entirely, which is why the estate's install diffs its CRDs on every bump and owes no out-of-band apply.

## [02]-[STACK_CONTRACT]

[CRD_ESTATE]: group `pulumi.com` — `Stack` (NAMESPACED, plural `stacks`, `v1` served and storage beside a served `v1alpha1`) and `Program` (`v1` only); group `auto.pulumi.com` — `Workspace` and `Update`, the execution companions the controller mints per reconcile.

| [INDEX] | [FIELD]                                         | [CAPABILITY]                                                                         |
| :-----: | :---------------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `spec.stack`                                    | `string` REQUIRED — the ONLY required field — `<org>/<stack>` — the reconcile target |
|  [02]   | `spec.projectRepo` `branch` `repoDir` `shallow` | `string` / `boolean` — the Git source of the desired-state program                   |
|  [03]   | `spec.fluxSource`                               | `{ sourceRef, dir }`, `sourceRef` REQUIRED — the Flux alternative to a direct repo   |
|  [04]   | `spec.programRef`                               | `{ name }` REQUIRED — an in-cluster `Program` CR as the source                       |
|  [05]   | `spec.refresh`                                  | `boolean` — re-read provider state each cycle                                        |
|  [06]   | `spec.continueResyncOnCommitMatch`              | `boolean` — make the loop continuous rather than commit-edge-triggered               |
|  [07]   | `spec.resyncFrequencySeconds`                   | `int` — the cadence; ANY value under 60, zero included, coerces to 60                |
|  [08]   | `spec.envRefs`                                  | the typed environment binding — the successor to three deprecated keys               |
|  [09]   | `spec.*` configuration                          | configuration and secret material by reference                                       |
|  [10]   | `spec.workspaceTemplate`                        | strategic-merge patch over `Workspace` — the execution pod's whole shape             |
|  [11]   | `spec.updateTemplate`                           | patch — the per-update shape                                                         |
|  [12]   | `spec.workspaceReclaimPolicy`                   | `Retain` \| `Delete`, default `Retain` — whether the workspace survives the update   |
|  [13]   | `spec.*` scoping                                | scoped updates and cross-stack ordering                                              |
|  [14]   | `spec.*` remainder                              | lifecycle and execution posture                                                      |

[STACK_REMAINDER]: `destroyOnFinalize` `preview` `retryOnUpdateConflict` `retryMaxBackoffDurationSeconds` `expectNoRefreshChanges` `useLocalStackOnly` `runProgram` `serviceAccountName`
[RESOURCE_REF]: `type` closes on `Env | FS | Secret | Literal` and selects the sibling selector — `env.{name}`, `filesystem.{path}`, `literal.{value}`, or `secret.{name,key,namespace}` with `key` and `name` both required. TRAP: `secret.namespace` is deprecated and a non-empty value is INVALID unless controller namespace isolation is disabled.
[WORKSPACE_TEMPLATE]: the patched `Workspace` spec carries `env` `envFrom` `flux` `git` `image` `imagePullPolicy` `local` `podTemplate` `projectInfo` `pulumiLogLevel` `resources` `securityProfile` `serviceAccountName` `serviceTemplate` `stacks`.
[DEPRECATED]: `envs` and `envSecrets` yield to `envRefs`, `secrets` to `secretsRef`, `gitAuthSecret` to `gitAuth`, and `accessTokenSecret` to an `envRefs` secret entry keyed `PULUMI_ACCESS_TOKEN`. `spec.stack`, `projectRepo`, `branch`, `repoDir`, `refresh`, `continueResyncOnCommitMatch`, `resyncFrequencySeconds`, and `envRefs` are confirmed NOT deprecated.
[V2_ONLY]: `updateTemplate`, `workspaceReclaimPolicy`, `shallow`, `runProgram`, `expectNoRefreshChanges`, `targets`, `targetDependents`, `prerequisites`, `configRef`, `secretsRef`, and `environment` exist on v2 alone.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Two clocks never watch one stack: an estate under this operator drops out of the deploy-host drift fleet, so evidence has one producer per stack and the remediation posture stays deliberate on both paths.
- The workspace facts are one vocabulary across two execution planes — the backend URL and the config passphrase the deploy host reads from its own environment reach the in-cluster loop as `envRefs` entries over ONE Secret this tier mints, so a CR referencing material nothing created has no spelling.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the controller and its CRDs as managed resources, and `core.v1.Secret` carries the workspace facts the `Stack` CR binds by reference rather than by literal.
- `../crds/pko` (crd2pulumi): `v1.Stack` and `v1.Program` are the branch's compile-checked spelling of this CRD estate; the module regenerates against the pinned chart on every operator bump.
- `operate/policy#RECONCILE_LOOP`: the owner installing this chart, minting the workspace Secret, and authoring one `Stack` per reconciled estate under the one-clock law.
- `kube/tenant#TENANT_TIER`: a tenant-submitted CR reconciles inside the tenancy boundary that page draws, so this operator is the self-service provisioning engine and the tenancy arm is what bounds what a tenant CR may reach.

[LOCAL_ADMISSION]:
- Reference the chart by its OCI URL inline; a `repositoryOpts.repo` row pointed at the Pages host resolves nothing.
- Treat `rbac.createClusterRole` as the reach control and state it explicitly — a namespaced install on chart defaults still holds cluster-wide grants, and the namespace row is not the scope.
- Bind every credential through `envRefs` with `type: "Secret"`; the four deprecated carriers still validate, which is precisely why an estate settles on one spelling.
- Leave `secret.namespace` empty; a non-empty value is invalid under the controller's own namespace isolation.
- Read the cadence floor: a `resyncFrequencySeconds` under 60 is silently raised, so a tighter loop is not expressible and a fence spelling one states an intent the controller discards.
- Derive the address from the UNSUFFIXED Service name; the `-controller-manager` suffix belongs to the Deployment alone.

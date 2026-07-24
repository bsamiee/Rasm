# [TS_IAC_API_PULUMI_EKS]

`@pulumi/eks` owns the managed-Kubernetes escalation of the `aws` column — EKS control plane, access-entry auth, IRSA/OIDC wiring, managed and self-managed node capacity, Fargate, EKS Auto Mode, and addon lifecycle as one `Cluster` component with satellite node/addon components. `cluster.kubeconfigJson` binds a `k8s.Provider`, so promoting the `aws` column to a k8s-shaped estate is a provider-seam swap that reuses the `kube/*` tier roster unchanged. `Cluster` composes `@pulumi/aws` and `@pulumi/kubernetes` in-process, so its children are typed resources under Pulumi diff and CrossGuard.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/eks`
- package: `@pulumi/eks` (Apache-2.0)
- import: `@pulumi/eks` flat namespace — `Cluster` is the plane owner; node/addon classes, the enum-const vocabularies, and the `create*`/`getRoleProvider` function twins ride alongside
- owner: `iac`
- rail: kubernetes / prepared-aws
- runtime: Node deploy-host; drives the AWS API, then the EKS API server through the composed `@pulumi/kubernetes`
- depends: `@pulumi/aws`, `@pulumi/kubernetes`, `@pulumi/pulumi`
- capability: EKS control plane, access-entry auth, OIDC/IRSA provisioning, managed/self-managed node groups, Fargate profiles, Auto Mode, addon lifecycle with conflict-resolution vocabulary, and kubeconfig egress with role/profile assumption

## [02]-[CLUSTER_COMPONENT]

[CLUSTER_SCOPE]: `Cluster` — the one plane owner

`new Cluster(name, ClusterArgs?, opts?)` groups `ClusterArgs` into axes on one component — network (`vpcId`/`publicSubnetIds`/`privateSubnetIds`), access (`authenticationMode`/`accessEntries`), identity (`serviceRole`/`createOidcProvider`), capacity (`skipDefaultNodeGroup`/`nodeGroupOptions`), posture (`version`/`enabledClusterLogTypes`), addons (`vpcCniOptions`), serverless (`fargate`/`autoMode`). `createOidcProvider` mints the IRSA anchors `oidcProviderArn`/`oidcProviderUrl`; `accessEntries` keys `AccessEntryArgs` by name, each `principalArn` with a scoped `accessPolicies` map.

[ACCESS_ENUMS]: `AuthenticationMode` `AccessEntryType`

| [INDEX] | [MEMBER]                  | [SHAPE_MEANING]                                                                                            |
| :-----: | :------------------------ | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `cluster.kubeconfigJson`  | `Output<string>` — the provider seam; `kubeconfig` is its structured `Output<any>` twin                    |
|  [02]   | `cluster.getKubeconfig`   | `(profileName?, roleArn?)` → `Output<{ result: string }>`; role/profile-scoped kubeconfig for off-host use |
|  [03]   | `cluster.provider`        | ready-bound provider handle; explicit `k8s.Provider({ kubeconfig })` is the arm-seam spelling              |
|  [04]   | `cluster.eksCluster`      | `Output<aws.eks.Cluster>` — the underlying typed resource for raw-attribute reach                          |
|  [05]   | `cluster.core`            | `Output<CoreData>` — assembled internals (subnets, roles, security groups) satellite components consume    |
|  [06]   | `cluster.createNodeGroup` | `(name, ClusterNodeGroupOptionsArgs)` mixin — a self-managed group bound to this plane                     |
|  [07]   | `AccessEntryArgs`         | `principalArn`, `accessPolicies?`, `kubernetesGroups?`, `type?`, `username?`                               |
|  [08]   | `AuthenticationMode.Api`  | `"API"` — the access-entry auth mode the component sets                                                    |

## [03]-[NODES_AND_ADDONS]

[CAPACITY_SCOPE]: node groups and addons — managed first

Every node group and addon is `new X(name, args, opts?)` binding `cluster` (required). A node group carries the scaling axis (`instanceTypes`/`scalingConfig`/`capacityType`/`amiType`/`operatingSystem`/`launchTemplate`); `NodeGroupV2` adds `minRefreshPercentage`/`launchTemplateTagSpecifications`.

An addon binds `resolveConflictsOnCreate`/`resolveConflictsOnUpdate`, `VpcCniAddon` adds the CNI knobs (`enableNetworkPolicy`/`enablePrefixDelegation`/`enablePodEni`/`warm*Target`), and `Addon` carries `addonVersion`/`configurationValues`/`serviceAccountRoleArn`. `createManagedNodeGroup`/`createNodeGroupSecurityGroup`/`createStorageClass`/`getRoleProvider` compose the component rows in `.apply` folds.

[NODE_ENUMS]: `AmiType` `OperatingSystem` `ClusterNodePools` `ResolveConflictsOnCreate` `ResolveConflictsOnUpdate`

| [INDEX] | [SYMBOL]                      | [SHAPE_BOUNDARY]                                                                                    |
| :-----: | :---------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `ManagedNodeGroup`            | default capacity row (`cluster: Cluster \| CoreDataArgs` required) → `nodeGroup: aws.eks.NodeGroup` |
|  [02]   | `NodeGroupV2`                 | ASG-native self-managed group → `autoScalingGroup: aws.autoscaling.Group`                           |
|  [03]   | `Addon`                       | generic EKS-addon lifecycle (`addonName` required)                                                  |
|  [04]   | `VpcCniAddon`                 | CNI specialization (`clusterName` required); post-plane twin of `ClusterArgs.vpcCniOptions`         |
|  [05]   | `ClusterCreationRoleProvider` | `{ profile?, region? }` → `role: Output<aws.iam.Role>`; creator identity for `creationRoleProvider` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- network: the plane rides the arm's existing `awsx.ec2.Vpc` — `vpcId`/`publicSubnetIds`/`privateSubnetIds` bind the network axis so network intent has one owner; `endpointPublicAccess: false` with `publicAccessCidrs` is the private-plane posture, decided by spec data.
- access: `authenticationMode: "API"` with `accessEntries` is the access spelling — an entry is `principalArn` with scoped `accessPolicies`, so cluster RBAC is data on the component.
- identity: `createOidcProvider: true` mints the IRSA anchors once; a workload identity is `Addon.serviceAccountRoleArn` or an `aws.iam.Role` trust-bound to `oidcProviderArn`, never a widened node instance role.
- capacity: `ManagedNodeGroup` is the default row, `NodeGroupV2` the launch-template escalation, `skipDefaultNodeGroup: true` wherever explicit groups exist so capacity has named owners; `fargate` and `autoMode` are spec-profile decisions on the same component.
- addon: cluster addons ride `Addon`/`VpcCniAddon` under the `ResolveConflictsOnCreate`/`ResolveConflictsOnUpdate` vocabulary; a `helm.v4.Chart` owns only what the addon catalog does not carry.

[STACKING]:
- `@pulumi/awsx`(`.api/pulumi-awsx.md`): `ec2.Vpc` outputs `vpcId`/`publicSubnetIds`/`privateSubnetIds` bind the `ClusterArgs` network axis, so the EKS plane rides the arm's realized VPC graph.
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `cluster.kubeconfigJson` binds `new k8s.Provider({ kubeconfig, enableServerSideApply: true })`, and every `kube/*` row — `helm.v4.Chart` operators, `apiextensions.CustomResource` — rides the EKS plane through that one provider.
- `@pulumi/postgresql`(`.api/pulumi-postgresql.md`): the `kube/data` CNPG `Cluster` declared through the k8s provider exposes its `-rw` service host into `postgresql.Provider`, so the data plane finalizes over the EKS-hosted CNPG operator.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): the component's children are typed resources under Pulumi diff and CrossGuard, and a construction failure folds into the `automation.UpResult` typed run receipt.
- within-lib: the `provider/dispatch` `aws` arm promotes to a k8s-shaped estate by swapping only the provider seam — `ManagedNodeGroup`/`NodeGroupV2` capacity and `createOidcProvider` IRSA anchors ride the same `Cluster` the arm already constructs.

[RAIL_LAW]:
- Package: `@pulumi/eks`
- Owns: the managed EKS plane — control plane, access entries, OIDC/IRSA, node capacity, Fargate/Auto Mode, addon lifecycle, kubeconfig egress
- Accept: `kubeconfigJson` into the arm's one `k8s.Provider` seam, `authenticationMode: "API"` + `accessEntries`, `ManagedNodeGroup`/`NodeGroupV2` capacity, `awsx.ec2.Vpc` outputs, `createOidcProvider` IRSA anchors, typed `Addon` rows
- Reject: hand-rolled `aws.eks.*` assemblies duplicating the component, kubeconfig literals or per-resource providers, node-role widening in place of IRSA, chart installs of native addons

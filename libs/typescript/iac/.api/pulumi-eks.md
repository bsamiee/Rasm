# [TS_IAC_API_PULUMI_EKS]

`@pulumi/eks` owns the managed-Kubernetes escalation of the `aws` column — EKS control plane, access-entry auth, IRSA/OIDC wiring, managed and self-managed node capacity, Fargate, EKS Auto Mode, and addon lifecycle as one `Cluster` component with satellite node/addon components. `cluster.kubeconfigJson` binds a `k8s.Provider`, so promoting the `aws` column to a k8s-shaped deployment is a provider swap that reuses the `kube/*` tier roster unchanged. `Cluster` composes `@pulumi/aws` and `@pulumi/kubernetes` in-process, so its children are typed resources under Pulumi diff and CrossGuard.

## [01]-[CLUSTER_COMPONENT]

[CLUSTER_SCOPE]: `Cluster` — the one plane owner

`new Cluster(name, ClusterArgs?, opts?)` groups `ClusterArgs` into axes on one component — network (`vpcId`/`publicSubnetIds`/`privateSubnetIds`), access (`authenticationMode`/`accessEntries`), identity (`serviceRole`/`createOidcProvider`), capacity (`skipDefaultNodeGroup`/`nodeGroupOptions`), posture (`version`/`enabledClusterLogTypes`), addons (`vpcCniOptions`), serverless (`fargate`/`autoMode`). `createOidcProvider` mints the IRSA anchors `oidcProviderArn`/`oidcProviderUrl`; `accessEntries` keys `AccessEntryArgs` by name, each `principalArn` with a scoped `accessPolicies` map.

[ACCESS_ENUMS]: `AuthenticationMode` `AccessEntryType`

Every vocabulary is a frozen const object beside its `keyof typeof` union, re-exported at the PACKAGE ROOT rather than behind a `types.enums` path, so `eks.AuthenticationMode.Api` is the call-site spelling and no import reaches deeper.

Deprecated aliases sit beside live members and map onto the same values — `AuthenticationMode.API` beside `.Api`, `AmiType.AL2X86_64`, `OperatingSystem.RECOMMENDED` resolving to `AL2023` — so a roster derived from the const carries duplicate spellings of one member. Unlike `@pulumi/aws`, this SDK types several args to the closed union directly: `ClusterArgs.authenticationMode` takes `AuthenticationMode` with no string widening and no `Input` lift, and `operatingSystem` takes `Input<OperatingSystem>`.

| [INDEX] | [MEMBER]                  | [SHAPE_MEANING]                                                                                            |
| :-----: | :------------------------ | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `cluster.kubeconfigJson`  | `Output<string>` — the provider binding; `kubeconfig` is its structured `Output<any>` twin                 |
|  [02]   | `cluster.getKubeconfig`   | `(profileName?, roleArn?)` → `Output<{ result: string }>`; role/profile-scoped kubeconfig for off-host use |
|  [03]   | `cluster.provider`        | ready-bound provider handle; explicit `k8s.Provider({ kubeconfig })` is the arm-level binding              |
|  [04]   | `cluster.eksCluster`      | `Output<aws.eks.Cluster>` — the underlying typed resource for raw-attribute reach                          |
|  [05]   | `cluster.core`            | `Output<CoreData>` — assembled internals (subnets, roles, security groups) satellite components consume    |
|  [06]   | `cluster.createNodeGroup` | `(name, ClusterNodeGroupOptionsArgs)` mixin — a self-managed group bound to this plane                     |
|  [07]   | `AccessEntryArgs`         | `principalArn`, `accessPolicies?`, `kubernetesGroups?`, `type?`, `username?`                               |
|  [08]   | `AuthenticationMode.Api`  | `"API"` — the access-entry auth mode the component sets                                                    |

## [02]-[NODES_AND_ADDONS]

[CAPACITY_SCOPE]: node groups and addons — managed first

Every node group and addon is `new X(name, args, opts?)` binding `cluster` (required). Node groups carry the scaling axis (`instanceTypes`/`scalingConfig`/`capacityType`/`amiType`/`operatingSystem`/`launchTemplate`); `NodeGroupV2` adds `minRefreshPercentage`/`launchTemplateTagSpecifications`.

Capacity is where this SDK stops closing: `instanceTypes` is `Input<Input<string>[]>`, `amiType` and `capacityType` are `Input<string>` whose rosters live only in an arg comment, and `operatingSystem` alone takes the exported union as `Input<OperatingSystem>`. Closing a capacity coordinate therefore spends `@pulumi/aws`'s `types.enums.ec2.InstanceType` at whichever surface owns it, since no argument here refuses an unspellable value.

`scalingConfig` is the aws-generated `types.input.eks.NodeGroupScalingConfig` — `minSize`/`maxSize`/`desiredSize`, with the ordering invariant enforced by the API, never the type.

AMI selection runs one derivation against two overrides. `operatingSystem` names the FAMILY and the component resolves the EKS-optimized image from it with the instance types and gpu configuration, so an arm64 capacity value picks its own architecture; `amiType` names the image type and `amiId` the image itself, each superseding that derivation, and `amiId` is documented mutually exclusive with both `gpu` and `amiType`. `OperatingSystem.RECOMMENDED` aliases whichever family AWS recommends, so it collapses onto that family's literal and pins nothing a bump cannot move.

Addons bind `resolveConflictsOnCreate`/`resolveConflictsOnUpdate`, `VpcCniAddon` adds the CNI knobs (`enableNetworkPolicy`/`enablePrefixDelegation`/`enablePodEni`/`warm*Target`), and `Addon` carries `addonVersion`/`configurationValues`/`serviceAccountRoleArn`. `createManagedNodeGroup`/`createNodeGroupSecurityGroup`/`createStorageClass`/`getRoleProvider` compose the component rows in `.apply` folds.

[NODE_ENUMS]: `AmiType` `OperatingSystem` `ClusterNodePools` `ResolveConflictsOnCreate` `ResolveConflictsOnUpdate`

| [INDEX] | [SYMBOL]                      | [SHAPE_BOUNDARY]                                                                                    |
| :-----: | :---------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `ManagedNodeGroup`            | default capacity row (`cluster: Cluster \| CoreDataArgs` required) → `nodeGroup: aws.eks.NodeGroup` |
|  [02]   | `NodeGroupV2`                 | ASG-native self-managed group → `autoScalingGroup: aws.autoscaling.Group`                           |
|  [03]   | `Addon`                       | generic EKS-addon lifecycle (`addonName` required)                                                  |
|  [04]   | `VpcCniAddon`                 | CNI specialization (`clusterName` required); post-plane twin of `ClusterArgs.vpcCniOptions`         |
|  [05]   | `ClusterCreationRoleProvider` | `{ profile?, region? }` → `role: Output<aws.iam.Role>`; creator identity for `creationRoleProvider` |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- network: the plane rides the arm's existing `awsx.ec2.Vpc` — `vpcId`/`publicSubnetIds`/`privateSubnetIds` bind the network axis so network intent has one owner; `endpointPublicAccess: false` with `publicAccessCidrs` is the private-plane posture, decided by spec data.
- access: `authenticationMode: eks.AuthenticationMode.Api` with `accessEntries` is the access spelling — the arg takes the closed union, so the constant is the spelling and a bare literal states the same value while naming no vocabulary — an entry is `principalArn` with scoped `accessPolicies`, so cluster RBAC is data on the component.
- identity: `createOidcProvider: true` mints the IRSA anchors once; a workload identity is `Addon.serviceAccountRoleArn` or an `aws.iam.Role` trust-bound to `oidcProviderArn`, never a widened node instance role.
- capacity: `ManagedNodeGroup` is the default row, `NodeGroupV2` the launch-template escalation, `skipDefaultNodeGroup: true` wherever explicit groups exist so capacity has named owners; `fargate` and `autoMode` are spec-profile decisions on the same component. Every group pins `operatingSystem` off the exported const, because an unpinned family leaves image selection to a default the SDK moves — and pins the FAMILY rather than `RECOMMENDED`, whose value tracks AWS's own recommendation.
- addon: cluster addons ride `Addon`/`VpcCniAddon` under the `ResolveConflictsOnCreate`/`ResolveConflictsOnUpdate` vocabulary; a `helm.v4.Chart` owns only what the addon catalog does not carry.

[STACKING]:
- `@pulumi/awsx`(`.api/pulumi-awsx.md`): `ec2.Vpc` outputs `vpcId`/`publicSubnetIds`/`privateSubnetIds` bind the `ClusterArgs` network axis, so the EKS plane rides the arm's realized VPC graph.
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `cluster.kubeconfigJson` binds `new k8s.Provider({ kubeconfig, enableServerSideApply: true })`, and every `kube/*` row — `helm.v4.Chart` operators, `apiextensions.CustomResource` — rides the EKS plane through that one provider.
- `@pulumi/postgresql`(`.api/pulumi-postgresql.md`): the `kube/data` CNPG `Cluster` declared through the k8s provider exposes its `-rw` service host into `postgresql.Provider`, so the data plane finalizes over the EKS-hosted CNPG operator.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): the component's children are typed resources under Pulumi diff and CrossGuard, and a construction failure rejects the lifecycle operation and maps to `DeployFault`.
- within-lib: the `provider/dispatch` `aws` arm promotes to a k8s-shaped deployment by swapping only the provider binding — `ManagedNodeGroup`/`NodeGroupV2` capacity and `createOidcProvider` IRSA anchors ride the same `Cluster` the arm already constructs.

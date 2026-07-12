# [TS_IAC_API_PULUMI_EKS]

`@pulumi/eks` is the managed-Kubernetes escalation of the `aws` column — the one AWS surface neither `@pulumi/awsx` (VPC/ECS/ALB) nor `@pulumi/kubernetes` (in-cluster only) reaches: EKS control plane, access entries, IRSA/OIDC wiring, managed node groups, Fargate, and addon lifecycle as ONE `Cluster` component plus satellite node/addon components. Structurally it is the managed twin of the `Bootstrap` tier: where the selfhosted arm's `remote.Command` stdout becomes the kubeconfig, here `cluster.kubeconfigJson` is the same seam — a kubeconfig `Output` binding a `k8s.Provider` — so every `kube/*` tier rides an EKS plane unchanged, and promoting the `aws` column to a k8s-shaped estate is a provider-seam swap, not a tier rewrite. The component composes `@pulumi/aws` and `@pulumi/kubernetes` in-process, so its children are ordinary typed resources under Pulumi diff and CrossGuard.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/eks`
- package: `@pulumi/eks`
- license: Apache-2.0
- import: `@pulumi/eks` → `{ Cluster, ManagedNodeGroup, NodeGroupV2, NodeGroup, Addon, VpcCniAddon, NodeGroupSecurityGroup, ClusterCreationRoleProvider, Provider, getRoleProvider, createManagedNodeGroup, createNodeGroupSecurityGroup, createStorageClass, types }` + the enum consts `AuthenticationMode`, `AccessEntryType`, `AmiType`, `OperatingSystem`, `ClusterNodePools`, `ResolveConflictsOnCreate`, `ResolveConflictsOnUpdate`
- owner: `iac`
- rail: kubernetes / prepared-aws
- runtime: Node deploy-host; drives the AWS API and, post-plane, the EKS API server through the composed `@pulumi/kubernetes`
- depends-on: `@pulumi/aws`, `@pulumi/kubernetes`, `@pulumi/pulumi` (in-process — children are typed resources, not plugin-opaque)
- capability: EKS control plane with access-entry auth, OIDC/IRSA provisioning, managed/self-managed node groups, Fargate profiles, EKS Auto Mode, addon lifecycle with conflict-resolution vocabulary, kubeconfig egress with role/profile assumption
- abi-note: `kubeconfig` is `Output<any>` (the structured document); `kubeconfigJson` is the `Output<string>` a `k8s.Provider` binds; `NodeGroup` carries the deprecation marker — `ManagedNodeGroup` and the ASG-native `NodeGroupV2` are the live rows

## [02]-[CLUSTER_COMPONENT]

[CLUSTER_SCOPE]: `Cluster` — the one plane owner
- rail: prepared-aws
- `new Cluster(name, args?: ClusterArgs, opts?: pulumi.ComponentResourceOptions)`; `ClusterArgs` groups into axes — network (`vpcId`, `publicSubnetIds`, `privateSubnetIds`, `nodeSubnetIds`, `endpointPrivateAccess`/`endpointPublicAccess`, `publicAccessCidrs`, `ipFamily`, `kubernetesServiceIpAddressRange`), access (`authenticationMode`, `accessEntries`, the retired `roleMappings`/`userMappings` aws-auth rows), identity (`serviceRole`, `instanceRole(s)`, `createInstanceRole`, `createOidcProvider`, `creationRoleProvider`), default capacity (`instanceType`, `desiredCapacity`/`minSize`/`maxSize`, `nodeAmiId`, `nodeRootVolume*`, `skipDefaultNodeGroup`, `nodeGroupOptions`), plane posture (`version`, `upgradePolicy`, `deletionProtection`, `enabledClusterLogTypes`, `encryptionConfigKeyArn`, `skipDefaultSecurityGroups`), addons (`vpcCniOptions`, `corednsAddonOptions`, `kubeProxyAddonOptions`, `useDefaultVpcCni`, `bootstrapSelfManagedAddons`), serverless (`fargate: boolean | FargateProfileArgs`, `autoMode: AutoModeOptionsArgs`), and `storageClasses`.

| [INDEX] | [MEMBER]                                                                                                                 | [SHAPE_MEANING]                                                                                                                                                                   |
| :-----: | :----------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `cluster.kubeconfigJson`                                                                                                 | `Output<string>` — the provider seam; `kubeconfig` is its structured `Output<any>` twin                                                                                           |
|  [02]   | `cluster.getKubeconfig({ profileName?, roleArn? })`                                                                      | `Output<{ result: string }>` — a kubeconfig under an assumed role/profile for non-deploy-host consumers                                                                           |
|  [03]   | `cluster.provider`                                                                                                       | the ready-bound provider handle for in-cluster children; an explicit `k8s.Provider({ kubeconfig })` remains the arm-seam spelling                                                 |
|  [04]   | `cluster.eksCluster`                                                                                                     | `Output<aws.eks.Cluster>` — the underlying typed resource for raw-attribute reach                                                                                                 |
|  [05]   | `cluster.core`                                                                                                           | `Output<CoreData>` — the component's assembled internals (subnets, roles, security groups) satellite components consume                                                           |
|  [06]   | `cluster.oidcProviderArn` / `oidcProviderUrl` / `oidcIssuer`                                                             | the IRSA anchors minted under `createOidcProvider`                                                                                                                                |
|  [07]   | `cluster.clusterSecurityGroupId` / `nodeSecurityGroupId` / `instanceRoles` / `defaultNodeGroup` / `autoModeNodeRoleName` | realized security/identity/capacity evidence                                                                                                                                      |
|  [08]   | `cluster.createNodeGroup(name, ClusterNodeGroupOptionsArgs)`                                                             | mixin — a self-managed group bound to this plane                                                                                                                                  |
|  [09]   | `AccessEntryArgs`                                                                                                        | `{ principalArn, accessPolicies?: {[k]: { policyArn, accessScope }}, kubernetesGroups?, type?: AccessEntryType, username? }` — keyed by entry name on `ClusterArgs.accessEntries` |
|  [10]   | `AuthenticationMode.Api`                                                                                                 | the current mode; `ConfigMap` and `ApiAndConfigMap` carry deprecation markers with the aws-auth rows they feed                                                                    |

```ts contract
import * as eks from "@pulumi/eks"
import * as k8s from "@pulumi/kubernetes"

// the managed twin of Bootstrap.kubeconfig — same seam, managed plane
declare const cluster: eks.Cluster
const provider = new k8s.Provider("k8s", { kubeconfig: cluster.kubeconfigJson, enableServerSideApply: true })
```

## [03]-[NODES_AND_ADDONS]

[CAPACITY_SCOPE]: node groups — managed first
- rail: prepared-aws

| [INDEX] | [SYMBOL]                                                                         | [SHAPE_BOUNDARY]                                                                                                                                                                                                                                                                                                                                                   |
| :-----: | :------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ManagedNodeGroup`                                                               | `{ cluster: Cluster \| CoreDataArgs (required), instanceTypes?, scalingConfig?, capacityType?, amiType?, operatingSystem?, labels?, taints?, launchTemplate?, diskSize?, enableIMDSv2?, kubeletExtraArgs?, nodeadmExtraOptions?, releaseVersion?, forceUpdateVersion?, ignoreScalingChanges? }` → outputs `nodeGroup: aws.eks.NodeGroup`; the default capacity row |
|  [02]   | `NodeGroupV2`                                                                    | ASG-native self-managed group (`autoScalingGroup: aws.autoscaling.Group` output, `minRefreshPercentage`, `launchTemplateTagSpecifications`) — the row for launch-template control managed groups do not expose                                                                                                                                                     |
|  [03]   | `NodeGroup`                                                                      | DEPRECATED CloudFormation-backed twin (`cfnStack` output); never author it new                                                                                                                                                                                                                                                                                     |
|  [04]   | `Addon`                                                                          | `{ addonName (required), cluster (required), addonVersion?, configurationValues?, serviceAccountRoleArn?, preserve?, resolveConflictsOnCreate?, resolveConflictsOnUpdate? }` — the generic EKS-addon lifecycle row                                                                                                                                                 |
|  [05]   | `VpcCniAddon`                                                                    | the CNI specialization (`clusterName` required; `enableNetworkPolicy`, `enablePrefixDelegation`, `enablePodEni`, `warmEniTarget`/`warmIpTarget`/`warmPrefixTarget`, `customNetworkConfig`, `externalSnat`) — the post-plane twin of `ClusterArgs.vpcCniOptions`                                                                                                    |
|  [06]   | `ClusterCreationRoleProvider` / `getRoleProvider`                                | `{ profile?, region? }` → `role: Output<aws.iam.Role>` — a distinct creator identity for `ClusterArgs.creationRoleProvider`                                                                                                                                                                                                                                        |
|  [07]   | `createManagedNodeGroup` / `createNodeGroupSecurityGroup` / `createStorageClass` | function twins of the component rows for composition inside `.apply` folds                                                                                                                                                                                                                                                                                         |

## [04]-[IMPLEMENTATION_LAW]

[PLANE_TOPOLOGY]:
- escalation law: the `aws` arm's realized workload cell is `awsx.ecs.FargateService`; EKS is the k8s-shaped escalation — when an AWS estate needs the `kube/*` tiers (CNPG data plane, typed ingress, LGTM observe), the arm swaps its provider seam to `cluster.kubeconfigJson → k8s.Provider` and reuses the tier roster the selfhosted arm already realizes; the equivalence map's GKE↔workloads reading applies to the EKS cells identically.
- network law: the plane rides the arm's existing `awsx.ec2.Vpc` — `vpcId`, `publicSubnetIds`, `privateSubnetIds` bind the Vpc component's outputs so network intent has one owner; `endpointPublicAccess: false` plus `publicAccessCidrs` is the private-plane posture, decided by spec data.
- access law: `authenticationMode: "API"` with `accessEntries` rows is the only access spelling — the deprecated ConfigMap modes and their `roleMappings`/`userMappings` exist for adopting a retired plane, never for new ones; an entry is `principalArn` plus scoped `accessPolicies`, so cluster RBAC is data on the component.
- identity law: `createOidcProvider: true` mints the IRSA anchors once; a workload identity is `Addon.serviceAccountRoleArn` or an `aws.iam.Role` trust-bound to `oidcProviderArn` — node instance roles never widen to serve pod-level permissions.
- capacity law: `ManagedNodeGroup` is the default row, `NodeGroupV2` the launch-template escalation, `skipDefaultNodeGroup: true` whenever explicit groups exist so capacity has named owners; `fargate` and `autoMode` are spec-profile decisions on the same component, not parallel cluster kinds.
- addon law: cluster addons ride `Addon`/`VpcCniAddon` with the `ResolveConflictsOnCreate`/`ResolveConflictsOnUpdate` vocabulary — never a `helm.v4.Chart` re-install of what EKS manages natively; charts own only what the addon catalog does not carry.

[RAIL_LAW]:
- Package: `@pulumi/eks`
- Owns: the managed EKS plane — control plane, access entries, OIDC/IRSA, node capacity, Fargate/Auto Mode, addon lifecycle, kubeconfig egress
- Accept: `kubeconfigJson` into the arm's one `k8s.Provider` seam, `authenticationMode: "API"` + `accessEntries`, `ManagedNodeGroup` capacity, VPC outputs from `awsx.ec2.Vpc`, `createOidcProvider` IRSA anchors, typed `Addon` rows
- Reject: the deprecated `NodeGroup`/ConfigMap-auth rows in new work, hand-rolled `aws.eks.*` assemblies duplicating the component, kubeconfig literals or per-resource providers, node-role widening in place of IRSA, chart installs of native addons

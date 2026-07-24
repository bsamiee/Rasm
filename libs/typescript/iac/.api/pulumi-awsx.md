# [TS_IAC_API_PULUMI_AWSX]

`@pulumi/awsx` composes intent-level `ComponentResource` classes that expand one spec — an AZ+strategy, an ECS service, an ECR build — into the raw `@pulumi/aws` resource graph backing the `aws` dispatch row. Every class extends `ComponentResource`, so it carries no `id`, no `static get`, and no provider credential: the empty `ProviderArgs` marks that awsx rides the arm's `aws.Provider` via `opts.provider`, and its outputs are raw `aws` resources the arm wires further.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/awsx`
- package: `@pulumi/awsx` (Apache-2.0)
- import: `@pulumi/awsx` → `{ ec2, ecs, ecr, lb, cloudtrail, types, Provider }`
- owner: `iac`
- rail: cloud-row / aws (ComponentResource tier)
- runtime: the `aws.Provider` credential context; `ecr.Image` build also needs a local Docker/buildx CLI on the deploy host
- build-floor: `@pulumi/pulumi` `^catalog`
- depends-on: `@pulumi/pulumi` (`ComponentResource`/`Output`), `@pulumi/aws` (every arg/output is a raw `aws` type), `@pulumi/docker-build` (the bundled `ecr.Image` buildx builder)
- namespaces: `awsx.ec2` (Vpc/DefaultVpc), `awsx.ecs` (Fargate/EC2 Service+TaskDefinition), `awsx.ecr` (Repository/Image/RegistryImage), `awsx.lb` (Application/Network LB + TargetGroupAttachment), `awsx.cloudtrail` (Trail), `awsx.types`
- capability: intent-spec compositions expanded into the raw `aws` resource graph
- abi-note: outputs are raw `pulumiAws.*` resources (`vpc.subnets: aws.ec2.Subnet[]`, `alb.loadBalancer: aws.lb.LoadBalancer`), never scalars — a composition exposes its sub-resources for further wiring

## [02]-[COMPOSITION_ABI]

[ABI_SCOPE]: the parameterized ComponentResource shape every class instantiates

One shape owns every family: `new X(name, XArgs, opts?: pulumi.ComponentResourceOptions)` — intent args in, raw `pulumiAws.*` sub-resources out, a new composition one row on this pattern. `opts.provider` threads the arm's `aws.Provider` and `opts.parent` the stack tier; a ComponentResource is authored, never adopted, and its outputs wire into the next composition's `Input`.

| [INDEX] | [MEMBER]                               | [SHAPE_BOUNDARY]                                                                  |
| :-----: | :------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `new X(name, XArgs, opts?)`            | compose; `opts.provider` = the `aws` arm provider, `opts.parent` = the stack tier |
|  [02]   | `X.isInstance(obj)`                    | multi-SDK-safe guard `obj is X`                                                   |
|  [03]   | `x.<subResource>: Output<pulumiAws.*>` | the expanded raw `aws` resources — wire into the next composition's `Input`       |

## [03]-[COMPOSITION_FAMILIES]

[NETWORK_SCOPE]: `awsx.ec2` — VPC topology
- `Vpc(VpcArgs) -> { vpcId, publicSubnetIds, privateSubnetIds, isolatedSubnetIds, subnets, natGateways, internetGateway, vpc }`; args `cidrBlock, numberOfAvailabilityZones, natGateways, subnetSpecs, subnetStrategy, vpcEndpointSpecs`. Subnet ids feed every compute and LB composition.

| [INDEX] | [SYMBOL]         | [ROLE]                                               |
| :-----: | :--------------- | :--------------------------------------------------- |
|  [01]   | `ec2.Vpc`        | AZ+strategy intent → full subnet/NAT/IGW/route graph |
|  [02]   | `ec2.DefaultVpc` | adopt the account default VPC + its subnets          |

[COMPUTE_SCOPE]: `awsx.ecs` — ECS services
- `FargateService`/`EC2Service(args)` compose service + inline `taskDefinitionArgs` + log group + IAM in one resource; args `cluster, desiredCount, networkConfiguration, loadBalancers, taskDefinitionArgs, deploymentCircuitBreaker, assignPublicIp`.
- `FargateTaskDefinition`/`EC2TaskDefinition(container | containers)` → image + log group + exec role, the standalone task-def compositions.

| [INDEX] | [SYMBOL]                                              | [ROLE]                                      |
| :-----: | :---------------------------------------------------- | :------------------------------------------ |
|  [01]   | `ecs.FargateService` / `ecs.EC2Service`               | service + inline task-def + log group + IAM |
|  [02]   | `ecs.FargateTaskDefinition` / `ecs.EC2TaskDefinition` | standalone task-def composition             |

[IMAGE_SCOPE]: `awsx.ecr` — repository + build-and-push
- `Repository(args) -> { repository, url, lifecyclePolicy }` composes an ECR repo + lifecycle policy; args `imageScanningConfiguration, imageTagMutability, encryptionConfigurations, lifecyclePolicy, forceDelete`.
- `Image(args) -> imageUri` builds a Dockerfile through the bundled `@pulumi/docker-build` and pushes the pinned ref; args `repositoryUrl, context, dockerfile, cacheFrom, platform, target, builderVersion`.

| [INDEX] | [SYMBOL]            | [ROLE]                                       |
| :-----: | :------------------ | :------------------------------------------- |
|  [01]   | `ecr.Repository`    | ECR repo + lifecycle policy                  |
|  [02]   | `ecr.Image`         | Dockerfile build+push → the pushed image ref |
|  [03]   | `ecr.RegistryImage` | push an existing local image to a repo       |

[TRAFFIC_SCOPE]: `awsx.lb` — load balancers
- `ApplicationLoadBalancer`/`NetworkLoadBalancer -> { loadBalancer, defaultTargetGroup, listeners, defaultSecurityGroup, vpcId }` compose the LB + default listener + target group + security group; `defaultTargetGroup` wires into an ECS service's `loadBalancers`.

| [INDEX] | [SYMBOL]                                                | [ROLE]                                                |
| :-----: | :------------------------------------------------------ | :---------------------------------------------------- |
|  [01]   | `lb.ApplicationLoadBalancer` / `lb.NetworkLoadBalancer` | LB + default listener + target group + security group |
|  [02]   | `lb.TargetGroupAttachment`                              | attach an instance/IP/Lambda target to a group        |

[AUDIT_SCOPE]: `awsx.cloudtrail`
- `cloudtrail.Trail` composes a trail + its S3 bucket + optional CloudWatch log group in one resource.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- awsx is the ComponentResource tier of the `aws` arm: it expands INTENT (AZ count, NAT strategy, subnet spec) into the raw `aws` graph, then the arm wires the exposed sub-resources further — `vpc.privateSubnetIds` → `FargateService.networkConfiguration`, `alb.defaultTargetGroup` → `FargateService.loadBalancers`; awsx composes `@pulumi/aws`, never replaces it.
- raw sub-resources (`vpcId`, subnet ids, `loadBalancer.dnsName`) project through typed `StackOutputs` (`stack/output`) into the service-equivalence map; a scalar wires via `Input`, never re-read from aws.

[STACKING]:
- `@pulumi/aws`(`.api/pulumi-aws.md`): every awsx output is a raw `aws.*` resource (`vpc.subnets: aws.ec2.Subnet[]`, `alb.loadBalancer: aws.lb.LoadBalancer`) — the wiring currency the arm threads into the next resource's `Input` and the return type of every output.
- `@pulumi/docker-build`(`.api/pulumi-docker-build.md`): `ecr.Image` bundles it as the buildx builder, and the ECR-pushed `imageUri` digest is the aws-arm counterpart to a direct `docker-build.Image` `ref`/`digest` selected by dispatch arm — canonical build ownership lives at that catalog's `[05]-[IMPLEMENTATION_LAW]`.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): the shared `aws.Provider` creds decode from the `StackSpec` Doppler secret Output.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): `opts.provider`/`opts.parent` thread the arm provider and the `stack/component` tier; a composition failure folds into the `automation.UpResult` run receipt.
- within-lib: the `aws` `Match.exhaustive` arm (`effect`, `libs/typescript/.api/effect.md`) `Schema`-decodes a `StackSpec`, constructs the raw `aws.Provider`, threads it to every awsx composition via `opts.provider`, and projects the raw sub-resources into typed `StackOutputs`.

[LOCAL_ADMISSION]:
- awsx carries no provider credential of its own — the empty `ProviderArgs` marks that every composition rides the arm's single `aws.Provider` via `opts.provider`.
- an `ecr.Image` push and a direct `docker-build.Image` build are the two arm-specific build egresses one buildx-native builder feeds — the mixed stack selects between them by dispatch arm.

[RAIL_LAW]:
- Package: `@pulumi/awsx`
- Owns: opinionated ComponentResource compositions for the `aws` row — VPC topology, ECS Fargate/EC2 services, ECR build-and-push, load balancers, CloudTrail
- Accept: modern-module compositions under the arm's `aws.Provider` via `opts.provider`, intent-spec inputs, raw-`aws`-resource output wiring, `ecr.Image` buildx push, `opts.parent` tiering
- Reject: an awsx-level provider credential, re-reading a scalar output, hand-rolling the raw `aws` graph awsx already composes, mutable-tag ECR image refs

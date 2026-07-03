# [TS_IAC_API_PULUMI_AWSX]

`@pulumi/awsx` is the higher-level ComponentResource crosswalk backing the prepared `aws` dispatch row — intent-level compositions that expand to dozens of raw `@pulumi/aws` resources. It is NOT a bridged provider: every class extends `pulumi.ComponentResource` (not `CustomResource`), so there is no `static get`, no `id`, and no `Provider` credential of its own — an empty `ProviderArgs` marks that awsx components ride the ambient/passed `aws.Provider` via `opts.provider`. One composition ABI (`new X(name, XArgs, opts?: ComponentResourceOptions)`, intent args in, raw `pulumiAws.*` sub-resources out) owns five families: `ec2.Vpc` (subnet/NAT/IGW/route topology from an AZ+strategy spec), `ecs.FargateService`/`EC2Service` (+ TaskDefinition), `ecr.Repository`/`Image` (build-and-push to ECR via bundled `@pulumi/docker-build`), `lb.ApplicationLoadBalancer`/`NetworkLoadBalancer`, and `cloudtrail.Trail`. awsx bundles `@pulumi/aws ^7.32.0` + `@pulumi/docker-build` as its own deps, so its outputs are typed `aws` resources the `aws` arm composes further.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/awsx`
- package: `@pulumi/awsx`
- version: `3.6.0`
- license: Apache-2.0
- import: `@pulumi/awsx` → `{ ec2, ecs, ecr, lb, cloudtrail, classic, types, Provider }`
- owner: `iac`
- rail: cloud-row / aws (ComponentResource tier)
- runtime: the `aws.Provider` credential context; `ecr.Image` build also needs a local Docker/buildx CLI on the deploy host
- build-floor: `@pulumi/pulumi` `^3.142.0` (catalog pins `3.250.0`)
- depends-on: `@pulumi/pulumi` (`ComponentResource`/`Output`), `@pulumi/aws ^7.32.0` (every arg/output is a raw `aws` type — awsx composes it, `.api/pulumi-aws.md`), `@pulumi/docker-build` (the `ecr.Image` buildx path)
- namespaces: `awsx.ec2` (Vpc/DefaultVpc), `awsx.ecs` (Fargate/EC2 Service+TaskDefinition), `awsx.ecr` (Repository/Image/RegistryImage), `awsx.lb` (Application/Network LB + TargetGroupAttachment), `awsx.cloudtrail` (Trail), `awsx.classic` (legacy pre-schema components — superseded), `awsx.types`
- capability: opinionated multi-resource compositions (VPC topology, ECS services, ECR build+push, load balancers, CloudTrail) that expand an intent spec into the raw `aws` resource graph
- abi-note: outputs are raw `pulumiAws.*` resources (`vpc.subnets: aws.ec2.Subnet[]`, `alb.loadBalancer: aws.lb.LoadBalancer`), NOT scalars — the composition exposes its sub-resources for further wiring

## [02]-[COMPOSITION_ABI]

[ABI_SCOPE]: the parameterized ComponentResource shape every class instantiates
- rail: aws
- One shape owns all five families; a new composition is a row on this pattern. Construction is `new X(name, XArgs, opts?: pulumi.ComponentResourceOptions)`; `opts.parent`/`opts.provider` thread the `aws` arm's provider and the parent tier (`.api/pulumi-pulumi.md`). There is NO `static get` — a ComponentResource is authored, never adopted by id. Intent args (an AZ count, a NAT strategy, a subnet spec) expand to the raw `aws` graph; outputs expose those raw resources for downstream `Input` wiring.

| [INDEX] | [MEMBER] | [SHAPE / BOUNDARY] |
| :-----: | :------- | :----------------- |
|  [01]   | `new X(name, XArgs, opts?)` | compose; `opts.provider` = the `aws` arm provider, `opts.parent` = the stack tier |
|  [02]   | `X.isInstance(obj)` | multi-SDK-safe guard `obj is X` |
|  [03]   | `x.<subResource>: Output<pulumiAws.*>` | the expanded raw `aws` resources — wire into the next composition's `Input` |

## [03]-[COMPOSITION_FAMILIES]

[NETWORK_SCOPE]: `awsx.ec2` — VPC topology
- rail: aws
- `Vpc` turns an AZ+strategy intent into the full subnet/NAT/IGW/route graph; `DefaultVpc` adopts the account default. The subnet ids feed every compute/LB composition.

| [INDEX] | [SYMBOL] | [INTENT ARGS → OUTPUTS] |
| :-----: | :------- | :---------------------- |
|  [01]   | `ec2.Vpc` | `{ cidrBlock, numberOfAvailabilityZones, natGateways, subnetSpecs, subnetStrategy, vpcEndpointSpecs }` → `vpcId`, `publicSubnetIds`/`privateSubnetIds`/`isolatedSubnetIds`, `subnets: aws.ec2.Subnet[]`, `natGateways`, `internetGateway`, `vpc: aws.ec2.Vpc` |
|  [02]   | `ec2.DefaultVpc` | adopt the account default VPC + its subnets |

[COMPUTE_SCOPE]: `awsx.ecs` — ECS services
- rail: aws
- `FargateService`/`EC2Service` compose the service + inline `taskDefinitionArgs` (container/image/logging/role) + log group + IAM in one resource; the TaskDefinition variants are the standalone task-def compositions.

| [INDEX] | [SYMBOL] | [KEY ARGS] |
| :-----: | :------- | :--------- |
|  [01]   | `ecs.FargateService` / `ecs.EC2Service` | `{ cluster, desiredCount, networkConfiguration, loadBalancers, taskDefinitionArgs, deploymentCircuitBreaker, assignPublicIp }` |
|  [02]   | `ecs.FargateTaskDefinition` / `ecs.EC2TaskDefinition` | standalone task-def composition (`container`/`containers` → `image`, log group, exec role) |

[IMAGE_SCOPE]: `awsx.ecr` — repository + build-and-push
- rail: aws
- `Repository` composes an ECR repo + lifecycle policy; `Image` builds a Dockerfile (via bundled `@pulumi/docker-build`) and pushes to that repo, emitting the pushed image ref for a task definition.

| [INDEX] | [SYMBOL] | [INTENT ARGS → OUTPUTS] |
| :-----: | :------- | :---------------------- |
|  [01]   | `ecr.Repository` | `{ imageScanningConfiguration, imageTagMutability, encryptionConfigurations, lifecyclePolicy, forceDelete }` → `repository: aws.ecr.Repository`, `url`, `lifecyclePolicy` |
|  [02]   | `ecr.Image` | `{ repositoryUrl, context, dockerfile, cacheFrom, platform, target, builderVersion }` → pushed image ref |
|  [03]   | `ecr.RegistryImage` | push an existing local image to a repo |

[TRAFFIC_SCOPE]: `awsx.lb` — load balancers
- rail: aws
- `ApplicationLoadBalancer`/`NetworkLoadBalancer` compose the LB + default listener + target group + security group; the `defaultTargetGroup` wires into an ECS service's `loadBalancers`.

| [INDEX] | [SYMBOL] | [OUTPUTS] |
| :-----: | :------- | :-------- |
|  [01]   | `lb.ApplicationLoadBalancer` / `lb.NetworkLoadBalancer` | `loadBalancer: aws.lb.LoadBalancer`, `defaultTargetGroup: aws.lb.TargetGroup`, `listeners`, `defaultSecurityGroup`, `vpcId` |
|  [02]   | `lb.TargetGroupAttachment` | attach an instance/IP/Lambda target to a group |

[AUDIT_SCOPE]: `awsx.cloudtrail`
- rail: aws
- `cloudtrail.Trail` composes a trail + its S3 bucket + optional CloudWatch log group in one resource.

## [04]-[IMPLEMENTATION_LAW]

[AWSX_TOPOLOGY]:
- tier law: awsx is the `stack/component` ComponentResource tier of the `aws` arm — the `aws` `Match.exhaustive` arm (`provider/dispatch`, `libs/typescript/.api/effect.md`) constructs the raw `aws.Provider` from a `Schema`-decoded `StackSpec` and passes it to every awsx composition via `opts.provider`; the empty awsx `ProviderArgs` is never populated.
- compose law: awsx expands INTENT (AZ count, NAT strategy, subnet spec) into the raw `aws` graph, then the arm wires the exposed raw sub-resources further — `vpc.privateSubnetIds` → `FargateService.networkConfiguration`, `alb.defaultTargetGroup` → `FargateService.loadBalancers`; awsx composes `@pulumi/aws`, never replaces it.
- image law: `ecr.Repository.url` + `ecr.Image` (build+push via bundled `@pulumi/docker-build`) is the aws-arm build counterpart to `@pulumi/docker.Image` on the selfhosted arm (`.api/pulumi-docker.md`); the pushed ref pins a TaskDefinition container image.
- output law: the composition's raw sub-resources (`vpcId`, subnet ids, `loadBalancer.dnsName`) project into typed StackOutputs (`stack/output`) that feed the service-equivalence map; a scalar is never re-derived by re-reading aws.
- legacy law: `awsx.classic` is the pre-schema component set — superseded by the top-level `ec2`/`ecs`/`ecr`/`lb` modules; author the modern module, never `classic`.

[LOCAL_ADMISSION]:
- awsx components share the `aws` arm's single `aws.Provider` (creds from `StackSpec`/`@pulumiverse/doppler`, `.api/pulumiverse-doppler.md`); raw `@pulumi/aws` resources (`.api/pulumi-aws.md`) are the wiring currency between compositions and the return type of every output.
- `ecr.Image` shares the buildx path with `@pulumi/docker-build`; the pushed image digest and `@pulumi/docker.Image.repoDigest` are the two arm-specific build outputs a mixed stack selects between by dispatch arm.
- the arm folds composition failures into the `program/automation` typed run receipt (`@pulumi/pulumi` `automation.UpResult`, `.api/pulumi-pulumi.md`); `effect` owns arm dispatch and the StackSpec/StackOutputs `Schema`; `opts.parent` threads the ComponentResource hierarchy of `stack/component`.
- canonical law: the modern `ec2`/`ecs`/`ecr`/`lb` modules over `classic`; raw-resource outputs wired via `Input`, never re-read.

[RAIL_LAW]:
- Package: `@pulumi/awsx`
- Owns: opinionated ComponentResource compositions for the `aws` row — VPC topology, ECS Fargate/EC2 services, ECR build-and-push, load balancers, CloudTrail
- Accept: modern-module compositions under the arm's `aws.Provider` via `opts.provider`, intent-spec inputs, raw-`aws`-resource output wiring, `ecr.Image` buildx push, `opts.parent` tiering
- Reject: an awsx-level provider credential, `awsx.classic` legacy components, re-reading a scalar output, hand-rolling the raw `aws` graph awsx already composes, mutable-tag ECR image refs

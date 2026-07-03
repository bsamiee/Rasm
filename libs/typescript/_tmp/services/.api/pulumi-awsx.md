# [API_CATALOGUE] @pulumi/awsx

`@pulumi/awsx` supplies higher-level AWS component resources built on top of `@pulumi/aws`: an opinionated `Vpc` with automatic subnet layout and NAT gateways, `FargateService` / `FargateTaskDefinition` / `EC2Service` / `EC2TaskDefinition` for ECS, `ApplicationLoadBalancer` / `NetworkLoadBalancer` for load balancing, and `Repository` / `Image` for ECR. All components extend `pulumi.ComponentResource` and expose `pulumi.Output`-typed output properties that wrap the underlying `@pulumi/aws` resource instances.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/awsx`
- package: `@pulumi/awsx`
- module: `@pulumi/awsx`
- asset: opinionated VPC, ECS Fargate/EC2 components, ALB/NLB, ECR components
- rail: deployment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EC2 networking component family
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [RAIL]                                                   |
| :-----: | :-------------------- | :----------------- | :------------------------------------------------------- |
|  [01]   | `awsx.ec2.Vpc`        | component resource | VPC + subnets + NAT gateways + route tables              |
|  [02]   | `awsx.ec2.VpcArgs`    | args interface     | `cidrBlock`, `natGateways`, `subnetSpecs`, `numberOfAzs` |
|  [03]   | `awsx.ec2.DefaultVpc` | component resource | reference to the account default VPC                     |

[PUBLIC_TYPE_SCOPE]: Vpc output properties
- rail: deployment

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]                     | [RAIL]                       |
| :-----: | :---------------------- | :-------------------------------- | :--------------------------- |
|  [01]   | `Vpc.vpcId`             | `Output<string>`                  | VPC ID                       |
|  [02]   | `Vpc.vpc`               | `Output<aws.ec2.Vpc>`             | underlying VPC resource      |
|  [03]   | `Vpc.publicSubnetIds`   | `Output<string[]>`                | public subnet IDs            |
|  [04]   | `Vpc.privateSubnetIds`  | `Output<string[]>`                | private subnet IDs           |
|  [05]   | `Vpc.isolatedSubnetIds` | `Output<string[]>`                | isolated subnet IDs          |
|  [06]   | `Vpc.publicSubnets`     | `Output<aws.ec2.Subnet[]>`        | public subnet resources      |
|  [07]   | `Vpc.privateSubnets`    | `Output<aws.ec2.Subnet[]>`        | private subnet resources     |
|  [08]   | `Vpc.natGateways`       | `Output<aws.ec2.NatGateway[]>`    | NAT gateway resources        |
|  [09]   | `Vpc.internetGateway`   | `Output<aws.ec2.InternetGateway>` | internet gateway             |
|  [10]   | `Vpc.eips`              | `Output<aws.ec2.Eip[]>`           | elastic IPs for NAT gateways |

[PUBLIC_TYPE_SCOPE]: ECS component family
- rail: deployment

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [RAIL]                                          |
| :-----: | :------------------------------- | :----------------- | :---------------------------------------------- |
|  [01]   | `awsx.ecs.FargateService`        | component resource | ECS service on Fargate launch type              |
|  [02]   | `awsx.ecs.FargateServiceArgs`    | args interface     | `cluster`, `taskDefinitionArgs`, `desiredCount` |
|  [03]   | `awsx.ecs.FargateTaskDefinition` | component resource | Fargate task definition                         |
|  [04]   | `awsx.ecs.EC2Service`            | component resource | ECS service on EC2 launch type                  |
|  [05]   | `awsx.ecs.EC2TaskDefinition`     | component resource | EC2 task definition                             |

[PUBLIC_TYPE_SCOPE]: FargateService output properties
- rail: deployment

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]                                 | [RAIL]                               |
| :-----: | :------------------------------ | :-------------------------------------------- | :----------------------------------- |
|  [01]   | `FargateService.service`        | `Output<aws.ecs.Service>`                     | underlying ECS service resource      |
|  [02]   | `FargateService.taskDefinition` | `Output<aws.ecs.TaskDefinition \| undefined>` | task definition if created from args |

[PUBLIC_TYPE_SCOPE]: load balancer component family
- rail: deployment

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]      | [RAIL]                                         |
| :-----: | :------------------------------------ | :----------------- | :--------------------------------------------- |
|  [01]   | `awsx.lb.ApplicationLoadBalancer`     | component resource | ALB + default target group + listener          |
|  [02]   | `awsx.lb.ApplicationLoadBalancerArgs` | args interface     | `subnetIds`, `defaultTargetGroup`, `listeners` |
|  [03]   | `awsx.lb.NetworkLoadBalancer`         | component resource | NLB + target groups                            |
|  [04]   | `awsx.lb.TargetGroupAttachment`       | component resource | target group attachment                        |

[PUBLIC_TYPE_SCOPE]: ECR component family
- rail: deployment

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [RAIL]                                                         |
| :-----: | :------------------------ | :----------------- | :------------------------------------------------------------- |
|  [01]   | `awsx.ecr.Repository`     | component resource | ECR repository + lifecycle policy                              |
|  [02]   | `awsx.ecr.RepositoryArgs` | args interface     | `lifecyclePolicy`, `forceDelete`, `imageScanningConfiguration` |
|  [03]   | `awsx.ecr.Image`          | component resource | builds and pushes Docker image to ECR                          |
|  [04]   | `awsx.ecr.RegistryImage`  | component resource | ECR registry image reference                                   |

[PUBLIC_TYPE_SCOPE]: Repository output properties
- rail: deployment

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]                                  | [RAIL]                         |
| :-----: | :--------------------------- | :--------------------------------------------- | :----------------------------- |
|  [01]   | `Repository.repository`      | `Output<aws.ecr.Repository>`                   | underlying ECR repository      |
|  [02]   | `Repository.lifecyclePolicy` | `Output<aws.ecr.LifecyclePolicy \| undefined>` | lifecycle policy if configured |
|  [03]   | `Repository.url`             | `Output<string>`                               | full repository URL            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: component constructors
- rail: deployment

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new awsx.ec2.Vpc(name, args?, opts?)`                       | constructor    | full VPC stack with subnets and NAT gateways |
|  [02]   | `new awsx.ec2.DefaultVpc(name, args?, opts?)`                | constructor    | reference to account default VPC             |
|  [03]   | `awsx.ec2.getDefaultVpc(args?, opts?): Promise<Result>`      | async lookup   | default VPC lookup                           |
|  [04]   | `awsx.ec2.getDefaultVpcOutput(args?, opts?): Output<Result>` | output lookup  | default VPC lookup, `Output`-wrapped         |
|  [05]   | `new awsx.ecs.FargateService(name, args?, opts?)`            | constructor    | Fargate ECS service                          |
|  [06]   | `new awsx.ecs.FargateTaskDefinition(name, args?, opts?)`     | constructor    | Fargate task definition                      |
|  [07]   | `new awsx.ecs.EC2Service(name, args?, opts?)`                | constructor    | EC2 ECS service                              |
|  [08]   | `new awsx.ecs.EC2TaskDefinition(name, args?, opts?)`         | constructor    | EC2 task definition                          |
|  [09]   | `new awsx.lb.ApplicationLoadBalancer(name, args?, opts?)`    | constructor    | ALB with default listener and target group   |
|  [10]   | `new awsx.lb.NetworkLoadBalancer(name, args?, opts?)`        | constructor    | NLB                                          |
|  [11]   | `new awsx.ecr.Repository(name, args?, opts?)`                | constructor    | ECR repository with lifecycle policy         |
|  [12]   | `new awsx.ecr.Image(name, args?, opts?)`                     | constructor    | build + push image to ECR                    |

## [04]-[IMPLEMENTATION_LAW]

[COMPONENT_TOPOLOGY]:
- All `awsx` components extend `pulumi.ComponentResource`; they call `registerOutputs` to declare their output map
- `FargateService` wraps `aws.ecs.Service` and optionally `aws.ecs.TaskDefinition`; the underlying resources are accessible via `.service` and `.taskDefinition` outputs
- `Vpc` creates one public and one private subnet per AZ by default; set `natGateways.strategy` to `"None"` to skip NAT gateways in dev environments
- `Vpc` subnet layout strategies: `"Legacy"` (default), `"Auto"`, `"AutoMerge"`, `"Exact"`, or explicit `cidrBlocks` per subnet spec
- `Repository.url` is the full ECR URL in the form `<account>.dkr.ecr.<region>.amazonaws.com/<name>`; pass to `Image.imageName` or `docker.Image` for builds

[LOCAL_ADMISSION]:
- `FargateServiceArgs.taskDefinitionArgs` creates an inline `FargateTaskDefinition`; omit it and pass `taskDefinition` to reuse an existing task definition ARN
- `ApplicationLoadBalancerArgs.defaultTargetGroup` configures the default target group; additional target groups attach via `TargetGroupAttachment`
- `awsx.ec2.Vpc.privateSubnetIds` and `publicSubnetIds` are `Output<string[]>` and feed directly into ECS service `networkConfiguration.subnets`

[RAIL_LAW]:
- Package: `@pulumi/awsx`
- Owns: opinionated AWS component resources
- Accept: `awsx` components as the primary surface for VPC, Fargate, ALB, and ECR; use `@pulumi/aws` directly only for resources `awsx` does not cover
- Reject: manually constructing VPCs, subnets, and route tables when `awsx.ec2.Vpc` covers the topology

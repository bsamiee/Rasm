# [API_CATALOGUE] @pulumi/awsx

`@pulumi/awsx` supplies opinionated multi-resource AWS component resources over `@pulumi/aws`: `awsx.ec2.Vpc` (automatic subnet layout + NAT gateways + route tables), `awsx.ecs.FargateService`/`FargateTaskDefinition`/`EC2Service`/`EC2TaskDefinition`, `awsx.lb.ApplicationLoadBalancer`/`NetworkLoadBalancer` (LB + default target group + listener), and `awsx.ecr.Repository`/`Image`/`RegistryImage` (repository + lifecycle policy + Docker build/push). Every component extends `pulumi.ComponentResource` (sibling `.api/pulumi-pulumi.md`) and exposes `pulumi.Output`-typed properties that wrap the underlying `@pulumi/aws` instances (sibling `.api/pulumi-aws.md`). In `services` it is the `DeployMode:"cloud"` network/compute/registry builder for `provisioning/contract#PROVISIONING`: `Vpc` stands up the network, `FargateService` the API workload, `ApplicationLoadBalancer` the ingress, and `Repository`+`Image` the build-and-push feeding the container. The whole set is reachable only behind the `./provisioning` exports subpath, never on the durable hot path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/awsx`
- package: `@pulumi/awsx` (3.6.0, Apache-2.0, © Pulumi Corporation)
- module format: CommonJS with generated `.d.ts`; the barrel re-exports the `ec2`/`ecs`/`lb`/`ecr`/`cloudtrail`/`classic`/`types` namespaces + `Provider`; components use the dual `type`/`const` export pattern; peers `@pulumi/aws` ^7.x + `@pulumi/pulumi` ^3.x + `@pulumi/docker`
- runtime target: node deploy-time only — behind the `./provisioning` subpath so the `@pulumi/*` closure never enters the durable hot path
- surface: multi-resource `ComponentResource` classes — `ec2.Vpc`/`DefaultVpc`, `ecs.FargateService`/`FargateTaskDefinition`/`EC2Service`/`EC2TaskDefinition`, `lb.ApplicationLoadBalancer`/`NetworkLoadBalancer`/`TargetGroupAttachment`, `ecr.Repository`/`Image`/`RegistryImage` — each with an `*Args` and `Output`-typed output properties wrapping `@pulumi/aws` instances; `ec2.getDefaultVpc`/`getDefaultVpcOutput`; the `SubnetAllocationStrategy`/`NatGatewayStrategy`/`SubnetType` enums
- consumer: `provisioning/contract#PROVISIONING` cloud `TierStack`; wraps `@pulumi/aws` (`.api/pulumi-aws.md`); `ComponentResource`/`Output` from `@pulumi/pulumi` (`.api/pulumi-pulumi.md`)
- rail: deployment/aws-components

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ec2 networking component + args

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]      | [CAPABILITY]                                                                             |
| :-----: | :--------------------------- | :----------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `awsx.ec2.Vpc`               | component resource | VPC + subnets + NAT gateways + route tables in one component                            |
|  [02]   | `awsx.ec2.VpcArgs`           | args interface     | `cidrBlock?`, `numberOfAvailabilityZones?`, `availabilityZoneNames?`, `natGateways?: NatGatewayConfigurationArgs`, `subnetSpecs?: SubnetSpecArgs[]`, `subnetStrategy?: SubnetAllocationStrategy` |
|  [03]   | `awsx.ec2.DefaultVpc` / `DefaultVpcArgs` | component resource | reference to the account default VPC                                          |

[PUBLIC_TYPE_SCOPE]: Vpc output properties (all `pulumi.Output<...>`, wrapping `@pulumi/aws` instances)

| [INDEX] | [SYMBOL]                       | [TYPE]                             | [CAPABILITY]                          |
| :-----: | :----------------------------- | :-------------------------------- | :------------------------------------ |
|  [01]   | `Vpc.vpcId` / `Vpc.vpc`        | `Output<string>` / `Output<aws.ec2.Vpc>` | VPC ID / underlying VPC resource |
|  [02]   | `Vpc.publicSubnetIds` / `privateSubnetIds` / `isolatedSubnetIds` | `Output<string[]>` | subnet IDs by tier — feed ECS/ALB directly |
|  [03]   | `Vpc.publicSubnets` / `privateSubnets` / `subnets` | `Output<aws.ec2.Subnet[]>` | subnet resources by tier / all subnets |
|  [04]   | `Vpc.natGateways` / `eips`     | `Output<aws.ec2.NatGateway[]>` / `Output<aws.ec2.Eip[]>` | NAT gateways + their elastic IPs |
|  [05]   | `Vpc.internetGateway` / `routeTables` | `Output<aws.ec2.InternetGateway>` / `Output<aws.ec2.RouteTable[]>` | IGW / route tables |
|  [06]   | `Vpc.vpcEndpoints`             | `Output<aws.ec2.VpcEndpoint[]>`   | interface/gateway VPC endpoints        |

[PUBLIC_TYPE_SCOPE]: ecs component family + FargateService outputs

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [CAPABILITY]                                                                             |
| :-----: | :------------------------------- | :----------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `awsx.ecs.FargateService`        | component resource | ECS service on Fargate launch type                                                      |
|  [02]   | `awsx.ecs.FargateServiceArgs`    | args interface     | `cluster?`, `desiredCount?`, `taskDefinitionArgs?: FargateServiceTaskDefinitionArgs`, `taskDefinition?` (existing ARN), `assignPublicIp?`, `networkConfiguration?`, `continueBeforeSteadyState?` |
|  [03]   | `awsx.ecs.FargateTaskDefinition` / `EC2Service` / `EC2TaskDefinition` | component resource | Fargate task def / EC2-launch service + task def       |
|  [04]   | `FargateService.service` / `taskDefinition` | `Output<aws.ecs.Service>` / `Output<aws.ecs.TaskDefinition \| undefined>` | underlying service / task def (present only when created from `taskDefinitionArgs`) |

[PUBLIC_TYPE_SCOPE]: load balancer component family + ALB outputs

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]      | [CAPABILITY]                                                                             |
| :-----: | :------------------------------------ | :----------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `awsx.lb.ApplicationLoadBalancer`     | component resource | ALB + default target group + default listener in one component                          |
|  [02]   | `awsx.lb.ApplicationLoadBalancerArgs` | args interface     | `subnetIds?`, `defaultTargetGroup?`, `listeners?`, `defaultTargetGroupPort?`, `securityGroups?` |
|  [03]   | `awsx.lb.NetworkLoadBalancer` / `TargetGroupAttachment` | component resource | NLB + target groups / target-group attachment                        |
|  [04]   | `ApplicationLoadBalancer.loadBalancer` / `defaultTargetGroup` / `listeners` / `vpcId` | `Output<aws.lb.LoadBalancer>` / `Output<aws.lb.TargetGroup>` / `Output<aws.lb.Listener[] \| undefined>` / `Output<string \| undefined>` | LB / default target group / listeners / VPC ID |

[PUBLIC_TYPE_SCOPE]: ecr component family + Repository/Image shapes

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                                             |
| :-----: | :------------------------ | :----------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `awsx.ecr.Repository`     | component resource | ECR repository + lifecycle policy                                                       |
|  [02]   | `awsx.ecr.RepositoryArgs` | args interface     | `lifecyclePolicy?`, `forceDelete?`, `imageScanningConfiguration?`, `imageTagMutability?` |
|  [03]   | `awsx.ecr.Image`          | component resource | build + push a Docker image; `repositoryUrl` (required), `context?`, `dockerfile?`, `platform?`, `args?`, `imageName?`, `imageTag?`, `target?`, `builderVersion?` → `imageUri: Output<string>` output |
|  [04]   | `awsx.ecr.RegistryImage`  | component resource | push a pre-built image to a registry by reference                                       |
|  [05]   | `Repository.repository` / `lifecyclePolicy` / `url` | `Output<aws.ecr.Repository>` / `Output<aws.ecr.LifecyclePolicy \| undefined>` / `Output<string>` | underlying repository / lifecycle policy / full repo URL |

[PUBLIC_TYPE_SCOPE]: layout strategy vocabularies (`awsx.types.enums.ec2`)

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                             |
| :-----: | :------------------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `SubnetAllocationStrategy` | enum          | `Legacy` (default — private→public→isolated), `Auto`, `AutoMerge`, `Exact`               |
|  [02]   | `NatGatewayStrategy`       | enum          | `None` (no NAT — dev), `Single` (one, non-HA), `OnePerAz` (prod-recommended)             |
|  [03]   | `SubnetType`               | enum          | `Public`, `Private`, `Isolated`, `Unused` — the `SubnetSpecArgs.type` axis               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: component constructors (all `new awsx.<ns>.<C>(name, args?, opts?: pulumi.ComponentResourceOptions)`)

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `new awsx.ec2.Vpc(name, args?, opts?)`                      | constructor    | full VPC stack — subnets + NAT + route tables in one component  |
|  [02]   | `new awsx.ec2.DefaultVpc(name, args?, opts?)`               | constructor    | reference the account default VPC                              |
|  [03]   | `awsx.ec2.getDefaultVpc(args?, opts?)` / `getDefaultVpcOutput(args?, opts?)` | async / output | default VPC lookup (`Promise` / `Output`-wrapped) |
|  [04]   | `new awsx.ecs.FargateService(name, args?, opts?)` (+ `FargateTaskDefinition`/`EC2Service`/`EC2TaskDefinition`) | constructor | Fargate/EC2 ECS service or task definition   |
|  [05]   | `new awsx.lb.ApplicationLoadBalancer(name, args?, opts?)` (+ `NetworkLoadBalancer`) | constructor | ALB with default listener + target group / NLB    |
|  [06]   | `new awsx.ecr.Repository(name, args?, opts?)` / `new awsx.ecr.Image(name, args?, opts?)` | constructor | ECR repository with lifecycle policy / build + push image |

## [04]-[IMPLEMENTATION_LAW]

[COMPONENT_TOPOLOGY]:
- every component extends `pulumi.ComponentResource` and calls `registerOutputs` to declare its output map; child resources are created with `parent: this`, so a component is one logical node in the resource tree.
- `Vpc` creates one public and one private subnet per AZ by default; `subnetStrategy` selects the layout (`Legacy` default, or `Auto`/`AutoMerge`/`Exact` for explicit `subnetSpecs`), and `natGateways.strategy` selects NAT (`OnePerAz` for prod HA, `Single` for cost, `None` to skip in dev).
- `FargateService` wraps `aws.ecs.Service` and optionally `aws.ecs.TaskDefinition` — pass `taskDefinitionArgs` to create the task def inline (surfaced on `.taskDefinition`), or `taskDefinition` (an ARN) to reuse an existing one.
- `Repository.url` is `<account>.dkr.ecr.<region>.amazonaws.com/<name>` and feeds `Image.repositoryUrl` (the required arg); `Image` builds+pushes and yields `imageUri` for a task-definition container `image`.

[STACKING]:
- cloud tier build (`provisioning/contract#PROVISIONING`): the `DeployMode:"cloud"` `TierStack` compute arm is `awsx.ecs.FargateService` over an `awsx.ec2.Vpc` fronted by `awsx.lb.ApplicationLoadBalancer` — `Vpc.privateSubnetIds` (`Output<string[]>`) feeds the Fargate `networkConfiguration.subnets` and `Vpc.publicSubnetIds` feeds the ALB `subnetIds` directly, `natGateways.strategy: "OnePerAz"` for prod / `"None"` for dev, so the whole topology is one component tree with `parent: this`, never hand-built subnets/route-tables.
- registry → image → workload (`provisioning/contract#PROVISIONING` + `@pulumi/docker`): `awsx.ecr.Repository.url` feeds `awsx.ecr.Image.repositoryUrl` (the required arg) — `Image` builds+pushes and yields `imageUri` (`Output<string>`), which the `FargateService.taskDefinitionArgs` container `image` consumes; the `@pulumi/docker` build executor (`.api/pulumi-docker.md`) is the self-hosted DeployMode equivalent of the awsx `Image` build.
- underlying-resource access (`@pulumi/aws`): every output property wraps a concrete `@pulumi/aws` instance (`Vpc.vpc`, `FargateService.service`, `Repository.repository`, `ApplicationLoadBalancer.loadBalancer`) — reach through for a field the component does not surface; the `contract#PROVISIONING PolicyGuard` rules still validate the wrapped `@pulumi/aws` resource types, so wrapping does not exempt a component from CrossGuard.
- output algebra (`@pulumi/pulumi`): components take `ComponentResourceOptions` (`parent`/`dependsOn`/`providers`) and expose `Output`-typed properties via `registerOutputs`; feed the shared `Output.apply`/`all`/`interpolate` algebra (`.api/pulumi-pulumi.md`), and prefer `getDefaultVpcOutput` over the async `getDefaultVpc` inside a component.

[RAIL_LAW]:
- package: `@pulumi/awsx` (3.6.0)
- owns: opinionated multi-resource AWS component resources (VPC, Fargate/EC2 ECS, ALB/NLB, ECR repository + image build/push) over `@pulumi/aws`
- accept: `awsx` components as the primary surface for VPC, Fargate, ALB, and ECR; `Vpc.privateSubnetIds`/`publicSubnetIds` feeding ECS/ALB directly; `Repository.url` → `Image.repositoryUrl` → `FargateService` container image; `getDefaultVpcOutput` for the default-VPC lookup; raw `@pulumi/aws` only for a resource `awsx` does not cover
- reject: manually constructing VPCs, subnets, and route tables where `awsx.ec2.Vpc` covers the topology; `VpcArgs.numberOfAzs` (the field is `numberOfAvailabilityZones`); `Image.imageName` as the required build target where `repositoryUrl` is the required arg; `@pulumi/awsx` imported on the durable hot path outside the `./provisioning` subpath

```ts contract
// @pulumi/awsx 3.6.0 — the cloud TierStack network/compute/registry the contract page composes
import * as awsx from "@pulumi/awsx"

const vpc = new awsx.ec2.Vpc("net", {
  cidrBlock: "10.0.0.0/16",
  numberOfAvailabilityZones: 3,                 // NOT numberOfAzs
  natGateways: { strategy: "OnePerAz" },        // NatGatewayStrategy: prod HA
  subnetStrategy: "Auto",                       // SubnetAllocationStrategy
}, { parent: this })

const repo = new awsx.ecr.Repository("api", { forceDelete: true }, { parent: this })
const image = new awsx.ecr.Image("api-img", {
  repositoryUrl: repo.url,                       // required arg; Repository.url → Image.repositoryUrl
  context: "./service", platform: "linux/arm64",
}, { parent: this })

const svc = new awsx.ecs.FargateService("api", {
  cluster: cluster.arn,
  taskDefinitionArgs: { container: { name: "api", image: image.imageUri, cpu: 256, memory: 512 } },
  networkConfiguration: { subnets: vpc.privateSubnetIds, securityGroups: [sg.id] },
  desiredCount: 3,
}, { parent: this })
// svc.service: Output<aws.ecs.Service> — reach through to the wrapped resource
```

# [API_CATALOGUE] @pulumi/aws

`@pulumi/aws` supplies the Pulumi AWS provider: a `pulumi.CustomResource` subclass per AWS service resource across ~200 service namespaces (`s3`, `ec2`, `ecs`, `iam`, `rds`, `ecr`, `eks`, `lambda`, `kms`, `sqs`, `sns`, `cloudwatch`, `secretsmanager`, `lb`, `elasticache`, …), each with a matching `*Args` input interface and `*State` import shape, top-level and per-namespace `get*`/`get*Output` lookups, the `aws.Provider`, `aws.config`, and the `aws.types.input.*`/`aws.types.output.*` nested-object trees. In `services` it is the `DeployMode:"cloud"` data + compute-host executor for `provisioning/contract#PROVISIONING`: the data tier (`rds.Instance`/`elasticache.Cluster`/`s3.Bucket`), the `eks.Cluster`/`NodeGroup` whose kubeconfig feeds the `@pulumi/kubernetes` compute tier (`pulumi-kubernetes.md`), the `ecr.getAuthorizationToken` registry-push credential, `secretsmanager`/`kms` for encryption-at-rest, and the `PolicyGuard` CrossGuard target surface (no-public-bucket, encryption-at-rest, tag-presence). Every resource-arg field is `pulumi.Input<T>` and every output `pulumi.Output<T>` from the sibling `@pulumi/pulumi` (`pulumi-pulumi.md`); the whole set is reachable only behind the `./provisioning` exports subpath, never on the durable runtime hot path. The network topology (VPC, subnets, NAT) and ECR image build/push are the sibling `@pulumi/awsx` (`pulumi-awsx.md`) — reach for raw `@pulumi/aws` only for a resource `awsx` does not cover.

- package: `@pulumi/aws`
- version: `7.35.0`
- license: `Apache-2.0`
- tier: `node` — deploy-time only, reachable through the `./provisioning` (`iac`) subpath; the cloud-mode data + EKS-host + registry-auth executor, never on the durable runtime hot path, never browser-reachable.
- rail: deployment

## [01]-[PACKAGE_SURFACE]

The generated barrel `index.d.ts` re-exports ~200 service namespaces (`export { s3, ec2, … }`) plus the top-level `get*`/`get*Output` lookups, `Provider`, `config`, and `types`; every resource is dual-exported as an instance `type Foo = import("./ns/foo").Foo` and a class `const Foo: typeof import("./ns/foo").Foo`. The surface is a `pulumi.CustomResource` subclass per service resource across ~200 namespaces, each with a `*Args` input interface and a `*State` import shape; the `aws.types.input.*`/`aws.types.output.*` trees carry the nested object shapes for complex resource fields. Peer `@pulumi/pulumi ^3.x` supplies the `Input`/`Output` algebra.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: data tier + S3 policy-guard targets

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------------------------------------------- |
|  [01]   | `aws.rds.Instance` / `aws.rds.Cluster`            | resource class | RDS database instance / Aurora cluster — the cloud Postgres tier    |
|  [02]   | `aws.elasticache.Cluster`                         | resource class | ElastiCache (Redis) — the cloud cache tier                          |
|  [03]   | `aws.s3.Bucket` / `aws.s3.BucketArgs`             | resource class | S3 bucket (`bucket` optional → auto-named); the object-store tier   |
|  [04]   | `aws.s3.BucketPublicAccessBlock`                  | resource class | public-access-block settings — the `PolicyGuard` no-public-bucket target |
|  [05]   | `aws.s3.BucketServerSideEncryptionConfigurationV2`| resource class | encryption-at-rest config — the `PolicyGuard` encryption-required target |
|  [06]   | `aws.s3.BucketVersioningV2` / `BucketObjectLockConfiguration` | resource class | versioning / object-lock retention                       |
|  [07]   | `aws.s3.BucketObject` / `aws.s3.BucketPolicy`     | resource class | object upload / bucket policy document                              |

[PUBLIC_TYPE_SCOPE]: network + compute primitives (`@pulumi/awsx` wraps the VPC/ECR set)

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------------------------------------------- |
|  [01]   | `aws.ec2.Vpc` / `Subnet` / `SecurityGroup`        | resource class | VPC / subnet / security group — the network base awsx assembles     |
|  [02]   | `aws.ec2.InternetGateway` / `NatGateway` / `Eip`  | resource class | internet + NAT egress + elastic IP                                  |
|  [03]   | `aws.ec2.RouteTable` / `Route` / `RouteTableAssociation` | resource class | routing + subnet association                                 |
|  [04]   | `aws.ec2.Instance`                                | resource class | EC2 instance                                                       |
|  [05]   | `aws.ecs.Cluster` / `TaskDefinition` / `Service`  | resource class | ECS cluster / task def / service (the fuller container surface awsx wraps) |
|  [06]   | `aws.ecs.CapacityProvider` / `ClusterCapacityProviders` | resource class | capacity provider + cluster binding                          |
|  [07]   | `aws.lb.LoadBalancer` / `TargetGroup` / `Listener`| resource class | ALB/NLB + target group + listener (the fuller ingress surface)      |

[PUBLIC_TYPE_SCOPE]: identity, registry, secrets, keys

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------------------------------------------- |
|  [01]   | `aws.iam.Role` / `RolePolicy` / `RolePolicyAttachment` | resource class | IAM role + inline policy + managed-policy attachment          |
|  [02]   | `aws.iam.Policy` / `InstanceProfile`              | resource class | managed policy document / EC2 instance profile                      |
|  [03]   | `aws.ecr.Repository` / `LifecyclePolicy`          | resource class | container-image registry + image-retention policy                   |
|  [04]   | `aws.secretsmanager.Secret` / `SecretVersion`     | resource class | Secrets Manager secret + value version                              |
|  [05]   | `aws.kms.Key`                                     | resource class | customer-managed KMS key (the encryption-at-rest key source)        |

[PUBLIC_TYPE_SCOPE]: serverless, container-orchestration, messaging, telemetry

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------------------------------------------- |
|  [01]   | `aws.lambda.Function`                             | resource class | Lambda function                                                    |
|  [02]   | `aws.eks.Cluster` / `NodeGroup`                   | resource class | EKS control plane + managed node group — the cloud compute host     |
|  [03]   | `aws.sqs.Queue` / `aws.sns.Topic`                 | resource class | SQS queue / SNS topic                                               |
|  [04]   | `aws.cloudwatch.LogGroup` / `MetricAlarm`         | resource class | CloudWatch log group / alarm                                       |

[PUBLIC_TYPE_SCOPE]: provider, config, and the type namespaces

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------------------------------------------- |
|  [01]   | `aws.Provider` / `aws.ProviderArgs`               | provider class | non-default credential/region scope — `region`, `accessKey`, `secretKey`, `profile`, `assumeRole(s)`, `assumeRoleWithWebIdentity`, `defaultTags` |
|  [02]   | `aws.config`                                      | config module  | ambient provider config bag (region/profile) read from stack config |
|  [03]   | `aws.types.input.*` / `aws.types.output.*`        | type namespace | nested object-arg / output-shape trees for complex resource fields   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource construction pattern (uniform across every namespace)

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `new aws.<ns>.<R>(name, args: <R>Args, opts?: pulumi.CustomResourceOptions)` | constructor | provision a resource; every `args` field accepts `Input<T>`, every output property is `Output<T>` |
|  [02]   | `aws.<ns>.<R>.get(name, id, state?, opts?)`                 | static lookup  | import an existing resource by provider ID into state           |
|  [03]   | `aws.<ns>.<R>.isInstance(obj): obj is <R>`                  | static guard   | runtime resource type check                                    |

[ENTRYPOINT_SCOPE]: account/region lookups (each `get*` has an `Output`-wrapped `get*Output`)

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `aws.getCallerIdentity(args?)` / `getCallerIdentityOutput(args?)` | async / output | current account ID, ARN, user ID                       |
|  [02]   | `aws.getRegion(args?)` / `getRegions(args?)` (+ `*Output`)  | async / output | current region / enabled regions                               |
|  [03]   | `aws.getAvailabilityZones(args?)` / `getAvailabilityZone(args?)` (+ `*Output`) | async / output | AZs in the current region / one AZ detail         |
|  [04]   | `aws.getPartition` / `getArn` / `getIpRanges` / `getService` / `getServicePrincipal` / `getBillingServiceAccount` / `getDefaultTags` (+ `*Output`) | async / output | partition, ARN parse, CIDR ranges, service endpoint/principal, billing account, default tags |
|  [05]   | `aws.ecr.getAuthorizationToken(args?, opts?)` / `getAuthorizationTokenOutput(...)` | async / output | ECR registry auth token — the `@pulumi/docker` / `@pulumi/awsx` push credential |

[ENTRYPOINT_SCOPE]: provider construction

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `new aws.Provider(name, args: ProviderArgs, opts?)`         | provider       | an explicit provider for a non-default region/credential/assume-role scope, passed via `opts.provider` / `ComponentResourceOptions.providers` |

## [04]-[IMPLEMENTATION_LAW]

[PROVIDER_TOPOLOGY]:
- every resource follows `new aws.<ns>.<R>(name: string, args: <R>Args, opts?: pulumi.CustomResourceOptions)`; the generated barrel dual-exports each resource as an instance `type` and a class `const`.
- every `*Args` input field is `pulumi.Input<T>` (`T | Promise<T> | Output<T>`) and every resource output property is `pulumi.Output<T>`; `aws.types.input.*`/`aws.types.output.*` carry the nested object shapes for complex fields.
- lookups come in pairs: the async `get*` returns a `Promise<Result>`, the `get*Output` returns an `Output<Result>` that threads into the dependency graph — never `await` a `get*` inside a `ComponentResource` constructor.
- `aws.Provider` scopes region/credentials/assume-role only when the ambient provider is insufficient; otherwise the ambient `aws.config` (stack config) supplies region/profile.

[DEPLOY_STACK]: how the `provisioning/contract#PROVISIONING` cloud arm stacks this onto `@pulumi/pulumi` (`pulumi-pulumi.md`) core and the Effect rails.
- Data tier + compute host: the `DeployMode:"cloud"` `TierStack` data arm instantiates `rds.Instance`/`rds.Cluster` (Postgres) + `elasticache.Cluster` (Redis) + `s3.Bucket` (object store) as `ComponentResource` children with `{ parent: this }`, and provisions the compute host as `eks.Cluster` + `eks.NodeGroup` — whose kubeconfig feeds the `@pulumi/kubernetes` `Provider` (`pulumi-kubernetes.md`) that runs the actual `Deployment`/`Service`/`HPA`/`Ingress` workload tier; `StackOutputs` exports the provisioned DSN/bucket/endpoint as typed `Redacted`/string reference rows.
- Credential ingress: one `aws.Provider(ProviderArgs)` at the composition root scopes region/credentials/assume-role; the Doppler-injected secrets (`doppler run -- pulumi up`) never enter code or stack state, and `secretsmanager.Secret` + `kms.Key` back encryption-at-rest.
- PolicyGuard targets: the CrossGuard `PolicyGuard` mandatory rules validate these resource types at engine time via `@pulumi/policy` `ResourceValidationPolicy` (`pulumi-policy.md`) — `s3.BucketPublicAccessBlock` present + `blockPublicAcls`/`restrictPublicBuckets` (no-public-bucket), `s3.BucketServerSideEncryptionConfigurationV2` present (encryption-at-rest), tag-presence across all — failing the deploy before a runtime audit.
- Registry auth: `ecr.getAuthorizationTokenOutput` supplies the `@pulumi/awsx` (`pulumi-awsx.md`) / `@pulumi/docker` (`pulumi-docker.md`) registry-push credential; prefer `get*Output` over the async `get*` inside a `ComponentResource` so the dependency graph is preserved.
- Output algebra + Effect boundary: every cross-resource value flows as `pulumi.Output<T>` composed with `pulumi.output`/`all`/`interpolate`/`jsonStringify` (`pulumi-pulumi.md`) — `iam.RolePolicyAttachment.policyArn` accepts an AWS-managed ARN as a plain string, `s3.Bucket.bucket` is optional (Pulumi auto-names); the whole graph applies through the `@pulumi/pulumi/automation` `Stack` under `Effect.tryPromise`/`Effect.async` (the deploy/drift fold in `pulumi-pulumi.md`).

[SIBLING_STACK]:
- `@pulumi/pulumi` core owns the `Output`/`Input` algebra every arg accepts, `ComponentResource`/`registerOutputs`, and the `CustomResourceOptions` (`parent`/`dependsOn`/`provider`/`protect`) these constructors take.
- `@pulumi/awsx` (`pulumi-awsx.md`) wraps the network (`ec2.Vpc` + subnets + NAT) and registry (`ecr.Repository`/`Image`) primitives into opinionated components — construct raw `@pulumi/aws` only for a resource `awsx` does not cover; `@pulumi/kubernetes` (`pulumi-kubernetes.md`) runs the cloud workload tier on the `eks.Cluster` this package provisions.
- `@pulumi/policy` (`pulumi-policy.md`) filters its `validateResourceOfType` rules on these AWS resource classes; `@pulumi/random` (`pulumi-random.md`) seeds generated credentials, `@pulumi/command` (`pulumi-command.md`) the imperative post-provision steps.
- `effect` (`libs/typescript/.api/effect.md`) owns the `Match.exhaustive` `DeployMode` dispatch, the `Effect.tryPromise`/`Effect.async` automation bridges, and the `Config`/`Redacted` receipts `StackOutputs`/`SecretResolver` return.

[RAIL_LAW]:
- Package: `@pulumi/aws`
- Owns: AWS provider resource classes across ~200 service namespaces, the `get*`/`get*Output` lookups, `aws.Provider`, `aws.config`, and the `aws.types.input/output` trees.
- Accept: `new aws.<ns>.<R>(name, args, opts)` at composition roots with `Input<T>` args; `get*Output` for cross-resource value flow; `aws.Provider(ProviderArgs)` for a non-default region/credential scope; direct use only for resources `@pulumi/awsx` does not cover.
- Reject: `await` on an async `get*` inside a `ComponentResource` constructor (use `get*Output`); hand-constructing a VPC/subnet/route-table topology where `awsx.ec2.Vpc` owns it; a public-readable `s3.Bucket` without a `BucketPublicAccessBlock`; a plaintext credential where `aws.Provider` resolves the Doppler-injected secret; `@pulumi/aws` imported on the durable hot path outside the `./provisioning` subpath.

```ts contract
// @pulumi/aws 7.35.0 — the construction + Output-lookup pattern the cloud TierStack composes
import * as aws from "@pulumi/aws"
import * as pulumi from "@pulumi/pulumi"

// data tier: every arg is Input<T>, every output is Output<T>
const bucket = new aws.s3.Bucket("assets", { forceDestroy: false }, { parent: this })
new aws.s3.BucketPublicAccessBlock("assets-block", {
  bucket: bucket.id,                                   // Output<string> flows as Input<string>
  blockPublicAcls: true, restrictPublicBuckets: true, // PolicyGuard: no-public-bucket
}, { parent: this })
const dsn = new aws.rds.Instance("pg", { engine: "postgres", instanceClass: "db.t4g.medium" }, { parent: this })

// compute host: the EKS cluster whose kubeconfig feeds the @pulumi/kubernetes Provider
const cluster = new aws.eks.Cluster("compute", { roleArn: role.arn, vpcConfig: { subnetIds: vpc.privateSubnetIds } }, { parent: this })

// lookup: Output-wrapped so it threads the dependency graph (never `await` in a ComponentResource ctor)
const identity: pulumi.Output<string> = aws.getCallerIdentityOutput().accountId

// explicit provider for a non-default region/credential scope
const provider = new aws.Provider("us-west", { region: "us-west-2", profile: "deploy" })
```

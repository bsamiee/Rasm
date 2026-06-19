# [API_CATALOGUE] @pulumi/aws

`@pulumi/aws` supplies the Pulumi AWS provider: a resource class per AWS service namespace (`s3`, `ec2`, `ecs`, `iam`, `rds`, `ecr`, `eks`, `lambda`, `kms`, `sqs`, `sns`, `cloudwatch`, and 80+ more), matching `Args` input types, `Output`-typed state fields, lookup functions (`get*`, `get*Output`), and provider-level config for the services deploy tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/aws`
- package: `@pulumi/aws`
- module: `@pulumi/aws`
- asset: AWS provider resource classes, lookup functions, type namespaces `types/input` and `types/output`
- rail: deployment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: S3 resource family
- rail: deployment

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [RAIL]                       |
| :-----: | :------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `aws.s3.Bucket`                        | resource class | S3 general-purpose bucket    |
|  [02]   | `aws.s3.BucketArgs`                    | args interface | bucket configuration input   |
|  [03]   | `aws.s3.BucketObject`                  | resource class | object upload to bucket      |
|  [04]   | `aws.s3.BucketPolicy`                  | resource class | bucket policy document       |
|  [05]   | `aws.s3.BucketPublicAccessBlock`       | resource class | public access block settings |
|  [06]   | `aws.s3.BucketVersioningV2`            | resource class | versioning configuration     |
|  [07]   | `aws.s3.BucketObjectLockConfiguration` | resource class | object lock configuration    |

[PUBLIC_TYPE_SCOPE]: EC2 resource family
- rail: deployment

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [RAIL]                   |
| :-----: | :------------------------------ | :------------- | :----------------------- |
|  [01]   | `aws.ec2.Vpc`                   | resource class | VPC network              |
|  [02]   | `aws.ec2.Subnet`                | resource class | subnet                   |
|  [03]   | `aws.ec2.SecurityGroup`         | resource class | security group           |
|  [04]   | `aws.ec2.InternetGateway`       | resource class | internet gateway         |
|  [05]   | `aws.ec2.NatGateway`            | resource class | NAT gateway              |
|  [06]   | `aws.ec2.RouteTable`            | resource class | route table              |
|  [07]   | `aws.ec2.Route`                 | resource class | route table entry        |
|  [08]   | `aws.ec2.RouteTableAssociation` | resource class | subnet-route association |
|  [09]   | `aws.ec2.Eip`                   | resource class | elastic IP address       |
|  [10]   | `aws.ec2.Instance`              | resource class | EC2 instance             |

[PUBLIC_TYPE_SCOPE]: ECS resource family
- rail: deployment

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]  | [RAIL]                   |
| :-----: | :--------------------------------- | :------------- | :----------------------- |
|  [01]   | `aws.ecs.Cluster`                  | resource class | ECS cluster              |
|  [02]   | `aws.ecs.TaskDefinition`           | resource class | ECS task definition      |
|  [03]   | `aws.ecs.Service`                  | resource class | ECS service              |
|  [04]   | `aws.ecs.CapacityProvider`         | resource class | capacity provider        |
|  [05]   | `aws.ecs.ClusterCapacityProviders` | resource class | cluster capacity binding |

[PUBLIC_TYPE_SCOPE]: IAM resource family
- rail: deployment

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [RAIL]                    |
| :-----: | :----------------------------- | :------------- | :------------------------ |
|  [01]   | `aws.iam.Role`                 | resource class | IAM role                  |
|  [02]   | `aws.iam.RolePolicy`           | resource class | inline role policy        |
|  [03]   | `aws.iam.RolePolicyAttachment` | resource class | managed policy attachment |
|  [04]   | `aws.iam.Policy`               | resource class | managed policy document   |
|  [05]   | `aws.iam.InstanceProfile`      | resource class | EC2 instance profile      |

[PUBLIC_TYPE_SCOPE]: ECR / EKS / Lambda / RDS resource family
- rail: deployment

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                   |
| :-----: | :------------------------ | :------------- | :----------------------- |
|  [01]   | `aws.ecr.Repository`      | resource class | container image registry |
|  [02]   | `aws.ecr.LifecyclePolicy` | resource class | image retention policy   |
|  [03]   | `aws.eks.Cluster`         | resource class | EKS control plane        |
|  [04]   | `aws.eks.NodeGroup`       | resource class | managed node group       |
|  [05]   | `aws.lambda.Function`     | resource class | Lambda function          |
|  [06]   | `aws.rds.Instance`        | resource class | RDS database instance    |
|  [07]   | `aws.rds.Cluster`         | resource class | Aurora cluster           |
|  [08]   | `aws.elasticache.Cluster` | resource class | ElastiCache cluster      |

[PUBLIC_TYPE_SCOPE]: KMS / SQS / SNS / CloudWatch / Secrets resource family
- rail: deployment

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]  | [RAIL]                     |
| :-----: | :--------------------------------- | :------------- | :------------------------- |
|  [01]   | `aws.kms.Key`                      | resource class | KMS customer-managed key   |
|  [02]   | `aws.sqs.Queue`                    | resource class | SQS queue                  |
|  [03]   | `aws.sns.Topic`                    | resource class | SNS topic                  |
|  [04]   | `aws.cloudwatch.LogGroup`          | resource class | CloudWatch log group       |
|  [05]   | `aws.cloudwatch.MetricAlarm`       | resource class | CloudWatch alarm           |
|  [06]   | `aws.secretsmanager.Secret`        | resource class | Secrets Manager secret     |
|  [07]   | `aws.secretsmanager.SecretVersion` | resource class | secret value version       |
|  [08]   | `aws.lb.LoadBalancer`              | resource class | Application / Network LB   |
|  [09]   | `aws.lb.TargetGroup`               | resource class | load balancer target group |
|  [10]   | `aws.lb.Listener`                  | resource class | load balancer listener     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource constructor pattern
- rail: deployment

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `new aws.<ns>.<Resource>(name, args, opts?)`             | constructor    | provision resource; all args accept `Input<T>` |
|  [02]   | `aws.<ns>.<Resource>.get(name, id, state?, opts?)`       | static lookup  | import existing resource by provider ID        |
|  [03]   | `aws.<ns>.<Resource>.isInstance(obj): obj is <Resource>` | static guard   | runtime type check                             |

[ENTRYPOINT_SCOPE]: lookup functions
- rail: deployment

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `aws.getCallerIdentity(args?): Promise<Result>`                | async lookup   | current account ID, ARN, user ID |
|  [02]   | `aws.getCallerIdentityOutput(args?): Output<Result>`           | output lookup  | same, `Output`-wrapped           |
|  [03]   | `aws.getRegion(args?): Promise<Result>`                        | async lookup   | current AWS region               |
|  [04]   | `aws.getRegionOutput(args?): Output<Result>`                   | output lookup  | same, `Output`-wrapped           |
|  [05]   | `aws.getAvailabilityZones(args?): Promise<Result>`             | async lookup   | available AZs in current region  |
|  [06]   | `aws.getPartition(args?): Promise<Result>`                     | async lookup   | current AWS partition            |
|  [07]   | `aws.ecr.getAuthorizationToken(args?, opts?): Promise<Result>` | async lookup   | ECR registry auth token          |
|  [08]   | `aws.ecr.getAuthorizationTokenOutput(...): Output<Result>`     | output lookup  | same, `Output`-wrapped           |

## [04]-[IMPLEMENTATION_LAW]

[PROVIDER_TOPOLOGY]:
- All resource constructors follow `new aws.<namespace>.<ClassName>(name: string, args: <ClassName>Args, opts?: pulumi.CustomResourceOptions)`
- All resource output properties are `pulumi.Output<T>`; all `Args` input fields accept `pulumi.Input<T>`
- Sub-namespaces are accessed as `aws.s3`, `aws.ec2`, `aws.ecs`, `aws.iam`, `aws.rds`, `aws.ecr`, `aws.eks`, `aws.lambda`, `aws.kms`, `aws.sqs`, `aws.sns`, `aws.cloudwatch`, `aws.secretsmanager`, `aws.lb`, etc.
- Top-level `aws` module also exports `getCallerIdentity`, `getRegion`, `getAvailabilityZone(s)`, `getPartition`, `getBillingServiceAccount`, and their `*Output` variants
- `aws.types.input.*` and `aws.types.output.*` carry nested object types for complex resource args and output shapes
- `aws.Provider` accepts `region`, `accessKey`, `secretKey`, `profile`, and related credential fields when a non-default provider is needed

[LOCAL_ADMISSION]:
- Use `aws.ecr.getAuthorizationTokenOutput` (not the async form) when the token feeds into another `Output<T>` field to preserve the dependency graph
- `aws.iam.RolePolicyAttachment.policyArn` accepts the ARN of any managed policy including AWS-managed ARNs as plain strings
- `aws.s3.Bucket.bucket` is optional; Pulumi generates a unique name when omitted

[RAIL_LAW]:
- Package: `@pulumi/aws`
- Owns: AWS provider resource classes and lookup functions
- Accept: `Input<T>` for all resource args; `Output<T>` for cross-resource value flow via `pulumi.output` and `Output.apply`
- Reject: direct `await` on async lookup functions inside `ComponentResource` constructors; use the `*Output` variant instead

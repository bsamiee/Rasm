# [API_CATALOGUE] @pulumi/aws

`@pulumi/aws` supplies the Pulumi AWS provider: a resource class per AWS service namespace (`s3`, `ec2`, `ecs`, `iam`, `rds`, `ecr`, `eks`, `lambda`, `kms`, `sqs`, `sns`, `cloudwatch`, and 80+ more), matching `Args` input types, `Output`-typed state fields, lookup functions (`get*`, `get*Output`), and provider-level config for the services deploy tier.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/aws`
- package: `@pulumi/aws`
- module: `@pulumi/aws`
- asset: AWS provider resource classes, lookup functions, type namespaces `types/input` and `types/output`
- rail: deployment

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: S3 resource family
- rail: deployment

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [RAIL]                       |
| :-----: | :------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `aws.s3.Bucket`                        | resource class | S3 general-purpose bucket    |
|   [2]   | `aws.s3.BucketArgs`                    | args interface | bucket configuration input   |
|   [3]   | `aws.s3.BucketObject`                  | resource class | object upload to bucket      |
|   [4]   | `aws.s3.BucketPolicy`                  | resource class | bucket policy document       |
|   [5]   | `aws.s3.BucketPublicAccessBlock`       | resource class | public access block settings |
|   [6]   | `aws.s3.BucketVersioningV2`            | resource class | versioning configuration     |
|   [7]   | `aws.s3.BucketObjectLockConfiguration` | resource class | object lock configuration    |

[PUBLIC_TYPE_SCOPE]: EC2 resource family
- rail: deployment

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [RAIL]                   |
| :-----: | :------------------------------ | :------------- | :----------------------- |
|   [1]   | `aws.ec2.Vpc`                   | resource class | VPC network              |
|   [2]   | `aws.ec2.Subnet`                | resource class | subnet                   |
|   [3]   | `aws.ec2.SecurityGroup`         | resource class | security group           |
|   [4]   | `aws.ec2.InternetGateway`       | resource class | internet gateway         |
|   [5]   | `aws.ec2.NatGateway`            | resource class | NAT gateway              |
|   [6]   | `aws.ec2.RouteTable`            | resource class | route table              |
|   [7]   | `aws.ec2.Route`                 | resource class | route table entry        |
|   [8]   | `aws.ec2.RouteTableAssociation` | resource class | subnet-route association |
|   [9]   | `aws.ec2.Eip`                   | resource class | elastic IP address       |
|  [10]   | `aws.ec2.Instance`              | resource class | EC2 instance             |

[PUBLIC_TYPE_SCOPE]: ECS resource family
- rail: deployment

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]  | [RAIL]                   |
| :-----: | :--------------------------------- | :------------- | :----------------------- |
|   [1]   | `aws.ecs.Cluster`                  | resource class | ECS cluster              |
|   [2]   | `aws.ecs.TaskDefinition`           | resource class | ECS task definition      |
|   [3]   | `aws.ecs.Service`                  | resource class | ECS service              |
|   [4]   | `aws.ecs.CapacityProvider`         | resource class | capacity provider        |
|   [5]   | `aws.ecs.ClusterCapacityProviders` | resource class | cluster capacity binding |

[PUBLIC_TYPE_SCOPE]: IAM resource family
- rail: deployment

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [RAIL]                    |
| :-----: | :----------------------------- | :------------- | :------------------------ |
|   [1]   | `aws.iam.Role`                 | resource class | IAM role                  |
|   [2]   | `aws.iam.RolePolicy`           | resource class | inline role policy        |
|   [3]   | `aws.iam.RolePolicyAttachment` | resource class | managed policy attachment |
|   [4]   | `aws.iam.Policy`               | resource class | managed policy document   |
|   [5]   | `aws.iam.InstanceProfile`      | resource class | EC2 instance profile      |

[PUBLIC_TYPE_SCOPE]: ECR / EKS / Lambda / RDS resource family
- rail: deployment

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                   |
| :-----: | :------------------------ | :------------- | :----------------------- |
|   [1]   | `aws.ecr.Repository`      | resource class | container image registry |
|   [2]   | `aws.ecr.LifecyclePolicy` | resource class | image retention policy   |
|   [3]   | `aws.eks.Cluster`         | resource class | EKS control plane        |
|   [4]   | `aws.eks.NodeGroup`       | resource class | managed node group       |
|   [5]   | `aws.lambda.Function`     | resource class | Lambda function          |
|   [6]   | `aws.rds.Instance`        | resource class | RDS database instance    |
|   [7]   | `aws.rds.Cluster`         | resource class | Aurora cluster           |
|   [8]   | `aws.elasticache.Cluster` | resource class | ElastiCache cluster      |

[PUBLIC_TYPE_SCOPE]: KMS / SQS / SNS / CloudWatch / Secrets resource family
- rail: deployment

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]  | [RAIL]                     |
| :-----: | :--------------------------------- | :------------- | :------------------------- |
|   [1]   | `aws.kms.Key`                      | resource class | KMS customer-managed key   |
|   [2]   | `aws.sqs.Queue`                    | resource class | SQS queue                  |
|   [3]   | `aws.sns.Topic`                    | resource class | SNS topic                  |
|   [4]   | `aws.cloudwatch.LogGroup`          | resource class | CloudWatch log group       |
|   [5]   | `aws.cloudwatch.MetricAlarm`       | resource class | CloudWatch alarm           |
|   [6]   | `aws.secretsmanager.Secret`        | resource class | Secrets Manager secret     |
|   [7]   | `aws.secretsmanager.SecretVersion` | resource class | secret value version       |
|   [8]   | `aws.lb.LoadBalancer`              | resource class | Application / Network LB   |
|   [9]   | `aws.lb.TargetGroup`               | resource class | load balancer target group |
|  [10]   | `aws.lb.Listener`                  | resource class | load balancer listener     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource constructor pattern
- rail: deployment

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------------------------- |
|   [1]   | `new aws.<ns>.<Resource>(name, args, opts?)`             | constructor    | provision resource; all args accept `Input<T>` |
|   [2]   | `aws.<ns>.<Resource>.get(name, id, state?, opts?)`       | static lookup  | import existing resource by provider ID        |
|   [3]   | `aws.<ns>.<Resource>.isInstance(obj): obj is <Resource>` | static guard   | runtime type check                             |

[ENTRYPOINT_SCOPE]: lookup functions
- rail: deployment

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `aws.getCallerIdentity(args?): Promise<Result>`                | async lookup   | current account ID, ARN, user ID |
|   [2]   | `aws.getCallerIdentityOutput(args?): Output<Result>`           | output lookup  | same, `Output`-wrapped           |
|   [3]   | `aws.getRegion(args?): Promise<Result>`                        | async lookup   | current AWS region               |
|   [4]   | `aws.getRegionOutput(args?): Output<Result>`                   | output lookup  | same, `Output`-wrapped           |
|   [5]   | `aws.getAvailabilityZones(args?): Promise<Result>`             | async lookup   | available AZs in current region  |
|   [6]   | `aws.getPartition(args?): Promise<Result>`                     | async lookup   | current AWS partition            |
|   [7]   | `aws.ecr.getAuthorizationToken(args?, opts?): Promise<Result>` | async lookup   | ECR registry auth token          |
|   [8]   | `aws.ecr.getAuthorizationTokenOutput(...): Output<Result>`     | output lookup  | same, `Output`-wrapped           |

## [4]-[IMPLEMENTATION_LAW]

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

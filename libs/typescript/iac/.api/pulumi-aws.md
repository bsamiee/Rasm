# [TS_IAC_API_PULUMI_AWS]

`@pulumi/aws` is the generated Pulumi SDK: every service namespace shares one uniform `pulumi.CustomResource` pattern — `(name, args, opts)` constructor, `static get`, `static isInstance`, `Output<T>` properties — under an explicit `Provider` built from a `StackSpec`.

`aws` rides a prepared `Match.exhaustive` dispatch row, never first-class: a `StackSpec` value finalizes it, a new cloud is one new arm, and its carried worth is the service-equivalence subset mapping AWS managed services onto the `selfhosted-k8s` capability matrix.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the uniform resource pattern (every namespace)

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]  | [CAPABILITY]                                                 |
| :-----: | :---------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `<ns>.<Resource> extends pulumi.CustomResource` | resource class | one managed AWS resource (e.g. `s3.BucketV2`, `rds.Cluster`) |
|  [02]   | `<ns>.<Resource>Args`                           | input record   | create arguments (`readonly x?: pulumi.Input<T>`)            |
|  [03]   | `<ns>.<Resource>State`                          | input record   | adoption state for `static get`                              |
|  [04]   | `<Resource>.<prop>: pulumi.Output<T>`           | output prop    | computed attribute; a dependency edge when passed as `Input` |
|  [05]   | `types.input.*` / `types.output.*`              | nested type    | the generated nested input/output type trees                 |

[PUBLIC_TYPE_SCOPE]: generated enum vocabularies (`aws.types.enums.*`)

Each vocabulary is a frozen const object of literal members beside a `keyof typeof` union of the same name, so every union below is CLOSED and the const is the roster a caller spreads. Reach runs through `types.enums` alone — a service namespace re-exports its resources, args, and invokes but never its enum twin, so `aws.ec2.InstanceType` resolves to the `getInstanceType` invoke family while `aws.types.enums.ec2.InstanceType` is the vocabulary.

Consuming args stay string-widened by the provider itself (`Input<string>`), so this SDK closes nothing at the call site: a union assigns straight in, and a closed admission exists only where a caller spends the roster on a coordinate it owns. Members grow additively across releases, so a derived roster widens with the installed tree and pins no version.

Rows below name the vocabularies this estate ruled on; the remaining trees carry the same shape and no estate coordinate — `ec2.{InstancePlatform,PlacementStrategy,ProtocolType,Tenancy}`, `rds.{EngineType,EngineMode,InstanceType}`, and the `alb`, `applicationloadbalancing`, `autoscaling`, `ecr`, `lambda`, and `ssm` trees.

| [INDEX] | [MEMBER]                    | [CAPABILITY]                                                                          |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `Region`                    | every published region literal; `ProviderArgs.region` is `Input<string \| undefined>` |
|  [02]   | `ec2.InstanceType`          | every EC2 capacity literal — the one closed surface a node-pool coordinate has        |
|  [03]   | `iam.PolicyDocumentVersion` | the two policy-language versions a `PolicyDocument.Version` admits                    |
|  [04]   | `iam.PolicyStatementEffect` | `Allow`/`Deny` on `PolicyStatement.Effect`                                            |
|  [05]   | `s3.CannedAcl`              | the canned bucket ACLs; enforced object ownership disables ACLs, so none apply        |
|  [06]   | `route53.RecordType`        | the Route 53 record roster; a Cloudflare-owned dns cell reaches no part of it         |
|  [07]   | `rds.StorageType`           | the managed-volume classes; a CNPG data row names none                                |

[ENTRYPOINT_SCOPE]: bucket access posture (the private-origin set)

Object ownership carries no exported roster — `BucketOwnershipControlsRule.objectOwnership` is `Input<string>` whose values (`BucketOwnerPreferred`, `ObjectWriter`, `BucketOwnerEnforced`) live in an arg comment alone, so a posture literal is the provider's own openness and not the estate's. `BucketPolicy.policy` takes the typed `types.input.s3.PolicyDocument` beside its string twin, so a grant spells as a value under the `iam` enum constants and never as serialized JSON.

Public-access refusals ride four independent `Input<boolean>` args on one resource: `blockPublicAcls`, `blockPublicPolicy`, `ignorePublicAcls`, and `restrictPublicBuckets`.

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------------- | :------ | :------------------------------------------------------- |
|  [01]   | `new s3.BucketOwnershipControls(name, { bucket, rule })`  | ctor    | `rule.objectOwnership` — ACL applicability per bucket    |
|  [02]   | `new s3.BucketPublicAccessBlock(name, { bucket, …four })` | ctor    | the four public-access refusals named above              |
|  [03]   | `new s3.BucketPolicy(name, { bucket, policy })`           | ctor    | `PolicyDocument` — `Version` beside typed `Statement[]`  |
|  [04]   | `types.input.iam.PolicyStatement`                         | nested  | `Effect`, `Principal`, `Action`, `Resource`, `Condition` |
|  [05]   | `types.input.iam.ServicePrincipal`                        | nested  | `{ Service }` — the principal an OAC grant names         |
|  [06]   | `cloudfront.Distribution.arn`                             | output  | the `AWS:SourceArn` value scoping that grant             |

[ENTRYPOINT_SCOPE]: private task publication

`BucketObjectv2` writes literal or base64 content under an owned key, while `iam.getPolicyDocumentOutput` builds the task policy as a provider-bound `Output` document. A recursive Fargate materializer lists only its generation prefix and reads only objects under it from a dedicated public-blocked bucket; serving-origin policy never reaches that bucket.

[PUBLIC_TYPE_SCOPE]: provider + engine model

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `aws.Provider extends pulumi.ProviderResource` | provider       | explicit AWS provider (account/region/role scope)   |
|  [02]   | `aws.ProviderArgs`                             | input record   | provider config: region, profile, roles, tags, keys |
|  [03]   | `pulumi.Output<T>` / `pulumi.Input<T>`         | graph value    | async computed value / accepted input value         |
|  [04]   | `pulumi.CustomResourceOptions`                 | options record | per-resource provider, deps, parent, protect        |

[PUBLIC_TYPE_SCOPE]: data-source invokes (the `getX` pattern)

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `Get<X>Result`                    | result record | data-source return shapes (e.g. `GetRegionResult`) |
|  [02]   | `Get<X>Args` / `Get<X>OutputArgs` | input record  | invoke args; `OutputArgs` is `Input`-lifted        |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider construction (from StackSpec)

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `new aws.Provider(name, { region, profile?, assumeRoles?, defaultTags? })` | ctor    | explicit account/region/role provider  |
|  [02]   | `aws.Provider.isInstance(obj)`                                             | guard   | cross-SDK-copy provider instance check |

[ENTRYPOINT_SCOPE]: the resource-construction pattern (uniform, every namespace)

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `new <ns>.<Resource>(name, args, { provider, dependsOn?, parent? })` | ctor    | register a resource under the arm's provider |
|  [02]   | `<ns>.<Resource>.get(name, id, state?, opts?)`                       | static  | adopt an existing cloud resource by id       |
|  [03]   | `<ns>.<Resource>.isInstance(obj)`                                    | guard   | cross-SDK-copy resource instance check       |

[ENTRYPOINT_SCOPE]: service-equivalence map (the `aws` row's mapped subset)

Classes are `aws.*` with the prefix elided; `awsx.*` is called out. This bounded table is the arm's real surface — the `provider/surface` map resolving each capability to a resource class — never the full service roster.

| [INDEX] | [CAPABILITY]               | [AWS_RESOURCE_CLASS]                                       | [SELFHOSTED_K8S_EQUIVALENT]          |
| :-----: | :------------------------- | :--------------------------------------------------------- | :----------------------------------- |
|  [01]   | managed Postgres (`store`) | `rds.Cluster` / `rds.Instance` (Aurora PG18)               | CNPG PG18.4-extension image          |
|  [02]   | object store               | `s3.BucketV2`                                              | conditional-put self-host object row |
|  [03]   | container compute          | `ecs.Cluster` / `eks.Cluster`                              | typed `@pulumi/kubernetes` workloads |
|  [04]   | ingress / load balancing   | `lb.LoadBalancer` (+ `Listener`/`TargetGroup`)             | the `kube/traffic` ingress row       |
|  [05]   | TLS certificate            | `acm.Certificate`                                          | the `kube/traffic` cert row          |
|  [06]   | DNS                        | `route53.Zone` / `route53.Record`                          | the `kube/traffic` dns row           |
|  [07]   | network fabric             | `ec2.Vpc` / `Subnet` / `SecurityGroup` (or `awsx.ec2.Vpc`) | owned metal/VPS cluster network      |
|  [08]   | identity / access          | `iam.Role` / `Policy` / `RolePolicyAttachment`             | k8s ServiceAccount + RBAC            |
|  [09]   | cache                      | `elasticache.*` / `efs.*`                                  | in-cluster cache/volume rows         |
|  [10]   | served-header edge (CDN)   | `cloudfront.Distribution`                                  | the `kube/traffic` edge row          |

[ENTRYPOINT_SCOPE]: the cloudfront edge (the `Source.edge` render surface)

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `new cloudfront.OriginAccessControl(name, OriginAccessControlArgs)`     | ctor    | private S3 origin access               |
|  [02]   | `new cloudfront.CachePolicy(name, CachePolicyArgs)`                     | ctor    | owned cache-key policy                 |
|  [03]   | `new cloudfront.ResponseHeadersPolicy(name, ResponseHeadersPolicyArgs)` | ctor    | one policy per path posture            |
|  [04]   | `new cloudfront.Distribution(name, DistributionArgs)`                   | ctor    | the front binding origins to behaviors |

- `cloudfront.OriginAccessControl`: `originAccessControlOriginType: "s3"` with `signingBehavior: "always"` and `signingProtocol: "sigv4"`; `origins[].originAccessControlId` binds it to the distribution.
- `cloudfront.CachePolicy`: `minTtl`/`defaultTtl`/`maxTtl` over `parametersInCacheKeyAndForwardedToOrigin`, whose `cookiesConfig.cookieBehavior`, `headersConfig.headerBehavior`, `queryStringsConfig.queryStringBehavior`, `enableAcceptEncodingBrotli`, and `enableAcceptEncodingGzip` fix the cache key; `cachePolicyId` rides every behavior.
- `cloudfront.ResponseHeadersPolicy`: `customHeadersConfig.items[]` carries `header`, `value`, and `override` per static header.
- `cloudfront.Distribution`: `enabled`, `origins`, `defaultCacheBehavior`, `orderedCacheBehaviors`, `restrictions.geoRestriction.restrictionType`, and `viewerCertificate.cloudfrontDefaultCertificate`; an ordered behavior binds `pathPattern`, `targetOriginId`, `viewerProtocolPolicy`, `allowedMethods`/`cachedMethods`, and `responseHeadersPolicyId`.
- CloudFront binds the FIRST behavior whose `pathPattern` matches, so per-path headers order narrow to wide and each behavior's response policy carries the full header union its path owes — a wide behavior first silently strips every narrower posture.

[ENTRYPOINT_SCOPE]: data-source invokes

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `aws.getCallerIdentity()` / `aws.getCallerIdentityOutput()`    | invoke  | resolve the deploying account/arn/user id |
|  [02]   | `aws.getRegion()` / `aws.getAvailabilityZones()` (+ `…Output`) | invoke  | active region / AZ list for subnet spread |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `@pulumi/aws` is a generated SDK carrying one uniform resource pattern across every service namespace — `CustomResource` subclass, `(name, args, opts)` constructor, `static get`, `static isInstance`, `Output<T>` properties — with service namespaces as seed data behind it; the catalog documents the pattern and the mapped service-equivalence subset, never a flat per-service roster.
- `aws` is one closed `Match.exhaustive` dispatch row in `provider/dispatch`; a new cloud is one new arm and finalizing one is app data (a `StackSpec` value). `aws` is prepared — `selfhosted-k8s` is the first-class arm, and `aws`/`gcp`/`cloudflare` carry the service-equivalence map so a `StackSpec` retargets without rewriting topology.
- `aws` arm constructs exactly one explicit `aws.Provider` from the `StackSpec` (region/profile/assumeRoles/defaultTags) threaded as `{ provider }` into every resource's options, so one program drives many accounts/regions with no ambient AWS env/config.
- Resources compose by passing `Output<T>` properties as downstream `Input<T>` args; the engine derives the dependency graph from those edges, combined only through `pulumi.all`/`.apply`/`interpolate` and never resolved to a plain value inside a program.

[STACKING]:
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): AWS resources are `CustomResource`s in the `LocalWorkspace.createOrSelectStack` inline program that `program/automation` wraps; `Output<T>`/`Input<T>` are the graph currency, `pulumi.all([...])`/`interpolate` combine outputs, `pulumi.secret` marks credentials, typed `StackOutputs` (an RDS endpoint) exit the arm, and `ComponentResourceOptions.provider` scopes the arm's provider down a component tree.
- `@pulumi/awsx`(`.api/pulumi-awsx.md`): the higher-level `ComponentResource` compositions backing the prepared row — `awsx.ec2.Vpc` (multi-AZ VPC with subnets/NAT), `awsx.ecs.FargateService`, `awsx.lb.ApplicationLoadBalancer` — own standard compositions over the equivalent hand-wired `aws.ec2.*`/`aws.lb.*` primitives; raw `aws.*` serves only an attribute a component does not expose.
- `effect`(`libs/typescript/.api/effect.md`): provider dispatch is `Match.exhaustive` over the arm union (`provider/dispatch`), the `StackSpec`/`StackOutputs` vocabulary is `Schema` (`program/spec`, `stack/output`), the service-equivalence rows are the `provider/surface` map, and typed `StackOutputs → ShardingConfig` is the sole value crossing to `work`; Automation returns Pulumi's operation-specific lifecycle result unchanged on success.

[LOCAL_ADMISSION]:
- `aws` arm is one dispatch row reading a `StackSpec`, constructing one `aws.Provider`, and realizing the service-equivalence subset with that provider scoped in; the `provider/surface` map is the single place a capability resolves to an `aws.*`/`awsx.*` resource class.
- `awsx` components own standard compositions (VPC, Fargate service, ALB) and raw `aws.*` resources own fine-grained control, both taking the arm's explicit StackSpec-derived provider.
- Credentials and account selection ride the `StackSpec` Doppler project ref into `ProviderArgs` (`profile`/`assumeRoles`) marked `pulumi.secret`, sourced through the `@pulumiverse/doppler`/`security/crypt/secret` read path.
- `<Resource>.get(name, id)` adopts a pre-existing cloud resource into the arm's graph under the same explicit provider, so an estate built by hand enters the typed program as library code and the plane keeps zero authored `Pulumi.yaml`.

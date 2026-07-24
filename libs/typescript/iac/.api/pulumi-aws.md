# [TS_IAC_API_PULUMI_AWS]

`@pulumi/aws` is the generated Pulumi SDK: every service namespace shares one uniform `pulumi.CustomResource` pattern — `(name, args, opts)` constructor, `static get`, `static isInstance`, `Output<T>` properties — under an explicit `Provider` built from a `StackSpec`.

`aws` rides a prepared `Match.exhaustive` dispatch row, never first-class: a `StackSpec` value finalizes it, a new cloud is one new arm, and its carried worth is the service-equivalence subset mapping AWS managed services onto the `selfhosted-k8s` capability matrix.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/aws`
- package: `@pulumi/aws` (Apache-2.0)
- module: `@pulumi/aws` with per-service subpath exports — `aws.s3.BucketV2`, `aws.ec2.Vpc`, `aws.rds.Cluster`
- runtime: node Automation-API program process; the `pulumi` CLI and AWS provider plugin resolve on the deploy host, wrapped once in `program`
- rail: deploy — the prepared `aws` provider dispatch row
- depends: `@pulumi/pulumi` (the `Output`/`Input`/`CustomResource`/`ComponentResource`/`ProviderResource` model and Automation API engine), `@pulumi/awsx` (higher-level `ComponentResource` compositions backing this row)
- namespaces: top-level `Provider`/`config`/`types` and the `getArn`/`getRegion`/`getCallerIdentity`/`getAvailabilityZones` data-source invokes, and one service namespace per AWS service (`ec2`, `s3`, `rds`, `eks`, `ecs`, `iam`, `acm`, `route53`, `lb`, `efs`, `elasticache`, …)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the uniform resource pattern (every namespace)

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]  | [CAPABILITY]                                                 |
| :-----: | :---------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `<ns>.<Resource> extends pulumi.CustomResource` | resource class | one managed AWS resource (e.g. `s3.BucketV2`, `rds.Cluster`) |
|  [02]   | `<ns>.<Resource>Args`                           | input record   | create arguments (`readonly x?: pulumi.Input<T>`)            |
|  [03]   | `<ns>.<Resource>State`                          | input record   | adoption state for `static get`                              |
|  [04]   | `<Resource>.<prop>: pulumi.Output<T>`           | output prop    | computed attribute; a dependency edge when passed as `Input` |
|  [05]   | `types.input.*` / `types.output.*`              | nested type    | the generated nested input/output type trees                 |

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

## [03]-[ENTRYPOINTS]

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

[ENTRYPOINT_SCOPE]: data-source invokes

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `aws.getCallerIdentity()` / `aws.getCallerIdentityOutput()`    | invoke  | resolve the deploying account/arn/user id |
|  [02]   | `aws.getRegion()` / `aws.getAvailabilityZones()` (+ `…Output`) | invoke  | active region / AZ list for subnet spread |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `@pulumi/aws` is a generated SDK carrying one uniform resource pattern across every service namespace — `CustomResource` subclass, `(name, args, opts)` constructor, `static get`, `static isInstance`, `Output<T>` properties — with service namespaces as seed data behind it; the catalog documents the pattern and the mapped service-equivalence subset, never a flat per-service roster.
- `aws` is one closed `Match.exhaustive` dispatch row in `provider/dispatch`; a new cloud is one new arm and finalizing one is app data (a `StackSpec` value). `aws` is prepared — `selfhosted-k8s` is the first-class arm, and `aws`/`gcp`/`cloudflare` carry the service-equivalence map so a `StackSpec` retargets without rewriting topology.
- `aws` arm constructs exactly one explicit `aws.Provider` from the `StackSpec` (region/profile/assumeRoles/defaultTags) threaded as `{ provider }` into every resource's options, so one program drives many accounts/regions with no ambient AWS env/config.
- Resources compose by passing `Output<T>` properties as downstream `Input<T>` args; the engine derives the dependency graph from those edges, combined only through `pulumi.all`/`.apply`/`interpolate` and never resolved to a plain value inside a program.

[STACKING]:
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): AWS resources are `CustomResource`s in the `LocalWorkspace.createOrSelectStack` inline program that `program/automation` wraps; `Output<T>`/`Input<T>` are the graph currency, `pulumi.all([...])`/`interpolate` combine outputs, `pulumi.secret` marks credentials, typed `StackOutputs` (an RDS endpoint) exit the arm, and `ComponentResourceOptions.provider` scopes the arm's provider down a component tree.
- `@pulumi/awsx`(`.api/pulumi-awsx.md`): the higher-level `ComponentResource` compositions backing the prepared row — `awsx.ec2.Vpc` (multi-AZ VPC with subnets/NAT), `awsx.ecs.FargateService`, `awsx.lb.ApplicationLoadBalancer` — own standard compositions over the equivalent hand-wired `aws.ec2.*`/`aws.lb.*` primitives; raw `aws.*` serves only an attribute a component does not expose.
- `effect`(`libs/typescript/.api/effect.md`): provider dispatch is `Match.exhaustive` over the arm union (`provider/dispatch`), the `StackSpec`/`StackOutputs` vocabulary is `Schema` (`program/spec`, `stack/output`), the service-equivalence rows are the `provider/surface` map, and typed `StackOutputs → ShardingConfig` is the sole value crossing to `work`; the Pulumi program wraps once in `program`, returning a typed receipt over the up | preview | refresh | destroy ledger.

[LOCAL_ADMISSION]:
- `aws` arm is one dispatch row reading a `StackSpec`, constructing one `aws.Provider`, and realizing the service-equivalence subset with that provider scoped in; the `provider/surface` map is the single place a capability resolves to an `aws.*`/`awsx.*` resource class.
- `awsx` components own standard compositions (VPC, Fargate service, ALB) and raw `aws.*` resources own fine-grained control, both taking the arm's explicit StackSpec-derived provider.
- Credentials and account selection ride the `StackSpec` Doppler project ref into `ProviderArgs` (`profile`/`assumeRoles`) marked `pulumi.secret`, sourced through the `@pulumiverse/doppler`/`security/secret` read path.
- `<Resource>.get(name, id)` adopts a pre-existing cloud resource, the whole plane lib code with zero authored `Pulumi.yaml`.

[RAIL_LAW]:
- Package: `@pulumi/aws`
- Owns: the prepared `aws` provider dispatch row — the uniform AWS resource pattern, the explicit StackSpec-derived `Provider`, the data-source invokes, and the service-equivalence subset mapping AWS managed services to the `selfhosted-k8s` capability matrix
- Accept: one `Match.exhaustive` arm constructing one `aws.Provider` from a `StackSpec`, resources scoped via `{ provider }`, `Output`→`Input` dependency wiring, `awsx` components for standard compositions, `<Resource>.get` adoption, StackSpec-derived secret credentials
- Reject: a flat per-service roster, ambient AWS config, resolving `Output` to plain values inside a program, hand-wiring compositions `awsx` already owns, inline credential literals, authored `Pulumi.yaml`, promoting `aws` above the first-class `selfhosted-k8s` arm

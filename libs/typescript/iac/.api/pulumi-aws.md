# [TS_IAC_API_PULUMI_AWS]

`@pulumi/aws` is the generated Pulumi SDK for AWS: every service namespace (`ec2`, `s3`, `rds`, `eks`, `iam`, …) shares one uniform resource pattern — every resource is a `pulumi.CustomResource` subclass with a `(name, args, opts)` constructor, a `static get` adoption entry, a `static isInstance` guard, and `pulumi.Output<T>` properties — plus an explicit `Provider` the `aws` dispatch arm constructs from a `StackSpec`, and `getX`/`getXOutput` data-source invoke pairs for lookups. Service namespaces are seed data behind that single pattern; the catalog documents the pattern and the bounded *service-equivalence* subset the prepared `aws` row maps onto the first-class `selfhosted-k8s` capability matrix (compute, database, object store, cert/dns/ingress, IAM, network). It is a prepared row, never first-class — an app finalizes it by supplying a `StackSpec` value, and adding a cloud is one new `Match.exhaustive` arm, never a fork.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/aws`
- package: `@pulumi/aws` (Apache-2.0)
- asset: generated provider SDK (`.d.ts` per service namespace); peer `@pulumi/pulumi`; the `pulumi` CLI binary + the AWS provider plugin resolved on the deploy host (CLI-on-PATH deploy-host fact, wrapped once in `program`)
- owner: `iac`
- rail: deploy
- peer: `@pulumi/pulumi` (`Output`/`Input`/`CustomResource`/`ComponentResource`/`ProviderResource` model + the Automation API engine), `@pulumi/awsx` (higher-level component compositions backing this row)
- namespaces: top-level (`Provider`, `config`, `types`, and the `getArn`/`getRegion`/`getCallerIdentity`/`getAvailabilityZones`/… data-source invokes) plus one service namespace per AWS service (`ec2`, `s3`, `rds`, `eks`, `ecs`, `iam`, `acm`, `route53`, `lb`, `autoscaling`, `cloudwatch`, `efs`, `elasticache`, …)
- capability: the uniform generated-SDK resource pattern (every service resource is a `CustomResource` with `(name, args, opts)`/`static get`/`isInstance`/`Output<T>` props), the explicit `Provider` constructed from a `StackSpec`, the `getX`/`getXOutput` invoke pairs, and the service-equivalence subset mapping AWS managed services to the `selfhosted-k8s` capability matrix
- import: `import * as aws from "@pulumi/aws"` (per-namespace: `aws.s3.BucketV2`, `aws.ec2.Vpc`, `aws.rds.Cluster`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the uniform resource pattern (every namespace)
- rail: deploy
- Every AWS resource — across every service namespace — is the *same* shape. Learn the pattern once; the specific service is a namespace lookup, never a new API. `<Resource>Args` is the create-input record (fields are `pulumi.Input<T>`), `<Resource>State` is the adopt-input record, and instance properties are `pulumi.Output<T>` that flow into other resources' args as dependency edges.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]  | [RAIL]                                                       |
| :-----: | :---------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `<ns>.<Resource> extends pulumi.CustomResource` | resource class | one managed AWS resource (e.g. `s3.BucketV2`, `rds.Cluster`) |
|  [02]   | `<ns>.<Resource>Args`                           | input record   | create arguments (`readonly x?: pulumi.Input<T>`)            |
|  [03]   | `<ns>.<Resource>State`                          | input record   | adoption state for `static get`                              |
|  [04]   | `<Resource>.<prop>: pulumi.Output<T>`           | output prop    | computed attribute; a dependency edge when passed as `Input` |
|  [05]   | `types.input.*` / `types.output.*`              | nested type    | the generated nested input/output type trees                 |

[PUBLIC_TYPE_SCOPE]: provider + engine model
- rail: deploy
- `Provider` is the explicit provider the `aws` arm constructs from the `StackSpec` (rather than relying on ambient env/config); passing it as `{ provider }` in `opts` scopes every resource in the arm to that account/region/role. Resource-graph primitives (`Output`/`Input`/`CustomResource`/`ComponentResource`) are re-exported from `@pulumi/pulumi`.
- `aws.ProviderArgs` carries `{ region?, profile?, assumeRoles?, defaultTags?, accessKey?, secretKey?, token? }`; `pulumi.CustomResourceOptions` carries `{ provider?, dependsOn?, parent?, protect?, ignoreChanges? }`; `pulumi.Input<T>` accepts `T\|Promise<T>\|Output<T>`.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]  | [RAIL]                                              |
| :-----: | :--------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `aws.Provider extends pulumi.ProviderResource` | provider       | explicit AWS provider (account/region/role scope)   |
|  [02]   | `aws.ProviderArgs`                             | input record   | provider config: region, profile, roles, tags, keys |
|  [03]   | `pulumi.Output<T>` / `pulumi.Input<T>`         | graph value    | async computed value / accepted input value         |
|  [04]   | `pulumi.CustomResourceOptions`                 | options record | per-resource provider, deps, parent, protect        |

[PUBLIC_TYPE_SCOPE]: data-source invokes (the `getX` pattern)
- rail: deploy
- Read-only lookups follow one dual pattern: `getX(args?, opts?): Promise<GetXResult>` for the async form and `getXOutput(args?, opts?): Output<GetXResult>` for the graph form. Use the `Output` form inside a program so the lookup participates in the dependency graph. Result shapes: `GetRegionResult`/`GetCallerIdentityResult`/`GetAvailabilityZonesResult`.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                             |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `Get<X>Result`                    | result record | data-source return shapes (e.g. `GetRegionResult`) |
|  [02]   | `Get<X>Args` / `Get<X>OutputArgs` | input record  | invoke args; `OutputArgs` is `Input`-lifted        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider construction (from StackSpec)
- rail: deploy
- `aws` arm constructs one `Provider` from the `StackSpec` value (target arm, capability profile, region/domain, credential path). Every resource in the arm takes `{ provider }` in its options, so the whole arm is scoped to that account/region/role — no ambient/global config.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `new aws.Provider(name, { region, profile?, assumeRoles?, defaultTags? })` | provider       | explicit account/region/role provider  |
|  [02]   | `aws.Provider.isInstance(obj)`                                             | guard          | cross-SDK-copy provider instance check |

[ENTRYPOINT_SCOPE]: the resource-construction pattern (uniform)
- rail: deploy
- Three entrypoints per resource, identical across every namespace: the constructor creates and registers, `static get` adopts an existing resource by cloud id, `static isInstance` guards. Wire resources together by passing one resource's `Output<T>` property as another's `Input<T>` arg — that *is* the dependency edge, resolved by the engine.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new <ns>.<Resource>(name, args, { provider, dependsOn?, parent? })` | create         | register a resource under the arm's provider |
|  [02]   | `<ns>.<Resource>.get(name, id, state?, opts?)`                       | adopt          | bind an existing cloud resource by id        |
|  [03]   | `<ns>.<Resource>.isInstance(obj)`                                    | guard          | cross-SDK-copy resource instance check       |

[ENTRYPOINT_SCOPE]: service-equivalence map (the aws row's mapped subset)
- rail: deploy
- Prepared `aws` row maps the first-class `selfhosted-k8s` capability matrix onto AWS managed services. This bounded table — not the full service roster — is the arm's real surface; each row is a capability the `provider/surface` map resolves to a resource class. Reach past this set only when a new capability enters the matrix. Classes below are `aws.*` (the prefix elided); `awsx.*` is called out explicitly.

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
- rail: deploy
- Account/region/AZ lookups the arm needs to parameterize resources; the `Output` form keeps the lookup in the dependency graph.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `aws.getCallerIdentity()` / `aws.getCallerIdentityOutput()`    | invoke         | resolve the deploying account/arn/user id |
|  [02]   | `aws.getRegion()` / `aws.getAvailabilityZones()` (+ `…Output`) | invoke         | active region / AZ list for subnet spread |

## [04]-[IMPLEMENTATION_LAW]

[AWS_TOPOLOGY]:
- `@pulumi/aws` is a generated SDK with one uniform resource pattern across every service namespace: `CustomResource` subclass, `(name, args, opts)` constructor, `static get` adoption, `static isInstance` guard, `Output<T>` properties. Service namespaces are seed data behind that pattern — the catalog documents the pattern and the mapped service-equivalence subset, never a flat per-service roster.
- `aws` arm is one closed `Match.exhaustive` dispatch row in `provider/dispatch`; adding a cloud is one new arm in the lib, and finalizing one is app data (a `StackSpec` value), never a lib edit and never a fork. `aws` is a *prepared* row — `selfhosted-k8s` is the first-class arm; `aws`/`gcp`/`cloudflare` carry the service-equivalence map so a StackSpec can retarget without rewriting topology.
- `aws` arm constructs exactly one explicit `aws.Provider` from the `StackSpec` (region/profile/assumeRoles/defaultTags) and threads it as `{ provider }` into every resource's options — no ambient AWS env/config, so one program can drive many accounts/regions.
- resources compose by passing `Output<T>` properties as downstream `Input<T>` args; the engine derives the dependency graph from those edges. Never resolve an `Output` to a plain value inside a program — transform it with `pulumi.all`/`.apply`/`interpolate`.

[STACKS_WITH]:
- `@pulumi/pulumi` (`.api/pulumi-pulumi.md`): the engine and resource model. AWS resources are `CustomResource`s in the `LocalWorkspace.createOrSelectStack` inline program that `program/automation` wraps; `Output<T>`/`Input<T>` are the graph currency, `pulumi.all([...])`/`interpolate` combine outputs, `pulumi.secret` marks credentials, and typed `StackOutputs` (e.g. an RDS endpoint) exit the arm. `pulumi.ComponentResourceOptions.provider` scopes the arm's provider down a component tree.
- `@pulumi/awsx` (`.api/pulumi-awsx.md`): the higher-level `ComponentResource` compositions backing the prepared row — `awsx.ec2.Vpc` (multi-AZ VPC with subnets/NAT in one resource), `awsx.ecs.FargateService`, `awsx.lb.ApplicationLoadBalancer`. `awsx` components own standard compositions over hand-wiring the equivalent `aws.ec2.*`/`aws.lb.*` primitives; drop to raw `aws.*` only for attributes the component does not expose.
- `effect` (`.api/effect.md`) + iac design pages: the provider dispatch is `Match.exhaustive` over the arm union (`provider/dispatch`); the `StackSpec`/`StackOutputs` vocabulary is `Schema` (`program/spec`, `stack/output`); the service-equivalence rows are the `provider/surface` map; the `store/capability` matrix (`kube/data`) is what the `rds`/`s3` rows realize on the AWS arm, and typed `StackOutputs → ShardingConfig` is the sole value crossing to `work`. Pulumi program is wrapped once in `program`, returning a typed receipt over the up | preview | refresh | destroy ledger.

[LOCAL_ADMISSION]:
- `aws` arm is authored as one dispatch row that reads a `StackSpec`, constructs one `aws.Provider`, and realizes the service-equivalence subset with that provider scoped in; the `provider/surface` map is the single place a capability resolves to an `aws.*`/`awsx.*` resource class.
- Use `awsx` components for standard compositions (VPC, Fargate service, ALB) and raw `aws.*` resources for fine-grained control; both take the arm's explicit provider. Never rely on ambient AWS config — the provider is StackSpec-derived.
- Credentials and account selection ride the `StackSpec` Doppler project ref into `ProviderArgs` (`profile`/`assumeRoles`), marked `pulumi.secret`; the `@pulumiverse/doppler`/`security/secret` read path is the source, never inline literals.
- Adopt pre-existing cloud resources with `<Resource>.get(name, id)` rather than importing YAML; the whole plane is lib code with zero authored `Pulumi.yaml`.

[RAIL_LAW]:
- Package: `@pulumi/aws`
- Owns: the prepared `aws` provider dispatch row — the uniform AWS resource pattern, the explicit StackSpec-derived `Provider`, the data-source invokes, and the service-equivalence subset mapping AWS managed services to the `selfhosted-k8s` capability matrix
- Accept: one `Match.exhaustive` arm constructing one `aws.Provider` from a `StackSpec`, resources scoped via `{ provider }`, `Output`→`Input` dependency wiring, `awsx` components for standard compositions, `<Resource>.get` adoption, StackSpec-derived secret credentials
- Reject: a flat per-service roster in place of the pattern, ambient/global AWS config instead of the explicit provider, resolving `Output` to plain values inside a program, hand-wiring compositions `awsx` already owns, inline credential literals, authored `Pulumi.yaml`, promoting `aws` above the first-class `selfhosted-k8s` arm

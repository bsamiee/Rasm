# [TS_IAC_API_PULUMI_POSTGRESQL]

`@pulumi/postgresql` is the Terraform-bridged Pulumi provider SDK for PostgreSQL's logical surface; every managed object is one generated resource quadruple (`class X extends pulumi.CustomResource` + `XArgs` + `XState` + `X.get`/`X.isInstance`), so the package is ONE pattern over a roster.

In the `iac` plane it is the `kube/data` egress — finalizing per-app databases, roles, and extensions against the CNPG cluster `@pulumi/kubernetes` exposes, the declarative half of the `store/capability` seam `store` verifies at startup.

[EXPORTS]: `Provider`
[EXPORTS]: `Database` `Schema` `Role` `Grant` `GrantRole` `Extension` `Function` `DefaultPrivileges` `DefaultPrivileg` `Publication` `Subscription` `ReplicationSlot` `PhysicalReplicationSlot` `SecurityLabel` `UserMapping` `Server`
[EXPORTS]: `getSchemas` `getSchemasOutput` `getSequences` `getSequencesOutput` `getTables` `getTablesOutput` `config` `types`

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/postgresql`
- package: `@pulumi/postgresql` (Apache-2.0)
- module: `@pulumi/postgresql` — flat namespace, per-resource subpaths, `config`/`types` sub-namespaces
- runtime: `node` — Automation-API program process; peer `@pulumi/pulumi` owns `CustomResource`/`Output`/`Input`; needs the `pulumi-resource-postgresql` plugin binary and a reachable server
- asset: generated `CustomResource` classes, the connection `Provider`, `get*`/`get*Output` data-source pairs, `config` readers, `types.input`/`types.output` shapes
- rail: iac / data-provisioning

## [02]-[PUBLIC_TYPES]

### [02.1]-[RESOURCE_QUADRUPLE]

[PUBLIC_TYPE_SCOPE]: the shared Terraform-bridge codegen shape
- rail: iac / data-provisioning

Every resource fills this quadruple; the roster in [02.2] is the data fed to it, not sixteen bespoke APIs.

| [INDEX] | [MEMBER]           | [SHAPE]                                                                                   |
| :-----: | :----------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `class X`          | `extends pulumi.CustomResource`; `readonly <attr>: pulumi.Output<T>` per schema attribute |
|  [02]   | `constructor`      | `(name, args?: XArgs, opts?: pulumi.CustomResourceOptions)`                               |
|  [03]   | `X.get`            | `static get(name, id: pulumi.Input<pulumi.ID>, state?: XState, opts?): X`                 |
|  [04]   | `X.isInstance`     | `static isInstance(obj): obj is X`                                                        |
|  [05]   | `interface XArgs`  | construction inputs; every field `pulumi.Input<T \| undefined>`                           |
|  [06]   | `interface XState` | `.get` lookup inputs; mirrors `XArgs` all-optional                                        |

[DATABASE_ATTRS]: `Database.name: pulumi.Output<string>` `Database.owner` `Database.template` `Database.encoding` `Database.lcCollate` `Database.lcCtype` `Database.tablespaceName` `Database.connectionLimit: pulumi.Output<number|undefined>` `Database.allowConnections: pulumi.Output<boolean|undefined>` `Database.isTemplate: pulumi.Output<boolean>` `Database.alterObjectOwnership: pulumi.Output<boolean|undefined>` — `DatabaseArgs` mirrors each as `pulumi.Input<T|undefined>`

### [02.2]-[RESOURCE_ROSTER]

[PUBLIC_TYPE_SCOPE]: managed objects
- rail: iac / data-provisioning

| [INDEX] | [RESOURCE]                | [PROVISIONS]                   | [ARGS_SPINE]                                                |
| :-----: | :------------------------ | :----------------------------- | :---------------------------------------------------------- |
|  [01]   | `Database`                | logical database               | `name`, `owner`, `template`, `encoding`, `lcCollate`        |
|  [02]   | `Schema`                  | schema within a database       | `name`, `database`, `owner`, `ifNotExists`, `policies`      |
|  [03]   | `Role`                    | login or group role            | `name`, `login`, `password`, `superuser`, `createDatabase`  |
|  [04]   | `Grant`                   | object-level privilege grant   | `role`, `database`, `schema`, `objectType`, `privileges`    |
|  [05]   | `GrantRole`               | role membership grant          | `role`, `grantRole`, `withAdminOption`                      |
|  [06]   | `Extension`               | `CREATE EXTENSION`             | `name`, `database`, `schema`, `version`, `createCascade`    |
|  [07]   | `Function`                | stored function                | `name`, `database`, `schema`, `body`, `returns`, `language` |
|  [08]   | `DefaultPrivileges`       | default ACL for future objects | `role`, `database`, `schema`, `objectType`, `privileges`    |
|  [09]   | `Publication`             | logical-replication publisher  | `name`, `database`, `tables`                                |
|  [10]   | `Subscription`            | logical-replication subscriber | `conninfo`, `publications`                                  |
|  [11]   | `ReplicationSlot`         | logical WAL slot               | `name`, `database`, `plugin`                                |
|  [12]   | `PhysicalReplicationSlot` | physical WAL slot              | `name`, `database`                                          |
|  [13]   | `SecurityLabel`           | `SECURITY LABEL`               | `label`, `labelProvider`, `objectType`, `objectName`        |
|  [14]   | `UserMapping`             | FDW user mapping               | `serverName`, `userName`, `options`                         |
|  [15]   | `Server`                  | foreign server (FDW)           | `serverName`, `fdwName`, `options`, `serverType`            |
|  [16]   | `DefaultPrivileg`         | singular codegen alias         | mirrors `DefaultPrivileges`                                 |

### [02.3]-[DATA_SOURCES]

[PUBLIC_TYPE_SCOPE]: drift-read data sources
- rail: iac / drift-read

Each source ships a dual — an eager `get*(args, opts?): Promise<Result>` and a lifted `get*Output(args, opts?): pulumi.Output<Result>`; use `*Output` inside the Automation program, the `Promise` form for pre-graph inspection.

| [INDEX] | [DATA_SOURCE]  | [RETURNS]                  | [ARGS]                                                                 |
| :-----: | :------------- | :------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `getSchemas`   | `{ schemas: string[]; … }` | `database`, `includeSystemSchemas?`, `like*Patterns?`, `regexPattern?` |
|  [02]   | `getTables`    | `{ tables: {…}[]; … }`     | `database`, `schemas?`, `tableTypes?`, `like*Patterns?`                |
|  [03]   | `getSequences` | `{ sequences: {…}[]; … }`  | `database`, `schemas?`, `like*Patterns?`                               |

[SURFACES]: `getSchemas(GetSchemasArgs,pulumi.InvokeOptions?) -> Promise<GetSchemasResult>` `getSchemasOutput(GetSchemasOutputArgs,pulumi.InvokeOutputOptions?) -> pulumi.Output<GetSchemasResult>`
[LIKE_PATTERNS]: `likeAllPatterns` `likeAnyPatterns` `notLikeAllPatterns` — the `like*Patterns?` glob every data source carries

### [02.4]-[PROVIDER]

[PUBLIC_TYPE_SCOPE]: the connection boundary
- rail: iac / data-provisioning

An explicit `Provider` carries the DSN and binds every resource in the arm to the CNPG cluster; pass it via `opts.provider`, never ambient package `config`. Auth is polymorphic — one shape, mode chosen by which fields are set.

| [INDEX] | [MEMBER]              | [SIGNATURE_FIELD]                                                                                          |
| :-----: | :-------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `class Provider`      | `extends pulumi.ProviderResource`; `constructor(name, args?: ProviderArgs, opts?: pulumi.ResourceOptions)` |
|  [02]   | `Provider.isInstance` | `static isInstance(obj): obj is Provider`                                                                  |
|  [03]   | `terraformConfig`     | `(): pulumi.Output<Provider.TerraformConfigResult>` — TF-namecased config for module interop               |
|  [04]   | connection fields     | `host`, `port`, `username`, `password`, `database`, `databaseUsername`, `scheme`                           |
|  [05]   | TLS fields            | `sslmode`, `sslrootcert`, `clientcert` (`inputs.ProviderClientcert`)                                       |
|  [06]   | mode/behavior fields  | `superuser`, `expectedVersion`, `connectTimeout`, `maxConnections`                                         |
|  [07]   | AWS RDS IAM fields    | `awsRdsIamAuth`, `awsRdsIam{Profile,ProviderRoleArn,Region}`                                               |
|  [08]   | Azure identity fields | `azureIdentityAuth`, `azureTenantId`                                                                       |
|  [09]   | GCP IAM fields        | `gcpIamImpersonateServiceAccount`                                                                          |

[PROVIDER]: `Provider(string,ProviderArgs?,pulumi.ResourceOptions?)` `Provider.isInstance(any) -> obj is Provider` `Provider.terraformConfig() -> pulumi.Output<Provider.TerraformConfigResult>`

`config` re-exports every `ProviderArgs` field as a package-wide read from `postgresql:*` stack config; an explicit `Provider` lets one program bind several clusters. `types.input`/`types.output` carry the nested shapes (`ProviderClientcert`, grant `policies`).

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every managed object is the quadruple: `XArgs` fields are `pulumi.Input<T>` (a raw value, a `Promise`, or an upstream `Output<T>`), and `X` attributes are `pulumi.Output<T>` threaded through the DAG — compose with `.apply` / `pulumi.all([...]).apply` / `pulumi.output`, never read an `Output` synchronously.
- `X.get(name, id, state?, opts?)` adopts an out-of-band object (a bootstrap `postgres` superuser, a pre-seeded schema); `new X` is the create/manage path and the default, adoption the escape hatch.
- `forceNew` bridge fields (`encoding`/`lcCollate`/`lcCtype`/`template` on `Database`) replace-on-change — model them as create-time `StackSpec` constants, never mutable knobs.
- One explicit `new postgresql.Provider(name, {...})` per target cluster binds the whole per-app subgraph to the CNPG service; every resource passes `{ provider }` in `opts`.
- Auth is discriminated by field presence, not a mode enum — `password`, `awsRdsIamAuth`, `azureIdentityAuth`, `gcpIamImpersonateServiceAccount`; the self-hosted arm uses password auth with a Doppler secret, and a cloud row flips to platform IAM with no resource-code change.
- Apply opens a real libpq connection through the `pulumi-resource-postgresql` plugin, so the arm runs only where the CNPG service is reachable — an in-cluster job or a bootstrap host with line-of-sight.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `apiextensions.CustomResource` declares the CNPG `postgresql.cnpg.io/v1` `Cluster`; its `-rw` Service host `Output` feeds `Provider.host` via `pulumi.all([cluster.metadata, ns.metadata.name]).apply(...)`. This package provisions INTO that cluster — the operator CR is the `kube/data` owner, never authored here.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): the per-app subgraph runs inside a `Layer`-composed program under `LocalWorkspace.createOrSelectStack`; realized `Output`s decode through a `Schema` `StackOutputs` record — the DB host/port/role crossing to `work` as `ShardingConfig`.
- `@pulumi/policy`(`.api/pulumi-policy.md`): `validateResourceOfType(postgresql.Role, (role, _, report) => role.superuser && report(...))` narrows CrossGuard against the exact `Role`/`Grant` classes exported here.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`) / `@pulumi/random`(`.api/pulumi-random.md`): `Role`/`Provider` `password` takes a secret `Output` — a Doppler config read or `RandomPassword.result` — marked `pulumi.secret(...)`, never a literal.
- within-lib: the `Extension` roster realizes the PG18.4 capability profile the `StackSpec` capability column names, one `Extension` per `store/capability` entry with `database` bound to the per-app `Database` and `version` pinned; `getSchemas`/`getTables` feed the `policy/drift` read-back and any `store`-side conformance check; `provider/dispatch` `Match.exhaustive` selects this subgraph as the `selfhosted-k8s` arm.

[RAIL_LAW]:
- Package: `@pulumi/postgresql`
- Owns: declarative PostgreSQL logical-object provisioning — the DDL half of `store/capability`, applied against a running server
- Accept: `pulumi.Input<T>` for every arg; an explicit `Provider` bound to the CNPG service; a secret `Output` for `password`; a `Schema`-decoded `StackOutputs` crossing to `work`
- Reject: raw libpq or `pg`-client DDL in the deploy program; ambient package `config` where an explicit `Provider` binds the cluster; a mutable knob on a `forceNew` field
- Faults: apply failures carry no typed `Result` here; bridged-provider `diagnostics` fold into the run receipt at the `@pulumi/pulumi` engine rail

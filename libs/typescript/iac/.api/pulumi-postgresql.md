# [TS_IAC_API_PULUMI_POSTGRESQL]

`@pulumi/postgresql` is the Terraform-bridged Pulumi provider SDK for the PostgreSQL logical surface: it manages `Database`, `Schema`, `Role`, `Grant`/`GrantRole`, `Extension`, `Function`, `Publication`/`Subscription`, `ReplicationSlot`/`PhysicalReplicationSlot`, `DefaultPrivileges`, `SecurityLabel`, and `UserMapping` objects against a running server, plus the `Provider` that carries the connection DSN and three `get*` data sources. Every managed object is one generated resource quadruple — `class X extends pulumi.CustomResource` + `XArgs` + `XState` + `X.get`/`X.isInstance` — so the whole package is ONE pattern applied to a roster, not sixteen bespoke APIs. In the `iac` deploy plane it is the `kube/data` egress: the CNPG-provisioned PG18.4 cluster (declared by `@pulumi/kubernetes` `apiextensions.CustomResource`) exposes a service host `Output`, and this package finalizes per-app databases/schemas/roles/extensions against it — the declarative half of the `store/capability` seam that `store` verifies at startup.

```ts
// @pulumi/postgresql — resources · data sources · provider · config/types
export { Database, Schema, Role, Grant, GrantRole, Extension, Function,
         DefaultPrivileges, DefaultPrivileg, Publication, Subscription,
         ReplicationSlot, PhysicalReplicationSlot, SecurityLabel, UserMapping, Server } // pulumi.CustomResource subclasses
export { Provider }                                                                    // pulumi.ProviderResource
export { getSchemas, getSchemasOutput, getSequences, getSequencesOutput, getTables, getTablesOutput } // data sources
export { config, types }                                                               // config reads + input/output shape namespaces
```

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/postgresql`
- package: `@pulumi/postgresql`
- license: `Apache-2.0`
- build-floor: peer `@pulumi/pulumi ^catalog` (sole runtime dependency; the engine owns `CustomResource`/`Output`/`Input`)
- target: `node` (runs inside the Automation-API program process; needs the `pulumi-resource-postgresql` plugin binary on the deploy host)
- entry: `@pulumi/postgresql` (flat namespace) plus per-resource sub-paths and the `config`/`types` sub-namespaces
- asset: 16 generated `CustomResource` classes, the connection `Provider`, three `get*`/`get*Output` data-source pairs, package-wide `config` readers, and the `types.input`/`types.output` shape namespaces
- rail: iac / data-provisioning

## [02]-[PUBLIC_TYPES]

### the generated resource quadruple — every managed object

[PUBLIC_TYPE_SCOPE]: resource pattern
- rail: iac / data-provisioning
- entry: `@pulumi/postgresql`

Every resource in this package (and in `@pulumi/gcp`, and the typed half of `@pulumi/kubernetes`) is the SAME Terraform-bridge codegen shape. Learn it once; the roster below is data.

| [INDEX] | [MEMBER] | [SHAPE] |
|:-----: |:----------------- |:---------------------------------------------------------------------------------------- |
| [01] | `class X` | `extends pulumi.CustomResource`; `readonly <attr>: pulumi.Output<T>` per schema attribute |
| [02] | `constructor` | `(name: string, args?: XArgs, opts?: pulumi.CustomResourceOptions)` — registers the resource |
| [03] | `X.get` | `static get(name, id: pulumi.Input<pulumi.ID>, state?: XState, opts?): X` — adopt existing |
| [04] | `X.isInstance` | `static isInstance(obj): obj is X` — multi-SDK-safe brand check |
| [05] | `interface XArgs` | construction inputs; every field `pulumi.Input<T \| undefined>` |
| [06] | `interface XState` | `.get` lookup/filter inputs; mirrors `XArgs` as all-optional `Input` |

```ts contract
import * as pulumi from "@pulumi/pulumi"

// The canonical shape, shown on `Database` (the per-app provisioning spine). Every other
// resource row is this exact structure over a different attribute set.
declare class Database extends pulumi.CustomResource {
  static get(name: string, id: pulumi.Input<pulumi.ID>, state?: DatabaseState, opts?: pulumi.CustomResourceOptions): Database
  static isInstance(obj: any): obj is Database
  readonly name: pulumi.Output<string>
  readonly owner: pulumi.Output<string>
  readonly template: pulumi.Output<string>
  readonly encoding: pulumi.Output<string>
  readonly lcCollate: pulumi.Output<string>
  readonly lcCtype: pulumi.Output<string>
  readonly tablespaceName: pulumi.Output<string>
  readonly connectionLimit: pulumi.Output<number | undefined>
  readonly allowConnections: pulumi.Output<boolean | undefined>
  readonly isTemplate: pulumi.Output<boolean>
  readonly alterObjectOwnership: pulumi.Output<boolean | undefined>
  constructor(name: string, args?: DatabaseArgs, opts?: pulumi.CustomResourceOptions)
}
interface DatabaseArgs {
  readonly name?: pulumi.Input<string | undefined>
  readonly owner?: pulumi.Input<string | undefined>
  readonly template?: pulumi.Input<string | undefined>          // "template0" for a clean extension base
  readonly encoding?: pulumi.Input<string | undefined>          // "UTF8" | "DEFAULT" | …
  readonly lcCollate?: pulumi.Input<string | undefined>
  readonly lcCtype?: pulumi.Input<string | undefined>
  readonly tablespaceName?: pulumi.Input<string | undefined>
  readonly connectionLimit?: pulumi.Input<number | undefined>   // -1 = unlimited
  readonly allowConnections?: pulumi.Input<boolean | undefined>
  readonly isTemplate?: pulumi.Input<boolean | undefined>
  readonly alterObjectOwnership?: pulumi.Input<boolean | undefined>
}
interface DatabaseState { /* every DatabaseArgs field, still Input<T | undefined> */ }
```

### resource roster — the DATA fed to the quadruple

[PUBLIC_TYPE_SCOPE]: managed objects
- rail: iac / data-provisioning
- entry: `@pulumi/postgresql`

| [INDEX] | [RESOURCE] | [PROVISIONS] | [ARGS_SPINE] |
|:-----: |:-------------------------------------- |:--------------------------------------------------------------------- |:------------------------------------------------------------- |
| [01] | `Database` | a logical database | `name`, `owner`, `template`, `encoding`, `lcCollate` |
| [02] | `Schema` | a schema within a database | `name`, `database`, `owner`, `ifNotExists`, `policies` (grant) |
| [03] | `Role` | a login/group role | `name`, `login`, `password`, `superuser`, `createDatabase`, `roles`, `connectionLimit` |
| [04] | `Grant` | object-level privilege grant | `role`, `database`, `schema`, `objectType`, `privileges`, `objects` |
| [05] | `GrantRole` | role membership grant | `role`, `grantRole`, `withAdminOption` |
| [06] | `Extension` | a `CREATE EXTENSION` (the PG18.4 capability matrix realizer) | `name`, `database`, `schema`, `version`, `createCascade` |
| [07] | `Function` | a stored function | `name`, `database`, `schema`, `body`, `args`, `returns`, `language` |
| [08] | `DefaultPrivileges` | default-ACL for future objects | `role`, `database`, `schema`, `owner`, `objectType`, `privileges` |
| [09] | `Publication` / `Subscription` | logical-replication publisher / subscriber | `name`, `database`, `tables` / `conninfo`, `publications` |
| [10] | `ReplicationSlot` / `PhysicalReplicationSlot` | logical / physical WAL replication slot | `name`, `database`, `plugin` |
| [11] | `SecurityLabel` | a `SECURITY LABEL` (RLS/SELinux/anon labels) | `label`, `labelProvider`, `objectType`, `objectName` |
| [12] | `UserMapping` | FDW user mapping | `serverName`, `userName`, `options` |
| [13] | `Server` | a foreign server (FDW) | `serverName`, `fdwName`, `options`, `serverType`, `serverVersion` |
| [14] | `DefaultPrivileg` | singular alias retained by codegen; prefer `DefaultPrivileges` | mirrors `DefaultPrivileges` |

### `get*` data sources — the dual eager/Output pattern

[PUBLIC_TYPE_SCOPE]: data sources
- rail: iac / drift-read
- entry: `@pulumi/postgresql`

Each data source ships a dual: an eager `get*(args, opts?): Promise<Result>` (plain-value `GetXArgs`) and a lifted `get*Output(args, opts?): pulumi.Output<Result>` (`GetXOutputArgs` with `Input<T>` fields) for composition inside a resource graph. Use the `*Output` form inside the Automation program; reserve the `Promise` form for pre-graph inspection.

| [INDEX] | [DATA_SOURCE] | [RETURNS] | [ARGS] |
|:-----: |:------------ |:-------------------------------------------------- |:------------------------------------------------------- |
| [01] | `getSchemas` | `{ schemas: string[]; … }` | `database`, `includeSystemSchemas?`, `like{All,Any}Patterns?`, `notLikeAllPatterns?`, `regexPattern?` |
| [02] | `getTables` | `{ tables: {…}[]; … }` | `database`, `schemas?`, `tableTypes?`, `like*Patterns?` |
| [03] | `getSequences`| `{ sequences: {…}[]; … }` | `database`, `schemas?`, `like*Patterns?` |

```ts contract
import * as pulumi from "@pulumi/pulumi"

function getSchemas(args: GetSchemasArgs, opts?: pulumi.InvokeOptions): Promise<GetSchemasResult>
function getSchemasOutput(args: GetSchemasOutputArgs, opts?: pulumi.InvokeOutputOptions): pulumi.Output<GetSchemasResult>
interface GetSchemasArgs {
  readonly database: string
  readonly includeSystemSchemas?: boolean
  readonly likeAllPatterns?: string[]; readonly likeAnyPatterns?: string[]
  readonly notLikeAllPatterns?: string[]; readonly regexPattern?: string
}
interface GetSchemasResult { readonly id: string; readonly database: string; readonly schemas: string[] /* + echoed args */ }
interface GetSchemasOutputArgs { readonly database: pulumi.Input<string>; readonly includeSystemSchemas?: pulumi.Input<boolean | undefined> /* … */ }
```

### `Provider` — the connection boundary

[PUBLIC_TYPE_SCOPE]: provider
- rail: iac / data-provisioning
- entry: `@pulumi/postgresql`

An explicit `Provider` instance carries the DSN so every resource in the arm binds to the CNPG cluster (never package-wide ambient config). Pass it via `opts.provider`. Auth is polymorphic: password, AWS RDS IAM, Azure identity, or GCP IAM impersonation — one provider shape, mode chosen by which fields are set.

| [INDEX] | [MEMBER] | [SIGNATURE_FIELD] |
|:-----: |:-------------------- |:---------------------------------------------------------------------------------- |
| [01] | `class Provider` | `extends pulumi.ProviderResource`; `constructor(name, args?: ProviderArgs, opts?: pulumi.ResourceOptions)` |
| [02] | `Provider.isInstance` | `static isInstance(obj): obj is Provider` |
| [03] | `terraformConfig` | `(): pulumi.Output<Provider.TerraformConfigResult>` — TF-namecased config for module interop |
| [04] | connection fields | `host`, `port`, `username`, `password`, `database`, `databaseUsername`, `scheme` |
| [05] | TLS fields | `sslmode`, `sslrootcert`, `clientcert` (`inputs.ProviderClientcert`), `sslMode` (deprecated) |
| [06] | mode/behavior fields | `superuser`, `expectedVersion`, `connectTimeout`, `maxConnections` |
| [07] | cloud-IAM auth fields | `awsRdsIamAuth`+`awsRdsIam{Profile,ProviderRoleArn,Region}`, `azureIdentityAuth`+`azureTenantId`, `gcpIamImpersonateServiceAccount` |

```ts contract
import * as pulumi from "@pulumi/pulumi"
import * as inputs from "./types/input"

declare class Provider extends pulumi.ProviderResource {
  static isInstance(obj: any): obj is Provider
  constructor(name: string, args?: ProviderArgs, opts?: pulumi.ResourceOptions)
  terraformConfig(): pulumi.Output<Provider.TerraformConfigResult>
}
interface ProviderArgs {
  readonly host?: pulumi.Input<string | undefined>
  readonly port?: pulumi.Input<number | undefined>
  readonly username?: pulumi.Input<string | undefined>
  readonly password?: pulumi.Input<string | undefined>          // ← @pulumiverse/doppler or @pulumi/random secret Output
  readonly database?: pulumi.Input<string | undefined>          // bootstrap db (defaults "postgres")
  readonly sslmode?: pulumi.Input<string | undefined>           // "require" | "verify-full" | …
  readonly sslrootcert?: pulumi.Input<string | undefined>       // ← @pulumi/tls CA material
  readonly clientcert?: pulumi.Input<inputs.ProviderClientcert | undefined>
  readonly superuser?: pulumi.Input<boolean | undefined>        // false when connecting as a non-superuser app role
  readonly expectedVersion?: pulumi.Input<string | undefined>
  readonly connectTimeout?: pulumi.Input<number | undefined>
  readonly maxConnections?: pulumi.Input<number | undefined>
  readonly awsRdsIamAuth?: pulumi.Input<boolean | undefined>
  readonly azureIdentityAuth?: pulumi.Input<boolean | undefined>
  readonly gcpIamImpersonateServiceAccount?: pulumi.Input<string | undefined>
}
```

`config` re-exports every `ProviderArgs` field as a package-wide read (`config.host`, `config.superuser`, …) sourced from `postgresql:*` stack config; prefer an explicit `Provider` over ambient `config` so one program can bind several clusters. `types.input`/`types.output` carry the nested shapes (`ProviderClientcert`, grant `policies`, …).

## [03]-[IMPLEMENTATION_LAW]

[RESOURCE_TOPOLOGY]:
- Every managed object is the quadruple above. `XArgs` fields are `pulumi.Input<T>` (accept a raw value, a `Promise`, or an upstream `Output<T>`); `X` attributes are `pulumi.Output<T>` (the realized value, threaded through the DAG). Never read an `Output` synchronously — compose with `.apply`, `pulumi.all([...]).apply(...)`, or `pulumi.output(...)` from the engine.
- `X.get(name, id, state?, opts?)` adopts an object that already exists out-of-band (a bootstrap `postgres` superuser, a pre-seeded schema); construction (`new X`) is the create/manage path. Prefer construction — adoption is the escape hatch.
- The Terraform bridge means `forceNew` fields (`encoding`, `lcCollate`, `lcCtype`, `template` on `Database`) trigger replace-on-change; model them as create-time constants in the `StackSpec`, never mutable knobs.

[PROVIDER_TOPOLOGY]:
- One explicit `new postgresql.Provider(name, { host, port, username, password, database, sslmode, sslrootcert })` per target cluster; every `Database`/`Role`/`Grant` passes `{ provider }` in `opts`. This binds the whole per-app subgraph to the CNPG service without ambient state.
- Auth mode is discriminated by field presence, not a mode enum: password (`password`), AWS RDS IAM (`awsRdsIamAuth: true`), Azure (`azureIdentityAuth: true`), GCP IAM (`gcpIamImpersonateServiceAccount`). The `iac` self-hosted arm uses password auth with a Doppler-sourced secret; the prepared cloud rows can flip to their platform IAM without changing resource code.

[STACK_LAW]:
- CNPG SEAM (`kube/data`): `@pulumi/kubernetes` `apiextensions.CustomResource` declares the CNPG `postgresql.cnpg.io/v1` `Cluster` (the PG18.4-extension image). Its `.get`-able service `Output` (host of the `-rw` Service) is passed as `host` into this package's `Provider` — `pulumi.all([cluster.metadata, ns.metadata.name]).apply(([m, n]) => \`${m.name}-rw.${n}.svc\`)`. This package applies the DDL half of `store/capability`; `store` verifies it at startup. Never author the CNPG cluster here — the operator CR is the `kube/data` owner; this package provisions INTO the cluster it exposes.
- CAPABILITY MATRIX: the `Extension` roster realizes the PG18.4 extension profile named by the `StackSpec` capability column — one `Extension` row per `store/capability` entry, `database` bound to the per-app `Database`, `version` pinned. `getSchemas`/`getTables` feed the `policy/drift` read-back and any `store`-side conformance check.
- SECRET RAIL: role/`Provider` `password` fields take a secret `Output` — `@pulumiverse/doppler` config read or `@pulumi/random.RandomPassword.result` — never a literal. Mark generated credentials with the engine's `pulumi.secret(...)` so they are encrypted in state and redacted in the run receipt.
- EFFECT WEAVE: the whole per-app subgraph (Provider → Database → Schema → Role → Grant → Extension) is authored inside the `gcp`/`selfhosted-k8s` dispatch arm the `provider/dispatch` `Match.exhaustive` selects; the arm is a `Layer`-composed program run by `program/automation` `LocalWorkspace.createOrSelectStack`. Realized `Output`s project through a `Schema`-decoded `StackOutputs` record — the DB host/port/role that becomes `ShardingConfig` crossing to `work`.
- CROSSGUARD: `@pulumi/policy` gates these resources by class — `validateResourceOfType(postgresql.Role, (role, _, report) => role.superuser && report("app roles must not be superuser"))` — the pack narrows against the very `Role`/`Grant` classes exported here.

[RAIL_LAW]:
- iac / data-provisioning rail; `node`-tier. The provider plugin opens a real libpq connection at apply time, so this arm only runs where the CNPG service is reachable (in-cluster job or a bootstrap host with network line-of-sight). Failures surface as Automation-API `diagnostics` events folded into the typed run receipt — there is no in-band typed error class; the failure channel is the engine event stream, not a `Result`.

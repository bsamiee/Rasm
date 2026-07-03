# [IAC_DATA]

The data plane of the `selfhosted-k8s` arm: the object-store row (`ObjectStore` — the MinIO-versus-Garage engine choice as one vocabulary row realized through `helm.v4` typed values) and the Postgres row (`Postgres` — the CNPG operator chart, the `postgresql.cnpg.io/v1` `Cluster` CR running the PG18.4-extension image, scheduled backups with PITR retention aimed at the object store, and the per-app logical finalization through `@pulumi/postgresql`). The `store` seam lands here: `Matrix.image` from `@rasm/ts/store` is the extension roster the image must carry, the profile's `extensions` subset resolves against `Matrix.rows` or aborts, each granted row becomes one `Extension` resource pinned to its floor, and `store` verifies the same matrix at startup — iac applies the declarative DDL, the runtime proves it. The module is `iac/src/kube/data.ts`; a new extension is a store-matrix row realized here with zero structural change, a new engine is one `_engines` row, and a backup-policy axis is one profile field.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                               | [PUBLIC]      |
| :-----: | :------------- | :---------------------------------------------------------------------- | :------------ |
|  [01]   | `OBJECT_STORE` | the engine vocabulary row and the chart-realized store + credentials   | `ObjectStore` |
|  [02]   | `CNPG_CLUSTER` | the operator chart, the cluster CR, backup/PITR, and the `-rw` host    | `Postgres`    |
|  [03]   | `APP_FINALIZE` | the per-app database, role, grants, and matrix-pinned extension rows   | `Postgres`    |

## [2]-[OBJECT_STORE]

[OBJECT_STORE]:
- Owner: `ObjectStore` — the interior `_engines` table keyed by the profile's `minio | garage` literal, each row carrying the chart coordinates and a `values` column that folds root credentials, persistence size, and the provisioned bucket into that chart's own value dialect; the tier realizes one `helm.v4.Chart` from the selected row and projects `endpoint` and `bucket`.
- Law: the engine choice is data — one profile field selects the row, both engines expose one S3-compatible seam, and a third engine is one row whose `values` column speaks its dialect; engine-specific branches outside the row are the named defect.
- Law: credentials are Doppler-first — the root user/password mint through `Secrets` entries and arrive here as in-graph reads; chart values receive them as `Output`s, and the endpoint published to `StackOutputs` carries no credential.
- Law: the endpoint is a row-owned convention — the in-cluster service DNS derives from release name and namespace on the engine row, centralizing the pinned chart's naming so a chart bump edits one projection; `verify`/`version` pin chart provenance per run.
- Entry: `new ObjectStore("objects", { spec, namespace, version, auth }, opts)`; `objects.endpoint`/`objects.bucket` feed the CNPG backup block and `StackOutputs.object`.
- Growth: one `_engines` row per engine; one `values` key per new chart fact.
- Boundary: chart-value keys are the pinned chart's contract (data under the typed `values` map, drifting only with the pinned `version`); bucket lifecycle policy is the engine's own surface.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`); `../program/spec.ts` (`StackSpec`); `../stack/component.ts` (`Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import type { StackSpec } from "../program/spec.ts"
import { Tier } from "../stack/component.ts"

declare namespace ObjectStore {
  type Auth = { readonly user: pulumi.Input<string>; readonly password: pulumi.Input<string> }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly version: pulumi.Input<string>
    readonly auth: Auth
  }
}

const _engines = {
  minio: {
    chart: "minio",
    repo: "https://charts.min.io/",
    values: (auth: ObjectStore.Auth, size: string, bucket: string): Record<string, unknown> => ({
      rootUser: auth.user,
      rootPassword: auth.password,
      persistence: { size },
      buckets: [{ name: bucket }],
    }),
    endpoint: (release: string, namespace: pulumi.Input<string>): pulumi.Output<string> =>
      pulumi.interpolate`http://${release}.${namespace}.svc:9000`,
  },
  garage: {
    chart: "garage",
    repo: "https://git.deuxfleurs.fr/Deuxfleurs/garage",
    values: (_auth: ObjectStore.Auth, size: string, _bucket: string): Record<string, unknown> => ({
      persistence: { size },
    }),
    endpoint: (release: string, namespace: pulumi.Input<string>): pulumi.Output<string> =>
      pulumi.interpolate`http://${release}.${namespace}.svc:3900`,
  },
} as const

class ObjectStore extends Tier {
  readonly endpoint: pulumi.Output<string>
  readonly bucket: string
  constructor(name: string, args: ObjectStore.Args, opts?: pulumi.ComponentResourceOptions) {
    super("ObjectStore", name, opts)
    const engine = _engines[args.spec.profile.objectEngine]
    this.bucket = `${args.spec.app}-artifacts`
    new k8s.helm.v4.Chart(name, {
      chart: engine.chart,
      repositoryOpts: { repo: engine.repo },
      version: args.version,
      namespace: args.namespace,
      values: engine.values(args.auth, args.spec.profile.data.storage, this.bucket),
    }, this.child())
    this.endpoint = engine.endpoint(name, args.namespace)
    this.seal({ endpoint: this.endpoint, bucket: this.bucket })
  }
}
```

## [3]-[CNPG_CLUSTER]

[CNPG_CLUSTER]:
- Owner: `Postgres` — the CNPG operator installs as one `helm.v4.Chart` (typed values, `skipCrds` false so the CRDs ride the chart), the cluster is one `apiextensions.CustomResource` (`apiVersion: "postgresql.cnpg.io/v1"`, `kind: "Cluster"`) whose spec carries `instances` and `storage` from the profile, `imageName` as the PG18.4-extension image ref, and `backup.barmanObjectStore` aimed at the `ObjectStore` endpoint with the profile's `retention`; a second `CustomResource` (`kind: "ScheduledBackup"`) drives the profile's `backupCron`, closing scheduled-backup plus PITR on one object-store destination.
- Law: the image realizes the matrix — `imageName` must carry every `Matrix.image` row (`{ extension, floor }` from `@rasm/ts/store`); the image ref is an argument (built and published out-of-band), conformance is proven twice — the extension rows below pin floors at DDL time, `store`'s capability probe verifies at startup — and an image missing a row fails the probe, never silently degrades.
- Law: operator vocabulary rides the CR catch-all — CNPG spec fields beyond the carrier's typed `apiVersion`/`kind`/`metadata` travel the `CustomResourceArgs` index signature; their contract is the operator's own, versioned by the operator chart pin, and `protect: true` marks the cluster irreplaceable.
- Law: the admin credential is ours, not the operator's — a `kubernetes.io/basic-auth` secret carries the Doppler-generated password and the CR's `superuserSecret`/`enableSuperuserAccess` rows point at it, so the `postgresql.Provider` connects with material the secret owner already governs and no operator-minted credential exists outside the rotation epoch.
- Law: the `-rw` host is a derivation — `pulumi.all([cluster.metadata, namespace])` projects `${name}-rw.${namespace}.svc`, the write-service DNS the CNPG operator maintains; that one `Output` is the host every logical resource binds and the `StackOutputs.data.host` value.
- Entry: interior to `Postgres`; consumers read `postgres.host`, `postgres.database`, `postgres.role`.
- Growth: a new operator fact (a pooler, a replica cluster) is one CR row on this tier.
- Boundary: the operator chart's values and the CR field dialect drift with the pinned versions — the pins are args; the object-store row is `[2]`'s.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`, `apiextensions.CustomResource`); `@pulumi/pulumi` (`all`, `interpolate`); `@rasm/ts/store` (`Matrix`).

```typescript
import { Matrix } from "@rasm/ts/store"
import { Array, Option } from "effect"

declare namespace Postgres {
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly image: pulumi.Input<string>
    readonly operatorVersion: pulumi.Input<string>
    readonly objects: ObjectStore
    readonly password: pulumi.Input<string>
  }
}

class Postgres extends Tier {
  readonly host: pulumi.Output<string>
  readonly database: string
  readonly role: string
  constructor(name: string, args: Postgres.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Postgres", name, opts)
    const operator = new k8s.helm.v4.Chart(`${name}-operator`, {
      chart: "cloudnative-pg",
      repositoryOpts: { repo: "https://cloudnative-pg.github.io/charts" },
      version: args.operatorVersion,
      namespace: args.namespace,
      skipCrds: false,
    }, this.child())
    const admin = new k8s.core.v1.Secret(`${name}-admin`, {
      metadata: { namespace: args.namespace },
      type: "kubernetes.io/basic-auth",
      stringData: { username: "postgres", password: args.password },
    }, this.child())
    const cluster = new k8s.apiextensions.CustomResource(name, {
      apiVersion: "postgresql.cnpg.io/v1",
      kind: "Cluster",
      metadata: { namespace: args.namespace },
      spec: {
        instances: args.spec.profile.data.instances,
        imageName: args.image,
        storage: { size: args.spec.profile.data.storage },
        enableSuperuserAccess: true,
        superuserSecret: { name: admin.metadata.name },
        backup: {
          retentionPolicy: args.spec.profile.data.retention,
          barmanObjectStore: {
            destinationPath: pulumi.interpolate`s3://${args.objects.bucket}/postgres`,
            endpointURL: args.objects.endpoint,
          },
        },
      },
    }, this.child({ dependsOn: [operator], protect: true }))
    new k8s.apiextensions.CustomResource(`${name}-backup`, {
      apiVersion: "postgresql.cnpg.io/v1",
      kind: "ScheduledBackup",
      metadata: { namespace: args.namespace },
      spec: {
        schedule: args.spec.profile.data.backupCron,
        cluster: { name },
      },
    }, this.child({ dependsOn: [cluster] }))
    this.host = pulumi.all([cluster.metadata, args.namespace]).apply(([meta, namespace]) => `${meta.name}-rw.${namespace}.svc`)
    this.database = `${args.spec.app}`
    this.role = `${args.spec.app}_app`
    _finalized(this, args, this.child({ dependsOn: [cluster] }))
    this.seal({ host: this.host, database: this.database, role: this.role })
  }
}
```

## [4]-[APP_FINALIZE]

[APP_FINALIZE]:
- Law: finalization is one provider plus row folds — `_finalized` binds one `postgresql.Provider` to the `-rw` host (password from the Doppler read, `superuser: false` posture, `sslmode` per cluster policy), then constructs `Database` (owner-bound, `template0` base for a clean extension surface), `Role` (login, connection-limited, Doppler-sourced password), `Grant` (database-level connect/create/temporary), and one `Extension` per granted matrix row with `version` pinned to the row's floor.
- Law: the profile subset proves against the matrix — every `profile.extensions` name resolves through `Array.findFirst` over `Matrix.rows` on the `extension` column; a name the matrix does not carry throws `pulumi.RunError` naming it, because an unproven extension is a spec defect, not a provider surprise.
- Law: replace-on-change fields are create-time constants — `template`, `encoding`, and locale fields on `Database` never appear as mutable knobs; changing them is a new database by construction, and the CR's `protect` guards the cluster above it.
- Law: reads feed drift, not logic — `getSchemas`/`getTables` are `policy/drift`'s read-back material; the finalize path constructs and never probes.
- Growth: a grant tier (read-only analyst role) is one more `Role` + `Grant` row pair on the same provider; a second app database on one cluster is a second `Postgres` construction, never a widened tier.
- Boundary: what each granted capability unlocks is the `store` consumer's law; role secret rotation is the spec `epoch` through the `Secrets` entries.
- Packages: `@pulumi/postgresql` (`Provider`, `Database`, `Role`, `Grant`, `Extension`); `effect` (`Array`, `Option`); `@rasm/ts/store` (`Matrix`).

```typescript
import * as postgresql from "@pulumi/postgresql"

const _granted = (names: ReadonlyArray<string>): ReadonlyArray<(typeof Matrix.rows)[number]> =>
  Array.map(names, (name) =>
    Option.getOrThrowWith(
      Array.findFirst(Matrix.rows, (row) => row.extension === name),
      () => new pulumi.RunError(`<unknown-extension:${name}>`),
    ))

const _finalized = (owner: Postgres, args: Postgres.Args, child: pulumi.CustomResourceOptions): void => {
  const provider = new postgresql.Provider("pg", {
    host: owner.host,
    port: 5432,
    username: "postgres",
    password: args.password,
    sslmode: "require",
    superuser: false,
  }, child)
  const bound = { ...child, provider }
  const role = new postgresql.Role(owner.role, {
    name: owner.role,
    login: true,
    password: args.password,
    connectionLimit: 64,
  }, bound)
  const database = new postgresql.Database(owner.database, {
    name: owner.database,
    owner: role.name,
    template: "template0",
    encoding: "UTF8",
  }, bound)
  new postgresql.Grant(`${owner.database}-connect`, {
    role: role.name,
    database: database.name,
    objectType: "database",
    privileges: ["CONNECT", "CREATE", "TEMPORARY"],
  }, bound)
  Array.map(_granted(args.spec.profile.extensions), (row) =>
    new postgresql.Extension(row.extension, {
      name: row.extension,
      database: database.name,
      version: row.floor,
      createCascade: true,
    }, bound))
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ObjectStore, Postgres }
```

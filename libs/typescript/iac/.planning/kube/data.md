# [IAC_DATA]

The data plane of the `selfhosted-k8s` arm in three durable rows: the object store (`ObjectStore` — conditional-put-conforming self-host engines as one vocabulary row realized through `helm.v4` typed values; the CRDT-metadata engine that cannot honor `If-None-Match: *` has no row here, ruled by the data plane's engine table), the fanout store (`Nats` — the JetStream server the runtime's `jetstream` engine row dials: websocket listener, file-store persistence with fsync-per-write hardening, replica quorum from the profile), and the Postgres row (`Postgres` — the CNPG operator chart, the `postgresql.cnpg.io/v1` `Cluster` CR running the image that realizes the `data` extension matrix, scheduled backups with PITR retention aimed at the object store, and the per-app logical finalization as declarative CNPG rows: a `managed.roles` entry on the cluster and one `Database` CR, applied by the operator from inside the cluster because the deploy host reaches only the Kubernetes API, never the `.svc` network). The data seam lands here whole: `Pg.image` from `@rasm/ts/data` is the extension roster the image must carry with its `flags` priced, the profile's `extensions` subset resolves against `Pg.rows` or aborts, each granted row becomes one Database-CR `extensions` entry pinned to its floor with its preload requirement derived, and the ensure-DDL roster — relations, `Tenancy.rls` policy rows, partman parent registrations — applies at provision through one in-cluster job while the runtime's capability probe verifies the same matrix at startup: iac applies, data verifies, the runtime never mutates. The module is `iac/src/kube/data.ts`; a new extension is a data-matrix row realized here with zero structural change, a new engine is one `_engines` row, a backup or quorum axis is one profile field.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                    | [PUBLIC]      |
| :-----: | :------------- | :-------------------------------------------------------------------------- | :------------ |
|  [01]   | `OBJECT_STORE` | the conforming-engine vocabulary and the chart-realized store + credentials | `ObjectStore` |
|  [02]   | `FANOUT_STORE` | the NATS JetStream server row: websocket, fsync hardening, quorum           | `Nats`        |
|  [03]   | `CNPG_CLUSTER` | the operator chart, the cluster CR, preload rows, backup/PITR, the -rw host | `Postgres`    |
|  [04]   | `APP_FINALIZE` | the managed app role, the Database CR with matrix-pinned extensions, the ensure job | `Postgres`    |

## [2]-[OBJECT_STORE]

[OBJECT_STORE]:
- Owner: `ObjectStore` — the interior `_engines` table keyed by the profile's `minio | ceph` literal, each row carrying the chart coordinates and a `values` column that folds root credentials, persistence size, and the provisioned bucket into that chart's own value dialect; the tier realizes one `helm.v4.Chart` from the selected row and projects `endpoint` and `bucket`.
- Law: only conforming engines have rows — the `minio` row pins the maintained continuation image over the community chart and the `ceph` row is the RGW alternative; both honor `If-None-Match: *` so the data plane's write-once identity algebra holds, and the engine that cannot CAS is refused as data in the data plane's own conformance table — no literal exists to select it, so the argument is never re-had here.
- Law: credentials are Doppler-first — the root user/password mint through `Secrets` entries and arrive as in-graph reads; chart values receive them as `Output`s, the same pair lands in one namespace `Secret` (`ACCESS_KEY_ID`/`SECRET_ACCESS_KEY`) exposed as `credentials` — the sink the CNPG barman rows reference — and the endpoint published to `StackOutputs` carries no credential. The pair binds the `minio` row as chart root credentials; the `ceph` row's RGW user provisioning (the user CR that honors the same pair) is a standing RESEARCH row, so the barman reference is proven on the `minio` row and the ceph cell settles with its user row.
- Law: the endpoint is a row-owned convention — the in-cluster service DNS derives from release name and namespace on the engine row, centralizing the pinned chart's naming so a chart bump edits one projection; `version` pins the chart and provenance verify rides the pins when a keyring asset accompanies them.
- Law: lifecycle rules live with the reference ledger — the tier provisions bucket and credentials only; retention classes, reference-sweep GC, and the bucket lifecycle configuration are the data plane's own S3-API rows against the provisioned bucket, so aging policy lives beside the ledger that proves an object unreferenced, never in a chart value.
- Entry: `new ObjectStore("objects", { spec, namespace, version, auth }, opts)`; `objects.endpoint`/`objects.bucket` feed the CNPG backup block and `StackOutputs.object`.
- Growth: one `_engines` row per conforming engine; one `values` key per new chart fact.
- Boundary: chart-value keys are the pinned chart's contract, drifting only with the pinned `version`; the managed object cells (S3, R2) are the prepared arms' rows in `program/provider.md`.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`, `core.v1.Secret`); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Tier, type StackSpec } from "../program/spec.ts"

declare namespace ObjectStore {
  type Auth = { readonly user: pulumi.Input<string>; readonly password: pulumi.Input<string> }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly version: pulumi.Input<string>
    readonly auth: Auth
  }
  type _Rows<T extends Record<StackSpec.Profile["objectEngine"], {
    readonly chart: string
    readonly repo: string
    readonly values: (auth: Auth, size: string, bucket: string) => Record<string, unknown>
    readonly endpoint: (release: string, namespace: pulumi.Input<string>) => pulumi.Output<string>
  }> = typeof _engines> = T
}

const _engines = {
  minio: {
    chart: "minio",
    repo: "https://charts.min.io/",
    values: (auth: ObjectStore.Auth, size: string, bucket: string): Record<string, unknown> => ({
      image: { repository: "pgsty/minio" },
      rootUser: auth.user,
      rootPassword: auth.password,
      persistence: { size },
      buckets: [{ name: bucket }],
    }),
    endpoint: (release: string, namespace: pulumi.Input<string>): pulumi.Output<string> =>
      pulumi.interpolate`http://${release}.${namespace}.svc:9000`,
  },
  ceph: {
    chart: "rook-ceph-cluster",
    repo: "https://charts.rook.io/release",
    values: (_auth: ObjectStore.Auth, size: string, bucket: string): Record<string, unknown> => ({
      cephObjectStores: [{ name: bucket, spec: { dataPool: { size: 1 } } }],
      storage: { size },
    }),
    endpoint: (release: string, namespace: pulumi.Input<string>): pulumi.Output<string> =>
      pulumi.interpolate`http://rook-ceph-rgw-${release}.${namespace}.svc:80`,
  },
} as const

class ObjectStore extends Tier {
  readonly endpoint: pulumi.Output<string>
  readonly bucket: string
  readonly credentials: k8s.core.v1.Secret
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
    this.credentials = new k8s.core.v1.Secret(`${name}-auth`, {
      metadata: { namespace: args.namespace },
      stringData: { ACCESS_KEY_ID: args.auth.user, SECRET_ACCESS_KEY: args.auth.password },
    }, this.child())
    this.endpoint = engine.endpoint(name, args.namespace)
    this.seal({ endpoint: this.endpoint, bucket: this.bucket })
  }
}
```

## [3]-[FANOUT_STORE]

[FANOUT_STORE]:
- Owner: `Nats` — one `helm.v4.Chart` from the NATS repository realizing the JetStream server the runtime's fanout engine dials, with three value groups the tier states from the profile: the websocket listener (the browser-and-node `wsconnect` origin — no client speaks the bare NATS port), JetStream file-store persistence sized by `profile.fanout.storage`, and the cluster replica count from `profile.fanout.replicas`; `origin` projects the in-cluster websocket DNS the `fanout` output plane publishes and the `RUNTIME_FANOUT_ORIGIN` env row carries.
- Law: durability is hardened at the server, priced as data — the JetStream file store runs with fsync-per-write (`sync_interval: always` in the server's jetstream block, merged through the chart's config passthrough), because the engine's default periodic fsync loses acknowledged writes under coordinated power failure; the throughput cost is accepted, the stream is still never the system of record, and the data journal remains the record of truth the runtime law already seals.
- Law: quorum is the replica row — three replicas is the default file-store quorum (R3 tolerates one node loss), a single-replica dev profile is a deliberate spec delta, and stream-level replica counts stay the runtime's `jsm.streams.add` fact — the server row provisions capacity, the topic row spends it.
- Law: retention is the topic's, capacity is the tier's — `max_age`, dedup windows, and replay depth ride the runtime's topic policy rows; this tier states only the storage envelope and storage class, so a topic change never touches the deploy plane.
- Entry: `new Nats("fanout", { spec, namespace, version }, opts)`; `fanout.origin` feeds `StackOutputs.fanout` and the workload env.
- Growth: a leaf-node or gateway topology axis is one values group when a multi-site estate earns it; a TLS listener row composes the `Certs` chain when the origin leaves the cluster.
- Boundary: chart-value keys are the pinned chart's contract; publish/consume semantics, ack posture, and the dedup window are the runtime fanout owner's; the deployment posture here is the durability fact that page deliberately does not carry.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `@pulumi/pulumi` (`interpolate`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
class Nats extends Tier {
  readonly origin: pulumi.Output<string>
  constructor(name: string, args: { readonly spec: StackSpec; readonly namespace: pulumi.Input<string>; readonly version: pulumi.Input<string> }, opts?: pulumi.ComponentResourceOptions) {
    super("Nats", name, opts)
    new k8s.helm.v4.Chart(name, {
      chart: "nats",
      repositoryOpts: { repo: "https://nats-io.github.io/k8s/helm/charts/" },
      version: args.version,
      namespace: args.namespace,
      values: {
        config: {
          cluster: { enabled: args.spec.profile.fanout.replicas > 1, replicas: args.spec.profile.fanout.replicas },
          jetstream: {
            enabled: true,
            fileStore: { enabled: true, pvc: { size: args.spec.profile.fanout.storage } },
            merge: { sync_interval: "always" },
          },
          websocket: { enabled: true, port: 8080, no_tls: true },
        },
      },
    }, this.child())
    this.origin = pulumi.interpolate`ws://${name}.${args.namespace}.svc:8080`
    this.seal({ origin: this.origin })
  }
}
```

## [4]-[CNPG_CLUSTER]

[CNPG_CLUSTER]:
- Owner: `Postgres` — the CNPG operator installs as one `helm.v4.Chart` (typed values, `skipCrds` false so the CRDs ride the chart), the cluster is one `apiextensions.CustomResource` (`apiVersion: "postgresql.cnpg.io/v1"`, `kind: "Cluster"`) whose spec carries `instances` and `storage` from the profile, `imageName` as the extension-image ref, the preload roster, and `backup.barmanObjectStore` aimed at the `ObjectStore` endpoint with the profile's `retention` and authenticated by the object tier's `credentials` secret rows; a second `CustomResource` (`kind: "ScheduledBackup"`) drives the profile's `backupCron` — CNPG's six-field seconds-first cron dialect the spec default already speaks — closing scheduled-backup plus PITR on one object-store destination.
- Law: the image realizes the matrix — `imageName` must carry every `Pg.image` row (`{ extension, floor, flags }` from `@rasm/ts/data`); the image ref is a pin (built and published out-of-band), conformance is proven twice — the extension rows below pin floors at DDL time, the data plane's capability probe verifies at startup — and an image missing a row fails the probe, never silently degrades. The `flags` column prices the roster at derivation: a `tsl` row is source-available and stays self-managed, and an `excludesSharding` row bars a sharding engine from the same image — the conflict is a compile-visible table fact, never a runtime surprise.
- Law: preload derives from the granted subset — `_preload` filters the granted rows to the extensions demanding `shared_preload_libraries` (`pg_cron`, `timescaledb`) and stamps the CR's `postgresql.shared_preload_libraries` list; `pgaudit` is CNPG-managed — the operator injects its preload automatically when the extension lands — so no hand list exists and an unloaded preload cannot pass the startup probe.
- Law: operator vocabulary rides the CR catch-all — CNPG spec fields beyond the carrier's typed `apiVersion`/`kind`/`metadata` travel the `CustomResourceArgs` index signature; their contract is the operator's own, versioned by the operator chart pin, and `protect: true` marks the cluster irreplaceable.
- Law: the credentials are ours, not the operator's — two `kubernetes.io/basic-auth` secrets carry the Doppler-generated `admin` and `app` entries: the CR's `superuserSecret`/`enableSuperuserAccess` rows point at the first (the ensure job connects with it), and the `managed.roles` row's `passwordSecret` points at the second, so no operator-minted credential exists outside the rotation epoch; the admin and app credentials are two distinct Doppler entries by construction — one leaked app role never opens the cluster.
- Law: the `-rw` host is a derivation — `pulumi.all([cluster.metadata, namespace])` projects `${name}-rw.${namespace}.svc`, the write-service DNS the CNPG operator maintains; that one `Output` is the host every logical resource binds and the `StackOutputs.data.host` value, and `port` rides beside it as the operator's fixed write-service port so no composition site re-spells the number.
- Entry: interior to `Postgres`; consumers read `postgres.host`, `postgres.port`, `postgres.database`, `postgres.role`.
- Growth: a new operator fact (a pooler, a replica cluster, a logical-replication `Publication` seam) is one CR or resource row on this tier.
- Boundary: the operator chart's values and the CR field dialect drift with the pinned versions — the pins are args; the object-store row is `[2]`'s; the fanout row is `[3]`'s.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`, `apiextensions.CustomResource`); `@pulumi/pulumi` (`all`, `interpolate`); `@rasm/ts/data` (`Pg`); `effect` (`Array`).

```typescript
import { Pg } from "@rasm/ts/data"
import { Array, Option } from "effect"

const _preload = (granted: ReadonlyArray<(typeof Pg.rows)[number]>): ReadonlyArray<string> =>
  Array.filterMap(granted, (row) =>
    row.extension === "pg_cron" || row.extension === "timescaledb" ? Option.some(row.extension) : Option.none())

declare namespace Postgres {
  type Auth = { readonly admin: pulumi.Input<string>; readonly app: pulumi.Input<string> }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly image: pulumi.Input<string>
    readonly operatorVersion: pulumi.Input<string>
    readonly objects: ObjectStore
    readonly auth: Auth
    readonly ensures?: ReadonlyArray<string>
  }
}

class Postgres extends Tier {
  readonly host: pulumi.Output<string>
  readonly port: number = 5432
  readonly database: string
  readonly role: string
  constructor(name: string, args: Postgres.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Postgres", name, opts)
    this.database = args.spec.app
    this.role = `${args.spec.app}_app`
    const granted = _granted(args.spec.profile.extensions)
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
      stringData: { username: "postgres", password: args.auth.admin },
    }, this.child())
    const app = new k8s.core.v1.Secret(`${name}-app`, {
      metadata: { namespace: args.namespace },
      type: "kubernetes.io/basic-auth",
      stringData: { username: this.role, password: args.auth.app },
    }, this.child())
    const cluster = new k8s.apiextensions.CustomResource(name, {
      apiVersion: "postgresql.cnpg.io/v1",
      kind: "Cluster",
      metadata: { namespace: args.namespace },
      spec: {
        instances: args.spec.profile.data.instances,
        imageName: args.image,
        storage: { size: args.spec.profile.data.storage },
        postgresql: { shared_preload_libraries: [..._preload(granted)] },
        enableSuperuserAccess: true,
        superuserSecret: { name: admin.metadata.name },
        managed: {
          roles: [{
            name: this.role,
            ensure: "present",
            login: true,
            superuser: false,
            connectionLimit: 64,
            passwordSecret: { name: app.metadata.name },
          }],
        },
        backup: {
          retentionPolicy: args.spec.profile.data.retention,
          barmanObjectStore: {
            destinationPath: pulumi.interpolate`s3://${args.objects.bucket}/postgres`,
            endpointURL: args.objects.endpoint,
            s3Credentials: {
              accessKeyId: { name: args.objects.credentials.metadata.name, key: "ACCESS_KEY_ID" },
              secretAccessKey: { name: args.objects.credentials.metadata.name, key: "SECRET_ACCESS_KEY" },
            },
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
    _finalized(this, name, args, granted, admin, this.child({ dependsOn: [cluster] }))
    this.seal({ host: this.host, port: this.port, database: this.database, role: this.role })
  }
}
```

## [5]-[APP_FINALIZE]

[APP_FINALIZE]:
- Law: finalization is declarative and in-cluster — the deploy host reaches only the Kubernetes API, never the cluster's `.svc` network, so no deploy-side SQL provider can bind the `-rw` service; the app role is the Cluster CR's `managed.roles` row (login, non-superuser, connection-limited, `passwordSecret` at the app basic-auth secret — never the admin entry), and the per-app database is one `Database` CR (`cluster`-referenced, `owner`-bound to the managed role, `template0` base for a clean extension surface) whose `extensions` rows pin every granted matrix row to its floor with `ensure: "present"` — the operator applies both from inside the cluster, so finalization needs no network path the estate does not have; `pg_incremental`'s hard `cron` dependency is a matrix flag the grant subset must prove, so an inconsistent profile fails at `_granted`, never mid-apply.
- Law: the profile subset proves against the matrix — every `profile.extensions` name resolves through `Array.findFirst` over `Pg.rows` on the `extension` column, then every resolved row proves its dependency edges through the `_DEMANDS` table (a flag on the row demands a capability some granted peer carries — `requiresCron` demands a `cron` grant); an unknown name or an ungranted dependency throws `pulumi.RunError` naming it, because an unproven extension is a spec defect, not a provider surprise.
- Law: the ensure roster applies at provision — the `ensures` rows (relation DDL, `Tenancy.rls(relation)` policy blocks, `pg_partman` parent registrations, `pg_cron` schedule rows the data maintenance plane emits) arrive as SQL strings the app root collects from the data plane's published roster, and one `batch/v1.Job` executes them through `psql` against the `-rw` service with `PGPASSWORD` from the admin secret reference, `dependsOn` the Database CR, job name suffixed by the spec `epoch` so a roster change re-runs deliberately; the runtime's `Capability` probe then verifies fail-closed at startup — apply here, verify there, mutate never at runtime.
- Law: ownership carries the grants — the managed role owns its database, so connect and create are ownership facts; a grant tier beyond ownership (a read-only analyst role) is one more `managed.roles` row plus one grant statement on the ensure roster, never a second grant surface.
- Law: replace-on-change fields are create-time constants — `template`, `encoding`, and locale rows on the Database CR never appear as mutable knobs; changing them is a new database by construction, and `protect` guards the cluster above it.
- Law: schema-scoped tenancy is a Database-CR row — a `SchemaPerApp` scope adds `schemas` rows mirroring the data plane's locus derivation; the RLS case needs no schema row because `Tenancy.rls` rows ride the ensure roster.
- Growth: a second app database on one cluster is a second `Postgres` construction, never a widened tier; a new extension dependency edge is one `_DEMANDS` row; the `@pulumi/postgresql` row family survives only on the `selfhosted-docker` cell, where the daemon host coordinate is deploy-reachable.
- Boundary: what each granted capability unlocks is the data plane's consumer law; role secret rotation is the spec `epoch` through the `Secrets` entries; the `managed.roles` and Database-CR field dialect is the operator's own, versioned by the operator chart pin; out-of-band DDL drift detection is the runtime verify, ruled there.
- Packages: `@pulumi/kubernetes` (`apiextensions.CustomResource`, `batch.v1.Job`); `effect` (`Array`, `Option`); `@rasm/ts/data` (`Pg`).

```typescript
const _DEMANDS = [["requiresCron", "cron"]] as const

const _granted = (names: ReadonlyArray<string>): ReadonlyArray<(typeof Pg.rows)[number]> =>
  ((rows: ReadonlyArray<(typeof Pg.rows)[number]>) =>
    Array.map(rows, (row) =>
      Option.getOrThrowWith(
        Option.liftPredicate(row, () =>
          Array.every(_DEMANDS, ([flag, capability]) =>
            !Array.contains(row.flags, flag) || Array.some(rows, (peer) => Array.contains(peer.capabilities, capability)))),
        () => new pulumi.RunError(`<ungranted-dependency:${row.extension}>`),
      )))(
    Array.map(names, (name) =>
      Option.getOrThrowWith(
        Array.findFirst(Pg.rows, (row) => row.extension === name),
        () => new pulumi.RunError(`<unknown-extension:${name}>`),
      )),
  )

const _finalized = (
  owner: Postgres,
  cluster: string,
  args: Postgres.Args,
  granted: ReadonlyArray<(typeof Pg.rows)[number]>,
  admin: k8s.core.v1.Secret,
  child: pulumi.CustomResourceOptions,
): void => {
  const database = new k8s.apiextensions.CustomResource(owner.database, {
    apiVersion: "postgresql.cnpg.io/v1",
    kind: "Database",
    metadata: { namespace: args.namespace },
    spec: {
      cluster: { name: cluster },
      name: owner.database,
      owner: owner.role,
      template: "template0",
      encoding: "UTF8",
      extensions: Array.map(granted, (row) => ({ name: row.extension, version: row.floor, ensure: "present" })),
    },
  }, child)
  const ensures = args.ensures ?? []
  if (ensures.length > 0) {
    new k8s.batch.v1.Job(`${owner.database}-ensure-${args.spec.epoch}`, {
      metadata: { namespace: args.namespace },
      spec: {
        backoffLimit: 2,
        template: {
          spec: {
            restartPolicy: "Never",
            containers: [{
              name: "ensure",
              image: args.image,
              command: pulumi.all([owner.host]).apply(([host]) => ["psql", "-v", "ON_ERROR_STOP=1", "-h", host, "-d", owner.database, "-U", "postgres", "-c", ensures.join(";\n")]),
              env: [{ name: "PGPASSWORD", valueFrom: { secretKeyRef: { name: admin.metadata.name, key: "password" } } }],
            }],
          },
        },
      },
    }, { ...child, dependsOn: [database] })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Nats, ObjectStore, Postgres }
```

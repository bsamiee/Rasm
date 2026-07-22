# [IAC_DATA]

`ObjectStore`, `Nats`, and `Postgres` own k8s durability. `ObjectStore` admits conditional-put engines through typed chart values; `Nats` realizes the JetStream endpoint, websocket, persistence, and quorum; `Postgres` composes CNPG archiving, backups, PgBouncer, and finalization. `_TENANCY` selects shared RLS, database-per-tenant, or cluster-per-tenant. `Pg.rows` governs extensions and preload demand; one job per database applies ensure DDL, and runtime verifies capability. Growth is one matrix, engine, tenancy, backup, or quorum row.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                      | [PUBLIC]      |
| :-----: | :------------- | :-------------------------------------------------------------------------- | :------------ |
|  [01]   | `OBJECT_STORE` | the conforming-engine vocabulary and the chart-realized store + credentials | `ObjectStore` |
|  [02]   | `FANOUT_STORE` | the NATS JetStream server row: websocket, fsync hardening, quorum           | `Nats`        |
|  [03]   | `CNPG_CLUSTER` | operator + plugin charts, the typed cluster, archive CR, pooler, backups    | `Postgres`    |
|  [04]   | `APP_FINALIZE` | the tenancy escalation record, Database CRs, replication seam, ensure job   | `Postgres`    |

## [02]-[OBJECT_STORE]

[OBJECT_STORE]:
- Owner: `ObjectStore` — the interior `_engines` table keyed by the profile's `minio | ceph` literal, each row carrying the chart coordinates and a `values` column that folds root credentials, persistence size, and the provisioned bucket into that chart's own value dialect; the tier realizes one `helm.v4.Chart` from the selected row and projects `endpoint` and `bucket`.
- Law: only conforming engines have rows — the `minio` row pins the maintained continuation image over the community chart and the `ceph` row is the RGW alternative; both honor `If-None-Match: *` so the data plane's write-once identity algebra holds, and the engine that cannot CAS is refused as data in the data plane's own conformance table — no literal exists to select it, so the argument is never re-had here.
- Law: credentials are row-selected sinks — the `minio` row binds the Doppler-generated pair as chart root credentials and lands the same pair in one namespace `Secret` (`ACCESS_KEY_ID`/`SECRET_ACCESS_KEY`); the `ceph` row provisions one typed `CephObjectStoreUser` CR whose operator mints the RGW key pair into its own `rook-ceph-object-user-<store>-<user>` secret (`AccessKey`/`SecretKey` keys) — the operator, never this tier, owns that material, so the Doppler pair has no ceph spelling; each row's `credentials` projection names its secret and key spellings, the barman `ObjectStore` CR references whichever the selected row yields, and the endpoint published to `StackOutputs` carries no credential.
- Law: the endpoint is a row-owned convention — the in-cluster service DNS derives from release name and namespace on the engine row, centralizing the pinned chart's naming so a chart bump edits one projection; `version` pins the chart and provenance verify rides the pins when a keyring asset accompanies them.
- Law: lifecycle rules live with the reference ledger — the tier provisions bucket and credentials only; retention classes, reference-sweep GC, and the bucket lifecycle configuration are the data plane's own S3-API rows against the provisioned bucket, so aging policy lives beside the ledger that proves an object unreferenced, never in a chart value.
- Entry: `new ObjectStore("objects", { spec, namespace, version, auth }, opts)`; `objects.endpoint`/`objects.bucket` feed the barman archive CR and `StackOutputs.object`.
- Growth: one `_engines` row per conforming engine; one `values` key per new chart fact.
- Boundary: chart-value keys are the pinned chart's contract, drifting only with the pinned `version`; the managed object cells (S3, R2, GCS) are the cloud arms' rows in `program/provider.md`.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`, `core.v1.Secret`); `../crds/rook` (`ceph.v1.CephObjectStoreUser` — crd2pulumi, regenerated on chart bumps like every committed CRD module); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import * as rook from "../crds/rook"
import { Tier, type StackSpec } from "../program/spec.ts"

declare namespace ObjectStore {
  type Auth = { readonly user: pulumi.Input<string>; readonly password: pulumi.Input<string> }
  type Credentials = {
    readonly name: pulumi.Output<string> // the secret the barman ObjectStore CR references
    readonly keys: { readonly access: string; readonly secret: string } // the row's own key spellings inside it
  }
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
    readonly credentials: (owner: ObjectStore, name: string, args: Args, bucket: string, child: pulumi.CustomResourceOptions) => Credentials
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
    credentials: (_owner: ObjectStore, name: string, args: ObjectStore.Args, _bucket: string, child: pulumi.CustomResourceOptions): ObjectStore.Credentials => ({
      // Doppler-generated credentials land in the namespace secret referenced by the archive CR.
      name: new k8s.core.v1.Secret(`${name}-auth`, {
        metadata: { namespace: args.namespace },
        stringData: { ACCESS_KEY_ID: args.auth.user, SECRET_ACCESS_KEY: args.auth.password },
      }, child).metadata.name,
      keys: { access: "ACCESS_KEY_ID", secret: "SECRET_ACCESS_KEY" },
    }),
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
    credentials: (_owner: ObjectStore, name: string, args: ObjectStore.Args, bucket: string, child: pulumi.CustomResourceOptions): ObjectStore.Credentials => {
      // Operator custody mints the RGW pair; this tier owns only the user CR.
      const user = new rook.ceph.v1.CephObjectStoreUser(`${name}-user`, {
        metadata: { namespace: args.namespace },
        spec: { store: bucket, displayName: `${name}-archive` },
      }, child)
      return {
        name: user.metadata.apply((meta) => `rook-ceph-object-user-${bucket}-${meta.name}`),
        keys: { access: "AccessKey", secret: "SecretKey" },
      }
    },
  },
} as const

class ObjectStore extends Tier {
  readonly endpoint: pulumi.Output<string>
  readonly bucket: string
  readonly credentials: ObjectStore.Credentials
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
    this.credentials = engine.credentials(this, name, args, this.bucket, this.child())
    this.endpoint = engine.endpoint(name, args.namespace)
    this.seal({ endpoint: this.endpoint, bucket: this.bucket })
  }
}
```

## [03]-[FANOUT_STORE]

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

## [04]-[CNPG_CLUSTER]

[CNPG_CLUSTER]:
- Owner: `Postgres` — the CNPG operator and the Barman Cloud plugin install as two `helm.v4.Chart` rows (typed values, `skipCrds` false so the CRDs ride the charts), and every CNPG object is a COMMITTED `crd2pulumi`-generated class from `../crds/cnpg` — `postgresql.v1.Cluster`/`Database`/`ScheduledBackup`/`Pooler`/`Publication`/`Subscription` and `barmancloud.v1.ObjectStore` — so the operator vocabulary is compile-checked where the estate is PG-heaviest and a raw `CustomResource<any>` catch-all has no spelling here; the generated module regenerates on operator bumps, never an npm pin.
- Law: WAL archiving rides the plugin, never the in-tree block — the removal-slated `spec.backup.barmanObjectStore` has no spelling; the cluster declares `spec.plugins: [{ name: "barman-cloud.cloudnative-pg.io", isWALArchiver: true, parameters: { barmanObjectName } }]` against one `barmancloud.cnpg.io/v1` `ObjectStore` CR carrying `configuration.destinationPath`/`endpointURL`/`s3Credentials` aimed at the object tier's `credentials` sink, and the `ScheduledBackup` CR drives the profile's `backupCron` with `method: "plugin"` — scheduled backup with PITR on one object-store destination, all four resources typed.
- Law: the image realizes the matrix — `imageName` must carry every `Pg.image` row (`{ extension, floor, flags }` from `@rasm/ts/data`); the image ref is a pin, conformance is proven twice — the extension rows below pin floors at DDL time, the data plane's capability probe verifies at startup — and an image missing a row fails the probe, never silently degrades. `flags` price the roster at derivation: `tsl` stays self-managed, `excludesSharding` bars a sharding engine from the same image, and `preload` marks the `shared_preload_libraries` demand.
- Law: preload derives from the matrix flag — `_preload` filters the granted rows on the `preload` flag and stamps the cluster's `shared_preload_libraries` list, so the next preload-demanding extension lands as a data-matrix flag with zero code edit here; `pgaudit` is CNPG-managed — the operator injects its preload automatically — so no hand list exists and an unloaded preload cannot pass the startup probe.
- Law: the pooler is the published bind — one `Pooler` CR (`type: "rw"`, PgBouncer `poolMode: "transaction"`, two instances) fronts the write service, `postgres.host` publishes the pooler service DNS so every app connection multiplexes through it, and the operator-maintained `-rw` service stays the interior host the ensure job's DDL binds directly — pooled for the many, direct for the DDL.
- Law: the credentials are ours, not the operator's — three `kubernetes.io/basic-auth` secrets carry the Doppler-generated `admin`, `app`, and `analyst` entries: the cluster's `superuserSecret`/`enableSuperuserAccess` rows point at the first, and the two `managed.roles` rows bind the others, so no operator-minted credential exists outside the rotation epoch.
- Law: dependent-CR cluster references are create-only and explicitly named — `Database`, `ScheduledBackup`, and `Pooler` `cluster.name` references are CEL-validated immutable on the operator's CRDs, the generators treat them as create-time constants, and re-pointing one is a new resource by construction; the `Cluster` CR states its `metadata.name` because a nameless metadata autonames under the provider, every literal `cluster.name` reference then dangles, and the cluster name is the `-rw` service-DNS root — referenced CRs and autonaming never mix.
- Entry: interior to `Postgres`; consumers read `postgres.host`, `postgres.port`, `postgres.database`, `postgres.role`.
- Growth: a new operator fact (a replica cluster, an `ImageCatalog` row) is one typed CR row on this tier; the logical-replication seam is `[5]`'s static pair.
- Boundary: the operator chart's values and the CR field dialect drift with the pinned versions — the pins are args; the object-store row is `[2]`'s; the fanout row is `[3]`'s.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`, `core.v1.Secret`); `../crds/cnpg` (typed CNPG + barmancloud classes — crd2pulumi); `@pulumi/pulumi` (`all`, `interpolate`); `@rasm/ts/data` (`Pg`); `effect` (`Array`).

```typescript
import { Pg } from "@rasm/ts/data"
import { Array, Option } from "effect"
import * as cnpg from "../crds/cnpg"

const _preload = (granted: ReadonlyArray<(typeof Pg.rows)[number]>): ReadonlyArray<string> =>
  Array.map(
    Array.filter(granted, (row) => Array.contains(row.flags, "preload")),
    (row) => row.extension,
  )

declare namespace Postgres {
  type Auth = {
    readonly admin: pulumi.Input<string>
    readonly app: pulumi.Input<string>
    readonly analyst: pulumi.Input<string>
  }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly image: pulumi.Input<string>
    readonly operatorVersion: pulumi.Input<string>
    readonly barmanVersion: pulumi.Input<string>
    readonly objects: ObjectStore
    readonly auth: Auth
    readonly ensures?: ReadonlyArray<string>
  }
}

class Postgres extends Tier {
  static readonly publication = (
    name: string,
    args: Postgres.Replication & { readonly target: { readonly allTables: boolean } },
    child: pulumi.CustomResourceOptions,
  ): cnpg.postgresql.v1.Publication =>
    new cnpg.postgresql.v1.Publication(name, {
      metadata: { namespace: args.namespace },
      spec: { cluster: { name: args.cluster }, dbname: args.database, name, target: args.target },
    }, child)
  static readonly subscription = (
    name: string,
    args: Postgres.Replication & { readonly publication: string; readonly external: string },
    child: pulumi.CustomResourceOptions,
  ): cnpg.postgresql.v1.Subscription =>
    new cnpg.postgresql.v1.Subscription(name, {
      metadata: { namespace: args.namespace },
      spec: {
        cluster: { name: args.cluster },
        dbname: args.database,
        name,
        publicationName: args.publication,
        externalClusterName: args.external,
      },
    }, child)
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
    const barman = new k8s.helm.v4.Chart(`${name}-barman`, {
      chart: "plugin-barman-cloud",
      repositoryOpts: { repo: "https://cloudnative-pg.github.io/charts" },
      version: args.barmanVersion,
      namespace: args.namespace,
      skipCrds: false,
    }, this.child({ dependsOn: [operator] }))
    const archive = new cnpg.barmancloud.v1.ObjectStore(`${name}-archive`, {
      metadata: { namespace: args.namespace },
      spec: {
        configuration: {
          destinationPath: pulumi.interpolate`s3://${args.objects.bucket}/postgres`,
          endpointURL: args.objects.endpoint,
          s3Credentials: {
            // Selected engine rows own sink and key spellings for MinIO and Ceph custody.
            accessKeyId: { name: args.objects.credentials.name, key: args.objects.credentials.keys.access },
            secretAccessKey: { name: args.objects.credentials.name, key: args.objects.credentials.keys.secret },
          },
        },
      },
    }, this.child({ dependsOn: [barman] }))
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
    const analystRole = `${args.spec.app}_analyst`
    const analyst = new k8s.core.v1.Secret(`${name}-analyst`, {
      metadata: { namespace: args.namespace },
      type: "kubernetes.io/basic-auth",
      stringData: { username: analystRole, password: args.auth.analyst },
    }, this.child())
    const cluster = new cnpg.postgresql.v1.Cluster(name, {
      metadata: { name, namespace: args.namespace },
      spec: {
        instances: args.spec.profile.data.instances,
        imageName: args.image,
        storage: { size: args.spec.profile.data.storage },
        postgresql: { "shared_preload_libraries": [..._preload(granted)] },
        replicationSlots: { highAvailability: { enabled: true } }, // physical slots survive failover as operator data; the logical pair rides the typed CRs
        enableSuperuserAccess: true,
        superuserSecret: { name: admin.metadata.name },
        plugins: [{
          name: "barman-cloud.cloudnative-pg.io",
          isWALArchiver: true,
          parameters: { barmanObjectName: archive.metadata.name },
        }],
        managed: {
          roles: [
            {
              name: this.role,
              ensure: "present",
              login: true,
              superuser: false,
              connectionLimit: 64,
              passwordSecret: { name: app.metadata.name },
            },
            {
              name: analystRole,
              ensure: "present",
              login: true,
              superuser: false,
              inRoles: ["pg_read_all_data"],
              passwordSecret: { name: analyst.metadata.name },
            },
          ],
        },
        backup: { retentionPolicy: args.spec.profile.data.retention },
      },
    }, this.child({ dependsOn: [operator, archive], protect: true }))
    new cnpg.postgresql.v1.ScheduledBackup(`${name}-backup`, {
      metadata: { namespace: args.namespace },
      spec: {
        schedule: args.spec.profile.data.backupCron,
        cluster: { name },
        method: "plugin",
        pluginConfiguration: { name: "barman-cloud.cloudnative-pg.io" },
      },
    }, this.child({ dependsOn: [cluster] }))
    const pool = new cnpg.postgresql.v1.Pooler(`${name}-pool`, {
      metadata: { namespace: args.namespace },
      spec: {
        cluster: { name },
        instances: 2,
        type: "rw",
        pgbouncer: { poolMode: "transaction" },
      },
    }, this.child({ dependsOn: [cluster] }))
    const direct = pulumi.all([cluster.metadata, args.namespace]).apply(([meta, namespace]) => `${meta.name}-rw.${namespace}.svc`)
    this.host = pulumi.all([pool.metadata, args.namespace]).apply(([meta, namespace]) => `${meta.name}.${namespace}.svc`)
    _finalized({ owner: this, cluster: name, args, granted, admin, app, analyst, direct, child: this.child({ dependsOn: [cluster] }) })
    this.seal({ host: this.host, port: this.port, database: this.database, role: this.role })
  }
}
```

## [05]-[APP_FINALIZE]

[APP_FINALIZE]:
- Law: finalization is declarative and in-cluster — the deploy host reaches only the Kubernetes API, never the cluster's `.svc` network, so no deploy-side SQL provider can bind the `-rw` service; the app role is the cluster's `managed.roles` row (login, non-superuser, connection-limited, `passwordSecret` at the app basic-auth secret — never the admin entry), and the per-app database is one typed `Database` CR (`cluster`-referenced create-only, `owner`-bound to the managed role, `template0` base) whose `extensions` rows pin every granted matrix row to its floor with `ensure: "present"`; `pg_incremental`'s hard `cron` dependency is a matrix flag the grant subset must prove, so an inconsistent profile fails at `_granted`, never mid-apply.
- Law: tenancy escalation is the `_TENANCY` record — keyed by `StackSpec.Tenancy["pgTier"]`, exhaustive by mapped annotation: `shared-rls` adds nothing here because `Tenancy.rls` policy rows ride the ensure roster against the one database; `db-per-tenant` realizes one `Database` CR per tenant slug on the shared cluster (declarative `owner`/`extensions` — the CRD exists for exactly this, never hand-rolled `CREATE DATABASE`); `cluster-per-tenant` realizes a dedicated `Cluster` with its `Database` per tenant, sized by the same profile rows, for the tenant whose isolation demands its own WAL and failure domain; a fourth tier is one record row.
- Law: the replication seam is the typed static pair — `Postgres.publication(name, { cluster, database, target }, child)` and `Postgres.subscription(name, { cluster, database, publication, external }, child)` construct the CNPG `Publication`/`Subscription` CRs for the multi-region or tenant-migration estate; the pair is dormant capability with a typed spelling, so a replication topology is rows at the composition site, never a tier rewrite.
- Law: the profile subset proves against the matrix — every `profile.extensions` name resolves through `Array.findFirst` over `Pg.rows` on the `extension` column, then every resolved row proves its dependency edges through the `_DEMANDS` table (a flag on the row demands a capability some granted peer carries — `requiresCron` demands a `cron` grant); an unknown name or an ungranted dependency throws `pulumi.RunError` naming it — this is the one platform-seam throw the tier keeps, because the matrix rows are foreign table data, not spec coordinates.
- Law: the ensure roster applies transactionally at provision — the `ensures` rows (relation DDL, `Tenancy.rls(relation)` policy blocks, `pg_partman` parent registrations, `pg_cron` schedule rows the data maintenance plane emits) arrive as SQL strings the app root collects from the data plane's published roster, and one `batch/v1.Job` per `_DatabaseTarget` executes them through `psql` with `ON_ERROR_STOP=1` and an explicit `BEGIN`/`COMMIT` wrapper against that database's DIRECT `-rw` service (DDL never rides the transaction-pooled bind), `PGPASSWORD` from the admin secret reference, `dependsOn` the target Database CR, job name suffixed by the spec `epoch` so a roster change re-runs deliberately and a mid-roster failure leaves no partial DDL; the runtime's `Capability` probe then verifies fail-closed at startup — apply here, verify there, mutate never at runtime.
- Law: ownership carries the grants — the managed role owns its database, so connect and create are ownership facts; the analyst read tier is one more `managed.roles` row (`login`, non-superuser, `inRoles: ["pg_read_all_data"]`, its own `passwordSecret`) with one `ALTER DEFAULT PRIVILEGES` statement on the ensure roster, never a second grant surface — the docker cell's bridged `Grant`/`GrantRole`/`DefaultPrivileges` rows are the same tier in the other carrier's spelling.
- Law: the replication seam carries slots on both spellings — the cluster's `replicationSlots.highAvailability` row keeps physical slots synchronized across failover as CNPG data, the typed `Publication`/`Subscription` pair carries the logical topology, and the docker cell's bridged `ReplicationSlot` row is the same seam where the daemon host is deploy-reachable; a security-label posture (`SECURITY LABEL` provider rows) rides the ensure roster on this arm and the bridged `SecurityLabel` class on the docker cell.
- Law: replace-on-change fields are create-time constants — `template`, `encoding`, and locale rows on the Database CR never appear as mutable knobs; changing them is a new database by construction, and `protect` guards the cluster above it.
- Growth: a second app database on one cluster is a second `Postgres` construction, never a widened tier; a new extension dependency edge is one `_DEMANDS` row; a new tenancy tier is one `_TENANCY` row; the `@pulumi/postgresql` row family survives only on the `selfhosted-docker` cell, where the daemon host coordinate is deploy-reachable — its `getSchemas`/`getTables` read-back feeds `operate/policy.md`'s drift correlation on that cell alone.
- Boundary: what each granted capability unlocks is the data plane's consumer law; role secret rotation is the spec `epoch` through the `Secrets` entries; the CR field dialect is the operator's own, versioned by the operator chart pin and the regenerated `crds/cnpg` module; out-of-band DDL drift detection is the runtime verify, ruled there.
- Packages: `@pulumi/kubernetes` (`batch.v1.Job`); `../crds/cnpg` (`postgresql.v1.Database`, `Cluster`, `Publication`, `Subscription`); `effect` (`Array`, `Option`); `@rasm/ts/data` (`Pg`).

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

type _Finalize = {
  readonly owner: Postgres
  readonly cluster: string
  readonly args: Postgres.Args
  readonly granted: ReadonlyArray<(typeof Pg.rows)[number]>
  readonly admin: k8s.core.v1.Secret
  readonly app: k8s.core.v1.Secret
  readonly analyst: k8s.core.v1.Secret
  readonly direct: pulumi.Output<string>
  readonly child: pulumi.CustomResourceOptions
}

type _DatabaseTarget = {
  readonly name: string
  readonly database: string
  readonly direct: pulumi.Output<string>
  readonly ready: cnpg.postgresql.v1.Database
}

const _extensions = (granted: ReadonlyArray<(typeof Pg.rows)[number]>): ReadonlyArray<{ name: string; version: string; ensure: string }> =>
  Array.map(granted, (row) => ({ name: row.extension, version: row.floor, ensure: "present" }))

const _database = (
  name: string,
  database: string,
  ctx: _Finalize,
  cluster: string,
  child: pulumi.CustomResourceOptions = ctx.child,
): cnpg.postgresql.v1.Database =>
  new cnpg.postgresql.v1.Database(name, {
    metadata: { namespace: ctx.args.namespace },
    spec: {
      cluster: { name: cluster },
      name: database,
      owner: ctx.owner.role,
      template: "template0",
      encoding: "UTF8",
      extensions: [..._extensions(ctx.granted)],
    },
  }, child)

const _TENANCY: {
  readonly [K in StackSpec.Tenancy["pgTier"]]: (ctx: _Finalize) => ReadonlyArray<_DatabaseTarget>
} = {
  "shared-rls": () => [],
  "db-per-tenant": (ctx) =>
    Array.map(ctx.args.spec.tenants, (tenant) => {
      const name = `${ctx.owner.database}-${tenant}`
      const database = `${ctx.owner.database}_${tenant}`
      return { name, database, direct: ctx.direct, ready: _database(name, database, ctx, ctx.cluster) }
    }),
  "cluster-per-tenant": (ctx) =>
    Array.map(ctx.args.spec.tenants, (tenant) => {
      const cluster = `${ctx.cluster}-${tenant}`
      const name = `${ctx.owner.database}-${tenant}`
      const dedicated = new cnpg.postgresql.v1.Cluster(cluster, {
        metadata: { name: cluster, namespace: ctx.args.namespace },
        spec: {
          instances: ctx.args.spec.profile.data.instances,
          imageName: ctx.args.image,
          storage: { size: ctx.args.spec.profile.data.storage },
          superuserSecret: { name: ctx.admin.metadata.name },
          managed: {
            roles: [
              {
                name: ctx.owner.role,
                ensure: "present",
                login: true,
                superuser: false,
                passwordSecret: { name: ctx.app.metadata.name },
              },
              {
                name: `${ctx.owner.database}_analyst`,
                ensure: "present",
                login: true,
                superuser: false,
                inRoles: ["pg_read_all_data"],
                passwordSecret: { name: ctx.analyst.metadata.name },
              },
            ],
          },
        },
      }, { ...ctx.child, protect: true })
      return {
        name,
        database: ctx.owner.database,
        direct: pulumi.interpolate`${cluster}-rw.${ctx.args.namespace}.svc`,
        ready: _database(name, ctx.owner.database, ctx, cluster, { ...ctx.child, dependsOn: [dedicated] }),
      }
    }),
}

const _finalized = (ctx: _Finalize): void => {
  const database = _database(ctx.owner.database, ctx.owner.database, ctx, ctx.cluster)
  const targets: ReadonlyArray<_DatabaseTarget> = [
    { name: ctx.owner.database, database: ctx.owner.database, direct: ctx.direct, ready: database },
    ..._TENANCY[ctx.args.spec.profile.tenancy.pgTier](ctx),
  ]
  const ensures = [
    ...(ctx.args.ensures ?? []),
    `ALTER DEFAULT PRIVILEGES FOR ROLE ${ctx.owner.role} IN SCHEMA public GRANT SELECT ON TABLES TO ${ctx.owner.database}_analyst`,
  ]
  Array.forEach(targets, (target) => {
    new k8s.batch.v1.Job(`${target.name}-ensure-${ctx.args.spec.epoch}`, {
      metadata: { namespace: ctx.args.namespace },
      spec: {
        backoffLimit: 2,
        template: {
          spec: {
            restartPolicy: "Never",
            containers: [{
              name: "ensure",
              image: ctx.args.image,
              command: target.direct.apply((host) => [
                "psql", "-v", "ON_ERROR_STOP=1", "-h", host, "-d", target.database, "-U", "postgres",
                "-c", `BEGIN;\n${ensures.join(";\n")};\nCOMMIT;`,
              ]),
              env: [{ name: "PGPASSWORD", valueFrom: { secretKeyRef: { name: ctx.admin.metadata.name, key: "password" } } }],
            }],
          },
        },
      },
    }, { ...ctx.child, dependsOn: [target.ready] })
  })
}

declare namespace Postgres {
  type Replication = {
    readonly cluster: string
    readonly database: string
    readonly namespace: pulumi.Input<string>
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Nats, ObjectStore, Postgres }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

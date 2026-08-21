# [IAC_DATA]

`ObjectStore`, `Nats`, and `Postgres` own k8s durability — conditional-put object engines through typed chart values, the JetStream endpoint with websocket, persistence, and quorum, and CNPG archiving, backup, recovery, PgBouncer, and database targets behind one `admit` rail whose `DataRefused` faults name the extension, pooling, and recovery axes. `_TIERS` selects the tenant escalation and `_custody` gives each realized cluster its own credential triple and archive prefix. Growth is one matrix, engine, tier, pooling, or recovery row.

## [01]-[INDEX]

- [02]-[OBJECT_STORE]: the conforming-engine vocabulary and the chart-realized store + credentials; `ObjectStore`.
- [03]-[FANOUT_STORE]: the NATS JetStream server row: websocket, fsync hardening, quorum; `Nats`.
- [04]-[CNPG_CLUSTER]: the admission rail, operator charts, per-scope custody, pooler, backups; `Postgres`.
- [05]-[APP_FINALIZE]: data-tier escalation, empty Database targets, and the replication seam; `Postgres`.

## [02]-[OBJECT_STORE]

[OBJECT_STORE]:
- Owner: `ObjectStore` — the interior `_engines` table keyed by the profile's `minio | ceph` literal, each row carrying the chart coordinates and a `values` column that folds root credentials, persistence size, and the provisioned bucket into that chart's own value dialect; the tier realizes one `helm.v4.Chart` from the selected row and projects `endpoint` and `bucket`.
- Law: only conforming engines have rows — the `minio` row pins the maintained continuation image over the community chart and the `ceph` row is the RGW alternative; both honor `If-None-Match: *` so the data plane's write-once identity algebra holds, and the engine that cannot CAS is refused as data in the data plane's own conformance table — no literal exists to select it, so the argument is never re-had here.
- Law: credentials are row-selected sinks — the `minio` row binds the Doppler-generated pair as chart root credentials and lands the same pair in one namespace `Secret` (`ACCESS_KEY_ID`/`SECRET_ACCESS_KEY`); the `ceph` row provisions one typed `CephObjectStoreUser` CR whose operator mints the RGW key pair into its own `rook-ceph-object-user-<store>-<user>` secret (`AccessKey`/`SecretKey` keys) — the operator, never this tier, owns that material, so the Doppler pair has no ceph spelling; each row's `credentials` projection names its secret and key spellings, the barman `ObjectStore` CR references whichever the selected row yields, and the endpoint published to `StackOutputs` carries no credential.
- Law: the endpoint is a row-owned convention — the in-cluster service DNS derives from release name and namespace on the engine row, centralizing the pinned chart's naming so a chart bump edits one projection; `version` pins the chart, and provenance is realized rather than asserted — `_provenance` is the one fold every chart row on this page spreads, so a keyring accompanying the pins makes each row verify its signature at render and a tampered chart fails before a resource exists.
- Law: each engine row states the default-on topology it inherited — the `minio` chart defaults to a SIXTEEN-replica distributed pool whose members each claim the full persistence size, so a row supplying one storage quantity and no topology key installs a capacity commitment the profile never made; the row states `standalone` and a single replica, and a distributed pool earns a profile axis before it earns a values key. The console Service the chart renders beside the API door carries no `enabled` key at all, so this estate leaves it rendered and publishes the API door alone — an inherited default named rather than a refusal claimed against a key that does not exist.
- Law: lifecycle rules live with the reference ledger — the tier provisions bucket and credentials only; retention classes, reference-sweep GC, and the bucket lifecycle configuration are the data plane's own S3-API rows against the provisioned bucket, so aging policy lives beside the ledger that proves an object unreferenced, never in a chart value.
- Entry: `new ObjectStore("objects", { spec, namespace, version, auth, keyring? }, opts)`; `objects.endpoint`/`objects.bucket` feed the barman archive CR and `StackOutputs.object`.
- Growth: one `_engines` row per conforming engine; one `values` key per new chart fact.
- Boundary: chart-value keys are the pinned chart's contract, drifting only with the pinned `version`; the managed object cells (S3, R2, GCS) are the cloud arms' rows in `program/provider.md`.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`, `core.v1.Secret`); `@pulumi/pulumi` (`asset.Asset`); `../crds/rook` (`ceph.v1.CephObjectStoreUser` — crd2pulumi, regenerated on chart bumps like every committed CRD module); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript signature
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import * as rook from "../crds/rook"
import { Tier, type StackSpec } from "../program/spec.ts"

// Provenance is one fold every chart row on this page spreads, exactly as `operate/observe.md` spreads it: a
// keyring accompanying the pins makes each row verify its signature at render, so a tampered chart fails before
// a resource exists and the estate's content-addressed discipline reaches its chart supply too. Absent keyring
// yields no keys at all rather than `verify: false`, because the chart argument is presence-shaped.
const _provenance = (keyring: pulumi.asset.Asset | undefined): { readonly verify?: boolean; readonly keyring?: pulumi.asset.Asset } =>
  keyring === undefined ? {} : { verify: true, keyring }

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
    readonly keyring?: pulumi.asset.Asset
  }
  type _Rows<T extends Record<StackSpec.Profile["objectEngine"], {
    readonly chart: string
    readonly repo: string
    // `values` takes the release because every row must PIN its own rendered name, and `endpoint` takes the bucket
    // because a row whose Service its operator renders is named for the custom resource, never for the release.
    readonly values: (release: string, auth: Auth, size: string, bucket: string) => Record<string, unknown>
    readonly endpoint: (release: string, bucket: string, namespace: pulumi.Input<string>) => pulumi.Output<string>
    readonly credentials: (owner: ObjectStore, name: string, args: Args, bucket: string, child: pulumi.CustomResourceOptions) => Credentials
  }> = typeof _engines> = T
}

const _engines = {
  minio: {
    chart: "minio",
    repo: "https://charts.min.io/",
    values: (release: string, auth: ObjectStore.Auth, size: string, bucket: string): Record<string, unknown> => ({
      // Without this pin the chart renders `<release>-minio`, because the collapse helper drops the chart name
      // only when the release name already contains it — and no release named for its ROLE ever does.
      fullnameOverride: release,
      // The chart's own default is `distributed` at SIXTEEN replicas across one pool, each claiming the full
      // `persistence.size` — a topology and a capacity commitment no profile coordinate selected, and one the
      // single storage quantity this row is handed cannot express. `standalone` is what the profile states:
      // one server, one claim. A distributed pool earns its own profile axis before it earns a values key.
      mode: "standalone",
      replicas: 1,
      image: { repository: "pgsty/minio" },
      rootUser: auth.user,
      rootPassword: auth.password,
      persistence: { size },
      buckets: [{ name: bucket }],
    }),
    endpoint: (release: string, _bucket: string, namespace: pulumi.Input<string>): pulumi.Output<string> =>
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
    // Ceph capacity is CLAIMED DEVICES, never a size string: the cluster's own storage block takes node and device
    // selection at `cephClusterSpec.storage`, so the profile's storage coordinate has no seat on this engine and a
    // top-level `storage: { size }` is a key the chart reads nowhere. Replication rides `dataPool.replicated.size`
    // — a bare `size` beside `dataPool` is not a `PoolSpec` field and leaves the erasure-coded default standing.
    values: (_release: string, _auth: ObjectStore.Auth, _size: string, bucket: string): Record<string, unknown> => ({
      cephObjectStores: [{ name: bucket, spec: { dataPool: { replicated: { size: 1 } } } }],
    }),
    // The RGW Service is the OPERATOR's decoration over the `CephObjectStore` CR name — which is the bucket, not
    // the release — so the address reads the CR this row declared and never the chart's own release name.
    endpoint: (_release: string, bucket: string, namespace: pulumi.Input<string>): pulumi.Output<string> =>
      pulumi.interpolate`http://rook-ceph-rgw-${bucket}.${namespace}.svc:80`,
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
      ..._provenance(args.keyring),
      values: engine.values(name, args.auth, args.spec.profile.data.storage, this.bucket),
    }, this.child())
    this.credentials = engine.credentials(this, name, args, this.bucket, this.child())
    this.endpoint = engine.endpoint(name, this.bucket, args.namespace)
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
- Entry: `new Nats("fanout", { spec, namespace, version, keyring? }, opts)`; `fanout.origin` feeds `StackOutputs.fanout` and the workload env.
- Growth: a leaf-node or gateway topology axis is one values group when a multi-site estate earns it; a TLS listener row composes the `Certs` chain when the origin leaves the cluster.
- Boundary: chart-value keys are the pinned chart's contract; publish/consume semantics, ack posture, and the dedup window are the runtime fanout owner's; the deployment posture here is the durability fact that page deliberately does not carry.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `@pulumi/pulumi` (`interpolate`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript signature
class Nats extends Tier {
  readonly origin: pulumi.Output<string>
  constructor(name: string, args: { readonly spec: StackSpec; readonly namespace: pulumi.Input<string>; readonly version: pulumi.Input<string>; readonly keyring?: pulumi.asset.Asset }, opts?: pulumi.ComponentResourceOptions) {
    super("Nats", name, opts)
    new k8s.helm.v4.Chart(name, {
      chart: "nats",
      repositoryOpts: { repo: "https://nats-io.github.io/k8s/helm/charts/" },
      version: args.version,
      namespace: args.namespace,
      ..._provenance(args.keyring),
      values: {
        // Without this pin the chart renders `<release>-nats` and the websocket origin below addresses nothing:
        // the collapse helper drops the chart name only for a release name already containing it, and this row's
        // release is named for the SIGNAL it carries.
        fullnameOverride: name,
        config: {
          cluster: { enabled: args.spec.profile.fanout.replicas > 1, replicas: args.spec.profile.fanout.replicas },
          jetstream: {
            enabled: true,
            fileStore: { enabled: true, pvc: { size: args.spec.profile.fanout.storage } },
            merge: { sync_interval: "always" },
          },
          // `no_tls` is the chart's own render, not a values key — the websocket config file emits it whenever
          // `tls.enabled` is false, and this block's template reads `port` and `tls` alone. A raw server directive
          // that has no values seat rides `config.websocket.merge`, the same seam the jetstream row above uses.
          websocket: { enabled: true, port: 8080 },
        },
        // A listener is reachable only where BOTH halves agree: the config row opens the server port and this
        // row publishes it. The websocket service half ships default-ON while its config half ships OFF, so a
        // fence stating one half rides a chart default for the other and a values-side flip of either default
        // silently closes the one door every browser and node client dials.
        service: { ports: { websocket: { enabled: true } } },
        // The chart stands up a `natsBox` utility Deployment by DEFAULT — a shell pod with the CLI, which no tier
        // here declared and nothing dials; the estate's no-chart-defaults law deletes it.
        natsBox: { enabled: false },
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
- Law: the operator states its own watch scope — `config.clusterWide` shapes RBAC and nothing else while `config.data.WATCH_NAMESPACE` decides what the controller reconciles, so an operator row silent on both installs into one namespace and reconciles every namespace in the cluster, tenant namespaces and virtual planes included; the arm seats every cluster this tier declares in one namespace, so the watch key states it and the grant narrows to match. The webhook Service name is fixed by the chart and no address here derives from the pinned name.
- Law: Barman plugin rows own WAL archiving, scheduled backup, full recovery, and PITR against one typed `ObjectStore` resource.
- Law: `Postgres.Recovery` admits empty or archive bootstrap; `RecoveryPoint` closes latest, time, LSN, XID, named, and immediate targets.
- Archive recovery names a distinct source server; `_cluster` folds image, preload, slots, roles, backup, and recovery into every cluster.
- Law: every instance carries its scheduling envelope — `spec.resources` is `core/v1` `ResourceRequirements` verbatim, the `requests`/`limits` quantity maps beside the `claims` list, and it reads the profile's own `data.requests`/`data.limits` pair, since a cluster CR stating neither runs BestEffort and makes the one workload holding durable state the first eviction candidate under node pressure; the stamp is PER-CONTAINER and the CR carries no pod-level seat at all — the operator copies that one envelope onto the `postgres` container, the `bootstrap-controller` init container, every primary Job it constructs (initdb, import, base-backup, full-recovery, join, snapshot-recovery), and the major-upgrade `prepare` container, so the pod's effective request stays the envelope rather than a multiple of it because the init container runs to completion before the postmaster starts, and the Guaranteed class the postmaster's OOM adjustment rides is earned only where requests equal limits on both axes. Quantity grammar rides the spec's brand, so a malformed envelope fails at decode rather than at the operator.
- Law: the image realizes the matrix — `imageName` must carry every `Pg.image` row (`{ extension, floor, flags }` from `@rasm/ts/data`); the image ref is a pin and the floor is a lower bound the startup capability probe alone enforces, because CNPG's extension `version` field demands an exact match and a floor fed to it refuses every image shipping a newer build; an image missing a row fails the probe, never silently degrades. `flags` price the roster at derivation: `tsl` stays self-managed and `preload` marks the `shared_preload_libraries` demand.
- Law: preload derives from the matrix flag — `_preload` filters the granted rows on the `preload` flag and stamps the cluster's `shared_preload_libraries` list, so the next preload-demanding extension lands as a data-matrix flag with zero code edit here; `pgaudit` is CNPG-managed — the operator injects its preload automatically — so no hand list exists and an unloaded preload cannot pass the startup probe.
- Law: `admit` is the one entry — the extension matrix, the pooling axis, and every realized scope's recovery source prove on the typed `DataRefused` rail before a chart is declared, each fault naming its axis; a refused spec never half-constructs a tier and construction-time refusal has no spelling left on this page.
- Law: the pooler mode is the spec's pooling axis, not a tier literal — `_POOLING` resolves `spec.profile.data.pooling` to the operator spelling `pgbouncer.poolMode` admits, `pooling` publishes that resolved mode on the tier and the `data` output plane, and each target exposes the operator-maintained direct host so convergence runners hold a session the pooler mode never truncates.
- Law: the operator admits fewer modes than the bouncer does — the `Pooler` CRD closes `poolMode` on `session | transaction`, so `statement` is a PgBouncer posture with no CNPG spelling and the pooling axis refuses it here rather than letting the API server reject a CR the tier already declared; the same table prices the voided primitives, so one row states both what a mode costs and whether this operator can run it at all.
- Law: the pooler is sized from the profile like the cluster above it — `data.pool` carries the replica count, the scheduling envelope, and the two connection ceilings (`max_client_conn`, `default_pool_size`) the bouncer meters clients and server sessions by, so `spec.instances` reads a profile column rather than a tier literal and the CRD's own default of ONE never decides a pool's width. Both ceilings ride `pgbouncer.parameters`, a `map[string]string` the admission webhook proves key-by-key against its own allowlist and rejects unlisted as `Invalid or reserved parameter` — so a ceiling is a string literal and a mistyped knob dies at the API server rather than inside a bouncer that already accepted the pod. Sizing lands on the one container entry named `pgbouncer`, which the operator merges BY NAME — patching image, command, ports, env, and probes while writing `resources` nowhere — so that entry is the only seat reaching the process, and `template.spec.containers` is mandatory the moment a template appears at all. `template.spec.resources` beside it is the alpha pod-level `PodLevelResources` field the operator reads to size the bootstrap init container alone, so a row stating only that leaves the bouncer BestEffort in front of a Guaranteed database.
- Law: the credentials are ours, not the operator's, and they key by scope — `_custody` mints one `kubernetes.io/basic-auth` triple (`admin`, `app`, `analyst`) and one barman `ObjectStore` per realized cluster out of the caller's `auth(scope)` mint, the cluster's `superuserSecret`/`enableSuperuserAccess` rows pointing at that scope's admin and its two `managed.roles` rows at the others; a shared superuser secret or a shared WAL destination across dedicated clusters returns exactly the blast radius the dedicated tier buys, so the archive prefix carries the scope.
- Law: dependent-CR cluster references are create-only and explicitly named — `Database`, `ScheduledBackup`, and `Pooler` `cluster.name` references are CEL-validated immutable on the operator's CRDs, the generators treat them as create-time constants, and re-pointing one is a new resource by construction; the `Cluster` CR states its `metadata.name` because a nameless metadata autonames under the provider, every literal `cluster.name` reference then dangles, and the cluster name is the `-rw` service-DNS root — referenced CRs and autonaming never mix. Poolers carry a second name law beside that one: a pooler's own `metadata.name` may never equal a cluster name in the namespace, which the `-pool` suffix satisfies by construction.
- Entry: `Postgres.admit("data", { spec, namespace, image, operatorVersion, barmanVersion, objects, auth, recovery, keyring? }, opts)` inside the k8s arm; consumers read `postgres.host`, `postgres.port`, `postgres.database`, `postgres.role`, `postgres.pooling`.
- Growth: a new operator fact is one typed CR row; a recovery criterion is one `RecoveryPoint` case; a new refusal axis is one `DataRefused` literal with its proof in `admit`; logical replication is `[5]`'s static pair.
- Boundary: the operator chart's values and the CR field dialect drift with the pinned versions — the pins are args; the object-store row is `[2]`'s; the fanout row is `[3]`'s; the per-scope credential mint is the composing arm's `auth` callback.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`, `core.v1.Secret`); `../crds/cnpg` (typed CNPG + barmancloud classes — crd2pulumi); `@pulumi/pulumi` (`all`, `interpolate`); `@rasm/ts/data` (`Pg`); `effect` (`Array`, `Data`, `Effect`, `Match`, `Option`).

```typescript signature
import { Pg } from "@rasm/ts/data"
import { Array, Data, Effect, Match, Option } from "effect"
import * as cnpg from "../crds/cnpg"

// --- [ERRORS] ----------------------------------------------------------------------------

class DataRefused extends Data.TaggedError("DataRefused")<{
  readonly axis: "extensions" | "pooling" | "recovery"
  readonly detail: string
}> {}

const _preload = (granted: ReadonlyArray<(typeof Pg.rows)[number]>): ReadonlyArray<string> =>
  Array.map(
    Array.filter(granted, (row) => Array.contains(row.flags, "preload")),
    (row) => row.extension,
  )

declare namespace Postgres {
  // The operator's own enum, not the bouncer's: `statement` has no seat here, so the resolved mode is what
  // the tier publishes and the runtime capability rail reads back.
  type PoolMode = "session" | "transaction"
  type RecoveryBound = {
    readonly backup: Option.Option<string>
    readonly exclusive: boolean
  }
  type RecoveryPoint =
    | { readonly _tag: "latest" }
    | ({ readonly _tag: "time"; readonly at: string } & RecoveryBound)
    | ({ readonly _tag: "lsn"; readonly at: string } & RecoveryBound)
    | { readonly _tag: "xid"; readonly at: string; readonly backup: string; readonly exclusive: boolean }
    | { readonly _tag: "name"; readonly at: string; readonly backup: string; readonly exclusive: boolean }
    | { readonly _tag: "immediate"; readonly backup: string }
  type Recovery =
    | { readonly _tag: "empty" }
    | { readonly _tag: "archive"; readonly server: string; readonly point: RecoveryPoint }
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
    readonly auth: (scope: string) => Auth
    readonly recovery: (scope: string) => Recovery
    readonly keyring?: pulumi.asset.Asset
  }
  type Target = {
    readonly name: string
    readonly database: string
    readonly direct: pulumi.Output<string>
    readonly ready: cnpg.postgresql.v1.Database
    readonly env: ReadonlyArray<k8s.types.input.core.v1.EnvVar>
  }
  type Targets = readonly [Target, ...ReadonlyArray<Target>]
}

const _BARMAN_PLUGIN = "barman-cloud.cloudnative-pg.io"

// PgBouncer's mode decides which server-side state survives a client's next statement, so the mode is a
// capability input: every primitive a row names dies under that mode across the pooled bind. `spelling` is
// the operator's half of the same fact — the `Pooler` CRD's `poolMode` enum admits `session` and
// `transaction` alone, so the bouncer's third posture resolves to nothing and admission refuses it on this
// one table rather than in a second place that could disagree with it.
const _POOLING: {
  readonly [K in StackSpec.Pooling]: {
    readonly spelling: Option.Option<Postgres.PoolMode>
    readonly voids: ReadonlyArray<Pg.Primitive>
  }
} = {
  session: { spelling: Option.some("session"), voids: [] },
  transaction: { spelling: Option.some("transaction"), voids: ["advisory", "channel"] },
  statement: { spelling: Option.none(), voids: ["advisory", "channel", "skipLocked"] },
}

// `_scopes` enumerates every cluster the data-tier escalation realizes, so recovery proves against
// that whole set and never the primary alone.
const _scopes = (name: string, spec: StackSpec): ReadonlyArray<string> =>
  spec.pgTier === "cluster-per-tenant"
    ? [name, ...Array.map(spec.tenants, (tenant) => `${name}-${tenant}`)]
    : [name]

const _optionalBackup = (backup: Option.Option<string>) =>
  Option.match(backup, {
    onNone: () => ({}),
    onSome: (backupID) => ({ backupID }),
  })

const _recoveryTarget = (point: Postgres.RecoveryPoint) =>
  Match.value(point).pipe(
    Match.tagsExhaustive({
      latest: () => ({}),
      time: ({ at, backup, exclusive }) => ({
        recoveryTarget: { targetTime: at, exclusive, ..._optionalBackup(backup) },
      }),
      lsn: ({ at, backup, exclusive }) => ({
        recoveryTarget: { targetLSN: at, exclusive, ..._optionalBackup(backup) },
      }),
      xid: ({ at, backup, exclusive }) => ({
        recoveryTarget: { targetXID: at, backupID: backup, exclusive },
      }),
      name: ({ at, backup, exclusive }) => ({
        recoveryTarget: { targetName: at, backupID: backup, exclusive },
      }),
      immediate: ({ backup }) => ({
        recoveryTarget: { targetImmediate: true, backupID: backup },
      }),
    }),
  )

// Admission already proved the source names a foreign server for every realized scope, so the fold
// is total and no construction-time refusal remains.
const _bootstrap = (
  target: string,
  recovery: Postgres.Recovery,
  archive: pulumi.Input<string>,
) =>
  Match.value(recovery).pipe(
    Match.tagsExhaustive({
      empty: () => ({}),
      archive: ({ server, point }) => {
        const source = `${target}-recovery`
        return {
          bootstrap: { recovery: { source, ..._recoveryTarget(point) } },
          externalClusters: [{
            name: source,
            plugin: {
              name: _BARMAN_PLUGIN,
              parameters: { barmanObjectName: archive, serverName: server },
            },
          }],
        }
      },
    }),
  )

type _Custody = {
  readonly archive: cnpg.barmancloud.v1.ObjectStore
  readonly admin: k8s.core.v1.Secret
  readonly app: k8s.core.v1.Secret
  readonly analyst: k8s.core.v1.Secret
}

type _ClusterArgs = {
  readonly args: Postgres.Args
  readonly granted: ReadonlyArray<(typeof Pg.rows)[number]>
  readonly custody: _Custody
  readonly role: string
  readonly analystRole: string
}

// One cluster, one custody envelope: the scope keys its own credential triple out of the caller's
// per-scope mint and its own archive prefix, so a dedicated cluster shares neither superuser
// material nor WAL destination with the primary or with a sibling tenant.
const _custody = (
  scope: string,
  args: Postgres.Args,
  role: string,
  analystRole: string,
  child: pulumi.CustomResourceOptions,
): _Custody => {
  const auth = args.auth(scope)
  const secret = (kind: string, username: string, password: pulumi.Input<string>): k8s.core.v1.Secret =>
    new k8s.core.v1.Secret(`${scope}-${kind}`, {
      metadata: { namespace: args.namespace },
      type: "kubernetes.io/basic-auth",
      stringData: { username, password },
    }, child)
  return {
    archive: new cnpg.barmancloud.v1.ObjectStore(`${scope}-archive`, {
      metadata: { namespace: args.namespace },
      spec: {
        configuration: {
          destinationPath: pulumi.interpolate`s3://${args.objects.bucket}/postgres/${scope}`,
          endpointURL: args.objects.endpoint,
          s3Credentials: {
            // Selected engine rows own sink and key spellings for MinIO and Ceph custody.
            accessKeyId: { name: args.objects.credentials.name, key: args.objects.credentials.keys.access },
            secretAccessKey: { name: args.objects.credentials.name, key: args.objects.credentials.keys.secret },
          },
        },
      },
    }, child),
    admin: secret("admin", "postgres", auth.admin),
    app: secret("app", role, auth.app),
    analyst: secret("analyst", analystRole, auth.analyst),
  }
}

const _cluster = (
  name: string,
  ctx: _ClusterArgs,
  child: pulumi.CustomResourceOptions,
): cnpg.postgresql.v1.Cluster => {
  const cluster = new cnpg.postgresql.v1.Cluster(name, {
    metadata: { name, namespace: ctx.args.namespace },
    spec: {
      instances: ctx.args.spec.profile.data.instances,
      imageName: ctx.args.image,
      storage: { size: ctx.args.spec.profile.data.storage },
      // Without this block every instance schedules BestEffort, so the estate's only stateful pod is the first
      // one the kubelet evicts under node pressure and its CPU is whatever the node has left after the app.
      resources: { requests: ctx.args.spec.profile.data.requests, limits: ctx.args.spec.profile.data.limits },
      postgresql: { "shared_preload_libraries": [..._preload(ctx.granted)] },
      replicationSlots: { highAvailability: { enabled: true } },
      enableSuperuserAccess: true,
      superuserSecret: { name: ctx.custody.admin.metadata.name },
      plugins: [{
        name: _BARMAN_PLUGIN,
        isWALArchiver: true,
        parameters: { barmanObjectName: ctx.custody.archive.metadata.name },
      }],
      managed: {
        roles: [
          {
            name: ctx.role,
            ensure: "present",
            login: true,
            superuser: false,
            connectionLimit: 64,
            passwordSecret: { name: ctx.custody.app.metadata.name },
          },
          {
            name: ctx.analystRole,
            ensure: "present",
            login: true,
            superuser: false,
            inRoles: ["pg_read_all_data"],
            passwordSecret: { name: ctx.custody.analyst.metadata.name },
          },
        ],
      },
      backup: { retentionPolicy: ctx.args.spec.profile.data.retention },
      ..._bootstrap(name, ctx.args.recovery(name), ctx.custody.archive.metadata.name),
    },
  }, { ...child, protect: true })
  new cnpg.postgresql.v1.ScheduledBackup(`${name}-backup`, {
    metadata: { namespace: ctx.args.namespace },
    spec: {
      schedule: ctx.args.spec.profile.data.backupCron,
      cluster: { name },
      method: "plugin",
      pluginConfiguration: { name: _BARMAN_PLUGIN },
    },
  }, { ...child, dependsOn: [cluster] })
  return cluster
}

class Postgres extends Tier {
  static readonly scopes = _scopes
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
  // `admit` is the one entry: the extension matrix, the pooling axis, and every realized scope's
  // recovery source prove on the typed rail before a chart is declared, so a refused spec never
  // half-constructs a tier and no construction-time throw stands in for a spec-derivable verdict.
  static admit(
    name: string,
    args: Postgres.Args,
    opts?: pulumi.ComponentResourceOptions,
  ): Effect.Effect<Postgres, DataRefused> {
    return Effect.gen(function* () {
      const granted = yield* _granted(args.spec.profile.extensions)
      yield* _pooled(args.spec.profile.data)
      yield* _recoverable(name, args)
      return new Postgres(name, granted, args, opts)
    })
  }
  readonly host: pulumi.Output<string>
  readonly port: number = 5432
  readonly database: string
  readonly role: string
  readonly pooling: StackSpec.Pooling
  readonly targets: Postgres.Targets
  private constructor(
    name: string,
    granted: ReadonlyArray<(typeof Pg.rows)[number]>,
    args: Postgres.Args,
    opts?: pulumi.ComponentResourceOptions,
  ) {
    super("Postgres", name, opts)
    this.database = args.spec.app
    this.role = `${args.spec.app}_app`
    this.pooling = args.spec.profile.data.pooling
    const operator = new k8s.helm.v4.Chart(`${name}-operator`, {
      chart: "cloudnative-pg",
      repositoryOpts: { repo: "https://cloudnative-pg.github.io/charts" },
      version: args.operatorVersion,
      namespace: args.namespace,
      skipCrds: false,
      ..._provenance(args.keyring),
      values: {
        // `clusterWide` shapes RBAC and NOTHING else: the controller reconciles whatever `WATCH_NAMESPACE`
        // names, and an unset key means every namespace in the cluster — including every tenant namespace
        // Capsule governs and every virtual plane a vcluster tenant owns. This estate's clusters all live in
        // the arm's one namespace, so the watch scope states it and the grant narrows to match.
        config: { clusterWide: false, data: { WATCH_NAMESPACE: args.namespace } },
      },
    }, this.child())
    const barman = new k8s.helm.v4.Chart(`${name}-barman`, {
      chart: "plugin-barman-cloud",
      repositoryOpts: { repo: "https://cloudnative-pg.github.io/charts" },
      version: args.barmanVersion,
      namespace: args.namespace,
      skipCrds: false,
      ..._provenance(args.keyring),
    }, this.child({ dependsOn: [operator] }))
    const analystRole = `${args.spec.app}_analyst`
    const custody = _custody(name, args, this.role, analystRole, this.child({ dependsOn: [barman] }))
    const cluster = _cluster(name, { args, granted, custody, role: this.role, analystRole },
      this.child({ dependsOn: [operator, custody.archive] }))
    const pooled = args.spec.profile.data.pool
    // Suffixing with `-pool` is load-bearing: the operator refuses a Pooler whose own name matches any
    // cluster name in the namespace, and this tier declares both halves of that collision.
    const pool = new cnpg.postgresql.v1.Pooler(`${name}-pool`, {
      metadata: { namespace: args.namespace },
      spec: {
        cluster: { name },
        instances: pooled.instances,
        type: "rw",
        pgbouncer: {
          poolMode: this.pooling,
          // `parameters` types as `map[string]string`, so a ceiling is a STRING — and the admission webhook
          // proves every key against its own allowlist, rejecting an unlisted one as `Invalid or reserved
          // parameter`. Both spellings here sit on that list, so the profile's ceilings are the ones that run.
          parameters: {
            "max_client_conn": `${pooled.clients}`,
            "default_pool_size": `${pooled.sessions}`,
          },
        },
        // CNPG merges this template by container NAME: it patches image, command, ports, env, and probes
        // onto the entry named `pgbouncer` and writes `resources` on it NOWHERE, so this is the only seat
        // reaching the bouncer. Pod-level `spec.resources` beside it sizes the bootstrap init container
        // alone. `containers` is mandatory the moment a template exists — `[]` is the empty form.
        template: {
          spec: {
            containers: [{
              name: "pgbouncer",
              resources: { requests: pooled.requests, limits: pooled.limits },
            }],
          },
        },
      },
    }, this.child({ dependsOn: [cluster] }))
    const direct = pulumi.all([cluster.metadata, args.namespace]).apply(([meta, namespace]) => `${meta.name}-rw.${namespace}.svc`)
    this.host = pulumi.all([pool.metadata, args.namespace]).apply(([meta, namespace]) => `${meta.name}.${namespace}.svc`)
    this.targets = _finalized({
      owner: this,
      cluster: name,
      args,
      granted,
      custody,
      analystRole,
      direct,
      child: this.child({ dependsOn: [cluster] }),
    })
    this.seal({
      host: this.host,
      port: this.port,
      database: this.database,
      role: this.role,
      pooling: this.pooling,
      targets: Array.map(this.targets, (target) => target.name),
    })
  }
}
```

## [05]-[APP_FINALIZE]

[APP_FINALIZE]:
- Law: finalization is declarative and in-cluster; the deploy host reaches only Kubernetes, never the cluster's `.svc` network.
- Law: each typed `Database` CR converges the managed app role, `template0`, and granted extension floors after cluster readiness.
- Law: `_granted` refuses unknown extensions and ungranted dependency capabilities on the rail before any target reaches the provider.
- Law: `_TIERS` exhaustively maps each `pgTier` to zero or more additional targets and reads `spec.pgTier`, the one total projection over the separation union; a fourth tier is one record row.
- Law: `Postgres.targets` exposes readiness and libpq environment rows; `Converge` owns framework materialization, hydration, and proof.
- Law: a target authenticates as its scope's managed owner — the libpq rows carry `ctx.owner.role` against that scope's app secret, so every relation the runner authors lands owned by the role the application then binds as, and no grant fold repairs a superuser-owned schema after the fact.
- Law: the replication seam is the typed static pair — `Postgres.publication(name, { cluster, database, target }, child)` and `Postgres.subscription(name, { cluster, database, publication, external }, child)` construct the CNPG `Publication`/`Subscription` CRs for the multi-region or tenant-migration estate; the pair is dormant capability with a typed spelling, so a replication topology is rows at the composition site, never a tier rewrite.
- Law: the profile subset proves against the matrix and the matrix alone owns the demand pairs — every `profile.extensions` name resolves through `Array.findFirst` over `Pg.rows` on the `extension` column, then every resolved row proves its dependency edges against `Pg.demands`, the relation-carrying rows the owning matrix already exports; a second deploy-side demand table evaluates a different closure from the runtime probe's and drifts on the first edge either side adds.
- Law: pooling proves against the primitives the app declares — `_VOIDS` names what each PgBouncer mode kills across the pooled bind (`transaction` ends the session `advisory` locks and `channel` listeners survive on, `statement` ends the transaction `skipLocked` claims inside), the admission refuses the intersection with `profile.data.primitives` naming both mode and casualties, and the realized mode publishes so the runtime capability rail proves the roster it cannot see from inside a connection.
- Law: the managed role owns its database; generated grants and default privileges remain framework artifacts on the convergence runner.
- Law: CNPG owns physical slot survival; typed `Publication` and `Subscription` CRs own logical topology.
- Law: security labels remain generated framework artifacts; each carrier applies its native projection through convergence.
- Law: replace-on-change fields are create-time constants — `template`, `encoding`, and locale rows on the Database CR never appear as mutable knobs; changing them is a new database by construction, and `protect` guards the cluster above it.
- Growth: a second app database is another `Postgres`; a dependency edge is one `Pg.demands` row at the matrix owner; a data tier is one `_TIERS` row; a pooling mode is one `_VOIDS` row.
- Boundary: recovery bootstraps a new cluster; the ordinary database and convergence folds then materialize, hydrate, prove, and publish it; the primitive roster and its grant semantics are the matrix owner's.
- Packages: `@pulumi/kubernetes`; `../crds/cnpg`; `effect` (`Array`, `Effect`, `Option`); `@rasm/ts/data` (`Pg`).

```typescript signature
const _granted = (names: ReadonlyArray<string>): Effect.Effect<ReadonlyArray<(typeof Pg.rows)[number]>, DataRefused> =>
  Effect.forEach(names, (name) =>
    Effect.mapError(
      Array.findFirst(Pg.rows, (row) => row.extension === name),
      () => new DataRefused({ axis: "extensions", detail: `<unknown-extension:${name}>` }),
    )).pipe(
      Effect.tap((rows) =>
        Effect.forEach(rows, (row) =>
          // One equality over the relation replaces a two-arm ladder: a `requires` row holds when the demanded
          // grant is present and an `excludes` row when it is absent, so a corner the matrix later declares is
          // graded here the day it lands rather than read as an implication that silently always passes.
          Effect.forEach(Pg.demands, (demand) =>
            !Array.contains(row.flags, demand.flag)
              || (demand.relation === "requires") === Array.some(rows, (peer) => Array.contains(peer.capabilities, demand.grant))
              ? Effect.void
              : Effect.fail(new DataRefused({
                  axis: "extensions",
                  detail: `<${demand.relation}-dependency:${row.extension}:${demand.grant}>`,
                })), { discard: true }), { discard: true })),
    )

const _pooled = (data: StackSpec.Data): Effect.Effect<void, DataRefused> => {
  const voided = Array.filter(_VOIDS[data.pooling], (primitive) => Array.contains(data.primitives, primitive))
  return Array.isNonEmptyReadonlyArray(voided)
    ? Effect.fail(new DataRefused({ axis: "pooling", detail: `${data.pooling}:${voided.join(",")}` }))
    : Effect.void
}

const _recoverable = (name: string, args: Postgres.Args): Effect.Effect<void, DataRefused> =>
  Effect.forEach(_scopes(name, args.spec), (scope) => {
    const recovery = args.recovery(scope)
    return recovery._tag === "archive" && recovery.server === scope
      ? Effect.fail(new DataRefused({ axis: "recovery", detail: `<recovery-source-conflicts:${scope}>` }))
      : Effect.void
  }, { discard: true })

type _Finalize = {
  readonly owner: Postgres
  readonly cluster: string
  readonly args: Postgres.Args
  readonly granted: ReadonlyArray<(typeof Pg.rows)[number]>
  readonly custody: _Custody
  readonly analystRole: string
  readonly direct: pulumi.Output<string>
  readonly child: pulumi.CustomResourceOptions
}

// CNPG's `version` is an exact demand while a matrix `floor` is a lower bound, so the DDL row asks
// for presence and the startup capability probe alone refuses an image sitting below the floor.
const _extensions = (granted: ReadonlyArray<(typeof Pg.rows)[number]>): ReadonlyArray<{ name: string; ensure: string }> =>
  Array.map(granted, (row) => ({ name: row.extension, ensure: "present" }))

const _target = (
  name: string,
  database: string,
  direct: pulumi.Output<string>,
  ready: cnpg.postgresql.v1.Database,
  custody: _Custody,
  ctx: _Finalize,
): Postgres.Target => ({
  name,
  database,
  direct,
  ready,
  env: [
    { name: "PGHOST", value: direct },
    { name: "PGPORT", value: "5432" },
    { name: "PGDATABASE", value: database },
    // Convergence authors the relations the application then reads, so its runner binds as this
    // scope's managed owner; a superuser run leaves every relation owned off-role.
    { name: "PGUSER", value: ctx.owner.role },
    {
      name: "PGPASSWORD",
      valueFrom: { secretKeyRef: { name: custody.app.metadata.name, key: "password" } },
    },
  ],
})

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

const _TIERS: {
  readonly [K in StackSpec.PgTier]: (ctx: _Finalize) => ReadonlyArray<Postgres.Target>
} = {
  "shared-rls": () => [],
  "db-per-tenant": (ctx) =>
    Array.map(ctx.args.spec.tenants, (tenant) => {
      const name = `${ctx.owner.database}-${tenant}`
      const database = `${ctx.owner.database}_${tenant}`
      return _target(name, database, ctx.direct, _database(name, database, ctx, ctx.cluster), ctx.custody, ctx)
    }),
  "cluster-per-tenant": (ctx) =>
    Array.map(ctx.args.spec.tenants, (tenant) => {
      const cluster = `${ctx.cluster}-${tenant}`
      const name = `${ctx.owner.database}-${tenant}`
      const custody = _custody(cluster, ctx.args, ctx.owner.role, ctx.analystRole, ctx.child)
      const dedicated = _cluster(cluster, {
        args: ctx.args,
        granted: ctx.granted,
        custody,
        role: ctx.owner.role,
        analystRole: ctx.analystRole,
      }, ctx.child)
      const direct = pulumi.interpolate`${cluster}-rw.${ctx.args.namespace}.svc`
      const ready = _database(name, ctx.owner.database, ctx, cluster, { ...ctx.child, dependsOn: [dedicated] })
      return _target(name, ctx.owner.database, direct, ready, custody, ctx)
    }),
}

const _finalized = (ctx: _Finalize): Postgres.Targets => {
  const database = _database(ctx.owner.database, ctx.owner.database, ctx, ctx.cluster)
  return [
    _target(ctx.owner.database, ctx.owner.database, ctx.direct, database, ctx.custody, ctx),
    ..._TIERS[ctx.args.spec.pgTier](ctx),
  ]
}

declare namespace Postgres {
  type Replication = {
    readonly cluster: string
    readonly database: string
    readonly namespace: pulumi.Input<string>
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { DataRefused, Nats, ObjectStore, Postgres }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

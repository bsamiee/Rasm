# [IAC_PROVIDER]

Provider dispatch and the service surface as ONE owner keyed by one union: the `_map` equivalence table and the `_ARMS` handler record both key on `StackSpec.Arm` — the map is the audit surface capability reads, the record is the construction it describes, and review pressure holds them adjacent. Rows are capabilities, columns are arms, cells name the exact resource-family spelling, and a hole is honest absence read as `Option`. Adding a cloud is one record row and one map column; finalizing one is a `StackSpec` value, never a lib edit. `iac/src/program/provider.ts` is the module.

Each arm is a total function from spec, host material, and pins to a `PulumiFn`: `_proven`, `_coord`, and `_staged` prove every spec-derived coordinate as typed `DeployFault`s before the program body is entered. `_estate` is the single k8s-estate builder — the selfhosted arm feeds it a `Bootstrap` kubeconfig provider, the aws `cluster` row an `eks.Cluster.kubeconfigJson` provider — so promoting a cloud swaps one provider seam. Every estate builder rides `Effect` over `Dispatch.EstateFault`, and `_bodied` runs that rail at the `PulumiFn` seam where the engine's error contract takes over.

## [01]-[INDEX]

- [02]-[EQUIVALENCE_MAP]: the capability-by-arm table and its `Option`-lifted projections; `Dispatch`.
- [03]-[ARM_CONTRACT]: the material read, pins, the coordinate proofs, the exhaustive record; `Dispatch`.
- [04]-[CLUSTER_BOOTSTRAP]: first boot, staging, install, connection hardening, kubeconfig egress; `Bootstrap`.
- [05]-[ARM_PROGRAMS]: the shared k8s estate builder and the five arm bodies; `Dispatch`.

## [02]-[EQUIVALENCE_MAP]

[EQUIVALENCE_MAP]:
- Owner: the interior `_capabilities` key tuple anchoring row order, the `_map` table carrying per-arm cells as exact-optional keys (a hole is an omitted key, never a sentinel), and the two projections riding the exported owner: `cell(capability, arm)` lifts the unproven cell read to `Option`, `column(arm)` folds an arm's realized subset in row order. Reads ride `_cells`, the table widened to `Dispatch.Cell` rows — a declared-key access on the literal union demands the key on every row, so the bracket read is index trust lifted at the seam while `_map` keeps its literals.
- Law: cells are family spellings, not mechanics — a cell names only the resource classes or owning row `_ARMS` constructs for that arm and posture; capability audits read the map, and an absent construction is an absent cell.
- Law: the object row admits only conditional-put-conforming engines — the self-host cells name the maintained MinIO continuation and Ceph RGW, the managed cells name S3 and R2; the CRDT-metadata engine that cannot honor `If-None-Match: *` has no cell anywhere, because the data plane's write-once identity algebra is non-negotiable and the refusal is `data`'s engine table read as deployment law. The `gcp` cell is a DISTRIBUTION cell wearing the object row's key — `_FOLDERS.gcp` converges the static frontend onto that bucket and nothing on this estate writes objects through it under the identity algebra — because the GCS interoperability API answers create-if-absent through its own generation precondition rather than the header the object client emits, and until `data`'s engine table carries the row that verdict is not this plane's to grant.
- Law: the data row admits only engines whose extension roster the arm can REALIZE — the object row's twin, and the reason every self-host and escalated cell is a CNPG cluster: the derivation controls the image, so the profile's extension subset loads. A managed cell states the subset it carries or narrows, because a coverage claim the arm cannot honor is the same defect as an unconforming object engine wearing a cell.
- Law: Doppler is canonical wherever an arm constructs `Secrets`; a cloud secret manager is reachable only as a mirror, so no arm grows a second secret source of truth.
- Law: a column is a realized inventory, not a promotion promise; dormant provider SDK families stay absent until their arm constructs and returns the capability. Three admitted engines have no cell and the reason is the inventory, never silence — `tigris` conforms on the object row and no arm this estate deploys offers it; `d1` is an admitted data engine on an arm carrying no workload cell, so a cell there would publish server coordinates no process on that column dials; `clickhouse` IS realized, as the analytics residence `operate/observe.md` installs off `observe.analytics`, which is a telemetry residence and not the app's transactional store.
- Entry: `Dispatch.column(spec.target)` inside an arm; `Dispatch.cell("data", "aws")` for a point read.
- Growth: a new capability is one `_capabilities` entry and one `_map` row; a new arm is one cell per realized row under the new column key.
- Boundary: kube-row mechanics are `kube/*`; the object/data engine choices are `StackSpec.profile` values; the tenant row's mechanics are `kube/tenant.md`; the in-cluster reconcile row's mechanics are `operate/policy.md`; cross-stack output reads ride `StackReference` inside the tenant seam `kube/tenant.md` owns.
- Packages: `effect` (`Array`, `Option`, `Record`); `./spec.ts` (`StackSpec`).

```typescript
import { Array, Option, Order, Record } from "effect"
import type { StackSpec } from "./spec.ts"

const _capabilities = [
  "bootstrap", "workload", "data", "object", "fanout", "cert", "dns",
  "ingress", "secret", "registry", "network", "identity", "observe",
  "tenant", "distribution",
] as const

const _map = {
  bootstrap: {
    "selfhosted-k8s": "cloudinit.getConfigOutput first boot + command.remote.Command + CopyToRemote",
    "selfhosted-docker": "cloudinit.getConfigOutput first boot + command.remote.Command",
  },
  workload: {
    "selfhosted-k8s": "kubernetes.apps/v1.Deployment + core/v1.Service",
    "selfhosted-docker": "docker.Container",
    aws: "awsx.ecs.FargateService | eks.Cluster + eks.ManagedNodeGroup (compute: cluster)",
  },
  data: {
    "selfhosted-k8s": "cnpg Cluster CR + Database CR + Converge Jobs",
    "selfhosted-docker": "docker.Container(postgres) + postgresql.Database/Role/Grant/Extension",
    aws: "cnpg Cluster CR + Database CR + Converge Jobs (compute: cluster)",
    gcp: "gcp.sql.DatabaseInstance + gcp.sql.Database + gcp.sql.User (core extensions only)",
  },
  object: {
    "selfhosted-k8s": "helm minio-continuation | ceph-rgw",
    "selfhosted-docker": "docker.Container(minio-continuation)",
    aws: "aws.s3.BucketV2 (compute: serverless) | helm minio-continuation | ceph-rgw (compute: cluster)",
    gcp: "gcp.storage.Bucket (distribution origin; no conditional-put row)",
    cloudflare: "cloudflare.R2Bucket",
  },
  fanout: {
    "selfhosted-k8s": "helm nats (jetstream + websocket)",
    "selfhosted-docker": "docker.Container(nats)",
    aws: "helm nats (compute: cluster)",
  },
  cert: {
    "selfhosted-k8s": "tls chain -> core/v1.Secret(kubernetes.io/tls)",
    "selfhosted-docker": "acme.Registration + acme.Certificate (CSR over tls chain)",
    aws: "tls chain -> core/v1.Secret(kubernetes.io/tls) (compute: cluster)",
  },
  dns: {
    "selfhosted-k8s": "cloudflare.DnsRecord",
    "selfhosted-docker": "cloudflare.DnsRecord",
    aws: "cloudflare.DnsRecord (compute: cluster)",
    cloudflare: "cloudflare.DnsRecord",
  },
  ingress: {
    "selfhosted-k8s": "kubernetes.networking/v1.Ingress | cloudflare.ZeroTrustTunnelCloudflared",
    "selfhosted-docker": "docker.Container ports",
    aws: "awsx.lb.ApplicationLoadBalancer (compute: serverless) | Traffic (compute: cluster)",
    cloudflare: "cloudflare.PagesProject + cloudflare.DnsRecord",
  },
  secret: {
    "selfhosted-k8s": "doppler canonical",
    "selfhosted-docker": "doppler canonical",
    aws: "doppler canonical (compute: cluster)",
    gcp: "doppler canonical",
    cloudflare: "doppler canonical",
  },
  registry: {
    "selfhosted-docker": "docker-build.Image",
    aws: "awsx.ecr.Repository + awsx.ecr.Image (compute: serverless)",
  },
  network: {
    "selfhosted-k8s": "kubernetes.networking/v1.NetworkPolicy",
    "selfhosted-docker": "docker.Network + docker.Volume",
    aws: "awsx.ec2.Vpc",
  },
  identity: {
    "selfhosted-k8s": "core/v1.ServiceAccount + rbac/v1.Role + rbac/v1.RoleBinding",
    aws: "eks.Cluster(createOidcProvider) (compute: cluster)",
  },
  observe: {
    "selfhosted-k8s": "store-row backend charts + otel collector + grafana apply",
    "selfhosted-docker": "collector container + grafana apply",
    aws: "store-row backend charts + otel collector + grafana apply (compute: cluster)",
  },
  tenant: {
    "selfhosted-k8s": "capsule Tenant CR | vcluster chart (crd2pulumi typed)",
    aws: "kube tenant tier over the EKS estate (compute: cluster)",
  },
  distribution: {
    aws: "synced-folder.S3BucketFolder over aws.s3.BucketV2 + cloudfront front rendering Source.edge (compute: serverless)",
    gcp: "synced-folder.GoogleCloudFolder over gcp.storage.Bucket + compute.URLMap front rendering Source.edge",
    cloudflare: "cloudflare.PagesProject origin (uploads out of graph) + Ruleset rendering Source.edge",
  },
} as const

declare namespace Dispatch {
  type Capability = (typeof _capabilities)[number]
  type Column = ReadonlyArray<readonly [Dispatch.Capability, string]>
  type Cell = Partial<Record.ReadonlyRecord<StackSpec.Arm, string>>
  type _Rows<T extends Record.ReadonlyRecord<Capability, Cell> = typeof _map> = T
  type _Keys<K extends keyof typeof _map = Capability> = K
}

const _cells: Record.ReadonlyRecord<Dispatch.Capability, Dispatch.Cell> = _map
```

## [03]-[ARM_CONTRACT]

[ARM_CONTRACT]:
- Owner: the arm signature and the record law — `material` is the one deploy-host Config read the arms share (`IAC_SSH_KEY` as an optional `Redacted`, resolved under `doppler run`), `program(spec, material, pins)` is the generic indexed call over `_ARMS`, and the record's mapped annotation `{ readonly [K in StackSpec.Arm]: Dispatch.Arm }` is the exhaustiveness proof — a `StackSpec.arms` entry with no row fails compilation at the record.
- Law: arms prove, never assume — `_coord` lifts any spec `Option` onto the rail minting an `input` fault naming the coordinate, `_proven` zips connection and key, and `_staged` proves the entire traffic-edge coordinate set (domain, zone, and the exposure row's own demand: the connection host under `direct`, the account under `tunnel`; `internal` demands nothing and stages the app edgeless, so a worker-only workload deploys with no domain coordinate at all) into one `Option`-carried `Traffic.Edge` tagged case; no arm body or tier constructor ever meets an unproven `Option`, and a construction-time `RunError` for a spec-derivable value is the named defect this proof family deletes.
- Law: a provider vocabulary is a roster the arm ADMITS against, not a type it trusts — `_vocab` lifts the coordinate and its roster in one step, so the aws arm's `region` refuses a value the provider never published on the same `input` fault every other coordinate mints, and the roster is the generated const itself, so it widens with the installed tree. Only the arm-dependent case earns this proof — a coordinate whose governing roster is the same on every arm closes at `spec.md` as an admission alphabet (`profile.capacity.instanceType` is that shape) and never reaches this family, while `region` names one roster on aws and an unpublished one on gcp and therefore cannot close at the schema at all.
- Law: `Dispatch.Pins` carries deploy-time facts absent from `StackSpec`. Backend material is optional on the common shape; Kubernetes projects it through immutable ConfigMaps, Docker uploads the exact document and pointer, and Fargate materializes its private S3 publication through the pinned AWS CLI image into one ephemeral task volume. Site decoder digests and managed-cell coverage remain instance facts consumed by their owning arms.
- Law: one provider seam per arm — the arm constructs its provider (kubeconfig-bound `k8s.Provider`, `ssh://` `docker.Provider`, credentialed cloud provider) exactly once and threads it through tier options; per-resource providers are the named defect, and the credential arrives from `Secrets.read` in-graph or the ambient `doppler run` env, never a literal.
- Law: the `PulumiFn` body is the deploy plane's program seam — a promise-returning composition of tier constructors bound to consts and one returned outputs record; the platform owns that shape, and everything the arm computes before entering it stays on the rail.
- Entry: `Effect.flatMap(Dispatch.material, (material) => Dispatch.program(spec, material, pins))` then `Automation.stack(spec, program)`.
- Growth: one record row and one map column per cloud; a new shared deploy-time fact is one `Pins` field, a new shared secret fact is one `material` field; a new spec coordinate a tier requires is one `_coord` call in its arm's proof.
- Boundary: the run and receipt are `automation.md`'s; outputs keys are `spec.md`'s contract.
- Law: `Dispatch.EstateFault` is the program body's whole failure vocabulary: tier admissions plus `DeployFault`. A new admitting tier widens one type alias and every arm body inherits it.
- Packages: `effect` (`Array`, `Config`, `Effect`, `Option`, `Record`, `Redacted`); `./spec.ts` (`StackSpec`); `./source.ts` (`Source.AssetInput`, `Source.Distribution`); `./automation.ts` (`DeployFault`); `../kube/data.ts` (`DataRefused`, `Postgres`); `../kube/traffic.ts` (`Traffic.Edge`); `../operate/converge.ts` (`Converge`, `ConvergeRefused`); `../operate/observe.ts` (`Lgtm.Versions`); `@rasm/data` (`Backend`).

```typescript
import type { PulumiFn } from "@pulumi/pulumi/automation"
import type { Backend } from "@rasm/data"
import { Array, Config, Effect, Encoding, Option, Record, Redacted } from "effect"
import type { Board, Reliability } from "@rasm/core"
import type { DataRefused, Postgres } from "../kube/data.ts"
import { Traffic } from "../kube/traffic.ts"
import type { Converge, ConvergeRefused } from "../operate/converge.ts"
import type { Lgtm } from "../operate/observe.ts"
import { DeployFault } from "./automation.ts"
import type { StackSpec } from "./spec.ts"

declare namespace Dispatch {
  type Material = { readonly sshKey: Option.Option<Redacted.Redacted<string>> }
  type Arm = (spec: StackSpec, material: Material, pins: Pins) => Effect.Effect<PulumiFn, DeployFault>
  type App = { readonly image: string; readonly edge: Option.Option<Traffic.Edge> }
  type Planes = Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, pulumi.Input<string | number>>>
  type EstateFault = ConvergeRefused | DataRefused | DeployFault
  type ManagedEngine = (typeof _ENGINES)[number]
  type Managed = {
    readonly engine: ManagedEngine
    readonly version: string
    readonly tier: string
    readonly extensions: ReadonlyArray<string>
  }
  type Pins = {
    readonly install: string
    readonly firstBoot: ReadonlyArray<{ readonly content: string; readonly contentType?: string; readonly filename?: string; readonly mergeType?: string }>
    readonly facts: ReadonlyArray<string>
    readonly pgImage: string
    readonly operator: string
    readonly barman: string
    readonly object: string
    readonly objectImage: string
    readonly nats: string
    readonly natsImage: string
    readonly observe: Lgtm.Versions & { readonly dev: string }
    readonly dns: string
    readonly cloudflared: string
    readonly capsule: string
    readonly vcluster: string
    readonly acme?: {
      readonly email: string
      readonly challenge: { readonly provider: string; readonly config: Record<string, string> }
    }
    readonly port: number
    readonly context: string
    readonly registry?: { readonly address: string; readonly user: string }
    readonly managedData: Managed
    readonly backend?: {
      readonly projection: Backend.Projection
      readonly runner: Converge.Runner
      readonly materializer: pulumi.Input<string>
      readonly publication: string
      readonly recovery: (target: string) => Postgres.Recovery
    }
    readonly site?: {
      readonly path: string
      readonly assets: ReadonlyArray<Source.AssetInput>
      readonly decoders?: Source.Distribution["decoders"]
    }
    readonly boards: ReadonlyArray<typeof Board.DashboardModel.Encoded>
    readonly alerts: ReadonlyArray<Reliability.Alert.Spec>
    readonly objectives: ReadonlyArray<Reliability.Objective>
    readonly contacts: Partial<Record<"page" | "ticket", {
      readonly webhook: string
      readonly quiet?: ReadonlyArray<{ readonly days: ReadonlyArray<string>; readonly start: string; readonly end: string }>
    }>>
  }
}

const _ENGINES = [
  "pg", "pglite", "sqliteServer", "sqliteWasm", "libsql", "d1", "duckdbNode", "duckdbWasm", "clickhouse",
] as const

const _material = Config.unwrap({
  sshKey: Config.option(Config.redacted("IAC_SSH_KEY")),
})

const _input = (spec: StackSpec, detail: string): DeployFault =>
  new DeployFault({ case: { reason: "input", stack: spec.name, detail } })

const _coord = <A>(spec: StackSpec, held: Option.Option<A>, name: string): Effect.Effect<A, DeployFault> =>
  Effect.mapError(held, () => _input(spec, `<missing-${name}>`))

const _vocab = <A extends string>(
  spec: StackSpec,
  held: Option.Option<string>,
  name: string,
  roster: Record.ReadonlyRecord<string, A>,
): Effect.Effect<A, DeployFault> =>
  Effect.flatMap(_coord(spec, held, name), (value) =>
    Option.match(Array.findFirst(Object.values(roster), (member) => member === value), {
      onNone: () => Effect.fail(_input(spec, `<unpublished-${name}:${value}>`)),
      onSome: Effect.succeed,
    }))

const _proven = (spec: StackSpec, material: Dispatch.Material): Effect.Effect<{
  readonly connection: StackSpec.Connection
  readonly key: Redacted.Redacted<string>
}, DeployFault> =>
  Option.zipWith(spec.connection, material.sshKey, (connection, key) => ({ connection, key })).pipe(
    Effect.mapError(() => _input(spec, "<missing-connection-or-key>")),
  )

const _staged = (spec: StackSpec): Effect.Effect<Option.Option<Dispatch.App>, DeployFault> =>
  Option.match(spec.image, {
    onNone: () => Effect.succeed(Option.none()),
    onSome: (image) =>
      spec.profile.exposure === "internal"
        ? Effect.succeed(Option.some({ image, edge: Option.none() }))
        : Effect.all([_coord(spec, spec.domain, "domain"), _coord(spec, spec.zone, "zone")]).pipe(
            Effect.flatMap(([domain, zone]) =>
              spec.profile.exposure === "direct"
                ? Effect.map(_coord(spec, spec.connection, "connection"), (connection) =>
                    Traffic.Edge.Direct({ domain, zone, address: connection.host }))
                : Effect.map(_coord(spec, spec.account, "account"), (account) =>
                    Traffic.Edge.Tunnel({ domain, zone, account }))),
            Effect.map((edge) => Option.some({ image, edge: Option.some(edge) })),
          ),
  })

const _managed = (spec: StackSpec, pins: Dispatch.Pins): Effect.Effect<Dispatch.Managed, DeployFault> => {
  const unrealized = Array.difference(spec.profile.extensions, pins.managedData.extensions)
  return pins.managedData.engine !== "pg"
    ? Effect.fail(_input(spec, `<managed-engine-unrealized:${pins.managedData.engine}>`))
    : Array.isEmptyReadonlyArray(unrealized)
      ? Effect.succeed(pins.managedData)
      : Effect.fail(_input(spec, `<managed-extensions-unrealized:${unrealized.join(",")}>`))
}

const _edged = (spec: StackSpec): Effect.Effect<Option.Option<{ readonly domain: string; readonly zone: string }>, DeployFault> =>
  spec.profile.exposure === "internal"
    ? Effect.succeed(Option.none())
    : spec.profile.exposure === "tunnel"
      ? Effect.fail(_input(spec, "<unsupported-exposure:tunnel>"))
      : Option.match(spec.domain, {
          onNone: () => Effect.succeed(Option.none()),
          onSome: (domain) => Effect.map(_coord(spec, spec.zone, "zone"), (zone) => Option.some({ domain, zone })),
        })
```

## [04]-[CLUSTER_BOOTSTRAP]

[CLUSTER_BOOTSTRAP]:
- Owner: `Bootstrap`, the tier that turns owned metal into a cluster — `Bootstrap.firstBoot(parts, encoding?)` renders the multi-part MIME user-data a host-provisioning resource consumes as its pre-SSH product, staged assets ride `remote.CopyToRemote` (rendered install artifacts as `Asset`/`Archive` values, never checked-in paths), the control plane installs through one `remote.Command` whose CRUD slots own install (`create`) and teardown (`delete`), and `kubeconfig` egresses as the secret-tracked stdout the `@pulumi/kubernetes` `Provider` binds.
- Law: cloud-init owns first boot, `command` owns steady state — `firstBoot` composes `cloudinit.getConfigOutput` over ordered typed parts (one `text/cloud-config` declarative part with `text/x-shellscript` steps, `mergeType` on composed cloud-config parts), the rendered body lays the SSH surface (users, keys, packages, daemon) the `Connection` coordinates then reach, and part content carries coordinates and installers only — user-data is metadata-endpoint-readable, so credential material inside a part is the named defect; a first-boot step re-run over SSH, or an SSH step folded into user-data, is the same defect in two directions.
- Law: the connection is coordinates with injected material, hardened — `StackSpec.Connection` supplies host/user/port, the PEM key arrives as a `pulumi.Input<string>` already secret-tracked from `Dispatch.material`, `hostKey` pins the host's public key so a MITM re-key fails the dial instead of silently trusting, `proxy` is the bastion hop as one `ProxyConnectionArgs` row on the same connection inheriting the injected key unless the row carries its own (a bastion coordinate with no dial credential is an unreachable hop, not a hardening), `perDialTimeout`/`dialErrorLimit` bound the dial budget as data, and `logging: "none"` gates every credential-bearing step so captured output never echoes key material.
- Law: re-run is trigger-driven and fact-aware — the `triggers` list carries the spec `epoch`, the staged-asset references, and the `local.runOutput` host facts (`facts` rows: kernel version, an existing k3s token, a daemon fingerprint — each an unconditional deploy-host read threading the graph), so a bootstrap re-runs exactly when its real inputs change and never by blind epoch-only replacement.
- Law: the takeover boundary is absolute — after the kubeconfig exists, every workload is a typed `@pulumi/kubernetes` resource; a shell command that duplicates a typed provider resource is the named defect, and `command` survives only for bare-metal mutation no typed provider owns; an unconditional host fact is `local.runOutput` when it threads the graph, `local.run` for an eager read inside the program body.
- Entry: `new Bootstrap("plane", { connection, key, epoch, install, facts, hostKey, proxy }, opts)` inside the selfhosted arms; `bootstrap.kubeconfig` feeds `new k8s.Provider(...)`; `Bootstrap.firstBoot(pins.firstBoot)` wherever a host-provisioning resource takes user-data.
- Growth: a new host mutation is one `remote.Command` row inside this tier with its own triggers; a new staged artifact is one `assets` entry; a new re-run discriminant is one `facts` row.
- Boundary: the install script and first-boot part content are app data handed in as pins; the k8s provider construction is the arm body's; VPS provisioning itself (the resource consuming `firstBoot`) is the owning cloud arm's when an estate provisions rather than adopts its metal.
- Packages: `@pulumi/command` (`remote.Command`, `remote.CopyToRemote`, `local.run`, `local.runOutput`, `types.input.remote.ProxyConnectionArgs`); `@pulumi/cloudinit` (`getConfigOutput`); `@pulumi/pulumi` (`Output`, `secret`, `asset`); `./spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as cloudinit from "@pulumi/cloudinit"
import * as command from "@pulumi/command"
import * as pulumi from "@pulumi/pulumi"
import { Tier } from "./spec.ts"

declare namespace Bootstrap {
  type Part = {
    readonly content: pulumi.Input<string>
    readonly contentType?: string
    readonly filename?: string
    readonly mergeType?: string
  }
  type Args = {
    readonly connection: StackSpec.Connection
    readonly key: pulumi.Input<string>
    readonly epoch: string
    readonly install: pulumi.Input<string>
    readonly remove?: pulumi.Input<string>
    readonly facts?: ReadonlyArray<string>
    readonly hostKey?: pulumi.Input<string>
    readonly proxy?: command.types.input.remote.ProxyConnectionArgs
    readonly dial?: { readonly perDialTimeout: number; readonly dialErrorLimit: number }
    readonly assets?: ReadonlyArray<{
      readonly source: pulumi.asset.Asset | pulumi.asset.Archive
      readonly remotePath: string
    }>
  }
}

class Bootstrap extends Tier {
  static readonly firstBoot = (
    parts: ReadonlyArray<Bootstrap.Part>,
    encoding?: { readonly gzip: boolean; readonly base64: boolean },
  ): pulumi.Output<string> =>
    cloudinit.getConfigOutput({
      gzip: encoding?.gzip ?? false,
      base64Encode: encoding?.base64 ?? false,
      parts: [...parts],
    }).rendered
  readonly kubeconfig: pulumi.Output<string>
  constructor(name: string, args: Bootstrap.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Bootstrap", name, opts)
    const connection = {
      host: args.connection.host,
      user: args.connection.user,
      port: args.connection.port,
      privateKey: args.key,
      perDialTimeout: args.dial?.perDialTimeout ?? 15,
      dialErrorLimit: args.dial?.dialErrorLimit ?? 10,
      ...(args.hostKey !== undefined && { hostKey: args.hostKey }),
      ...(args.proxy !== undefined && { proxy: { privateKey: args.key, ...args.proxy } }),
    }
    const facts = (args.facts ?? []).map((probe, rank) =>
      command.local.runOutput({ command: probe }).stdout.apply((fact) => `${rank}:${fact}`))
    const staged = (args.assets ?? []).map((asset, rank) =>
      new command.remote.CopyToRemote(`${name}-asset-${rank}`, {
        connection,
        source: asset.source,
        remotePath: asset.remotePath,
        triggers: [args.epoch],
      }, this.child()))
    const plane = new command.remote.Command(`${name}-plane`, {
      connection,
      create: args.install,
      ...(args.remove !== undefined && { delete: args.remove }),
      triggers: [args.epoch, ...facts, ...staged.map((copy) => copy.remotePath)],
      logging: "none",
    }, this.child({ dependsOn: staged }))
    this.kubeconfig = pulumi.secret(plane.stdout)
    this.seal({ kubeconfig: this.kubeconfig })
  }
}
```

## [05]-[ARM_PROGRAMS]

[ARM_PROGRAMS]:
- Law: `_estate` is the one k8s-estate composition — namespace → `Secrets` over `_credentials` (the object and Grafana rows beside one `DB_ADMIN_PASSWORD`/`DB_PASSWORD`/`DB_ANALYST_PASSWORD` triple per scope `Postgres.scopes` enumerates, so the data tier's per-scope `auth` callback reads a mint no sibling cluster shares; `CLOUDFLARE_API_TOKEN` pre-exists on the app's config) → `ObjectStore` → `Nats` → `Postgres.admit` (the scope-keyed admin, app, and analyst reads) → `Lgtm` → `Boards` → `Tenants` when the tenancy mode escalates past `single` → the `RandomUuid7` deployment identity → `Workload.token` → optional `Workload` whose live-`Output` env pairs ride `StackOutputs.pairsOf` with the `pulumi.output(value).apply(String)` renderer — the same flatten the decoded getter rides — → and, only when the staged edge is realized (an `internal` exposure stands service-only), one `Certs.root` CA → `Traffic` over the workload service and its own published selector, with the issuance capability and the proven `Edge` case injected; graph-late material (`GRAFANA_AUTOMATION_TOKEN` from `Boards.automation`, the per-tenant `GRAFANA_VIEWER_*` keys from `Boards.viewers`, `MESH_CA_KEY` from the CA root) lands through `secrets.store` so it outlives the graph in the one canonical store; the object plane's coordinates thread into `Lgtm` so the mimir escalation binds one storage truth; it returns every realized `StackOutputs` plane, `deploy` included. Both k8s-plane sources feed it: the selfhosted arm's `Bootstrap.kubeconfig` and the aws arm's `eks.Cluster.kubeconfigJson`, so the entire tier roster is plane-agnostic by construction.
- Law: app images are one buildx product — the docker arm and any registry cell build through `docker-build.Image` with `push: true`, the immutable `ref`/`digest` pinning every runtime; `platforms` rows make the build multi-arch, `cacheFrom`/`cacheTo` registry rows reuse layers across runs, the push credential rides the `registries` row — `pins.registry` coordinates with the `REGISTRY_PASSWORD` fan-in read, so a `push: true` build carries its own auth instead of assuming an ambient login — and by-value `secrets` bind Doppler outputs so no build credential touches disk. One rust build stage runs `wasm-pack build` over the pinned `fastcdc` crate and the runtime stage copies the pkg, so the chunking artifact ships inside the image digest and no second artifact pipeline exists.
- Law: the docker arm realizes its whole column — `_grounded` (the one Bootstrap spelling both selfhosted arms share, folding the connection's `hostKey`/`bastion` hardening coordinates in) lays the daemon, the `ssh://` `docker.Provider` binds the proven connection's own `ssh` projection with `dependsOn` the daemon so the first `up` cannot race the install, and the machine estate mirrors `_estate` at container depth: one `Secrets` store with the generated credential entries, one `docker.Network` fence, the mount table minting one `docker.Volume` per store beside its path so mount spellings exist once, the postgres container loopback-published (`ip` bind + fence alias — the data plane exposes no public interface, in-fence consumers dial the alias, and the deployer's `postgresql.Provider` reaches it through one control-socket SSH forward riding the proven connection's own hardening coordinates, so `sslmode: "disable"` grades as a loopback fact rather than a cleartext credential hop) and finalized through that bridged provider at full logical depth (`Role`/`Database`/`Extension` rows from the profile's extension subset, the analyst read tier as one `Role` with its `pg_read_all_data` `GrantRole` membership, its schema `Grant`, and its `DefaultPrivileges` future-object ACL, and the `ReplicationSlot` logical seam — the read-back `operate/policy.md`'s `conform` correlates), the MinIO-continuation container whose filesystem bucket pre-creates in its own command, the NATS container configured through an `uploads` row (jetstream fsync-per-write, websocket listener — the same durability law the chart row states), the app container pinning the built digest, uploading the exact backend document and pointer at read-only file modes when armed, and injecting their runtime coordinates beside `DOPPLER_TOKEN` and the collector endpoint, the `Dev` all-in-one estate realizing the observe cell with `Boards` applied over its URL plane and the automation token landed through `secrets.store`, the `Direct`-edge `DnsRecord` and the ACME trusted pair landed through `secrets.store` when `pins.acme` arms the lane (`_edged` proves domain/zone and refuses the unsupported tunnel posture on the rail), and the `RandomUuid7` deploy identity — the arm returns every plane it realizes: `data`, `object`, `fanout`, `otlp`, `grafana`, `deploy`, and `ingress` under a proven edge.
- Law: the estate builder rides the rail its tiers admit on — `_estate` and every `_AWS` row return `Effect<Dispatch.Planes, Dispatch.EstateFault>`, so `Converge.admit`, `Postgres.admit`, and the `_coord` proofs compose in one `Effect.gen` and a refused axis surfaces as a typed value rather than a half-built graph; `_bodied` is the sole conversion, the `PulumiFn` seam where the engine's one in-band error contract takes the rail's failure, and a `throw` anywhere inside a tier or an arm body is the defect this owner deletes.
- Law: the aws arm dispatches its compute posture as data — `_AWS` is a handler record keyed by `StackSpec.Profile["compute"]`: the `serverless` row realizes VPC → ECR build → Fargate behind an ALB with the S3 object cell; the `cluster` row escalates to `eks.Cluster` (`eks.AuthenticationMode.Api` for access entries, `createOidcProvider: true` for IRSA, `skipDefaultNodeGroup: true`) with one `ManagedNodeGroup` sized from the profile's own `capacity` row — the node-group arg is `Input<string>`, so the closed instance-type roster is spent at the spec boundary and this row spells no capacity literal — binds `kubeconfigJson` into the arm's one `k8s.Provider` seam, and reuses `_estate` whole — the managed twin of `Bootstrap.kubeconfig`, one seam swap and zero tier edits.
- Law: an armed Fargate backend publishes the exact contract and generation under one generation-scoped prefix in a dedicated versioned, ownership-enforced, public-blocked bucket; the task role reads only that prefix, the pinned AWS CLI materializer copies it into one ephemeral volume, and the app container depends on its `SUCCESS` before mounting the volume read-only. The general object/site bucket never holds this control material, and no document bytes or alternate variable names cross through environment.
- Law: the gcp arm binds `credentials` from the `GCP_CREDENTIALS` fan-in read, realizes the versioned `gcp.storage.Bucket` object cell and the `gcp.sql.DatabaseInstance` + `Database` + `User` data cell, and returns only those planes with optional served assets; the cloudflare arm binds `apiToken` from the fan-in, realizes the `R2Bucket` object cell with its `R2BucketLifecycle` aging row and the `PagesProject` static origin, and lands the dns cell as the CNAME onto the project's `pages.dev` subdomain — each returns exactly the planes it realizes.
- Law: the distribution cells construct what the map advertises — the aws and gcp arms converge the built frontend through `Source.distribute` over their own object cells when `pins.site` arrives (the versioned `BucketV2` behind one `BucketVersioningV2` row on aws, the versioned bucket on gcp), each returning the caller-owned `served` slug-to-path record as an output plane; the cloudflare arm's static origin stays its `PagesProject` rows, whose build product uploads out of graph.
- Law: a converging arm publishes the viewer's decoder leaves beside the app's artifacts, because `pins.site.decoders` is a digest map both arms forward unchanged — the decoder distributions are estate-invariant and `_DECODERS` owns their leaf names, so no arm body spells a filename and the ui codec gate resolves `draco`, `ktx2`, and `meshopt` off the served plane rather than refusing `codec-absent` against addresses nothing published; a baked texture or environment set rides the same plane through the composing root's `Source.set` mints, so decoder leaves and set planes share one digest-directory law and one presence gate.
- Law: `enableServerSideApply` is armed for field-manager conflict detection over the estate's OWN objects, never to adopt foreign ones — every object `_estate` composes is authored by a tier on this branch, so the apply mode buys a named manager and a typed conflict when an operator edits out of band, while `<Kind>Patch` and `CustomResourcePatch` have no site anywhere: the SSA twins exist to mutate an object the program did not create, and an estate that installs its own operators and authors its own custom resources never holds one. A Patch row beside a tier that owns its object mints a second author for one field, which is precisely the conflict this mode exists to report.
- Law: an object cell states its access posture rather than inheriting one — the aws arm's single `BucketV2` serves the object plane and the site origin together, so one lockdown covers both: `BucketOwnershipControls` declares `BucketOwnerEnforced` (which disables ACLs and is why no canned-ACL coordinate exists on this arm at all), `BucketPublicAccessBlock` sets all four refusals, and one `BucketPolicy` grants `s3:GetObject` to the CloudFront service principal under a `SourceArn` condition naming this distribution — the grant without which a closed bucket behind an OAC origin answers 403 to every request. Enforced ownership is already a fresh bucket's default and the row declares it anyway, because an unstated default is a posture `operate/policy.md` cannot assert and a provider bump can move. Coverage closes on one cell: the `cluster` posture mints no bucket at all (its object cell is the in-cluster engine), the gcp cell carries the same posture as `uniformBucketLevelAccess`, and R2 exposes no ACL model to close.
- Law: every arm fronting served bytes folds the ONE header roster into its own dialect — the aws and gcp arms read `Source.distribute(...).edge`, the cloudflare arm reads `Source.edge` because it converges no folder, and `_EDGED` renders each row's `pattern`/`header`/`value` with no literal of its own: aws mints one `cloudfront.ResponseHeadersPolicy` per posture bound through the distribution's ordered behaviors over the site bucket's `OriginAccessControl` origin, gcp renders route rules carrying `headerAction.responseHeadersToAdds` on the `URLMap` fronting its CDN-enabled `BackendBucket` (`prefixMatch` for the bare-star pattern, `pathTemplateMatch` `/**.{ext}` for a suffix pattern — `regexMatch` is unspellable on the external managed scheme), and cloudflare rewrites through one `http_response_headers_transform` `Ruleset` over its Pages origin.
- Law: dialect match semantics decide the fold shape — aws and gcp bind the FIRST matching behavior per request, so `_postures` folds each pattern's covering rows into one header set and orders patterns narrow to wide (under the two-shape grammar a covered pattern always spells longer than its coverer); the cloudflare rules engine applies EVERY matching header-transform rule, so its arm renders the roster rows verbatim, one rule per row in roster order.
- Law: every arm funds the boards — the encoded models and alert specs enter as pins where the arm realizes an observe cell; an arm without the observe cell returns no `grafana` plane and drops nothing silently.
- Growth: realizing an arm is one realizer body or one `_AWS`-style posture row; a new cloud is one record row and one map column; a new tier admission fault is one member of `Dispatch.EstateFault`.
- Boundary: tier mechanics live on the tier pages; the credential vocabulary is `operate/secret.md`'s and the scope roster `kube/data.md`'s; the declared realizers' argument catalogues are the standing research items on the provider `.api` files.
- Packages: `@pulumi/kubernetes`, `@pulumi/eks`, `@pulumi/docker`, `@pulumi/docker-build`, `@pulumi/aws`, `@pulumi/awsx`, `@pulumi/gcp`, `@pulumi/cloudflare`, `@pulumi/random` (providers + composed classes); every folder tier.

```typescript
import * as aws from "@pulumi/aws"
import * as awsx from "@pulumi/awsx"
import * as cloudflare from "@pulumi/cloudflare"
import * as docker from "@pulumi/docker"
import * as dockerBuild from "@pulumi/docker-build"
import * as eks from "@pulumi/eks"
import * as gcp from "@pulumi/gcp"
import * as k8s from "@pulumi/kubernetes"
import * as postgresql from "@pulumi/postgresql"
import * as random from "@pulumi/random"
import { Nats, ObjectStore, Postgres } from "../kube/data.ts"
import { Tenants } from "../kube/tenant.ts"
import { Traffic } from "../kube/traffic.ts"
import { Workload } from "../kube/workload.ts"
import { Converge } from "../operate/converge.ts"
import { Boards, Dev, Lgtm } from "../operate/observe.ts"
import { Certs, Secrets } from "../operate/secret.ts"
import { Source } from "./source.ts"
import { StackOutputs } from "./spec.ts"

const _grounded = (
  name: string,
  spec: StackSpec,
  proven: { readonly connection: StackSpec.Connection; readonly key: Redacted.Redacted<string> },
  pins: Dispatch.Pins,
): Bootstrap =>
  new Bootstrap(name, {
    connection: proven.connection,
    key: pulumi.secret(Redacted.value(proven.key)),
    epoch: spec.epoch,
    install: pins.install,
    facts: pins.facts,
    ...Option.match(proven.connection.hostKey, { onNone: () => ({}), onSome: (hostKey) => ({ hostKey }) }),
    ...Option.match(proven.connection.bastion, { onNone: () => ({}), onSome: (proxy) => ({ proxy }) }),
  })

const _DB_KEYS = ["DB_ADMIN_PASSWORD", "DB_PASSWORD", "DB_ANALYST_PASSWORD"] as const

const _scoped = (key: string, scope: string): string => `${key}_${scope.toUpperCase().replaceAll("-", "_")}`

const _credentials = (spec: StackSpec, data: string): Record.ReadonlyRecord<string, Secrets.Entry> => ({
  OBJECT_USER: { generate: { special: false, length: 20 } },
  OBJECT_PASSWORD: { generate: {} },
  GRAFANA_PASSWORD: { generate: {} },
  ...Record.fromEntries(Array.flatMap(
    Postgres.scopes(data, spec),
    (scope) => Array.map(_DB_KEYS, (key) => [_scoped(key, scope), { generate: {} }] as const),
  )),
})

const _dockerBackend = (backend: Dispatch.Pins["backend"]): {
  readonly envs: ReadonlyArray<string>
  readonly uploads: ReadonlyArray<docker.types.input.ContainerUpload>
} => backend === undefined
  ? { envs: [], uploads: [] }
  : {
      envs: [
        `${StackOutputs.backend.root}=${backend.runner.contractRoot}`,
        `${StackOutputs.backend.pointer}=${backend.runner.contractRoot}/generation`,
      ],
      uploads: [
        {
          file: `${backend.runner.contractRoot}/contract.json`,
          contentBase64: Encoding.encodeBase64(backend.projection.files.contract),
          permissions: "0444",
        },
        {
          file: `${backend.runner.contractRoot}/generation`,
          content: backend.projection.contract.id,
          permissions: "0444",
        },
      ],
    }

const _estate = (
  spec: StackSpec,
  pins: Dispatch.Pins,
  provider: k8s.Provider,
  app: Option.Option<Dispatch.App>,
): Effect.Effect<Dispatch.Planes, Dispatch.EstateFault> =>
  Effect.gen(function* () {
    const backend = yield* _coord(spec, Option.fromNullable(pins.backend), "backend")
    const bound = { providers: [provider] }
    const ns = new k8s.core.v1.Namespace(spec.name, { metadata: { name: spec.name } }, { provider })
    const secrets = new Secrets("secrets", { spec, entries: _credentials(spec, "data") })
    const objects = new ObjectStore("objects", {
      spec,
      namespace: ns.metadata.name,
      version: pins.object,
      auth: { user: secrets.read("OBJECT_USER"), password: secrets.read("OBJECT_PASSWORD") },
    }, bound)
    const fanout = new Nats("fanout", { spec, namespace: ns.metadata.name, version: pins.nats }, bound)
    const data = yield* Postgres.admit("data", {
      spec,
      namespace: ns.metadata.name,
      image: pins.pgImage,
      operatorVersion: pins.operator,
      barmanVersion: pins.barman,
      objects,
      recovery: backend.recovery,
      auth: (scope) => ({
        admin: secrets.read(_scoped("DB_ADMIN_PASSWORD", scope)),
        app: secrets.read(_scoped("DB_PASSWORD", scope)),
        analyst: secrets.read(_scoped("DB_ANALYST_PASSWORD", scope)),
      }),
    }, bound)
    const identity = new random.RandomUuid7("deploy-id", { keepers: { epoch: spec.epoch } })
    const converge = (target: Postgres.Target): Effect.Effect<Converge, ConvergeRefused> =>
      Converge.admit(`backend-${target.name}`, {
        namespace: ns.metadata.name,
        profile: spec.profile,
        backend: backend.projection,
        runner: backend.runner,
        target,
        publication: {
          name: `${backend.publication}-${target.name}`,
          fence: identity.result,
        },
      }, bound)
    const [head, ...followers] = data.targets
    const primary = yield* converge(head)
    const convergences = [primary, ...yield* Effect.forEach(followers, converge)]
    const lgtm = new Lgtm("observe", {
      spec,
      namespace: ns.metadata.name,
      versions: pins.observe,
      auth: secrets.read("GRAFANA_PASSWORD"),
      data: { host: data.host, port: data.port, database: data.database, user: data.role, password: secrets.read(_scoped("DB_PASSWORD", "data")) },
      objects: { endpoint: objects.endpoint, bucket: objects.bucket },
      alerts: pins.alerts,
    }, bound)
    const boards = new Boards("boards", {
      spec,
      urls: lgtm.urls,
      targets: lgtm.targets,
      auth: secrets.read("GRAFANA_PASSWORD"),
      boards: pins.boards,
      alerts: pins.alerts,
      objectives: pins.objectives,
      contacts: pins.contacts,
      deploy: { id: identity.result },
    })
    secrets.store("GRAFANA_AUTOMATION_TOKEN", boards.automation)
    Array.map(Record.toEntries(boards.viewers), ([tenant, key]) =>
      secrets.store(`GRAFANA_VIEWER_${tenant.toUpperCase()}`, key))
    if (spec.profile.separation.mode !== "single") {
      new Tenants("tenants", { spec, versions: { capsule: pins.capsule, vcluster: pins.vcluster } }, bound)
    }
    const custody = Workload.token("doppler-token", { namespace: ns.metadata.name, token: secrets.token }, { provider })
    const outputs = {
      data: { host: data.host, port: data.port, database: data.database, role: data.role, pooling: data.pooling },
      object: { endpoint: objects.endpoint, bucket: objects.bucket },
      fanout: { origin: fanout.origin },
      otlp: { endpoint: lgtm.collectorEndpoint },
      grafana: { url: lgtm.urls.grafana },
      ...Option.match(lgtm.targets.analytics, {
        onNone: () => ({}),
        onSome: ({ residence }) => ({
          analytics: { residence, endpoint: lgtm.urls.query.residence, database: Lgtm.residence.catalog },
        }),
      }),
      deploy: { id: identity.result },
    }
    if (Option.isNone(app)) {
      return outputs
    }
    const { image, edge } = app.value
    const workload = new Workload("app", {
      spec,
      namespace: ns.metadata.name,
      image,
      role: { _tag: "service", port: pins.port },
      env: Workload.rows(custody, StackOutputs.pairsOf(outputs, (value) => pulumi.output(value).apply(String))),
      backend: {
        contract: primary.contract,
        pointer: primary.pointer,
        root: backend.runner.contractRoot,
      },
    }, pulumi.mergeOptions(bound, { dependsOn: convergences }))
    if (Option.isNone(edge)) {
      return outputs
    }
    const ca = Certs.root("mesh-ca")
    secrets.store("MESH_CA_KEY", ca.key.privateKeyPem)
    const service = yield* _coord(spec, workload.service, "workload-service")
    const traffic = new Traffic("traffic", {
      spec,
      namespace: ns.metadata.name,
      service: service.metadata.name,
      selector: workload.selector,
      port: pins.port,
      connector: pins.cloudflared,
      dnsVersion: pins.dns,
      issue: (hostname) => Certs.issue("edge", { ca, hostname }),
      apiToken: secrets.read("CLOUDFLARE_API_TOKEN"),
      edge: edge.value,
    }, bound)
    return { ...outputs, ingress: { hostname: traffic.hostname } }
  })

const _bodied = <A>(program: Effect.Effect<A, Dispatch.EstateFault>): Promise<A> => Effect.runPromise(program)

const _split = (pattern: string) => {
  const star = pattern.indexOf("*")
  return { prefix: pattern.slice(0, star), suffix: pattern.slice(star + 1) }
}

const _covers = (general: string, specific: string): boolean => {
  const wide = _split(general)
  const narrow = _split(specific)
  return narrow.prefix.startsWith(wide.prefix) && (wide.suffix === "" || wide.suffix === narrow.suffix)
}

const _postures = (rules: Source.Distributed["edge"]) =>
  Array.map(
    Array.sort(
      Array.dedupe(Array.map(rules, (rule) => rule.pattern)),
      Order.mapInput(Order.reverse(Order.number), (pattern: string) => pattern.length),
    ),
    (pattern) => ({
      pattern,
      headers: Array.map(
        Array.filter(rules, (rule) => _covers(rule.pattern, pattern)),
        (rule) => ({ header: rule.header, value: rule.value }),
      ),
    }),
  )

const _EDGED = {
  aws: (name: string, rules: Source.Distributed["edge"], site: { readonly bucket: aws.s3.BucketV2 }, opts: { readonly provider: aws.Provider }) => {
    const access = new aws.cloudfront.OriginAccessControl(`${name}-access`, {
      originAccessControlOriginType: "s3",
      signingBehavior: "always",
      signingProtocol: "sigv4",
    }, opts)
    const cache = new aws.cloudfront.CachePolicy(`${name}-cache`, {
      minTtl: 0,
      defaultTtl: 86400,
      maxTtl: 31536000,
      parametersInCacheKeyAndForwardedToOrigin: {
        cookiesConfig: { cookieBehavior: "none" },
        headersConfig: { headerBehavior: "none" },
        queryStringsConfig: { queryStringBehavior: "none" },
        enableAcceptEncodingBrotli: true,
        enableAcceptEncodingGzip: true,
      },
    }, opts)
    const _behavior = (cachePolicyId: pulumi.Input<string>) => ({
      targetOriginId: "site",
      cachePolicyId,
      viewerProtocolPolicy: "redirect-to-https",
      allowedMethods: ["GET", "HEAD", "OPTIONS"],
      cachedMethods: ["GET", "HEAD"],
    })
    return new aws.cloudfront.Distribution(`${name}-front`, {
      enabled: true,
      defaultRootObject: "index.html",
      origins: [{ originId: "site", domainName: site.bucket.bucketRegionalDomainName, originAccessControlId: access.id }],
      defaultCacheBehavior: _behavior(cache.id),
      orderedCacheBehaviors: Array.map(_postures(rules), (posture, rank) => ({
        ..._behavior(cache.id),
        pathPattern: posture.pattern,
        responseHeadersPolicyId: new aws.cloudfront.ResponseHeadersPolicy(`${name}-posture-${rank}`, {
          customHeadersConfig: {
            items: Array.map(posture.headers, (row) => ({ header: row.header, value: row.value, override: true })),
          },
        }, opts).id,
      })),
      restrictions: { geoRestriction: { restrictionType: "none" } },
      viewerCertificate: { cloudfrontDefaultCertificate: true },
    }, opts)
  },
  gcp: (name: string, rules: Source.Distributed["edge"], site: { readonly bucket: gcp.storage.Bucket }, opts: { readonly provider: gcp.Provider }) => {
    const backend = new gcp.compute.BackendBucket(`${name}-origin`, { bucketName: site.bucket.name, enableCdn: true }, opts)
    const routes = new gcp.compute.URLMap(`${name}-routes`, {
      defaultService: backend.id,
      hostRules: [{ hosts: ["*"], pathMatcher: "served" }],
      pathMatchers: [{
        name: "served",
        defaultService: backend.id,
        routeRules: Array.map(_postures(rules), (posture, rank) => {
          const shape = _split(posture.pattern)
          return {
            priority: rank + 1,
            service: backend.id,
            matchRules: [
              shape.suffix === ""
                ? { prefixMatch: `/${shape.prefix}` }
                : { pathTemplateMatch: `/${shape.prefix}**${shape.suffix}` },
            ],
            headerAction: {
              responseHeadersToAdds: Array.map(posture.headers, (row) => ({ headerName: row.header, headerValue: row.value, replace: true })),
            },
          }
        }),
      }],
    }, opts)
    const proxy = new gcp.compute.TargetHttpProxy(`${name}-proxy`, { urlMap: routes.id }, opts)
    return new gcp.compute.GlobalForwardingRule(`${name}-door`, {
      target: proxy.id,
      portRange: "80",
      loadBalancingScheme: "EXTERNAL_MANAGED",
    }, opts)
  },
  cloudflare: (name: string, rules: Source.Distributed["edge"], site: { readonly zone: pulumi.Input<string>; readonly app: string }, opts: { readonly provider: cloudflare.Provider }) =>
    new cloudflare.Ruleset(`${name}-headers`, {
      zoneId: site.zone,
      name: `${site.app}-served-headers`,
      kind: "zone",
      phase: "http_response_headers_transform",
      rules: Array.map(rules, (rule) => {
        const shape = _split(rule.pattern)
        return {
          expression: shape.suffix === ""
            ? `starts_with(http.request.uri.path, "/${shape.prefix}")`
            : `starts_with(http.request.uri.path, "/${shape.prefix}") and ends_with(http.request.uri.path, "${shape.suffix}")`,
          action: "rewrite",
          actionParameters: { headers: { [rule.header]: { operation: "set", value: rule.value } } },
        }
      }),
    }, opts),
} as const

const _fargateBackend = (
  backend: NonNullable<Dispatch.Pins["backend"]>,
  image: pulumi.Input<string>,
  port: number,
  opts: { readonly provider: aws.Provider; readonly region: aws.types.enums.Region },
): {
  readonly dependsOn: ReadonlyArray<pulumi.Resource>
  readonly taskDefinitionArgs: awsx.types.input.ecs.FargateServiceTaskDefinitionArgs
} => {
  const root = backend.runner.contractRoot
  const volume = "backend"
  const prefix = `${backend.publication}/${backend.projection.contract.id}`
  const bucket = new aws.s3.BucketV2("backend", {}, opts)
  const versioning = new aws.s3.BucketVersioningV2("backend-versioning", {
    bucket: bucket.id,
    versioningConfiguration: { status: "Enabled" },
  }, opts)
  const ownership = new aws.s3.BucketOwnershipControls("backend-ownership", {
    bucket: bucket.id,
    rule: { objectOwnership: "BucketOwnerEnforced" },
  }, opts)
  const closed = new aws.s3.BucketPublicAccessBlock("backend-closed", {
    bucket: bucket.id,
    blockPublicAcls: true,
    blockPublicPolicy: true,
    ignorePublicAcls: true,
    restrictPublicBuckets: true,
  }, opts)
  const contract = new aws.s3.BucketObjectv2("backend-contract", {
    bucket: bucket.id,
    key: `${prefix}/contract.json`,
    contentBase64: Encoding.encodeBase64(backend.projection.files.contract),
    contentType: "application/json",
  }, pulumi.mergeOptions(opts, { dependsOn: [versioning, ownership, closed] }))
  const pointer = new aws.s3.BucketObjectv2("backend-generation", {
    bucket: bucket.id,
    key: `${prefix}/generation`,
    content: backend.projection.contract.id,
    contentType: "text/plain",
  }, pulumi.mergeOptions(opts, { dependsOn: [versioning, ownership, closed] }))
  const access = aws.iam.getPolicyDocumentOutput({
    statements: [
      {
        actions: ["s3:ListBucket"],
        resources: [bucket.arn],
        conditions: [{ test: "StringLike", variable: "s3:prefix", values: [`${prefix}/*`] }],
      },
      {
        actions: ["s3:GetObject"],
        resources: [pulumi.interpolate`${bucket.arn}/${prefix}/*`],
      },
    ],
  }, opts)
  return {
    dependsOn: [contract, pointer],
    taskDefinitionArgs: {
      containers: {
        app: {
          name: "app",
          image,
          essential: true,
          portMappings: [{ containerPort: port }],
          dependsOn: [{ containerName: "backend", condition: "SUCCESS" }],
          environment: [
            { name: StackOutputs.backend.root, value: root },
            { name: StackOutputs.backend.pointer, value: `${root}/generation` },
          ],
          mountPoints: [{ sourceVolume: volume, containerPath: root, readOnly: true }],
        },
        backend: {
          name: "backend",
          image: backend.materializer,
          essential: false,
          environment: [{ name: "AWS_REGION", value: opts.region }],
          command: [
            "s3", "cp", pulumi.interpolate`s3://${bucket.bucket}/${prefix}/`, root,
            "--recursive", "--only-show-errors",
          ],
          mountPoints: [{ sourceVolume: volume, containerPath: root, readOnly: false }],
        },
      },
      volumes: [{ name: volume }],
      taskRole: { args: { inlinePolicies: [{ name: "backend-read", policy: access.json }] } },
    },
  }
}

const _AWS: {
  readonly [K in StackSpec.Profile["compute"]]: (
    spec: StackSpec,
    pins: Dispatch.Pins,
    app: Option.Option<Dispatch.App>,
    opts: { readonly provider: aws.Provider; readonly region: aws.types.enums.Region },
  ) => Effect.Effect<Dispatch.Planes, Dispatch.EstateFault>
} = {
  serverless: (_spec, pins, _app, opts) => Effect.sync(() => {
    const vpc = new awsx.ec2.Vpc("net", { numberOfAvailabilityZones: 2, natGateways: { strategy: "Single" } }, opts)
    const repo = new awsx.ecr.Repository("registry", { forceDelete: false }, opts)
    const image = new awsx.ecr.Image("app", { repositoryUrl: repo.url, context: pins.context }, opts)
    const alb = new awsx.lb.ApplicationLoadBalancer("edge", {}, opts)
    const cluster = new aws.ecs.Cluster("compute", {}, opts)
    const backend = pins.backend === undefined
      ? {
          dependsOn: [] as ReadonlyArray<pulumi.Resource>,
          taskDefinitionArgs: {
            container: { name: "app", image: image.imageUri, portMappings: [{ containerPort: pins.port }] },
          } satisfies awsx.types.input.ecs.FargateServiceTaskDefinitionArgs,
        }
      : _fargateBackend(pins.backend, image.imageUri, pins.port, opts)
    new awsx.ecs.FargateService("app", {
      cluster: cluster.arn,
      desiredCount: 2,
      networkConfiguration: { subnets: vpc.privateSubnetIds },
      loadBalancers: [{ targetGroupArn: alb.defaultTargetGroup.arn, containerName: "app", containerPort: pins.port }],
      taskDefinitionArgs: backend.taskDefinitionArgs,
    }, pulumi.mergeOptions(opts, { dependsOn: backend.dependsOn }))
    const bucket = new aws.s3.BucketV2("objects", {}, opts)
    new aws.s3.BucketVersioningV2("objects-versioning", { bucket: bucket.id, versioningConfiguration: { status: "Enabled" } }, opts)
    new aws.s3.BucketOwnershipControls("objects-ownership", {
      bucket: bucket.id,
      rule: { objectOwnership: "BucketOwnerEnforced" },
    }, opts)
    new aws.s3.BucketPublicAccessBlock("objects-closed", {
      bucket: bucket.id,
      blockPublicAcls: true,
      blockPublicPolicy: true,
      ignorePublicAcls: true,
      restrictPublicBuckets: true,
    }, opts)
    const site = pins.site === undefined
      ? Option.none<Source.Distributed>()
      : Option.some(Source.distribute("frontend", {
          arm: "aws",
          path: pins.site.path,
          bucket: bucket.bucket,
          assets: pins.site.assets,
          decoders: pins.site.decoders,
        }, { providers: [opts.provider] }))
    Option.map(site, (held) => {
      const front = _EDGED.aws("frontend", held.edge, { bucket }, opts)
      new aws.s3.BucketPolicy("objects-origin", {
        bucket: bucket.id,
        policy: {
          Version: aws.types.enums.iam.PolicyDocumentVersion.PolicyDocumentVersion_2012_10_17,
          Statement: [{
            Effect: aws.types.enums.iam.PolicyStatementEffect.ALLOW,
            Principal: { Service: "cloudfront.amazonaws.com" },
            Action: "s3:GetObject",
            Resource: pulumi.interpolate`${bucket.arn}/*`,
            Condition: { StringEquals: { "AWS:SourceArn": front.arn } },
          }],
        },
      }, opts)
    })
    return {
      object: { endpoint: bucket.bucketRegionalDomainName, bucket: bucket.bucket },
      ingress: { hostname: alb.loadBalancer.dnsName },
      ...Option.match(site, { onNone: () => ({}), onSome: (held) => ({ served: held.served }) }),
    }
  }),
  cluster: (spec, pins, app, opts) => Effect.suspend(() => {
    const vpc = new awsx.ec2.Vpc("net", { numberOfAvailabilityZones: 2, natGateways: { strategy: "Single" } }, opts)
    const plane = new eks.Cluster("plane", {
      vpcId: vpc.vpcId,
      publicSubnetIds: vpc.publicSubnetIds,
      privateSubnetIds: vpc.privateSubnetIds,
      authenticationMode: eks.AuthenticationMode.Api,
      createOidcProvider: true,
      skipDefaultNodeGroup: true,
    }, opts)
    const capacity = spec.profile.capacity
    new eks.ManagedNodeGroup("capacity", {
      cluster: plane,
      instanceTypes: [capacity.instanceType],
      operatingSystem: capacity.os,
      scalingConfig: { desiredSize: capacity.min, minSize: capacity.min, maxSize: capacity.max },
    }, opts)
    const provider = new k8s.Provider("k8s", { kubeconfig: plane.kubeconfigJson, enableServerSideApply: true })
    return _estate(spec, pins, provider, app)
  }),
}

const _ARMS: { readonly [K in StackSpec.Arm]: Dispatch.Arm } = {
  "selfhosted-k8s": (spec, material, pins) =>
    Effect.map(
      Effect.all({ proven: _proven(spec, material), app: _staged(spec) }),
      ({ proven, app }) => () => {
        const bootstrap = _grounded("plane", spec, proven, pins)
        const provider = new k8s.Provider("k8s", { kubeconfig: bootstrap.kubeconfig, enableServerSideApply: true })
        return _bodied(_estate(spec, pins, provider, app))
      },
    ),
  "selfhosted-docker": (spec, material, pins) =>
    Effect.map(
      Effect.all({ proven: _proven(spec, material), ref: _coord(spec, spec.image, "image"), edge: _edged(spec) }),
      ({ proven, ref, edge }) => async () => {
        const daemon = _grounded("daemon", spec, proven, pins)
        const provider = new docker.Provider("engine", { host: proven.connection.ssh }, { dependsOn: [daemon] })
        const machine = { provider }
        const secrets = new Secrets("secrets", { spec, entries: _credentials(spec, "data") })
        const image = new dockerBuild.Image("app", {
          push: true,
          tags: [ref],
          context: { location: pins.context },
          platforms: ["linux/amd64", "linux/arm64"],
          cacheFrom: [{ registry: { ref: `${ref}-cache` } }],
          cacheTo: [{ registry: { ref: `${ref}-cache` } }],
          ...(pins.registry !== undefined && {
            registries: [{
              address: pins.registry.address,
              username: pins.registry.user,
              password: secrets.read("REGISTRY_PASSWORD"),
            }],
          }),
        })
        const fence = new docker.Network("fence", { driver: "bridge", internal: false }, machine)
        const store = Record.map(
          { data: "/var/lib/postgresql/data", object: "/data", fanout: "/data", app: "/var/lib/rasm" } as const,
          (path, name) => ({ path, volume: new docker.Volume(name, { driver: "local" }, machine) }),
        )
        const backend = _dockerBackend(pins.backend)
        const bucket = `${spec.app}-artifacts`
        const data = new docker.Container("data", {
          image: pins.pgImage,
          restart: "unless-stopped",
          envs: [pulumi.interpolate`POSTGRES_PASSWORD=${secrets.read(_scoped("DB_ADMIN_PASSWORD", "data"))}`, `POSTGRES_DB=${spec.app}`],
          ports: [{ internal: 5432, external: 5432, ip: "127.0.0.1" }],
          networksAdvanced: [{ name: fence.name, aliases: ["data"] }],
          volumes: [{ volumeName: store.data.volume.name, containerPath: store.data.path }],
        }, machine)
        new docker.Container("object", {
          image: pins.objectImage,
          restart: "unless-stopped",
          command: ["sh", "-c", `mkdir -p /data/${bucket} && exec minio server /data --console-address :9001`],
          envs: [
            pulumi.interpolate`MINIO_ROOT_USER=${secrets.read("OBJECT_USER")}`,
            pulumi.interpolate`MINIO_ROOT_PASSWORD=${secrets.read("OBJECT_PASSWORD")}`,
          ],
          ports: [{ internal: 9000, external: 9000 }],
          networksAdvanced: [{ name: fence.name }],
          volumes: [{ volumeName: store.object.volume.name, containerPath: store.object.path }],
        }, machine)
        new docker.Container("fanout", {
          image: pins.natsImage,
          restart: "unless-stopped",
          command: ["-c", "/etc/nats/nats.conf"],
          uploads: [{
            file: "/etc/nats/nats.conf",
            content: `jetstream { store_dir: "/data", sync_interval: always }\nwebsocket { port: 8080, no_tls: true }`,
          }],
          ports: [{ internal: 4222, external: 4222 }, { internal: 8080, external: 8080 }],
          networksAdvanced: [{ name: fence.name }],
          volumes: [{ volumeName: store.fanout.volume.name, containerPath: store.fanout.path }],
        }, machine)
        const observe = new Dev("observe", {
          image: pins.observe.dev,
          host: proven.connection.host,
          network: fence.name,
          auth: secrets.read("GRAFANA_PASSWORD"),
        }, machine)
        const pinned = Option.match(proven.connection.hostKey, {
          onNone: () => `-o StrictHostKeyChecking=accept-new`,
          onSome: () => `-o UserKnownHostsFile="$PWD/sql.known" -o StrictHostKeyChecking=yes`,
        })
        const dial = `-p ${proven.connection.port} ${proven.connection.user}@${proven.connection.host}`
        const forward = new command.local.Command("sql-forward", {
          create: [
            `umask 077; printf '%s\n' "$SSH_PEM" > "$PWD/sql.pem"`,
            `[ -z "$SSH_HOST_KEY" ] || printf '%s %s\n' "${proven.connection.host}" "$SSH_HOST_KEY" > "$PWD/sql.known"`,
            `ssh -i "$PWD/sql.pem" ${pinned} -o ExitOnForwardFailure=yes -o BatchMode=yes -M -S "$PWD/sql.sock" -f -N -L 127.0.0.1:15432:127.0.0.1:5432 ${dial}`,
          ].join(" && "),
          delete: `ssh -S "$PWD/sql.sock" -O exit ${dial} 2>/dev/null; rm -f "$PWD/sql.pem" "$PWD/sql.known"`,
          environment: {
            SSH_PEM: pulumi.secret(Redacted.value(proven.key)),
            SSH_HOST_KEY: Option.getOrElse(proven.connection.hostKey, () => ""),
          },
          logging: "none",
        }, { dependsOn: [data] })
        const sql = new postgresql.Provider("sql", {
          host: "127.0.0.1",
          port: 15432,
          username: "postgres",
          password: secrets.read(_scoped("DB_ADMIN_PASSWORD", "data")),
          sslmode: "disable",
        }, { dependsOn: [forward] })
        const role = new postgresql.Role("app-role", {
          name: `${spec.app}_app`,
          login: true,
          password: secrets.read(_scoped("DB_PASSWORD", "data")),
        }, { provider: sql })
        const database = new postgresql.Database("app", { name: spec.app, owner: role.name }, { provider: sql })
        Array.map(spec.profile.extensions, (extension) =>
          new postgresql.Extension(extension, { name: extension, database: database.name }, { provider: sql }))
        const analyst = new postgresql.Role("analyst-role", {
          name: `${spec.app}_analyst`,
          login: true,
          password: secrets.read(_scoped("DB_ANALYST_PASSWORD", "data")),
        }, { provider: sql })
        new postgresql.GrantRole("analyst-read-all", {
          role: analyst.name,
          grantRole: "pg_read_all_data",
        }, { provider: sql })
        new postgresql.Grant("analyst-select", {
          database: database.name,
          role: analyst.name,
          schema: "public",
          objectType: "table",
          privileges: ["SELECT"],
        }, { provider: sql })
        new postgresql.DefaultPrivileges("analyst-future", {
          database: database.name,
          role: analyst.name,
          owner: role.name,
          schema: "public",
          objectType: "table",
          privileges: ["SELECT"],
        }, { provider: sql })
        new postgresql.ReplicationSlot("outbox", {
          database: database.name,
          name: `${spec.app}_outbox`,
          plugin: "pgoutput",
        }, { provider: sql })
        new docker.Container("app", {
          image: image.ref,
          restart: "unless-stopped",
          envs: [
            pulumi.interpolate`DOPPLER_TOKEN=${secrets.token}`,
            pulumi.interpolate`${StackOutputs.channels["otlp.endpoint"]}=${observe.collectorEndpoint}`,
            ...backend.envs,
          ],
          uploads: [...backend.uploads],
          ports: [{ internal: pins.port, external: pins.port }],
          networksAdvanced: [{ name: fence.name }],
          volumes: [{ volumeName: store.app.volume.name, containerPath: store.app.path }],
        }, { ...machine, dependsOn: [data] })
        const identity = new random.RandomUuid7("deploy-id", { keepers: { epoch: spec.epoch } })
        const boards = new Boards("boards", {
          spec,
          urls: observe.urls,
          targets: observe.targets,
          auth: secrets.read("GRAFANA_PASSWORD"),
          boards: pins.boards,
          alerts: pins.alerts,
          objectives: pins.objectives,
          contacts: pins.contacts,
          deploy: { id: identity.result },
        }, { dependsOn: [observe] })
        secrets.store("GRAFANA_AUTOMATION_TOKEN", boards.automation)
        return {
          data: { host: "data", port: 5432, database: spec.app, role: `${spec.app}_app`, pooling: "session" },
          object: { endpoint: `http://${proven.connection.host}:9000`, bucket },
          fanout: { origin: `ws://${proven.connection.host}:8080` },
          otlp: { endpoint: observe.collectorEndpoint },
          grafana: { url: observe.urls.grafana },
          deploy: { id: identity.result },
          ...Option.match(edge, {
            onNone: () => ({}),
            onSome: ({ domain, zone }) => {
              const hostname = `${spec.app}.${domain}`
              const cf = new cloudflare.Provider("cf", { apiToken: secrets.read("CLOUDFLARE_API_TOKEN") })
              new cloudflare.DnsRecord("edge", {
                zoneId: zone,
                type: "A",
                name: hostname,
                content: proven.connection.host,
                proxied: false,
                ttl: 1,
              }, { provider: cf })
              if (pins.acme !== undefined) {
                const registration = Certs.register("edge", { email: pins.acme.email })
                const trusted = Certs.trusted("edge", { registration, hostname, challenge: pins.acme.challenge })
                secrets.store("EDGE_TLS_KEY", trusted.key)
                secrets.store("EDGE_TLS_CERT", trusted.cert)
              }
              return { ingress: { hostname } }
            },
          }),
        }
      },
    ),
  aws: (spec, _material, pins) =>
    Effect.map(
      Effect.all({ region: _vocab(spec, spec.region, "region", aws.types.enums.Region), app: _staged(spec) }),
      ({ region, app }) => () => {
        const provider = new aws.Provider("aws", { region })
        return _bodied(_AWS[spec.profile.compute](spec, pins, app, { provider, region }))
      },
    ),
  gcp: (spec, _material, pins) =>
    Effect.map(
      Effect.all({
        region: _coord(spec, spec.region, "region"),
        project: _coord(spec, spec.project, "project"),
        managed: _managed(spec, pins),
      }),
      ({ region, project, managed }) => async () => {
        const secrets = new Secrets("secrets", { spec, entries: { [_scoped("DB_PASSWORD", "data")]: { generate: {} } } })
        const provider = new gcp.Provider("gcp", { project, region, credentials: secrets.read("GCP_CREDENTIALS") })
        const opts = { provider }
        const bucket = new gcp.storage.Bucket("objects", { location: region, uniformBucketLevelAccess: true, versioning: { enabled: true } }, opts)
        const sql = new gcp.sql.DatabaseInstance("data", {
          databaseVersion: managed.version,
          region,
          settings: { tier: managed.tier },
        }, opts)
        new gcp.sql.Database("app", { instance: sql.name, name: spec.app }, opts)
        new gcp.sql.User("app-role", { instance: sql.name, name: `${spec.app}_app`, password: secrets.read(_scoped("DB_PASSWORD", "data")) }, opts)
        const site = pins.site === undefined
          ? Option.none<Source.Distributed>()
          : Option.some(Source.distribute("frontend", {
              arm: "gcp",
              path: pins.site.path,
              bucket: bucket.name,
              assets: pins.site.assets,
              decoders: pins.site.decoders,
            }, { providers: [provider] }))
        Option.map(site, (held) => _EDGED.gcp("frontend", held.edge, { bucket }, opts))
        return {
          object: { endpoint: bucket.url, bucket: bucket.name },
          data: { host: sql.publicIpAddress, port: 5432, database: spec.app, role: `${spec.app}_app`, pooling: "session" },
          ...Option.match(site, { onNone: () => ({}), onSome: (held) => ({ served: held.served }) }),
        }
      },
    ),
  cloudflare: (spec, _material, _pins) =>
    Effect.map(
      Effect.all([
        _coord(spec, spec.domain, "domain"),
        _coord(spec, spec.zone, "zone"),
        _coord(spec, spec.account, "account"),
      ]),
      ([domain, zone, account]) => async () => {
        const secrets = new Secrets("secrets", { spec, entries: {} })
        const provider = new cloudflare.Provider("cf", { apiToken: secrets.read("CLOUDFLARE_API_TOKEN") })
        const store = new cloudflare.R2Bucket("objects", { accountId: account, name: `${spec.app}-artifacts` }, { provider })
        new cloudflare.R2BucketLifecycle("objects-aging", {
          accountId: account,
          bucketName: store.name,
          rules: [{
            id: "abort-stale-multipart",
            enabled: true,
            conditions: { prefix: "" },
            abortMultipartUploadsTransition: { condition: { type: "Age", maxAge: 604800 } },
          }],
        }, { provider })
        const site = new cloudflare.PagesProject("site", {
          accountId: account,
          name: spec.app,
          productionBranch: "main",
        }, { provider })
        new cloudflare.DnsRecord("apex", {
          zoneId: zone,
          type: "CNAME",
          name: `${spec.app}.${domain}`,
          content: pulumi.interpolate`${site.name}.pages.dev`,
          proxied: true,
          ttl: 1,
        }, { provider })
        _EDGED.cloudflare("site", Source.edge, { zone, app: spec.app }, { provider })
        return {
          object: { endpoint: pulumi.interpolate`https://${account}.r2.cloudflarestorage.com/${store.name}`, bucket: store.name },
          ingress: { hostname: `${spec.app}.${domain}` },
        }
      },
    ),
}

const Dispatch = {
  ..._map,
  capabilities: _capabilities,
  cell: (capability: Dispatch.Capability, arm: StackSpec.Arm): Option.Option<string> =>
    Option.fromNullable(_cells[capability][arm]),
  column: (arm: StackSpec.Arm): Dispatch.Column =>
    Array.filterMap(_capabilities, (capability) =>
      Option.map(Option.fromNullable(_cells[capability][arm]), (family) => [capability, family] as const)),
  material: _material,
  program: (spec: StackSpec, material: Dispatch.Material, pins: Dispatch.Pins): Effect.Effect<PulumiFn, DeployFault> =>
    _ARMS[spec.target](spec, material, pins),
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { Bootstrap, Dispatch }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

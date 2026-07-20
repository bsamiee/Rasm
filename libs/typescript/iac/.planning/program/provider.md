# [IAC_PROVIDER]

Provider dispatch and the service surface as ONE owner keyed by one union: the `_map` equivalence table and the `_ARMS` handler record both key on `StackSpec.Arm` — the map is the audit surface capability reads, the record is the construction it describes, and review pressure holds them adjacent. Rows are capabilities, columns are arms, cells name the exact resource-family spelling, and a hole is honest absence read as `Option`. Adding a cloud is one record row and one map column; finalizing one is a `StackSpec` value, never a lib edit. `iac/src/program/provider.ts` is the module.

Each arm is a total function from spec, host material, and pins to a `PulumiFn`: `_proven`, `_coord`, and `_staged` prove every spec-derived coordinate as typed `DeployFault`s before the program body is entered, so no tier throws for a value the spec already proves. `_estate` is the single k8s-estate builder — the selfhosted arm feeds it a `Bootstrap` kubeconfig provider, the aws `cluster` row an `eks.Cluster.kubeconfigJson` provider — so the whole tier roster rides either plane and promoting a cloud is a provider-seam swap, never a tier rewrite.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                | [PUBLIC]    |
| :-----: | :------------------ | :-------------------------------------------------------------------- | :---------- |
|  [01]   | `EQUIVALENCE_MAP`   | the capability-by-arm table and its `Option`-lifted projections       | `Dispatch`  |
|  [02]   | `ARM_CONTRACT`      | the material read, pins, the coordinate proofs, the exhaustive record | `Dispatch`  |
|  [03]   | `CLUSTER_BOOTSTRAP` | first boot, staging, install, connection hardening, kubeconfig egress | `Bootstrap` |
|  [04]   | `ARM_PROGRAMS`      | the shared k8s estate builder and the five arm bodies                 | `Dispatch`  |

## [02]-[EQUIVALENCE_MAP]

[EQUIVALENCE_MAP]:
- Owner: the interior `_capabilities` key tuple anchoring row order, the `_map` table carrying per-arm cells as exact-optional keys (a hole is an omitted key, never a sentinel), and the two projections riding the exported owner: `cell(capability, arm)` lifts the unproven cell read to `Option`, `column(arm)` folds an arm's realized subset in row order. Reads ride `_cells`, the table widened to `Dispatch.Cell` rows — a declared-key access on the literal union demands the key on every row, so the bracket read is index trust lifted at the seam while `_map` keeps its literals.
- Law: cells are family spellings, not mechanics — a cell names the resource classes (`gcp.container.Cluster + NodePool`) or the owning row (`helm minio-continuation | ceph-rgw`), and the page that constructs them is the mechanics owner; capability audits and promotion reviews read the map, and the `_ARMS` record is the construction it describes.
- Law: the object row admits only conditional-put-conforming engines — the self-host cells name the maintained MinIO continuation and Ceph RGW, the managed cells name S3, R2, and GCS; the CRDT-metadata engine that cannot honor `If-None-Match: *` has no cell anywhere, because the data plane's write-once identity algebra is non-negotiable and the refusal is `data`'s engine table read as deployment law.
- Law: the canonical secret owner spans every column — the `secret` row is Doppler on all five arms, and a cloud secret manager is reachable only as a mirror (`secretssync.<Target>` where the Doppler provider ships the destination, an in-graph write fed by the fan-in read where it does not), so no arm ever grows a second secret source of truth.
- Law: a prepared column's filled cells are its finalization contract — the `gcp` column names GKE, Cloud SQL, GCS, Cloud DNS, and the Secret Manager mirror against the same capability rows the primary arm realizes, so finalizing means instantiating the named subset with the `StackSpec` value; the dormant remainder of a provider SDK is unreachable by construction.
- Entry: `Dispatch.column(spec.target)` inside an arm; `Dispatch.cell("data", "aws")` for a point read.
- Growth: a new capability is one `_capabilities` entry and one `_map` row; a new arm is one cell per realized row under the new column key.
- Boundary: kube-row mechanics are `kube/*`; the object/data engine choices are `StackSpec.profile` values; the tenant row's mechanics are `kube/tenant.md`; the in-cluster reconcile row's mechanics are `operate/policy.md`; cross-stack output reads ride `StackReference` inside the tenant seam `kube/tenant.md` owns.
- Packages: `effect` (`Array`, `Option`, `Record`); `./spec.ts` (`StackSpec`).

```typescript
import { Array, Option, Record } from "effect"
import type { StackSpec } from "./spec.ts"

const _capabilities = [
  "bootstrap", "workload", "data", "object", "fanout", "cert", "dns",
  "ingress", "secret", "registry", "network", "identity", "observe",
  "tenant", "distribution", "reconcile",
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
    gcp: "gcp.container.Cluster + gcp.container.NodePool",
    cloudflare: "cloudflare.WorkersScript + cloudflare.WorkersRoute",
  },
  data: {
    "selfhosted-k8s": "cnpg Cluster CR (managed roles, plugin-barman-cloud) + Database CR + ensure Job",
    "selfhosted-docker": "docker.Container(postgres) + postgresql.Database/Role/Grant/Extension",
    aws: "aws.rds.Cluster",
    gcp: "gcp.sql.DatabaseInstance + gcp.sql.Database + gcp.sql.User",
    cloudflare: "cloudflare.D1Database",
  },
  object: {
    "selfhosted-k8s": "helm minio-continuation | ceph-rgw",
    "selfhosted-docker": "docker.Container(minio-continuation)",
    aws: "aws.s3.BucketV2",
    gcp: "gcp.storage.Bucket + gcp.storage.BucketIAMMember",
    cloudflare: "cloudflare.R2Bucket",
  },
  fanout: {
    "selfhosted-k8s": "helm nats (jetstream + websocket)",
    "selfhosted-docker": "docker.Container(nats)",
  },
  cert: {
    "selfhosted-k8s": "tls chain -> core/v1.Secret(kubernetes.io/tls) | acme.Certificate (cluster-external)",
    "selfhosted-docker": "acme.Registration + acme.Certificate (CSR over tls chain)",
    aws: "aws.acm.Certificate",
    gcp: "gcp.certificatemanager.Certificate",
    cloudflare: "cloudflare.OriginCaCertificate + cloudflare.TotalTls",
  },
  dns: {
    "selfhosted-k8s": "cloudflare.DnsRecord",
    "selfhosted-docker": "cloudflare.DnsRecord",
    aws: "aws.route53.Zone + aws.route53.Record | cloudflare.DnsRecord (cluster estate)",
    gcp: "gcp.dns.ManagedZone + gcp.dns.RecordSet",
    cloudflare: "cloudflare.Zone + cloudflare.DnsRecord",
  },
  ingress: {
    "selfhosted-k8s": "kubernetes.networking/v1.Ingress | cloudflare.ZeroTrustTunnelCloudflared",
    "selfhosted-docker": "docker.Container ports",
    aws: "awsx.lb.ApplicationLoadBalancer",
    gcp: "gcp.compute.GlobalAddress + gcp.compute.URLMap",
    cloudflare: "cloudflare.ZeroTrustTunnelCloudflared + cloudflare.ZeroTrustAccessApplication",
  },
  secret: {
    "selfhosted-k8s": "doppler canonical",
    "selfhosted-docker": "doppler canonical",
    aws: "doppler canonical + secretssync.AwsSecretsManager mirror",
    gcp: "doppler canonical + gcp.secretmanager mirror",
    cloudflare: "doppler canonical",
  },
  registry: {
    "selfhosted-k8s": "docker-build.Image registry export",
    "selfhosted-docker": "docker-build.Image + docker.RegistryImage",
    aws: "awsx.ecr.Repository + awsx.ecr.Image",
    gcp: "gcp.artifactregistry.Repository",
  },
  network: {
    "selfhosted-k8s": "kubernetes.networking/v1.NetworkPolicy",
    "selfhosted-docker": "docker.Network + docker.Volume",
    aws: "awsx.ec2.Vpc",
    gcp: "gcp.compute.Network + gcp.compute.Subnetwork + gcp.compute.Firewall",
  },
  identity: {
    "selfhosted-k8s": "core/v1.ServiceAccount + rbac/v1.Role + rbac/v1.RoleBinding",
    aws: "aws.iam.Role + aws.iam.Policy + aws.iam.RolePolicyAttachment | eks IRSA (createOidcProvider)",
    gcp: "gcp.serviceaccount.Account + gcp.projects.IAMMember",
  },
  observe: {
    "selfhosted-k8s": "store-row backend charts + otel collector + grafana apply",
    "selfhosted-docker": "collector container + grafana apply",
  },
  tenant: {
    "selfhosted-k8s": "capsule Tenant CR | vcluster chart (crd2pulumi typed)",
    aws: "kube tenant tier over the EKS estate (compute: cluster)",
  },
  distribution: {
    aws: "synced-folder.S3BucketFolder over aws.s3.BucketV2",
    gcp: "synced-folder.GoogleCloudFolder over gcp.storage.Bucket",
    cloudflare: "cloudflare.PagesProject | R2Bucket static origin",
  },
  reconcile: {
    "selfhosted-k8s": "PKO Stack/Program CR (in-cluster reconcile loop)",
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
- Law: pins are a parameter, never a module read — `Dispatch.Pins` carries the deploy-time facts the spec does not (chart and operator versions including the external-dns pin, the extension-image ref, the machine container images the docker cells run — `objectImage`, `natsImage`, the `observe.dev` all-in-one image — the optional `registry` push row (address and user; the password is the `REGISTRY_PASSWORD` fan-in read), the cloudflared connector image, the install script and its first-boot parts, the host-fact probe commands, the managed-capacity and managed-data rows for the prepared arms, the build context, the ensure-DDL roster the data plane publishes, the encoded boards and alert specs from the core observe suite, and the optional `acme` row — directory email with the DNS-01 challenge — that arms the trusted-cert lane); the app root resolves them from its own config and suite call, so ingress is parameterized end to end and the lib hardcodes no version anywhere.
- Law: one provider seam per arm — the arm constructs its provider (kubeconfig-bound `k8s.Provider`, `ssh://` `docker.Provider`, credentialed cloud provider) exactly once and threads it through tier options; per-resource providers are the named defect, and the credential arrives from `Secrets.read` in-graph or the ambient `doppler run` env, never a literal.
- Law: the `PulumiFn` body is the deploy plane's program seam — a promise-returning composition of tier constructors bound to consts and one returned outputs record; the platform owns that shape, and everything the arm computes before entering it stays on the rail.
- Entry: `Effect.flatMap(Dispatch.material, (material) => Dispatch.program(spec, material, pins))` then `Automation.stack(spec, program)`.
- Growth: one record row and one map column per cloud; a new shared deploy-time fact is one `Pins` field, a new shared secret fact is one `material` field; a new spec coordinate a tier requires is one `_coord` call in its arm's proof.
- Boundary: the run and receipt are `automation.md`'s; outputs keys are `spec.md`'s contract.
- Packages: `effect` (`Config`, `Effect`, `Option`, `Redacted`); `./spec.ts` (`StackSpec`); `./automation.ts` (`DeployFault`); `../kube/traffic.ts` (`Traffic.Edge`); `../operate/observe.ts` (`Lgtm.Versions`).

```typescript
import type { PulumiFn } from "@pulumi/pulumi/automation"
import { Config, Effect, Option, Redacted } from "effect"
import type { Alert, DashboardModel, Slo } from "@rasm/ts/core"
import { Traffic } from "../kube/traffic.ts"
import { DeployFault } from "./automation.ts"
import type { StackSpec } from "./spec.ts"

declare namespace Dispatch {
  type Material = { readonly sshKey: Option.Option<Redacted.Redacted<string>> }
  type Arm = (spec: StackSpec, material: Material, pins: Pins) => Effect.Effect<PulumiFn, DeployFault>
  type App = { readonly image: string; readonly edge: Option.Option<Traffic.Edge> }
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
    readonly observe: { readonly [K in keyof Lgtm.Versions | "dev"]: string } // the chart roster derives from the observe tier's own vocabulary; dev is the docker arm's all-in-one image
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
    readonly nodes: { readonly instanceType: string; readonly min: number; readonly max: number }
    readonly managedData: { readonly engine: string; readonly tier: string }
    readonly ensures: ReadonlyArray<string>
    readonly boards: ReadonlyArray<typeof DashboardModel.Encoded>
    readonly alerts: ReadonlyArray<Alert.Spec>
    readonly objectives: ReadonlyArray<Slo.Objective>
    readonly contacts: Partial<Record<"page" | "ticket", {
      readonly webhook: string
      readonly quiet?: ReadonlyArray<{ readonly days: ReadonlyArray<string>; readonly start: string; readonly end: string }>
    }>>
  }
}

const _material = Config.unwrap({
  sshKey: Config.option(Config.redacted("IAC_SSH_KEY")),
})

const _input = (spec: StackSpec, detail: string): DeployFault =>
  new DeployFault({ reason: "input", stack: spec.name, detail })

const _coord = <A>(spec: StackSpec, held: Option.Option<A>, name: string): Effect.Effect<A, DeployFault> =>
  Effect.mapError(held, () => _input(spec, `<missing-${name}>`))

const _proven = (spec: StackSpec, material: Dispatch.Material): Effect.Effect<{
  readonly connection: StackSpec.Connection
  readonly key: Redacted.Redacted<string>
}, DeployFault> =>
  Option.zipWith(spec.connection, material.sshKey, (connection, key) => ({ connection, key })).pipe(
    Effect.mapError(() => _input(spec, "<missing-connection-or-key>")),
  )

const _staged = (spec: StackSpec): Effect.Effect<Option.Option<Dispatch.App>, DeployFault> =>
  Option.match(spec.image, {
    onNone: () => Effect.succeedNone,
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

const _edged = (spec: StackSpec): Effect.Effect<Option.Option<{ readonly domain: string; readonly zone: string }>, DeployFault> =>
  spec.profile.exposure === "internal"
    ? Effect.succeedNone
    : spec.profile.exposure === "tunnel"
      ? Effect.fail(_input(spec, "<unrealized-exposure:tunnel>"))
      : Option.match(spec.domain, {
          onNone: () => Effect.succeedNone,
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
- Law: `_estate` is the one k8s-estate composition — namespace → `Secrets` (generated entries: `DB_ADMIN_PASSWORD`, `DB_PASSWORD`, `OBJECT_USER`, `OBJECT_PASSWORD`, `GRAFANA_PASSWORD`; `CLOUDFLARE_API_TOKEN` pre-exists on the app's config) → `ObjectStore` → `Nats` → `Postgres` (the admin and app credentials as two distinct reads) → `Lgtm` → `Boards` → `Tenants` when the tenancy mode escalates past `single` → the `RandomUuid7` deployment identity → `Workload.token` → optional `Workload` whose live-`Output` env pairs ride `StackOutputs.pairsOf` with the `pulumi.output(value).apply(String)` renderer — the same flatten the decoded getter rides — → and, only when the staged edge is realized (an `internal` exposure stands service-only), one `Certs.root` CA → `Traffic` over the workload service with the issuance capability and the proven `Edge` case injected; graph-late material (`GRAFANA_AUTOMATION_TOKEN` from `Boards.automation`, `MESH_CA_KEY` from the CA root) lands through `secrets.store` so it outlives the graph in the one canonical store; it returns every realized `StackOutputs` plane, `deploy` included. Both k8s-plane sources feed it: the selfhosted arm's `Bootstrap.kubeconfig` and the aws arm's `eks.Cluster.kubeconfigJson`, so the entire tier roster is plane-agnostic by construction.
- Law: the app image is one buildx product — the docker arm and any registry cell build through `docker-build.Image` with `push: true`, the immutable `ref`/`digest` pinning every runtime; `platforms` rows make the build multi-arch, `cacheFrom`/`cacheTo` registry rows reuse layers across runs, the push credential rides the `registries` row — `pins.registry` coordinates with the `REGISTRY_PASSWORD` fan-in read, so a `push: true` build carries its own auth instead of assuming an ambient login — and by-value `secrets` bind Doppler outputs so no build credential touches disk. A rust build stage runs `wasm-pack build` over the pinned `fastcdc` crate and the runtime stage copies the pkg, so the chunking artifact ships inside the image digest and no second artifact pipeline exists.
- Law: the docker arm realizes its whole column — `_grounded` (the one Bootstrap spelling both selfhosted arms share, folding the connection's `hostKey`/`bastion` hardening coordinates in) lays the daemon, the `ssh://` `docker.Provider` binds the proven connection's own `ssh` projection with `dependsOn` the daemon so the first `up` cannot race the install, and the machine estate mirrors `_estate` at container depth: one `Secrets` store with the generated credential entries, one `docker.Network` fence, the mount table minting one `docker.Volume` per store beside its path so mount spellings exist once, the postgres container finalized through the bridged `postgresql.Provider` (`Role`/`Database`/`Extension` rows from the profile's extension subset — the read-back `operate/policy.md`'s `conform` correlates), the MinIO-continuation container whose filesystem bucket pre-creates in its own command, the NATS container configured through an `uploads` row (jetstream fsync-per-write, websocket listener — the same durability law the chart row states), the app container pinning the built digest and injecting `DOPPLER_TOKEN` beside the collector-endpoint row so the baked `doppler run` entrypoint resolves config at start and telemetry exports byte-identically to the estate arm, the `Dev` all-in-one estate realizing the observe cell with `Boards` applied over its URL plane and the automation token landed through `secrets.store`, the `Direct`-edge `DnsRecord` and the ACME trusted pair landed through `secrets.store` when `pins.acme` arms the lane (`_edged` proves domain/zone and refuses the unrealized tunnel posture on the rail), and the `RandomUuid7` deploy identity — the arm returns every plane it realizes: `data`, `object`, `fanout`, `otlp`, `grafana`, `deploy`, and `ingress` under a proven edge.
- Law: the aws arm dispatches its compute posture as data — `_AWS` is a handler record keyed by `StackSpec.Profile["compute"]`: the `serverless` row realizes VPC → ECR build → Fargate behind an ALB with the S3 object cell; the `cluster` row escalates to `eks.Cluster` (`authenticationMode: "API"`, `createOidcProvider: true` for IRSA, `skipDefaultNodeGroup: true`) with one `ManagedNodeGroup` sized from `pins.nodes`, binds `kubeconfigJson` into the arm's one `k8s.Provider` seam, and reuses `_estate` whole — the managed twin of `Bootstrap.kubeconfig`, one seam swap and zero tier edits.
- Law: the gcp arm realizes its finalization contract — the provider binds `credentials` from the `GCP_CREDENTIALS` fan-in read, the GKE anchor stands, the object cell is `gcp.storage.Bucket` (uniform bucket-level access), and the data cell is `gcp.sql.DatabaseInstance` + `Database` + `User` with the engine tag and machine tier arriving as `pins.managedData`; the cloudflare arm binds `apiToken` from the fan-in, realizes the `R2Bucket` object cell and the `PagesProject` static origin against the proven `account`, and lands the dns cell as the CNAME onto the project's `pages.dev` subdomain against the proven `zone` — the record targets a project the arm minted, never a dangling coordinate; each returns exactly the planes it realizes.
- Law: every arm funds the boards — the encoded models and alert specs enter as pins where the arm realizes an observe cell; an arm without the observe cell returns no `grafana` plane and drops nothing silently.
- Growth: promoting a prepared arm is one realizer body or one `_AWS`-style posture row; a new cloud is one record row and one map column.
- Boundary: tier mechanics live on the tier pages; the declared realizers' argument catalogues are the standing research items on the provider `.api` files.
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
import { Boards, Dev, Lgtm } from "../operate/observe.ts"
import { Certs, Secrets } from "../operate/secret.ts"
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

const _estate = (
  spec: StackSpec,
  pins: Dispatch.Pins,
  provider: k8s.Provider,
  app: Option.Option<Dispatch.App>,
): Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, pulumi.Input<string | number>>> => {
  const bound = { providers: [provider] }
  const ns = new k8s.core.v1.Namespace(spec.name, { metadata: { name: spec.name } }, { provider })
  const secrets = new Secrets("secrets", {
    spec,
    entries: {
      DB_ADMIN_PASSWORD: { generate: {} },
      DB_PASSWORD: { generate: {} },
      OBJECT_USER: { generate: { special: false, length: 20 } },
      OBJECT_PASSWORD: { generate: {} },
      GRAFANA_PASSWORD: { generate: {} },
    },
  })
  const objects = new ObjectStore("objects", {
    spec,
    namespace: ns.metadata.name,
    version: pins.object,
    auth: { user: secrets.read("OBJECT_USER"), password: secrets.read("OBJECT_PASSWORD") },
  }, bound)
  const fanout = new Nats("fanout", { spec, namespace: ns.metadata.name, version: pins.nats }, bound)
  const data = new Postgres("data", {
    spec,
    namespace: ns.metadata.name,
    image: pins.pgImage,
    operatorVersion: pins.operator,
    barmanVersion: pins.barman,
    objects,
    auth: { admin: secrets.read("DB_ADMIN_PASSWORD"), app: secrets.read("DB_PASSWORD") },
    ensures: pins.ensures,
  }, bound)
  const lgtm = new Lgtm("observe", {
    spec,
    namespace: ns.metadata.name,
    versions: pins.observe,
    auth: secrets.read("GRAFANA_PASSWORD"),
    // pg-server metrics endpoint: in-graph woven, never a published output
    dsn: pulumi.interpolate`postgresql://${data.role}:${secrets.read("DB_PASSWORD")}@${data.host}:${data.port}/${data.database}`,
  }, bound)
  const identity = new random.RandomUuid7("deploy-id", { keepers: { epoch: spec.epoch } })
  const boards = new Boards("boards", {
    spec,
    urls: lgtm.urls,
    auth: secrets.read("GRAFANA_PASSWORD"),
    boards: pins.boards,
    alerts: pins.alerts,
    objectives: pins.objectives,
    contacts: pins.contacts,
    deploy: { id: identity.result },
  })
  secrets.store("GRAFANA_AUTOMATION_TOKEN", boards.automation)
  if (spec.profile.tenancy.mode !== "single") {
    new Tenants("tenants", { spec, versions: { capsule: pins.capsule, vcluster: pins.vcluster } }, bound)
  }
  const token = Workload.token("doppler-token", { namespace: ns.metadata.name, token: secrets.token }, { provider })
  const outputs = {
    data: { host: data.host, port: data.port, database: data.database, role: data.role },
    object: { endpoint: objects.endpoint, bucket: objects.bucket },
    fanout: { origin: fanout.origin },
    otlp: { endpoint: lgtm.collectorEndpoint },
    grafana: { url: lgtm.urls.grafana },
    deploy: { id: identity.result },
  }
  return Option.match(app, {
    onNone: () => outputs,
    onSome: ({ image, edge }) => {
      const workload = new Workload("app", {
        spec,
        namespace: ns.metadata.name,
        image,
        port: pins.port,
        env: Workload.rows(token.metadata.name, StackOutputs.pairsOf(outputs, (value) => pulumi.output(value).apply(String))),
      }, bound)
      return Option.match(edge, {
        onNone: () => outputs,
        onSome: (proven) => {
          const ca = Certs.root("mesh-ca")
          secrets.store("MESH_CA_KEY", ca.key.privateKeyPem)
          const traffic = new Traffic("traffic", {
            spec,
            namespace: ns.metadata.name,
            service: workload.service.metadata.name,
            port: pins.port,
            connector: pins.cloudflared,
            dnsVersion: pins.dns,
            issue: (hostname) => Certs.issue("edge", { ca, hostname }),
            apiToken: secrets.read("CLOUDFLARE_API_TOKEN"),
            edge: proven,
          }, bound)
          return { ...outputs, ingress: { hostname: traffic.hostname } }
        },
      })
    },
  })
}

const _AWS: {
  readonly [K in StackSpec.Profile["compute"]]: (
    spec: StackSpec,
    pins: Dispatch.Pins,
    app: Option.Option<Dispatch.App>,
    opts: { readonly provider: aws.Provider },
  ) => Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, pulumi.Input<string | number>>>
} = {
  serverless: (_spec, pins, _app, opts) => {
    const vpc = new awsx.ec2.Vpc("net", { numberOfAvailabilityZones: 2, natGateways: { strategy: "Single" } }, opts)
    const repo = new awsx.ecr.Repository("registry", { forceDelete: false }, opts)
    const image = new awsx.ecr.Image("app", { repositoryUrl: repo.url, context: pins.context }, opts)
    const alb = new awsx.lb.ApplicationLoadBalancer("edge", {}, opts)
    const cluster = new aws.ecs.Cluster("compute", {}, opts)
    new awsx.ecs.FargateService("app", {
      cluster: cluster.arn,
      desiredCount: 2,
      networkConfiguration: { subnets: vpc.privateSubnetIds },
      loadBalancers: [{ targetGroupArn: alb.defaultTargetGroup.arn, containerName: "app", containerPort: pins.port }],
      taskDefinitionArgs: { container: { name: "app", image: image.imageUri, portMappings: [{ containerPort: pins.port }] } },
    }, opts)
    const bucket = new aws.s3.BucketV2("objects", {}, opts)
    return {
      object: { endpoint: bucket.bucketRegionalDomainName, bucket: bucket.bucket },
      ingress: { hostname: alb.loadBalancer.dnsName },
    }
  },
  cluster: (spec, pins, app, opts) => {
    const vpc = new awsx.ec2.Vpc("net", { numberOfAvailabilityZones: 2, natGateways: { strategy: "Single" } }, opts)
    const plane = new eks.Cluster("plane", {
      vpcId: vpc.vpcId,
      publicSubnetIds: vpc.publicSubnetIds,
      privateSubnetIds: vpc.privateSubnetIds,
      authenticationMode: eks.AuthenticationMode.Api,
      createOidcProvider: true,
      skipDefaultNodeGroup: true,
    }, opts)
    new eks.ManagedNodeGroup("capacity", {
      cluster: plane,
      instanceTypes: [pins.nodes.instanceType],
      scalingConfig: { desiredSize: pins.nodes.min, minSize: pins.nodes.min, maxSize: pins.nodes.max },
    }, opts)
    const provider = new k8s.Provider("k8s", { kubeconfig: plane.kubeconfigJson, enableServerSideApply: true })
    return _estate(spec, pins, provider, app)
  },
}

const _ARMS: { readonly [K in StackSpec.Arm]: Dispatch.Arm } = {
  "selfhosted-k8s": (spec, material, pins) =>
    Effect.map(
      Effect.all({ proven: _proven(spec, material), app: _staged(spec) }),
      ({ proven, app }) => async () => {
        const bootstrap = _grounded("plane", spec, proven, pins)
        const provider = new k8s.Provider("k8s", { kubeconfig: bootstrap.kubeconfig, enableServerSideApply: true })
        return _estate(spec, pins, provider, app)
      },
    ),
  "selfhosted-docker": (spec, material, pins) =>
    Effect.map(
      Effect.all({ proven: _proven(spec, material), ref: _coord(spec, spec.image, "image"), edge: _edged(spec) }),
      ({ proven, ref, edge }) => async () => {
        const daemon = _grounded("daemon", spec, proven, pins)
        const provider = new docker.Provider("engine", { host: proven.connection.ssh }, { dependsOn: [daemon] })
        const machine = { provider }
        const secrets = new Secrets("secrets", {
          spec,
          entries: {
            DB_ADMIN_PASSWORD: { generate: {} },
            DB_PASSWORD: { generate: {} },
            OBJECT_USER: { generate: { special: false, length: 20 } },
            OBJECT_PASSWORD: { generate: {} },
            GRAFANA_PASSWORD: { generate: {} },
          },
        })
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
        const bucket = `${spec.app}-artifacts`
        const data = new docker.Container("data", {
          image: pins.pgImage,
          restart: "unless-stopped",
          envs: [pulumi.interpolate`POSTGRES_PASSWORD=${secrets.read("DB_ADMIN_PASSWORD")}`, `POSTGRES_DB=${spec.app}`],
          ports: [{ internal: 5432, external: 5432 }],
          networksAdvanced: [{ name: fence.name }],
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
        const sql = new postgresql.Provider("sql", {
          host: proven.connection.host,
          port: 5432,
          username: "postgres",
          password: secrets.read("DB_ADMIN_PASSWORD"),
          sslmode: "disable",
        }, { dependsOn: [data] })
        const role = new postgresql.Role("app-role", {
          name: `${spec.app}_app`,
          login: true,
          password: secrets.read("DB_PASSWORD"),
        }, { provider: sql })
        const database = new postgresql.Database("app", { name: spec.app, owner: role.name }, { provider: sql })
        Array.map(spec.profile.extensions, (extension) =>
          new postgresql.Extension(extension, { name: extension, database: database.name }, { provider: sql }))
        new docker.Container("app", {
          image: image.ref,
          restart: "unless-stopped",
          envs: [
            pulumi.interpolate`DOPPLER_TOKEN=${secrets.token}`,
            pulumi.interpolate`OTEL_EXPORTER_OTLP_ENDPOINT=${observe.collectorEndpoint}`, // the one SDK export row: byte-identical to the k8s arm's env seam
          ],
          ports: [{ internal: pins.port, external: pins.port }],
          networksAdvanced: [{ name: fence.name }],
          volumes: [{ volumeName: store.app.volume.name, containerPath: store.app.path }],
        }, { ...machine, dependsOn: [data] })
        const identity = new random.RandomUuid7("deploy-id", { keepers: { epoch: spec.epoch } })
        const boards = new Boards("boards", {
          spec,
          urls: observe.urls,
          auth: secrets.read("GRAFANA_PASSWORD"),
          boards: pins.boards,
          alerts: pins.alerts,
          objectives: pins.objectives,
          contacts: pins.contacts,
          deploy: { id: identity.result },
        }, { dependsOn: [observe] })
        secrets.store("GRAFANA_AUTOMATION_TOKEN", boards.automation)
        return {
          data: { host: proven.connection.host, port: 5432, database: spec.app, role: `${spec.app}_app` },
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
      Effect.all({ region: _coord(spec, spec.region, "region"), app: _staged(spec) }),
      ({ region, app }) => async () => {
        const provider = new aws.Provider("aws", { region })
        return _AWS[spec.profile.compute](spec, pins, app, { provider })
      },
    ),
  gcp: (spec, _material, pins) =>
    Effect.map(
      Effect.all([_coord(spec, spec.region, "region"), _coord(spec, spec.project, "project")]),
      ([region, project]) => async () => {
        const secrets = new Secrets("secrets", { spec, entries: { DB_PASSWORD: { generate: {} } } })
        const provider = new gcp.Provider("gcp", { project, region, credentials: secrets.read("GCP_CREDENTIALS") })
        const opts = { provider }
        new gcp.container.Cluster("plane", { location: region, initialNodeCount: 2 }, opts)
        const bucket = new gcp.storage.Bucket("objects", { location: region, uniformBucketLevelAccess: true }, opts)
        const sql = new gcp.sql.DatabaseInstance("data", {
          databaseVersion: pins.managedData.engine,
          region,
          settings: { tier: pins.managedData.tier },
        }, opts)
        new gcp.sql.Database("app", { instance: sql.name, name: spec.app }, opts)
        new gcp.sql.User("app-role", { instance: sql.name, name: `${spec.app}_app`, password: secrets.read("DB_PASSWORD") }, opts)
        return {
          object: { endpoint: bucket.url, bucket: bucket.name },
          data: { host: sql.publicIpAddress, port: 5432, database: spec.app, role: `${spec.app}_app` },
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bootstrap, Dispatch }
```
